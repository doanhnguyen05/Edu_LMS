# 2) Unit test (He thong EcduLMS.Web)

Thu muc nay luu tai lieu bao cao unit test cho he thong `EcduLMS.Web`.

Luu y quan trong:

1. Muc nay chi bao gom **Unit test**.
2. **Khong** bao gom test trong thu muc `EcduLMS.Web.Tests/Integration`.
3. Neu can xem tich hop, xem muc `tests/4-kiem-thu-tich-hop`.

## Bao cao chi tiet

- `BAO_CAO_UNIT_TEST_HE_THONG_ECDULMS_WEB.md`

## Ma unit test thuc te

Bo test C# nam tai:

- `EcduLMS.Web.Tests/`

## Lenh chay

### Chay dung Unit test (khong gom Integration)

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --filter "FullyQualifiedName!~Integration" --logger "console;verbosity=minimal"
```

### Chay toan bo suite (Unit + Integration)

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj
```
