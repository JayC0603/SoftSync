# Gamified Learn journey

The authenticated `/roadmap` and `/roadmap/{UserId}` routes now show an illustrated current-level hero, compact activity rows, and a responsive SVG map. The existing application shell and lecture routes are reused.

## Files

- `SoftSync.Presentation/Components/Pages/RoadmapOverview.razor`: authenticated loading, skill selection, persisted progress, guarded navigation, empty/error states.
- `SoftSync.Presentation/Components/Journey/CurrentLevelHero.razor`: current lesson, real activity progress, primary learning action, illustration and existing SYNCY mascot.
- `SoftSync.Presentation/Components/Journey/CurrentLevelLessons.razor`: six existing learning activities with review/continue/locked actions.
- `SoftSync.Presentation/Components/Journey/SkillRoadmap.razor`: accessible HTML checkpoint controls, selection, unlock explanations and legend.
- `SoftSync.Presentation/Components/Journey/RoadmapScene.razor`: scalable SVG road, scenery, islands, illuminated completed segments and summit trophy.
- `SoftSync.Presentation/Components/Journey/JourneyModels.cs`: themes, shared coordinate system, frontend projection of the lecture page's existing prerequisite rules.
- `SoftSync.Presentation/wwwroot/css/learning-journey.css`: Learn-scoped tokens, layout, focus, animation and responsive styles. A Learn-only mobile compatibility rule prevents legacy desktop nav styles from exposing the sidebar links above the mobile header.
- `SoftSync.Presentation/wwwroot/images/journey/conversation.jpg` and `valley.jpg`: generated illustration assets, encoded as JPEG, approximately 565 KB combined. The hero and scenery contain no UI; the actual text, progression path and controls are rendered independently. Other skills use symbolic hero artwork and themed SVG landmarks.
- `SoftSync.Tests/JourneyProgressTests.cs` and `SoftSync.Tests/SoftSync.Tests.csproj`: frontend prerequisite/resume and geometry tests, linking the presentation-only model into the existing test project.

## Data and progression

Only real `IRoadmapService` items are shown. Titles and objectives come from those items; existing translations are reused for known catalog titles. Unknown skill categories remain separate. No backend records, schemas, APIs or authentication changes were introduced.

The first incomplete item is the active level, preserving the previous overview ordering. Completed levels are revisitable. Future levels explain their unlock prerequisite and cannot navigate through the action handlers. The optional existing content review mode is still respected, without fabricating completion.

Activities use the same persisted flags/timestamps and prerequisite checks as `RoadmapLecture`. The main action resumes an accessible saved step, falling back to the next accessible incomplete step. Counts alone cannot unlock activities.

The gold **Boss** destination opens step 6 of the final real item: reflection and AI feedback. It is an existing learning activity presented as the destination, not a newly invented assessment, reward or backend challenge.

Communication, time management and critical thinking can be selected when returned by the service. Teamwork has a prepared theme but no fabricated course is exposed. Themes change colors, motifs and landmarks (clock tower, reasoning labyrinth, collaboration bridge).

## Validation

- `dotnet build SoftSync.Presentation/SoftSync.Presentation.csproj --no-restore --verbosity quiet`: successful, with six pre-existing warnings in unrelated files.
- `dotnet test SoftSync.Tests/SoftSync.Tests.csproj --no-restore --verbosity quiet`: 58 passed, including four new journey tests.
- Vite's existing build target ran as part of the frontend build. The package defines no lint or separate TypeScript check; this frontend is Blazor/C#.
- Actual headless Edge verification on the existing Development demo profile with an in-memory database: login, authenticated Learn render, locked-checkpoint explanation, lecture navigation, video completion persisted as `1/6`, returning to Learn, and resuming Script at `?step=2`.
- Switched between all three real demo categories and verified accents and checkpoint data. Checked reduced-motion animation suppression.
- Existing sidebar destinations `/dashboard`, `/roadmap`, `/progress`, `/resources`, `/game`, `/assistant` all returned HTTP 200 in the authenticated smoke check.
- Checked widths 1600, 1366, 1024, 768, 390 and 320: no horizontal overflow, checkpoint labels stay inside the map, hero/map/lessons stack on small screens. Screenshots were captured and visually inspected; the mobile header compatibility issue found in the first capture was corrected.
- Hero, mascot and scenery requests loaded. No application console exceptions were observed; Edge's built-in translation extension emitted unrelated extension warnings during category switches.

## Limits

XP rewards, estimated durations and streaks are omitted because the roadmap DTO does not supply authoritative values. The number of levels reflects the real personalized roadmap, so it may differ from the five-stage reference.

No new curriculum, completion rules or challenge backend was created. Existing unavailable lesson content remains unavailable. The full AI-evaluated quiz/practice/reflection sequence was not exercised end to end; the existing lecture implementation is unchanged. Teamwork's visual theme is prepared but cannot be browser-tested against production curriculum until actual teamwork items are supplied.

The approved composition is implemented with new illustration assets and scalable SVG, rather than pixel-identical artwork or a static screenshot. Desktop and mobile captures are in `docs/screenshots/learn-journey-*.png`.
