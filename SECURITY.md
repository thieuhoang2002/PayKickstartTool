# Chính Sách Bảo Mật (Security Policy)

Dự án **PayKickstartTool** xử lý các thông tin nhạy cảm bao gồm email, mật khẩu ứng dụng (App Passwords) và dữ liệu tài khoản tự động. Chúng tôi cam kết bảo vệ an toàn thông tin cho người sử dụng.

---

## 🛡️ Phiên Bản Được Hỗ Trợ

| Phiên Bản | Được Hỗ Trợ Bản Vá Bảo Mật |
| :--- | :--- |
| `v1.1+` / Nhánh `main` | :white_check_mark: Có |
| Các bản cũ hơn | :x: Không |

---

## ⚠️ Lưu Ý An Toàn Quan Trọng Khi Sử Dụng Tool

1. **Bảo mật App Password**: Tuyệt đối không chia sẻ mật khẩu ứng dụng Gmail (App Password) hoặc commit các file chứa mật khẩu này lên repository công khai.
2. **Quản lý dữ liệu cục bộ**: File kết quả trong thư mục `Results/` và dữ liệu đầu vào trong `Data/` được xử lý 100% trên máy cục bộ của bạn, không gửi về bất kỳ máy chủ bên thứ ba nào ngoại trừ các dịch vụ được bạn cấu hình (như PayKickstart và máy chủ mail IMAP).

---

## 🚨 Báo Cáo Lỗ Hổng Bảo Mật

Nếu bạn phát hiện bất kỳ vấn đề bảo mật nghiêm trọng nào (chẳng hạn như rò rỉ thông tin xác thực, lỗ hổng inject mã hoặc bypass cơ chế an toàn):

1. **Vui lòng KHÔNG mở Issue công khai trên GitHub**.
2. Hãy gửi thông báo bảo mật riêng tư tới:
   📧 **thieuhoangent@gmail.com**  
   *(Hoặc sử dụng tính năng **Private vulnerability reporting** trong tab **Security** của repo GitHub)*.
3. Vui lòng cung cấp:
   - Mô tả chi tiết về lỗ hổng.
   - Các bước cụ thể để tái hiện lỗi.
   - Mức độ ảnh hưởng ước tính.

Chúng tôi sẽ kiểm tra, xác nhận và phản hồi trong vòng **24 - 48 giờ** và tiến hành phát hành bản vá kịp thời.

Cảm ơn bạn đã hỗ trợ bảo vệ an toàn cho dự án!
