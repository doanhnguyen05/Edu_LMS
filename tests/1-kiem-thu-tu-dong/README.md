# 1) Kiem thu tu dong

Muc tieu cua thu muc nay:

- Tu dong hoa kiem thu chuc nang Dang nhap.
- Chay lai nhanh sau moi lan cap nhat he thong.
- Co bang chung ro rang: anh screenshot + bao cao ket qua.

Du an Selenium hien co:

- `tests/1-kiem-thu-tu-dong/login-selenium`

Cac tinh huong da duoc tu dong hoa:

1. PASS theo vai tro: Learner, Admin, Instructor.
2. PASS theo bien the du lieu: email viet hoa, learner thu 2.
3. FAIL xac thuc: sai mat khau, email khong ton tai.
4. FAIL validation: trong email, trong mat khau, trong ca hai, email sai dinh dang, email co khoang trang.
5. FAIL bao mat co ban: SQL injection/XSS.

Tong cong: 15 test case.

Khi chay `npm run test:login`, script se hoi ban chon browser de chay (Safari/Coc Coc/Chrome/tat ca).

