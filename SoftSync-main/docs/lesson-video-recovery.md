# Lesson video playback recovery

On 2026-10-28, lesson requests on `softsync-tech.vercel.app` redirected to a Vercel Blob store returning HTTP 403 with `Your store is blocked`. The manifest also omitted time-management lesson 4.

The video manifest now contains all eight original lesson files at `https://softsync.id.vn/videos/`. At verification time this hostname serves the existing media server, independently of the Vercel application. All eight files match the original Git LFS file lengths, use H.264/AAC, return HTTP 206 for byte ranges, and play and seek in Edge.

The application keeps its existing `/videos/{fileName}` URLs, supports GET and HEAD, and redirects only to an allowlisted HTTPS origin. Unknown files return 404. Playback completion and learning-progress behavior are unchanged.

Keep this media origin available. Before moving `softsync.id.vn` traffic to Vercel, move the media to a separate working origin and update the manifest and endpoint allowlist; pointing the source back to the same video redirect would create a loop. The blocked Blob store itself has not been unblocked.

Validation: presentation build; HTTP checks of all eight routes, HEAD, MP4 signatures, original file lengths, initial and seek byte ranges; real browser metadata, playback and seek for all eight videos.
