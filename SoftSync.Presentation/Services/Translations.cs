namespace SoftSync.Presentation.Services;

/// <summary>
/// Central translation table. Each key maps to an (English, Vietnamese) pair.
/// Add new keys here as pages are localized; <see cref="Get"/> falls back to
/// English, then to the raw key, so a missing entry never crashes the UI.
/// </summary>
public static class Translations
{
    public static string Get(string key, AppLanguage lang)
    {
        if (_map.TryGetValue(key, out var pair))
            return lang == AppLanguage.Vi ? pair.Vi : pair.En;
        return key;
    }

    private static readonly Dictionary<string, (string En, string Vi)> _map = new()
    {
        // ===== Nav =====
        ["nav.home"] = ("Home", "Trang chủ"),
        ["nav.assistant"] = ("AI Assistant", "Trợ lý AI"),
        ["nav.caseStudies"] = ("Case Studies", "Tình huống"),
        ["nav.progress"] = ("Progress", "Tiến độ"),
        ["nav.community"] = ("Community", "Cộng đồng"),

        // ===== Home =====
        ["home.tagline"] = ("AI-powered soft skills learning app", "Ứng dụng học kỹ năng mềm bằng AI"),
        ["home.heroTitlePrefix"] = ("Master your future with ", "Làm chủ tương lai với "),
        ["home.heroTitleHighlight"] = ("Intelligence", "Trí tuệ"),
        ["home.heroDesc"] = (
            "SoftSync is an AI-powered platform that helps you assess your soft skills, practice through real-world scenarios, and follow a personalized growth path tailored just for you.",
            "SoftSync là nền tảng ứng dụng AI giúp bạn đánh giá kỹ năng mềm, luyện tập qua các tình huống thực tế, và theo một lộ trình phát triển cá nhân hóa dành riêng cho bạn."),
        ["home.startJourney"] = ("Start Your Journey", "Bắt đầu hành trình"),
        ["home.stat.coreSkills"] = ("Core Skills", "Kỹ năng cốt lõi"),
        ["home.stat.assessment"] = ("Assessment", "Đánh giá"),
        ["home.stat.support"] = ("AI Support", "Hỗ trợ AI"),
        ["home.mockup.title"] = ("SoftSync AI", "SoftSync AI"),
        ["home.mockup.roadmap"] = ("Personal Roadmap", "Lộ trình cá nhân"),
        ["home.mockup.chatAssistant"] = ("Hello! How can I help you today?", "Xin chào! Tôi có thể giúp gì cho bạn?"),
        ["home.mockup.chatUser"] = ("I want to improve my teamwork.", "Tôi muốn cải thiện kỹ năng làm việc nhóm."),
        ["home.badge.verified"] = ("Skill Verified", "Đã xác thực kỹ năng"),
        ["home.badge.aiPowered"] = ("AI Powered", "Vận hành bởi AI"),
        ["home.feature.assessment.title"] = ("AI Assessment", "Đánh giá bằng AI"),
        ["home.feature.assessment.desc"] = (
            "Discover your true potential with our intelligent skill evaluation system.",
            "Khám phá tiềm năng thật sự của bạn với hệ thống đánh giá kỹ năng thông minh."),
        ["home.feature.roadmap.title"] = ("Personalized Roadmap", "Lộ trình cá nhân hóa"),
        ["home.feature.roadmap.desc"] = (
            "A step-by-step learning journey customized to your strengths and goals.",
            "Hành trình học từng bước, tùy chỉnh theo điểm mạnh và mục tiêu của bạn."),
        ["home.feature.assistant.title"] = ("AI Assistant", "Trợ lý AI"),
        ["home.feature.assistant.desc"] = (
            "Get instant feedback and guidance from your always-available AI mentor.",
            "Nhận phản hồi và hướng dẫn tức thì từ người cố vấn AI luôn sẵn sàng."),

        // ===== Assistant =====
        ["assistant.title"] = ("AI Assistant", "Trợ lý AI"),
        ["assistant.subtitle"] = ("Ask me anything about soft skills or your progress.", "Hỏi tôi bất cứ điều gì về kỹ năng mềm hoặc tiến độ của bạn."),
        ["assistant.typing"] = ("Typing...", "Đang nhập..."),
        ["assistant.inputPlaceholder"] = ("Type your message...", "Nhập tin nhắn của bạn..."),

        // ===== Assessment =====
        ["assessment.preparing"] = ("Preparing your AI-driven assessment...", "Đang chuẩn bị bài đánh giá bằng AI cho bạn..."),
        ["assessment.analyzing"] = ("Analyzing your soft skills with AI...", "Đang phân tích kỹ năng mềm của bạn bằng AI..."),
        ["assessment.question"] = ("Question", "Câu hỏi"),
        ["assessment.of"] = ("of", "trên"),
        ["assessment.skillCommunication"] = ("Skill: Communication", "Kỹ năng: Giao tiếp"),
        ["assessment.finish"] = ("Finish", "Hoàn thành"),
        ["assessment.nextQuestion"] = ("Next Question", "Câu hỏi tiếp theo"),

        // ===== Select Skills =====
        ["selectSkills.title"] = ("Which skills do you want to improve?", "Bạn muốn cải thiện những kỹ năng nào?"),
        ["selectSkills.subtitle"] = ("Select as many as you'd like. Our AI will focus on these.", "Chọn bao nhiêu tùy bạn. AI của chúng tôi sẽ tập trung vào những kỹ năng này."),
        ["selectSkills.loading"] = ("Loading skills...", "Đang tải kỹ năng..."),
        ["selectSkills.startAssessment"] = ("Start Assessment", "Bắt đầu đánh giá"),

        // ===== Case Studies =====
        ["caseStudies.titlePrefix"] = ("Interactive", "Tình huống"),
        ["caseStudies.titleHighlight"] = ("Case Studies", "tương tác"),
        ["caseStudies.subtitle"] = ("Apply your knowledge to real-world scenarios.", "Vận dụng kiến thức của bạn vào các tình huống thực tế."),
        ["caseStudies.loading"] = ("Loading case studies...", "Đang tải tình huống..."),
        ["caseStudies.label"] = ("CASE STUDY:", "TÌNH HUỐNG:"),
        ["caseStudies.howRespond"] = ("How would you respond?", "Bạn sẽ phản hồi như thế nào?"),
        ["caseStudies.correctTitle"] = ("Great Choice!", "Lựa chọn tuyệt vời!"),
        ["caseStudies.incorrectTitle"] = ("Room for Improvement", "Vẫn còn điều cần cải thiện"),
        ["caseStudies.tryAnother"] = ("Try Another Case", "Thử tình huống khác"),

        // ===== Community =====
        ["community.title.highlight"] = ("Mentor", "Cố vấn"),
        ["community.title.rest"] = ("Support", "Hỗ trợ"),
        ["community.subtitle"] = ("Connect with experts to accelerate your growth.", "Kết nối với các chuyên gia để tăng tốc quá trình phát triển của bạn."),
        ["community.loading"] = ("Loading experts...", "Đang tải danh sách chuyên gia..."),
        ["community.connect"] = ("Connect", "Kết nối"),
        ["community.notification"] = (
            "Feature coming soon! We'll notify you when {0} is available for coaching.",
            "Tính năng sắp ra mắt! Chúng tôi sẽ thông báo cho bạn khi {0} sẵn sàng huấn luyện."),

        // ===== Profile Setup =====
        ["profile.heading"] = ("Set up your profile", "Thiết lập hồ sơ của bạn"),
        ["profile.fullName"] = ("Full Name", "Họ và tên"),
        ["profile.fullName.placeholder"] = ("e.g. John Doe", "Ví dụ: Nguyễn Văn A"),
        ["profile.age"] = ("Age", "Tuổi"),
        ["profile.role"] = ("Role", "Vai trò"),
        ["profile.role.student"] = ("Student / Sinh viên", "Sinh viên"),
        ["profile.goal"] = ("What is your learning goal?", "Mục tiêu học tập của bạn là gì?"),
        ["profile.goal.placeholder"] = ("e.g. Improve communication skills for job interviews", "Ví dụ: Cải thiện kỹ năng giao tiếp cho các buổi phỏng vấn"),
        ["profile.continue"] = ("Continue", "Tiếp tục"),

        // ===== Progress =====
        ["progress.title.prefix"] = ("Learning", "Tiến độ"),
        ["progress.title.highlight"] = ("Progress", "Học tập"),
        ["progress.subtitle"] = ("Track your growth across all soft skills.", "Theo dõi sự tiến bộ của bạn trên mọi kỹ năng mềm."),
        ["progress.loading"] = ("Loading your statistics...", "Đang tải số liệu của bạn..."),
        ["progress.empty.title"] = ("No data yet", "Chưa có dữ liệu"),
        ["progress.empty.description"] = ("Start learning and completing your roadmap to see progress here!", "Hãy bắt đầu học và hoàn thành lộ trình để xem tiến độ tại đây!"),
        ["progress.empty.cta"] = ("Start Now", "Bắt đầu ngay"),
        ["progress.lastActive"] = ("Last active:", "Hoạt động gần nhất:"),

        // ===== Roadmap =====
        ["roadmap.title.prefix"] = ("Weekly", "Lộ trình"),
        ["roadmap.title.highlight"] = ("Learning Roadmap", "Học tập hằng tuần"),
        ["roadmap.subtitle"] = ("Follow this path to master your target skills.", "Theo lộ trình này để thành thạo các kỹ năng bạn hướng tới."),
        ["roadmap.demoMode"] = ("Demo Mode", "Chế độ dùng thử"),
        ["roadmap.loading"] = ("Loading your personalized roadmap...", "Đang tải lộ trình cá nhân hóa của bạn..."),
        ["roadmap.week"] = ("Week", "Tuần"),

        // ===== Assessment Result =====
        ["result.title.prefix"] = ("Your", "Hồ sơ"),
        ["result.title.highlight"] = ("Soft Skills Profile", "Kỹ năng mềm của bạn"),
        ["result.subtitle"] = ("Based on your assessment, here is where you stand.", "Dựa trên bài đánh giá, đây là vị trí hiện tại của bạn."),
        ["result.loading"] = ("Loading results...", "Đang tải kết quả..."),
        ["result.feedback.high"] = ("Excellent work! Keep honing this skill.", "Làm rất tốt! Hãy tiếp tục trau dồi kỹ năng này."),
        ["result.feedback.low"] = ("Focusing on this area will significantly help your career.", "Tập trung vào lĩnh vực này sẽ giúp ích rất nhiều cho sự nghiệp của bạn."),
        ["result.recommendation.title"] = ("AI Recommendation", "Đề xuất từ AI"),
        ["result.recommendation.part1"] = ("You show a great foundation in ", "Bạn có nền tảng vững chắc về "),
        ["result.recommendation.part2"] = (", but your roadmap suggests prioritizing ", ", nhưng lộ trình gợi ý bạn nên ưu tiên các tình huống "),
        ["result.recommendation.part3"] = (" scenarios to balance your skill set.", " để cân bằng bộ kỹ năng của mình."),
        ["result.viewRoadmap"] = ("View My Roadmap", "Xem lộ trình của tôi"),

        // ===== Language switch =====
        ["lang.switchLabel"] = ("VN", "EN"),

        // ===== Auth: nav =====
        ["nav.login"] = ("Sign in", "Đăng nhập"),
        ["nav.logout"] = ("Sign out", "Đăng xuất"),
        ["nav.search"] = ("Search", "Tìm kiếm"),
        ["nav.notifications"] = ("Notifications", "Thông báo"),
        ["nav.student"] = ("Student", "Sinh viên"),
        ["nav.profile"] = ("My profile", "Hồ sơ của tôi"),
        ["nav.myProfile"] = ("My Profile", "Hồ sơ của tôi"),
        ["nav.myRoadmap"] = ("My Roadmap", "Lộ trình của tôi"),
        ["nav.settings"] = ("Settings", "Cài đặt"),

        // ===== Logout confirm modal =====
        ["logout.confirm.title"] = ("Leave your learning journey?", "Rời khỏi hành trình học?"),
        ["logout.confirm.body"] = ("Your saved progress will still be here when you return.", "Tiến độ đã lưu vẫn còn khi bạn quay lại."),
        ["logout.confirm.cancel"] = ("Cancel", "Ở lại"),

        // ===== Welcome toast =====
        ["welcome.title"] = ("Welcome back,", "Chào mừng trở lại,"),
        ["welcome.subtitle"] = ("SYNCY is ready to help you today!", "SYNCY đã sẵn sàng hỗ trợ bạn hôm nay!"),
        ["nav.level"] = ("Level", "Cấp"),
        ["lang.switchTitle"] = ("Switch language", "Chuyển ngôn ngữ"),

        // ===== SYNCY onboarding wizard =====
        ["wizard.greeting.hi"] = ("Hi", "Chào"),
        ["wizard.greeting.iam"] = ("I'm SYNCY", "Mình là SYNCY"),
        ["wizard.greeting.intro"] = ("Before we start, I'd like to know a little about your goals.", "Trước khi bắt đầu, mình muốn hiểu một chút về mục tiêu của bạn."),
        ["wizard.about.q"] = ("First, tell me a little about you.", "Đầu tiên, cho mình biết đôi chút về bạn nhé."),
        ["wizard.about.ageGroup"] = ("Your age group", "Nhóm tuổi của bạn"),
        ["wizard.about.gender"] = ("So I can pick the right buddy for you", "Để mình chọn người bạn đồng hành phù hợp"),
        ["wizard.age.under18"] = ("Under 18", "Dưới 18"),
        ["wizard.age.1822"] = ("18–22", "18–22"),
        ["wizard.age.2327"] = ("23–27", "23–27"),
        ["wizard.age.28plus"] = ("28+", "28+"),
        ["wizard.gender.male"] = ("Male", "Nam"),
        ["wizard.gender.female"] = ("Female", "Nữ"),
        ["wizard.gender.other"] = ("Prefer not", "Không nói"),
        ["wizard.goal.q"] = ("What's your main goal right now?", "Mục tiêu chính của bạn lúc này là gì?"),
        ["wizard.goal.communication"] = ("Improve communication", "Cải thiện giao tiếp"),
        ["wizard.goal.internship"] = ("Prepare for internship", "Chuẩn bị cho thực tập"),
        ["wizard.goal.leadership"] = ("Build leadership skills", "Xây dựng kỹ năng lãnh đạo"),
        ["wizard.goal.teamwork"] = ("Improve teamwork", "Cải thiện làm việc nhóm"),
        ["wizard.skills.q"] = ("Which skills would you like to focus on?", "Bạn muốn tập trung vào những kỹ năng nào?"),
        ["wizard.done.title"] = ("Great! Your journey is ready.", "Tuyệt! Hành trình của bạn đã sẵn sàng."),
        ["wizard.done.subtitle"] = ("SYNCY will help you build your skills step by step.", "SYNCY sẽ giúp bạn rèn kỹ năng từng bước một."),
        ["wizard.back"] = ("Back", "Quay lại"),
        ["wizard.next"] = ("Continue", "Tiếp tục"),
        ["wizard.start"] = ("Start My Journey", "Bắt đầu hành trình"),

        // ===== Profile: avatar =====
        ["profile.avatar.change"] = ("Change photo", "Đổi ảnh"),
        ["profile.avatar.hint"] = ("Tap the camera to upload a photo (max 3MB)", "Nhấn biểu tượng máy ảnh để tải ảnh lên (tối đa 3MB)"),
        ["profile.avatar.tooLarge"] = ("Image is too large (max 3MB).", "Ảnh quá lớn (tối đa 3MB)."),
        ["profile.avatar.notImage"] = ("Please choose an image file.", "Vui lòng chọn một tệp ảnh."),
        ["profile.avatar.uploadFailed"] = ("Upload failed:", "Tải ảnh thất bại:"),
        ["profile.edit"] = ("Edit profile", "Chỉnh sửa hồ sơ"),
        ["profile.details"] = ("Account details", "Thông tin tài khoản"),
        ["profile.stat.assessments"] = ("Assessments done", "Bài đánh giá đã làm"),
        ["profile.stat.skillsTracked"] = ("Skills tracked", "Kỹ năng theo dõi"),
        ["profile.stat.currentLevel"] = ("Current level", "Cấp hiện tại"),

        // ===== Auth: login =====
        ["auth.login.title"] = ("Sign in", "Đăng nhập"),
        ["auth.login.subtitle"] = ("Welcome back to SoftSync.", "Chào mừng bạn trở lại SoftSync."),
        ["auth.login.submit"] = ("Sign in", "Đăng nhập"),
        ["auth.identifier"] = ("Email or phone number", "Email hoặc số điện thoại"),
        ["auth.identifier.placeholder"] = ("you@example.com or +84...", "you@example.com hoặc +84..."),
        ["auth.password"] = ("Password", "Mật khẩu"),
        ["auth.rememberMe"] = ("Remember me", "Ghi nhớ đăng nhập"),
        ["auth.forgot"] = ("Forgot password?", "Quên mật khẩu?"),
        ["auth.loginWithOtp"] = ("Sign in with a code", "Đăng nhập bằng mã OTP"),
        ["auth.noAccount"] = ("No account yet?", "Chưa có tài khoản?"),
        ["auth.haveAccount"] = ("Already have an account?", "Đã có tài khoản?"),

        // ===== Auth: register =====
        ["auth.register"] = ("Create account", "Tạo tài khoản"),
        ["auth.register.title"] = ("Create your account", "Tạo tài khoản"),
        ["auth.register.subtitle"] = ("Start your soft-skills journey.", "Bắt đầu hành trình kỹ năng mềm của bạn."),
        ["auth.register.submit"] = ("Create account", "Tạo tài khoản"),
        ["auth.email"] = ("Email", "Email"),
        ["auth.phone"] = ("Phone number", "Số điện thoại"),
        ["auth.optional"] = ("optional", "không bắt buộc"),
        ["auth.confirmPassword"] = ("Confirm password", "Nhập lại mật khẩu"),
        ["auth.newPassword"] = ("New password", "Mật khẩu mới"),

        // ===== Auth: OTP login =====
        ["auth.otp.title"] = ("Sign in with a code", "Đăng nhập bằng mã OTP"),
        ["auth.otp.subtitle"] = ("We'll text a code to your phone.", "Chúng tôi sẽ gửi mã về số điện thoại của bạn."),
        ["auth.sendCode"] = ("Send code", "Gửi mã"),
        ["auth.enterCode"] = ("Verification code", "Mã xác nhận"),
        ["auth.verifyLogin"] = ("Verify & sign in", "Xác nhận & đăng nhập"),
        ["auth.codeSent"] = ("A code has been sent. Enter it below.", "Mã đã được gửi. Nhập mã bên dưới."),
        ["auth.backToLogin"] = ("Back to sign in", "Quay lại đăng nhập"),

        // ===== Auth: forgot / reset =====
        ["auth.forgot.title"] = ("Forgot password", "Quên mật khẩu"),
        ["auth.forgot.subtitle"] = ("We'll send a code to reset your password.", "Chúng tôi sẽ gửi mã để đặt lại mật khẩu."),
        ["auth.forgot.channel"] = ("Send the code via", "Gửi mã qua"),
        ["auth.forgot.viaEmail"] = ("Email", "Email"),
        ["auth.forgot.viaSms"] = ("SMS (phone)", "SMS (điện thoại)"),
        ["auth.forgot.destination"] = ("Email or phone number", "Email hoặc số điện thoại"),
        ["auth.forgot.destinationPlaceholder"] = ("Enter the email/phone on your account", "Nhập email/số điện thoại của tài khoản"),
        ["auth.reset.title"] = ("Reset password", "Đặt lại mật khẩu"),
        ["auth.reset.subtitle"] = ("Enter the code we sent and your new password.", "Nhập mã đã gửi và mật khẩu mới."),
        ["auth.reset.submit"] = ("Reset password", "Đặt lại mật khẩu"),
        ["auth.reset.success"] = ("Your password has been reset. Please sign in.", "Mật khẩu đã được đặt lại. Vui lòng đăng nhập."),

        // ===== Auth: external (Google) =====
        ["auth.googleSignIn"] = ("Continue with Google", "Tiếp tục với Google"),
        ["auth.googleNotConfigured"] = ("Google sign-in is not configured yet.", "Đăng nhập Google chưa được cấu hình."),
        ["auth.external.title"] = ("Signing in...", "Đang đăng nhập..."),
        ["auth.external.processing"] = ("Completing sign-in, please wait...", "Đang hoàn tất đăng nhập, vui lòng đợi..."),

        // ===== Auth: errors =====
        ["auth.error.invalidLogin"] = ("Error: Invalid login attempt.", "Lỗi: Thông tin đăng nhập không đúng."),
        ["auth.error.lockedOut"] = ("Error: This account is locked out.", "Lỗi: Tài khoản đang bị khóa."),
        ["auth.error.phoneNotFound"] = ("Error: No account found for this phone number.", "Lỗi: Không tìm thấy tài khoản với số điện thoại này."),
        ["auth.error.codeExpired"] = ("The code has expired. Please request a new one.", "Mã đã hết hạn. Vui lòng yêu cầu mã mới."),
        ["auth.error.codeInvalid"] = ("The code is incorrect.", "Mã không đúng."),
        ["auth.error.codeNotFound"] = ("No active code. Please request a new one.", "Không có mã hợp lệ. Vui lòng yêu cầu mã mới."),
        ["auth.error.tooManyAttempts"] = ("Too many attempts. Please request a new code.", "Quá nhiều lần thử. Vui lòng yêu cầu mã mới."),
        ["auth.error.resetContextMissing"] = ("Reset session missing. Start again from Forgot password.", "Thiếu thông tin phiên. Vui lòng bắt đầu lại từ Quên mật khẩu."),
        ["auth.error.externalNoInfo"] = ("Could not read external login info.", "Không đọc được thông tin đăng nhập ngoài."),
        ["auth.error.externalNoEmail"] = ("The external provider did not return an email.", "Nhà cung cấp không trả về email."),
    };
}
