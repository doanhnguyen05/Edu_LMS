using EduLMS.Web.Models;
using EduLMS.Web.Models.Enums;
using EduLMS.Web.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace EduLMS.Web.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            await SeedRolesAsync(roleManager);
            var users = await SeedUsersAsync(userManager);

            await SeedCourseCategoriesAsync(context);
            await SeedNotificationRulesAsync(context);

            var courses = await SeedCoursesAsync(context, users.Instructors);
            await SeedCourseContentAsync(context, courses);

            await SeedEnrollmentsAsync(context, users.Learners, courses);
            await SeedLessonResourcesAsync(context, courses);
            await SeedLessonProgressAsync(context, users.Learners, courses);
            await SeedSubmissionsAndGradesAsync(context, users.Learners, users.Instructors, courses);
            await SeedPaymentsAndEarningsAsync(context, users.Admin, users.Instructors, users.Learners, courses);
            await SeedSchedulesAsync(context, users.Instructors, courses);
            await SeedCalendarEventsAsync(context, users.Instructors, users.Learners);
            await SeedNotificationsAsync(context, users.Admin, users.Instructors, users.Learners, courses);
            await SeedActivityLogsAsync(context, users.Admin, users.Instructors, users.Learners);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Instructor", "Learner" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task<SeedUsers> SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            var admin = await EnsureUserAsync(
                userManager,
                email: "admin@edulms.com",
                fullName: "System Admin",
                password: "Admin@123",
                role: "Admin");

            var instructors = new Dictionary<string, ApplicationUser>(StringComparer.OrdinalIgnoreCase)
            {
                ["lehoang@edulms.com"] = await EnsureUserAsync(
                    userManager,
                    email: "lehoang@edulms.com",
                    fullName: "Tien si Le Hoang",
                    password: "Instructor@123",
                    role: "Instructor",
                    bio: "Chuyen gia AI & Machine Learning",
                    bankName: "MB Bank",
                    bankAccountNumber: "0123456789",
                    bankAccountName: "LE HOANG",
                    commissionRate: 0.30m),

                ["minhtu@edulms.com"] = await EnsureUserAsync(
                    userManager,
                    email: "minhtu@edulms.com",
                    fullName: "ThS. Minh Tu",
                    password: "Instructor@123",
                    role: "Instructor",
                    bio: "Senior Product Designer",
                    bankName: "Vietcombank",
                    bankAccountNumber: "0987654321",
                    bankAccountName: "MINH TU",
                    commissionRate: 0.25m),

                ["tranbinh@edulms.com"] = await EnsureUserAsync(
                    userManager,
                    email: "tranbinh@edulms.com",
                    fullName: "Ky su Tran Binh",
                    password: "Instructor@123",
                    role: "Instructor",
                    bio: "Truong phong Ky thuat Phan mem",
                    bankName: "BIDV",
                    bankAccountNumber: "111122223333",
                    bankAccountName: "TRAN BINH",
                    commissionRate: 0.30m),

                ["anna@edulms.com"] = await EnsureUserAsync(
                    userManager,
                    email: "anna@edulms.com",
                    fullName: "Giang vien Anna",
                    password: "Instructor@123",
                    role: "Instructor",
                    bio: "Chuyen gia Marketing So",
                    bankName: "Techcombank",
                    bankAccountNumber: "222233334444",
                    bankAccountName: "ANNA",
                    commissionRate: 0.28m)
            };

            var learners = new Dictionary<string, ApplicationUser>(StringComparer.OrdinalIgnoreCase)
            {
                ["alex@example.com"] = await EnsureUserAsync(
                    userManager,
                    email: "alex@example.com",
                    fullName: "Alex Learner",
                    password: "Learner@123",
                    role: "Learner"),

                ["maria@example.com"] = await EnsureUserAsync(
                    userManager,
                    email: "maria@example.com",
                    fullName: "Maria Doe",
                    password: "Learner@123",
                    role: "Learner"),

                ["john@example.com"] = await EnsureUserAsync(
                    userManager,
                    email: "john@example.com",
                    fullName: "John Smith",
                    password: "Learner@123",
                    role: "Learner"),

                ["linda@example.com"] = await EnsureUserAsync(
                    userManager,
                    email: "linda@example.com",
                    fullName: "Linda May",
                    password: "Learner@123",
                    role: "Learner")
            };

            return new SeedUsers
            {
                Admin = admin,
                Instructors = instructors,
                Learners = learners
            };
        }

        private static async Task SeedCourseCategoriesAsync(ApplicationDbContext context)
        {
            var definitions = new[]
            {
                new CategorySeed("lap-trinh", "Lap trinh", 1),
                new CategorySeed("thiet-ke", "Thiet ke", 2),
                new CategorySeed("data-science", "Data Science", 3),
                new CategorySeed("marketing", "Marketing", 4),
                new CategorySeed("kinh-doanh", "Kinh doanh", 5)
            };

            foreach (var def in definitions)
            {
                var category = await context.CourseCategories.FirstOrDefaultAsync(c => c.Slug == def.Slug);
                if (category == null)
                {
                    context.CourseCategories.Add(new CourseCategory
                    {
                        Name = def.Name,
                        Slug = def.Slug,
                        DisplayOrder = def.DisplayOrder
                    });
                    continue;
                }

                category.Name = def.Name;
                category.DisplayOrder = def.DisplayOrder;
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedNotificationRulesAsync(ApplicationDbContext context)
        {
            var definitions = new[]
            {
                new NotificationRuleSeed(
                    "GradingDone",
                    "Cham diem xong",
                    "Thong bao cho hoc vien khi giang vien cham bai xong.",
                    true,
                    NotificationChannel.Both,
                    "Bai tap [Task_Name] da duoc cham diem. Diem cua ban: [Score]"),

                new NotificationRuleSeed(
                    "DeadlineReminder",
                    "Sap den han nop bai",
                    "Gui loi nhac truoc 24h deadline.",
                    true,
                    NotificationChannel.Both,
                    "Nhac nho: Ban co bai tap [Task_Name] can nop vao ngay mai."),

                new NotificationRuleSeed(
                    "NewCoursePublished",
                    "Khoa hoc moi xuat ban",
                    "Thong bao cho hoc vien khi co khoa hoc moi.",
                    false,
                    NotificationChannel.InApp,
                    "Khoa hoc moi: [Course_Name] da duoc xuat ban. Dang ky ngay!"),

                new NotificationRuleSeed(
                    "PaymentSuccess",
                    "Thanh toan thanh cong",
                    "Thong bao sau khi hoc vien thanh toan khoa hoc.",
                    true,
                    NotificationChannel.InApp,
                    "Thanh toan cho khoa hoc [Course_Name] da thanh cong.")
            };

            foreach (var def in definitions)
            {
                var rule = await context.NotificationRules.FirstOrDefaultAsync(r => r.EventType == def.EventType);
                if (rule == null)
                {
                    context.NotificationRules.Add(new NotificationRule
                    {
                        EventType = def.EventType,
                        DisplayName = def.DisplayName,
                        Description = def.Description,
                        IsEnabled = def.IsEnabled,
                        Channel = def.Channel,
                        TemplateBody = def.TemplateBody
                    });
                    continue;
                }

                rule.DisplayName = def.DisplayName;
                rule.Description = def.Description;
                rule.IsEnabled = def.IsEnabled;
                rule.Channel = def.Channel;
                rule.TemplateBody = def.TemplateBody;
                rule.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        private static async Task<Dictionary<string, Course>> SeedCoursesAsync(
            ApplicationDbContext context,
            Dictionary<string, ApplicationUser> instructors)
        {
            var categories = await context.CourseCategories
                .ToDictionaryAsync(c => c.Slug, StringComparer.OrdinalIgnoreCase);

            var now = DateTime.UtcNow;
            var definitions = new[]
            {
                new CourseSeed(
                    "FE201",
                    "Lap trinh Web Frontend Fullstack voi React va Node.js",
                    "Khoa hoc React.js va Node.js tu co ban den trien khai du an.",
                    "/uploads/courses/54f0d8d7-91dc-423a-a5b0-10600e32b3f6.png",
                    "lap-trinh",
                    "lehoang@edulms.com",
                    CourseLevel.Beginner,
                    48,
                    CourseType.Paid,
                    899000,
                    699000,
                    CourseStatus.Published,
                    4.8m,
                    1250,
                    45,
                    30),

                new CourseSeed(
                    "DES101",
                    "UI UX Design Masterclass",
                    "Huong dan xay dung design system, token, component va guideline.",
                    "/uploads/courses/2d597262-7992-43da-9f8c-97d18f118acf.jpeg",
                    "thiet-ke",
                    "minhtu@edulms.com",
                    CourseLevel.Advanced,
                    32,
                    CourseType.Paid,
                    299000,
                    null,
                    CourseStatus.Published,
                    4.9m,
                    840,
                    35,
                    20),

                new CourseSeed(
                    "DS301",
                    "Phan tich du lieu kinh doanh voi Python va SQL",
                    "Ung dung SQL va Python de phan tich du lieu doanh nghiep.",
                    "/uploads/courses/381ee305-12f9-44b0-95bc-046b952c2333.jpg",
                    "data-science",
                    "tranbinh@edulms.com",
                    CourseLevel.Intermediate,
                    40,
                    CourseType.Paid,
                    499000,
                    429000,
                    CourseStatus.Published,
                    4.7m,
                    560,
                    25,
                    15),

                new CourseSeed(
                    "MKT401",
                    "Digital Marketing Thuc chien 2026",
                    "Chien luoc marketing so toan dien cho doanh nghiep.",
                    "/uploads/courses/0d7d72b9-52a8-4fe0-84d7-29689a90fdda.jpg",
                    "marketing",
                    "anna@edulms.com",
                    CourseLevel.Beginner,
                    24,
                    CourseType.Paid,
                    199000,
                    null,
                    CourseStatus.Published,
                    4.6m,
                    320,
                    20,
                    10),

                new CourseSeed(
                    "REACT101",
                    "Lap trinh React.js Co ban",
                    "Khoa hoc nen tang React.js tu co ban den xay dung component.",
                    "/uploads/courses/d914041b-bb53-4bb7-8437-8ead5f800561.jpeg",
                    "lap-trinh",
                    "lehoang@edulms.com",
                    CourseLevel.Beginner,
                    20,
                    CourseType.Paid,
                    899000,
                    null,
                    CourseStatus.Published,
                    4.5m,
                    124,
                    70,
                    60)
            };

            foreach (var def in definitions)
            {
                if (!categories.TryGetValue(def.CategorySlug, out var category))
                {
                    continue;
                }

                if (!instructors.TryGetValue(def.InstructorEmail, out var instructor))
                {
                    continue;
                }

                var course = await context.Courses.FirstOrDefaultAsync(c => c.Code == def.Code);
                if (course == null)
                {
                    course = new Course
                    {
                        Code = def.Code,
                        CreatedAt = now.AddDays(-def.CreatedDaysAgo)
                    };
                    context.Courses.Add(course);
                }

                course.Name = def.Name;
                course.Description = def.Description;
                course.CoverImageUrl = def.CoverImageUrl;
                course.CategoryId = category.Id;
                course.InstructorId = instructor.Id;
                course.Level = def.Level;
                course.DurationHours = def.DurationHours;
                course.CourseType = def.CourseType;
                course.OriginalPrice = def.OriginalPrice;
                course.PromoPrice = def.PromoPrice;
                course.Status = def.Status;
                course.AverageRating = def.AverageRating;
                course.RatingCount = def.RatingCount;
                course.PublishedAt = now.AddDays(-def.PublishedDaysAgo);
                course.UpdatedAt = now;
            }

            await context.SaveChangesAsync();

            return await context.Courses
                .Where(c => definitions.Select(d => d.Code).Contains(c.Code))
                .ToDictionaryAsync(c => c.Code, StringComparer.OrdinalIgnoreCase);
        }

        private static async Task SeedCourseContentAsync(ApplicationDbContext context, Dictionary<string, Course> courses)
        {
            var now = DateTime.UtcNow;

            if (courses.TryGetValue("REACT101", out var reactCourse))
            {
                var ch1 = await EnsureChapterAsync(context, reactCourse.Id, "Gioi thieu ve React Hooks", 1);
                var ch2 = await EnsureChapterAsync(context, reactCourse.Id, "Quan ly state voi Context API", 2);
                var ch3 = await EnsureChapterAsync(context, reactCourse.Id, "Toi uu hieu suat component", 3);
                var ch4 = await EnsureChapterAsync(context, reactCourse.Id, "Bai tap thuc hanh cuoi khoa", 4);

                var l11 = await EnsureLessonAsync(context, ch1.Id, "useState va useEffect co ban", 45, 1, "Tim hieu ve useState va useEffect hooks.", "https://www.youtube.com/watch?v=O6P86uwfdR0");
                var l12 = await EnsureLessonAsync(context, ch1.Id, "useContext va Custom Hooks", 50, 2, "Tao custom hooks de tai su dung logic.", "https://www.youtube.com/watch?v=35lXWvCuM8o");
                var l21 = await EnsureLessonAsync(context, ch2.Id, "Context API va Global State", 40, 1, "Quan ly state toan cuc voi Context API.", "https://www.youtube.com/watch?v=5LrDIWkK_Bc");
                var l31 = await EnsureLessonAsync(context, ch3.Id, "React.memo va useMemo", 35, 1, "Toi uu render voi memo va useMemo.", "https://www.youtube.com/watch?v=MfB1Zwru0Q8");

                await EnsureAssignmentAsync(
                    context,
                    chapterId: ch1.Id,
                    lessonId: l12.Id,
                    title: "Bai tap 1: Component Lifecycle",
                    description: "Quiz ve lifecycle cua React components.",
                    maxScore: 10,
                    displayOrder: 1,
                    dueDate: now.AddDays(3));

                await EnsureAssignmentAsync(
                    context,
                    chapterId: ch2.Id,
                    lessonId: l21.Id,
                    title: "Bai tap 2: Custom Hooks",
                    description: "Tao custom hook cho form validation.",
                    maxScore: 10,
                    displayOrder: 1,
                    dueDate: now.AddDays(5));

                await EnsureAssignmentAsync(
                    context,
                    chapterId: ch3.Id,
                    lessonId: l31.Id,
                    title: "Bai tap 3: Router",
                    description: "Xay dung routing cho ung dung.",
                    maxScore: 10,
                    displayOrder: 1,
                    dueDate: now.AddDays(7));

                await EnsureAssignmentAsync(
                    context,
                    chapterId: ch4.Id,
                    lessonId: null,
                    title: "Do an cuoi ky",
                    description: "Xay dung ung dung quan ly chi tieu ca nhan.",
                    maxScore: 10,
                    displayOrder: 1,
                    dueDate: now.AddDays(10));
            }

            if (courses.TryGetValue("FE201", out var feCourse))
            {
                var ch1 = await EnsureChapterAsync(context, feCourse.Id, "Nen tang Frontend hien dai", 1);
                var ch2 = await EnsureChapterAsync(context, feCourse.Id, "Backend voi Node.js va Express", 2);

                var l11 = await EnsureLessonAsync(context, ch1.Id, "ES6 va TypeScript co ban", 40, 1, "On tap ES6, module va typing voi TypeScript.", "https://www.youtube.com/watch?v=ahCwqrYpIuM");
                var l12 = await EnsureLessonAsync(context, ch1.Id, "React component patterns", 50, 2, "Mau thiet ke component de tai su dung.", "https://www.youtube.com/watch?v=w7ejDZ8SWv8");
                var l21 = await EnsureLessonAsync(context, ch2.Id, "Thiet ke REST API", 45, 1, "Xay dung API chuan REST voi Express.", "https://www.youtube.com/watch?v=l8WPWK9mS5M");
                var l22 = await EnsureLessonAsync(context, ch2.Id, "Ket noi MySQL voi Prisma", 50, 2, "Truy van du lieu va migration voi Prisma.", "https://www.youtube.com/watch?v=RebA5J-rlwg");

                await EnsureAssignmentAsync(
                    context,
                    chapterId: ch1.Id,
                    lessonId: l12.Id,
                    title: "FE201 - Bai tap Landing Page",
                    description: "Xay dung landing page responsive theo mockup.",
                    maxScore: 10,
                    displayOrder: 1,
                    dueDate: now.AddDays(4));

                await EnsureAssignmentAsync(
                    context,
                    chapterId: ch2.Id,
                    lessonId: l22.Id,
                    title: "FE201 - Bai tap CRUD API",
                    description: "Xay dung API CRUD cho module khoa hoc.",
                    maxScore: 10,
                    displayOrder: 1,
                    dueDate: now.AddDays(8));
            }

            if (courses.TryGetValue("DES101", out var desCourse))
            {
                var ch1 = await EnsureChapterAsync(context, desCourse.Id, "Design Thinking va nghien cuu nguoi dung", 1);
                var l11 = await EnsureLessonAsync(context, ch1.Id, "Persona va User Journey", 35, 1, "Phan tich hanh vi nguoi dung.", "https://www.youtube.com/watch?v=9y8fdJ1CjJg");
                var l12 = await EnsureLessonAsync(context, ch1.Id, "Wireframe va Prototype", 45, 2, "Tao wireframe va prototype tren Figma.", "https://www.youtube.com/watch?v=FTFaQWZBqQ8");

                await EnsureAssignmentAsync(
                    context,
                    chapterId: ch1.Id,
                    lessonId: l12.Id,
                    title: "DES101 - Bai tap Prototype",
                    description: "Thiet ke prototype man hinh checkout.",
                    maxScore: 10,
                    displayOrder: 1,
                    dueDate: now.AddDays(6));
            }

            if (courses.TryGetValue("DS301", out var dsCourse))
            {
                var ch1 = await EnsureChapterAsync(context, dsCourse.Id, "SQL va truc quan hoa du lieu", 1);
                var l11 = await EnsureLessonAsync(context, ch1.Id, "SQL tong hop cho doanh nghiep", 40, 1, "Tong hop du lieu doanh thu va hoc vien.", "https://www.youtube.com/watch?v=HXV3zeQKqGY");
                var l12 = await EnsureLessonAsync(context, ch1.Id, "Dashboard voi Python", 50, 2, "Dung pandas va matplotlib cho bao cao.", "https://www.youtube.com/watch?v=vmEHCJofslg");

                await EnsureAssignmentAsync(
                    context,
                    chapterId: ch1.Id,
                    lessonId: l12.Id,
                    title: "DS301 - Bai tap Phan tich du lieu",
                    description: "Phan tich du lieu ban hang va de xuat insight.",
                    maxScore: 10,
                    displayOrder: 1,
                    dueDate: now.AddDays(9));
            }

            if (courses.TryGetValue("MKT401", out var mktCourse))
            {
                var ch1 = await EnsureChapterAsync(context, mktCourse.Id, "Chien dich Digital Marketing", 1);
                var l11 = await EnsureLessonAsync(context, ch1.Id, "Facebook Ads can ban", 30, 1, "Cau truc chien dich quang cao Facebook.", "https://www.youtube.com/watch?v=KmYfY8h5f5E");
                var l12 = await EnsureLessonAsync(context, ch1.Id, "Google Ads va toi uu ngan sach", 40, 2, "Toi uu CPC, CPA va ROAS.", "https://www.youtube.com/watch?v=Ks4c_JQkQY0");

                await EnsureAssignmentAsync(
                    context,
                    chapterId: ch1.Id,
                    lessonId: l12.Id,
                    title: "MKT401 - Bai tap Ke hoach quang cao",
                    description: "Lap ke hoach quang cao 30 ngay cho san pham moi.",
                    maxScore: 10,
                    displayOrder: 1,
                    dueDate: now.AddDays(5));
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedEnrollmentsAsync(
            ApplicationDbContext context,
            Dictionary<string, ApplicationUser> learners,
            Dictionary<string, Course> courses)
        {
            var now = DateTime.UtcNow;

            await EnsureEnrollmentAsync(
                context,
                learners["alex@example.com"].Id,
                courses["REACT101"].Id,
                progressPercent: 45,
                status: EnrollmentStatus.Active,
                enrolledAt: now.AddDays(-20),
                completedAt: null);

            await EnsureEnrollmentAsync(
                context,
                learners["alex@example.com"].Id,
                courses["DES101"].Id,
                progressPercent: 10,
                status: EnrollmentStatus.Active,
                enrolledAt: now.AddDays(-15),
                completedAt: null);

            await EnsureEnrollmentAsync(
                context,
                learners["alex@example.com"].Id,
                courses["FE201"].Id,
                progressPercent: 65,
                status: EnrollmentStatus.Active,
                enrolledAt: now.AddDays(-8),
                completedAt: null);

            await EnsureEnrollmentAsync(
                context,
                learners["maria@example.com"].Id,
                courses["FE201"].Id,
                progressPercent: 30,
                status: EnrollmentStatus.Active,
                enrolledAt: now.AddDays(-6),
                completedAt: null);

            await EnsureEnrollmentAsync(
                context,
                learners["maria@example.com"].Id,
                courses["DS301"].Id,
                progressPercent: 12,
                status: EnrollmentStatus.Active,
                enrolledAt: now.AddDays(-4),
                completedAt: null);

            await EnsureEnrollmentAsync(
                context,
                learners["john@example.com"].Id,
                courses["REACT101"].Id,
                progressPercent: 8,
                status: EnrollmentStatus.Active,
                enrolledAt: now.AddDays(-3),
                completedAt: null);

            await EnsureEnrollmentAsync(
                context,
                learners["john@example.com"].Id,
                courses["MKT401"].Id,
                progressPercent: 100,
                status: EnrollmentStatus.Completed,
                enrolledAt: now.AddDays(-25),
                completedAt: now.AddDays(-2));

            await EnsureEnrollmentAsync(
                context,
                learners["linda@example.com"].Id,
                courses["DES101"].Id,
                progressPercent: 55,
                status: EnrollmentStatus.Active,
                enrolledAt: now.AddDays(-10),
                completedAt: null);

            await EnsureEnrollmentAsync(
                context,
                learners["linda@example.com"].Id,
                courses["DS301"].Id,
                progressPercent: 0,
                status: EnrollmentStatus.Dropped,
                enrolledAt: now.AddDays(-18),
                completedAt: null);

            await context.SaveChangesAsync();
        }

        private static async Task SeedLessonResourcesAsync(ApplicationDbContext context, Dictionary<string, Course> courses)
        {
            var lessons = await context.Lessons
                .Include(l => l.Chapter)
                    .ThenInclude(ch => ch.Course)
                .ToListAsync();

            var lessonMap = lessons.ToDictionary(
                l => BuildLessonKey(l.Chapter.Course.Code, l.Title),
                StringComparer.OrdinalIgnoreCase);

            await EnsureLessonResourceAsync(
                context,
                lessonMap,
                courseCode: "REACT101",
                lessonTitle: "useState va useEffect co ban",
                fileName: "ReactHooks_Slides.pdf",
                fileUrl: "/files/reacthooks-slides.pdf",
                fileType: "pdf",
                fileSizeBytes: 2_400_000);

            await EnsureLessonResourceAsync(
                context,
                lessonMap,
                courseCode: "FE201",
                lessonTitle: "ES6 va TypeScript co ban",
                fileName: "FE201_ES6_Checklist.pdf",
                fileUrl: "/files/fe201-es6-checklist.pdf",
                fileType: "pdf",
                fileSizeBytes: 1_300_000);

            await EnsureLessonResourceAsync(
                context,
                lessonMap,
                courseCode: "DES101",
                lessonTitle: "Wireframe va Prototype",
                fileName: "DES101_Prototype_Kit.zip",
                fileUrl: "/files/des101-prototype-kit.zip",
                fileType: "zip",
                fileSizeBytes: 4_500_000);

            await EnsureLessonResourceAsync(
                context,
                lessonMap,
                courseCode: "DS301",
                lessonTitle: "Dashboard voi Python",
                fileName: "DS301_Dataset.csv",
                fileUrl: "/files/ds301-dataset.csv",
                fileType: "csv",
                fileSizeBytes: 560_000);

            await EnsureLessonResourceAsync(
                context,
                lessonMap,
                courseCode: "MKT401",
                lessonTitle: "Google Ads va toi uu ngan sach",
                fileName: "MKT401_Budget_Template.xlsx",
                fileUrl: "/files/mkt401-budget-template.xlsx",
                fileType: "xlsx",
                fileSizeBytes: 890_000);

            await context.SaveChangesAsync();
        }

        private static async Task SeedLessonProgressAsync(
            ApplicationDbContext context,
            Dictionary<string, ApplicationUser> learners,
            Dictionary<string, Course> courses)
        {
            var lessons = await context.Lessons
                .Include(l => l.Chapter)
                    .ThenInclude(ch => ch.Course)
                .ToListAsync();

            var lessonMap = lessons.ToDictionary(
                l => BuildLessonKey(l.Chapter.Course.Code, l.Title),
                StringComparer.OrdinalIgnoreCase);

            var now = DateTime.UtcNow;

            await EnsureLessonProgressAsync(
                context,
                learners["alex@example.com"].Id,
                lessonMap,
                courseCode: "REACT101",
                lessonTitle: "useState va useEffect co ban",
                status: LessonStatus.Completed,
                completedAt: now.AddDays(-6));

            await EnsureLessonProgressAsync(
                context,
                learners["alex@example.com"].Id,
                lessonMap,
                courseCode: "REACT101",
                lessonTitle: "useContext va Custom Hooks",
                status: LessonStatus.InProgress,
                completedAt: null);

            await EnsureLessonProgressAsync(
                context,
                learners["alex@example.com"].Id,
                lessonMap,
                courseCode: "FE201",
                lessonTitle: "ES6 va TypeScript co ban",
                status: LessonStatus.Completed,
                completedAt: now.AddDays(-4));

            await EnsureLessonProgressAsync(
                context,
                learners["alex@example.com"].Id,
                lessonMap,
                courseCode: "FE201",
                lessonTitle: "React component patterns",
                status: LessonStatus.Completed,
                completedAt: now.AddDays(-3));

            await EnsureLessonProgressAsync(
                context,
                learners["alex@example.com"].Id,
                lessonMap,
                courseCode: "FE201",
                lessonTitle: "Thiet ke REST API",
                status: LessonStatus.InProgress,
                completedAt: null);

            await EnsureLessonProgressAsync(
                context,
                learners["maria@example.com"].Id,
                lessonMap,
                courseCode: "FE201",
                lessonTitle: "ES6 va TypeScript co ban",
                status: LessonStatus.Completed,
                completedAt: now.AddDays(-2));

            await EnsureLessonProgressAsync(
                context,
                learners["maria@example.com"].Id,
                lessonMap,
                courseCode: "DS301",
                lessonTitle: "SQL tong hop cho doanh nghiep",
                status: LessonStatus.InProgress,
                completedAt: null);

            await EnsureLessonProgressAsync(
                context,
                learners["john@example.com"].Id,
                lessonMap,
                courseCode: "REACT101",
                lessonTitle: "useState va useEffect co ban",
                status: LessonStatus.Completed,
                completedAt: now.AddDays(-1));

            await context.SaveChangesAsync();
        }

        private static async Task SeedSubmissionsAndGradesAsync(
            ApplicationDbContext context,
            Dictionary<string, ApplicationUser> learners,
            Dictionary<string, ApplicationUser> instructors,
            Dictionary<string, Course> courses)
        {
            var assignments = await context.Assignments
                .Include(a => a.Chapter)
                    .ThenInclude(ch => ch.Course)
                .ToListAsync();

            var assignmentMap = assignments.ToDictionary(
                a => BuildAssignmentKey(a.Chapter.Course.Code, a.Title),
                StringComparer.OrdinalIgnoreCase);

            var now = DateTime.UtcNow;

            var alexA1 = await EnsureSubmissionAsync(
                context,
                assignmentMap,
                courseCode: "REACT101",
                assignmentTitle: "Bai tap 1: Component Lifecycle",
                userId: learners["alex@example.com"].Id,
                content: "Bai lam cua Alex ve component lifecycle.",
                status: SubmissionStatus.Graded,
                submittedAt: now.AddDays(-5),
                createdAt: now.AddDays(-5));

            await EnsureGradeAsync(
                context,
                alexA1,
                gradedById: courses["REACT101"].InstructorId,
                score: 8.5m,
                passStatus: PassStatus.Pass,
                comment: "Bai lam tot, can bo sung test case cho edge case.",
                gradedAt: now.AddDays(-4));

            await EnsureSubmissionAsync(
                context,
                assignmentMap,
                courseCode: "REACT101",
                assignmentTitle: "Bai tap 2: Custom Hooks",
                userId: learners["alex@example.com"].Id,
                content: "Da tao custom hook validate form.",
                status: SubmissionStatus.Submitted,
                submittedAt: now.AddDays(-2),
                createdAt: now.AddDays(-2));

            var mariaA1 = await EnsureSubmissionAsync(
                context,
                assignmentMap,
                courseCode: "FE201",
                assignmentTitle: "FE201 - Bai tap Landing Page",
                userId: learners["maria@example.com"].Id,
                content: "Landing page responsive theo Figma.",
                status: SubmissionStatus.Graded,
                submittedAt: now.AddDays(-3),
                createdAt: now.AddDays(-3));

            await EnsureGradeAsync(
                context,
                mariaA1,
                gradedById: instructors["lehoang@edulms.com"].Id,
                score: 9.0m,
                passStatus: PassStatus.Pass,
                comment: "UI on, bo cuc ro rang, code de doc.",
                gradedAt: now.AddDays(-2));

            var johnA1 = await EnsureSubmissionAsync(
                context,
                assignmentMap,
                courseCode: "MKT401",
                assignmentTitle: "MKT401 - Bai tap Ke hoach quang cao",
                userId: learners["john@example.com"].Id,
                content: "Ke hoach quang cao 30 ngay.",
                status: SubmissionStatus.Graded,
                submittedAt: now.AddDays(-6),
                createdAt: now.AddDays(-6));

            await EnsureGradeAsync(
                context,
                johnA1,
                gradedById: instructors["anna@edulms.com"].Id,
                score: 6.0m,
                passStatus: PassStatus.Fail,
                comment: "Can bo sung KPI va chia ngan sach theo kenh.",
                gradedAt: now.AddDays(-5));

            await EnsureSubmissionAsync(
                context,
                assignmentMap,
                courseCode: "DS301",
                assignmentTitle: "DS301 - Bai tap Phan tich du lieu",
                userId: learners["linda@example.com"].Id,
                content: "Ban nhap ban dau, chua hoan thien.",
                status: SubmissionStatus.Draft,
                submittedAt: null,
                createdAt: now.AddDays(-1));

            await context.SaveChangesAsync();
        }

        private static async Task SeedPaymentsAndEarningsAsync(
            ApplicationDbContext context,
            ApplicationUser admin,
            Dictionary<string, ApplicationUser> instructors,
            Dictionary<string, ApplicationUser> learners,
            Dictionary<string, Course> courses)
        {
            var now = DateTime.UtcNow;

            var p1 = await EnsurePaymentAsync(
                context,
                transactionCode: "SEEDPAY-1001",
                userId: learners["alex@example.com"].Id,
                courseId: courses["REACT101"].Id,
                amount: 899000,
                method: PaymentMethod.QRCode,
                status: PaymentStatus.Completed,
                createdAt: now.AddDays(-20),
                completedAt: now.AddDays(-20).AddMinutes(10));

            var p2 = await EnsurePaymentAsync(
                context,
                transactionCode: "SEEDPAY-1002",
                userId: learners["maria@example.com"].Id,
                courseId: courses["FE201"].Id,
                amount: 699000,
                method: PaymentMethod.BankTransfer,
                status: PaymentStatus.Completed,
                createdAt: now.AddDays(-7),
                completedAt: now.AddDays(-7).AddMinutes(5));

            await EnsurePaymentAsync(
                context,
                transactionCode: "SEEDPAY-1003",
                userId: learners["john@example.com"].Id,
                courseId: courses["DS301"].Id,
                amount: 429000,
                method: PaymentMethod.QRCode,
                status: PaymentStatus.Pending,
                createdAt: now.AddDays(-1),
                completedAt: null);

            await EnsurePaymentAsync(
                context,
                transactionCode: "SEEDPAY-1004",
                userId: learners["linda@example.com"].Id,
                courseId: courses["MKT401"].Id,
                amount: 199000,
                method: PaymentMethod.BankTransfer,
                status: PaymentStatus.Failed,
                createdAt: now.AddDays(-4),
                completedAt: null);

            await EnsurePaymentAsync(
                context,
                transactionCode: "SEEDPAY-1005",
                userId: learners["alex@example.com"].Id,
                courseId: courses["DES101"].Id,
                amount: 299000,
                method: PaymentMethod.QRCode,
                status: PaymentStatus.Refunded,
                createdAt: now.AddDays(-14),
                completedAt: now.AddDays(-13));

            var payoutNote = "Seed payout 2026-03";
            var payoutBatch = await context.PayoutBatches.FirstOrDefaultAsync(
                b => b.InstructorId == instructors["lehoang@edulms.com"].Id && b.Note == payoutNote);

            if (payoutBatch == null)
            {
                payoutBatch = new PayoutBatch
                {
                    AdminId = admin.Id,
                    InstructorId = instructors["lehoang@edulms.com"].Id,
                    Note = payoutNote,
                    TotalAmount = 0,
                    Status = PayoutBatchStatus.Completed,
                    CreatedAt = now.AddDays(-1),
                    CompletedAt = now.AddHours(-12)
                };
                context.PayoutBatches.Add(payoutBatch);
                await context.SaveChangesAsync();
            }

            var e1 = await EnsureInstructorEarningAsync(
                context,
                payment: p1,
                instructorId: courses["REACT101"].InstructorId,
                status: EarningStatus.Paid,
                createdAt: now.AddDays(-20),
                paidAt: now.AddHours(-12),
                payoutBatchId: payoutBatch.Id);

            var e2 = await EnsureInstructorEarningAsync(
                context,
                payment: p2,
                instructorId: courses["FE201"].InstructorId,
                status: EarningStatus.Pending,
                createdAt: now.AddDays(-7),
                paidAt: null,
                payoutBatchId: null);

            payoutBatch.TotalAmount = new[] { e1, e2 }
                .Where(e => e.PayoutBatchId == payoutBatch.Id)
                .Sum(e => e.NetAmount);

            await context.SaveChangesAsync();
        }

        private static async Task SeedSchedulesAsync(
            ApplicationDbContext context,
            Dictionary<string, ApplicationUser> instructors,
            Dictionary<string, Course> courses)
        {
            var anchor = DateTime.UtcNow.Date.AddDays(1);
            var seeds = new[]
            {
                new ScheduleSeed("FE201", "lehoang@edulms.com", "FE201 Live Session 01", "On tap React va TypeScript", anchor.AddHours(1),  anchor.AddHours(3), "https://zoom.us/j/1000000001"),
                new ScheduleSeed("FE201", "lehoang@edulms.com", "FE201 Live Session 02", "Component patterns va state management", anchor.AddDays(2).AddHours(6), anchor.AddDays(2).AddHours(8), "https://zoom.us/j/1000000001"),
                new ScheduleSeed("FE201", "lehoang@edulms.com", "FE201 Live Session 03", "REST API + Prisma", anchor.AddDays(6).AddHours(12), anchor.AddDays(6).AddHours(14), "https://zoom.us/j/1000000001"),

                new ScheduleSeed("REACT101", "lehoang@edulms.com", "REACT101 Live Session 01", "React hooks can ban", anchor.AddDays(1).AddHours(2),  anchor.AddDays(1).AddHours(4), "https://zoom.us/j/1000000003"),
                new ScheduleSeed("REACT101", "lehoang@edulms.com", "REACT101 Live Session 02", "Context API va custom hooks", anchor.AddDays(4).AddHours(7), anchor.AddDays(4).AddHours(9), "https://zoom.us/j/1000000003"),

                new ScheduleSeed("DS301", "tranbinh@edulms.com", "DS301 Live Session 01", "Thuc hanh SQL tong hop", anchor.AddDays(1).AddHours(6), anchor.AddDays(1).AddHours(8), "https://zoom.us/j/1000000002"),
                new ScheduleSeed("DS301", "tranbinh@edulms.com", "DS301 Live Session 02", "Dashboard voi Python", anchor.AddDays(5).AddHours(2),  anchor.AddDays(5).AddHours(4), "https://zoom.us/j/1000000002"),
                new ScheduleSeed("DS301", "tranbinh@edulms.com", "DS301 Live Session 03", "Case study doanh nghiep", anchor.AddDays(8).AddHours(12), anchor.AddDays(8).AddHours(14), "https://zoom.us/j/1000000002"),

                new ScheduleSeed("DES101", "minhtu@edulms.com", "DES101 Workshop 01", "Wireframe va user flow", anchor.AddDays(2).AddHours(2),  anchor.AddDays(2).AddHours(4), "https://zoom.us/j/1000000004"),
                new ScheduleSeed("DES101", "minhtu@edulms.com", "DES101 Workshop 02", "Prototype testing", anchor.AddDays(7).AddHours(7), anchor.AddDays(7).AddHours(10), "https://zoom.us/j/1000000004"),

                new ScheduleSeed("MKT401", "anna@edulms.com", "MKT401 Session 01", "Facebook Ads funnel", anchor.AddDays(3).AddHours(1),  anchor.AddDays(3).AddHours(3), "https://zoom.us/j/1000000005"),
                new ScheduleSeed("MKT401", "anna@edulms.com", "MKT401 Session 02", "Google Ads toi uu ROAS", anchor.AddDays(9).AddHours(6), anchor.AddDays(9).AddHours(8), "https://zoom.us/j/1000000005")
            };

            foreach (var seed in seeds)
            {
                await EnsureScheduleAsync(
                    context,
                    courseId: courses[seed.CourseCode].Id,
                    instructorId: instructors[seed.InstructorEmail].Id,
                    title: seed.Title,
                    description: seed.Description,
                    startTime: seed.StartTime,
                    endTime: seed.EndTime,
                    zoomLink: seed.ZoomLink);
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedCalendarEventsAsync(
            ApplicationDbContext context,
            Dictionary<string, ApplicationUser> instructors,
            Dictionary<string, ApplicationUser> learners)
        {
            var anchor = DateTime.UtcNow.Date.AddDays(1);

            await EnsureCalendarEventAsync(
                context,
                userId: learners["alex@example.com"].Id,
                title: "Nop bai FE201",
                description: "Hoan thanh bai tap CRUD API truoc han.",
                startTime: anchor.AddDays(3).AddHours(2),
                endTime: anchor.AddDays(3).AddHours(3),
                eventType: EventType.Assignment,
                relatedEntityId: null);

            await EnsureCalendarEventAsync(
                context,
                userId: learners["alex@example.com"].Id,
                title: "On React truoc buoi hoc",
                description: "Xem lai useEffect va useMemo.",
                startTime: anchor.AddDays(1).AddHours(1),
                endTime: anchor.AddDays(1).AddHours(2),
                eventType: EventType.Personal,
                relatedEntityId: null);

            await EnsureCalendarEventAsync(
                context,
                userId: learners["maria@example.com"].Id,
                title: "On tap DS301",
                description: "On SQL va pandas truoc buoi hoc.",
                startTime: anchor.AddDays(1).AddHours(5),
                endTime: anchor.AddDays(1).AddHours(6),
                eventType: EventType.Personal,
                relatedEntityId: null);

            await EnsureCalendarEventAsync(
                context,
                userId: learners["john@example.com"].Id,
                title: "Nop bai MKT401",
                description: "Hoan thien ke hoach ads va KPI.",
                startTime: anchor.AddDays(2).AddHours(13),
                endTime: anchor.AddDays(2).AddHours(14),
                eventType: EventType.Assignment,
                relatedEntityId: null);

            await EnsureCalendarEventAsync(
                context,
                userId: learners["linda@example.com"].Id,
                title: "Chuan bi prototype DES101",
                description: "Chuan bi khung wireframe truoc workshop.",
                startTime: anchor.AddDays(6).AddHours(9),
                endTime: anchor.AddDays(6).AddHours(10),
                eventType: EventType.Personal,
                relatedEntityId: null);

            await EnsureCalendarEventAsync(
                context,
                userId: instructors["lehoang@edulms.com"].Id,
                title: "Chuan bi slide FE201",
                description: "Cap nhat vi du cho buoi live session.",
                startTime: anchor.AddHours(0),
                endTime: anchor.AddHours(1),
                eventType: EventType.Personal,
                relatedEntityId: null);

            await EnsureCalendarEventAsync(
                context,
                userId: instructors["minhtu@edulms.com"].Id,
                title: "Review bai nop DES101",
                description: "Danh gia bai prototype cua hoc vien.",
                startTime: anchor.AddDays(2).AddHours(11),
                endTime: anchor.AddDays(2).AddHours(12),
                eventType: EventType.Personal,
                relatedEntityId: null);

            await EnsureCalendarEventAsync(
                context,
                userId: instructors["tranbinh@edulms.com"].Id,
                title: "Cap nhat dataset DS301",
                description: "Bo sung du lieu thuc te cho buoi case study.",
                startTime: anchor.AddDays(4).AddHours(8),
                endTime: anchor.AddDays(4).AddHours(9),
                eventType: EventType.Personal,
                relatedEntityId: null);

            await context.SaveChangesAsync();
        }

        private static async Task SeedNotificationsAsync(
            ApplicationDbContext context,
            ApplicationUser admin,
            Dictionary<string, ApplicationUser> instructors,
            Dictionary<string, ApplicationUser> learners,
            Dictionary<string, Course> courses)
        {
            var now = DateTime.UtcNow;

            await EnsureNotificationAsync(
                context,
                userId: learners["alex@example.com"].Id,
                title: "Ban co diem moi cho REACT101",
                message: "Bai tap Component Lifecycle da duoc cham: 8.5/10.",
                type: "grade",
                channel: NotificationChannel.InApp,
                isRead: false,
                createdAt: now.AddDays(-4),
                senderInstructorId: instructors["lehoang@edulms.com"].Id,
                senderAdminId: null,
                courseId: courses["REACT101"].Id);

            await EnsureNotificationAsync(
                context,
                userId: learners["john@example.com"].Id,
                title: "Ban co diem moi cho MKT401",
                message: "Bai tap Ke hoach quang cao da duoc cham: 6.0/10.",
                type: "grade",
                channel: NotificationChannel.InApp,
                isRead: true,
                createdAt: now.AddDays(-5),
                senderInstructorId: instructors["anna@edulms.com"].Id,
                senderAdminId: null,
                courseId: courses["MKT401"].Id);

            // Instructor broadcast to FE201 learners (for sent-history grouping).
            foreach (var learnerEmail in new[] { "alex@example.com", "maria@example.com" })
            {
                await EnsureNotificationAsync(
                    context,
                    userId: learners[learnerEmail].Id,
                    title: "Lich hoc FE201 tuan nay",
                    message: "Buoi live session dien ra vao ngay mai. Vui long vao dung gio.",
                    type: "instructor_announcement",
                    channel: NotificationChannel.InApp,
                    isRead: false,
                    createdAt: now.AddDays(-1).AddHours(-2),
                    senderInstructorId: instructors["lehoang@edulms.com"].Id,
                    senderAdminId: null,
                    courseId: courses["FE201"].Id);
            }

            // Admin broadcast to learners.
            foreach (var learner in learners.Values)
            {
                await EnsureNotificationAsync(
                    context,
                    userId: learner.Id,
                    title: "Thong bao he thong thang 04/2026",
                    message: "He thong bao tri nhe vao chu nhat 23:00 - 23:30.",
                    type: "admin_broadcast_learner",
                    channel: NotificationChannel.InApp,
                    isRead: false,
                    createdAt: now.AddHours(-10),
                    senderInstructorId: null,
                    senderAdminId: admin.Id,
                    courseId: null);
            }

            await EnsureNotificationAsync(
                context,
                userId: instructors["lehoang@edulms.com"].Id,
                title: "Thong bao he thong thang 04/2026",
                message: "He thong bao tri nhe vao chu nhat 23:00 - 23:30.",
                type: "admin_broadcast_instructor",
                channel: NotificationChannel.InApp,
                isRead: false,
                createdAt: now.AddHours(-10),
                senderInstructorId: null,
                senderAdminId: admin.Id,
                courseId: null);

            await context.SaveChangesAsync();
        }

        private static async Task SeedActivityLogsAsync(
            ApplicationDbContext context,
            ApplicationUser admin,
            Dictionary<string, ApplicationUser> instructors,
            Dictionary<string, ApplicationUser> learners)
        {
            var now = DateTime.UtcNow;
            var logs = new[]
            {
                new ActivityLogSeed("Login", "Admin dang nhap he thong", admin.Id, now.AddDays(-1).AddHours(-1)),
                new ActivityLogSeed("CreateCourse", "Giang vien Le Hoang cap nhat noi dung FE201", instructors["lehoang@edulms.com"].Id, now.AddDays(-1)),
                new ActivityLogSeed("Enroll", "Alex dang ky khoa hoc FE201", learners["alex@example.com"].Id, now.AddDays(-8)),
                new ActivityLogSeed("Enroll", "Maria dang ky khoa hoc DS301", learners["maria@example.com"].Id, now.AddDays(-4)),
                new ActivityLogSeed("CompleteLesson", "Alex hoan thanh bai hoc ES6 va TypeScript", learners["alex@example.com"].Id, now.AddDays(-4)),
                new ActivityLogSeed("SubmitAssignment", "Maria nop bai FE201 - Landing Page", learners["maria@example.com"].Id, now.AddDays(-3)),
                new ActivityLogSeed("Complete", "John hoan thanh khoa hoc MKT401", learners["john@example.com"].Id, now.AddDays(-2)),
                new ActivityLogSeed("Login", "Giang vien Anna dang nhap he thong", instructors["anna@edulms.com"].Id, now.AddHours(-18)),
                new ActivityLogSeed("CreateNotification", "Admin gui thong bao bao tri he thong", admin.Id, now.AddHours(-10)),
                new ActivityLogSeed("CreatePayout", "Admin tao dot thanh toan cho giang vien", admin.Id, now.AddHours(-8)),
                new ActivityLogSeed("Enroll", "John dang ky khoa hoc REACT101", learners["john@example.com"].Id, now.AddDays(-3)),
                new ActivityLogSeed("CompleteLesson", "John hoan thanh bai hoc useState va useEffect", learners["john@example.com"].Id, now.AddDays(-1))
            };

            foreach (var log in logs)
            {
                var exists = await context.ActivityLogs.AnyAsync(a =>
                    a.Action == log.Action &&
                    a.Description == log.Description);

                if (exists)
                {
                    continue;
                }

                context.ActivityLogs.Add(new ActivityLog
                {
                    UserId = log.UserId,
                    Action = log.Action,
                    Description = log.Description,
                    IpAddress = "127.0.0.1",
                    CreatedAt = log.CreatedAt
                });
            }

            await context.SaveChangesAsync();
        }

        private static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string fullName,
            string password,
            string role,
            string? bio = null,
            string? bankName = null,
            string? bankAccountNumber = null,
            string? bankAccountName = null,
            decimal? commissionRate = null)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    Bio = bio,
                    EmailConfirmed = true,
                    IsActive = true,
                    CommissionRate = commissionRate ?? 0.30m,
                    BankName = bankName,
                    BankAccountNumber = bankAccountNumber,
                    BankAccountName = bankAccountName
                };

                var createResult = await userManager.CreateAsync(user, password);
                EnsureIdentitySuccess(createResult, $"Create user {email}");
            }
            else
            {
                var changed = false;

                if (string.IsNullOrWhiteSpace(user.FullName))
                {
                    user.FullName = fullName;
                    changed = true;
                }

                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    changed = true;
                }

                if (!user.IsActive)
                {
                    user.IsActive = true;
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(bio) && string.IsNullOrWhiteSpace(user.Bio))
                {
                    user.Bio = bio;
                    changed = true;
                }

                if (commissionRate.HasValue && user.CommissionRate <= 0)
                {
                    user.CommissionRate = commissionRate.Value;
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(bankName) && string.IsNullOrWhiteSpace(user.BankName))
                {
                    user.BankName = bankName;
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(bankAccountNumber) && string.IsNullOrWhiteSpace(user.BankAccountNumber))
                {
                    user.BankAccountNumber = bankAccountNumber;
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(bankAccountName) && string.IsNullOrWhiteSpace(user.BankAccountName))
                {
                    user.BankAccountName = bankAccountName;
                    changed = true;
                }

                if (changed)
                {
                    var updateResult = await userManager.UpdateAsync(user);
                    EnsureIdentitySuccess(updateResult, $"Update user {email}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                var roleResult = await userManager.AddToRoleAsync(user, role);
                EnsureIdentitySuccess(roleResult, $"Assign role {role} for {email}");
            }

            return user;
        }

        private static async Task<Chapter> EnsureChapterAsync(
            ApplicationDbContext context,
            int courseId,
            string title,
            int displayOrder)
        {
            var chapter = await context.Chapters.FirstOrDefaultAsync(c => c.CourseId == courseId && c.Title == title);
            if (chapter == null)
            {
                chapter = new Chapter
                {
                    CourseId = courseId,
                    Title = title,
                    DisplayOrder = displayOrder
                };
                context.Chapters.Add(chapter);
                await context.SaveChangesAsync();
            }
            else
            {
                chapter.DisplayOrder = displayOrder;
            }

            return chapter;
        }

        private static async Task<Lesson> EnsureLessonAsync(
            ApplicationDbContext context,
            int chapterId,
            string title,
            int durationMinutes,
            int displayOrder,
            string? description,
            string? videoUrl = null)
        {
            var lesson = await context.Lessons.FirstOrDefaultAsync(l => l.ChapterId == chapterId && l.Title == title);
            if (lesson == null)
            {
                lesson = new Lesson
                {
                    ChapterId = chapterId,
                    Title = title,
                    DurationMinutes = durationMinutes,
                    DisplayOrder = displayOrder,
                    Description = description,
                    VideoUrl = videoUrl
                };
                context.Lessons.Add(lesson);
                await context.SaveChangesAsync();
            }
            else
            {
                lesson.DurationMinutes = durationMinutes;
                lesson.DisplayOrder = displayOrder;
                lesson.Description = description;
                lesson.VideoUrl = videoUrl;
            }

            return lesson;
        }

        private static async Task<Assignment> EnsureAssignmentAsync(
            ApplicationDbContext context,
            int chapterId,
            int? lessonId,
            string title,
            string? description,
            decimal maxScore,
            int displayOrder,
            DateTime? dueDate)
        {
            var assignment = await context.Assignments.FirstOrDefaultAsync(a => a.ChapterId == chapterId && a.Title == title);
            if (assignment == null)
            {
                assignment = new Assignment
                {
                    ChapterId = chapterId,
                    LessonId = lessonId,
                    Title = title,
                    Description = description,
                    MaxScore = maxScore,
                    DisplayOrder = displayOrder,
                    DueDate = dueDate
                };
                context.Assignments.Add(assignment);
                return assignment;
            }

            assignment.LessonId = lessonId;
            assignment.Description = description;
            assignment.MaxScore = maxScore;
            assignment.DisplayOrder = displayOrder;
            assignment.DueDate = dueDate;
            return assignment;
        }

        private static async Task EnsureEnrollmentAsync(
            ApplicationDbContext context,
            string userId,
            int courseId,
            decimal progressPercent,
            EnrollmentStatus status,
            DateTime enrolledAt,
            DateTime? completedAt)
        {
            var existing = await context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (existing != null)
            {
                return;
            }

            context.Enrollments.Add(new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                ProgressPercent = progressPercent,
                Status = status,
                EnrolledAt = enrolledAt,
                CompletedAt = completedAt
            });
        }

        private static async Task EnsureLessonResourceAsync(
            ApplicationDbContext context,
            Dictionary<string, Lesson> lessonMap,
            string courseCode,
            string lessonTitle,
            string fileName,
            string fileUrl,
            string fileType,
            long fileSizeBytes)
        {
            if (!lessonMap.TryGetValue(BuildLessonKey(courseCode, lessonTitle), out var lesson))
            {
                return;
            }

            var exists = await context.LessonResources.AnyAsync(r => r.LessonId == lesson.Id && r.FileName == fileName);
            if (exists)
            {
                return;
            }

            context.LessonResources.Add(new LessonResource
            {
                LessonId = lesson.Id,
                FileName = fileName,
                FileUrl = fileUrl,
                FileType = fileType,
                FileSizeBytes = fileSizeBytes
            });
        }

        private static async Task EnsureLessonProgressAsync(
            ApplicationDbContext context,
            string userId,
            Dictionary<string, Lesson> lessonMap,
            string courseCode,
            string lessonTitle,
            LessonStatus status,
            DateTime? completedAt)
        {
            if (!lessonMap.TryGetValue(BuildLessonKey(courseCode, lessonTitle), out var lesson))
            {
                return;
            }

            var exists = await context.LessonProgresses.AnyAsync(lp => lp.UserId == userId && lp.LessonId == lesson.Id);
            if (exists)
            {
                return;
            }

            context.LessonProgresses.Add(new LessonProgress
            {
                UserId = userId,
                LessonId = lesson.Id,
                Status = status,
                CompletedAt = completedAt
            });
        }

        private static async Task<AssignmentSubmission> EnsureSubmissionAsync(
            ApplicationDbContext context,
            Dictionary<string, Assignment> assignmentMap,
            string courseCode,
            string assignmentTitle,
            string userId,
            string? content,
            SubmissionStatus status,
            DateTime? submittedAt,
            DateTime createdAt)
        {
            if (!assignmentMap.TryGetValue(BuildAssignmentKey(courseCode, assignmentTitle), out var assignment))
            {
                throw new InvalidOperationException($"Assignment not found for seed: {courseCode} / {assignmentTitle}");
            }

            var existing = await context.AssignmentSubmissions
                .OrderBy(s => s.Id)
                .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.UserId == userId);

            if (existing != null)
            {
                return existing;
            }

            var submission = new AssignmentSubmission
            {
                AssignmentId = assignment.Id,
                UserId = userId,
                Content = content,
                Status = status,
                SubmittedAt = submittedAt,
                CreatedAt = createdAt,
                UpdatedAt = DateTime.UtcNow
            };

            context.AssignmentSubmissions.Add(submission);
            await context.SaveChangesAsync();
            return submission;
        }

        private static async Task EnsureGradeAsync(
            ApplicationDbContext context,
            AssignmentSubmission submission,
            string gradedById,
            decimal score,
            PassStatus passStatus,
            string? comment,
            DateTime gradedAt)
        {
            var exists = await context.Grades.AnyAsync(g => g.SubmissionId == submission.Id);
            if (exists)
            {
                return;
            }

            context.Grades.Add(new Grade
            {
                SubmissionId = submission.Id,
                GradedById = gradedById,
                Score = score,
                PassStatus = passStatus,
                Comment = comment,
                GradedAt = gradedAt
            });
        }

        private static async Task<Payment> EnsurePaymentAsync(
            ApplicationDbContext context,
            string transactionCode,
            string userId,
            int courseId,
            decimal amount,
            PaymentMethod method,
            PaymentStatus status,
            DateTime createdAt,
            DateTime? completedAt)
        {
            var payment = await context.Payments.FirstOrDefaultAsync(p => p.TransactionCode == transactionCode);
            if (payment == null)
            {
                payment = new Payment
                {
                    TransactionCode = transactionCode,
                    UserId = userId,
                    CourseId = courseId,
                    Amount = amount,
                    PaymentMethod = method,
                    Status = status,
                    CreatedAt = createdAt,
                    CompletedAt = completedAt
                };
                context.Payments.Add(payment);
                await context.SaveChangesAsync();
                return payment;
            }

            return payment;
        }

        private static async Task<InstructorEarning> EnsureInstructorEarningAsync(
            ApplicationDbContext context,
            Payment payment,
            string instructorId,
            EarningStatus status,
            DateTime createdAt,
            DateTime? paidAt,
            int? payoutBatchId)
        {
            var earning = await context.InstructorEarnings.FirstOrDefaultAsync(e => e.PaymentId == payment.Id);
            if (earning != null)
            {
                return earning;
            }

            var instructor = await context.Users.FirstAsync(u => u.Id == instructorId);
            var feeRate = instructor.CommissionRate;
            var platformFee = Math.Round(payment.Amount * feeRate, 0);

            earning = new InstructorEarning
            {
                InstructorId = instructorId,
                CourseId = payment.CourseId,
                PaymentId = payment.Id,
                GrossAmount = payment.Amount,
                PlatformFeeRate = feeRate,
                PlatformFee = platformFee,
                NetAmount = payment.Amount - platformFee,
                Status = status,
                CreatedAt = createdAt,
                PaidAt = paidAt,
                PayoutBatchId = payoutBatchId
            };

            context.InstructorEarnings.Add(earning);
            return earning;
        }

        private static async Task EnsureScheduleAsync(
            ApplicationDbContext context,
            int courseId,
            string instructorId,
            string title,
            string? description,
            DateTime startTime,
            DateTime endTime,
            string? zoomLink)
        {
            var schedule = await context.Schedules
                .FirstOrDefaultAsync(s => s.CourseId == courseId && s.Title == title);

            if (schedule == null)
            {
                context.Schedules.Add(new Schedule
                {
                    CourseId = courseId,
                    InstructorId = instructorId,
                    Title = title,
                    Description = description,
                    StartTime = startTime,
                    EndTime = endTime,
                    ZoomLink = zoomLink
                });
                return;
            }

            schedule.InstructorId = instructorId;
            schedule.Description = description;
            schedule.StartTime = startTime;
            schedule.EndTime = endTime;
            schedule.ZoomLink = zoomLink;
        }

        private static async Task EnsureCalendarEventAsync(
            ApplicationDbContext context,
            string userId,
            string title,
            string? description,
            DateTime startTime,
            DateTime endTime,
            EventType eventType,
            int? relatedEntityId)
        {
            var existing = await context.CalendarEvents
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Title == title);

            if (existing == null)
            {
                context.CalendarEvents.Add(new CalendarEvent
                {
                    UserId = userId,
                    Title = title,
                    Description = description,
                    StartTime = startTime,
                    EndTime = endTime,
                    EventType = eventType,
                    RelatedEntityId = relatedEntityId
                });
                return;
            }

            existing.Description = description;
            existing.StartTime = startTime;
            existing.EndTime = endTime;
            existing.EventType = eventType;
            existing.RelatedEntityId = relatedEntityId;
        }

        private static async Task EnsureNotificationAsync(
            ApplicationDbContext context,
            string userId,
            string title,
            string message,
            string type,
            NotificationChannel channel,
            bool isRead,
            DateTime createdAt,
            string? senderInstructorId,
            string? senderAdminId,
            int? courseId)
        {
            var existing = await context.Notifications.FirstOrDefaultAsync(n =>
                n.UserId == userId &&
                n.Title == title &&
                n.Type == type &&
                n.SenderInstructorId == senderInstructorId &&
                n.SenderAdminId == senderAdminId);

            if (existing != null)
            {
                return;
            }

            context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                Channel = channel,
                IsRead = isRead,
                CreatedAt = createdAt,
                ReadAt = isRead ? createdAt.AddMinutes(20) : null,
                SenderInstructorId = senderInstructorId,
                SenderAdminId = senderAdminId,
                CourseId = courseId
            });
        }

        private static void EnsureIdentitySuccess(IdentityResult result, string action)
        {
            if (result.Succeeded)
            {
                return;
            }

            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"{action} failed: {errors}");
        }

        private static string BuildLessonKey(string courseCode, string lessonTitle)
            => $"{NormalizeKeyPart(courseCode)}|{NormalizeKeyPart(lessonTitle)}";

        private static string BuildAssignmentKey(string courseCode, string assignmentTitle)
            => $"{NormalizeKeyPart(courseCode)}|{NormalizeKeyPart(assignmentTitle)}";

        private static string NormalizeKeyPart(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(decomposed.Length);

            foreach (var ch in decomposed)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(char.ToLowerInvariant(ch));
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private sealed class SeedUsers
        {
            public required ApplicationUser Admin { get; init; }
            public required Dictionary<string, ApplicationUser> Instructors { get; init; }
            public required Dictionary<string, ApplicationUser> Learners { get; init; }
        }

        private sealed record CategorySeed(string Slug, string Name, int DisplayOrder);

        private sealed record NotificationRuleSeed(
            string EventType,
            string DisplayName,
            string Description,
            bool IsEnabled,
            NotificationChannel Channel,
            string TemplateBody);

        private sealed record CourseSeed(
            string Code,
            string Name,
            string Description,
            string CoverImageUrl,
            string CategorySlug,
            string InstructorEmail,
            CourseLevel Level,
            decimal DurationHours,
            CourseType CourseType,
            decimal OriginalPrice,
            decimal? PromoPrice,
            CourseStatus Status,
            decimal AverageRating,
            int RatingCount,
            int CreatedDaysAgo,
            int PublishedDaysAgo);

        private sealed record ScheduleSeed(
            string CourseCode,
            string InstructorEmail,
            string Title,
            string? Description,
            DateTime StartTime,
            DateTime EndTime,
            string? ZoomLink);

        private sealed record ActivityLogSeed(
            string Action,
            string Description,
            string UserId,
            DateTime CreatedAt);
    }
}
