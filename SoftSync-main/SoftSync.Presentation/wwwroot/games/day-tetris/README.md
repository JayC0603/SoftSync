# Day Tetris (bản demo)

Game xếp lịch kiểu Tetris, viết bằng HTML5 Canvas + JavaScript thuần, không cần cài thư viện hay server.
Mỗi khối là một đầu việc, mỗi ô là một giờ, mỗi hàng là một ngày.

## Chạy game

- Cách nhanh nhất: mở file `dist/day-tetris.html` bằng trình duyệt (double-click). Đây là bản gộp 1 file, chạy offline
  (chỉ riêng font chữ lấy từ Google Fonts; không có mạng thì dùng font hệ thống, game vẫn chạy bình thường).
- Chạy từ mã nguồn: mở `index.html`, hoặc `npm start` rồi vào http://localhost:8080 (cần Python 3).
- Sau khi sửa code trong `js/` hoặc `css/`, chạy `npm run build` (cần Node 18+) để tạo lại `dist/`.

## Điều khiển

| Phím | Tác dụng |
|---|---|
| ← → (A D) | Di chuyển |
| ↑ / X, Z | Xoay thuận, xoay ngược |
| ↓ (S) | Rơi nhanh |
| Space | Thả ngay |
| C / Shift | Hoãn khối (cấp 4+) |
| R | Chèn khối nghỉ (cấp 3+, tối đa 2 lần/ngày) |
| B | Chèn ô đệm (cấp 4+, dùng chung 2 lượt với R) |
| 1 2 3 4 | Pomodoro, Lên danh sách, Ủy quyền, Ngày nghỉ thật |
| P / Esc | Tạm dừng |

Trên điện thoại có nút cảm ứng dưới bảng chơi.

## Đã làm so với bản thiết kế

| Mục thiết kế | Trạng thái |
|---|---|
| Lưới 10×14, hàng dưới cùng là hôm nay, đồng hồ ngày chốt hàng và dịch bảng xuống | Có |
| Khối I, O, T, L, J, S, Z, và các khối 1–3 ô; xoay đứng/ngang của I có nghĩa khác nhau | Có |
| Loại việc (màu), nhãn Eisenhower Q1–Q4, hạn chót là vạch trên bảng, ô trễ hạn bị sọc đỏ | Có |
| Mức năng lượng nhẹ/vừa/nặng (chấm cuối ô) | Có |
| Xem trước 3 khối; Lên danh sách xem 5 | Có |
| Hold: 1 lần/ngày, hoãn ≥ 2 ngày thì phình thêm 1 ô mỗi ngày và Q2 thành Q1 | Có |
| Xóa hàng: ngày cân bằng, quá tải, cày đêm (kèm trạng thái mệt mỏi) | Có (thêm "ngày thư giãn" chống lách luật) |
| Năng lượng (cạn thì khối rơi nhanh hơn, xoay đôi khi kẹt), Uy tín/GPA, Cân bằng cuộc sống, Điểm | Có |
| Bảng điểm gợi ý | Có, đúng các mức trong thiết kế |
| Sự kiện: việc đột xuất, bạn rủ đi chơi (hộp thoại chọn), ốm nhẹ, thầy cô dời hạn; ô đệm hấp thụ sự kiện | Có |
| 4 vật phẩm, mở khóa dần khi chơi tốt | Có |
| Thắng/thua: tràn backlog, uy tín về 0, kiệt sức 2 lần; thắng khi qua hết số ngày với uy tín ≥ 40 | Có |
| Chế độ: Chiến dịch 5 cấp, Tự do, Tùy chỉnh (nhập việc thật) | Có |
| Báo cáo cuối ván gắn với hành vi cụ thể và bài tập thực tế | Có |
| Lưu điểm cao, việc tùy chỉnh và giờ bắt đầu bằng localStorage | Có |

## Cách mình diễn giải những chỗ thiết kế chưa nói rõ

- **Ngày trôi**: khi một ngày kết thúc, hàng dưới cùng bị chốt (các ô đã xếp là việc đã làm) và cả bảng dịch xuống một nấc.
  Khối đang rơi mà không "đứng yên" được sau khi bảng dịch thì bị chốt luôn.
- **Hạn chót**: hạn của khối là một ngày cụ thể. Việc xong lúc nào tính theo ngày của hàng chứa ô cuối cùng khi nó bị xóa hoặc chốt.
  Xóa hàng ở dưới sẽ kéo các ô phía trên về sớm hơn một ngày. Quá hạn mà ô vẫn còn trên bảng thì bị phạt ngay khi hết hạn.
- **"Xếp Q3 mà bỏ lỡ Q2" (−30)**: tính khi khối Q3 được xếp thấp hơn (ngày sớm hơn) một việc Q2 đã có trên bảng, hoặc khi đang hoãn một việc Q2.
- **Ngày quá tải**: tính cả ô đệm là chỗ nghỉ. Hàng có từ 6 ô nghỉ/đệm trở lên là "ngày thư giãn" (40 điểm) để không thể thắng bằng cách chỉ xếp khối nghỉ.
- **Lịch bận (cấp 5)**: mỗi ngày mới chen 1–3 ô BẬN (cuối tuần 0–1) rơi xuống chạm chồng khối.
- **Chế độ Tùy chỉnh**: không có đồng hồ ngày và không thua vì kiệt sức/uy tín; xếp hết việc là kết thúc và nhận bảng nhận xét về lịch.

## Cấu trúc

```
index.html        khung trang (bản chạy từ mã nguồn)
css/style.css     giao diện
js/config.js      toàn bộ số liệu cân bằng: cấp độ, điểm, năng lượng, tên việc... (sửa ở đây để chỉnh độ khó)
js/game.js        lõi game, không phụ thuộc DOM
js/report.js      báo cáo cuối ván
js/render.js      vẽ canvas
js/main.js        menu, điều khiển, vòng lặp, HUD
tests/sim.js      bot tự chơi để kiểm tra lỗi và độ khó: node tests/sim.js 2500 10
tools/build.js    gộp thành dist/day-tetris.html
```

## Kiểm thử

`npm test` cho bot chơi mỗi cấp nhiều ván với hạt giống ngẫu nhiên khác nhau, phát hiện lỗi chạy và in tỉ lệ thắng, điểm, số lần trễ hạn.
Tham số: thời gian bot "suy nghĩ" mỗi khối (ms) và số ván mỗi cấp. Bot chơi tốt hơn người mới, nên người chơi thật sẽ thấy khó hơn.

## Hạn chế của bản demo

- Chưa có âm thanh, hiệu ứng xóa hàng và hướng dẫn tương tác từng bước (tutorial); mỗi cấp chỉ có màn giới thiệu cơ chế mới.
- Số liệu cân bằng (tốc độ, xác suất, điểm) mới chỉnh sơ bộ qua bot, chưa thử với người chơi thật.
- Tiến độ chiến dịch chưa khóa cấp: cả 5 cấp đều chọn được ngay.
