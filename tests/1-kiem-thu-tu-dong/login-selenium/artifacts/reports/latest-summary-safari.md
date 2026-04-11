# Bao cao kiem thu tu dong - Dang nhap (safari)

- Browser: safari
- Thoi gian bat dau: 2026-04-10T08:45:10.965Z
- Thoi gian ket thuc: 2026-04-10T08:46:08.286Z
- Tong so case: 15
- PASS: 15
- FAIL (logic): 0
- INFRA_FAIL: 0

| ID | Nhom | Ten test case | Ket qua | Loai loi | URL thuc te | Validation message | Loi (neu co) | Screenshot |
|---|---|---|---|---|---|---|---|---|
| AT_LOGIN_01 | PASS - role learner | Dang nhap thanh cong voi learner | PASS |  | http://localhost:5000/Learner |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154515_safari_AT_LOGIN_01_PASS.png |
| AT_LOGIN_02 | PASS - role admin | Dang nhap thanh cong voi admin | PASS |  | http://localhost:5000/Admin |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154524_safari_AT_LOGIN_02_PASS.png |
| AT_LOGIN_03 | PASS - role instructor | Dang nhap thanh cong voi instructor | PASS |  | http://localhost:5000/Instructor |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154527_safari_AT_LOGIN_03_PASS.png |
| AT_LOGIN_04 | PASS - data variation | Dang nhap thanh cong voi learner email viet hoa | PASS |  | http://localhost:5000/Learner |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154532_safari_AT_LOGIN_04_PASS.png |
| AT_LOGIN_05 | PASS - data variation | Dang nhap thanh cong voi tai khoan learner thu 2 | PASS |  | http://localhost:5000/Learner |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154536_safari_AT_LOGIN_05_PASS.png |
| AT_LOGIN_06 | FAIL - auth | Dang nhap that bai khi sai mat khau | PASS |  | http://localhost:5000/Account/Login |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154540_safari_AT_LOGIN_06_PASS.png |
| AT_LOGIN_07 | FAIL - auth | Dang nhap that bai voi email khong ton tai | PASS |  | http://localhost:5000/Account/Login |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154543_safari_AT_LOGIN_07_PASS.png |
| AT_LOGIN_08 | FAIL - validation | Dang nhap that bai khi bo trong email | PASS |  | http://localhost:5000/Account/Login |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154546_safari_AT_LOGIN_08_PASS.png |
| AT_LOGIN_09 | FAIL - validation | Dang nhap that bai khi bo trong mat khau | PASS |  | http://localhost:5000/Account/Login |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154549_safari_AT_LOGIN_09_PASS.png |
| AT_LOGIN_10 | FAIL - validation | Dang nhap that bai khi bo trong ca email va mat khau | PASS |  | http://localhost:5000/Account/Login |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154552_safari_AT_LOGIN_10_PASS.png |
| AT_LOGIN_11 | FAIL - validation | Dang nhap that bai khi email sai dinh dang | PASS |  | http://localhost:5000/Account/Login |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154554_safari_AT_LOGIN_11_PASS.png |
| AT_LOGIN_12 | PASS - data variation | Dang nhap thanh cong khi email co khoang trang dau/cuoi | PASS |  | http://localhost:5000/Learner |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154557_safari_AT_LOGIN_12_PASS.png |
| AT_LOGIN_13 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o email | PASS |  | http://localhost:5000/Account/Login |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154601_safari_AT_LOGIN_13_PASS.png |
| AT_LOGIN_14 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o mat khau | PASS |  | http://localhost:5000/Account/Login |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154604_safari_AT_LOGIN_14_PASS.png |
| AT_LOGIN_15 | FAIL - security | Dang nhap that bai voi email chua script | PASS |  | http://localhost:5000/Account/Login |  |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260410-154607_safari_AT_LOGIN_15_PASS.png |

## Nhan xet nhanh
- Tat ca test case deu PASS.