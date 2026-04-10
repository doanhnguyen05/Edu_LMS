# BAO CAO CHI TIET UNIT TEST HE THONG (EcduLMS.Web)

## 1. Tong quan

Bao cao nay mo ta bo unit test C#/.NET duoc viet de kiem tra truc tiep cac thanh phan xu ly logic trong he thong `EcduLMS.Web`.

- Du an test: `EcduLMS.Web.Tests`
- Framework test: `xUnit`
- Loai test: Unit test cho logic nghiep vu trong `ViewModel`, `Controller`
- Pham vi: chi unit test, khong bao gom test trong thu muc `EcduLMS.Web.Tests/Integration`

Lenh chay:

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --filter "FullyQualifiedName!~Integration" --logger "console;verbosity=detailed"
```

Ket qua thuc te moi nhat (chay ngay 2026-04-10):

- Total tests: `12`
- Passed: `12`
- Failed: `0`
- Tong thoi gian: `1.1432s`

## 2. Cau truc bo test

| Nhom trong de tai | File test | Thanh phan he thong duoc test |
|---|---|---|
| 3.2.2 Login validation | `EcduLMS.Web.Tests/Account/LoginViewModelTests.cs` | `EcduLMS.Web/ViewModels/Account/LoginViewModel.cs` |
| 3.2.3 Learning progress | `EcduLMS.Web.Tests/Learner/LessonControllerProgressTests.cs` | `EcduLMS.Web/Areas/Learner/Controllers/LessonController.cs` |
| 3.2.4 Score validation | `EcduLMS.Web.Tests/Instructor/GradingControllerScoreValidationTests.cs` | `EcduLMS.Web/Areas/Instructor/Controllers/GradingController.cs` |
| 3.2.5 Payment va auto payment | `EcduLMS.Web.Tests/Controllers/WebhookControllerPaymentTests.cs` | `EcduLMS.Web/Controllers/WebhookController.cs` |

## 3. Bao cao chi tiet theo tung nhom unit test

## 3.2.2 Unit test kiem tra du lieu dang nhap

### Muc dich

Muc tieu cua nhom test nay la xac nhan du lieu nhap vao form dang nhap duoc kiem tra dung truoc khi gui yeu cau dang nhap den backend. Nhom nay tap trung vao cac rule validation duoc khai bao trong `LoginViewModel`.

### Thanh phan duoc kiem thu

- Lop/ViewModel: `LoginViewModel`
- File nguon: `EcduLMS.Web/ViewModels/Account/LoginViewModel.cs`

Rule dang duoc test:

- `Email`: `[Required]`, `[EmailAddress]`, `[RegularExpression]`
- `Password`: `[Required]`

### Cac truong hop kiem thu

| ID | Ten case | Input | Ket qua mong doi |
|---|---|---|---|
| UT_LOGIN_01 | Email va mat khau hop le | `Email=learner@example.com`, `Password=Learner@123` | Model hop le, khong co loi validation |
| UT_LOGIN_02 | Bo trong email | `Email=""`, `Password=Learner@123` | Co loi `Vui lòng nhập email` |
| UT_LOGIN_03 | Email sai dinh dang | `Email=invalid-email`, `Password=Learner@123` | Co loi `Email không hợp lệ` |
| UT_LOGIN_04 | Email chua script | `Email=<script>alert(1)</script>@mail.com`, `Password=Learner@123` | Co loi `Email không hợp lệ` |
| UT_LOGIN_05 | Bo trong mat khau | `Email=learner@example.com`, `Password=""` | Co loi `Vui lòng nhập mật khẩu` |

### Ma unit test

- File: `EcduLMS.Web.Tests/Account/LoginViewModelTests.cs`
- So test case: `5`

### Ket qua mong doi cua nhom nay

- Du lieu hop le phai qua validation.
- Du lieu khong hop le phai bi chan va sinh dung thong diep loi.

## 3.2.3 Unit test tien do hoc tap

### Muc dich

Muc tieu cua nhom nay la kiem tra logic cap nhat tien do hoc tap cua learner khi danh dau hoan thanh bai hoc. Day khong phai la mot ham tinh toan don le kieu `Completed/Total`, ma la kiem tra truc tiep action `LessonController.MarkComplete(int lessonId)` voi day du quy tac nghiep vu.

### Thanh phan duoc kiem thu

- Action: `LessonController.MarkComplete(int lessonId)`
- File nguon: `EcduLMS.Web/Areas/Learner/Controllers/LessonController.cs`

Logic nghiep vu can xac nhan:

- Danh dau bai hoc hien tai la hoan thanh.
- Tao hoac cap nhat `LessonProgress`.
- Cap nhat `Enrollment.ProgressPercent`.
- Chan hoc nhay coc khi bai hoc truoc chua hoan thanh.
- Chuyen khoa hoc sang `Completed` khi dat 100%.

### Cac truong hop kiem thu

| ID | Ten case | Input/Trang thai dau vao | Ket qua mong doi |
|---|---|---|---|
| UT_PROGRESS_01 | Hoan thanh bai dau va tang tien do | Khoa hoc co 2 bai, chua bai nao complete, learner mark bai 1 | Tao `LessonProgress=Completed`, `ProgressPercent=50`, `EnrollmentStatus=Active` |
| UT_PROGRESS_02 | Chan hoc nhay coc khi chua xong bai truoc | Khoa hoc co 2 bai, bai 1 chua complete, learner mark bai 2 | Redirect ve bai 1, bai 2 khong duoc danh dau hoan thanh |
| UT_PROGRESS_03 | Hoan thanh bai cuoi va ket thuc khoa hoc | Bai 1 da complete, learner mark bai 2 | `ProgressPercent=100`, `EnrollmentStatus=Completed`, co `CompletedAt` |

### Ma unit test

- File: `EcduLMS.Web.Tests/Learner/LessonControllerProgressTests.cs`
- So test case: `3`

### Ket qua mong doi cua nhom nay

- Tien do hoc tap phai tang dung theo tung bai hoc.
- Khong duoc phep bo qua bai truoc de hoan thanh bai sau.
- Khi hoan thanh tat ca bai hoc, khoa hoc phai duoc danh dau hoan tat.

## 3.2.4 Unit test kiem tra du lieu diem so

### Muc dich

Muc tieu cua nhom nay la xac minh logic validate diem trong chuc nang cham bai cua giang vien. Pham vi hien tai cua nhom test chi tap trung vao 2 tinh huong da duoc code va chay thuc te: diem vuot thang diem va diem hop le.

### Thanh phan duoc kiem thu

- Action: `GradingController.SubmitGrade(SubmitGradeRequest req)`
- File nguon: `EcduLMS.Web/Areas/Instructor/Controllers/GradingController.cs`

Logic nghiep vu can xac nhan:

- Neu diem nho hon 0 hoac lon hon `MaxScore` thi tra ve `BadRequest`.
- Neu diem hop le thi luu `Grade` va chuyen `SubmissionStatus` sang `Graded`.

### Cac truong hop kiem thu

| ID | Ten case | Input/Trang thai dau vao | Ket qua mong doi |
|---|---|---|---|
| UT_SCORE_01 | Diem vuot thang diem bi tu choi | `MaxScore=10`, submit `Score=11` | `BadRequest`, khong tao `Grade`, `Submission` van la `Submitted` |
| UT_SCORE_02 | Diem hop le duoc luu thanh cong | `MaxScore=10`, submit `Score=8.5`, `PassStatus=Fail` | `Ok`, tao `Grade`, `Submission` chuyen sang `Graded`, luu dung score va pass status |

### Ma unit test

- File: `EcduLMS.Web.Tests/Instructor/GradingControllerScoreValidationTests.cs`
- So test case: `2`

### Ket qua mong doi cua nhom nay

- Chi chap nhan diem nam trong thang diem bai tap.
- Khi diem hop le, he thong phai luu ket qua cham bai chinh xac.

## 3.2.5 Unit test kiem tra thanh toan va thanh toan tu dong

### Muc dich

Muc tieu cua nhom nay la kiem tra ro rang co che xu ly thanh toan tu dong thong qua webhook. Cu the, unit test dang kiem tra action `WebhookController.SePay(...)`, la noi he thong tiep nhan phan hoi giao dich tu cong thanh toan.

Nhom test nay khong chi kiem tra viec payment co doi trang thai hay khong, ma con kiem tra day du logic lien quan:

- Xac dinh dung giao dich pending can xu ly.
- Doi `Payment.Status` sang `Completed` khi webhook hop le.
- Tu dong tao `Enrollment` cho learner sau thanh toan thanh cong.
- Tu dong tao `InstructorEarning` cho giang vien.
- Khong auto complete neu so tien chuyen vao nho hon so tien can thanh toan.

### Thanh phan duoc kiem thu

- Action: `WebhookController.SePay(SePayWebhookPayload payload)`
- File nguon: `EcduLMS.Web/Controllers/WebhookController.cs`

### Dien giai nghiep vu "thanh toan tu dong"

Trong he thong nay, "thanh toan tu dong" co nghia la:

1. Learner tao don thanh toan cho mot khoa hoc.
2. He thong luu payment o trang thai `Pending`.
3. Khi cong thanh toan gui webhook ve he thong, action `SePay(...)` doc `TransactionCode` va `TransferAmount`.
4. Neu thong tin hop le va du tien:
   - Payment duoc doi sang `Completed`
   - He thong tu dong ghi danh learner vao khoa hoc
   - He thong tu dong tao doanh thu cho giang vien
5. Neu webhook khong hop le hoac chuyen thieu tien:
   - Payment van giu `Pending`
   - Khong tao `Enrollment`
   - Khong tao `InstructorEarning`

Day la ly do nhom test nay can duoc mo ta ro rang la "kiem tra webhook thanh toan va xu ly ghi danh tu dong", thay vi mo ta chung chung la "test thanh toan".

### Cac truong hop kiem thu

| ID | Ten case | Input/Trang thai dau vao | Ket qua mong doi |
|---|---|---|---|
| UT_PAYMENT_01 | Webhook hop le va du tien | Payment dang `Pending`, payload co dung `TransactionCode`, `TransferAmount = Amount` | Payment chuyen `Completed`, tao `Enrollment=Active`, tao `InstructorEarning` dung so tien |
| UT_PAYMENT_02 | Webhook chuyen thieu tien | Payment dang `Pending`, payload co `TransferAmount < Amount` | Payment van `Pending`, khong tao `Enrollment`, khong tao `InstructorEarning` |

### Ma unit test

- File: `EcduLMS.Web.Tests/Controllers/WebhookControllerPaymentTests.cs`
- So test case: `2`

### Ket qua mong doi cua nhom nay

- Chi khi webhook hop le va du tien thi he thong moi auto xu ly thanh toan.
- Neu chuyen thieu tien thi khong duoc danh dau thanh toan thanh cong.

## 4. Ket qua thuc te theo tung unit test

Du lieu duoi day lay tu lan chay:

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --filter "FullyQualifiedName!~Integration" --logger "console;verbosity=detailed"
```

### 4.1 Nhom Login validation

| STT | Unit test | Trang thai | Thoi gian |
|---|---|---|---|
| 1 | `LoginViewModel_WithEmptyEmail_ShouldContainRequiredError` | PASS | `5 ms` |
| 2 | `LoginViewModel_WithInvalidEmailFormat_ShouldContainEmailFormatError` | PASS | `< 1 ms` |
| 3 | `LoginViewModel_WithEmptyPassword_ShouldContainRequiredError` | PASS | `< 1 ms` |
| 4 | `LoginViewModel_WithValidEmailAndPassword_ShouldBeValid` | PASS | `1 ms` |
| 5 | `LoginViewModel_WithScriptInEmail_ShouldContainEmailFormatError` | PASS | `< 1 ms` |

### 4.2 Nhom Learning progress

| STT | Unit test | Trang thai | Thoi gian |
|---|---|---|---|
| 1 | `MarkComplete_SecondLessonWithoutFirstCompleted_ShouldRedirectToPreviousLesson` | PASS | `696 ms` |
| 2 | `MarkComplete_LastLesson_ShouldCompleteEnrollmentAt100Percent` | PASS | `49 ms` |
| 3 | `MarkComplete_FirstLesson_ShouldCreateProgressAndIncreaseEnrollmentPercent` | PASS | `7 ms` |

### 4.3 Nhom Score validation

| STT | Unit test | Trang thai | Thoi gian |
|---|---|---|---|
| 1 | `SubmitGrade_WhenScoreValid_ShouldPersistGradeAndMarkSubmissionAsGraded` | PASS | `720 ms` |
| 2 | `SubmitGrade_WhenScoreOutOfRange_ShouldReturnBadRequestAndNotPersistGrade` | PASS | `11 ms` |

### 4.4 Nhom Payment va auto payment

| STT | Unit test | Trang thai | Thoi gian |
|---|---|---|---|
| 1 | `SePay_WhenPayloadMatchesPendingPayment_ShouldCompletePaymentAndCreateEnrollment` | PASS | `725 ms` |
| 2 | `SePay_WhenTransferAmountIsLowerThanPaymentAmount_ShouldNotCompletePayment` | PASS | `7 ms` |

## 5. Tong hop ket qua

| Nhom | So test | PASS | FAIL |
|---|---:|---:|---:|
| 3.2.2 Login validation | 5 | 5 | 0 |
| 3.2.3 Learning progress | 3 | 3 | 0 |
| 3.2.4 Score validation | 2 | 2 | 0 |
| 3.2.5 Payment va auto payment | 2 | 2 | 0 |
| **Tong** | **12** | **12** | **0** |

## 6. Nhan xet va ket luan

- Bo unit test hien tai da duoc cap nhat va doi chieu lai theo dung ma nguon he thong `EcduLMS.Web`.
- Ket qua hien tai la `12/12 PASS`, khong con la `11/11` nhu mot so phien ban bao cao cu.
- Phan `LoginViewModel` can duoc goi dung la lop/view model, khong nen mo ta la "ham".
- Phan tien do hoc tap can mo ta dung theo action `LessonController.MarkComplete(...)`, vi day la logic nghiep vu co cap nhat `LessonProgress`, `Enrollment.ProgressPercent` va trang thai khoa hoc.
- Phan diem so da duoc thu hep ve dung 2 case da code va da chay thuc te.
- Phan thanh toan tu dong da duoc lam ro la dang test webhook `WebhookController.SePay(...)` va logic auto xu ly sau thanh toan.

Ket luan: phan unit test cua he thong hien tai da bao phu 4 nhom nghiep vu quan trong va tat ca deu PASS, tao nen nen tang on dinh cho cac tang kiem thu tich hop va kiem thu chuc nang o cac muc tiep theo.
