# Branch integration — 2026-11-09

The existing `master` at `22e13bb` is the integration base. All remote branches
available after `git fetch origin --prune` are ancestors of the resulting local
`master`.

## Integrated branches

- `origin/review/softsync-merged-20261029` (`955bf59`), merged in `d5a13c0`:
  CV extraction/OCR, grounded CV review and history, provider fallback, account
  registration, assessment resume, settings, schedule and responsive learning.
- `origin/review/softsync-ui-polish-20261107` (`1b4d668`), merged in `a791e3d`:
  desktop workspace sizing, account sizing, responsive corrections, motion
  lifecycle/accessibility and video-production documents.
- The other 13 remote branches were already contained in the original `master`.

## Conflict decisions

- Keep the current mobile More navigation and back link.
- Keep all six mini games and their history endpoints: Eisenhower Matrix,
  Focus Fortress, Inbox Rush, Day Tetris, Luật Ẩn and Voice Practice.
- Incorporate the new desktop CSS and motion JavaScript, including reduced
  motion and keyboard handling. Retain the current full-width admin layout.
- Keep existing deployment files, browser checks and source assets. Do not
  reintroduce Visual Studio caches, `bin`/`obj`, user settings or build logs.
- Preserve the 12 pre-existing modified/untracked files. The root
  `.vercelignore` combines the local `.tools` exclusion with the merged Python
  cache exclusions. Other saved contents match after normalizing line endings.

## Validation

- .NET 10.0.403 build and Vite production build completed.
- .NET tests: 403 passed, 14 skipped, 0 failed. Skipped tests require PostgreSQL,
  live AI providers or additional acceptance-test configuration.
- JavaScript motion and reconnect tests: 14 passed, 0 failed.
- All 12 changed CSS files parsed with Lightning CSS without errors or warnings.
- Chrome with an ephemeral DemoMode database: completed the initial assessment,
  then checked dashboard, roadmap, courses, schedule, CV review, settings, games,
  More and assistant at 390, 768, 1024 and 1440px (36 page/viewport checks).
  No browser JavaScript errors or horizontal page overflow occurred. Verified
  the six game links, seven Settings sections, changing to Appearance and the
  mobile More → Courses → More navigation.
- `git diff --check` passed; no remote branch remains unmerged into local master.

The older `Scripts/check-ui-css.mjs` check parses the learning CSS successfully
but its imported roadmap check expects an inline `<style>` that the current
`RoadmapOverview.razor` no longer contains. Neither file changed in these merges;
the changed CSS was validated independently as described above.

Local test artifacts and screenshots are under
`.tools/merge-validation-20261109/`. Original edits are also copied under
`.tools/merge-backup-20261109/` and retained in a Git stash. The original master
is saved as `backup/master-before-merge-20261109`. No remote push or deployment
was performed; production database migrations and live AI/OCR calls were not run.
