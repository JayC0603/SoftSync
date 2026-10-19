# Learn V4 verification

The V4 view applies only at widths up to 900px. The existing desktop markup, components, stylesheet and resume behavior remain in place above that breakpoint.

Changed files:
- `SoftSync.Presentation/Components/Pages/RoadmapOverview.razor`: mobile hero asset, game map component and correct locked-level next-activity message.
- `SoftSync.Presentation/Components/Journey/MobileLearnMap.razor`: interactive checkpoints, curved SVG trail, platforms, BOSS trophy, localized states and data-dependent stage count.
- `SoftSync.Presentation/wwwroot/css/learn-v3.css`: replaces accumulated overrides with mobile-scoped V4 styling.
- `SoftSync.Presentation/wwwroot/images/learn-v4/`: hero-syncy.png and roadmap-background.webp copied from the supplied package.

Validation:
- Presentation build: passed, zero errors; six existing warnings outside the changed files.
- JourneyProgressTests and LearningWorkflowFixTests: 8 passed.
- Browser checks: light/dark at 320, 360, 375, 390, 414, 430, 768, 900, 901 and 1024px; desktop/mobile visibility, no horizontal overflow or clipped row content.
- All real demo skill groups: checkpoint counts/states retained; locked checkpoints disabled; stage labels do not overlap at 320, 390 and 430px.
- My learning / Explore, empty search, saved progress after navigation and matching hero/next-activity routes passed.
- Screenshots: `screenshots/learn-v4-light-390.png`, `screenshots/learn-v4-light-430.png`, dark equivalents, `screenshots/learn-v4-second-skill.png`, and full composition images.

Visual limitations:
- The supplied package's hero mascot and clean roadmap background differ from the reference illustration. README.md in the package explicitly identifies the landscape as a fallback. Exact illustration fidelity requires the original clean design layers; the reference images containing text/checkpoints are not used as runtime UI.
- Titles, progress, stage counts, avatar and available actions follow real application data, so they can differ from the example 4/6 time-management course shown in the reference.
- The current RoadmapItemDto/JourneyLesson contracts do not expose durations. Example durations are not fabricated.
- Device status-bar graphics are supplied by the device/browser, not drawn into the webpage. Screenshots use Edge mobile viewports; physical Safari browser chrome was not tested.
