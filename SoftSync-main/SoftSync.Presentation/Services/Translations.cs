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
        ["result.score"] = ("Score", "Điểm"),

        // Band labels (raw score 8–32 per skill: 8–14 / 15–20 / 21–26 / 27–32)
        ["result.level.passive"] = ("Passive", "Bị động"),
        ["result.level.developing"] = ("Developing", "Đang phát triển"),
        ["result.level.proactive"] = ("Proactive", "Chủ động"),
        ["result.level.mastery"] = ("Mastery", "Làm chủ"),

        // Per-skill × band descriptions. Keyed result.desc.<slug>.<band>; slug is
        // the skill name with spaces removed, lowercased (see AssessmentResult.razor).
        ["result.desc.timemanagement.passive"] = (
            "Right now you tend to study and work reactively — only starting when a deadline is near or when someone reminds you. Procrastination and losing focus are affecting your results quite a lot. This is a skill you should prioritize first, beginning with small changes like setting a fixed study time each day.",
            "Hiện tại bạn thường học và làm việc theo cách bị động — chỉ bắt tay vào khi gần đến hạn hoặc khi có người nhắc. Việc trì hoãn và mất tập trung đang ảnh hưởng khá nhiều đến kết quả học tập của bạn. Đây là kỹ năng bạn nên ưu tiên rèn luyện trước, bắt đầu từ những thay đổi nhỏ như đặt giờ học cố định mỗi ngày."),
        ["result.desc.timemanagement.developing"] = (
            "You're already conscious of planning and have tried some time-management methods, but you haven't kept them up consistently — you easily quit halfway or lose focus when things get hard. You're on the right track; you just need more persistence and a suitable reminder tool to keep the habit long term.",
            "Bạn đã có ý thức lên kế hoạch và thử áp dụng một số cách quản lý thời gian, nhưng chưa duy trì được đều đặn — dễ bỏ giữa chừng hoặc mất tập trung khi gặp việc khó. Bạn đang đi đúng hướng, chỉ cần rèn luyện thêm tính kiên trì và có công cụ nhắc nhở phù hợp để giữ được thói quen lâu dài."),
        ["result.desc.timemanagement.proactive"] = (
            "You organize your time fairly well: you plan, know how to prioritize important tasks, and stay focused most of the time. Occasionally you still misjudge how long things take or get distracted, but overall you are proactively in control of your schedule.",
            "Bạn đã biết cách sắp xếp thời gian khá tốt: có kế hoạch, biết ưu tiên việc quan trọng, và giữ được sự tập trung phần lớn thời gian. Thỉnh thoảng vẫn còn những lúc ước lượng thời gian chưa chính xác hoặc bị xao nhãng, nhưng nhìn chung bạn đang chủ động kiểm soát được lịch trình của mình."),
        ["result.desc.timemanagement.mastery"] = (
            "You manage your time very well: you have a clear plan, prioritize the right things, stay highly focused, and always have a backup plan for the unexpected. This is a strength of yours — keep it up, and consider sharing your approach to help others.",
            "Bạn quản lý thời gian rất tốt: có kế hoạch rõ ràng, biết ưu tiên đúng việc, giữ tập trung cao và luôn có phương án dự phòng khi có việc bất ngờ. Đây là một thế mạnh của bạn — hãy tiếp tục duy trì và có thể chia sẻ cách làm này để giúp đỡ người khác."),

        ["result.desc.communication.passive"] = (
            "You currently struggle to exchange with others — either focusing on speaking while listening little, avoiding disagreements, or reacting harshly to feedback. This can make others misread your meaning or hesitate to be honest with you. Starting with listening to the full sentence before you respond will help you improve quickly.",
            "Bạn hiện gặp khó khăn khi trao đổi với người khác — hoặc chỉ tập trung nói mà ít lắng nghe, hoặc né tránh khi có bất đồng, hoặc phản ứng gay gắt khi bị góp ý. Điều này có thể khiến người khác hiểu lầm ý bạn hoặc ngại chia sẻ thẳng thắn với bạn. Bắt đầu bằng việc tập lắng nghe hết câu trước khi trả lời sẽ giúp bạn cải thiện nhanh."),
        ["result.desc.communication.developing"] = (
            "You know you need to listen and express yourself more clearly, but you're still reluctant to check whether others understood you, or awkward when giving/receiving feedback. Practicing confirmation questions (\"does that make sense to you?\") will help you communicate more confidently.",
            "Bạn đã biết mình cần lắng nghe và diễn đạt rõ ràng hơn, nhưng vẫn còn ngại hỏi lại khi chưa chắc người khác hiểu ý mình, hoặc lúng túng khi phải góp ý/nhận góp ý. Luyện tập thêm cách đặt câu hỏi xác nhận (\"mình nói vậy bạn hiểu chưa?\") sẽ giúp bạn giao tiếp tự tin hơn."),
        ["result.desc.communication.proactive"] = (
            "You communicate fairly well: you listen, choose a way of speaking that fits each person, and handle back-and-forth feedback calmly. You still get a bit awkward in more sensitive situations (e.g. delivering bad news), but overall you're proactive in communication.",
            "Bạn giao tiếp khá tốt: biết lắng nghe, chọn cách nói phù hợp với từng người, và xử lý được các tình huống góp ý qua lại một cách bình tĩnh. Đôi khi vẫn còn lúng túng ở những tình huống nhạy cảm hơn (ví dụ báo tin không vui), nhưng nhìn chung bạn đã chủ động trong giao tiếp."),
        ["result.desc.communication.mastery"] = (
            "You communicate very effectively: you listen actively, express yourself clearly, adjust your style to each person and situation, and handle feedback/conflict calmly and respectfully. This is a skill you can put to great use in teamwork or a leadership role.",
            "Bạn giao tiếp rất hiệu quả: lắng nghe chủ động, diễn đạt rõ ràng, biết điều chỉnh cách nói theo từng người và hoàn cảnh, đồng thời xử lý góp ý/xung đột một cách bình tĩnh, tôn trọng người khác. Đây là kỹ năng bạn có thể phát huy tốt trong làm việc nhóm hoặc vai trò dẫn dắt."),

        ["result.desc.criticalthinking.passive"] = (
            "You tend to believe information immediately without verifying it, or only fix the surface of a problem without finding the real cause. This makes you prone to fake news or repeating the same mistake. Build the habit of asking yourself \"why do I think this is true?\" before believing or deciding anything.",
            "Bạn có xu hướng tin ngay vào thông tin mà không kiểm chứng, hoặc chỉ giải quyết vấn đề ở phần bên ngoài mà không tìm hiểu nguyên nhân thật sự. Điều này khiến bạn dễ bị ảnh hưởng bởi tin giả hoặc lặp lại cùng một lỗi sai. Hãy tập thói quen tự hỏi \"vì sao mình nghĩ điều này đúng?\" trước khi tin hoặc quyết định điều gì."),
        ["result.desc.criticalthinking.developing"] = (
            "You've started asking questions and looking for the root cause of a problem, but you're sometimes swayed by emotion or the majority's opinion, or don't verify sources thoroughly. Building the habit of cross-checking 2–3 sources before believing will make your thinking more solid.",
            "Bạn đã bắt đầu biết đặt câu hỏi và tìm nguyên nhân gốc rễ của vấn đề, nhưng đôi khi vẫn bị cảm xúc hoặc ý kiến số đông chi phối, hoặc chưa kiểm chứng nguồn tin một cách đầy đủ. Rèn thêm thói quen đối chiếu từ 2-3 nguồn trước khi tin sẽ giúp bạn tư duy vững vàng hơn."),
        ["result.desc.criticalthinking.proactive"] = (
            "You analyze problems fairly well: you find root causes, weigh multiple viewpoints, and are willing to change your mind when new evidence appears. You're also fairly careful with new information. Keep sharpening the habit of verifying sources even more thoroughly to go further.",
            "Bạn có khả năng phân tích vấn đề khá tốt: biết tìm nguyên nhân gốc, cân nhắc nhiều góc nhìn, và sẵn sàng thay đổi suy nghĩ khi có bằng chứng mới. Bạn cũng khá cẩn trọng khi tiếp nhận thông tin mới. Tiếp tục rèn luyện thói quen kiểm chứng nguồn tin kỹ hơn nữa sẽ giúp bạn tiến xa hơn."),
        ["result.desc.criticalthinking.mastery"] = (
            "Your critical thinking is very strong: you always verify information from multiple sources, analyze problems to their root, and adjust your views when the evidence is convincing. This strength helps you learn and make decisions effectively — keep it up in subjects that require analysis and research.",
            "Bạn tư duy phản biện rất tốt: luôn kiểm chứng thông tin từ nhiều nguồn, phân tích vấn đề đến gốc rễ, và sẵn sàng điều chỉnh quan điểm khi có bằng chứng thuyết phục. Đây là một điểm mạnh giúp bạn học tập và ra quyết định hiệu quả — hãy tiếp tục phát huy trong các môn học đòi hỏi phân tích, nghiên cứu."),

        // ===== Language switch =====
        ["lang.switchLabel"] = ("VI", "EN"),

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

        // ===== Settings =====
        ["settings.title"] = ("Settings", "Cài đặt"),
        ["settings.subtitle"] = ("Manage your account, learning experience and preferences.", "Quản lý tài khoản, trải nghiệm học tập và tùy chọn của bạn."),
        ["settings.save"] = ("Save changes", "Lưu thay đổi"),
        ["settings.saved"] = ("Your changes have been saved.", "Đã lưu thay đổi của bạn."),
        ["settings.back"] = ("Back to settings", "Quay lại cài đặt"),

        ["settings.tab.account"] = ("Account", "Tài khoản"),
        ["settings.tab.learning"] = ("Learning", "Học tập"),
        ["settings.tab.appearance"] = ("Appearance & language", "Giao diện & ngôn ngữ"),
        ["settings.tab.account.actions"] = ("Account actions", "Tác vụ tài khoản"),

        ["settings.displayName"] = ("Display name", "Tên hiển thị"),
        ["settings.security"] = ("Security & sign-in", "Bảo mật & đăng nhập"),
        ["settings.linkedAccounts"] = ("Linked Accounts", "Tài khoản liên kết"),
        ["settings.eyebrow"] = ("SETTINGS", "CÀI ĐẶT"),
        ["settings.side.sub"] = ("Manage your profile and SoftSync experience", "Quản lý hồ sơ và trải nghiệm SoftSync của bạn"),
        ["settings.tab.notifications"] = ("Notifications", "Thông báo"),
        ["settings.tab.privacy"] = ("Privacy & Community", "Quyền riêng tư & Cộng đồng"),
        ["settings.tab.billing"] = ("Subscription & Billing", "Gói dịch vụ & Thanh toán"),
        ["settings.profile.info"] = ("Profile Information", "Thông tin hồ sơ"),
        ["settings.secureNote"] = ("Your information is secure and encrypted.", "Thông tin của bạn được bảo mật và mã hóa."),
        ["settings.hero.quote"] = ("Small progress every day leads to big changes.", "Tiến bộ nhỏ mỗi ngày dẫn đến thay đổi lớn."),
        ["settings.hero.xp"] = ("XP Points", "Điểm XP"),
        ["settings.hero.streak"] = ("Day Streak", "Chuỗi ngày"),
        ["settings.hero.badges"] = ("Badges", "Huy hiệu"),
        ["settings.linked.connected"] = ("Connected", "Đã kết nối"),
        ["settings.linked.notConnected"] = ("Not connected", "Chưa kết nối"),
        ["settings.linked.connect"] = ("Connect", "Kết nối"),
        ["settings.premium.title"] = ("Go Premium", "Nâng cấp Premium"),
        ["settings.premium.sub"] = ("Unlock unlimited AI chats, advanced case studies and exclusive rewards.", "Mở khóa trò chuyện AI không giới hạn, tình huống nâng cao và phần thưởng độc quyền."),
        ["settings.premium.cta"] = ("Upgrade Now", "Nâng cấp ngay"),
        ["settings.notifications.hint"] = ("Choose how SoftSync keeps you in the loop.", "Chọn cách SoftSync giữ liên lạc với bạn."),
        ["settings.notif.study"] = ("Study reminders", "Nhắc nhở học tập"),
        ["settings.notif.study.hint"] = ("A gentle nudge to keep your streak going.", "Nhắc nhẹ để duy trì chuỗi ngày học của bạn."),
        ["settings.notif.progress"] = ("Progress summaries", "Tóm tắt tiến độ"),
        ["settings.notif.progress.hint"] = ("Weekly progress summaries and important updates.", "Tóm tắt tiến độ hằng tuần và cập nhật quan trọng."),
        ["settings.notif.community"] = ("Community activity", "Hoạt động cộng đồng"),
        ["settings.notif.community.hint"] = ("Real-time alerts for messages and community replies.", "Cảnh báo thời gian thực cho tin nhắn và phản hồi cộng đồng."),
        ["settings.notif.preview"] = ("These preferences are a preview and aren't saved yet.", "Các tùy chọn này đang ở dạng xem trước và chưa được lưu."),
        ["settings.privacy.hint"] = ("Control what other learners can see about you.", "Kiểm soát những gì người học khác có thể thấy về bạn."),
        ["settings.privacy.profile"] = ("Public profile", "Hồ sơ công khai"),
        ["settings.privacy.profile.hint"] = ("Let other members view your profile and progress.", "Cho phép thành viên khác xem hồ sơ và tiến độ của bạn."),
        ["settings.privacy.leaderboard"] = ("Show on leaderboard", "Hiển thị trên bảng xếp hạng"),
        ["settings.privacy.leaderboard.hint"] = ("Appear in community XP rankings.", "Xuất hiện trong bảng xếp hạng XP cộng đồng."),
        ["settings.billing.hint"] = ("Manage your subscription plan and payment options.", "Quản lý gói đăng ký và tùy chọn thanh toán của bạn."),
        ["settings.billing.currentPlan"] = ("Current plan", "Gói hiện tại"),
        ["settings.billing.freePlan"] = ("You're on the free plan. Upgrade any time to unlock premium features.", "Bạn đang dùng gói miễn phí. Nâng cấp bất cứ lúc nào để mở khóa tính năng cao cấp."),

        ["settings.learning.hint"] = ("SoftSync's AI uses these to personalize your roadmap and activity suggestions.", "AI của SoftSync dùng các thiết lập này để cá nhân hóa lộ trình và gợi ý hoạt động."),
        ["settings.prioritySkills"] = ("Priority skills", "Kỹ năng ưu tiên"),
        ["settings.currentLevel"] = ("Current level", "Mức độ hiện tại"),
        ["settings.level.beginner"] = ("Beginner", "Mới bắt đầu"),
        ["settings.level.intermediate"] = ("Intermediate", "Trung cấp"),
        ["settings.level.advanced"] = ("Advanced", "Nâng cao"),
        ["settings.dailyMinutes"] = ("Time to learn each day", "Thời gian học mỗi ngày"),
        ["settings.minutes"] = ("min", "phút"),
        ["settings.daysPerWeek"] = ("Days per week", "Số ngày mỗi tuần"),
        ["settings.preferredTime"] = ("Preferred study time", "Thời điểm học yêu thích"),
        ["settings.time.morning"] = ("Morning", "Buổi sáng"),
        ["settings.time.afternoon"] = ("Afternoon", "Buổi chiều"),
        ["settings.time.evening"] = ("Evening", "Buổi tối"),
        ["settings.time.night"] = ("Night", "Đêm khuya"),

        ["settings.language"] = ("Language", "Ngôn ngữ"),
        ["settings.theme"] = ("Theme", "Giao diện"),
        ["settings.theme.light"] = ("Light", "Sáng"),
        ["settings.theme.dark"] = ("Dark", "Tối"),
        ["settings.theme.system"] = ("System", "Theo hệ thống"),
        ["settings.reduceMotion"] = ("Reduce motion", "Giảm hiệu ứng chuyển động"),
        ["settings.reduceMotion.hint"] = ("Minimizes animations for a calmer, more accessible experience.", "Giảm hoạt ảnh để trải nghiệm nhẹ nhàng và dễ tiếp cận hơn."),

        ["settings.session"] = ("Session", "Phiên đăng nhập"),
        ["settings.danger"] = ("Danger zone", "Vùng nguy hiểm"),
        ["settings.danger.hint"] = ("These actions affect your whole account. Please read carefully.", "Các tác vụ này ảnh hưởng đến toàn bộ tài khoản. Hãy đọc kỹ."),
        ["settings.suspend.title"] = ("Suspend account", "Tạm khóa tài khoản"),
        ["settings.suspend.hint"] = ("Temporarily lock your account. Sign back in later to reactivate.", "Tạm khóa tài khoản. Đăng nhập lại sau để kích hoạt lại."),
        ["settings.suspend.button"] = ("Suspend", "Tạm khóa"),
        ["settings.suspend.confirm"] = ("You'll be signed out and won't be able to sign in until an admin restores your account. Continue?", "Bạn sẽ bị đăng xuất và không thể đăng nhập cho đến khi quản trị viên khôi phục tài khoản. Tiếp tục?"),
        ["settings.suspend.done"] = ("Your account has been suspended.", "Tài khoản của bạn đã bị tạm khóa."),
        ["settings.delete.title"] = ("Delete account", "Xóa tài khoản"),
        ["settings.delete.hint"] = ("Permanently delete your account and data. This cannot be undone.", "Xóa vĩnh viễn tài khoản và dữ liệu. Không thể hoàn tác."),
        ["settings.delete.button"] = ("Delete account", "Xóa tài khoản"),
        ["settings.delete.confirm"] = ("This permanently deletes your account and all associated data. This action cannot be undone.", "Thao tác này xóa vĩnh viễn tài khoản và toàn bộ dữ liệu liên quan. Không thể hoàn tác."),
        ["settings.delete.typePrompt"] = ("To confirm, type:", "Để xác nhận, hãy gõ:"),
        ["settings.delete.word"] = ("DELETE", "XÓA"),
        ["settings.delete.done"] = ("Your account has been deleted.", "Tài khoản của bạn đã được xóa."),

        // ===== Settings: change password =====
        ["settings.password.title"] = ("Change password", "Đổi mật khẩu"),
        ["settings.password.subtitle"] = ("Enter your current password and a new one.", "Nhập mật khẩu hiện tại và mật khẩu mới."),
        ["settings.password.setSubtitle"] = ("Set a password to sign in without Google.", "Đặt mật khẩu để đăng nhập không cần Google."),
        ["settings.password.current"] = ("Current password", "Mật khẩu hiện tại"),
        ["settings.password.new"] = ("New password", "Mật khẩu mới"),
        ["settings.password.confirm"] = ("Confirm new password", "Nhập lại mật khẩu mới"),
        ["settings.password.submit"] = ("Update password", "Cập nhật mật khẩu"),
        ["settings.password.changed"] = ("Your password has been updated.", "Mật khẩu của bạn đã được cập nhật."),

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
