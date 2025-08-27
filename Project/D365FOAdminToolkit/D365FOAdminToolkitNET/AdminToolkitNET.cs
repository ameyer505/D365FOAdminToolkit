using CsvHelper;
using D365FOAdminToolkitNET.Models;
using Microsoft.Dynamics.AX.Security.Management;
using Microsoft.Dynamics.AX.Security.Management.Querying;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.IO.Compression;
using Microsoft.Dynamics.Ax.Xpp;
using Microsoft.Dynamics.AX.Metadata.MetaModel;

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
                        ISecurityRelatedObjectsData srod = rsof.FindRelatedSecurityObjectsForRolesWithLicenseInfo(new List<string>() { role }, out Dictionary<string, ComputedLicense> rsol);

                        foreach (var roleAccess in srod.RelatedObjects)
                        {
                            RoleAccess ra = new RoleAccess();
                            ra.RoleIdentifier = roleAccess.RoleIdentifier;
                            ra.RoleName = roleAccess.RoleName;
                            ra.SubRoleIdentifer = roleAccess.SubRoleIdentifier;
                            ra.SubRoleName = roleAccess.SubRoleName;
                            ra.DutyIdentifier = roleAccess.DutyIdentifier;
                            ra.DutyName = roleAccess.DutyName;
                            ra.PrivilegeIdentifier = roleAccess.PrivilegeIdentifier;
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

        public static MemoryStream GenerateEnvironmentExport(
            SecurityRepository sr,
            ArrayList users,
            ArrayList userRoles,
            ArrayList userRoleOrgs)
        {
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                List<User> userList = GetUsers(users);
                List<SecurityUserRoleAssociation> userRoleList = GetUserRoles(userRoles);
                List<SecurityUserRoleOrganization> userRoleOrgsList = GetUserRoleOrgs(userRoleOrgs);

                List<CsvFile> files = new List<CsvFile>();

                //Users
                using (MemoryStream userListStream = new MemoryStream())
                {
                    var sw = new StreamWriter(userListStream);
                    using (var csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
                        csv.WriteRecords(userList);

                    CsvFile file = new CsvFile()
                    {
                        Name = "User.csv",
                        Contents = userListStream
                    };

                    files.Add(file);
                }

                //User Roles
                using (MemoryStream userRoleListStream = new MemoryStream())
                {
                    var sw = new StreamWriter(userRoleListStream);
                    using (var csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
                        csv.WriteRecords(userRoleList);

                    CsvFile file = new CsvFile()
                    {
                        Name = "SecurityUserRoleAssociation.csv",
                        Contents = userRoleListStream
                    };

                    files.Add(file);
                }

                //User Role Companies
                using (MemoryStream userRoleOrgListStream = new MemoryStream())
                {
                    var sw = new StreamWriter(userRoleOrgListStream);
                    using (var csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
                        csv.WriteRecords(userRoleOrgsList);

                    CsvFile file = new CsvFile()
                    {
                        Name = "SecurityUserRoleOrganization.csv",
                        Contents = userRoleOrgListStream
                    };

                    files.Add(file);
                }

                //Role Access
                MemoryStream roleAccessStream = GenerateRoleAccessCSV(sr);
                CsvFile raFile = new CsvFile()
                {
                    Name = "RoleAccess.csv",
                    Contents = roleAccessStream
                };
                files.Add(raFile);

                //Data Entities
                var dataEntityMetadata = GetDataEntityMetadata();
                var dataEntityRootDataSources = GetDataEntityDataSource(false);
                var dataEntityAllDataSources = GetDataEntityDataSource(true);

                using(MemoryStream dataEntityMetadataStream = new MemoryStream())
                {
                    var sw = new StreamWriter(dataEntityMetadataStream);
                    using (var csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
                        csv.WriteRecords(dataEntityMetadata);

                    CsvFile file = new CsvFile()
                    {
                        Name = "DataEntityMetadata.csv",
                        Contents = dataEntityMetadataStream
                    };

                    files.Add(file);
                }

                using(MemoryStream dataEntityRootDataSourcesStream = new MemoryStream())
                {
                    var sw = new StreamWriter(dataEntityRootDataSourcesStream);
                    using (var csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
                        csv.WriteRecords(dataEntityRootDataSources);

                    CsvFile file = new CsvFile()
                    {
                        Name = "DataEntityRootDataSources.csv",
                        Contents = dataEntityRootDataSourcesStream
                    };

                    files.Add(file);
                }

                using (MemoryStream dataEntityAllDataSourcesStream = new MemoryStream())
                {
                    var sw = new StreamWriter(dataEntityAllDataSourcesStream);
                    using (var csv = new CsvWriter(sw, CultureInfo.InvariantCulture))
                        csv.WriteRecords(dataEntityAllDataSources);

                    CsvFile file = new CsvFile()
                    {
                        Name = "DataEntityAllDataSources.csv",
                        Contents = dataEntityAllDataSourcesStream
                    };

                    files.Add(file);
                }

                //Write files to zip
                using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var entry = zipArchive.CreateEntry(file.Name);

                        using (var entryStream = entry.Open())
                        {
                            byte[] fileBytes = file.Contents.ToArray();
                            entryStream.Write(fileBytes, 0, fileBytes.Length);
                        }
                    }
                }

                return memoryStream;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }

        private static List<User> GetUsers(ArrayList users)
        {
            List<User> userList = new List<User>();
            foreach (var user in users)
            {
                User u = (User)user;
                userList.Add(u);
            }
            return userList;
        }

        private static List<SecurityUserRoleAssociation> GetUserRoles(ArrayList userRoles)
        {
            List<SecurityUserRoleAssociation> userRoleList = new List<SecurityUserRoleAssociation>();
            foreach (var userRole in userRoles)
            {
                SecurityUserRoleAssociation ur = (SecurityUserRoleAssociation)userRole;
                userRoleList.Add(ur);
            }
            return userRoleList;
        }

        private static List<SecurityUserRoleOrganization> GetUserRoleOrgs(ArrayList userRoleOrgs)
        {
            List<SecurityUserRoleOrganization> userRoleOrgList = new List<SecurityUserRoleOrganization>();
            foreach (var userRoleOrg in userRoleOrgs)
            {
                SecurityUserRoleOrganization uro = (SecurityUserRoleOrganization)userRoleOrg;
                userRoleOrgList.Add(uro);
            }
            return userRoleOrgList;
        }

        private static List<DataEntityMetadata> GetDataEntityMetadata()
        {
            List<DataEntityMetadata> dataEntityList = new List<DataEntityMetadata>();
            var des = MetadataSupport.GetDataEntityViewNames();
            foreach (var de in des)
            {
                AxDataEntity dataEntity = MetadataSupport.GetDataEntity(de);
                AxDataEntityViewBase dataEntityViewBase = (AxDataEntityViewBase)dataEntity;
                DataEntityMetadata dem = new DataEntityMetadata()
                {
                    Name = dataEntity.Name,
                    Label = LabelHelper.GetLabel(dataEntity.Label),
                    PublicCollectionName = dataEntityViewBase.PublicCollectionName,
                    PublicEntityName = dataEntityViewBase.PublicEntityName
                };
                dataEntityList.Add(dem);
            }

            return dataEntityList;
        }

        private static List<DataEntityDataSource> GetDataEntityDataSource(bool includeNestedDataSources) 
        {
            List<DataEntityDataSource> dataEntityList = new List<DataEntityDataSource>();
            var des = MetadataSupport.GetDataEntityViewNames();
            foreach (var de in des)
            {
                AxDataEntity dataEntity = MetadataSupport.GetDataEntity(de);
                AxDataEntityViewBase dataEntityViewBase = (AxDataEntityViewBase)dataEntity;
                AxDataEntityView dataEntityView = MetadataSupport.GetDataEntityView(de);
                AxQuerySimple query = dataEntityView.ViewMetadata;
                var dataSources = query.DataSources;

                List<string> dataSourceList = new List<string>();

                foreach (var dataSource in dataSources)
                {
                    AddDataSources(dataSource, dataSourceList, includeNestedDataSources);
                }

                foreach (var dataSource in dataSourceList)
                {

                    DataEntityDataSource dedd = new DataEntityDataSource()
                    {
                        DataEntityName = dataEntity.Name,
                        DataEntityLabel = LabelHelper.GetLabel(dataEntity.Label),
                        DataSourceName = dataSource
                    };

                    dataEntityList.Add(dedd);
                }
            }

            return dataEntityList;
        }

        private static void AddDataSources(AxQuerySimpleDataSource ds, List<string> dataSourceList, bool includeNestedDataSources)
        {
            dataSourceList.Add(ds.Name);
            if (includeNestedDataSources)
            {
                foreach (var dataSource in ds.DataSources)
                    AddDataSources(dataSource, dataSourceList, true);
            }
        }

    }
}
