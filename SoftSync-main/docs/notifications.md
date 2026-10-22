# Admin announcements

- Sign in with an Identity `Admin` account and open **Thông báo** in the admin sidebar (`/admin/notifications`). Enter a title and message, then select **Gửi cho tất cả**.
- A send saves one announcement and a recipient record for every account that exists at send time, including offline users. New accounts do not receive earlier broadcasts.
- Signed-in users see a bell in the workspace and public header. The bell shows five recent messages; `/notifications` has the complete paginated inbox, individual read actions and **Mark all as read**.
- Publishing and reading update connected circuits on the same application instance immediately. The bell also checks the database every 30 seconds to cover other instances and read actions from other devices.
- Read state is stored per recipient. Only the authenticated recipient can update it. Publishing requires both the Admin claim and current Identity role membership.
- Title and message limits are 160 and 4,000 characters. Message content renders as plain text, preserving line breaks.

## Database and deployment

The `AddAdminAnnouncements` EF migration creates `Announcements` and `UserNotifications`, with foreign keys and inbox indexes. Existing startup migration handling applies it when the production app starts. Development demo mode uses the existing ephemeral in-memory database; its notifications reset when that demo server restarts.

This feature sends in-app notifications. It does not send email or browser push messages.

## Validation

Run `dotnet test SoftSync.Tests/SoftSync.Tests.csproj`. Announcement tests cover admin access and revocation, recipient isolation, persistence through a fresh context, validation, pagination, mark-all, and disconnected subscriber handling.
