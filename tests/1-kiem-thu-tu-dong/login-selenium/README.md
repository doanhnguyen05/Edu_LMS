# Huong dan chi tiet: Kiem thu tu dong dang nhap bang Selenium

Tai lieu nay danh cho nguoi moi bat dau.

## 1. Muc tieu bao quat

Bo test login nay da duoc mo rong tu 5 case len 15 case, gom:

1. PASS theo vai tro: Learner, Admin, Instructor.
2. PASS theo bien the du lieu: email viet hoa, tai khoan learner thu 2.
3. FAIL xac thuc: sai mat khau, email khong ton tai.
4. FAIL validation: trong email, trong mat khau, trong ca hai, email sai dinh dang, email co khoang trang.
5. FAIL bao mat co ban: chuoi SQL injection/XSS.

## 2. Ho tro chon browser khi chay

Khi chay lenh:

```bash
npm run test:login
```

Script se hoi ban chon browser:

1. Coc Coc
2. Safari
3. Chrome
4. Coc Coc + Safari
5. Tat ca (Coc Coc + Safari + Chrome)

Neu ban chon `2`, script chi test tren Safari.
Neu ban chon `1`, script chi test tren Coc Coc.

## 3. Cau truc thu muc

```text
login-selenium/
  .env.example
  package.json
  scripts/
    login-automation.js
  artifacts/
    reports/
    screenshots/
```

## 4. Yeu cau truoc khi chay

1. Da cai `Node.js` (khuyen nghi ban 18 tro len).
2. Da cai `Coc Coc`.
3. Mac co Safari (mac dinh co san).
4. Bat Safari automation (lam 1 lan):
   - Safari -> Settings -> Advanced -> bat `Show Develop menu in menu bar`.
   - Menu Develop -> bat `Allow Remote Automation`.
   - Chay:
```bash
safaridriver --enable
```
5. Ung dung ASP.NET dang chay local (`http://localhost:5000`).
6. Database da seed tai khoan demo.

## 5. Cai dat bo test (lam 1 lan)

```bash
cd "tests/1-kiem-thu-tu-dong/login-selenium"
npm install
cp .env.example .env
```

Cap nhat `.env` theo may ban:

- `BASE_URL`, `LOGIN_PATH`
- `VALID_EMAIL`, `VALID_PASSWORD`
- `ADMIN_EMAIL`, `ADMIN_PASSWORD`
- `INSTRUCTOR_EMAIL`, `INSTRUCTOR_PASSWORD`
- `COCCOC_BINARY_PATH`

Gia tri goi y cho Mac:

```env
COCCOC_BINARY_PATH=/Applications/CocCoc.app/Contents/MacOS/CocCoc
```

## 6. Cac lenh chay nhanh

Chay co menu chon browser:

```bash
npm run test:login
```

Chay thang tung browser:

```bash
npm run test:login:coccoc
npm run test:login:safari
npm run test:login:chrome
```

Chay tat ca browser:

```bash
npm run test:login:all
```

Headless (chi dung cho Chromium):

```bash
HEADLESS=true npm run test:login:coccoc
```

Chay mot/nhieu case cu the (de debug nhanh):

```bash
HEADLESS=true ASK_BROWSER_ON_START=false BROWSERS=chrome TEST_CASE_IDS=AT_LOGIN_07,AT_LOGIN_08 node scripts/login-automation.js
```

## 7. Lay ket qua va minh chung

Sau khi chay xong:

1. Tong hop tat ca browser:
   - `artifacts/reports/latest-summary.json`
   - `artifacts/reports/latest-summary.md`
2. Chi tiet tung browser:
   - `artifacts/reports/latest-summary-coccoc.md`
   - `artifacts/reports/latest-summary-safari.md`
   - `artifacts/reports/latest-summary-chrome.md`
3. Screenshot moi test case:
   - `artifacts/screenshots/`

Y nghia:

- `PASS`: Dat ky vong theo `expectedType` (bao gom ca case ky vong that bai dang nhap).
- `FAIL`: Fail logic test case (khong dat expected nghiep vu).
- `INFRA_FAIL`: Loi ha tang/moi truong browser (vi du Safari remote automation chua bat).

## 8. Loi thuong gap
1. Coc Coc khong mo:
   - Kiem tra `COCCOC_BINARY_PATH`.
2. Safari bao loi remote automation:
   - Chay lai `safaridriver --enable`.
   - Bat `Develop > Allow Remote Automation`.
3. URL sai:
   - Kiem tra `BASE_URL`, `LOGIN_PATH`.
4. Case pass bi fail:
   - Kiem tra tai khoan trong `.env` co dung voi database seed.
5. He thong tra HTTP 500:
   - Kiem tra DB/MySQL dang chay va app ket noi duoc DB truoc khi chay Selenium.
