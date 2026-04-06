# BAO CAO KIEM THU HOAN CHINH - EcduLMS.Web (Ban Lam Lai)

Bao cao nay duoc lam lai ky luong theo yeu cau:

1. Test Plan
2. Test Scenario
3. Test Case
4. Traceability Matrix (RTM)

Va co bo sung phan danh gia tong ket chat luong (muc 3.5) theo dung tinh than "thuc thi den dau thi bao cao den do", tranh danh gia cam tinh.

## 0. Trang thai tong quan hien tai

**Ket luan ngan gon:**

- Tang code (Unit + Integration): on dinh, da co bang chung PASS.
- Tang automation login: da sua script, ket qua logic pass tren Chrome/CocCoc; Safari duoc tach ro la loi ha tang (`INFRA_FAIL`).
- Tang black-box manual cho My Training/Grades/Calendar/Admin/Instructor: da co testcase nhung chua dong ket qua run thuc te => con `Execution Gap`.

**Bang chung thuc thi moi nhat (2026-04-05):**

- Unit output: `tests/artifacts/unit-test-output-20260405.txt`
- Integration output: `tests/artifacts/integration-test-output-20260405.txt`
- Selenium all browsers output JSON:
  - `tests/1-kiem-thu-tu-dong/login-selenium/artifacts/reports/login-summary-all-20260405-150021.json`
  - `tests/1-kiem-thu-tu-dong/login-selenium/artifacts/reports/login-summary-all-20260405-150021.md`

---

## 1) TEST PLAN - Ke hoach kiem thu tong the

## 1.1 Muc tieu

1. Xac thuc dung hanh vi dang nhap theo role va du lieu sai/bao mat.
2. Xac thuc nghiep vu hoc tap: tien do, khoa bai, mo bai tap.
3. Xac thuc luong nop bai -> cham diem -> learner xem diem.
4. Xac thuc luong thanh toan webhook -> ghi danh -> hien thi My Training.
5. Xac thuc cac module chuc nang tren giao dien theo vai tro.

## 1.2 Chien luoc va tang kiem thu

| Tang kiem thu | Doi tuong | Trang thai hien tai |
|---|---|---|
| Unit Test | LoginViewModel, LessonController, GradingController, WebhookController | **Hoan thanh: 11/11 PASS** |
| Integration Test | Luong lien module (payment-enrollment, learning path, grading flow) | **Hoan thanh: 4/4 PASS** |
| Automation Login | Dang nhap da browser (coccoc/safari/chrome) | **Da lam lai script va da chay lai** |
| Black-box Function/UI | My Training, Grades, Calendar, Admin Users, Instructor Courses | **Chua dong ket qua run day du (Execution Gap)** |

## 1.3 Entry / Exit criteria

### Entry

1. Ung dung chay duoc local (`http://localhost:5000`).
2. Co du lieu role + user + khoa hoc + assignment.
3. Script test, xUnit test build duoc.

### Exit cho dot nay

1. Unit test co ket qua PASS 100%.
2. Integration test co ket qua PASS 100%.
3. Automation login tach duoc 3 trang thai ro rang: `PASS / FAIL(logic) / INFRA_FAIL`.
4. RTM the hien ro requirement nao da test, requirement nao chua thuc thi.

## 1.4 Rui ro then chot

| Rui ro | Tac dong | Cach xu ly |
|---|---|---|
| Safari chua san sang remote automation | Nhiu ket qua nhiu neu gom chung vao FAIL logic | Tach thanh `INFRA_FAIL` rieng |
| Script login false-negative | Bao cao sai ban chat loi | Sua assert theo `expectedType` + cho phep browser validation fallback |
| Black-box manual chua run day du | Kho ket luan "san sang ban giao" | Danh dau `NOT_EXECUTED` ro rang trong RTM |

---

## 2) TEST SCENARIO - Kich ban kiem thu cap cao

## 2.1 Thang muc do danh gia scenario

| Muc | Dien giai |
|---|---|
| S0 | Chua co kich ban |
| S1 | Da liet ke, mo ta con mo |
| S2 | Da thiet ke day du (actor, precondition, expected) |
| S3 | Da thuc thi va co bang chung |
| S4 | Da on dinh regression (co the lap lai dinh ky, theo doi nhiu ro rang) |

## 2.2 Danh gia 4 scenario trong tam (theo ban yeu cau)

### Scenario 01 - Xac thuc dang nhap da vai tro va du lieu bat thuong

- Pham vi: AT_LOGIN_01..15 tren CocCoc/Safari/Chrome.
- Ket qua run moi nhat:
  - CocCoc: 15 PASS / 0 FAIL logic / 0 INFRA
  - Chrome: 15 PASS / 0 FAIL logic / 0 INFRA
  - Safari: 0 PASS / 0 FAIL logic / 15 INFRA
- Muc danh gia: **S3 (Da xac minh)**
  - Ly do: da co execution evidence day du + phan loai ro logic vs ha tang.
  - Chua len S4 vi Safari moi truong chua san sang => cross-browser chua khong chep kin.

### Scenario 02 - Ghi danh va thanh toan tu dong

- Pham vi: Integration payment workflow + webhook.
- Bang chung: `PaymentWorkflow_WhenWebhookCompletesPayment_ShouldShowCourseInLearnerMyTraining` PASS.
- Muc danh gia: **S3**
  - Da xac minh o tang lien module.
  - Chua co dot manual black-box day du tren UI payment (xac minh man hinh theo bang UI/Function).

### Scenario 03 - Tien do hoc tap va kiem soat lo trinh

- Pham vi: Lesson progress + prerequisite lock/unlock.
- Bang chung: Unit progress PASS + Integration LearningPath PASS.
- Muc danh gia: **S3**
  - Da xac minh nghiep vu cot loi o tang code/lien module.
  - Black-box UI My Training/Detail con `CHUA CHAY` theo bang testcase thu cong.

### Scenario 04 - Nop bai va cham diem

- Pham vi: Assignment submit -> Grading -> Learner grades.
- Bang chung: Integration `AssignmentFlow_SubmitThenGrade...` PASS.
- Muc danh gia: **S3**
  - Luong lien module da thong suot.
  - Mot so testcase UI grades chi tiet (manual) chua co artifact run day du.

## 2.3 Tong ket muc do scenario

| Scenario | Muc hien tai | Nhan xet |
|---|---|---|
| Scenario 01 (Xac thuc) | S3 | Logic da on dinh tren CocCoc/Chrome; Safari la INFRA |
| Scenario 02 (Thanh toan/Ghi danh) | S3 | Da co evidence integration, can bo sung black-box UI |
| Scenario 03 (Tien do/Lo trinh) | S3 | Da pass code-level, con gap o tang manual UI |
| Scenario 04 (Cham diem) | S3 | Da pass lien module, can bo sung evidence UI |

**Danh gia trung thuc:** He thong **khong con gap o tang code cho 4 scenario trong tam**, nhung van con `Execution Gap` o phan black-box manual module.

---

## 3) TEST CASE - Truong hop kiem thu chi tiet

## 3.1 Ket qua test case da thuc thi (co artifact)

### A. Unit test

- Lenh: `dotnet test ... --filter "FullyQualifiedName!~Integration"`
- Ket qua: **11/11 PASS**
- Evidence: `tests/artifacts/unit-test-output-20260405.txt`

### B. Integration test

- Lenh: `dotnet test ... --filter "FullyQualifiedName~Integration"`
- Ket qua: **4/4 PASS**
- Evidence: `tests/artifacts/integration-test-output-20260405.txt`

### C. Selenium login da browser (da lam lai script)

- Lenh: `HEADLESS=true ASK_BROWSER_ON_START=false BROWSERS=coccoc,safari,chrome node scripts/login-automation.js`
- Ket qua tong: **45 case | PASS: 30 | FAIL(logic): 0 | INFRA_FAIL: 15**
- Evidence: `tests/1-kiem-thu-tu-dong/login-selenium/artifacts/reports/login-summary-all-20260405-150021.json`

### D. Tong hop cac test da thuc thi

| Nhom | So case | PASS | FAIL logic | INFRA_FAIL |
|---|---:|---:|---:|---:|
| Unit | 11 | 11 | 0 | 0 |
| Integration | 4 | 4 | 0 | 0 |
| Selenium Login | 45 | 30 | 0 | 15 |
| **Tong da thuc thi** | **60** | **45** | **0** | **15** |

## 3.2 Trich dan test case chi tiet theo yeu cau

| ID | Ten Test Case | Du lieu/Buoc | Trang thai moi nhat |
|---|---|---|---|
| AT_LOGIN_13 | SQL injection email | Nhap `' OR 1=1 --` vao Email | **PASS** tren Chrome/CocCoc; Safari = INFRA_FAIL |
| FT_TRAIN_02 | Hoan thanh bai tang tien do | Mo bai hoc -> bam "Xac nhan hoan thanh" | **CHUA CHAY manual** |
| FT_GRADE_03 | Chan bai tap khi chua du dieu kien | Truy cap bai tap khi chua hoc bai truoc | **CHUA CHAY manual** |

## 3.3 Ket qua lam lai phan automation (truoc vs sau)

| Chi so | Truoc khi sua script | Sau khi sua script |
|---|---:|---:|
| Trang thai report | Chi PASS/FAIL | PASS / FAIL(logic) / INFRA_FAIL |
| Full run 45 case | PASS 12, FAIL 33 (nhieu false fail) | PASS 30, FAIL logic 0, INFRA 15 |
| Nhan dien loi Safari | Tron vao FAIL | Tach rieng INFRA_FAIL |

## 3.4 Execution Gap con lai

Cac nhom duoi day da co test case thiet ke nhung chua co bo artifact run dong ket qua:

1. My Training (black-box manual).
2. Grades UI/Function manual day du.
3. Calendar (Ngay/Tuan/Thang) cho Learner/Instructor.
4. Admin Users va Instructor Courses o muc manual regression.

---

## 4) TRACEABILITY MATRIX (RTM)

## 4.1 Giai thich RTM de trinh bay voi giang vien

RTM tra loi 3 cau hoi:

1. Moi requirement da duoc test chua?
2. Test nao dang xac minh requirement do?
3. Trang thai requirement hien tai la PASS/PARTIAL/NOT_EXECUTED?

Neu khong co RTM:

- de bi "test nhieu nhung khong biet da test trung hay bo sot".

Neu co RTM:

- co the bao ve chat luong dua tren truy vet requirement that su.

## 4.2 RTM chi tiet cho he thong hien tai

| Requirement | Test lien ket | Trang thai kiem chung | Muc do hoan thien |
|---|---|---|---|
| R01: Dang nhap da vai tro | AT_LOGIN_01..05,12 | **PARTIAL** (2 browser PASS, Safari INFRA) | 80% |
| R02: Thanh toan tu dong mo khoa | IT PaymentWorkflow + UT Webhook | **PASS code-level** | 70% (thieu black-box UI payment) |
| R03: Kiem soat lo trinh hoc | IT LearningPath + UT LessonProgress + FT_TRAIN_04 | **PASS code-level / Manual chua chay** | 70% |
| R04: Quan ly lich hoc (Ngay/Tuan/Thang) | FT_CAL_01..06 | **NOT_EXECUTED** | 0% |
| R05: Nop bai -> Cham diem -> Xem diem | IT AssignmentFlow + UT Grading + FT_GRADE_xx | **PASS code-level / Manual chua day du** | 70% |
| R06: Validation login dau vao | UT Login + AT_LOGIN_08..11 | **PARTIAL** (Chrome/CocCoc PASS; Safari INFRA) | 80% |

## 4.3 Coverage tong hop tu RTM

- Requirement co test thiet ke: **6/6 (100%)**
- Requirement PASS hoan toan (bao gom UI/manual da dong): **0/6**
- Requirement PASS o tang code nhung chua dong manual UI: **3/6**
- Requirement PARTIAL do cross-browser infra: **2/6**
- Requirement NOT_EXECUTED: **1/6**

=> Nghia la: **thiet ke test da du, nhung execution completion chua du 100% do khoi manual UI va Safari infra.**

---

## 5) 3.5 TONG KET VA DANH GIA CHAT LUONG (Lam Lai)

## 5.1 Danh gia tinh trang thuc te

1. **Tang Back-end/logic hien tai on dinh**:
- Unit 11/11 PASS.
- Integration 4/4 PASS.

2. **Tang Automation login da duoc sua dung huong**:
- Khong con false negative logic tren Chrome/CocCoc.
- Da tach ro `INFRA_FAIL` cho Safari.

3. **Execution Gap van ton tai o black-box manual**:
- Nhieu bang testcase UI/Function da co, nhung chua co artifact thuc thi day du.

## 5.2 Tinh toan chi so chat luong (dua tren case da run)

- Tong case da run co evidence: `60`.
- PASS: `45`.
- FAIL logic: `0`.
- INFRA_FAIL: `15`.

Neu danh gia theo "chat luong logic he thong" (bo qua infra):

- Ty le PASS logic = `45 / (45 + 0) = 100%` tren nhom da thuc thi.

Neu danh gia theo "do san sang ban giao da browser + manual full-scope":

- Chua dat, vi Safari infra va manual black-box chua dong du.

## 5.3 Ket luan cuoi cung

He thong hien tai **khong con o muc "chi thiet ke ma chua thuc thi" cho cac scenario cot loi**, vi da co evidence run ro rang o Unit + Integration + Automation login.

Tuy nhien, de dat muc nghiem thu hoc phan va san sang bao ve chat luong toan dien, can hoan tat 2 viec bat buoc:

1. **Dong execution cho cac bang black-box manual** (My Training, Grades, Calendar, Admin Users, Instructor Courses).
2. **Xu ly hạ tầng Safari** (bat Remote Automation) de ket qua cross-browser dat dong bo.

---

## Phu luc A - Lenh run da su dung

```bash
# Unit
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --no-build --filter "FullyQualifiedName!~Integration" --logger "console;verbosity=detailed"

# Integration
dotnet test EcduLMS.Web.Tests/EcduLMS.Web.Tests.csproj --no-build --filter "FullyQualifiedName~Integration" --logger "console;verbosity=detailed"

# Selenium full browsers
cd "tests/1-kiem-thu-tu-dong/login-selenium"
HEADLESS=true ASK_BROWSER_ON_START=false BROWSERS=coccoc,safari,chrome node scripts/login-automation.js
```

## Phu luc B - File bang chung chinh

1. `tests/artifacts/unit-test-output-20260405.txt`
2. `tests/artifacts/integration-test-output-20260405.txt`
3. `tests/1-kiem-thu-tu-dong/login-selenium/artifacts/reports/login-summary-all-20260405-150021.json`
4. `tests/1-kiem-thu-tu-dong/login-selenium/artifacts/reports/login-summary-all-20260405-150021.md`
