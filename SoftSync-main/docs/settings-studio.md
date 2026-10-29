# Settings workspace

The `/settings` page groups preferences into seven sections: profile, security,
learning, notifications, appearance, privacy, and plan. Only the selected section
is rendered. Desktop has a side menu; mobile uses a horizontally scrolling menu
that brings the active section into view. Existing `?section=Appearance`,
`?section=Learning`, and other section links remain supported.

Profile and learning settings still save through the existing user service.
The save button shows whether anything has changed and prevents duplicate saves.
Language, theme, text size, motion, and contrast can be previewed immediately.
Text size and contrast remain device preferences. Account language, theme, and
motion preferences are persisted when the user saves.

Password changes use the existing Identity form endpoint and its validation.
The UI checks confirmation and supports showing passwords. Pause/delete actions
are behind an expandable section and confirmation dialogs, with a typed delete
confirmation. Dialogs trap keyboard focus, close with Escape, and restore focus
to the triggering control. No additional notification or billing functionality
is implied by the design; unavailable features are labeled explicitly.

## Local verification

Run the app with `SoftSync__DemoMode=true`,
`ASPNETCORE_ENVIRONMENT=Development`, and URL `http://127.0.0.1:5298`.
This uses an ephemeral in-memory database. Run a headless Chromium browser with
remote debugging on port 9498, then run from the repository root:

```powershell
dotnet test SoftSync-main/SoftSync.Tests/SoftSync.Tests.csproj --filter FullyQualifiedName~SettingsPersistenceTests
node tests/settings-studio.browser.mjs
```

The browser check uses only the local demo account. It checks profile and
learning persistence across reloads, password confirmation, keyboard handling,
all seven sections at 320/390/768/1440px, dark/light themes, large text,
contrast, reduced motion, and English. It writes screenshots and results to
`docs/screenshots/settings/`.

Preview images: [mobile profile](screenshots/settings/mobile-profile-light.png),
[desktop profile](screenshots/settings/desktop-profile-light.png),
[mobile dark appearance](screenshots/settings/mobile-appearance-dark.png).
