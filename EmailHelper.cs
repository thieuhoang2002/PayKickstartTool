using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.Net;
using System.Web;

public class EmailHelper
{
    private readonly string inboxEmail;
    private readonly string inboxAppPassword;

    public EmailHelper(string inboxEmail, string inboxAppPassword)
    {
        this.inboxEmail = inboxEmail;
        this.inboxAppPassword = inboxAppPassword;
    }

    // Hàm chung: Tìm nội dung email mới nhất dành cho alias (catch-all) có chứa từ khóa
    private string GetMailBody(string targetRecipient, string keywordInBody)
    {
        try
        {
            using (var client = new ImapClient())
            {
                client.Connect("imap.gmail.com", 993, true);
                client.Authenticate(inboxEmail, inboxAppPassword);

                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);

                // Chỉ quét mail từ PayKickstart, ưu tiên các mail gửi cho alias cụ thể
                var recipientQuery = SearchQuery.HeaderContains("Delivered-To", targetRecipient)
                                                .Or(SearchQuery.ToContains(targetRecipient));
                var query = SearchQuery.FromContains("paykickstart").And(recipientQuery);
                var uids = inbox.Search(query);

                // Duyệt từ mới nhất xuống cũ hơn để lấy đúng mail gần nhất cho alias
                for (int i = uids.Count - 1; i >= 0; i--)
                {
                    var uid = uids[i];
                    var message = inbox.GetMessage(uid);

                    // Double-check alias nằm trong header phòng trường hợp IMAP search bỏ sót
                    if (!IsRecipientMatch(message, targetRecipient)) continue;

                    string body = message.TextBody ?? message.HtmlBody ?? string.Empty;

                    if (body.IndexOf(keywordInBody, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        inbox.AddFlags(uid, MessageFlags.Seen, true); // Đánh dấu đã đọc để tránh trùng
                        client.Disconnect(true);
                        return body;
                    }
                }

                client.Disconnect(true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi Mail: " + ex.Message);
        }
        return null;
    }

    // 1. Lấy Link Verify (tìm các keywords liên quan đến verify)
    public string GetVerifyLink(string targetRecipient)
    {
        string[] keywords = { "verify", "confirming", "confirm", "verification", "paykickstart" };
        string body = null;

        foreach (var keyword in keywords)
        {
            body = GetMailBody(targetRecipient, keyword);
            if (!string.IsNullOrEmpty(body)) break;
        }

        if (string.IsNullOrEmpty(body)) return null;

        // Decode HTML entities trước khi regex
        body = WebUtility.HtmlDecode(body);

        // Regex bắt link verify: https://app.paykickstart.com/verify-email/...
        string pattern = @"https:\/\/app\.paykickstart\.com\/verify-email\/[a-zA-Z0-9\-_=]+";
        Match match = Regex.Match(body, pattern);
        return match.Success ? match.Value : null;
    }

    // 2. Lấy Password (Dựa trên mẫu file txt: có chữ "credentials")
    public string GetPasswordFromMail(string targetRecipient)
    {
        string body = GetMailBody(targetRecipient, "credentials");
        if (string.IsNullOrEmpty(body)) return null;

        // Decode HTML entities trước khi regex (vì email HTML có &gt;, &lt;, v.v.)
        body = WebUtility.HtmlDecode(body);

        string pattern = @"Password:\s*([^\s<]+)";
        Match match = Regex.Match(body, pattern);
        if (match.Success)
        {
            // Đảm bảo password được decode lần nữa nếu có entity
            string rawPass = match.Groups[1].Value.Trim();
            return WebUtility.HtmlDecode(rawPass);
        }
        return null;
    }

    private static bool IsRecipientMatch(MimeMessage message, string targetRecipient)
    {
        var lowered = targetRecipient?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(lowered)) return false;

        bool matchAddress(InternetAddressList list) => list
            .OfType<MailboxAddress>()
            .Any(addr => addr.Address.ToLowerInvariant().Contains(lowered));

        if (matchAddress(message.To)) return true;
        if (matchAddress(message.Cc)) return true;
        if (matchAddress(message.Bcc)) return true;

        // Fallback: check raw headers such as Delivered-To
        foreach (var header in message.Headers)
        {
            if (header.Field.Equals("Delivered-To", StringComparison.OrdinalIgnoreCase) &&
                header.Value.ToLowerInvariant().Contains(lowered))
            {
                return true;
            }
        }

        return false;
    }
}