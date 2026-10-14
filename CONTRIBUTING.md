# Hướng Dẫn Đóng Góp (Contributing Guide)

Cảm ơn bạn đã quan tâm đến việc đóng góp cho dự án **PayKickstartTool**!

Dự án này là công cụ Desktop Automation (Windows Forms trên nền tảng .NET 8) hỗ trợ tự động hóa đăng ký tài khoản, xác thực email, giải captcha và thu thập dữ liệu tiếp thị liên kết (affiliate).

Mọi sự hỗ trợ về báo lỗi, tối ưu hiệu năng luồng (threading), cập nhật selector Selenium hoặc phát triển thêm tính năng mới đều được hoan nghênh.

---

## 🛠️ Yêu Cầu Môi Trường Phát Triển

Để xây dựng và chạy dự án trên máy tính, bạn cần:
- **Hệ điều hành**: Windows 10 / Windows 11
- **.NET SDK**: .NET 8.0 SDK trở lên
- **IDE**: Visual Studio 2022 (với workload *.NET desktop development*) hoặc VS Code (với *C# Dev Kit*)
- **Trình duyệt**: Google Chrome (phiên bản ổn định mới nhất, tương thích với Selenium WebDriver)

---

## 🚀 Các Bước Cài Đặt & Chạy Thử

1. **Fork repository** trên GitHub về tài khoản của bạn.
2. **Clone** repo về máy:
   ```bash
   git clone https://github.com/<your-username>/PayKickstartTool.git
   cd PayKickstartTool
   ```
3. **Khôi phục các gói thư viện (NuGet packages)**:
   ```bash
   dotnet restore
   ```
4. **Build dự án**:
   ```bash
   dotnet build
   ```
5. **Chạy ứng dụng**:
   ```bash
   dotnet run --project tool-create-account-paykickstart.csproj
   ```
   *(Hoặc mở file solution trong Visual Studio và nhấn **F5**)*.

---

## 🌿 Quy Chuẩn Đặt Nhánh (Git Branching)

Hãy tạo nhánh riêng cho từng tính năng hoặc bản sửa lỗi:
- Nhánh tính năng: `feature/ten-tinh-nang` (ví dụ: `feature/proxy-rotation`)
- Nhánh sửa lỗi: `fix/ten-loi` (ví dụ: `fix/gmail-imap-timeout`)
- Nhánh tài liệu: `docs/cap-nhat`

```bash
git checkout -b feature/ten-tinh-nang
```

---

## 📝 Quy Chuẩn Commit Message

Khuyến khích sử dụng chuẩn Conventional Commits:
- `feat`: Thêm tính năng mới (ví dụ: `feat: add headless browser toggle option`)
- `fix`: Sửa lỗi phát sinh (ví dụ: `fix: handle captcha iframe selector change`)
- `docs`: Cập nhật tài liệu hướng dẫn (`docs: update quickstart guide`)
- `perf`: Tối ưu hiệu năng đa luồng hoặc giảm mức chiếm dụng RAM
- `refactor`: Tái cấu trúc mã nguồn không làm thay đổi hành vi người dùng

---

## 🧪 Lưu Ý Trước Khi Mở Pull Request

- Đảm bảo dự án build thành công mà không có cảnh báo/lỗi nghiêm trọng (`dotnet build -c Release`).
- Kiểm tra các thao tác đa luồng không gây treo UI hoặc xung đột tài nguyên file CSV.
- Tuyệt đối không commit các file nhạy cảm: App Passwords, API Keys cá nhân, file danh sách tài khoản thực tế (`Data/*.csv` nhạy cảm).
- Mở **Pull Request** vào nhánh `main` và mô tả chi tiết những gì bạn đã cải tiến.

Cảm ơn bạn đã đóng góp cho dự án! 🎉
