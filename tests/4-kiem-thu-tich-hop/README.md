# 4) Kiem thu tich hop

Thu muc nay dung cho integration test trong giai doan tiep theo.

Luu y quan trong:

1. Muc nay chi bao gom **Kiem thu tich hop**.
2. **Khong** bao gom Unit test thong thuong.
3. Unit test duoc tach rieng tai `tests/2-unit-test`.

Muc tieu:

1. Kiem tra luong end-to-end giua Controller -> Identity -> Database.
2. Kiem tra cac nghiep vu co lien quan den role va phan quyen.
3. Kiem tra cac API/luong thanh toan/webhook khi can.

## Tai lieu va ma da thuc hien

1. Bao cao chi tiet:
   - `tests/4-kiem-thu-tich-hop/BAO_CAO_KIEM_THU_TICH_HOP_ECDULMS_WEB.md`
2. Ma integration test:
   - `EcduLMS.Web.Tests/Integration/LearnerWorkflowIntegrationTests.cs`
   - `EcduLMS.Web.Tests/Integration/PaymentEnrollmentIntegrationTests.cs`

## Lenh chay nhanh

1. Chi chay integration:

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --filter "FullyQualifiedName~Integration" --logger "console;verbosity=detailed"
```

2. Chay toan bo bo test:

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj
```

3. Chi chay Unit test (de doi chieu, khong phai pham vi muc nay):

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --filter "FullyQualifiedName!~Integration"
```
