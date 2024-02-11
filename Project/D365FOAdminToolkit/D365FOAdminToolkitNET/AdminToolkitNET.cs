using CsvHelper;
using D365FOAdminToolkitNET.Models;
using Microsoft.Dynamics.AX.Security.Management;
using Microsoft.Dynamics.AX.Security.Management.Querying;
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
    }
}
