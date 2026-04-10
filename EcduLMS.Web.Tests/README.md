# EcduLMS.Web.Tests

Thu muc nay chua bo test C#/.NET cho he thong `EduLMS.Web`.

Pham vi hien tai gom 2 nhom:

- Unit test: kiem tra logic xu ly rieng le trong controller, view model va nghiep vu.
- Integration test: kiem tra cac luong lien module va luong nghiep vu hoan chinh.

## Cach chay

### Neu dang dung trong thu muc `EcduLMS.Web.Tests/`

```bash
dotnet test ./EcduLMS.Web.Tests.csproj
```

### Chi chay unit test

```bash
dotnet test ./EcduLMS.Web.Tests.csproj --filter 'FullyQualifiedName!~Integration' --logger 'console;verbosity=minimal'
```

### Chi chay integration test

```bash
dotnet test ./EcduLMS.Web.Tests.csproj --filter 'FullyQualifiedName~Integration' --logger 'console;verbosity=detailed'
```

### Neu dang dung o thu muc goc repo `WEB nang cao/`

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj
```

### Luu y

- Neu da `cd EcduLMS.Web.Tests` thi khong duoc chay lai duong dan `EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj`.
- Loi `MSB1009: Project file does not exist` thuong xay ra khi bi lap duong dan nhu tren.

## Ket qua hien tai (2026-04-04)

1. Unit test: `11/11 PASS`
2. Integration test: `4/4 PASS`
3. Tong suite: `15/15 PASS`

## Cau truc test hien co

- `Account/LoginViewModelTests.cs` - Kiem tra du lieu dang nhap.
- `Learner/LessonControllerProgressTests.cs` - Kiem tra cap nhat tien do hoc tap.
- `Instructor/GradingControllerScoreValidationTests.cs` - Kiem tra validate diem so khi cham bai.
- `Controllers/WebhookControllerPaymentTests.cs` - Kiem tra xu ly thanh toan webhook va tu dong ghi danh.
- `Integration/LearnerWorkflowIntegrationTests.cs` - Kiem tra luong tich hop hoc bai -> mo bai tap -> nop bai -> cham diem -> xem diem.
- `Integration/PaymentEnrollmentIntegrationTests.cs` - Kiem tra luong tich hop webhook thanh toan -> enrollment -> hien thi My Training.
