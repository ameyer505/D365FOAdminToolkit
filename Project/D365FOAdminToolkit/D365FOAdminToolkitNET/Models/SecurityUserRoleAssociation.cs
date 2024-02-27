namespace D365FOAdminToolkitNET.Models
{
    public class SecurityUserRoleAssociation
    {
        public RoleAssignmentMode AssignmentMode { get; set; }
        public RoleAssignmentStatus AssignmentStatus { get; set; }
        public string SecurityRoleIdentifier { get; set; }
        public string SecurityRoleName { get; set; }
        public string UserId { get; set; }
    }

    public enum RoleAssignmentMode
    {
        None = 0,
        Manual = 1,
        Automatic = 2
    }

    public enum RoleAssignmentStatus
    {
        None = 0,
        Enabled = 1,
        Disabled = 2
    }
}
