# Bao cao kiem thu tu dong - Dang nhap (chrome)

- Browser: chrome
- Thoi gian bat dau: 2026-04-05T07:37:05.294Z
- Thoi gian ket thuc: 2026-04-05T07:49:11.990Z
- Tong so case: 15
- PASS: 0
- FAIL (logic): 15
- INFRA_FAIL: 0

| ID | Nhom | Ten test case | Ket qua | Loai loi | URL thuc te | Validation message | Loi (neu co) | Screenshot |
|---|---|---|---|---|---|---|---|---|
| AT_LOGIN_01 | PASS - role learner | Dang nhap thanh cong voi learner | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15019ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-143815_chrome_AT_LOGIN_01_FAIL.png |
| AT_LOGIN_02 | PASS - role admin | Dang nhap thanh cong voi admin | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15028ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-143902_chrome_AT_LOGIN_02_FAIL.png |
| AT_LOGIN_03 | PASS - role instructor | Dang nhap thanh cong voi instructor | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15011ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-143949_chrome_AT_LOGIN_03_FAIL.png |
| AT_LOGIN_04 | PASS - data variation | Dang nhap thanh cong voi learner email viet hoa | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15005ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-144036_chrome_AT_LOGIN_04_FAIL.png |
| AT_LOGIN_05 | PASS - data variation | Dang nhap thanh cong voi tai khoan learner thu 2 | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15002ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-144122_chrome_AT_LOGIN_05_FAIL.png |
| AT_LOGIN_06 | FAIL - auth | Dang nhap that bai khi sai mat khau | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15005ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-144209_chrome_AT_LOGIN_06_FAIL.png |
| AT_LOGIN_07 | FAIL - auth | Dang nhap that bai voi email khong ton tai | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15202ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-144256_chrome_AT_LOGIN_07_FAIL.png |
| AT_LOGIN_08 | FAIL - validation | Dang nhap that bai khi bo trong email | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15181ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-144343_chrome_AT_LOGIN_08_FAIL.png |
| AT_LOGIN_09 | FAIL - validation | Dang nhap that bai khi bo trong mat khau | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15190ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-144430_chrome_AT_LOGIN_09_FAIL.png |
| AT_LOGIN_10 | FAIL - validation | Dang nhap that bai khi bo trong ca email va mat khau | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15185ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-144517_chrome_AT_LOGIN_10_FAIL.png |
| AT_LOGIN_11 | FAIL - validation | Dang nhap that bai khi email sai dinh dang | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15183ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-144604_chrome_AT_LOGIN_11_FAIL.png |
| AT_LOGIN_12 | PASS - data variation | Dang nhap thanh cong khi email co khoang trang dau/cuoi | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15190ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-144651_chrome_AT_LOGIN_12_FAIL.png |
| AT_LOGIN_13 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o email | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15043ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-144737_chrome_AT_LOGIN_13_FAIL.png |
| AT_LOGIN_14 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o mat khau | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15016ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-144824_chrome_AT_LOGIN_14_FAIL.png |
| AT_LOGIN_15 | FAIL - security | Dang nhap that bai voi email chua script | FAIL | ASSERTION | http://localhost:5000/Account/Login |  | Waiting for element to be located By(css selector, *[id="Email"])
Wait timed out after 15014ms | /Users/doanhnguyen/Documents/tài liệu/phầm mềm/WEB nâng cao/tests/1-kiem-thu-tu-dong/login-selenium/artifacts/screenshots/20260405-144911_chrome_AT_LOGIN_15_FAIL.png |

## Nhan xet nhanh
- Co case FAIL logic (khong dat expected theo nghiep vu test).
- Xem chi tiet trong cac truong `errorType`, `errorMessage`, `observedValidationMessage` cua JSON.