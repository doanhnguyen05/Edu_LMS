# Bao cao kiem thu tu dong - Dang nhap (safari)

- Browser: safari
- Thoi gian bat dau: 2026-03-31T10:15:06.161Z
- Thoi gian ket thuc: 2026-03-31T10:17:33.368Z
- Tong so case: 15
- PASS: 0
- FAIL: 15

| ID | Nhom | Ten test case | Ket qua | URL thuc te | Loi (neu co) | Screenshot |
|---|---|---|---|---|---|---|
| AT_LOGIN_01 | PASS - role learner | Dang nhap thanh cong voi learner | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_02 | PASS - role admin | Dang nhap thanh cong voi admin | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_03 | PASS - role instructor | Dang nhap thanh cong voi instructor | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_04 | PASS - data variation | Dang nhap thanh cong voi learner email viet hoa | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_05 | PASS - data variation | Dang nhap thanh cong voi tai khoan learner thu 2 | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_06 | FAIL - auth | Dang nhap that bai khi sai mat khau | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_07 | FAIL - auth | Dang nhap that bai voi email khong ton tai | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_08 | FAIL - validation | Dang nhap that bai khi bo trong email | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_09 | FAIL - validation | Dang nhap that bai khi bo trong mat khau | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_10 | FAIL - validation | Dang nhap that bai khi bo trong ca email va mat khau | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_11 | FAIL - validation | Dang nhap that bai khi email sai dinh dang | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_12 | PASS - data variation | Dang nhap thanh cong khi email co khoang trang dau/cuoi | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_13 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o email | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_14 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o mat khau | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_15 | FAIL - security | Dang nhap that bai voi email chua script | FAIL |  | Khong dang nhap duoc vao he thong. Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |

## Nhan xet nhanh
- Co case FAIL, nghia la khong dang nhap duoc vao he thong.
- Xem chi tiet trong truong `errorMessage` cua bao cao JSON.