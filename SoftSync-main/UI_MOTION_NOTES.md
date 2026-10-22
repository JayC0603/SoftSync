# SoftSync — Depth & Motion

## Motion system

`wwwroot/css/learning-platform.css` dùng tokens chung:

- `--ss-elevation-0`: flat; `1`: card; `2`: hover/focus; `3`: menu/sticky action; `4`: dialog.
- Shadow hai lớp, alpha thấp; không thêm blur layer hoặc `will-change` toàn trang.
- `--ss-motion-fast`: 120ms; `normal`: 180ms; `slow`: 240ms.
- `--ss-ease-standard`: cubic-bezier(.2,0,0,1); `--ss-ease-enter`: cubic-bezier(.16,1,.3,1).

## Components thực sự được áp dụng

- Course cards: elevation 1, hover nâng 2px, focus-within giữ border/shadow mà không dịch chuyển. Không zoom video/thumbnail.
- Dashboard: continue/progress surfaces elevation 1, course buttons hover; header/progress/courses entrance 180ms với delay 0/40/80ms. Không animate root ứng dụng.
- Teacher: course selection buttons border/shadow; editor entrance chỉ khi đổi panel/course hoặc chọn New/Edit lesson/question, bằng `editorVersion` key. Bind input, radio selection, busy state không tăng key; không chủ động replay animation sau mỗi re-render.
- Teacher SaveButton dùng `saving` của operation Save, disabled khi operation đang chạy. Không tự công bố Saved. Inline thông báo có icon/chữ, xuất hiện sau kết quả service, không blocking success modal; không thêm toast timer/queue.
- Teacher action bars sticky, offset riêng trên mobile để chừa bottom navigation. Publish course yêu cầu save thay đổi trước để không vô tình bỏ input chưa lưu.
- Navbar active state, menu entrance 120ms; dialog Mentor Dashboard entrance 180ms, elevation 4. Khi đóng, nội dung loại bỏ ngay, không trì hoãn state vì animation.
- Progress fill transition 240ms; số/ARIA cập nhật ngay. Quiz selection chỉ phản ánh lựa chọn, không hiển thị đúng/sai trước submit.
- Transcript native details: fade phần text khi mở, không animate arbitrary fixed height.
- Focus border/ring rõ ràng; không shake validation.

## Reduced motion

Cả `prefers-reduced-motion: reduce` và preference `data-reduce-motion="1"` tắt animation/transition và hover transform trong workspace/navbar/account, gồm pseudo-elements. Border, selected radio, text/ARIA và trạng thái operation vẫn hiện. Spinner trở thành chỉ báo tĩnh kèm chữ.

## Performance và giới hạn

- Không thêm thư viện animation, polling, artificial delay, interop animation hay pointer listener.
- Entrance dùng transform/opacity; hover shadow chỉ ở card nhỏ. Width transition chỉ trên progress fill khi giá trị đổi, không animate layout trang liên tục.
- `Scripts/check-ui-css.mjs` kiểm tra syntax stylesheet riêng bằng parser sẵn trong Tailwind toolchain vì file này không nằm trong bundle Vite. Đây không phải đo FPS/layout shift/GPU.
- Không thêm skeleton, fake AI stages, fake CV percent, drag-and-drop hoặc off-canvas chưa có flow đáng tin. AI/CV vẫn dùng trạng thái processing thực có; không giả token streaming.
- Không có performance trace, screenshot hay xác minh tương tác browser. Đã thử browser skill, runtime không có browser: Desktop/Mobile/Teacher/Learner/AI loading/Reduced motion đều **NOT TESTED** trên browser.
- Cần kiểm tra sticky/focus không che input, hover/pressed/menu, dialog, progress, radio, save thất bại/thành công, New/Edit và reduced-motion ở 390/768/1440px. Không khẳng định không jank/replay chỉ từ compilation hoặc CSS parser.

Giữ nguyên services, ownership, scoring, persistence, AI provider và schema. Chưa rebuild container web của người dùng, không commit/push.
