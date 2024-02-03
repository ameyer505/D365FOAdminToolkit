using CsvHelper;
using D365FOAdminToolkitNET.Clients;
using D365FOAdminToolkitNET.Models;
using Microsoft.Dynamics.AX.Security.Management;
using Microsoft.Dynamics.AX.Security.Management.Querying;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace D365FOAdminToolkitNET
{
    public static class AdminToolkitNET
    {
        public static MemoryStream GenerateRoleAccessCSV(SecurityRepository sr)
        {
            MemoryStream ms = new MemoryStream();
            using (StreamWriter sw = new StreamWriter(ms))
            {
                using (var csv = new CsvWriter(sw, CultureInfo.InvariantCulture, true))
                {
                    List<string> roleList = sr.Roles.ListObjects().ToList();
                    RelatedSecurityObjectsFinder rsof = new RelatedSecurityObjectsFinder(sr);

                    foreach (var role in roleList)
                    {
                        List<RoleAccess> accessList = new List<RoleAccess>();
                        Dictionary<string, ComputedLicense> rsol;
                        ISecurityRelatedObjectsData srod = rsof.FindRelatedSecurityObjectsForRolesWithLicenseInfo(new List<string>() { role }, out rsol);

                        foreach (var roleAccess in srod.RelatedObjects)
                        {
                            RoleAccess ra = new RoleAccess();
                            ra.RoleIdentifier = roleAccess.RoleIdentifier;
                            ra.RoleName = roleAccess.RoleName;
                            ra.SubRoleIdentifer = roleAccess.SubRoleIdentifier;
                            ra.SubRoleName = roleAccess.SubRoleName;
                            ra.DutySystemName = roleAccess.DutyIdentifier;
                            ra.DutyName = roleAccess.DutyName;
                            ra.PrivilegeSystemName = roleAccess.PrivilegeIdentifier;
                            ra.PrivilegeName = roleAccess.PrivilegeName;
                            ra.SecurableObject = roleAccess.Resource;
                            ra.SecurableType = roleAccess.ResourceType;
                            ra.Read = roleAccess.ResourceGrant.Read.ToString();
                            ra.Update = roleAccess.ResourceGrant.Update.ToString();
                            ra.Create = roleAccess.ResourceGrant.Create.ToString();
                            ra.Delete = roleAccess.ResourceGrant.Delete.ToString();
                            ra.Invoke = roleAccess.ResourceGrant.Invoke.ToString();
                            ra.ComputedLicense = roleAccess.ComputedLicense.LicenseType.ToString();
                            accessList.Add(ra);
                        }
                        csv.WriteRecords(accessList);
                    }
                }
                sw.Flush();
                MemoryStream roleAccessStream = new MemoryStream(ms.ToArray());
                return roleAccessStream;
            }
        }

        public static IEnumerable<EntUser> GetEntraIdUsers(string tenantId, string clientId, string clientSecret)
        {
            MsftGraphClient graphClient = new MsftGraphClient(new MsftGraphCredentials(clientId, clientSecret, tenantId));
            IEnumerable<EntUser> userList = new List<EntUser>();
            userList = graphClient.ListUsers();
            return userList;
        }

        public static IEnumerable<EntGroup> GetEntraIdGroups(string tenantId, string clientId, string clientSecret)
        {
            IEnumerable<EntGroup> groupList = new List<EntGroup>();
            MsftGraphClient graphClient = new MsftGraphClient(new MsftGraphCredentials(clientId, clientSecret, tenantId));
            groupList = graphClient.ListGroups();
            return groupList;
        }

        public static IEnumerable<EntUserGroup> GetEntraIdUserGroups(string tenantId, string clientId, string clientSecret, ArrayList d365foUsers)
        {
            IEnumerable<EntUser> userList = new List<EntUser>();
            IEnumerable<EntGroup> groupList = new List<EntGroup>();
            IEnumerable<EntUserGroup> userGroupList = new List<EntUserGroup>();
            MsftGraphClient graphClient = new MsftGraphClient(new MsftGraphCredentials(clientId, clientSecret, tenantId));

            userList = GetEntraIdUsers(tenantId, clientId, clientSecret);
            groupList = GetEntraIdGroups(tenantId, clientId, clientSecret);

            List<D365FOUser> d365FOUserList = new List<D365FOUser>();
            foreach(var u in d365foUsers)
            {
                D365FOUser usr = (D365FOUser)u;
                d365FOUserList.Add(usr);
            }

            var userSIDs = d365FOUserList.Where(u => u.AccountType == Models.UserAccountType.ClaimsGroup).Select(usr => usr.SID);
            userGroupList = graphClient.ListUserGroup(userSIDs, userList, groupList);
            return userGroupList;
        }
    }
}
