# Bao cao kiem thu tu dong - Dang nhap (chrome)

- Browser: chrome
- Thoi gian bat dau: 2026-03-31T10:17:33.371Z
- Thoi gian ket thuc: 2026-03-31T10:20:32.955Z
- Tong so case: 15
- PASS: 6
- FAIL: 9

| ID | Nhom | Ten test case | Ket qua | URL thuc te | Loi (neu co) | Screenshot |
|---|---|---|---|---|---|---|
| AT_LOGIN_01 | PASS - role learner | Dang nhap thanh cong voi learner | PASS | http://localhost:5000/Learner |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171743_chrome_AT_LOGIN_01_PASS.png |
| AT_LOGIN_02 | PASS - role admin | Dang nhap thanh cong voi admin | PASS | http://localhost:5000/Admin |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171746_chrome_AT_LOGIN_02_PASS.png |
| AT_LOGIN_03 | PASS - role instructor | Dang nhap thanh cong voi instructor | PASS | http://localhost:5000/Instructor |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171749_chrome_AT_LOGIN_03_PASS.png |
| AT_LOGIN_04 | PASS - data variation | Dang nhap thanh cong voi learner email viet hoa | PASS | http://localhost:5000/Learner |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171751_chrome_AT_LOGIN_04_PASS.png |
| AT_LOGIN_05 | PASS - data variation | Dang nhap thanh cong voi tai khoan learner thu 2 | PASS | http://localhost:5000/Learner |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171754_chrome_AT_LOGIN_05_PASS.png |
| AT_LOGIN_06 | FAIL - auth | Dang nhap that bai khi sai mat khau | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15127ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171811_chrome_AT_LOGIN_06_FAIL.png |
| AT_LOGIN_07 | FAIL - auth | Dang nhap that bai voi email khong ton tai | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15022ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171828_chrome_AT_LOGIN_07_FAIL.png |
| AT_LOGIN_08 | FAIL - validation | Dang nhap that bai khi bo trong email | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15008ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171845_chrome_AT_LOGIN_08_FAIL.png |
| AT_LOGIN_09 | FAIL - validation | Dang nhap that bai khi bo trong mat khau | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15075ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171902_chrome_AT_LOGIN_09_FAIL.png |
| AT_LOGIN_10 | FAIL - validation | Dang nhap that bai khi bo trong ca email va mat khau | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15037ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171919_chrome_AT_LOGIN_10_FAIL.png |
| AT_LOGIN_11 | FAIL - validation | Dang nhap that bai khi email sai dinh dang | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15201ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171937_chrome_AT_LOGIN_11_FAIL.png |
| AT_LOGIN_12 | PASS - data variation | Dang nhap thanh cong khi email co khoang trang dau/cuoi | PASS | http://localhost:5000/Learner |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171940_chrome_AT_LOGIN_12_PASS.png |
| AT_LOGIN_13 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o email | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15179ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171957_chrome_AT_LOGIN_13_FAIL.png |
| AT_LOGIN_14 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o mat khau | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15000ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-172015_chrome_AT_LOGIN_14_FAIL.png |
| AT_LOGIN_15 | FAIL - security | Dang nhap that bai voi email chua script | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15189ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-172032_chrome_AT_LOGIN_15_FAIL.png |

## Nhan xet nhanh
- Co case FAIL, nghia la khong dang nhap duoc vao he thong.
- Xem chi tiet trong truong `errorMessage` cua bao cao JSON.