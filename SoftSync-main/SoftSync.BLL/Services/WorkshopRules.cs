using SoftSync.Common.Enums;

namespace SoftSync.BLL.Services;

public static class WorkshopRules
{
    public static bool CanRegister(WorkshopStatus status, int currentParticipants, int capacity) =>
        status == WorkshopStatus.Published && capacity > 0 && currentParticipants < capacity;

    public static bool CanComplete(WorkshopEnrollmentStatus enrollmentStatus) =>
        enrollmentStatus is WorkshopEnrollmentStatus.Attended or WorkshopEnrollmentStatus.Completed;

    public static bool CanCreateEvidence(WorkshopStatus workshopStatus, WorkshopEnrollmentStatus enrollmentStatus) =>
        workshopStatus == WorkshopStatus.Completed && enrollmentStatus == WorkshopEnrollmentStatus.Completed;

    public static bool CanManage(int authenticatedUserId, int creatorUserId, bool isAdmin) =>
        authenticatedUserId > 0 && (isAdmin || authenticatedUserId == creatorUserId);
}
