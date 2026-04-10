# Khu vuc kiem thu (tach rieng)

Thu muc `tests/` duoc chia thanh 4 nhom de de quan ly:

1. `tests/1-kiem-thu-tu-dong`: Kiem thu tu dong giao dien/luong nghiep vu (Selenium).
2. `tests/2-unit-test`: Unit test cho tung ham/lop nho.
3. `tests/3-kiem-thu-chuc-nang`: Kiem thu chuc nang theo test case.
4. `tests/4-kiem-thu-tich-hop`: Kiem thu tich hop giua cac module.

## Phan biet ro Unit test va Integration test (tranh nham lan)

Luu y: cac lenh `dotnet test` ben duoi duoc viet theo duong dan tu thu muc goc repo `WEB nang cao/`.

| Noi dung | Unit test | Kiem thu tich hop |
|---|---|---|
| Muc tieu | Kiem tra tung ham/lop nho, quy tac xu ly rieng le | Kiem tra luong nghiep vu lien module (Controller -> DB -> module khac) |
| Vi tri ma test | `EcduLMS.Web.Tests/Account`, `Learner`, `Instructor`, `Controllers` (khong thu muc `Integration`) | `EcduLMS.Web.Tests/Integration` |
| Lenh chay rieng | `dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --filter "FullyQualifiedName!~Integration"` | `dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --filter "FullyQualifiedName~Integration"` |
| So test hien tai (2026-04-05) | `11` | `4` |
| Bao cao | `tests/2-unit-test/BAO_CAO_UNIT_TEST_HE_THONG_ECDULMS_WEB.md` | `tests/4-kiem-thu-tich-hop/BAO_CAO_KIEM_THU_TICH_HOP_ECDULMS_WEB.md` |

## Trang thai hien tai

1. Kiem thu tu dong dang nhap: da co report va artifact (run moi nhat 2026-04-05: `45 case | PASS: 30 | FAIL(logic): 0 | INFRA_FAIL: 15`).
2. Unit test: da tach rieng, ket qua `11/11 PASS` (khong gom integration).
3. Kiem thu chuc nang black-box: da co bao cao muc 3.3.
4. Kiem thu tich hop: da co report rieng, ket qua `4/4 PASS`.

## Bao cao tong hop de nop

1. Ban chot hoan chinh (khuyen nghi su dung khi nop): `tests/BAO_CAO_KIEM_THU_HOAN_CHINH_ECDULMS_WEB.md`
2. Ban nay da tong hop lai tu Unit + Integration + Automation + Black-box va tach ro pham vi tung nhom.
