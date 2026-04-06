# EduLMS - Hệ thống Quản lý Học tập Trực tuyến

Hệ thống LMS xây dựng bằng ASP.NET MVC (.NET 8) với 3 vai trò: Admin, Instructor, Learner.

## Tech Stack

- **Backend:** ASP.NET MVC (.NET 8), Entity Framework Core
- **Database:** MySQL (Pomelo Provider)
- **Auth:** ASP.NET Identity (Role-based)
- **Frontend:** Bootstrap 5, Bootstrap Icons, Chart.js, FullCalendar
- **Thanh toán:** VietQR (tạo mã QR), SePay.vn (webhook xác nhận tự động)
- **Export:** ClosedXML (Excel reports)

## Cài đặt & Chạy

### Yêu cầu
- .NET 8 SDK
- MySQL Server (port 3306)

### Bước 1: Cấu hình database
Sửa connection string trong `EduLMS.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=EduLMS;User=root;Password=YOUR_PASSWORD;CharSet=utf8mb4;"
}
```

### Bước 2: Cấu hình thanh toán (tuỳ chọn)
```json
"VietQR": {
  "BankBIN": "970418",
  "AccountNo": "YOUR_ACCOUNT_NO",
  "AccountName": "YOUR_NAME",
  "Template": "compact2"
},
"SePay": {
  "ApiKey": "YOUR_SEPAY_API_KEY"
}
```

### Bước 3: Chạy
```bash
cd EduLMS.Web
dotnet run
```
App tự tạo database, chạy migration và seed dữ liệu mẫu.

### Bước 4: Webhook (cho local development)
```bash
ngrok http 5099
```
Cấu hình webhook trên SePay dashboard: `https://xxxx.ngrok-free.app/api/webhook/sepay`

## Tài khoản Demo

| Vai trò    | Email                  | Mật khẩu       |
|------------|------------------------|-----------------|
| Admin      | admin@edulms.com       | Admin@123       |
| Instructor | lehoang@edulms.com     | Instructor@123  |
| Instructor | minhtu@edulms.com      | Instructor@123  |
| Instructor | tranbinh@edulms.com    | Instructor@123  |
| Instructor | anna@edulms.com        | Instructor@123  |
| Learner    | alex@example.com       | Learner@123     |
| Learner    | maria@example.com      | Learner@123     |
| Learner    | john@example.com       | Learner@123     |
| Learner    | lcinda@example.om      | Learner@123     |

## Cấu trúc dự án

```
EduLMS.Web/
├── Areas/
│   ├── Admin/          # Dashboard, Users, Roles, Notifications
│   ├── Instructor/     # Dashboard, Courses, Calendar, Grading, Progress
│   └── Learner/        # Dashboard, Training, Catalog, Calendar, Grades, Payment
├── Controllers/        # Home, About, Account, Help, Profile, Settings, Webhook
├── Models/             # Entity models + Enums
├── ViewModels/         # View models cho từng area
├── Data/               # DbContext + SeedData
├── Views/              # Public pages + Shared layouts
└── wwwroot/            # Static files + uploads
```

## Tính năng chính

### Admin
- Dashboard thống kê (biểu đồ Chart.js)
- Quản lý Users (CRUD, toggle status, đổi role)
- Quản lý Roles
- Cấu hình Notifications
- Xuất báo cáo Excel (Users, Courses, Grades, Payments)

### Instructor
- Dashboard (biểu đồ xu hướng, đối chiếu điểm)
- Quản lý khóa học (tạo, sửa, chapters, lessons, resources)
- Lịch giảng dạy (FullCalendar)
- Chấm điểm bài tập
- Theo dõi tiến độ học viên

### Learner
- Dashboard (welcome, stats, resume learning)
- Course Catalog (tìm kiếm, lọc, đăng ký)
- Thanh toán khóa học (QR VietQR + chuyển khoản, xác nhận tự động SePay)
- My Training (tiến độ học tập)
- Xem bài giảng, nộp bài tập
- Lịch học (FullCalendar, export .ics)
- Xem điểm số

### Chung
- Hồ sơ cá nhân (xem, sửa, upload avatar)
- Đổi mật khẩu
- Responsive layout với sidebar riêng cho mỗi role
