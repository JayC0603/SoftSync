# SoftSync reference landing page

The `/` route uses one responsive landing page at all viewport sizes. Its design follows the supplied blue mountain-and-lake reference: a translucent public header, a phone preview, a three-column introduction strip, four benefits, a four-step journey, four feature previews, mentors/community, and a final registration invitation.

`PublicLayout` still renders the existing `AppFooter` / `MobileSiteFooter`. Their source and footer styles were not changed. The landing stylesheet is scoped to `.ss-reference-home`; the shared application density rules do not determine its typography.

All primary and feature links use existing routes. Registration stays on static SSR; an authenticated visitor's primary action leads to `/dashboard`. VI/EN switching, theme switching, keyboard focus, and reduced-motion preferences remain supported. On the landing page, the hamburger is a reserved icon with no menu content, as requested. Signed-in visitors see the existing MainLayout taskbar and header; their source and styles are unchanged. The interface previews are illustrative, not a visitor's live progress. The three statistic labels reproduce the figures explicitly supplied in the approved reference: 10.000+, 500+, and 95%. These are supplied design copy, not live analytics. Mentor names and images use the existing project content.

## Background asset

Generated with the built-in imagegen tool. Project asset: `SoftSync.Presentation/wwwroot/images/landing/alpine-lake-reference-v2.webp` (136,852 bytes). The generated PNG was encoded as WebP at quality 0.84 without changing the composition. Other illustrations and mentor portraits are existing project assets.

Final generation prompt:

> Use the attached SoftSync landing page screenshot ONLY as a reference for its painted landscape background. Create just the background artwork, with no interface, no words, no letters, no UI, no phones, no logos, no people. Very closely match the reference's stylized illustrated blue alpine lake, pale blue snow-capped mountains, lush mint and emerald evergreen trees along both sides, fluffy white soft clouds and a tranquil turquoise lake across the center. This must be a soft hand-painted 3D cartoon storybook illustration, NOT a realistic photograph. Light airy pastel sky-blue palette, gentle diffused white morning light, smooth simplified painterly shapes, bright educational app adventure art. Foreground white rounded cloud banks along the lower corners and edges. Left central region light with relatively sparse detail for website headline overlay. Wide landscape 1536x1024.

## Validation

- `dotnet build SoftSync-main/SoftSync.Presentation/SoftSync.Presentation.csproj --no-restore`
- `node tests/landing-reference.browser.mjs` against the demo profile and Chromium CDP port 9466.
- The browser check covers mobile/tablet/desktop layout, horizontal overflow, image loads, one preserved footer, language switching, theme switching, the reserved hamburger, sign-in redirection for protected features, and the original taskbar on the authenticated landing page. Full-page screenshots are saved outside the repository under `.integration-tools/landing-reference/`.
