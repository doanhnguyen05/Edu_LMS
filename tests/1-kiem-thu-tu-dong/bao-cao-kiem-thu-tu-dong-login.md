# 1.1 Kiem thu tu dong

Kiem thu tu dong la hoat dong dung cong cu de mo phong thao tac cua nguoi dung va tu dong kiem tra ket qua thuc te so voi ket qua mong doi. So voi kiem thu thu cong, kiem thu tu dong giam thao tac lap lai, tang do chinh xac va de chay lai sau moi lan cap nhat he thong.

Trong de tai nay, nhom thuc hien kiem thu tu dong cho chuc nang **Dang nhap** vi day la chuc nang cot loi, co luong ro rang, co dau vao dau ra cu the va duoc su dung thuong xuyen boi tat ca vai tro nguoi dung.

## 1.1.1 Muc tieu kiem thu tu dong

Viec kiem thu tu dong chuc nang Dang nhap nham cac muc tieu sau:

1. Kiem tra dang nhap thanh cong khi nguoi dung nhap dung email va mat khau.
2. Kiem tra he thong xu ly dung khi mat khau sai.
3. Kiem tra he thong hien thi thong bao loi khi bo trong email.
4. Kiem tra he thong hien thi thong bao loi khi bo trong mat khau.
5. Kiem tra he thong hien thi thong bao loi khi email sai dinh dang.
6. Kiem tra kha nang dieu huong den trang Dashboard sau khi dang nhap thanh cong.
7. Tao bo kiem thu co the chay lai cho kiem thu hoi quy.

## 1.1.2 Cong cu su dung

Cong cu va moi truong su dung:

1. Selenium WebDriver.
2. JavaScript (Node.js) de viet script test.
3. Trinh duyet Coc Coc, Safari va Chrome (kiem tra da trinh duyet).
4. ASP.NET MVC (.NET 8) la he thong duoc kiem thu.
5. MySQL la co so du lieu cua he thong.
6. VS Code / Rider de viet va chay ma kiem thu.

Ly do chon Selenium:

1. Mo phong hanh vi nguoi dung tren trinh duyet.
2. De tich hop voi he thong web.
3. Ho tro chup screenshot minh chung ket qua moi test case.

## 1.1.3 Chuc nang duoc chon kiem thu tu dong

Chuc nang duoc chon: **Dang nhap he thong**.

Danh sach tinh huong kiem thu (15 case):

1. Nhom PASS: Learner, Admin, Instructor, email viet hoa, learner thu 2.
2. Nhom FAIL xac thuc: sai mat khau, email khong ton tai.
3. Nhom FAIL validation: trong email, trong mat khau, trong ca hai, email sai dinh dang, email co khoang trang.
4. Nhom FAIL bao mat co ban: SQL injection o email, SQL injection o mat khau, email chua script.

## 1.1.4 Moi truong kiem thu tu dong

Moi truong duoc thiet lap nhu sau:

1. Ung dung web ASP.NET chay local tai `http://localhost:5000`.
2. Duong dan trang dang nhap: `/Account/Login`.
3. Co so du lieu MySQL duoc seed du lieu tai khoan mau.
4. Trinh duyet chay test: Coc Coc, Safari, Chrome.
5. Tai khoan test hop le (seed san):
   - `alex@example.com / Learner@123` (Learner)
   - `admin@edulms.com / Admin@123` (Admin, neu muon doi role test)

## 1.1.5 Quy trinh thuc hien kiem thu tu dong

Quy trinh chi tiet:

1. Khoi dong ASP.NET app va dam bao trang login truy cap duoc.
2. Khoi dong browser bang Selenium WebDriver.
3. Truy cap URL login.
4. Tim cac phan tu nhap lieu:
   - `#Email`
   - `#Password`
   - `button[type='submit']`
5. Nhap du lieu test theo tung test case.
6. Bam nut Dang nhap.
7. Cho he thong phan hoi.
8. Kiem tra ket qua:
   - Neu case thanh cong: URL phai chua dashboard mong doi.
   - Neu case that bai: van o trang login va xuat hien thong bao loi mong doi.
9. Chup screenshot minh chung.
10. Ghi ket qua PASS/FAIL vao bao cao tong hop JSON + Markdown.
11. Dong browser.

## 1.1.6 Ma kiem thu

Project ma kiem thu dat tai:

- `tests/1-kiem-thu-tu-dong/login-selenium`

Thanh phan chinh:

1. `scripts/login-automation.js`: script chay test login.
2. `.env`: cau hinh URL, tai khoan test, duong dan Coc Coc, danh sach browser.
3. `artifacts/reports`: luu bao cao ket qua.
4. `artifacts/screenshots`: luu screenshot tung test case.

Bo test gom 15 case:

1. `AT_LOGIN_01` den `AT_LOGIN_05`: cac case PASS.
2. `AT_LOGIN_06` den `AT_LOGIN_07`: fail xac thuc.
3. `AT_LOGIN_08` den `AT_LOGIN_12`: fail validation.
4. `AT_LOGIN_13` den `AT_LOGIN_15`: fail bao mat co ban.

## 1.1.7 Ket qua mong doi

Neu du lieu hop le:

1. He thong dang nhap thanh cong.
2. Dieu huong den dung trang Dashboard.
3. Khong hien thi thong bao loi dang nhap.

Neu du lieu khong hop le:

1. He thong khong cho phep dang nhap.
2. Van o trang login.
3. Hien thi thong bao loi phu hop voi tung truong hop.

## Huong dan lay ket qua thuc te de dua vao bao cao

### Buoc A - Cai dat bo test

1. Mo terminal.
2. Chay:

```bash
cd "tests/1-kiem-thu-tu-dong/login-selenium"
npm install
cp .env.example .env
```

3. Mo `.env` va cap nhat dung thong tin cua may ban.

### Buoc B - Khoi dong he thong duoc test

1. Terminal 1:

```bash
cd EduLMS.Web
dotnet run
```

2. Xac nhan mo duoc: `http://localhost:5000/Account/Login`

### Buoc C - Chay automation

1. Terminal 2:

```bash
cd "tests/1-kiem-thu-tu-dong/login-selenium"
npm run test:login
```

Lenh tren se hien menu cho chon browser (vi du: chi Safari).

Hoac chay tat ca browser:

```bash
npm run test:login:all
```

2. Doi script chay het 15 test case tren tung browser da chon.

### Buoc D - Thu ket qua

Sau khi chay xong, lay file:

1. `artifacts/reports/latest-summary.md` (tong hop tat ca browser).
2. `artifacts/reports/latest-summary-coccoc.md` (chi tiet Coc Coc).
3. `artifacts/reports/latest-summary-safari.md` (chi tiet Safari).
4. `artifacts/reports/latest-summary-chrome.md` (chi tiet Chrome).
5. `artifacts/reports/latest-summary.json` (du lieu ky thuat tong hop).
6. Toan bo anh trong `artifacts/screenshots/` (minh chung Pass/Fail).

### Buoc E - Dien vao bang ket qua cua de tai

Su dung bang mau sau (dien day du cho 15 case):

| Ma case | Mo ta | Ket qua mong doi | Ket qua thuc te | Ket luan | Minh chung |
|---|---|---|---|---|---|
| AT_LOGIN_01 | Learner dang nhap hop le | Vao Learner Dashboard | ... | PASS/FAIL | ten file png |
| AT_LOGIN_02 | Admin dang nhap hop le | Vao Admin Dashboard | ... | PASS/FAIL | ten file png |
| AT_LOGIN_03 | Instructor dang nhap hop le | Vao Instructor Dashboard | ... | PASS/FAIL | ten file png |
| AT_LOGIN_04 | Learner email viet hoa | Dang nhap thanh cong | ... | PASS/FAIL | ten file png |
| AT_LOGIN_05 | Learner thu 2 dang nhap | Dang nhap thanh cong | ... | PASS/FAIL | ten file png |
| AT_LOGIN_06 | Sai mat khau | Bao loi sai thong tin | ... | PASS/FAIL | ten file png |
| AT_LOGIN_07 | Email khong ton tai | Bao loi sai thong tin | ... | PASS/FAIL | ten file png |
| AT_LOGIN_08 | Trong email | Bao loi nhap email | ... | PASS/FAIL | ten file png |
| AT_LOGIN_09 | Trong mat khau | Bao loi nhap mat khau | ... | PASS/FAIL | ten file png |
| AT_LOGIN_10 | Trong email va mat khau | Hien thi 2 thong bao bat buoc | ... | PASS/FAIL | ten file png |
| AT_LOGIN_11 | Email sai dinh dang | Bao loi email khong hop le | ... | PASS/FAIL | ten file png |
| AT_LOGIN_12 | Email co khoang trang | Bao loi email khong hop le | ... | PASS/FAIL | ten file png |
| AT_LOGIN_13 | SQLi trong email | Khong dang nhap duoc | ... | PASS/FAIL | ten file png |
| AT_LOGIN_14 | SQLi trong mat khau | Bao loi sai thong tin | ... | PASS/FAIL | ten file png |
| AT_LOGIN_15 | Email chua script | Khong dang nhap duoc | ... | PASS/FAIL | ten file png |
