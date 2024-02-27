namespace D365FOAdminToolkitNET.Models
{
    public class SecurityUserRoleOrganization
    {
        public string HierarchyType { get; set; }
        public OMOperatingUnitType OperatingUnitType { get; set; }
        public string OrganizationId { get; set; }
        public OMInternalOrganizationType OrganizationType { get; set; }
        public string SecurityRoleIdentifier { get; set; }
        public string UserId { get; set; }
    }

    public enum OMInternalOrganizationType
    {
        None = 0,
        LegalEntity = 1,
        OperatingUnit = 2,
        Team = 3
    }

    public enum OMOperatingUnitType
    {
        None = 0,
        OMDepartment = 1,
        OMCostCenter = 2,
        OMValueStream = 3,
        OMBusinessUnit = 4,
        OMAnyOU = 5,
        RetailChannel = 6,
        OMBranch = 20,
        OMRentalLocation = 21,
        OMRegion = 22,
    }
}
