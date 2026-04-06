# 3.3 Kiem thu chuc nang (Black-box testing)

## 3.3.1 Muc tieu

Kiem thu chuc nang (black-box) tap trung xac nhan:

1. Dau vao/hanh dong nguoi dung -> dau ra he thong dung nghiep vu.
2. Khong can biet code ben trong, chi danh gia hanh vi ben ngoai.
3. Tat ca luong chinh cua he thong hoc truc tuyen duoc kiem tra theo vai tro Learner/Instructor/Admin.

## 3.3.2 Pham vi, moi truong va du lieu kiem thu

- He thong test: `EcduLMS.Web`
- URL test: `http://localhost:5000`
- Du lieu tai khoan seed:
  - `alex@example.com / Learner@123` (Learner)
  - `admin@edulms.com / Admin@123` (Admin)
  - `lehoang@edulms.com / Instructor@123` (Instructor)
- Browser test dang nhap: `chrome`, `coccoc`, `safari`
- Nguon ket qua thuc te da chay:
  - `tests/1-kiem-thu-tu-dong/login-selenium/artifacts/reports/latest-summary.json`
  - Moc thoi gian: `2026-04-05T08:00:21.199Z` -> `2026-04-05T08:03:25.574Z`

## 3.3.3 Kiem thu function chuc nang Dang nhap

### Bang 3.3.1 - Thanh phan giao dien dang nhap

| STT | Thanh phan | Loai | Mo ta | Rang buoc |
|---|---|---|---|---|
| 1 | Email | Textbox | Nguoi dung nhap email dang nhap | Dung dinh dang email |
| 2 | Password | Password textbox | Nguoi dung nhap mat khau | Khong de trong voi case dang nhap hop le |
| 3 | Nut Dang nhap | Button | Gui thong tin dang nhap | Dieu huong theo role neu hop le |
| 4 | Thong bao loi | Message/validation | Bao loi khi du lieu sai | Hien thi dung theo truong hop |

### Bang 3.3.2 - Tong hop ket qua theo browser (du lieu thuc te da lam lai)

| Browser | Tong case | PASS | FAIL (logic) | INFRA_FAIL |
|---|---:|---:|---:|---:|
| chrome | 15 | 15 | 0 | 0 |
| coccoc | 15 | 15 | 0 | 0 |
| safari | 15 | 0 | 0 | 15 |
| **Tong** | **45** | **30** | **0** | **15** |

### Bang 3.3.3 - Tong hop theo nhom (Chrome va CocCoc sau khi sua script)

| Nhom test | So case/Browser | Chrome | CocCoc | Nhan xet |
|---|---:|---:|---:|---|
| PASS - role learner/admin/instructor | 3 | 3 PASS | 3 PASS | Dang nhap role da duoc xac minh on dinh |
| PASS - data variation | 3 | 3 PASS | 3 PASS | Email viet hoa / khoang trang / learner thu 2 deu pass |
| FAIL - auth | 2 | 2 PASS | 2 PASS | Case ky vong that bai da duoc assert dung |
| FAIL - validation | 4 | 4 PASS | 4 PASS | Validation da duoc danh gia dung theo expected |
| FAIL - security | 3 | 3 PASS | 3 PASS | SQLi/XSS email/password da duoc chan dung |

### Bang 3.3.4 - Tach ro loi ha tang browser va loi logic test

| Phan loai | So case | Browser | Mo ta |
|---|---:|---|---|
| INFRA_FAIL | 15 | Safari | Safari WebDriver chua san sang remote automation => khong tao session |
| FAIL logic | 0 | - | Khong con fail logic tren full run moi nhat |

### Bang 3.3.5 - Ket qua chi tiet tung test case dang nhap (run moi nhat)

| ID | Nhom kiem thu | Ten test case | Ket qua (Chrome) | Ket qua (CocCoc) | Ket qua (Safari) | Loai mong doi | Ghi chu |
|---|---|---|---|---|---|---|---|
| AT_LOGIN_01 | PASS - role learner | Dang nhap thanh cong voi learner | PASS | PASS | INFRA_FAIL | Thanh cong | Safari la loi ha tang |
| AT_LOGIN_02 | PASS - role admin | Dang nhap thanh cong voi admin | PASS | PASS | INFRA_FAIL | Thanh cong | Safari la loi ha tang |
| AT_LOGIN_03 | PASS - role instructor | Dang nhap thanh cong voi instructor | PASS | PASS | INFRA_FAIL | Thanh cong | Safari la loi ha tang |
| AT_LOGIN_04 | PASS - data variation | Email viet hoa | PASS | PASS | INFRA_FAIL | Thanh cong | Safari la loi ha tang |
| AT_LOGIN_05 | PASS - data variation | Tai khoan learner thu 2 | PASS | PASS | INFRA_FAIL | Thanh cong | Safari la loi ha tang |
| AT_LOGIN_06 | FAIL - auth | Sai mat khau | PASS | PASS | INFRA_FAIL | That bai dang nhap | Assert thong bao loi dung |
| AT_LOGIN_07 | FAIL - auth | Email khong ton tai | PASS | PASS | INFRA_FAIL | That bai dang nhap | Assert thong bao loi dung |
| AT_LOGIN_08 | FAIL - validation | Bo trong email | PASS | PASS | INFRA_FAIL | Validation loi | Assert dung |
| AT_LOGIN_09 | FAIL - validation | Bo trong mat khau | PASS | PASS | INFRA_FAIL | Validation loi | Assert dung |
| AT_LOGIN_10 | FAIL - validation | Bo trong ca email va mat khau | PASS | PASS | INFRA_FAIL | Validation loi | Assert dung |
| AT_LOGIN_11 | FAIL - validation | Email sai dinh dang | PASS | PASS | INFRA_FAIL | Browser validation | Lay `validationMessage` |
| AT_LOGIN_12 | PASS - data variation | Email co khoang trang | PASS | PASS | INFRA_FAIL | Thanh cong | Safari la loi ha tang |
| AT_LOGIN_13 | FAIL - security | SQL injection email | PASS | PASS | INFRA_FAIL | Browser validation | Lay `validationMessage` |
| AT_LOGIN_14 | FAIL - security | SQL injection password | PASS | PASS | INFRA_FAIL | That bai dang nhap | Assert thong bao loi dung |
| AT_LOGIN_15 | FAIL - security | Email chua script (XSS) | PASS | PASS | INFRA_FAIL | Browser validation | Lay `validationMessage` |

## 3.3.4 Kiem thu function My Training va tien do hoc tap (black-box)

Luu y: nhom case duoi day da duoc thiet ke theo nghiep vu moi cua he thong. Trang thai hien tai la **CHUA CHAY black-box E2E** (can thuc hien test manual/trinh duyet de lay PASS/FAIL thuc te).

### Bang 3.3.6 - Test case My Training

| Ma TC | Ten test case | Buoc test chinh | Ket qua mong doi | Trang thai |
|---|---|---|---|---|
| FT_TRAIN_01 | Hien thi khoa hoc dang hoc/hoan thanh | Vao `Learner/MyTraining` | Khoa hoc duoc tach dung 2 nhom, hien thi % tien do | CHUA CHAY |
| FT_TRAIN_02 | Hoan thanh bai hoc tang tien do | Mo bai hoc -> bam `Xac nhan hoan thanh` | % course tang len theo so bai da hoan thanh | CHUA CHAY |
| FT_TRAIN_03 | Bai da hoan thanh khong hien nut xac nhan nua | Mo lai bai da complete | Hien "Da hoan thanh", khong con nut xac nhan | CHUA CHAY |
| FT_TRAIN_04 | Chan hoc nhay coc bai tiep theo | Thu vao bai N+1 khi bai N chua complete | He thong chan truy cap, thong bao phai hoan thanh bai truoc | CHUA CHAY |
| FT_TRAIN_05 | Mo khoa bai tiep theo sau khi complete bai truoc | Complete bai N -> mo bai N+1 | Bai N+1 bo trang thai khoa, vao hoc duoc | CHUA CHAY |
| FT_TRAIN_06 | Hoan thanh toan bo khoa hoc | Complete tat ca bai | Progress = 100%, trang thai enrollment = Completed | CHUA CHAY |

## 3.3.5 Kiem thu function Grades va Assignment access control (black-box)

Luu y: nhom case duoi day da duoc thiet ke theo nghiep vu da cap nhat ("chua nop thi bam Lam bai ngay", "chua hoc den bai tap thi chan va bao ly do"). Trang thai hien tai: **CHUA CHAY black-box E2E**.

### Bang 3.3.7 - Test case Grades/Assignment

| Ma TC | Ten test case | Buoc test chinh | Ket qua mong doi | Trang thai |
|---|---|---|---|---|
| FT_GRADE_01 | Danh sach bai tap trong Course Detail | Vao `Learner/Grades/CourseDetail/{courseId}` | Hien dung trang thai: Chua nop/Chua cham/Pass/Fail | CHUA CHAY |
| FT_GRADE_02 | Chua nop + du dieu kien -> Lam bai ngay | Tai bai "Chua nop", bam `Lam bai ngay` | Dieu huong den `Learner/Assignment/Submit/{id}` | CHUA CHAY |
| FT_GRADE_03 | Chua nop + chua du dieu kien | Bai tap chua hoc den | Hien `Chua mo bai tap` + thong diep "Ban hay hoan thanh khoa hoc do truoc." | CHUA CHAY |
| FT_GRADE_04 | Nop bai thanh cong | Tu trang submit, nop bai | Trang thai chuyen sang `Chua cham` hoac `Da nop` | CHUA CHAY |
| FT_GRADE_05 | Bai da cham co diem | Co bai da grade | Hien diem so, badge Pass/Fail va vao duoc chi tiet diem | CHUA CHAY |
| FT_GRADE_06 | Khong cho sua bai da nop | Bai da submit, quay lai SaveDraft/Submit | He thong khong cho ghi de bai da nop (theo quy tac) | CHUA CHAY |

## 3.3.6 Kiem thu function Lich hoc/Lich day (black-box)

Luu y: nhom case duoi day tap trung vao yeu cau bo sung bo loc xem theo `Ngay/Tuan/Thang` cho ca Learner va Instructor. Trang thai hien tai: **CHUA CHAY black-box E2E**.

### Bang 3.3.8 - Test case Calendar

| Ma TC | Doi tuong | Ten test case | Buoc test chinh | Ket qua mong doi | Trang thai |
|---|---|---|---|---|---|
| FT_CAL_01 | Learner | Chuyen view Ngay/Tuan/Thang | Vao `Learner/Calendar`, doi tung view | Lich doi view dung, title theo view cap nhat dung | CHUA CHAY |
| FT_CAL_02 | Instructor | Chuyen view Ngay/Tuan/Thang | Vao `Instructor/Calendar`, doi tung view | Lich doi view dung, khong vo layout | CHUA CHAY |
| FT_CAL_03 | Learner | Render su kien theo loai | Tai view lich hoc | Mau su kien dung (Class/Assignment/Personal) | CHUA CHAY |
| FT_CAL_04 | Instructor | Tao lich day lap lai theo tuan | Tao lich voi recurrence weeks > 1 | Tao du so buoi, hien dung tren lich | CHUA CHAY |
| FT_CAL_05 | Instructor | Sua/Xoa lich day | Mo popup su kien -> Sua/Xoa | Du lieu cap nhat dung tren grid + danh sach sap toi | CHUA CHAY |
| FT_CAL_06 | Learner + Instructor | Sidebar su kien sap toi | Kiem tra box su kien ben phai | Sap xep dung theo thoi gian, nhom theo buoi hop ly | CHUA CHAY |

## 3.3.7 Tong ket muc 3.3

### Viec da thuc hien duoc

1. Da trinh bay lai muc 3.3 theo dinh dang bang black-box giong file mau:
   - Co phan muc tieu, moi truong, bang test case, bang tong hop ket qua.
2. Da dua so lieu thuc te tu bo automation dang nhap (`45` case) vao bao cao.
3. Da tach ro:
   - Loi ha tang browser (Safari setup): `15` case (`INFRA_FAIL`).
   - Loi logic script: `0` case tren lan chay moi nhat.
4. Da bo sung bo test case black-box day du cho My Training, Grades, Calendar de tiep tuc chay thu cong/E2E.
5. Da cap nhat script automation login de phan loai rieng `PASS / FAIL(logic) / INFRA_FAIL`.

### Viec chua thuc hien trong dot nay

1. Chua chay black-box E2E cho My Training, Grades, Calendar de dong PASS/FAIL thuc te.
2. Chua co bo artifact manual regression day du cho cac bang UI/Function thuoc Admin va Instructor.
