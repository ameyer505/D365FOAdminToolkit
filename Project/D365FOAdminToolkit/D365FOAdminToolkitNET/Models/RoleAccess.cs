namespace D365FOAdminToolkitNET.Models
{
    public class RoleAccess
    {
        public string RoleIdentifier { get; set; }
        public string RoleName { get; set; }
        public string SubRoleIdentifer { get; set; }
        public string SubRoleName { get; set; }
        public string DutyIdentifier { get; set; }
        public string DutyName { get; set; }
        public string PrivilegeIdentifier { get; set; }
        public string PrivilegeName { get; set; }
        public string SecurableObject { get; set; }
        public string SecurableType { get; set; }
        public string Invoke { get; set; }
        public string Read { get; set; }
        public string Update { get; set; }
        public string Create { get; set; }
        public string Delete { get; set; }
        public string ComputedLicense { get; set; }
    }
}
