# 🚀 Startup Guide - EduLMS

## Lệnh khởi động dự án

Chạy các lệnh sau theo thứ tự để bắt đầu dự án:

### 1️⃣ Khởi động MySQL
```bash
brew services start mysql
```

### 2️⃣ Tạo/Kiểm tra Database
```bash
mysql -u root -p07012005 -e "CREATE DATABASE IF NOT EXISTS EduLMS;"
```

### 3️⃣ Chạy ứng dụng ASP.NET (Terminal 1)
```bash
cd EduLMS.Web
dotnet run
```
- App sẽ chạy trên: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### 4️⃣ Expose via ngrok (Terminal 2 - tuỳ chọn)
```bash
ngrok http 5000
```
- Public URL sẽ hiển thị trong terminal ngrok
- Dùng cho webhooks: `https://xxxx.ngrok-free.app/api/webhook/sepay`


5. Run MySQL Docker
open -a Docker
docker run -d \
  --name mysql \
  -e MYSQL_ROOT_PASSWORD=root \
  -e MYSQL_DATABASE=edulms \
  -p 3306:3306 \
  mysql:8.0

---

## 📊 Tài khoản Demo

| Vai trò    | Email              | Mật khẩu      |
|------------|--------------------|---------------|
| Admin      | admin@edulms.com   | Admin@123     |
| Instructor | lehoang@edulms.com | Instructor@123|
| Learner    | alex@example.com   | Learner@123   |

---

## 🔗 Các URL quan trọng

| Dịch vụ | URL |
|---------|-----|
| App Local | http://localhost:5000 |
| App HTTPS | https://localhost:5001 |
| ngrok Dashboard | http://127.0.0.1:4040 |
| MySQL | localhost:3306 |

---

## 🛠️ One-liner (chạy tất cả cùng lúc)

Nếu muốn, mở 2 terminal và chạy:

**Terminal 1 (App):**
```bash
brew services start mysql && cd EduLMS.Web && dotnet run
```

**Terminal 2 (ngrok - sau khi app khởi động xong):**
```bash
ngrok http 5000
```

---

## ⚠️ Troubleshooting

**App không kết nối MySQL?**
- Kiểm tra MySQL đang chạy: `brew services list | grep mysql`
- Restart MySQL: `brew services restart mysql`

**Port 5000/5001 đang được dùng?**
- Kill process cũ: `pkill -f "dotnet run"`

**ngrok có lỗi?**
- Đảm bảo app đang chạy trên port 5000 trước
- Restart ngrok và đợi connection thiết lập
