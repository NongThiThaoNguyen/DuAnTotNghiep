namespace DuAnTotNghiep.Models.Enums
{
    public enum RoleCode
    {
        ADMIN,
        TEACHER,
        STUDENT
    }

    public enum UserStatus
    {
        ACTIVE,
        PENDING,
        LOCKED,
        DELETED
    }

    public enum AuditAction
    {
        REGISTER,
        LOGIN,
        LOGOUT,
        LOCK_USER,
        UNLOCK_USER,
        RESET_PASSWORD,
        CHANGE_PASSWORD,
        CHANGE_ROLE,
        UPDATE_PROFILE
    }
}
