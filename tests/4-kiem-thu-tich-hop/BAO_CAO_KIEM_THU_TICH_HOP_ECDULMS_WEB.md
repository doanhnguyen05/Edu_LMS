# BAO CAO KIEM THU TICH HOP HE THONG (EcduLMS.Web)

## 1. Muc tieu

Kiem thu tich hop duoc thuc hien de xac nhan cac module phoi hop dung theo nghiep vu thuc te:

1. Learner hoc bai theo thu tu -> mo bai tap dung dieu kien.
2. Learner nop bai -> Instructor cham diem -> Learner xem duoc ket qua.
3. Webhook thanh toan -> tu dong ghi danh -> khoa hoc xuat hien trong My Training.

## 2. Pham vi va thanh phan da test

Dot nay da bo sung nhom integration test moi trong:

- `EcduLMS.Web.Tests/Integration/LearnerWorkflowIntegrationTests.cs`
- `EcduLMS.Web.Tests/Integration/PaymentEnrollmentIntegrationTests.cs`

Luong tich hop da test:

1. `LessonController` + `TrainingController` + `GradesController`
2. `AssignmentController` + `GradingController` + `GradesController`
3. `WebhookController` + `TrainingController` + `ApplicationDbContext`

Ranh gioi pham vi:

1. Bao cao nay chi tinh ket qua test co ten/namespace `Integration`.
2. Cac Unit test (khong co `Integration`) duoc bao cao rieng tai `tests/2-unit-test/BAO_CAO_UNIT_TEST_HE_THONG_ECDULMS_WEB.md`.

## 3. Moi truong kiem thu

- Nen tang: `.NET 9`
- Du an test: `EcduLMS.Web.Tests`
- Framework: `xUnit`
- Database cho test: `EF Core InMemory`
- Ngay chay test: `2026-04-04`

## 4. Danh sach test case tich hop

### 4.1 Learner workflow integration

| ID | Ten test | Muc tieu tich hop | Ket qua mong doi |
|---|---|---|---|
| IT_LEARNER_01 | `LearningPath_WhenPrerequisiteIncomplete_ShouldLockLessonAndBlockAssignment` | Dong bo quy tac khoa bai hoc va chan bai tap giua Training + Grades | Bai hoc sau bi khoa, bai tap chua duoc mo |
| IT_LEARNER_02 | `LearningPath_WhenLearnerCompletesLessons_ShouldUnlockAssignmentInTrainingAndGrades` | Sau khi complete bai hoc, trang thai mo bai tap phai cap nhat nhat quan | Bai tap duoc mo o ca Training va Grades |
| IT_LEARNER_03 | `AssignmentFlow_SubmitThenGrade_ShouldAppearAsPassInLearnerGrades` | Nop bai -> cham diem -> hien thi diem qua nhieu module | Bai duoc grade Pass, learner xem duoc diem |

### 4.2 Payment enrollment integration

| ID | Ten test | Muc tieu tich hop | Ket qua mong doi |
|---|---|---|---|
| IT_PAYMENT_01 | `PaymentWorkflow_WhenWebhookCompletesPayment_ShouldShowCourseInLearnerMyTraining` | Webhook thanh toan phai tao enrollment va du lieu nay hien thi trong My Training | Payment Completed, khoa hoc xuat hien trong ActiveCourses |

## 5. Lenh chay va cach su dung

### 5.1 Chay rieng nhom integration test

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj \
  --filter "FullyQualifiedName~Integration" \
  --logger "console;verbosity=detailed"
```

Y nghia:

1. `FullyQualifiedName~Integration`: chi chay test trong namespace/lop integration.
2. `verbosity=detailed`: hien thi ro tung test case va thoi gian.

### 5.2 Chay toan bo test suite

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --logger "console;verbosity=minimal"
```

### 5.3 Chay 1 test case cu the

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj \
  --filter "FullyQualifiedName=EcduLMS.Web.Tests.Integration.LearnerWorkflowIntegrationTests.AssignmentFlow_SubmitThenGrade_ShouldAppearAsPassInLearnerGrades"
```

## 6. Ket qua thuc te (da chay)

### 6.1 Ket qua nhom integration

Lenh da chay:

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --filter "FullyQualifiedName~Integration" --logger "console;verbosity=detailed"
```

Ket qua:

- Total integration tests: `4`
- Passed: `4`
- Failed: `0`
- Tong thoi gian test run: `1.4061s`

Chi tiet:

1. `PaymentWorkflow_WhenWebhookCompletesPayment_ShouldShowCourseInLearnerMyTraining` -> PASS (`701 ms`)
2. `AssignmentFlow_SubmitThenGrade_ShouldAppearAsPassInLearnerGrades` -> PASS (`933 ms`)
3. `LearningPath_WhenPrerequisiteIncomplete_ShouldLockLessonAndBlockAssignment` -> PASS (`58 ms`)
4. `LearningPath_WhenLearnerCompletesLessons_ShouldUnlockAssignmentInTrainingAndGrades` -> PASS (`7 ms`)

Bang xac nhan PASS ro rang (Expected vs Actual):

| Test case | Dieu kien PASS (Expected) | Ket qua thuc te (Actual) | Ket luan |
|---|---|---|---|
| PaymentWorkflow_WhenWebhookCompletesPayment_ShouldShowCourseInLearnerMyTraining | Webhook thanh toan hop le phai: `Payment=Completed`, tao `Enrollment Active`, va khoa hoc xuat hien o `TrainingController.Index()` | Payment doi sang `Completed`; enrollment duoc tao; `TrainingViewModel.ActiveCourses` co dung `CourseId` da thanh toan | PASS |
| AssignmentFlow_SubmitThenGrade_ShouldAppearAsPassInLearnerGrades | Learner nop bai -> Instructor cham `Score=9, Pass` -> Learner thay trang thai `Pass` va xem duoc chi tiet diem | Submission tao trang thai `Submitted`; sau cham bai co diem `9`; `Grades.CourseDetail` hien `Pass`; `GradeDetail` tra ve score `9` | PASS |
| LearningPath_WhenPrerequisiteIncomplete_ShouldLockLessonAndBlockAssignment | Khi chua hoc bai truoc: bai sau phai bi khoa, assignment bi chan o Training va Grades | `Lesson2.IsLocked=true`; `Assignment.CanStart=false`; `GradeAssignment.CanStartAssignment=false` va co ly do bi chan | PASS |
| LearningPath_WhenLearnerCompletesLessons_ShouldUnlockAssignmentInTrainingAndGrades | Hoan thanh day du lesson thi assignment phai duoc mo o ca Training va Grades | `Enrollment.ProgressPercent=100`, `Status=Completed`; assignment duoc mo (`CanStart=true`) o ca 2 man | PASS |

Log terminal chi tiet da luu tai:

- `tests/4-kiem-thu-tich-hop/artifacts/integration-test-output-2026-04-04.txt`

### 6.2 Ket qua toan bo test suite

Lenh da chay:

```bash
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --logger "console;verbosity=minimal"
```

Ket qua:

- Total tests: `15`
- Passed: `15`
- Failed: `0`
- Skipped: `0`
- Duration: `856 ms`

Luu y:

1. `15` la tong hop **Unit + Integration**.
2. De tranh nham lan, ket qua danh gia muc nay su dung so lieu tai muc `6.1` (`4/4` integration).

### 6.3 Bang tach rieng Unit vs Integration

| Nhom | Lenh chay | So test | PASS | FAIL |
|---|---|---:|---:|---:|
| Unit test | `--filter "FullyQualifiedName!~Integration"` | 11 | 11 | 0 |
| Integration test | `--filter "FullyQualifiedName~Integration"` | 4 | 4 | 0 |
| Tong suite | khong filter | 15 | 15 | 0 |

## 7. Nhan xet va ket luan

1. Cac luong tich hop quan trong da thong suot, khong thay loi nghiep vu trong dot test nay.
2. Chuoi nghiep vu "hoc -> nop bai -> cham diem -> xem diem" da duoc xac nhan hoat dong xuyen suot giua nhieu controller va DB.
3. Chuoi nghiep vu "webhook thanh toan -> ghi danh -> hien thi My Training" da duoc xac nhan.
4. Toan bo test suite hien tai pass `15/15`, trong do integration pass `4/4`.

## 8. Luu y khi su dung

1. Trong output co canh bao `NU1603` lien quan version `X.PagedList...`; day la warning restore package, khong lam fail test.
2. Neu muon chi bao cao integration, luon dung filter `FullyQualifiedName~Integration`.
3. Neu bo sung integration test moi, dat ten lop/test co tu `Integration` de tai su dung filter de dang.
