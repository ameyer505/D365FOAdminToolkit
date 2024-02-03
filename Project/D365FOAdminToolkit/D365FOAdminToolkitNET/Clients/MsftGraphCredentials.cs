using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;

namespace D365FOAdminToolkitNET.Clients
{
    public class MsftGraphCredentials
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }

        public MsftGraphCredentials(string clientId, string clientSecret, string tenantId)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            TenantId = tenantId;
        }

        public string GetAuthenticationHeader()
        {
            TokenCache tokenCache = new TokenCache();
            var authority = $"https://login.microsoftonline.com/{TenantId}";
            var authContext = new AuthenticationContext(authority, tokenCache);
            var authResult = Task.Run(async () =>
                await authContext.AcquireTokenAsync(
                "https://graph.microsoft.com/",
                new ClientCredential(ClientId, ClientSecret)).ConfigureAwait(false)).GetAwaiter().GetResult();

            return authResult.AccessToken;
        }
    }
}
