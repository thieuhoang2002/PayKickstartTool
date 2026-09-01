using System;
using System.Drawing;
using System.Windows.Forms;

namespace PayKickstartAuto
{
    public class GuideForm : Form
    {
        private RichTextBox rtb;
        public GuideForm()
        {
            this.Text = "Hướng dẫn sử dụng";
            this.Size = new Size(700, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var bgDark = Color.FromArgb(24, 26, 27);
            var bgCard = Color.FromArgb(34, 36, 38);
            var fgText = Color.FromArgb(220, 220, 220);

            this.BackColor = bgDark;

            var header = new Label(){ Text = "Hướng dẫn sử dụng phần mềm", Location = new Point(20, 18), AutoSize = true, ForeColor = fgText, Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            var sub = new Label(){ Text = "Dành cho người dùng không chuyên kỹ thuật", Location = new Point(22, 46), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9, FontStyle.Regular) };

            rtb = new RichTextBox(){ Location = new Point(20, 80), Width = 640, Height = 460, ReadOnly = true, BackColor = Color.FromArgb(20,20,20), ForeColor = fgText, BorderStyle = BorderStyle.FixedSingle }; 
            rtb.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            rtb.Text = GetGuideText();

            var btnClose = new Button(){ Text = "Đóng", Location = new Point(580, 550), Width = 80, Height = 28 };
            btnClose.Click += (s,e)=> this.Close();

            this.Controls.Add(header);
            this.Controls.Add(sub);
            this.Controls.Add(rtb);
            this.Controls.Add(btnClose);
        }

        private string GetGuideText()
        {
            return string.Join("\n\n", new []{
                "1) Chuẩn bị",
                "- Email catch-all: Mua domain và bật catch-all routing (ví dụ Cloudflare Email Routing) để mọi email alias chuyển về một hộp thư Gmail trung tâm.",
                "- App password Gmail: Tạo app password cho Gmail catch-all và dùng chung cho mọi dòng trong CSV (cột MailPass).",
                "- Thư mục Data: App tự tạo thư mục Data cạnh file exe; đặt CSV vào đây (Data.csv, Data_*.csv...).",
                "- CSV mẫu: Gồm các cột Email, MailPass, FirstName, LastName. Mỗi dòng là một alias khác nhau (ví dụ: acc01@yourdomain.com).",

                "2) Cấu hình",
                "- Tab Settings: nhập email catch-all Gmail của bạn và bấm 'Lưu Email' (bắt buộc).",
                "- Tab Settings: nhập proxy nếu cần (tùy chọn), định dạng IP:Port hoặc IP:Port:User:Pass.",

                "3) Chọn CSV",
                "- Bấm nút 'Chọn File' (mặc định mở thư mục Data) rồi chọn file Data.csv hoặc Data_*.csv.",
                "- Nếu trong Data đã có Data.csv, ứng dụng tự chọn và hiển thị trước.",
                "- Bảng bên trái hiển thị danh sách để bạn kiểm tra nhanh.",

                "4) Chạy tự động",
                "- Chọn 'Số luồng' (khuyên dùng 2).",
                "- Bấm 'BẮT ĐẦU CHẠY'. Ứng dụng sẽ tự điền form PayKickstart, gửi verify, kích hoạt tài khoản, và ghi Password nhận từ email vào cột GeneratedPassword.",
                "- Bảng tiến độ và nhật ký (console) hiển thị bên phải.",

                "5) Kết quả",
                "- Khi xong, ứng dụng tạo file KetQua_HHmmss.csv trong thư mục Results và hiển thị ngay trên bảng.",
                "- Thư mục Results nằm cạnh file exe, chứa toàn bộ file KetQua_*.csv để bạn sao lưu dễ dàng.",

                "6) Lưu ý quan trọng",
                "- MailPass trong CSV: phải là app password của hộp thư catch-all Gmail trung tâm.",
                "- Email trong CSV: dùng alias thuộc domain catch-all của bạn (ví dụ: acc01@yourdomain.com).",
                "- Số luồng cao có thể khiến hệ thống chặn (rate-limit). Nếu gặp CAPTCHA nhiều, hãy giảm luồng.",

                "7) Proxy (tùy chọn nâng cao)",
                "- Khi xử lý số lượng lớn (100+), cân nhắc proxy để giảm rủi ro chặn. Mỗi phiên nên dùng proxy riêng.",
                "- Ưu tiên residential/ISP proxy nếu CAPTCHA/ban nghiêm trọng. Datacenter rẻ nhưng dễ bị chặn hơn.",

                "8) Khắc phục sự cố",
                "- Không thấy mail verify: đợi thêm vài phút và chạy lại. Kiểm tra alias đúng và catch-all hoạt động.",
                "- 'Email đã đăng ký': hệ thống tự nhận biết và vẫn lấy mail verify/password nếu có.",
                "- Lỗi kết nối: kiểm tra mạng, đăng nhập Gmail (app password), giảm số luồng.",
            });
        }
    }
}
