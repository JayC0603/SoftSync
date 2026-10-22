import type { UploadedFile, MenuType } from '../../../types/dashboard';
import { useRef } from 'react';
import FileCard from '../storage/FileCard';
import SavedCvPanel from '../storage/SavedCvPanel';

interface CvStorageSectionProps {
  files: UploadedFile[];
  loading: boolean;
  activeMenu: MenuType;
  savedCvsCount: number;
  onPreviewUpload: (e: React.ChangeEvent<HTMLInputElement>, category: 'CV' | 'JD') => void;
  onPreview: (file: UploadedFile) => void;
  onConfirmSave: (filename: string, category: string) => void;
  onRemove: (id: string) => void;
  onToggleSavedCvs: () => void;
  onNavigate: (menu: MenuType) => void;
}

export default function CvStorageSection({
  files,
  loading,
  activeMenu,
  savedCvsCount,
  onPreviewUpload,
  onPreview,
  onConfirmSave,
  onRemove,
  onToggleSavedCvs,
  onNavigate,
}: CvStorageSectionProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const workflow: { menu: MenuType; title: string; text: string; icon: string }[] = [
    { menu: 'CV-JD Matching', title: 'So khớp với công việc', text: 'Đối chiếu một CV với yêu cầu tuyển dụng và xem điểm phù hợp.', icon: '↗' },
    { menu: 'CV Ranking', title: 'Xếp hạng ứng viên', text: 'Phân tích nhiều CV để tìm hồ sơ phù hợp nhất với vị trí.', icon: '≡' },
    { menu: 'CV Comparison', title: 'So sánh các hồ sơ', text: 'Đặt các CV cạnh nhau để đánh giá kỹ năng và kinh nghiệm.', icon: '⇄' },
  ];
  return (
    <div className="storage-layout">
      <div className="storage-section">
        <div className="upload-zone">
          <input ref={inputRef} type="file" accept=".pdf,.txt" multiple disabled={loading} onChange={(e) => onPreviewUpload(e, 'CV')} id="cv-up" hidden />
          <div className="premium-upload-label" aria-busy={loading}>
            <div className="upload-icon-circle"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2"><path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4M17 8l-5-5-5 5M12 3v12" /></svg></div>
            <h3>{loading ? 'Đang tải CV…' : 'Bắt đầu với hồ sơ của bạn'}</h3>
            <p>Tải lên CV để xem trước, lưu vào kho và phân tích cùng các công cụ AI.</p>
            <button type="button" className="cv-upload-button" onClick={() => inputRef.current?.click()} disabled={loading}>{loading ? 'Đang xử lý…' : 'Chọn tệp CV'} <span aria-hidden="true">↑</span></button>
            <span className="cv-upload-hint">Hỗ trợ PDF và TXT · Có thể chọn nhiều tệp</span>
          </div>
        </div>
        <div className="file-grid">
          {files.filter(f => f.category === 'CV' && f.section === 'CV Storage').map(file => (
            <FileCard
              key={file.id}
              file={file}
              activeMenu={activeMenu}
              onPreview={onPreview}
              onConfirmSave={onConfirmSave}
              onRemove={onRemove}
            />
          ))}
        </div>
        <section className="cv-workflows" aria-labelledby="cv-workflow-title">
          <div className="cv-section-heading"><h2 id="cv-workflow-title">Bạn muốn làm gì tiếp theo?</h2><p>Chọn công cụ phù hợp với mục tiêu của bạn.</p></div>
          <div className="cv-workflow-grid">{workflow.map(item => <button type="button" className="cv-workflow-card" key={item.menu} onClick={() => onNavigate(item.menu)}><span className="cv-workflow-icon" aria-hidden="true">{item.icon}</span><strong>{item.title}</strong><span>{item.text}</span><span className="cv-workflow-cta">Mở công cụ <span aria-hidden="true">→</span></span></button>)}</div>
        </section>
      </div>
      <div className="storage-side"><SavedCvPanel count={savedCvsCount} onToggle={onToggleSavedCvs} />
        <section className="cv-start-guide" aria-labelledby="cv-guide-title"><span className="cv-guide-eyebrow">BẮT ĐẦU TRONG 3 BƯỚC</span><h2 id="cv-guide-title">Từ hồ sơ đến cơ hội</h2><ol><li><span>1</span><div><strong>Chọn tệp CV</strong><p>Dùng hồ sơ PDF hoặc TXT có nội dung rõ ràng.</p></div></li><li><span>2</span><div><strong>Xem trước và lưu</strong><p>Kiểm tra nội dung trước khi lưu hồ sơ vào kho.</p></div></li><li><span>3</span><div><strong>Phân tích cùng JD</strong><p>Thêm yêu cầu công việc để so khớp hoặc xếp hạng CV.</p></div></li></ol><button type="button" onClick={() => onNavigate('CV Searching')} className="cv-guide-link">Khám phá tìm kiếm CV →</button></section>
      </div>
    </div>
  );
}
