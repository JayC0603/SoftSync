# Progress language regression review

Build `SoftSync.Presentation`, then run from the solution directory:

```powershell
dotnet run --project docs/progress-localization-review/Review.csproj
```

For an isolated application build, pass its absolute output path with
`-p:PresentationOutput=C:/path/to/build`.

The review renders the production dashboard, chart and shared footer, switches
Vietnamese → English → Vietnamese on the same component instances, and checks
labels, chart tooltips, accessibility text, date/select controls, report text,
number formatting, empty state and preservation of filter/report state.
It needs no database or signed-in browser session. Lesson content in the fixture
is deliberately English so that leftover Vietnamese UI can be detected.
