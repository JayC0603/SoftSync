# Homepage reference assets

The inline reference images were visible in chat but had no downloadable file paths in the attachment directory. The five illustrations below were reconstructed with the built-in `imagegen` tool from the final full-page reference. They are not the user's original source files and should not be described as pixel-identical extractions.

Runtime files are in `SoftSync.Presentation/wwwroot/images/home/`:

| File | Reference region / final prompt |
| --- | --- |
| `syncy-study.webp` | Extract only the hero illustration: white blue headphone SYNCY sitting on books, holding a purple book, clock upper left, checklist upper right, lavender leaves and gold stars. Preserve composition and character, transparent background, no UI or text except the chest S. |
| `roadmap.webp` | Extract only the lower-right landscape: green hills, lavender mountain, winding yellow road, trees, pale clouds and purple summit flag. Preserve the reference composition and pastel 3D style; transparent background, no UI/text. |
| `syncy-assistant.webp` | Extract only the waving SYNCY from the assistant banner. Preserve face, blue headphones, chest S, lavender leaves and gold stars; transparent background, no UI/text. |
| `syncy-growth.webp` | Extract only the daily motivation art: cheerful SYNCY beside ascending purple bars and an upward arrow, stars, pastel leaves/clouds. Preserve composition; transparent background, no UI/text. |
| `welcome.webp` | Extract only the welcome decoration: pastel sun, pale clouds, lavender leaves. Preserve composition; transparent background, no robot, no UI/text. |

Original generated PNGs remain in the generator's output directory; optimized WebP copies preserve alpha. Hero is 640px wide, landscape and growth 480px, assistant and welcome 256px. The reference font is approximated using the existing application's Inter/system font stack. Small UI icons reuse Bootstrap Icons. Mentor names, portraits, specializations and descriptions come from `IMentorService`; no reference mentor or verified status was fabricated. Footer includes only existing project links.

Original source PNGs can replace these assets without changing component layout. Homepage markup and interaction remain real Blazor components, not a screenshot background.

## Validation

Four screenshot inspection and correction passes at 430px: removed nested input borders and excess container padding; corrected illustration scale and card spacing; restored the mentor, motivation and footer sections; reduced accidental overlap at narrow widths. Browser checks passed at 320, 360, 375, 390, 414, 430, 432, 480, 768 and 1024px in both light and dark modes, including real search results/empty/clear, resume-step navigation, mentor detail navigation, fixed header and visible footer. The .NET/Vite build passed with six existing C# warnings and no errors.

Remaining visual differences: reconstructed illustrations and Bootstrap UI icons are not exact copies of the original supplied assets; Inter/system typography may differ; actual user/mentor names, objectives and progress vary in length and can increase card heights. Only the project's existing social destinations are shown, rather than inventing YouTube or LinkedIn accounts.

The latest target-alignment pass corrected the overall page proportions at 430px: search 30px, hero 173px with real demo content, statistics 49px, roadmap 106px, assistant 64px, quick access 93px, mentors 91px and motivation 70px. The fixed header begins at the application viewport's top edge; operating-system status indicators are excluded. All ten viewport widths and navigation checks were rerun successfully after these adjustments.
