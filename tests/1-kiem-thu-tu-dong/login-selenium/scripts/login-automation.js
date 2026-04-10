"use strict";

require("dotenv").config();
const fs = require("fs");
const path = require("path");
const readline = require("readline");
const { Builder, By, until } = require("selenium-webdriver");
const chrome = require("selenium-webdriver/chrome");

const ROOT_DIR = path.resolve(__dirname, "..");
const ARTIFACTS_DIR = path.join(ROOT_DIR, "artifacts");
const REPORTS_DIR = path.join(ARTIFACTS_DIR, "reports");
const SCREENSHOTS_DIR = path.join(ARTIFACTS_DIR, "screenshots");

ensureDir(ARTIFACTS_DIR);
ensureDir(REPORTS_DIR);
ensureDir(SCREENSHOTS_DIR);

const KNOWN_LOGIN_ERROR_TEXTS = [
  "Email hoặc mật khẩu không đúng.",
  "Vui lòng nhập email",
  "Vui lòng nhập mật khẩu",
  "Email không hợp lệ"
];

const config = {
  baseUrl: process.env.BASE_URL || "http://localhost:5000",
  loginPath: process.env.LOGIN_PATH || "/Account/Login",
  browsers: parseBrowserList(process.env.BROWSERS || "coccoc,safari"),
  askBrowserOnStart: parseBoolean(process.env.ASK_BROWSER_ON_START, true),

  validEmail: process.env.VALID_EMAIL || "alex@example.com",
  validPassword: process.env.VALID_PASSWORD || "Learner@123",
  expectedDashboardPath: process.env.EXPECTED_DASHBOARD_PATH || "/Learner/Dashboard",

  secondaryLearnerEmail: process.env.SECONDARY_LEARNER_EMAIL || "maria@example.com",
  secondaryLearnerPassword: process.env.SECONDARY_LEARNER_PASSWORD || "Learner@123",
  secondaryLearnerDashboardPath:
    process.env.SECONDARY_LEARNER_DASHBOARD_PATH || "/Learner/Dashboard",

  adminEmail: process.env.ADMIN_EMAIL || "admin@edulms.com",
  adminPassword: process.env.ADMIN_PASSWORD || "Admin@123",
  adminDashboardPath: process.env.ADMIN_DASHBOARD_PATH || "/Admin/Dashboard",

  instructorEmail: process.env.INSTRUCTOR_EMAIL || "lehoang@edulms.com",
  instructorPassword: process.env.INSTRUCTOR_PASSWORD || "Instructor@123",
  instructorDashboardPath: process.env.INSTRUCTOR_DASHBOARD_PATH || "/Instructor/Dashboard",

  cocCocBinaryPath: process.env.COCCOC_BINARY_PATH || "",
  chromeBinaryPath: process.env.CHROME_BINARY_PATH || "",
  chromedriverPath: process.env.CHROMEDRIVER_PATH || "",

  timeoutMs: Number(process.env.TIMEOUT_MS || "10000"),
  headless: parseBoolean(process.env.HEADLESS, false),
  testCaseIds: parseCaseIdList(process.env.TEST_CASE_IDS || "")
};

const loginUrl = new URL(config.loginPath, normalizeBaseUrl(config.baseUrl)).toString();
const testCases = filterTestCases(buildLoginTestCases(config), config.testCaseIds);

function buildLoginTestCases(runtimeConfig) {
  const upperLearnerEmail = String(runtimeConfig.validEmail).toUpperCase();

  return [
    {
      id: "AT_LOGIN_01",
      group: "PASS - role learner",
      name: "Dang nhap thanh cong voi learner",
      email: runtimeConfig.validEmail,
      password: runtimeConfig.validPassword,
      expectedType: "success",
      expectedUrlContains: runtimeConfig.expectedDashboardPath,
      expectedUrlContainsAny: buildExpectedSuccessUrls(
        runtimeConfig.expectedDashboardPath,
        "/Learner"
      )
    },
    {
      id: "AT_LOGIN_02",
      group: "PASS - role admin",
      name: "Dang nhap thanh cong voi admin",
      email: runtimeConfig.adminEmail,
      password: runtimeConfig.adminPassword,
      expectedType: "success",
      expectedUrlContains: runtimeConfig.adminDashboardPath,
      expectedUrlContainsAny: buildExpectedSuccessUrls(runtimeConfig.adminDashboardPath, "/Admin")
    },
    {
      id: "AT_LOGIN_03",
      group: "PASS - role instructor",
      name: "Dang nhap thanh cong voi instructor",
      email: runtimeConfig.instructorEmail,
      password: runtimeConfig.instructorPassword,
      expectedType: "success",
      expectedUrlContains: runtimeConfig.instructorDashboardPath,
      expectedUrlContainsAny: buildExpectedSuccessUrls(
        runtimeConfig.instructorDashboardPath,
        "/Instructor"
      )
    },
    {
      id: "AT_LOGIN_04",
      group: "PASS - data variation",
      name: "Dang nhap thanh cong voi learner email viet hoa",
      email: upperLearnerEmail,
      password: runtimeConfig.validPassword,
      expectedType: "success",
      expectedUrlContains: runtimeConfig.expectedDashboardPath,
      expectedUrlContainsAny: buildExpectedSuccessUrls(
        runtimeConfig.expectedDashboardPath,
        "/Learner"
      )
    },
    {
      id: "AT_LOGIN_05",
      group: "PASS - data variation",
      name: "Dang nhap thanh cong voi tai khoan learner thu 2",
      email: runtimeConfig.secondaryLearnerEmail,
      password: runtimeConfig.secondaryLearnerPassword,
      expectedType: "success",
      expectedUrlContains: runtimeConfig.secondaryLearnerDashboardPath,
      expectedUrlContainsAny: buildExpectedSuccessUrls(
        runtimeConfig.secondaryLearnerDashboardPath,
        "/Learner"
      )
    },
    {
      id: "AT_LOGIN_06",
      group: "FAIL - auth",
      name: "Dang nhap that bai khi sai mat khau",
      email: runtimeConfig.validEmail,
      password: "SaiMatKhau@123",
      expectedType: "error",
      expectedTexts: ["Email hoặc mật khẩu không đúng."]
    },
    {
      id: "AT_LOGIN_07",
      group: "FAIL - auth",
      name: "Dang nhap that bai voi email khong ton tai",
      email: "khongtontai@example.com",
      password: "NoUser@123",
      expectedType: "error",
      expectedTexts: ["Email hoặc mật khẩu không đúng."]
    },
    {
      id: "AT_LOGIN_08",
      group: "FAIL - validation",
      name: "Dang nhap that bai khi bo trong email",
      email: "",
      password: runtimeConfig.validPassword,
      expectedType: "error",
      expectedTexts: ["Vui lòng nhập email"]
    },
    {
      id: "AT_LOGIN_09",
      group: "FAIL - validation",
      name: "Dang nhap that bai khi bo trong mat khau",
      email: runtimeConfig.validEmail,
      password: "",
      expectedType: "error",
      expectedTexts: ["Vui lòng nhập mật khẩu"]
    },
    {
      id: "AT_LOGIN_10",
      group: "FAIL - validation",
      name: "Dang nhap that bai khi bo trong ca email va mat khau",
      email: "",
      password: "",
      expectedType: "error",
      expectedTexts: ["Vui lòng nhập email", "Vui lòng nhập mật khẩu"],
      textMatchMode: "all"
    },
    {
      id: "AT_LOGIN_11",
      group: "FAIL - validation",
      name: "Dang nhap that bai khi email sai dinh dang",
      email: "abc.com",
      password: runtimeConfig.validPassword,
      expectedType: "error",
      expectedTexts: ["Email không hợp lệ"],
      allowValidationMessageFallback: true
    },
    {
      id: "AT_LOGIN_12",
      group: "PASS - data variation",
      name: "Dang nhap thanh cong khi email co khoang trang dau/cuoi",
      email: " alex@example.com ",
      password: runtimeConfig.validPassword,
      expectedType: "success",
      expectedUrlContains: runtimeConfig.expectedDashboardPath,
      expectedUrlContainsAny: buildExpectedSuccessUrls(
        runtimeConfig.expectedDashboardPath,
        "/Learner"
      )
    },
    {
      id: "AT_LOGIN_13",
      group: "FAIL - security",
      name: "Dang nhap that bai voi chuoi SQL injection o email",
      email: "' OR 1=1 --",
      password: "Any@123",
      expectedType: "error",
      expectedTexts: ["Email không hợp lệ"],
      allowValidationMessageFallback: true,
      allowAnyKnownLoginError: true
    },
    {
      id: "AT_LOGIN_14",
      group: "FAIL - security",
      name: "Dang nhap that bai voi chuoi SQL injection o mat khau",
      email: runtimeConfig.validEmail,
      password: "' OR 1=1 --",
      expectedType: "error",
      expectedTexts: ["Email hoặc mật khẩu không đúng."]
    },
    {
      id: "AT_LOGIN_15",
      group: "FAIL - security",
      name: "Dang nhap that bai voi email chua script",
      email: "<script>alert(1)</script>@mail.com",
      password: runtimeConfig.validPassword,
      expectedType: "error",
      expectedTexts: ["Email không hợp lệ"],
      allowValidationMessageFallback: true,
      allowAnyKnownLoginError: true
    }
  ];
}

async function resolveBrowsersToRun(runtimeConfig) {
  if (!runtimeConfig.askBrowserOnStart) {
    return runtimeConfig.browsers;
  }

  if (!process.stdin.isTTY || !process.stdout.isTTY) {
    return runtimeConfig.browsers;
  }

  return askBrowserSelection(runtimeConfig.browsers);
}

async function askBrowserSelection(defaultBrowsers) {
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
  });

  const question =
    "\nChon browser de chay:\n" +
    "1) Coc Coc\n" +
    "2) Safari\n" +
    "3) Chrome\n" +
    "4) Coc Coc + Safari\n" +
    "5) Tat ca (Coc Coc + Safari + Chrome)\n" +
    `Nhan Enter de dung mac dinh (${defaultBrowsers.join(", ")})\n` +
    "Lua chon cua ban: ";

  try {
    const answer = await askQuestion(rl, question);
    const normalized = String(answer || "").trim().toLowerCase();

    if (!normalized) {
      return defaultBrowsers;
    }

    if (normalized === "1") {
      return ["coccoc"];
    }
    if (normalized === "2") {
      return ["safari"];
    }
    if (normalized === "3") {
      return ["chrome"];
    }
    if (normalized === "4") {
      return ["coccoc", "safari"];
    }
    if (normalized === "5") {
      return ["coccoc", "safari", "chrome"];
    }

    const parsed = parseBrowserList(normalized);
    return parsed;
  } catch (error) {
    console.warn(
      `\n[WARN] Khong doc duoc lua chon browser, dung mac dinh: ${defaultBrowsers.join(", ")}`
    );
    return defaultBrowsers;
  } finally {
    rl.close();
  }
}

function askQuestion(rl, question) {
  return new Promise((resolve) => {
    rl.question(question, (answer) => resolve(answer));
  });
}

async function runSuiteForBrowser(browserName, suiteCases) {
  const startedAt = new Date();
  const results = [];
  const preflightError = await preflightBrowser(browserName);

  if (preflightError) {
    const classified = classifyTestFailure(preflightError);

    console.log(
      `[INFRA_FAIL] [${browserName}] Browser preflight that bai: ${classified.message}`
    );

    for (const testCase of suiteCases) {
      results.push(buildInfraResult(testCase, browserName, classified));
    }

    const endedAt = new Date();
    return {
      browser: browserName,
      startedAt: startedAt.toISOString(),
      endedAt: endedAt.toISOString(),
      durationMs: endedAt.getTime() - startedAt.getTime(),
      total: results.length,
      passCount: 0,
      nonPassCount: results.length,
      failCount: 0,
      infraFailCount: results.length,
      results
    };
  }

  for (const testCase of suiteCases) {
    const result = await runOneTestCase(testCase, browserName);
    results.push(result);
    const icon = result.status;
    console.log(`[${icon}] [${browserName}] ${result.id} - ${result.name}`);
  }

  const endedAt = new Date();
  const passCount = results.filter((x) => x.status === "PASS").length;
  const failCount = results.filter((x) => x.status === "FAIL").length;
  const infraFailCount = results.filter((x) => x.status === "INFRA_FAIL").length;
  const nonPassCount = failCount + infraFailCount;

  return {
    browser: browserName,
    startedAt: startedAt.toISOString(),
    endedAt: endedAt.toISOString(),
    durationMs: endedAt.getTime() - startedAt.getTime(),
    total: results.length,
    passCount,
    nonPassCount,
    failCount,
    infraFailCount,
    results
  };
}

async function preflightBrowser(browserName) {
  let driver;

  try {
    driver = await createDriver(browserName);
    await driver.get("about:blank");
    return null;
  } catch (error) {
    return error;
  } finally {
    if (driver) {
      try {
        await driver.quit();
      } catch (_) {
        // Ignore cleanup failure in preflight.
      }
    }
  }
}

function buildInfraResult(testCase, browserName, classified) {
  return {
    id: testCase.id,
    group: testCase.group || "",
    name: testCase.name,
    browser: browserName,
    status: "INFRA_FAIL",
    durationMs: 0,
    observedUrl: "",
    expectedType: testCase.expectedType,
    expectedTexts: normalizeExpectedTexts(testCase),
    expectedUrlContains: testCase.expectedUrlContains || "",
    expectedUrlContainsAny: getSuccessUrlCandidates(testCase),
    expectedValidationField: testCase.expectedValidationField || "",
    observedValidationMessage: "",
    observedBodySnippet: "",
    screenshotFile: "",
    errorType: classified.errorType,
    errorMessage: classified.message
  };
}

function writeBrowserReports(browserSummary, runTimestamp) {
  const browserSlug = sanitize(browserSummary.browser.toLowerCase());
  const jsonFile = path.join(REPORTS_DIR, `login-summary-${browserSlug}-${runTimestamp}.json`);
  const mdFile = path.join(REPORTS_DIR, `login-summary-${browserSlug}-${runTimestamp}.md`);
  const latestJsonFile = path.join(REPORTS_DIR, `latest-summary-${browserSlug}.json`);
  const latestMdFile = path.join(REPORTS_DIR, `latest-summary-${browserSlug}.md`);

  fs.writeFileSync(jsonFile, JSON.stringify(browserSummary, null, 2), "utf8");
  fs.writeFileSync(mdFile, buildBrowserMarkdownReport(browserSummary), "utf8");
  fs.writeFileSync(latestJsonFile, JSON.stringify(browserSummary, null, 2), "utf8");
  fs.writeFileSync(latestMdFile, buildBrowserMarkdownReport(browserSummary), "utf8");
}

async function runOneTestCase(testCase, browserName) {
  const startedAt = Date.now();
  let driver;
  let screenshotFile = "";
  let screenshotTag = "PASS";
  let status = "PASS";
  let errorType = "";
  let errorMessage = "";
  let observedUrl = "";
  let observedValidationMessage = "";
  let observedBodySnippet = "";

  try {
    driver = await createDriver(browserName);
    await driver.get(loginUrl);
    await driver.wait(until.elementLocated(By.id("Email")), config.timeoutMs);

    const emailInput = await driver.findElement(By.id("Email"));
    const passwordInput = await driver.findElement(By.id("Password"));
    const submitButton = await driver.findElement(By.css("button[type='submit']"));

    await setInputValue(driver, emailInput, testCase.email);
    await setInputValue(driver, passwordInput, testCase.password);
    await submitLoginForm(driver, submitButton);
    const assertionResult = await assertExpectedOutcome(
      driver,
      testCase,
      config.timeoutMs
    );
    observedUrl = assertionResult.observedUrl || "";
    observedValidationMessage = assertionResult.observedValidationMessage || "";
    observedBodySnippet = assertionResult.observedBodySnippet || "";
  } catch (error) {
    const classified = classifyTestFailure(error);
    status = classified.status;
    errorType = classified.errorType;
    errorMessage = classified.message;
    screenshotTag = status;

    if (driver) {
      observedUrl = await getCurrentUrlSafe(driver);
      if (!observedValidationMessage) {
        observedValidationMessage = await getAnyValidationMessage(driver);
      }
      if (!observedBodySnippet) {
        observedBodySnippet = await getBodySnippet(driver, 240);
      }

      if (
        status === "FAIL" &&
        looksLikeServerInfraFailure(observedBodySnippet, errorMessage)
      ) {
        status = "INFRA_FAIL";
        errorType = "INFRA_APP_UNHEALTHY";
      }
    }
  } finally {
    if (driver) {
      try {
        screenshotFile = await takeScreenshot(
          driver,
          `${browserName}_${testCase.id}_${screenshotTag}`
        );
      } catch (_) {
        screenshotFile = "";
      }
      await driver.quit();
    }
  }

  return {
    id: testCase.id,
    group: testCase.group || "",
    name: testCase.name,
    browser: browserName,
    status,
    durationMs: Date.now() - startedAt,
    observedUrl,
    expectedType: testCase.expectedType,
    expectedTexts: normalizeExpectedTexts(testCase),
    expectedUrlContains: testCase.expectedUrlContains || "",
    expectedUrlContainsAny: getSuccessUrlCandidates(testCase),
    expectedValidationField: testCase.expectedValidationField || "",
    observedValidationMessage,
    observedBodySnippet,
    screenshotFile,
    errorType,
    errorMessage
  };
}

async function assertExpectedOutcome(driver, testCase, timeoutMs) {
  const expectedType = String(testCase.expectedType || "success").trim();

  if (expectedType === "success") {
    return assertSuccessOutcome(driver, testCase, timeoutMs);
  }

  if (expectedType === "error") {
    return assertErrorOutcome(driver, testCase, timeoutMs);
  }

  if (expectedType === "browserValidation") {
    return assertBrowserValidationOutcome(driver, testCase);
  }

  throw new Error(`expectedType khong duoc ho tro: ${expectedType}`);
}

async function assertSuccessOutcome(driver, testCase, timeoutMs) {
  const observedUrl = await waitForLoginSuccess(driver, timeoutMs);
  const currentPath = extractPathname(observedUrl);
  const candidates = getSuccessUrlCandidates(testCase);
  const matched = candidates.some((candidate) => isExpectedPathMatch(currentPath, candidate));

  if (!matched) {
    throw new Error(
      `Dang nhap thanh cong nhung URL khong dung ky vong. URL=${observedUrl}; expected any of ${candidates.join(", ")}`
    );
  }

  await assertKnownErrorTextsNotVisible(driver, KNOWN_LOGIN_ERROR_TEXTS, 1000);

  return {
    observedUrl,
    observedValidationMessage: "",
    observedBodySnippet: await getBodySnippet(driver, 240)
  };
}

async function assertErrorOutcome(driver, testCase, timeoutMs) {
  const observedUrl = await waitForLoginFailure(driver, timeoutMs);
  const expectedTexts = normalizeExpectedTexts(testCase);
  const mode = testCase.textMatchMode === "all" ? "all" : "any";
  let bodyText = await getBodyText(driver);
  let textMatched = matchTexts(bodyText, expectedTexts, mode);

  if (!textMatched && expectedTexts.length > 0) {
    try {
      await waitForExpectedTexts(driver, expectedTexts, mode, Math.min(timeoutMs, 6000));
    } catch (_) {
      // Keep fallback checks below.
    }
    bodyText = await getBodyText(driver);
    textMatched = matchTexts(bodyText, expectedTexts, mode);
  }

  if (textMatched) {
    return {
      observedUrl,
      observedValidationMessage: "",
      observedBodySnippet: bodyText.slice(0, 240)
    };
  }

  if (testCase.allowAnyKnownLoginError) {
    const knownLoginErrorMatched = matchTexts(
      bodyText,
      KNOWN_LOGIN_ERROR_TEXTS,
      "any"
    );

    if (knownLoginErrorMatched) {
      return {
        observedUrl,
        observedValidationMessage: "",
        observedBodySnippet: bodyText.slice(0, 240)
      };
    }
  }

  if (testCase.allowValidationMessageFallback || testCase.expectedType === "browserValidation") {
    const validationMessage = await getAnyValidationMessage(driver);
    if (validationMessage) {
      return {
        observedUrl,
        observedValidationMessage: validationMessage,
        observedBodySnippet: bodyText.slice(0, 240)
      };
    }
  }

  const expectedTextHint =
    expectedTexts.length > 0 ? expectedTexts.join(" | ") : "(khong co expectedTexts)";
  throw new Error(
    `Khong tim thay thong bao loi mong doi. expected=${expectedTextHint}; url=${observedUrl}`
  );
}

async function assertBrowserValidationOutcome(driver, testCase) {
  const observedUrl = await getCurrentUrlSafe(driver);

  if (!isOnLoginPath(observedUrl)) {
    throw new Error(
      `Ky vong browser validation nhung he thong da roi khoi trang login. URL=${observedUrl}`
    );
  }

  const preferredField = (testCase.expectedValidationField || "email").toLowerCase();
  let observedValidationMessage = await getValidationMessageByField(driver, preferredField);
  if (!observedValidationMessage) {
    observedValidationMessage = await getAnyValidationMessage(driver);
  }

  if (!observedValidationMessage) {
    throw new Error(
      `Khong lay duoc browser validation message cho field=${preferredField}`
    );
  }

  return {
    observedUrl,
    observedValidationMessage,
    observedBodySnippet: await getBodySnippet(driver, 240)
  };
}

function normalizeExpectedTexts(testCase) {
  if (Array.isArray(testCase.expectedTexts)) {
    return testCase.expectedTexts.filter(Boolean);
  }
  if (typeof testCase.expectedText === "string" && testCase.expectedText.length > 0) {
    return [testCase.expectedText];
  }
  return [];
}

async function waitForExpectedTexts(driver, expectedTexts, mode, timeoutMs) {
  await driver.wait(async () => {
    const bodyText = await getBodyText(driver);
    return matchTexts(bodyText, expectedTexts, mode);
  }, timeoutMs);
}

function matchTexts(bodyText, expectedTexts, mode) {
  if (!Array.isArray(expectedTexts) || expectedTexts.length === 0) {
    return true;
  }
  const normalizedBody = normalizeTextForMatch(bodyText);
  const checks = expectedTexts.map((text) =>
    normalizedBody.includes(normalizeTextForMatch(text))
  );
  if (mode === "all") {
    return checks.every(Boolean);
  }
  return checks.some(Boolean);
}

function normalizeTextForMatch(input) {
  return String(input || "")
    .normalize("NFC")
    .replace(/\s+/g, " ")
    .trim()
    .toLowerCase();
}

function extractPathname(url) {
  try {
    const parsed = new URL(url);
    return normalizeExpectedPath(parsed.pathname || "");
  } catch (_) {
    return "";
  }
}

function isOnLoginPath(url) {
  const currentPath = extractPathname(url);
  const loginPath = normalizeExpectedPath(config.loginPath);
  return currentPath === loginPath;
}

async function waitForLoginSuccess(driver, timeoutMs) {
  await driver.wait(
    async () => {
      try {
        const currentUrl = await driver.getCurrentUrl();
        return !isOnLoginPath(currentUrl);
      } catch (_) {
        return false;
      }
    },
    timeoutMs,
    "Khong dang nhap duoc vao he thong."
  );

  return driver.getCurrentUrl();
}

async function waitForLoginFailure(driver, timeoutMs) {
  await driver.wait(
    async () => {
      const currentUrl = await getCurrentUrlSafe(driver);
      return isOnLoginPath(currentUrl);
    },
    timeoutMs,
    "Ky vong dang nhap that bai nhung URL khong o trang login."
  );

  const currentUrl = await getCurrentUrlSafe(driver);
  if (!isOnLoginPath(currentUrl)) {
    throw new Error(`Dang nhap that bai khong thanh cong, URL hien tai: ${currentUrl}`);
  }
  return currentUrl;
}

function buildLoginFailedMessage(error) {
  const raw = error instanceof Error ? error.message : String(error || "");
  const detail = String(raw || "").trim();
  if (!detail) {
    return "Khong dang nhap duoc vao he thong.";
  }
  if (detail.toLowerCase().includes("khong dang nhap duoc vao he thong")) {
    return detail;
  }
  return `Khong dang nhap duoc vao he thong. ${detail}`;
}

function classifyTestFailure(error) {
  const raw = error instanceof Error ? error.message : String(error || "");
  const message = String(raw || "").trim() || "Loi khong xac dinh";
  const normalized = message.toLowerCase();
  const infraPatterns = [
    "allow remote automation",
    "safaridriver",
    "session not created",
    "could not create session",
    "cannot find chrome binary",
    "khong tim thay coccoc binary",
    "browser khong duoc ho tro",
    "no such driver",
    "connection refused",
    "err_connection_refused",
    "net::err_connection_refused",
    "unable to connect",
    "target machine actively refused",
    "unable to obtain browser driver",
    "unable to obtain driver",
    "error executing command for",
    "selenium-manager",
    "chrome for testing",
    "last-known-good-versions-with-downloads.json",
    "googlechromelabs.github.io",
    "operation not permitted",
    "eperm",
    "listen eperm",
    "chromedriver"
  ];
  const isInfra = infraPatterns.some((x) => normalized.includes(x));

  if (isInfra) {
    return {
      status: "INFRA_FAIL",
      errorType: "INFRA",
      message
    };
  }

  return {
    status: "FAIL",
    errorType: "ASSERTION",
    message
  };
}

function looksLikeServerInfraFailure(bodySnippet, message) {
  const raw = `${bodySnippet || ""}\n${message || ""}`.toLowerCase();
  const indicators = [
    "http error 500",
    "trang này hiện không hoạt động",
    "localhost hiện không thể xử lý yêu cầu này",
    "connect timeout expired",
    "mysql",
    "couldn't connect to server"
  ];
  return indicators.some((x) => raw.includes(x));
}

function getSuccessUrlCandidates(testCase) {
  const raw = [];
  if (typeof testCase.expectedUrlContains === "string" && testCase.expectedUrlContains.length > 0) {
    raw.push(testCase.expectedUrlContains);
  }
  if (Array.isArray(testCase.expectedUrlContainsAny)) {
    raw.push(...testCase.expectedUrlContainsAny);
  }

  return [...new Set(raw.map(normalizeExpectedPath).filter(Boolean))];
}

function normalizeExpectedPath(input) {
  if (!input) {
    return "";
  }

  let value = String(input).trim();
  if (!value) {
    return "";
  }

  if (/^https?:\/\//i.test(value)) {
    try {
      value = new URL(value).pathname || "";
    } catch (_) {
      return "";
    }
  }

  if (!value.startsWith("/")) {
    value = `/${value}`;
  }

  if (value.length > 1 && value.endsWith("/")) {
    value = value.slice(0, -1);
  }

  return value;
}

function buildExpectedSuccessUrls(primaryPath, areaRootPath) {
  const candidates = new Set();
  const normalizedPrimary = normalizeExpectedPath(primaryPath);
  const normalizedAreaRoot = normalizeExpectedPath(areaRootPath);

  if (normalizedPrimary) {
    candidates.add(normalizedPrimary);
    if (normalizedPrimary.toLowerCase().endsWith("/dashboard")) {
      candidates.add(normalizedPrimary.slice(0, -"/Dashboard".length));
    }
  }

  if (normalizedAreaRoot) {
    candidates.add(normalizedAreaRoot);
  }

  return [...candidates];
}

function isExpectedPathMatch(currentPath, expectedPath) {
  const current = normalizeExpectedPath(currentPath);
  const expected = normalizeExpectedPath(expectedPath);
  if (!current || !expected) {
    return false;
  }
  if (current === expected) {
    return true;
  }
  return current.startsWith(`${expected}/`);
}

async function assertKnownErrorTextsNotVisible(driver, disallowedTexts, timeoutMs) {
  await driver.wait(async () => {
    const bodyText = await getBodyText(driver);
    return disallowedTexts.every((text) => !bodyText.includes(text));
  }, timeoutMs);
}

async function getCurrentUrlSafe(driver) {
  try {
    return await driver.getCurrentUrl();
  } catch (_) {
    return "";
  }
}

async function getValidationMessageByField(driver, field) {
  const selectorMap = {
    email: "#Email",
    password: "#Password"
  };
  const selector = selectorMap[field];
  if (!selector) {
    return "";
  }
  try {
    const element = await driver.findElement(By.css(selector));
    const message = await driver.executeScript(
      "return arguments[0] && arguments[0].validationMessage ? arguments[0].validationMessage : '';",
      element
    );
    return String(message || "").trim();
  } catch (_) {
    return "";
  }
}

async function setInputValue(driver, element, value) {
  const normalizedValue = typeof value === "string" ? value : "";

  await driver.executeScript(
    `
      const el = arguments[0];
      const nextValue = arguments[1];
      if (!el) {
        return;
      }

      el.focus();
      el.value = '';
      el.dispatchEvent(new Event('input', { bubbles: true }));
      el.dispatchEvent(new Event('change', { bubbles: true }));
      el.value = nextValue;
      el.dispatchEvent(new Event('input', { bubbles: true }));
      el.dispatchEvent(new Event('change', { bubbles: true }));
      el.blur();
    `,
    element,
    normalizedValue
  );

  const actualValue = await element.getAttribute("value");
  if (actualValue === normalizedValue) {
    return;
  }

  await element.click();
  try {
    await element.clear();
  } catch (_) {
    // Continue with sendKeys fallback below.
  }

  if (normalizedValue.length > 0) {
    await element.sendKeys(normalizedValue);
  }
}

async function submitLoginForm(driver, submitButton) {
  await driver.executeScript(
    `
      const btn = arguments[0];
      if (!btn) {
        return;
      }

      const form = btn.form || btn.closest('form');
      if (!form) {
        btn.click();
        return;
      }

      if (typeof form.requestSubmit === 'function') {
        form.requestSubmit(btn);
        return;
      }

      HTMLFormElement.prototype.submit.call(form);
    `,
    submitButton
  );
}

async function getAnyValidationMessage(driver) {
  const emailMessage = await getValidationMessageByField(driver, "email");
  if (emailMessage) {
    return emailMessage;
  }
  return getValidationMessageByField(driver, "password");
}

function createDriver(browserName) {
  if (browserName === "safari") {
    if (config.headless) {
      console.warn("[WARN] Safari khong ho tro headless, bo qua HEADLESS cho Safari.");
    }
    return new Builder().forBrowser("safari").build();
  }

  const options = new chrome.Options();
  const chromiumBinary = resolveChromiumBinary(browserName);

  if (chromiumBinary) {
    options.setChromeBinaryPath(chromiumBinary);
  }

  if (config.headless) {
    options.addArguments("--headless=new");
    options.addArguments("--window-size=1600,1000");
    options.addArguments("--disable-gpu");
  }

  options.addArguments("--disable-notifications");
  options.addArguments("--no-sandbox");

  let builder = new Builder().forBrowser("chrome").setChromeOptions(options);

  if (config.chromedriverPath) {
    const service = new chrome.ServiceBuilder(config.chromedriverPath);
    builder = builder.setChromeService(service);
  }

  return builder.build();
}

function resolveChromiumBinary(browserName) {
  if (browserName === "chrome") {
    return config.chromeBinaryPath || "";
  }

  if (browserName === "coccoc") {
    if (config.cocCocBinaryPath) {
      return config.cocCocBinaryPath;
    }

    const candidates = [
      "/Applications/CocCoc.app/Contents/MacOS/CocCoc",
      "/Applications/CocCoc Browser.app/Contents/MacOS/CocCoc Browser"
    ];

    for (const candidate of candidates) {
      if (fs.existsSync(candidate)) {
        return candidate;
      }
    }

    throw new Error(
      "Khong tim thay Coc Coc binary. Hay set COCCOC_BINARY_PATH trong file .env."
    );
  }

  throw new Error(`Browser khong duoc ho tro: ${browserName}`);
}

async function getBodyText(driver) {
  const text = await driver.executeScript(
    "return document && document.body ? (document.body.innerText || document.body.textContent || '') : '';"
  );
  return String(text || "");
}

async function getBodySnippet(driver, maxLen) {
  const body = await getBodyText(driver);
  return String(body || "").slice(0, maxLen);
}

async function takeScreenshot(driver, prefix) {
  const fileName = `${formatTimestamp(new Date())}_${sanitize(prefix)}.png`;
  const fullPath = path.join(SCREENSHOTS_DIR, fileName);
  const data = await driver.takeScreenshot();
  fs.writeFileSync(fullPath, data, "base64");
  return fullPath;
}

function formatTimestamp(date) {
  const pad = (n) => String(n).padStart(2, "0");
  const yyyy = date.getFullYear();
  const mm = pad(date.getMonth() + 1);
  const dd = pad(date.getDate());
  const hh = pad(date.getHours());
  const mi = pad(date.getMinutes());
  const ss = pad(date.getSeconds());
  return `${yyyy}${mm}${dd}-${hh}${mi}${ss}`;
}

function sanitize(input) {
  return input.replace(/[^a-zA-Z0-9_-]/g, "_");
}

function ensureDir(dir) {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

function normalizeBaseUrl(baseUrl) {
  if (!baseUrl.endsWith("/")) {
    return `${baseUrl}/`;
  }
  return baseUrl;
}

function escapePipe(value) {
  if (!value) {
    return "";
  }
  return String(value).replace(/\|/g, "\\|");
}

function parseBrowserList(raw) {
  const normalized = String(raw || "")
    .split(",")
    .map((x) => x.trim().toLowerCase())
    .filter(Boolean)
    .map((x) => {
      if (x === "coc-coc" || x === "coc_coc" || x === "coccoc") {
        return "coccoc";
      }
      if (x === "safari") {
        return "safari";
      }
      if (x === "chrome") {
        return "chrome";
      }
      if (x === "all") {
        return "all";
      }
      return x;
    });

  const expanded = [];
  for (const item of normalized) {
    if (item === "all") {
      expanded.push("coccoc", "safari", "chrome");
      continue;
    }
    expanded.push(item);
  }

  const unique = [...new Set(expanded)];
  const supported = ["coccoc", "safari", "chrome"];
  for (const browser of unique) {
    if (!supported.includes(browser)) {
      throw new Error(
        `Browser "${browser}" khong duoc ho tro. Hay dung: ${supported.join(", ")}`
      );
    }
  }

  return unique.length > 0 ? unique : ["coccoc"];
}

function parseCaseIdList(raw) {
  return String(raw || "")
    .split(",")
    .map((x) => x.trim().toUpperCase())
    .filter(Boolean);
}

function filterTestCases(allCases, selectedIds) {
  if (!Array.isArray(selectedIds) || selectedIds.length === 0) {
    return allCases;
  }

  const selectedSet = new Set(selectedIds);
  const filtered = allCases.filter((x) => selectedSet.has(String(x.id || "").toUpperCase()));

  if (filtered.length === 0) {
    throw new Error(
      `Khong tim thay test case nao theo TEST_CASE_IDS=${selectedIds.join(", ")}`
    );
  }

  return filtered;
}

function parseBoolean(value, defaultValue) {
  if (value === undefined || value === null || value === "") {
    return defaultValue;
  }
  return String(value).toLowerCase() === "true";
}

function buildBrowserMarkdownReport(summary) {
  const lines = [];
  lines.push(`# Bao cao kiem thu tu dong - Dang nhap (${summary.browser})`);
  lines.push("");
  lines.push(`- Browser: ${summary.browser}`);
  lines.push(`- Thoi gian bat dau: ${summary.startedAt}`);
  lines.push(`- Thoi gian ket thuc: ${summary.endedAt}`);
  lines.push(`- Tong so case: ${summary.total}`);
  lines.push(`- PASS: ${summary.passCount}`);
  lines.push(`- FAIL (logic): ${summary.failCount}`);
  lines.push(`- INFRA_FAIL: ${summary.infraFailCount || 0}`);
  lines.push("");
  lines.push(
    "| ID | Nhom | Ten test case | Ket qua | Loai loi | URL thuc te | Validation message | Loi (neu co) | Screenshot |"
  );
  lines.push("|---|---|---|---|---|---|---|---|---|");

  for (const item of summary.results) {
    lines.push(
      `| ${item.id} | ${escapePipe(item.group)} | ${escapePipe(item.name)} | ${item.status} | ${escapePipe(item.errorType)} | ${escapePipe(item.observedUrl)} | ${escapePipe(item.observedValidationMessage)} | ${escapePipe(item.errorMessage)} | ${escapePipe(item.screenshotFile)} |`
    );
  }

  lines.push("");
  lines.push("## Nhan xet nhanh");
  if (summary.failCount === 0 && (summary.infraFailCount || 0) === 0) {
    lines.push("- Tat ca test case deu PASS.");
  } else {
    if ((summary.infraFailCount || 0) > 0) {
      lines.push("- Co case INFRA_FAIL (loi ha tang browser/moi truong test).");
    }
    if (summary.failCount > 0) {
      lines.push("- Co case FAIL logic (khong dat expected theo nghiep vu test).");
    }
    lines.push("- Xem chi tiet trong cac truong `errorType`, `errorMessage`, `observedValidationMessage` cua JSON.");
  }

  return lines.join("\n");
}

function buildCombinedMarkdownReport(summary) {
  const lines = [];
  lines.push("# Bao cao kiem thu tu dong - Dang nhap (Tong hop nhieu browser)");
  lines.push("");
  lines.push(`- Thoi gian bat dau: ${summary.startedAt}`);
  lines.push(`- Thoi gian ket thuc: ${summary.endedAt}`);
  lines.push(`- Tong so test case duy nhat: ${summary.caseCount}`);
  lines.push(`- Tong so browser: ${summary.browserCount}`);
  lines.push(`- Tong so luot chay: ${summary.executionCount}`);
  lines.push(`- PASS: ${summary.passCount}`);
  lines.push(`- FAIL (logic): ${summary.failCount}`);
  lines.push(`- INFRA_FAIL: ${summary.infraFailCount || 0}`);
  lines.push("");
  lines.push("| Browser | Tong case | PASS | FAIL (logic) | INFRA_FAIL |");
  lines.push("|---|---|---|---|---|");

  for (const browserSummary of summary.browserSummaries) {
    lines.push(
      `| ${browserSummary.browser} | ${browserSummary.total} | ${browserSummary.passCount} | ${browserSummary.failCount} | ${browserSummary.infraFailCount || 0} |`
    );
  }

  lines.push("");
  lines.push("## Bao cao chi tiet tung browser");
  lines.push(
    "- Mo cac file `latest-summary-coccoc.md`, `latest-summary-safari.md`, `latest-summary-chrome.md` trong thu muc `artifacts/reports`."
  );

  return lines.join("\n");
}

main().catch((error) => {
  console.error("Loi khi chay test automation:", error);
  process.exit(1);
});

async function main() {
  const startedAt = new Date();
  const runTimestamp = formatTimestamp(startedAt);
  const browserSummaries = [];
  const browsersToRun = await resolveBrowsersToRun(config);
  const caseCount = testCases.length;
  const browserCount = browsersToRun.length;

  console.log("=== BAT DAU KIEM THU TU DONG: CHUC NANG DANG NHAP ===");
  console.log(`Login URL: ${loginUrl}`);
  console.log(`So test case duy nhat: ${caseCount}`);
  console.log(`So browser: ${browserCount}`);
  console.log(`Danh sach browser: ${browsersToRun.join(", ")}`);

  for (const browserName of browsersToRun) {
    console.log("");
    console.log(`--- CHAY BROWSER: ${browserName.toUpperCase()} ---`);
    const browserSummary = await runSuiteForBrowser(browserName, testCases);
    browserSummaries.push(browserSummary);
    writeBrowserReports(browserSummary, runTimestamp);
  }

  const endedAt = new Date();
  const executionCount = browserSummaries.reduce((acc, x) => acc + x.total, 0);
  const passCount = browserSummaries.reduce((acc, x) => acc + x.passCount, 0);
  const failCount = browserSummaries.reduce((acc, x) => acc + x.failCount, 0);
  const infraFailCount = browserSummaries.reduce((acc, x) => acc + (x.infraFailCount || 0), 0);
  const nonPassCount = failCount + infraFailCount;

  const combinedSummary = {
    startedAt: startedAt.toISOString(),
    endedAt: endedAt.toISOString(),
    durationMs: endedAt.getTime() - startedAt.getTime(),
    config: {
      baseUrl: config.baseUrl,
      loginPath: config.loginPath,
      loginUrl,
      timeoutMs: config.timeoutMs,
      headless: config.headless,
      browsers: browsersToRun
    },
    caseCount,
    browserCount,
    executionCount,
    total: executionCount,
    passCount,
    failCount,
    infraFailCount,
    nonPassCount,
    browserSummaries
  };

  const summaryJsonFile = path.join(REPORTS_DIR, `login-summary-all-${runTimestamp}.json`);
  const summaryMdFile = path.join(REPORTS_DIR, `login-summary-all-${runTimestamp}.md`);
  const latestJsonFile = path.join(REPORTS_DIR, "latest-summary.json");
  const latestMdFile = path.join(REPORTS_DIR, "latest-summary.md");

  fs.writeFileSync(summaryJsonFile, JSON.stringify(combinedSummary, null, 2), "utf8");
  fs.writeFileSync(summaryMdFile, buildCombinedMarkdownReport(combinedSummary), "utf8");
  fs.writeFileSync(latestJsonFile, JSON.stringify(combinedSummary, null, 2), "utf8");
  fs.writeFileSync(latestMdFile, buildCombinedMarkdownReport(combinedSummary), "utf8");

  console.log("");
  console.log(
    `Tong ket tat ca browser: ${caseCount} case x ${browserCount} browser = ${executionCount} luot chay | PASS: ${passCount} | FAIL(logic): ${failCount} | INFRA_FAIL: ${infraFailCount}`
  );
  for (const browserSummary of browserSummaries) {
    console.log(
      `  - ${browserSummary.browser}: ${browserSummary.total} case | PASS: ${browserSummary.passCount} | FAIL(logic): ${browserSummary.failCount} | INFRA_FAIL: ${browserSummary.infraFailCount || 0}`
    );
  }
  console.log(`Bao cao JSON: ${summaryJsonFile}`);
  console.log(`Bao cao Markdown: ${summaryMdFile}`);
  console.log(`Anh screenshot: ${SCREENSHOTS_DIR}`);

  if (nonPassCount > 0) {
    process.exitCode = 1;
  }
}
