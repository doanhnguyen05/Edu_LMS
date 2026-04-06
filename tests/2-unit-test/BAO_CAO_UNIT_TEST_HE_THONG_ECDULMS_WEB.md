# BAO CAO CHI TIET UNIT TEST HE THONG (EcduLMS.Web)

## 1. Tong quan

Bao cao nay mo ta bo unit test C#/.NET test truc tiep code trong he thong `EcduLMS.Web`.

- Du an test: `EcduLMS.Web.Tests`
- Framework test: `xUnit`
- Kieu test: Unit test cho logic nghiep vu trong controller/viewmodel cua he thong
- Pham vi: **chi Unit test**, khong bao gom test trong thu muc `EcduLMS.Web.Tests/Integration`
- Lenh chay:

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --filter "FullyQualifiedName!~Integration" --logger "console;verbosity=minimal"
```

- Ket qua thuc te (ngay 2026-04-04):
  - Passed: `11`
  - Failed: `0`
  - Total: `11`

## 2. Cau truc bo test

| Nhom trong de tai | File test | Thanh phan he thong duoc test |
|---|---|---|
| 3.2.1 Login validation | `EcduLMS.Web.Tests/Account/LoginViewModelTests.cs` | `EcduLMS.Web/ViewModels/Account/LoginViewModel.cs` |
| 3.2.2 Learning progress | `EcduLMS.Web.Tests/Learner/LessonControllerProgressTests.cs` | `EcduLMS.Web/Areas/Learner/Controllers/LessonController.cs` |
| 3.2.3 Score validation | `EcduLMS.Web.Tests/Instructor/GradingControllerScoreValidationTests.cs` | `EcduLMS.Web/Areas/Instructor/Controllers/GradingController.cs` |
| 3.2.4 Payment & auto payment | `EcduLMS.Web.Tests/Controllers/WebhookControllerPaymentTests.cs` | `EcduLMS.Web/Controllers/WebhookController.cs` |

## 3. Bao cao chi tiet theo tung unit

## 3.2.1 Unit test kiem tra du lieu dang nhap

### Muc dich
Dam bao du lieu dang nhap (email/password) duoc validate dung theo DataAnnotation truoc khi xu ly dang nhap.

### Ham/thanh phan duoc kiem thu
- Model: `LoginViewModel`
- Thuoc tinh validate:
  - `Email`: `[Required]`, `[EmailAddress]`
  - `Password`: `[Required]`

### Cac truong hop kiem thu

| ID | Ten case | Input | Ket qua mong doi |
|---|---|---|---|
| UT_LOGIN_01 | Email + password hop le | `Email=learner@example.com, Password=Learner@123` | Hop le (khong loi validation) |
| UT_LOGIN_02 | Email bo trong | `Email=""` | Co loi `Vui lòng nhập email` |
| UT_LOGIN_03 | Email sai dinh dang | `Email=invalid-email` | Co loi `Email không hợp lệ` |
| UT_LOGIN_04 | Password bo trong | `Password=""` | Co loi `Vui lòng nhập mật khẩu` |

### Ma unit test
- File: `EcduLMS.Web.Tests/Account/LoginViewModelTests.cs`
- So test case: `4`

### Ket qua mong doi cua unit nay
- Tat ca 4 case PASS.
- Du lieu hop le -> model valid.
- Du lieu sai -> sinh dung thong diep validation.

## 3.2.2 Unit test tinh tien do hoc tap

### Muc dich
Dam bao logic cap nhat tien do hoc tap cua learner dung khi danh dau hoan thanh bai hoc.

### Ham/thanh phan duoc kiem thu
- Action: `LessonController.MarkComplete(int lessonId)`
- Logic nghiep vu can xac nhan:
  - Danh dau bai hoc hoan thanh.
  - Tang `Enrollment.ProgressPercent`.
  - Chan hoc nhay coc (bai sau phai hoan thanh bai truoc).
  - Hoan thanh khoa hoc khi dat 100%.

### Cac truong hop kiem thu

| ID | Ten case | Input/Trang thai dau vao | Ket qua mong doi |
|---|---|---|---|
| UT_PROGRESS_01 | Hoan thanh bai dau | Khoa hoc 2 bai, chua bai nao complete, mark bai 1 | Tao `LessonProgress=Completed`, progress = `50%`, enrollment van `Active` |
| UT_PROGRESS_02 | Thu hoan thanh bai 2 khi bai 1 chua xong | Khoa hoc 2 bai, bai 1 chua complete, mark bai 2 | Bi redirect ve bai 1, bai 2 khong duoc complete |
| UT_PROGRESS_03 | Hoan thanh bai cuoi | Bai 1 da complete, mark bai 2 | Progress = `100%`, enrollment = `Completed`, co `CompletedAt` |

### Ma unit test
- File: `EcduLMS.Web.Tests/Learner/LessonControllerProgressTests.cs`
- So test case: `3`

### Ket qua mong doi cua unit nay
- 3 case PASS.
- Tien do va trang thai khoa hoc cap nhat dung theo nghiep vu.

## 3.2.3 Unit test kiem tra du lieu diem so

### Muc dich
Dam bao giang vien khong the cham diem vuot qua thang diem bai tap; diem hop le thi duoc luu dung.

### Ham/thanh phan duoc kiem thu
- Action: `GradingController.SubmitGrade(SubmitGradeRequest req)`
- Logic nghiep vu can xac nhan:
  - Neu diem < 0 hoac > MaxScore -> `BadRequest`.
  - Neu diem hop le -> tao/sua Grade va doi status submission sang `Graded`.

### Cac truong hop kiem thu

| ID | Ten case | Input/Trang thai dau vao | Ket qua mong doi |
|---|---|---|---|
| UT_SCORE_01 | Diem vuot thang diem | Assignment `MaxScore=10`, submit `Score=11` | `BadRequest`, khong tao Grade, submission van `Submitted` |
| UT_SCORE_02 | Diem hop le | Assignment `MaxScore=10`, submit `Score=8.5`, `PassStatus=Fail` | `Ok`, tao Grade, submission -> `Graded`, luu dung score/pass status |

### Ma unit test
- File: `EcduLMS.Web.Tests/Instructor/GradingControllerScoreValidationTests.cs`
- So test case: `2`

### Ket qua mong doi cua unit nay
- 2 case PASS.
- Rule validate diem duoc ap dung dung.

## 3.2.4 Unit test kiem tra chuc nang thanh toan va thanh toan tu dong

### Muc dich
Dam bao webhook thanh toan xu ly dung nghiep vu: chi hoan tat don khi du tien, tu dong ghi danh va tao earning cho giang vien.

### Ham/thanh phan duoc kiem thu
- Action: `WebhookController.SePay(SePayWebhookPayload payload)`
- Logic nghiep vu can xac nhan:
  - Match transaction code + du so tien -> payment completed.
  - Tu dong tao enrollment neu chua co.
  - Tu dong tao instructor earning.
  - Neu thieu tien -> khong complete payment.

### Cac truong hop kiem thu

| ID | Ten case | Input/Trang thai dau vao | Ket qua mong doi |
|---|---|---|---|
| UT_PAYMENT_01 | Payload hop le | Payment pending, content co transaction code, transferAmount = amount | Payment -> `Completed`, tao Enrollment `Active`, tao Earning (phi san 30%) |
| UT_PAYMENT_02 | Chuyen thieu tien | Payment pending, transferAmount < amount | Payment van `Pending`, khong tao Enrollment, khong tao Earning |

### Ma unit test
- File: `EcduLMS.Web.Tests/Controllers/WebhookControllerPaymentTests.cs`
- So test case: `2`

### Ket qua mong doi cua unit nay
- 2 case PASS.
- Webhook chi auto xu ly khi dung dieu kien.

## 4. Ket qua thuc te theo tung unit test (PASS/FAIL)

Du lieu duoi day lay tu lan chay:

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --filter "FullyQualifiedName!~Integration" --logger "console;verbosity=detailed"
```

### 4.1 Nhom 3.2.1 - Login validation

| STT | Unit test | Trang thai | Thoi gian |
|---|---|---|---|
| 1 | `LoginViewModel_WithEmptyEmail_ShouldContainRequiredError` | PASS | `6 ms` |
| 2 | `LoginViewModel_WithInvalidEmailFormat_ShouldContainEmailFormatError` | PASS | `< 1 ms` |
| 3 | `LoginViewModel_WithEmptyPassword_ShouldContainRequiredError` | PASS | `< 1 ms` |
| 4 | `LoginViewModel_WithValidEmailAndPassword_ShouldBeValid` | PASS | `3 ms` |

### 4.2 Nhom 3.2.2 - Learning progress

| STT | Unit test | Trang thai | Thoi gian |
|---|---|---|---|
| 1 | `MarkComplete_SecondLessonWithoutFirstCompleted_ShouldRedirectToPreviousLesson` | PASS | `715 ms` |
| 2 | `MarkComplete_LastLesson_ShouldCompleteEnrollmentAt100Percent` | PASS | `43 ms` |
| 3 | `MarkComplete_FirstLesson_ShouldCreateProgressAndIncreaseEnrollmentPercent` | PASS | `7 ms` |

### 4.3 Nhom 3.2.3 - Score validation

| STT | Unit test | Trang thai | Thoi gian |
|---|---|---|---|
| 1 | `SubmitGrade_WhenScoreValid_ShouldPersistGradeAndMarkSubmissionAsGraded` | PASS | `738 ms` |
| 2 | `SubmitGrade_WhenScoreOutOfRange_ShouldReturnBadRequestAndNotPersistGrade` | PASS | `10 ms` |

### 4.4 Nhom 3.2.4 - Payment & auto payment

| STT | Unit test | Trang thai | Thoi gian |
|---|---|---|---|
| 1 | `SePay_WhenPayloadMatchesPendingPayment_ShouldCompletePaymentAndCreateEnrollment` | PASS | `737 ms` |
| 2 | `SePay_WhenTransferAmountIsLowerThanPaymentAmount_ShouldNotCompletePayment` | PASS | `8 ms` |

## 5. Tong hop ket qua

| Nhom | So test | PASS | FAIL |
|---|---:|---:|---:|
| 3.2.1 Login validation | 4 | 4 | 0 |
| 3.2.2 Learning progress | 3 | 3 | 0 |
| 3.2.3 Score validation | 2 | 2 | 0 |
| 3.2.4 Payment & auto payment | 2 | 2 | 0 |
| **Tong** | **11** | **11** | **0** |

## 6. Ket luan

- Bo unit test da duoc lam lai de test truc tiep trong he thong `EcduLMS.Web` (khong phai bo JS mock rieng).
- Ket qua hien tai: `11/11` test PASS.
- Bao cao nay **chi danh cho Unit test**.
- Kiem thu tich hop duoc tach rieng tai: `tests/4-kiem-thu-tich-hop/BAO_CAO_KIEM_THU_TICH_HOP_ECDULMS_WEB.md`.
- Bao cao nay da hien thi theo dung thu tu ban yeu cau:
  - Ket qua qua tung unit test truoc.
  - Tong hop cuoi cung sau.
