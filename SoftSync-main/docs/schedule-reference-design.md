# Schedule reference update

The `/schedule` content follows the supplied blue calendar reference. Shared
navigation, header and footer files are unchanged. Month view uses seven columns
on phones; dots represent actual saved tasks. Selecting a populated day reveals
its agenda on mobile. Existing create/edit/complete/delete operations are retained.

The hero asset is `SoftSync.Presentation/wwwroot/images/backgrounds/schedule-blue-calendar.png`,
generated with the built-in imagegen tool. Original generation prompt:

> Use case: stylized-concept. Create a polished wide 2:1 landscape website hero background illustration for a personal calendar page. Soft luminous baby blue sky, delicate fluffy clouds, distant pastel blue mountain silhouettes and small soft green leaves at bottom edges. On the RIGHT THIRD, large charming 3D blue standing desk flip calendar with three blue binding rings, white calendar page, faint blue square cells and one vibrant blue cell with white checkmark, with a small round blue analog clock in front at the base. Rounded friendly clay-render aesthetic, gentle daylight, clean high quality, matching a premium learning productivity app. LEFT 60 PERCENT mostly empty pale ice blue sky with very subtle mountains low down to allow dark navy headline and paragraph overlay. Keep the calendar compact on the right, all objects fully in frame. Bottom blends to almost white pale blue. No words, no lettering, no typography, no logo, no numbers. This is an image asset only, not a website screenshot.

Validation: .NET build; browser checks at widths 320, 390, 430, 768, and 1440;
light/dark themes; month navigation; week view; creating, editing, completing,
reloading and deleting a task using the demo database.
