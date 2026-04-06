# Test cases chuc nang Dang nhap (AT_LOGIN_01 -> AT_LOGIN_15)

Bang duoi day dong bo voi script:
`tests/1-kiem-thu-tu-dong/login-selenium/scripts/login-automation.js`

| ID | Nhom kiem thu | Ten test case | Loai mong doi |
|---|---|---|---|
| AT_LOGIN_01 | PASS - role learner | Dang nhap thanh cong voi learner | Thanh cong |
| AT_LOGIN_02 | PASS - role admin | Dang nhap thanh cong voi admin | Thanh cong |
| AT_LOGIN_03 | PASS - role instructor | Dang nhap thanh cong voi instructor | Thanh cong |
| AT_LOGIN_04 | PASS - data variation | Dang nhap thanh cong voi learner email viet hoa | Thanh cong |
| AT_LOGIN_05 | PASS - data variation | Dang nhap thanh cong voi tai khoan learner thu 2 | Thanh cong |
| AT_LOGIN_06 | FAIL - auth | Dang nhap that bai khi sai mat khau | Loi he thong |
| AT_LOGIN_07 | FAIL - auth | Dang nhap that bai voi email khong ton tai | Loi he thong |
| AT_LOGIN_08 | FAIL - validation | Dang nhap that bai khi bo trong email | Loi he thong |
| AT_LOGIN_09 | FAIL - validation | Dang nhap that bai khi bo trong mat khau | Loi he thong |
| AT_LOGIN_10 | FAIL - validation | Dang nhap that bai khi bo trong ca email va mat khau | Loi he thong |
| AT_LOGIN_11 | FAIL - validation | Dang nhap that bai khi email sai dinh dang | Loi Browser |
| AT_LOGIN_12 | PASS - data variation | Dang nhap thanh cong khi email co khoang trang dau/cuoi | Thanh cong |
| AT_LOGIN_13 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o email | Loi Browser |
| AT_LOGIN_14 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o mat khau | Loi he thong |
| AT_LOGIN_15 | FAIL - security | Dang nhap that bai voi email chua script | Loi Browser |

## Tong hop ket qua thuc te lan chay gan nhat

Nguon: `tests/1-kiem-thu-tu-dong/login-selenium/artifacts/reports/latest-summary.json`

| Browser | Tong case | PASS | FAIL |
|---|---:|---:|---:|
| chrome | 15 | 6 | 9 |
| coccoc | 15 | 6 | 9 |
| safari | 15 | 0 | 15 |

Ghi chu nhanh:

1. Safari fail do ha tang WebDriver (`Allow remote automation` chua bat).
2. Chrome/CocCoc pass toan bo nhom dang nhap thanh cong.
3. Cac case auth/validation/security dang bi danh FAIL theo logic script hien tai (script chi PASS khi login vao he thong thanh cong).
