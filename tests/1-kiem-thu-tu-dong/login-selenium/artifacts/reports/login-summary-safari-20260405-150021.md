# Bao cao kiem thu tu dong - Dang nhap (safari)

- Browser: safari
- Thoi gian bat dau: 2026-04-05T08:00:45.214Z
- Thoi gian ket thuc: 2026-04-05T08:03:11.022Z
- Tong so case: 15
- PASS: 0
- FAIL (logic): 0
- INFRA_FAIL: 15

| ID | Nhom | Ten test case | Ket qua | Loai loi | URL thuc te | Validation message | Loi (neu co) | Screenshot |
|---|---|---|---|---|---|---|---|---|
| AT_LOGIN_01 | PASS - role learner | Dang nhap thanh cong voi learner | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_02 | PASS - role admin | Dang nhap thanh cong voi admin | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_03 | PASS - role instructor | Dang nhap thanh cong voi instructor | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_04 | PASS - data variation | Dang nhap thanh cong voi learner email viet hoa | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_05 | PASS - data variation | Dang nhap thanh cong voi tai khoan learner thu 2 | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_06 | FAIL - auth | Dang nhap that bai khi sai mat khau | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_07 | FAIL - auth | Dang nhap that bai voi email khong ton tai | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_08 | FAIL - validation | Dang nhap that bai khi bo trong email | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_09 | FAIL - validation | Dang nhap that bai khi bo trong mat khau | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_10 | FAIL - validation | Dang nhap that bai khi bo trong ca email va mat khau | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_11 | FAIL - validation | Dang nhap that bai khi email sai dinh dang | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_12 | PASS - data variation | Dang nhap thanh cong khi email co khoang trang dau/cuoi | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_13 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o email | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_14 | FAIL - security | Dang nhap that bai voi chuoi SQL injection o mat khau | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |
| AT_LOGIN_15 | FAIL - security | Dang nhap that bai voi email chua script | INFRA_FAIL | INFRA |  |  | Could not create a session: You must enable 'Allow remote automation' in the Developer section of Safari Settings to control Safari via WebDriver. |  |

## Nhan xet nhanh
- Co case INFRA_FAIL (loi ha tang browser/moi truong test).
- Xem chi tiet trong cac truong `errorType`, `errorMessage`, `observedValidationMessage` cua JSON.