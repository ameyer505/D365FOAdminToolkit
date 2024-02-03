namespace D365FOAdminToolkitNET.Models
{
    public class D365FOUser
    {
            public UserAccountType AccountType { get; set; }
            public string Alias { get; set; }
            public string Company { get; set; }
            public string Email { get; set; }
            public bool Enabled { get; set; }
            public bool ExternalUser { get; set; }
            public string Language { get; set; }
            public string NetworkDomain { get; set; }
            public string PersonName { get; set; }
            public string UserID { get; set; }
            public string UserName { get; set; }
            public string UserInfo_language { get; set; }
            public string Helplanguage { get; set; }
            public string SID { get; set; }
    }

    public enum UserAccountType
    {
        ADUser = 0,
        ADGroup = 1,
        ClaimsUser = 2,
        ClaimsGroup = 3
    }
}
