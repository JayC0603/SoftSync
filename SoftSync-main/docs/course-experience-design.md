# Course experience design

## Visual direction

The course catalog and student library use SoftSync's existing violet identity with warm ivory, ink and jade accents. Course titles lead the page; lesson count, progress and quiz state sit close to the action. Existing uploaded course thumbnails keep priority. A generated editorial learning scene gives courses without a thumbnail an intentional visual and anchors the catalog hero. The student view keeps lessons inside an accessible disclosure so the learner can scan progress before opening video and transcript content.

The Next.js public catalog/detail pages share `SoftSync.Next/app/course-experience.css`. The authenticated Blazor student library uses `SoftSync.Presentation/wwwroot/css/course-experience.css`. The artwork is copied into each application's public/static image folder; keep the copies in sync.

## Artwork

Mode: built-in imagegen, converted from 1536 × 1024 PNG to WebP (quality 83), 147,544 bytes.

Files:
- `SoftSync.Next/public/images/backgrounds/softsync-course-library.webp`
- `SoftSync.Presentation/wwwroot/images/backgrounds/softsync-course-library.webp`

Prompt:
> Create a premium editorial 3D illustration to anchor the course discovery experience for SoftSync, a Vietnamese learning platform. Landscape 3:2 composition, no words, no letters, no logos, no interface mockups, no people. A sculptural learning studio: elegant stacked book forms transform into ascending architectural steps, a small open book, abstract glass skill tiles and an illuminated mint-green route line moving toward a softly glowing arch. Arrange all meaningful objects in the right half, with a quiet hazy lavender and warm ivory field on the left so the image can crop into a website header. Palette deep indigo, porcelain, dusty lilac, muted jade and a restrained apricot accent. Sophisticated studio lighting, tactile paper, translucent glass, matte ceramic, long soft shadows, controlled negative space, contemporary art direction for a high-end education product, polished and distinctive, subtle depth, visually rich but calm.
