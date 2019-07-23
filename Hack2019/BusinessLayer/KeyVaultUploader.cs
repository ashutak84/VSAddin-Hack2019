using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace BusinessLayer
{
    public class KeyVaultUploader
    {
        const string CLIENTSECRET = "";
        const string CLIENTID = "cd830ebc-213c-4586-9246-db0f3e238e32";
        const string BASESECRETURI =
            "https://notificationdev1.vault.azure.net"; // available from the Key Vault resource page

        static KeyVaultClient kvc = null;

        public static object ConfigurationManager { get; private set; }

        public static async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(CLIENTID, CLIENTSECRET);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }

        public static void UploadToVault(Dictionary<string,string> keysToBeUploaded)
        {
            //kvc = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(GetToken));
            var connectionString = "";
            var KeyVaultBaseUri = "";


            var azureServiceTokenProvider = new AzureServiceTokenProvider(connectionString);
            kvc = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));


            // write
            writeKeyVault(keysToBeUploaded);
            
        }

        private static async void writeKeyVault(Dictionary<string, string> keysToBeUploaded) // string szPFX, string szCER, string szPassword)
        {
            SecretAttributes attribs = new SecretAttributes
            {
                Enabled = true,
                Expires = DateTime.UtcNow.AddYears(1), // if you want to expire the info
                NotBefore = DateTime.UtcNow.AddDays(1) // if you want the info to 
                 //start being available later
            };


            foreach (var key in keysToBeUploaded.Keys)
            {
                SecretBundle bundle = await kvc.SetSecretAsync
               (BASESECRETURI, key, keysToBeUploaded[key], null, null, attribs);
            }

        }
    }
}
