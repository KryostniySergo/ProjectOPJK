using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;

namespace ProjectOPJK
{
    public class GetCredentialAndDriveService
    {
        // TODO -> {Сделать через Environment.CurrentDirectory}
        string path = @"necessary equipment\";

        string[] Scopes = { DriveService.Scope.Drive };
        string ApplicationName = "Rp Update app";

        UserCredential GetUserCredential()
        {
            using (var stream = new FileStream($"{path}client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string creadPath = @"necessary equipment\Default";
                creadPath = Path.Combine(creadPath, "driveApiCredentials", "drive-credentials.json");

                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "User",
                    CancellationToken.None,
                    new FileDataStore(creadPath, true)
                    ).Result;
            }
        }

        public DriveService GetDriveService()
        {
            var credential = GetUserCredential();

            return new DriveService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                }
                );
        }
    }
}
