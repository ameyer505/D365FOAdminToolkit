namespace D365FOAdminToolkitNET.Models
{
    public class EntUser
    {
        public bool AccountEnabled { get; set; }
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public string OnPremisesUserPrincipalName { get; set; }
        public string UserPrincipalName { get; set; }
    }
}
