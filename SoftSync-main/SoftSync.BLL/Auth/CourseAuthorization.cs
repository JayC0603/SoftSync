namespace SoftSync.BLL.Auth;

/// <summary>
/// Stable role and policy names for the Course module. Identity role membership,
/// not the editable profile role field, is the authorization authority.
/// </summary>
public static class CourseAuthorization
{
    public const string UserRole = "User";
    public const string LegacyStudentRole = "Student";
    public const string TeacherRole = "Teacher";
    public const string AdminRole = "Admin";

    public const string UserPolicy = "CourseUser";
    public const string TeacherPolicy = "CourseTeacher";
    public const string AdminPolicy = "CourseAdmin";

    public static readonly string[] Roles = [UserRole, TeacherRole, AdminRole];

    public static bool CanManageTeacherRoles(bool callerIsAdmin, int callerUserId, int targetUserId) =>
        callerIsAdmin && callerUserId > 0 && targetUserId > 0 && callerUserId != targetUserId;

    /// <summary>Admin may manage every item; a teacher may manage only their own item.</summary>
    public static bool CanManageOwnedContent(int authenticatedUserId, int creatorUserId, bool isAdmin) =>
        authenticatedUserId > 0 && (isAdmin || authenticatedUserId == creatorUserId);

    /// <summary>User-owned attempt/result data is never accessible through a route id alone.</summary>
    public static bool CanAccessOwnedResult(int authenticatedUserId, int ownerUserId) =>
        authenticatedUserId > 0 && authenticatedUserId == ownerUserId;
}
