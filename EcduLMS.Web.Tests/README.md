# EcduLMS.Web.Tests

Bo unit test C#/.NET test truc tiep logic trong he thong `EcduLMS.Web`.

## Run

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj
```

## Run unit tests only (khong gom integration)

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --filter "FullyQualifiedName!~Integration" --logger "console;verbosity=minimal"
```

## Run integration tests only

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --filter "FullyQualifiedName~Integration" --logger "console;verbosity=detailed"
```

## Ket qua tach rieng hien tai (2026-04-04)

1. Unit test: `11/11 PASS`
2. Integration test: `4/4 PASS`
3. Tong suite: `15/15 PASS`

## Nhom test hien tai

- `Account/LoginViewModelTests.cs` - Kiem tra du lieu dang nhap.
- `Learner/LessonControllerProgressTests.cs` - Kiem tra cap nhat tien do hoc tap.
- `Instructor/GradingControllerScoreValidationTests.cs` - Kiem tra validate diem so khi cham bai.
- `Controllers/WebhookControllerPaymentTests.cs` - Kiem tra xu ly thanh toan webhook va tu dong ghi danh.
- `Integration/LearnerWorkflowIntegrationTests.cs` - Kiem tra luong tich hop hoc bai -> mo bai tap -> nop bai -> cham diem -> xem diem.
- `Integration/PaymentEnrollmentIntegrationTests.cs` - Kiem tra luong tich hop webhook thanh toan -> enrollment -> hien thi My Training.
