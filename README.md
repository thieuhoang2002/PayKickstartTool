# PayKickstart Account Creator - Hướng Dẫn Chi Tiết

## 📋 Giới Thiệu

Tool tự động tạo hàng loạt tài khoản PayKickstart (affiliate marketing platform) với các tính năng:

- ✅ Tự động điền form đăng ký
- ✅ Tự động xác thực email qua Gmail
- ✅ Tự động lấy password từ email
- ✅ Chạy song song nhiều tài khoản (1-10 luồng)
- ✅ Hỗ trợ 2 cách sử dụng: **Catch-All Email** (custom domain) hoặc **Gmail Account (riêng lẻ)**
- ✅ Xuất kết quả ra CSV với trạng thái chi tiết
- ✅ **Crawl Data**: Thu thập dữ liệu affiliate từ PayKickstart dashboard
- ✅ **Chọn tài khoản**: Chọn lọc tài khoản để crawl (checkbox)
- ✅ **Navigation nhanh**: Nút mở thư mục Data và Results

---

## 🎯 Yêu Cầu Trước Khi Sử Dụng

## � 2 Cách Sử Dụng Tool

### **Cách 1: Catch-All Email (Recommended)**
Mua domain rẻ + dùng email catch-all để hứng tất cả. Phù hợp khi **tạo hàng loạt tài khoản từ 1 domain**.

**Yêu cầu:**
1. Một tên miền (Domain) rẻ
2. Tài khoản Cloudflare (Email Routing catch-all)
3. Gmail gốc + 1 App Password duy nhất (chung cho tất cả account)

**CSV format:**
```csv
Email,MailPass,FirstName,LastName
acc01@mydomain.com,shared_app_pass_16_chars,John,Smith
acc02@mydomain.com,shared_app_pass_16_chars,Emma,Johnson
```
→ Cột `MailPass` chứa 1 password chung dùng cho mọi account (Gmail App Password của inbox chính)

---

### **Cách 2: Gmail Accounts (Riêng Lẻ)**
Dùng **nhiều tài khoản Gmail khác nhau**, mỗi account có **App Password riêng**. Phù hợp khi **test hoặc số lượng nhỏ**.

**Yêu cầu:**
1. Nhiều tài khoản Gmail
2. Mỗi Gmail cần bật xác thực 2 bước và tạo App Password

**CSV format:**
```csv
Email,MailPass,FirstName,LastName
user01@gmail.com,app_password_của_user01_16_chars,John,Smith
user02@gmail.com,app_password_của_user02_16_chars,Emma,Johnson
```
→ Cột `MailPass` chứa **App Password riêng cho từng Gmail**

---

## 🛠️ Yêu Cầu Chuẩn Bị (Tùy Cách Chọn)

### 1. Một Tên Miền (Domain) Rẻ
- Mua loại rẻ nhất (.space, .online, .xyz) giá khoảng $1 - $2/năm.
- **Khuyên dùng:** [Porkbun](https://porkbun.com/) hoặc [Namecheap](https://www.namecheap.com/).
- **Chỉ cần cho Cách 1 (Catch-All)**

### 2. Tài Khoản Cloudflare (Miễn Phí)
- Dùng để quản lý DNS và bật tính năng **Email Routing** (Hứng email miễn phí).
- **Chỉ cần cho Cách 1 (Catch-All)**

### 3. Gmail Gốc & App Password
- Cách 1: Cần 1 Gmail chính để nhận tất cả thư từ catch-all domain.
- Cách 2: Cần nhiều Gmail, mỗi Gmail có App Password riêng.
- **Bắt buộc:** Phải bật xác thực 2 bước và tạo **Mật khẩu ứng dụng (App Password)**.

### 4. (Bỏ proxy) Tập trung cấu hình email

Proxy đã gỡ bỏ khỏi tool. Dùng mạng hiện có.

---

## 📖 Hướng Dẫn Cài Đặt Hệ Thống (Bước-theo-Bước)

### BƯỚC 1: Mua Tên Miền & Chuyển Về Cloudflare
*Lý do: Các nhà cung cấp tên miền thường tính phí dịch vụ Email Forwarding, nhưng Cloudflare cho dùng miễn phí và không giới hạn.*

1.  **Mua tên miền:** Vào Porkbun/Namecheap mua 1 tên miền (Ví dụ: `yourdomain.com`).
2.  **Thêm vào Cloudflare:**
    - Đăng nhập [Cloudflare](https://dash.cloudflare.com/), bấm **Add a site**.
    - Nhập tên miền vừa mua -> Chọn gói **Free** ($0).
3.  **Thay đổi Nameservers (Quan trọng):**
    - Cloudflare sẽ cấp cho bạn 2 dòng Nameserver (VD: `treasure.ns.cloudflare.com` và `zod.ns.cloudflare.com`).
    - Quay lại trang quản lý tên miền (Porkbun), tìm mục **Authoritative Nameservers**.
    - Xóa cái cũ đi, dán 2 cái mới của Cloudflare vào -> Bấm **Submit**.
    - Quay lại Cloudflare bấm "Check nameservers" và đợi khoảng 15 phút đến khi hiện tích xanh ✅.

### BƯỚC 2: Cấu Hình "Hứng Email" (Catch-all) - **CHỈ CẦN CHO CÁCH 1**
*Mục tiêu: Bất kỳ email nào gửi tới `*@domaincuaban.com` đều bay về Gmail.*

**Nếu bạn chọn Cách 2 (Gmail riêng lẻ), bỏ qua bước này.**

1.  Trên Cloudflare, vào menu trái: **Email** -> **Email Routing**.
2.  Bấm **Get Started**. Cloudflare sẽ yêu cầu thêm bản ghi DNS, bấm **Add records and enable** (Màu xanh).
3.  **Tab "Destination addresses":**
    - Thêm Gmail chính của bạn vào.
    - Vào hộp thư Gmail, tìm mail từ Cloudflare và bấm nút **Verify**.
    - Trạng thái trên Cloudflare chuyển thành **Verified** là OK.
4.  **Tab "Routing rules" (Tab quan trọng nhất):**
    - Tìm mục **Catch-all address**.
    - Bấm **Edit** -> Bật lên (**Enabled**).
    - Action: `Send to an email`.
    - Destination: Chọn Gmail của bạn -> Bấm **Save**.

### BƯỚC 3: Tạo Gmail App Password (Áp Dụng Cho Cả 2 Cách)

#### Cách 1 (Catch-All): Tạo 1 App Password duy nhất
1. Vào [Google Account Security](https://myaccount.google.com/security)
2. **2-Step Verification** → **Get started** (nếu chưa bật)
3. Nhập số điện thoại → nhận mã xác thực
4. Vào [App Passwords](https://myaccount.google.com/apppasswords)
5. **Select app:** Mail | **Select device:** Windows Computer
6. Click **Generate** → Copy mật khẩu 16 ký tự (dạng: `abcd efgh ijkl mnop`)

#### Cách 2 (Gmail Riêng): Tạo App Password cho từng Gmail
1. **Đối với mỗi tài khoản Gmail**, làm lặp lại quá trình trên
2. Copy App Password của **user01@gmail.com** → đặt vào CSV dòng đầu
3. Copy App Password của **user02@gmail.com** → đặt vào CSV dòng 2
4. ...và cứ thế với các account còn lại

> 📌 **Video hướng dẫn:** [Gmail App Password Tutorial](https://www.youtube.com/results?search_query=gmail+app+password)

---

### BƯỚC 4: Chuẩn Bị File Data.csv

#### Cách 1 (Catch-All Email)
Mở `Data/Data.csv`, format:

```csv
Email,MailPass,FirstName,LastName
acc01@yourdomain.com,your-gmail-app-password-16-chars,John,Smith
acc02@yourdomain.com,your-gmail-app-password-16-chars,Emma,Johnson
acc03@yourdomain.com,your-gmail-app-password-16-chars,Michael,Brown
```

**Chi tiết:**
- **Email**: Email alias từ domain (phải khác nhau)
- **MailPass**: **Chung 1 password** (App Password của Gmail inbox chính)
- **FirstName**: Tên đầu (có thể giả)
- **LastName**: Họ (có thể giả)

#### Cách 2 (Gmail Riêng)
Mở `Data/Data.csv`, format:

```csv
Email,MailPass,FirstName,LastName
user01@gmail.com,app-password-of-user01,John,Smith
user02@gmail.com,app-password-of-user02,Emma,Johnson
user03@gmail.com,app-password-of-user03,Michael,Brown
```

**Chi tiết:**
- **Email**: Email Gmail (mỗi cái khác nhau)
- **MailPass**: **Riêng cho từng Gmail** (App Password của user đó)
- **FirstName**: Tên đầu (có thể giả)
- **LastName**: Họ (có thể giả)

#### Ví Dụ Thực Tế (Cách 1 - 10 tài khoản)
```csv
Email,MailPass,FirstName,LastName
paykick001@yourdomain.com,your-gmail-app-password-16-chars,James,Wilson
paykick002@yourdomain.com,your-gmail-app-password-16-chars,Sarah,Martinez
paykick003@yourdomain.com,your-gmail-app-password-16-chars,David,Anderson
paykick004@yourdomain.com,your-gmail-app-password-16-chars,Emily,Thomas
paykick005@yourdomain.com,your-gmail-app-password-16-chars,Daniel,Taylor
paykick006@yourdomain.com,your-gmail-app-password-16-chars,Jessica,Moore
paykick007@yourdomain.com,your-gmail-app-password-16-chars,Christopher,Jackson
paykick008@yourdomain.com,your-gmail-app-password-16-chars,Ashley,Martin
paykick009@yourdomain.com,your-gmail-app-password-16-chars,Matthew,Lee
paykick010@yourdomain.com,your-gmail-app-password-16-chars,Amanda,Perez
```

> 💡 **Tip:** Dùng Excel/Google Sheets để tạo hàng loạt, sau đó export sang CSV

---

## 🚀 Cách Sử Dụng Tool

### 🔹 Navigation & Tiện Ích

**Nút mở thư mục nhanh:**
- **Mở Data**: Truy cập nhanh thư mục `Data/` (chứa Data.csv)
- **Mở Results**: Truy cập nhanh thư mục `Results/` (chứa output)

> 💡 **Tip:** Các nút này nằm ở cột trái, giúp bạn nhanh chóng truy cập các thư mục quan trọng

---

### 📋 Phần A: Tạo Tài Khoản Mới

#### Bước 1: Cấu Hình Settings

**Cách 1 (Catch-All):**
1. **Mở tool** → Tab **Settings**
2. **Email Catch-All Configuration:** Nhập Gmail nhận catch-all (ví dụ: `your-catchall@gmail.com`)
3. Click **LƯU EMAIL**
4. Thông báo "Lưu thành công" xuất hiện ✅

**Cách 2 (Gmail Riêng):**
- Không cần cấu hình Settings, vì mỗi email Gmail trong CSV đã có password riêng

**Proxy (tùy chọn, người dùng tự nhập):**
- Bật "Bật proxy" nếu muốn chạy qua proxy xoay
- Nhập `Host`, `Port`, `User`, `Pass` theo thông tin proxy của bạn
- Lưu ý: Tool không kèm proxy. Bạn cần tự mua và nhập vào

![Settings Tab](docs/screenshots/settings-tab.png)
> ⚠️ **Lưu ý:** Nếu không có ảnh `docs/screenshots/`, tạo thư mục và thêm screenshot sau

---

#### Bước 2: Chọn File CSV

1. Tab **Log** → Click **CHỌN FILE**
2. Chọn `Data/Data.csv` (hoặc file CSV khác)
3. **Preview** xuất hiện bên trái:
   - Hiển thị danh sách tài khoản
   - Kiểm tra Email/FirstName/LastName

![CSV Preview](docs/screenshots/csv-preview.png)

---

#### Bước 3: Chạy Tự Động

1. **Số luồng song song:**
   - Khuyên: `2` luồng (ổn định, tối đa 10)
2. Click **BẮT ĐẦU CHẠY**
3. **Quan sát log:**
   ```
   [12:34:56] Bắt đầu xử lý: acc01@yourdomain.com
   [12:35:12] Điền form thành công
   [12:35:28] Đợi email verify...
   [12:35:45] Nhận link verify: https://paykickstart.com/verify/...
   [12:35:45] Kiểm tra email Gmail (riêng)...
   [12:36:02] Kích hoạt tài khoản thành công
   [12:36:15] Password: Abc123XYZ!@#
   [12:36:20] ✅ HOÀN TẤT: acc01@yourdomain.com
   ```

![Running Process](docs/screenshots/running-log.png)

---

#### Bước 4: Xem Kết Quả (Tab Log)

Khi hoàn tất:
- File kết quả: `Results/KetQua_143025.csv` (tự động đặt tên theo giờ chạy)
- Mở file CSV:

```csv
Email,MailPass,FirstName,LastName,GeneratedPassword,Status
acc01@yourdomain.com,your-gmail-app-password-16-chars,John,Smith,Abc123XYZ!@#,Thành công
user02@gmail.com,app-password-of-user02,Emma,Johnson,Def456UVW!@#,Thành công
acc03@yourdomain.com,your-gmail-app-password-16-chars,Michael,Brown,ERROR: Không nhận được email,Lỗi
```

**Giải thích Status:**
- `Thành công`: Tài khoản tạo thành công
- `Lỗi`: Lỗi xảy ra (xem cột Status chi tiết)

---

### 📊 Phần B: Thu Thập Dữ Liệu Affiliate (Crawl Data)

**Mục đích:** Sau khi tạo tài khoản, dùng tab **Crawl Data** để thu thập thông tin affiliate (clicks, conversions, revenue) từ dashboard của từng tài khoản.

#### Bước 1: Tải File Kết Quả

1. Tab **Crawl Data** → Click **TẢI FILE**
2. Chọn file `Results/KetQua_*.csv` (file output từ bước tạo tài khoản)
3. Danh sách tài khoản hiển thị trong bảng với các cột:
   - **Checkbox**: Chọn tài khoản muốn crawl
   - **Email**: Địa chỉ email tài khoản
   - **Password**: Password đã tạo
   - **FirstName / LastName**: Thông tin cá nhân

---

#### Bước 2: Chọn Tài Khoản Cần Crawl

**Cách 1: Chọn từng tài khoản**
- Tick vào checkbox của các tài khoản muốn crawl

**Cách 2: Crawl tất cả**
- Bỏ qua, chọn số luồng và bấm nút **CẬP NHẬT TẤT CẢ** (crawl toàn bộ danh sách)

> 💡 **Tip:** Nút **CẬP NHẬT ACC ĐÃ CHỌN** chỉ bật khi có ít nhất 1 checkbox được tick

---

#### Bước 3: Cấu Hình & Chạy Crawl

1. **Nhập số luồng song song** (1-10):
   - Khuyên dùng: `3-5` luồng cho crawl data (nhanh hơn tạo tài khoản)
2. **Chọn chế độ:**
   - **CẬP NHẬT ACC ĐÃ CHỌN**: Chỉ crawl các tài khoản đã tick checkbox
   - **CẬP NHẬT TẤT CẢ**: Crawl toàn bộ danh sách trong file CSV
3. **Quan sát tiến trình:**
   - **Progress Bar**: Hiển thị tiến độ % hoàn thành
   - **Status Label**: Hiển thị số lượng "X/Y hoàn tất"
   - **Log console**: Chi tiết từng bước (login, fetch data, save JSON)

```
[14:23:45] Crawl acc01@yourdomain.com - Đang đăng nhập...
[14:23:52] Crawl acc01@yourdomain.com - Lấy dữ liệu dashboard...
[14:24:05] Crawl acc01@yourdomain.com - Lưu: Results/Crawl/acc01@yourdomain.com.json
[14:24:06] ✅ Hoàn tất: acc01@yourdomain.com (1/10)
```

---

#### Bước 4: Xem Kết Quả Crawl

**File output:**
- Thư mục: `Results/Crawl/`
- Format: `{email}.json` (VD: `acc01@yourdomain.com.json`)

**Nội dung JSON:**
```json
{
  "email": "acc01@yourdomain.com",
  "total_clicks": 1250,
  "total_conversions": 45,
  "total_revenue": "$1,234.56",
  "campaigns": [
    {
      "name": "Campaign ABC",
      "clicks": 800,
      "conversions": 30,
      "revenue": "$890.00"
    }
  ],
  "timestamp": "2026-01-23T14:24:05Z"
}
```

**Mở nhanh thư mục Results:**
- Bấm nút **Mở Results** ở cột trái → Windows Explorer tự động mở

---

### 🎯 Workflow Hoàn Chỉnh (End-to-End)

```
1. Settings → Cấu hình email catch-all (nếu dùng Cách 1)
2. Tab Log → Chọn Data.csv → BẮT ĐẦU CHẠY → Tạo 50 tài khoản
3. Kiểm tra Results/KetQua_*.csv → Xác nhận danh sách tài khoản
4. Tab Crawl Data → TẢI FILE → Chọn KetQua_*.csv
5. Tick checkbox các tài khoản cần crawl (hoặc crawl tất cả)
6. Chọn số luồng (3-5) → CẬP NHẬT ACC ĐÃ CHỌN
7. Kiểm tra Results/Crawl/ → Phân tích dữ liệu JSON
```

---

## ❗ Troubleshooting - Khắc Phục Lỗi

### 🔴 Lỗi: "Vui lòng nhập Catch-All Email" (Cách 1)
**Nguyên nhân:** Chưa cấu hình Settings  
**Giải pháp:**
1. Tab Settings → nhập email catch-all
2. Click "LƯU EMAIL"

> **Note:** Cách 2 không cần cấu hình này vì dùng Gmail riêng

---

### 🔴 Lỗi: "Không nhận được email verify"
**Nguyên nhân:**
- Catch-all chưa hoạt động (Cách 1)
- Gmail App Password sai hoặc không khớp
- Email đã tồn tại trong hệ thống

**Giải pháp:**
1. **Kiểm tra Catch-all (Cách 1):**
   - Gửi email test đến `test@yourdomain.com`
   - Xem có nhận trong Gmail không
2. **Kiểm tra App Password:**
   - Tạo lại App Password mới
   - Cập nhật vào Data.csv (chính xác từng dòng)
   - Đối với Cách 2, đảm bảo mỗi Gmail có password đúng
3. **Đợi lâu hơn:**
   - PayKickstart đôi khi gửi chậm (5-10 phút)
   - Chạy lại tool sau 10 phút


---

### 🔴 Lỗi: "CAPTCHA xuất hiện nhiều"
**Nguyên nhân:** Chạy quá nhiều request từ 1 IP  
   2. **Dùng Proxy:** (đã gỡ khỏi tool, bỏ qua)
   3. **Chia nhỏ batch:**
      - Thay vì chạy 100 acc cùng lúc
      - Chạy 10 acc → nghỉ 30 phút → chạy tiếp 10 acc
   - Thay vì chạy 100 acc cùng lúc
   - Chạy 10 acc → nghỉ 30 phút → chạy tiếp 10 acc

---

### 🔴 Lỗi: "Login failed: AUTHENTICATE failed"
**Nguyên nhân:** Gmail App Password sai hoặc hết hạn  
**Giải pháp:**
1. Vào [Gmail App Passwords](https://myaccount.google.com/apppasswords)
2. Xóa password cũ
3. Tạo password mới (16 ký tự)
4. Cập nhật vào cột `MailPass` trong CSV
5. Chạy lại tool

---

### 🔴 Lỗi: "Email already registered"
**Không phải lỗi!** Tool tự động:
- Nhận diện email đã tồn tại
- Vẫn đọc email để lấy password
- Ghi vào Results với Status: "Already Registered"

---

## 📊 FAQ - Câu Hỏi Thường Gặp

### ❓ Tôi phải dùng Cloudflare không? Có thể dùng Google Workspace?
**Có thể!** Bất kỳ dịch vụ nào hỗ trợ catch-all email:
- **Cloudflare Email Routing** (miễn phí, khuyên dùng)
- **Google Workspace** ($6/tháng/user - đắt)
- **Microsoft 365** ($5/tháng/user)
- **Zoho Mail** (miễn phí 5 users)
- **ImprovMX** (miễn phí catch-all forward)

---

### ❓ Tôi có thể tạo 1000 tài khoản cùng lúc không?
**Không nên!** Lý do:
- PayKickstart có rate limit (giới hạn request)
- Risk cao bị ban IP/domain

**Khuyến nghị:**
- **Cách 1 (Catch-All):** 20-30 acc/ngày, chia 3-4 batch
- **Cách 2 (Gmail riêng):** 10-20 acc/ngày (vì mỗi Gmail khác nhau, chậm hơn)
- **Delay giữa batch:** 30-60 phút

---

### ❓ Sự khác biệt giữa Cách 1 và Cách 2 là gì?

| Tiêu Chí | Cách 1 (Catch-All) | Cách 2 (Gmail Riêng) |
|----------|-------------------|----------------------|
| **Domain** | Cần mua domain | Không cần |
| **Thiết lập** | Phức tạp hơn (Cloudflare setup) | Đơn giản (Gmail App Password) |
| **Tốc độ** | Nhanh (40-50 acc/ngày) | Chậm hơn (10-20 acc/ngày) |
| **Chi phí** | $1-2/năm (domain) | Miễn phí (Gmail) |
| **Số lượng** | Scale lớn (100+ accounts) | Scale nhỏ (10-50 accounts) |
| **Password** | 1 password chung | Mỗi account có password riêng |

**Khuyên dùng:**
- Cách 1 nếu tạo hàng loạt (50+ accounts)
- Cách 2 nếu test hoặc số lượng ít (< 20 accounts)

---


### ❓ Domain tôi mới mua có dùng ngay được không?
**Nên chờ 24-48h** để:
- DNS propagate (lan truyền toàn cầu)
- Cloudflare Email Routing ổn định

**Kiểm tra domain ready:**
```bash
# Mở Command Prompt/Terminal
nslookup yourdomain.com

# Nếu hiện IP → domain đã active
```

---

### ❓ Proxy miễn phí có dùng được không?
**Không khuyến khích** vì:
- ❌ Chậm, timeout nhiều
- ❌ IP đã bị blacklist
- ❌ Không ổn định

**Dịch vụ proxy uy tín:**
- [Bright Data](https://brightdata.com/) (residential, đắt)
- [Oxylabs](https://oxylabs.io/) (residential)
- [IPRoyal](https://iproyal.com/) (residential, giá tốt)
- [Webshare](https://www.webshare.io/) (datacenter, rẻ)

---

### ❓ Tool có hoạt động trên Mac/Linux không?
**Không**, tool build cho Windows (.NET 8 WinForms)

**Giải pháp cho Mac/Linux:**
- Dùng Windows VM (VirtualBox/Parallels)
- Dùng Wine/CrossOver (chưa test)

---

### ❓ Tôi muốn tạo 100 dòng CSV tự động, có cách nào?
Dùng **Google Sheets** với formula:

```
A2: =CONCATENATE("acc", TEXT(ROW()-1,"000"), "@yourdomain.com")
B2: your-gmail-app-password-16-chars
C2: =INDEX(SPLIT("John,Emma,Michael,Sarah,David,James,Emily,Daniel,Christopher,Jessica"," "),1,MOD(ROW(),10)+1)
D2: =INDEX(SPLIT("Smith,Johnson,Brown,Martinez,Wilson,Anderson,Taylor,Thomas,Moore,Lee"," "),1,MOD(ROW(),10)+1)
```

Kéo xuống 100 dòng → Download as CSV

---

## 🎓 Tips & Best Practices

### ✅ Nên Làm
- ✅ Test với **5-10 tài khoản** trước khi chạy hàng loạt
- ✅ Backup file `Results/` thường xuyên
- ✅ Dùng **proxy chất lượng** khi scale lên 50+
- ✅ Monitor log realtime để phát hiện lỗi sớm
- ✅ Đặt tên domain **giống business thật** (tránh spam filter)

### ❌ Không Nên
- ❌ Chạy 100+ tài khoản không proxy
- ❌ Dùng domain vừa mua chưa warm-up
- ❌ Set số luồng = 4 ngay lần đầu
- ❌ Ignore lỗi CAPTCHA (dấu hiệu bị flag)
- ❌ Share tool với Gmail/Domain đã config (security risk)

---

## 📞 Liên Hệ & Hỗ Trợ

**Nếu gặp lỗi không giải quyết được:**
1. Chụp screenshot console log
2. Copy nội dung file `Results/KetQua_*.csv`

**GitHub Issues:**
- Report bug: [Link to GitHub Issues]
- Feature request: [Link to Discussions]

---

## 📝 License & Disclaimer

⚠️ **Disclaimer:**
- Người dùng chịu trách nhiệm tuân thủ Terms of Service của PayKickstart
- Không khuyến khích spam/abuse

**License:** MIT License (sửa theo nhu cầu)

---

## 📚 Tài Liệu Tham Khảo

- [Cloudflare Email Routing Docs](https://developers.cloudflare.com/email-routing/)
- [Gmail App Passwords Guide](https://support.google.com/accounts/answer/185833)
- [PayKickstart Official Site](https://paykickstart.com/)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)

---

**Version:** 1.0.0  
**Last Updated:** January 22, 2026  
**Maintainer:** ndkth
