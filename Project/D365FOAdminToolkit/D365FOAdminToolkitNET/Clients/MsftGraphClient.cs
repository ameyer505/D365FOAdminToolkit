using D365FOAdminToolkitNET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.Graph.Models;

namespace D365FOAdminToolkitNET.Clients
{
    public interface IMsftGraphClient
        {
            IEnumerable<EntUser> ListUsers();

            IEnumerable<EntGroup> ListGroups();

            IEnumerable<EntUserGroup> ListUserGroup(IEnumerable<string> userSIDs, IEnumerable<EntUser> users, IEnumerable<EntGroup> groups);
        }

        public class MsftGraphClient : IMsftGraphClient
        {
            private readonly HttpClient _httpClient;
            private const string _baseURL = "https://graph.microsoft.com";
            private const string _apiVersion = "v1.0"; //originally was 'beta'

            public MsftGraphClient(MsftGraphCredentials creds)
            {
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _httpClient.DefaultRequestHeaders.Add("Authorization", creds.GetAuthenticationHeader());
            }

            public IEnumerable<EntGroup> ListGroups()
            {
                List<EntGroup> groups = new List<EntGroup>();
                string select = "?$select=Description,DisplayName,Id";
                var response = _httpClient.GetAsync($"{_baseURL}/{_apiVersion}/groups{select}").GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    throw new Exception(content);
                }
                var responseStr = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var groupResponse = JsonConvert.DeserializeObject<MsftGraphResponse<EntGroup>>(responseStr);
                groups.AddRange(groupResponse.value);

                while (groupResponse.nextLink != null)
                {
                    response = _httpClient.GetAsync(groupResponse.nextLink).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        throw new Exception(content);
                    }
                    responseStr = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    groupResponse = JsonConvert.DeserializeObject<MsftGraphResponse<EntGroup>>(responseStr);
                    groups.AddRange(groupResponse.value);
                }

                return groups;
            }

            public IEnumerable<EntUser> ListUsers()
            {
                List<EntUser> users = new List<EntUser>();
                string select = "?$select=AccountEnabled,DisplayName,Id,OnPremisesUserPrincipalName,UserPrincipalName";
                var response = _httpClient.GetAsync($"{_baseURL}/{_apiVersion}/users{select}").GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    throw new Exception(content);
                }
                var responseStr = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var userResponse = JsonConvert.DeserializeObject<MsftGraphResponse<EntUser>>(responseStr);
                users.AddRange(userResponse.value);

                while (userResponse.nextLink != null)
                {
                    response = _httpClient.GetAsync(userResponse.nextLink).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        throw new Exception(content);
                    }
                    responseStr = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    userResponse = JsonConvert.DeserializeObject<MsftGraphResponse<EntUser>>(responseStr);
                    users.AddRange(userResponse.value);
                }

                return users;
            }


            public IEnumerable<EntUserGroup> ListUserGroup(IEnumerable<string> userSIDs, IEnumerable<EntUser> users, IEnumerable<EntGroup> groups)
            {
                List<EntUserGroup> userGroups = new List<EntUserGroup>();
                foreach (var userSID in userSIDs)
                {
                    var azureAdGroup = groups.FirstOrDefault(aadg => string.Equals(aadg.Id, userSID, StringComparison.CurrentCultureIgnoreCase));
                    if (azureAdGroup != null)
                    {
                        var groupMembers = ListGroupMembersAsync(azureAdGroup.Id);

                        foreach (var groupMember in groupMembers)
                        {
                            var user = users.FirstOrDefault(u => string.Equals(u.Id, groupMember.Id, StringComparison.CurrentCultureIgnoreCase));
                            if (user != null)
                            {
                                userGroups.Add(new EntUserGroup()
                                {
                                    UserId = new Guid(groupMember.Id),
                                    GroupId = new Guid(azureAdGroup.Id)
                                });
                            }
                        }
                    }
                }
                return userGroups;
            }

            private IEnumerable<DirectoryObject> ListGroupMembersAsync(string groupId)
            {
                List<DirectoryObject> objs = new List<DirectoryObject>();
                var response = _httpClient.GetAsync($"{_baseURL}/{_apiVersion}/groups/{groupId}/transitiveMembers").GetAwaiter().GetResult();
                if (!response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    throw new Exception(content);
                }
                var responseStr = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var transitiveMemberResponse = JsonConvert.DeserializeObject<MsftGraphResponse<DirectoryObject>>(responseStr);
                objs.AddRange(transitiveMemberResponse.value);

                while (transitiveMemberResponse.nextLink != null)
                {
                    response = _httpClient.GetAsync(transitiveMemberResponse.nextLink).GetAwaiter().GetResult();
                    if (!response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        throw new Exception(content);
                    }
                    responseStr = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    transitiveMemberResponse = JsonConvert.DeserializeObject<MsftGraphResponse<DirectoryObject>>(responseStr);
                    objs.AddRange(transitiveMemberResponse.value);
                }

                return objs;
            }
        }
}
