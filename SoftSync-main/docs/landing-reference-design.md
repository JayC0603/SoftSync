# SoftSync reference landing page

The `/` route uses one responsive landing page at all viewport sizes. Its design follows the supplied blue mountain-and-lake reference: a translucent public header, a phone preview, a three-column introduction strip, four benefits, a four-step journey, four feature previews, mentors/community, and a final registration invitation.

`PublicLayout` still renders the existing `AppFooter` / `MobileSiteFooter`. Their source and footer styles were not changed. The landing stylesheet is scoped to `.ss-reference-home`; the shared application density rules do not determine its typography.

All primary and feature links use existing routes. Registration stays on static SSR; an authenticated visitor's primary action leads to `/dashboard`. VI/EN switching, theme switching, keyboard focus, mobile navigation, and reduced-motion preferences remain supported. The interface previews are illustrative, not a visitor's live progress. Unverified adoption and satisfaction figures from the reference were replaced with descriptions of the learning experience. Mentor names and images use the existing project content.

## Background asset

Generated with the built-in imagegen tool. Project asset: `SoftSync.Presentation/wwwroot/images/landing/alpine-lake.webp` (320,296 bytes). The generated PNG was encoded as WebP at quality 0.84 without changing the composition. Other illustrations and mentor portraits are existing project assets.

Final generation prompt:

> Create a wide 1536x1024 landscape background illustration for SoftSync educational website hero. Match a polished bright blue storybook 3D painterly alpine lake: crystal turquoise lake in lower middle, beautiful tall blue and pale turquoise mountain peaks in distance, pine trees and lush green foliage on far left and right edges, soft white fluffy clouds, pale airy blue sky occupying top half. Morning sunshine soft luminous clean inviting. Open central and left-center area for overlaying dark navy website headline, no text, no letters, no phones, no devices, no people, no buildings, no logos, no UI. Exquisite detailed illustration like a premium learning adventure app, mainly ice blue, sky blue, turquoise, fresh green. Full bleed horizontal landscape.

## Validation

- `dotnet build SoftSync-main/SoftSync.Presentation/SoftSync.Presentation.csproj --no-restore`
- `node tests/landing-reference.browser.mjs` against the demo profile and Chromium CDP port 9466.
- The browser check covers mobile/tablet/desktop layout, horizontal overflow, image loads, one preserved footer, language switching, theme switching, the mobile menu, and sign-in redirection for protected features. Full-page screenshots are saved outside the repository under `.integration-tools/landing-reference/`.
