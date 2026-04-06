# Bao cao kiem thu tu dong - Dang nhap (coccoc)

- Browser: coccoc
- Thoi gian bat dau: 2026-03-31T10:12:05.195Z
- Thoi gian ket thuc: 2026-03-31T10:15:06.153Z
- Tong so case: 15
- PASS: 6
- FAIL: 9

| ID | Nhom | Ten test case | Ket qua | URL thuc te | Loi (neu co) | Screenshot |
|---|---|---|---|---|---|---|
| AT_LOGIN_01 | PASS - role learner | Dang nhap thanh cong voi learner | PASS | http://localhost:5000/Learner |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171216_coccoc_AT_LOGIN_01_PASS.png |
| AT_LOGIN_02 | PASS - role admin | Dang nhap thanh cong voi admin | PASS | http://localhost:5000/Admin |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171219_coccoc_AT_LOGIN_02_PASS.png |
| AT_LOGIN_03 | PASS - role instructor | Dang nhap thanh cong voi instructor | PASS | http://localhost:5000/Instructor |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171221_coccoc_AT_LOGIN_03_PASS.png |
| AT_LOGIN_04 | PASS - data variation | Dang nhap thanh cong voi learner email viet hoa | PASS | http://localhost:5000/Learner |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171224_coccoc_AT_LOGIN_04_PASS.png |
| AT_LOGIN_05 | PASS - data variation | Dang nhap thanh cong voi tai khoan learner thu 2 | PASS | http://localhost:5000/Learner |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171226_coccoc_AT_LOGIN_05_PASS.png |
| AT_LOGIN_06 | FAIL - auth | Dang nhap that bai khi sai mat khau | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15149ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171244_coccoc_AT_LOGIN_06_FAIL.png |
| AT_LOGIN_07 | FAIL - auth | Dang nhap that bai voi email khong ton tai | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15036ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171301_coccoc_AT_LOGIN_07_FAIL.png |
| AT_LOGIN_08 | FAIL - validation | Dang nhap that bai khi bo trong email | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15067ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171318_coccoc_AT_LOGIN_08_FAIL.png |
| AT_LOGIN_09 | FAIL - validation | Dang nhap that bai khi bo trong mat khau | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15066ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171335_coccoc_AT_LOGIN_09_FAIL.png |
| AT_LOGIN_10 | FAIL - validation | Dang nhap that bai khi bo trong ca email va mat khau | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15027ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171352_coccoc_AT_LOGIN_10_FAIL.png |
| AT_LOGIN_11 | FAIL - validation | Dang nhap that bai khi email sai dinh dang | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15172ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171410_coccoc_AT_LOGIN_11_FAIL.png |
| AT_LOGIN_12 | PASS - data variation | Dang nhap thanh cong khi email co khoang trang dau/cuoi | PASS | http://localhost:5000/Learner |  | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171413_coccoc_AT_LOGIN_12_PASS.png |
| AT_LOGIN_13 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o email | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15197ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171430_coccoc_AT_LOGIN_13_FAIL.png |
| AT_LOGIN_14 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o mat khau | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15024ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171448_coccoc_AT_LOGIN_14_FAIL.png |
| AT_LOGIN_15 | FAIL - security | Dang nhap that bai voi email chua script | FAIL | http://localhost:5000/Account/Login | Khong dang nhap duoc vao he thong.
Wait timed out after 15017ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260331-171506_coccoc_AT_LOGIN_15_FAIL.png |

## Nhan xet nhanh
- Co case FAIL, nghia la khong dang nhap duoc vao he thong.
- Xem chi tiet trong truong `errorMessage` cua bao cao JSON.