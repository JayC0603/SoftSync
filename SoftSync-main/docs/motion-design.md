# SoftSync interface motion and background

Design: preserve existing typography and purple/blue identity. Porcelain reading surfaces, indigo workspace banner, lavender architectural background. The generated scene is decorative and contains no text. Analytics stay stationary during automatic refreshes.

Installed skill: https://github.com/emilkowalski/skills/tree/main/skills/animate
Local installation: C:/Users/phing/.codex/skills/animate
Also applied existing frontend-design and clear-ui-design guidance.

Buttons use 160 ms transform transitions for feedback. Page entrances use WAAPI opacity/transform, 240 ms, cubic-bezier(.23,1,.32,1), without delaying links or forms. Keyboard input, OS reduced motion and the app's reduced-motion preference cancel motion. Hover movement requires a fine pointer. No animation library was added.

Shared assets are copied between Presentation/wwwroot and Next/public (CSS lives in Next/app). Keep those copies identical when editing. Next's PageMotion signals client navigation; Blazor observes route/content replacement. Chart refreshes do not replay the entrance.

## Generated image

Mode: built-in imagegen. Original generated image was converted to WebP (quality 84) using Pillow: 1774 × 887, 98,056 bytes.

Assets:
- SoftSync.Presentation/wwwroot/images/backgrounds/softsync-learning-atrium.webp
- SoftSync.Next/public/images/backgrounds/softsync-learning-atrium.webp

Prompt:
> Use case: stylized-concept. Asset type: wide website background image for SoftSync, a Vietnamese soft-skills learning platform and its admin workspace. Create a sophisticated editorial architectural 3D scene: a sunlit contemporary learning atrium with sweeping curved white plaster walls, tiered stair-like platforms suggesting personal growth, frosted translucent violet glass partitions, a few subtle pale blue accents. No people, no furniture clutter, no typography, no logo, no user interface. Wide landscape composition roughly 2:1: calm uncluttered softly illuminated space across the left half for an overlay headline; the distinctive sculptural stairs and glass architecture mostly on the right half. Palette porcelain white, pale periwinkle, lavender, deep indigo accents, daylight blue. Matte mineral surfaces, gentle architectural shadows, high-end realistic materials, atmospheric depth, restrained color. Attractive as a cropped website banner and faint page backdrop, crisp architectural detail with no busy visual texture. Render a finished bitmap image.
