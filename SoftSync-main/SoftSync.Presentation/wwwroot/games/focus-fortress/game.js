const $=id=>document.getElementById(id);
const clamp=v=>Math.max(0,Math.min(100,v));
const R=[{t:40,g:3.4},{t:45,g:2.6},{t:50,g:2}];
// [icon, nguồn, nội dung, loại]: noise = vô bổ, idea = ý tưởng, urgent = khẩn thật
const D=[
['💬','Zalo lớp','47 tin nhắn mới: “Ai xem điểm chưa??”','noise'],
['😂','TikTok','Video mèo nhảy múa đang hot, 2 triệu lượt xem','noise'],
['🛒','Shopee','Flash sale 12h: tai nghe giảm 50%!','noise'],
['📺','YouTube','Gợi ý cho bạn: “Xem hết 1 series trong 10 phút”','noise'],
['🔔','Facebook','Bạn có 9 thông báo mới','noise'],
['🎮','Bạn bè','Làm một ván nhanh không? Chỉ 15 phút thôi','noise'],
['🧠','Trong đầu','Nhớ ra: cần đăng ký lịch bảo vệ đồ án','idea'],
['🧠','Trong đầu','Nên đọc thêm một bài báo cho phần so sánh','idea'],
['🧠','Trong đầu','Tuần sau sinh nhật bạn thân, chưa mua quà','idea'],
['🧠','Trong đầu','Có thể đổi tên vài biến cho dễ đọc hơn','idea'],
['📞','Giảng viên','Cần file nộp trong 10 phút nữa, gửi gấp!','urgent'],
['📞','Mẹ','Bếp ga ở nhà bị rò, cần xử lý ngay','urgent'],
['💬','Nhóm đồ án','Server demo sập, 30 phút nữa bảo vệ!','urgent'],
['📱','Instagram','Crush vừa đăng story mới, xem ngay kẻo hết 24h!','noise'],
['🎵','Spotify','Playlist “Lofi học bài” vừa cập nhật, vào chọn nhạc lại nhé','noise'],
['🍜','GrabFood','Trà sữa mua 1 tặng 1 trong 15 phút nữa, đặt không?','noise'],
['📰','Tin tức','Ngôi sao A vừa lộ ảnh hẹn hò, xem chi tiết','noise'],
['🎬','Netflix','Tập mới của series bạn theo dõi vừa lên sóng','noise'],
['🤝','Messenger','Bạn gửi meme: “Sinh viên năm cuối là đây”','noise'],
['🛍️','TikTok Shop','Áo hoodie sinh viên đồng giá 99k, chỉ còn 3 cái','noise'],
['🎮','Discord','Cả hội đang lên voice chơi game, vào đi!','noise'],
['🏆','Quiz lạ','Bạn đã trúng iPhone! Bấm nhận trong 5 phút','noise'],
['📸','Facebook','Bạn được gắn thẻ trong 12 ảnh khóa cũ','noise'],
['🧩','Game','Giữ chuỗi 7 ngày liền, vào chơi để không mất chuỗi','noise'],
['📧','Spam','Khóa học “Làm giàu 30 ngày” giảm 90% hôm nay','noise'],
['🤳','Reels','Chỉ lướt 1 video thôi... rồi lại 1 video nữa','noise'],
['📦','Shopee','Đơn hàng đang giao, theo dõi từng phút trên bản đồ','noise'],
['💭','Trong đầu','Giờ này không biết bạn bè đang đi chơi ở đâu nhỉ?','noise'],
['🧹','Trong đầu','Bàn học bừa quá, dọn lại cái đã (lần thứ ba trong ngày)','noise'],
['🎧','Trong đầu','Phải tìm tai nghe xịn hơn thì mới học được','noise'],
['📱','Zalo','Nhóm cấp 3: 120 tin chưa đọc về chuyện họp lớp 2 tháng nữa','noise'],
['🐶','Facebook','Video chó con biết nói tiếng người, xem là cười','noise'],
['📈','App chứng khoán','VN-Index vừa nhảy, vào xem nhanh một cái','noise'],
['🧠','Trong đầu','Cần in tài liệu cho buổi thuyết trình thứ Sáu','idea'],
['🧠','Trong đầu','Hỏi thầy cách trích dẫn nguồn trong báo cáo','idea'],
['🧠','Trong đầu','Đăng ký thi chứng chỉ tiếng Anh trước hạn cuối tháng sau','idea'],
['🧠','Trong đầu','Nhớ trả sách thư viện trước ngày 25','idea'],
['🧠','Trong đầu','Nên làm flashcard cho công thức chương 4','idea'],
['🧠','Trong đầu','Rủ cả nhóm họp phân công việc tuần sau','idea'],
['🧠','Trong đầu','Cập nhật CV để nộp thực tập hè','idea'],
['🧠','Trong đầu','Đóng tiền ký túc xá trước ngày 30','idea'],
['🧠','Trong đầu','Có thể ghi âm bài giảng để nghe lại trên đường đi học','idea'],
['🧠','Trong đầu','Xin anh khóa trên đề cương môn Xác suất thống kê','idea'],
['🧠','Trong đầu','Giặt đồ trước khi hết đồ mặc vào cuối tuần','idea'],
['🧠','Trong đầu','Đặt lịch khám răng, hoãn mãi rồi','idea'],
['🧠','Trong đầu','Nảy ra một cách giải khác cho bài thuật toán hôm qua','idea'],
['🧠','Trong đầu','Lên kế hoạch ôn thi giữa kỳ theo từng môn','idea'],
['🧠','Trong đầu','Mua thêm bút highlight và giấy nhớ cho mùa thi','idea'],
['📞','Giảng viên','Deadline nộp bài dời sang 12h trưa nay, em kịp không?','urgent'],
['💬','Nhóm bài tập','Nhóm trưởng: file slide của bạn đâu? 15 phút nữa lên thuyết trình!','urgent'],
['📧','Phòng đào tạo','Hạn chót đăng ký học phần là 17h hôm nay, quá giờ sẽ bị khóa','urgent'],
['📞','Bạn cùng phòng','Mưa tạt vào cửa sổ, tài liệu trên bàn học đang ướt hết!','urgent'],
['📞','Mẹ','Mẹ gọi nhỡ 3 cuộc, nhắn: gọi về gấp có chuyện trong nhà','urgent'],
['⏰','Hệ thống thi','Bài thi online sẽ tự khóa sau 5 phút, bạn chưa bấm nộp!','urgent'],
['💬','Nhóm đồ án','Git bị lỗi mất code, 20 phút nữa phải nộp, cần bạn khôi phục!','urgent'],
['📞','Nhà trường','Buổi báo cáo đổi sang phòng khác, bắt đầu sau 10 phút','urgent'],
['💬','Bạn thân','Mình mất ví, cần mượn tiền bắt xe về, bạn đang ở gần không?','urgent'],
['📧','Nhà tuyển dụng','Phỏng vấn online 14h, cần xác nhận tham gia trước 12h','urgent'],
['📞','Ký túc xá','Sắp cắt điện cả khu, hãy lưu bài đang làm ngay!','urgent'],
['⚠️','Máy tính','Pin chỉ còn 3%, báo cáo chưa lưu!','urgent'],
['💬','Giảng viên hướng dẫn','Thầy cần bản nháp trước 11h để kịp lịch họp hội đồng','urgent'],
['📞','Bạn cùng nhóm','Mình ốm đột xuất, không thuyết trình được, bạn gánh giúp nhé','urgent'],
['💳','Ngân hàng','Học phí đến hạn hôm nay, trễ sẽ bị phạt phí','urgent'],
['🎤','YouTube','Livestream concert đang diễn ra, vào xem kẻo lỡ','noise'],
['🍿','Facebook','Top 10 phim hay nhất năm, bạn xem được mấy phim?','noise'],
['🧠','Quiz','Bạn thuộc nhóm tính cách nào? Làm trắc nghiệm 3 phút','noise'],
['💅','Pinterest','Ý tưởng decor phòng trọ xinh, lưu ngay vào bảng','noise'],
['⚽','Tin thể thao','Trận tối qua có bàn thắng gây tranh cãi, xem lại clip','noise'],
['🔔','Zalo','Bạn A vừa thả cảm xúc vào bài đăng cũ của bạn','noise'],
['🛒','Lazada','Mã giảm 50k sắp hết hạn, vào săn ngay','noise'],
['💘','App làm quen','Có 5 người vừa thích bạn, xem ai nhé','noise'],
['🎲','Game','Sự kiện cuối tuần sắp kết thúc, còn 2 phần thưởng','noise'],
['📺','Trong đầu','Xem 1 tập thôi rồi học, hứa chỉ 1 tập','noise'],
['🍕','Zalo phòng','Tối nay cả phòng order pizza, chọn vị liền đi!','noise'],
['🪞','Trong đầu','Đổi layout Notion cho đẹp rồi mới học','noise'],
['🗂️','Trong đầu','Tô màu lại thời khóa biểu cho đẹp (lần thứ hai trong tuần)','noise'],
['📚','Trong đầu','Cần xem thêm 10 video “cách học hiệu quả” trước khi học thật','noise'],
['🎨','Trong đầu','Đổi hình nền máy tính cho có động lực học','noise'],
['😴','Trong đầu','Nằm một chút cho đỡ mệt, 5 phút thôi (chắc vậy)','noise'],
['💬','Messenger','Bạn gửi 8 sticker liên tiếp, chẳng có nội dung gì','noise'],
['📱','Thời gian dùng máy','Hôm nay bạn dùng điện thoại 5 giờ, xem báo cáo chi tiết','noise'],
['🧃','Trong đầu','Đi lấy nước, tiện xem tủ lạnh có gì (lần thứ tư)','noise'],
['📡','Wifi','Đổi tên wifi cho hài hước, tiện đo tốc độ mạng luôn','noise'],
['👗','Instagram','Gợi ý outfit đi chơi cuối tuần, lướt xem cho vui','noise'],
['👥','Facebook','Hội “sinh viên than thở” có bài mới 3 nghìn bình luận','noise'],
['🎞️','Trong đầu','Dựng lại video kỷ niệm cho CLB, làm tí cho vui','noise'],
['🧪','App','Thử ngay trào lưu AI ghép mặt đang hot','noise'],
['🧠','Trong đầu','Hỏi giảng viên về đề tài khóa luận trước tuần sau','idea'],
['🧠','Trong đầu','Chuẩn bị câu hỏi cho buổi gặp mentor ngày mai','idea'],
['🧠','Trong đầu','Tải trước tài liệu offline cho chuyến về quê dịp lễ','idea'],
['🧠','Trong đầu','Lập bảng chi tiêu tháng này, tiền ăn đang hơi vượt','idea'],
['🧠','Trong đầu','Gửi lời cảm ơn bạn đã cho mượn vở tuần trước','idea'],
['🧠','Trong đầu','Đăng ký tham gia CLB tình nguyện mùa hè','idea'],
['🧠','Trong đầu','Xem lại ghi chú bài giảng thứ Ba, còn vài chỗ chưa hiểu','idea'],
['🧠','Trong đầu','Xin giấy xác nhận sinh viên cho hồ sơ học bổng','idea'],
['🧠','Trong đầu','Viết nhật ký học tập cuối tuần để rút kinh nghiệm','idea'],
['🧠','Trong đầu','Tìm tài liệu tham khảo cho phần cơ sở lý thuyết','idea'],
['🧠','Trong đầu','Đặt báo thức sớm hơn cho các buổi học 7h','idea'],
['🧠','Trong đầu','Nhớ chụp lại bảng thầy viết trước khi quên','idea'],
['🧠','Trong đầu','Sắp xếp lại thư mục tài liệu theo từng môn','idea'],
['🧠','Trong đầu','Hỏi khi nào mở đơn học bổng khuyến khích học tập','idea'],
['🧠','Trong đầu','Thử học nhóm 2 người cho môn khó nhất','idea'],
['🧠','Trong đầu','Mua vé xe về quê trước dịp nghỉ lễ kẻo hết chỗ','idea'],
['📞','Giảng viên','Em còn ở trường không? Thầy cần em ký biên bản ngay bây giờ','urgent'],
['💬','Nhóm bài tập','Bạn nhóm nộp sai file lên hệ thống, 10 phút nữa đóng cổng!','urgent'],
['⏰','Báo thức','Bạn có buổi thi sau 40 phút, cần xuất phát ngay!','urgent'],
['📧','Học bổng','Hồ sơ học bổng thiếu một giấy tờ, bổ sung trước 16h hôm nay','urgent'],
['📞','Quán làm thêm','Quản lý hỏi bạn có đi ca tối nay không, cần trả lời ngay để xếp người','urgent'],
['🔥','Ký túc xá','Chuông báo cháy tầng bạn đang kêu, hãy rời phòng ngay!','urgent'],
['📞','Bạn cùng phòng','Mình để quên chìa khóa và bị khóa ngoài, bạn về mở giúp','urgent'],
['⚠️','Máy tính','Ổ cứng báo lỗi, cần sao lưu đồ án ngay trước khi hỏng hẳn!','urgent'],
['📧','Phòng thi','Đề thi giữa kỳ dời sớm hơn 1 tiếng, bạn đã xem thông báo chưa?','urgent'],
['💬','Nhóm CLB','Sự kiện bắt đầu sau 20 phút mà bạn đang giữ chìa khóa phòng','urgent'],
['📞','Em họ','Em kẹt ở trạm xe vì lỡ chuyến cuối, nhờ bạn đón giúp ngay','urgent'],
['📧','Thư viện','Sách quá hạn hôm nay, gia hạn online trước 17h để khỏi bị phạt','urgent'],
['🌧️','Bạn cùng phòng','Trời sắp mưa to, đồ phơi và laptop ngoài ban công, thu ngay!','urgent'],
['📞','Hội đồng','Buổi bảo vệ của bạn được dời lên sớm 1 tiếng!','urgent'],
['💬','Cô chủ nhiệm','Cô cần xác nhận đi dã ngoại, hạn trả lời còn 15 phút','urgent'],
['🏥','Bạn cùng lớp','Có bạn bị ngất ở giảng đường, cần người ở gần hỗ trợ ngay','urgent'],
['🎥','Twitch','Streamer yêu thích vừa lên sóng, vào xem tí thôi','noise'],
['📖','Webtoon','Chương mới của truyện bạn theo dõi vừa ra','noise'],
['🍰','Instagram','Công thức bánh flan 5 phút, lưu lại làm thử','noise'],
['🎰','Quảng cáo','Vòng quay may mắn: quay ngay nhận 100k','noise'],
['👟','Shopee','Đôi giày trong giỏ hàng vừa giảm thêm 20%','noise'],
['🧘','Trong đầu','Xem vài video thiền trước đã, có tinh thần mới học tốt','noise'],
['🗣️','Zalo nhóm','Mọi người đang cãi nhau chuyện chia tiền ăn, bạn có ý kiến không?','noise'],
['🎶','TikTok','Bài nhạc đang trend, bạn nghe thử đi','noise'],
['📸','Trong đầu','Chụp góc học tập cho đẹp rồi đăng story','noise'],
['🔄','Email','Bản tin tuần của 6 trang web bạn từng đăng ký','noise'],
['🧮','Trong đầu','Tính xem học từ giờ thì mấy giờ mới xong (mà chưa học)','noise'],
['🌐','Trình duyệt','Mở thêm tab “cách tập trung khi học” và đọc 10 bài','noise'],
['🏀','Bạn bè','Rủ đá bóng chiều nay, đội thiếu người, vào bàn đội hình đi','noise'],
['🎬','YouTube','Video reaction phim bạn chưa xem, xem để biết spoil','noise'],
['🥤','Trong đầu','Ra cửa hàng tiện lợi mua nước, tiện thể đi dạo','noise'],
['🪙','App crypto','Đồng coin bạn từng xem vừa tăng 30%, xem biểu đồ','noise'],
['💄','TikTok','Review son mới đang hot, 3 triệu lượt xem','noise'],
['📅','Trong đầu','Lên kế hoạch kỳ nghỉ hè năm sau (còn 9 tháng)','noise'],
['🤖','Chatbot','Hỏi AI vài câu cho vui: mèo hay chó ngầu hơn?','noise'],
['🛋️','Trong đầu','Sắp xếp lại bàn ghế trong phòng cho hợp phong thủy','noise'],
['📲','Zalo','Bạn chuyển tiếp 5 tin “chia sẻ ngay kẻo mất may mắn”','noise'],
['🧿','Facebook','Bói bài online: tình duyên tuần này của bạn ra sao?','noise'],
['🎯','App thử thách','Thử thách 30 ngày vận động bắt đầu hôm nay, tải app về ngay','noise'],
['👀','Trong đầu','Xem lại điểm các môn học kỳ trước lần thứ năm','noise'],
['🗳️','Facebook','Bình chọn bạn thích đội nào hơn, tham gia cho vui','noise'],
['🍔','Trong đầu','Tìm quán ăn mới cho cuối tuần, đọc review cả tiếng','noise'],
['🎒','Shopee','Balo sinh viên chống nước giảm 40%, chỉ hôm nay','noise'],
['🧸','Trong đầu','Chơi với mèo hàng xóm một tí rồi học','noise'],
['📺','Trong đầu','Xem lại clip hài đã xem 3 lần vì buồn cười quá','noise'],
['🧁','Zalo','Nhóm lớp hỏi tối nay ăn gì, bàn qua 40 tin nhắn','noise'],
['🧠','Trong đầu','Xin thư giới thiệu của giảng viên cho hồ sơ thực tập','idea'],
['🧠','Trong đầu','Tìm khóa học Excel trên mạng để học cuối tuần','idea'],
['🧠','Trong đầu','Tìm nhóm học chung cho môn Cấu trúc dữ liệu','idea'],
['🧠','Trong đầu','Tìm hiểu chương trình trao đổi sinh viên mùa sau','idea'],
['🧠','Trong đầu','Lập danh sách đồ cần mang khi đi thực tế','idea'],
['🧠','Trong đầu','Nhắn thầy xin phản hồi về dàn ý đã gửi tuần trước','idea'],
['🧠','Trong đầu','Mua USB hoặc ổ cứng để sao lưu bài vở','idea'],
['🧠','Trong đầu','Cài sao lưu tự động cho thư mục đồ án','idea'],
['🧠','Trong đầu','Ghi lại câu hỏi để hỏi trong giờ thảo luận','idea'],
['🧠','Trong đầu','Xem lịch thi cuối kỳ và sắp xếp thời gian ôn từng môn','idea'],
['🧠','Trong đầu','Đi cắt tóc trước buổi phỏng vấn tuần sau','idea'],
['🧠','Trong đầu','Hẹn bạn cùng khóa luyện nói tiếng Anh cuối tuần','idea'],
['🧠','Trong đầu','Kiểm tra số dư thẻ xe buýt sinh viên','idea'],
['🧠','Trong đầu','Cài ứng dụng chặn mạng xã hội trong giờ học','idea'],
['🧠','Trong đầu','Làm bản tóm tắt một trang cho chương 2','idea'],
['🧠','Trong đầu','Đặt mục tiêu đọc 20 trang sách mỗi ngày','idea'],
['🧠','Trong đầu','Chuẩn bị lời cảm ơn gửi thầy cô cũ dịp lễ tri ân','idea'],
['🧠','Trong đầu','Tìm học liệu miễn phí cho môn Giải tích','idea'],
['📞','Giảng viên','Em gửi lại file qua email thầy trong 10 phút để kịp họp khoa nhé','urgent'],
['⏰','Hệ thống','Link nộp bài đóng sau 3 phút, bạn mới tải lên được nửa file!','urgent'],
['📧','Phòng công tác sinh viên','Xác nhận tham dự buổi gặp doanh nghiệp, hạn phản hồi 30 phút nữa','urgent'],
['📞','Bạn cùng phòng','Nước tràn ra từ nhà vệ sinh phòng mình, cần xử lý ngay!','urgent'],
['💬','Nhóm đồ án','Bạn đang giữ file dữ liệu duy nhất, cả nhóm chờ bạn gửi gấp!','urgent'],
['🚌','Xe buýt','Chuyến xe cuối về ký túc xá chạy sau 10 phút nữa','urgent'],
['📧','Công ty thực tập','Bạn được nhận! Cần ký hợp đồng và phản hồi trước 15h hôm nay','urgent'],
['📞','Phòng tài vụ','Số tài khoản của bạn bị sai, sửa trước 16h để nhận học bổng','urgent'],
['💬','Bạn thân','Mình bị lạc ở khu lạ, trời tối, bạn gọi giúp mình được không?','urgent'],
['⚠️','Hệ thống','Email trường sắp bị khóa nếu chưa xác thực, còn 10 phút','urgent'],
['📞','Nhà trọ','Chập điện ở dãy bên, cần tắt cầu dao phòng bạn ngay','urgent'],
['📧','Giảng viên','Bài kiểm tra bất ngờ trên hệ thống mở trong 15 phút, vào làm ngay!','urgent'],
['💬','Nhóm thuyết trình','Máy chiếu hỏng, cần bạn mang laptop sang phòng B203 ngay!','urgent'],
['📞','Tổ chức sự kiện','Diễn giả kẹt xe, cần bạn lên sân khấu dẫn trước 10 phút','urgent'],
['⚠️','Lớp trưởng','Điểm danh bù trong 20 phút nữa, ai chưa có mặt sẽ bị ghi vắng','urgent'],
['📧','Học bổng','Lịch phỏng vấn học bổng dời sang 10 phút nữa, vào phòng họp ngay!','urgent'],
['📱','Bạn cùng nhóm','Thầy đang đứng chờ ngoài phòng họp, nhóm mình thiếu bạn đó!','urgent'],
['🔌','Máy tính','Laptop đang tự cập nhật, hủy ngay kẻo mất bài chưa lưu!','urgent']];
const M={noise:{block:'good',park:'ok',now:'bad'},idea:{park:'good',block:'ok',now:'bad'},urgent:{now:'good',park:'bad',block:'bad'}};
const MSG={'noise.block':'🚫 Chặn gọn! Thứ vô bổ không cần vào đầu.','noise.park':'📝 Ghi lại cũng được, nhưng loại này chặn luôn đỡ tốn công.','noise.now':'⚡ Lướt “một chút” là mất cả chục phút để lấy lại mạch.',
'idea.park':'📝 Ghi vào bãi đỗ: đầu nhẹ hẳn mà không mất ý tưởng.','idea.block':'🚫 Chặn mất một ý tưởng hay. Lần sau hãy ghi lại.','idea.now':'⚡ Ý tưởng chưa khẩn, làm ngay là tự cắt đứt mạch.',
'urgent.now':'⚡ Đúng! Việc khẩn thật thì xử lý ngay, xong quay lại.','urgent.park':'📝 Việc khẩn mà để sau sẽ trả giá đắt.','urgent.block':'🚫 Chặn nhầm việc khẩn, hậu quả đến ngay.'};
let s,run=false,last=0,best=0,raf=0,paused=false;
try{best=+localStorage.getItem('ff_best')||0}catch(e){}
const mult=()=>1+.5*Math.min(s.combo,6);
function show(id){document.querySelectorAll('.screen').forEach(e=>e.classList.toggle('hide',e.id!==id));$('best').textContent=best?'Kỷ lục: '+best:''}
function say(t,c){const e=$('say');e.textContent=t;e.className='status '+(c||'')}
function newGame(){s={r:0,score:0,combo:0,maxC:0,dec:0,good:0,prog:[],rev:0,revOk:0,tray:[],deck:{},err:{}};startRound()}
function startRound(){const c=R[s.r];Object.assign(s,{time:c.t,tot:c.t,gap:c.g,p:0,focus:100,pause:0,sp:1.2,parked:[],reviewed:0,cd:3});
 s.tray.forEach(n=>n.el.remove());s.tray=[];show('gm');$('hR').textContent='🍅 Phiên '+(s.r+1)+'/3';
 say('Sẵn sàng! Chặn nhiễu, giữ khiên tập trung.');run=true;paused=false;$('tray').classList.remove('hide');$('pb2').textContent='⏸';last=performance.now();raf=requestAnimationFrame(tick)}
function draw(t){let k=s.deck[t];if(!k||!k.length)k=s.deck[t]=D.filter(d=>d[3]===t).sort(()=>Math.random()-.5);return k.pop()}
function resolve(n){s.tray=s.tray.filter(x=>x!==n);n.el.remove()}
function spawn(){const w=Math.random(),t=w<.55?'noise':w<.8?'idea':'urgent',d=draw(t);
 const el=document.createElement('div');el.className='note';
 el.innerHTML=`<div class="nh"><span>${d[0]}</span><b>${d[1]}</b></div><div>${d[2]}</div><div class="tt"><i></i></div><div class="acts"><button data-a="block">🚫 Chặn <small class="k">1</small></button><button data-a="park">📝 Hoãn <small class="k">2</small></button><button data-a="now">⚡ Ngay <small class="k">3</small></button></div>`;
 const n={type:t,d,el,ttl:t==='urgent'?9:7,bar:el.querySelector('.tt i')};n.max=n.ttl;
 el.onclick=e=>{const a=e.target.dataset.a;if(a)act(n,a)};$('tray').appendChild(el);s.tray.push(n)}
function act(n,a){if(!run||!n||!s.tray.includes(n))return;const o=M[n.type][a],m=mult();resolve(n);s.dec++;
 if(a==='park')s.parked.push(n);
 if(o==='good'){s.good++;s.score+=50*m;s.combo++;s.maxC=Math.max(s.maxC,s.combo);s.focus+=4}
 else if(o==='ok'){s.good+=.5;s.score+=15}
 else{s.combo=0;s.focus-=15;const k=n.type==='urgent'?'urgent.miss':n.type+'.'+a;s.err[k]=(s.err[k]||0)+1}
 if(a==='now'){s.pause=o==='good'?2.5:4;if(o==='good')s.focus-=8}
 s.focus=clamp(s.focus);say(MSG[n.type+'.'+a]+(o==='good'?' +'+Math.round(50*m):o==='ok'?' +15':''),o);const c=$('cd');c.classList.remove('f-good','f-ok','f-bad');void c.offsetWidth;c.classList.add('f-'+o)}
function tick(now){if(!run)return;const dt=Math.min((now-last)/1000,.1);last=now;if(s.cd>0){s.cd-=dt;render();raf=requestAnimationFrame(tick);return}s.time-=dt;
 if(s.pause>0)s.pause-=dt;else{s.p=Math.min(100,s.p+dt*115/s.tot*(.4+.6*s.focus/100));s.score+=dt*3*mult()}
 s.focus=clamp(s.focus+(s.tray.length?-1.2*s.tray.length:2)*dt);
 s.sp-=dt;if(s.sp<=0&&s.tray.length<3){spawn();s.sp=s.gap*(.7+Math.random()*.6)}
 s.tray.slice().forEach(n=>{n.ttl-=dt;n.bar.style.width=n.ttl/n.max*100+'%';
  if(n.ttl<=0){resolve(n);s.dec++;s.combo=0;s.focus=clamp(s.focus-10);s.err.expire=(s.err.expire||0)+1;say('⏰ Bỏ mặc thông báo vẫn làm bạn phân tâm.','bad')}});
 if(s.time<=0){endRound();return}
 render();raf=requestAnimationFrame(tick)}
function render(){$('tm').textContent=s.cd>0?Math.ceil(s.cd):Math.ceil(s.time)+'s';$('tb').style.width=s.time/s.tot*100+'%';
 const f=$('rf');f.style.strokeDashoffset=327*(1-s.focus/100);f.classList.toggle('low',s.focus<35);$('fv').textContent=Math.round(s.focus);
 $('pb').style.width=s.p+'%';$('pg').textContent=Math.round(s.p)+'%';
 $('hS').textContent=Math.round(s.score);$('hM').textContent=mult().toFixed(1);$('hC').textContent=s.combo;$('pk').textContent=s.parked.length;
 s.tray.forEach((n,i)=>n.el.classList.toggle('next',i===0));$('cd').classList.toggle('paused',s.pause>0);$('pz').classList.toggle('hide',s.pause<=0)}
function endRound(){run=false;s.prog.push(s.p);s.tray.forEach(n=>n.el.remove());s.tray=[];
 const last=s.r===2;$('bt').textContent=last?'Phiên cuối: dọn bãi đỗ':'☕ Giải lao sau phiên '+(s.r+1);
 $('bs').textContent='Hoàn thành '+Math.round(s.p)+'% việc chính. '+(s.parked.length?'Xử lý các việc đã hoãn: việc nào đáng làm thì lên lịch, việc vô bổ thì bỏ.':'Bãi đỗ trống, bạn không hoãn việc nào.');
 const box=$('bl');box.innerHTML='';
 s.parked.forEach(n=>{const it=document.createElement('div');it.className='card item';
  it.innerHTML=`<div>${n.d[0]} <b>${n.d[1]}</b>: ${n.d[2]}</div><div class="acts"><button data-c="1">📅 Lên lịch</button><button data-c="0">🗑 Bỏ</button></div><div class="verdict"></div>`;
  it.onclick=e=>{const c=e.target.dataset.c;if(c===undefined||it.dataset.done)return;it.dataset.done=1;
   const ok=(c==='1')===(n.type!=='noise');s.rev++;s.reviewed++;warn();if(ok){s.revOk++;s.score+=30}
   it.querySelectorAll('button').forEach(b=>b.disabled=true);
   const v=it.querySelector('.verdict');v.textContent=ok?'✅ Đúng, +30 điểm.':(n.type==='noise'?'❌ Thứ này không đáng chiếm lịch của bạn.':'❌ Việc này đáng làm, đừng bỏ.');v.style.color=ok?'var(--good)':'var(--bad)'};
  box.appendChild(it)});
 $('nx').textContent=last?'Xem kết quả':'Vào phiên '+(s.r+2);warn();show('br')}
function finish(){const lost=s.parked.length-s.reviewed;if(lost>0)s.score-=10*lost;
 if(s.r<2){s.r++;startRound();return}
 const sc=Math.max(0,Math.round(s.score)),ap=s.prog.reduce((a,b)=>a+b,0)/3,acc=s.good/Math.max(1,s.dec),ix=.5*ap+.5*acc*100,g=ix>=85?'S':ix>=70?'A':ix>=50?'B':'C';
 const nb=sc>best;if(nb){best=sc;try{localStorage.setItem('ff_best',sc)}catch(e){}}
 window.SoftSyncGameHistory?.append('focus-fortress',{
   title:'Focus Fortress',score:sc,unit:'điểm',summary:`Hạng ${g} · Hoàn thành ${Math.round(ap)}% việc chính.`,
   metrics:[{label:'Quyết định đúng',value:`${Math.round(acc*100)}%`},{label:'Hoàn thành việc chính',value:`${Math.round(ap)}%`},{label:'Chuỗi tối đa',value:`x${(1+.5*Math.min(s.maxC,6)).toFixed(1)}`},{label:'Dọn bãi đỗ',value:`${s.revOk}/${s.rev}`}],
   details:s.prog.map((progress,index)=>`Phiên ${index+1}: hoàn thành ${Math.round(progress)}% việc chính`)
 });
 $('en').innerHTML=`<h2>Kết thúc 3 phiên</h2><div class="card" style="text-align:center"><div class="grade">${g}</div><b>${sc} điểm</b>${nb?' (kỷ lục mới!)':''}</div>
 <div class="grid"><div><b>${Math.round(acc*100)}%</b>quyết định đúng</div><div><b>${Math.round(ap)}%</b>việc chính hoàn thành</div><div><b>x${(1+.5*Math.min(s.maxC,6)).toFixed(1)}</b>hệ số cao nhất</div><div><b>${s.revOk}/${s.rev}</b>dọn bãi đỗ đúng</div></div>
 <div class="card" style="margin-top:12px"><b>Điều cần cải thiện</b><br>${tip()}<br><br><b>Rút ra</b><br>• Ghi lại rồi xử lý sau giúp giữ ý tưởng mà không cắt mạch.<br>• Mỗi thông báo bỏ mặc là một “tab mở” trong đầu, rút dần khiên của bạn.<br>• Chỉ việc khẩn thật mới đáng để chuyển ngữ cảnh.</div>
 <button class="btn pri" id="again">Chơi lại</button>`;show('en');$('again').onclick=newGame}
function warn(){const l=s.parked.length-s.reviewed;$('bw').textContent=l>0?'Còn '+l+' việc chưa dọn. Bỏ qua sẽ bị trừ 10 điểm mỗi việc.':''}
function tip(){const T={'noise.now':'Bạn hay mở ngay những thứ vô bổ. Quy tắc nhanh: nếu không ai cần bạn trả lời trong 1 giờ tới, hãy chặn.','idea.now':'Ý tưởng chợt đến hay kéo bạn bỏ dở việc chính. Hãy “đổ” nó vào ghi chú nhanh rồi quay lại.','urgent.miss':'Bạn đã bỏ lỡ vài việc khẩn thật. Tự hỏi: có hạn chót hoặc hậu quả trong vài giờ tới không? Nếu có, hãy xử lý ngay.','expire':'Bạn hay để thông báo trôi qua mà không quyết định. Quyết nhanh còn hơn để nó lơ lửng trong đầu.'};
 const e=Object.entries(s.err).sort((a,b)=>b[1]-a[1])[0];return e&&T[e[0]]?T[e[0]]:'Bạn gần như không mắc lỗi nào. Hãy chơi lại để phá kỷ lục!'}
function pauseToggle(){if(!s||!(run||paused)||s.cd>0)return;paused=!paused;
 if(paused){run=false;cancelAnimationFrame(raf);say('⏸ Đã tạm dừng. Bấm ▶ hoặc phím P để chơi tiếp.')}else{run=true;last=performance.now();raf=requestAnimationFrame(tick);say('Tiếp tục, giữ vững khiên!')}
 $('pb2').textContent=paused?'▶':'⏸';$('tray').classList.toggle('hide',paused)}
$('pb2').onclick=pauseToggle;
$('go').onclick=newGame;$('nx').onclick=finish;
addEventListener('keydown',e=>{const a={1:'block',2:'park',3:'now'}[e.key];if(a&&run)act(s.tray[0],a);if(e.key==='p'||e.key==='P')pauseToggle()});
show('st');
window.SoftSyncGameHistory?.mount({gameKey:'focus-fortress',storageKey:'softsync.focus-fortress.history.v1',title:'Focus Fortress'});
