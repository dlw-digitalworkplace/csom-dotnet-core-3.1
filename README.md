# Demo CSOM in .NET CORE (3.1)

This repo contains a sample project to create CSOM contexts in various ways in .NET core (3.1).

The master branch includes:
- CsomHelper class to create a context using:
  - Username & password
    - \- Only suited for local dev, but even then not so much
    - \+ Easiest to set up
  - Managed Identity
    - \- Only for deployed app
    - \+ Easy setup, see [Rick's blog](https://www.rickvanrousselt.com/stop-using-clientid-and-secret-to-access-your-office-365-services/)
  - Using our own certificates (for local dev & deployed app)
    - \- This comes with a lot of extra code (AuthenticationManager, EncryptionUtility, ...)
    - \- You need to manage the certificates yourself on TST, QA, PROD
    - \+ Consistent approach for local dev & deployed app

The DefaultAzureCredential branch contains a more updated sample that uses the IAccessTokenProvider interface to ensure access tokens using a consistent approach across both local & dev. This approach lets Azure handle all certificate details using Managed Identity (no expiring certificates, not needing to upload them yourself), does use a certificate for local development for consistency, but adds little extra code to support both local dev & deployed app.

Note: the following documentation targets the DefaultAzureCredential branch.

Note: this way of working replaces the classic ACS way (adding app permission request xml via appinv.aspx). [When not using ACS, but only AAD app permissions, you are OBLIGED to use certificate authentication](https://docs.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly-azuread#can-i-use-other-means-besides-certificates-for-realizing-app-only-access-for-my-azure-ad-app) (Managed Identity is in the certificate auth category as it uses certifications to auth in the background already).

## Prerequisites

- Have an AAD app with SharePoint app permissions added & granted to it. The clientId app setting is its AAD app id

# DefaultAzureCredential

[DefaultAzureCredential](https://docs.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet) will check in the following [order of precedence](https://www.rahulpnath.com/blog/defaultazurecredential_from_azure_sdk/) for getting an access token and use the first valid one:
1. EnvironmentCredential
2. ManagedIdentityCredential
3. SharedTokenCacheCredential
4. VisualStudioCredential
5. VisualStudioCodeCredential
6. AzureCliCredential
7. InteractiveBrowserCredential

## Use own certificate for local development
- Install [openssl](https://slproweb.com/products/Win32OpenSSL.html)
- cd C:\temp (choose folder of your liking)
- Execute the following 3 commands to create a <privateKeyWithPassphrase>.pem file
  - You can rename the output files to match the name of your customer-project-env
  - openssl req -x509 -sha256 -nodes -days 365 -newkey rsa:2048 -keyout privateKey.key -out certificate.cer
     - You need to add some info, but not really important (e.g. country, State or province, locality, org name, ...); small input suffices
  - openssl pkcs12 -export -out protected.pfx -inkey privateKey.key -in certificate.cer -password pass:"<yourSecurePassword>"
  - openssl pkcs12 -in protected.pfx -out privateKeyWithPassphrase.pem -nodes
- Upload the .pem file to your AAD app
- Reference the path to the .pem file in the appsettings of the web api (secrets.json)
- The code using AzureDefaultCredential will use the EnvironmentCredential (see Startup.cs: env.isDevelopment())

      Important note: if you misconfigure the above EnvironmentCredential on your local dev whilst being logged in with your global admin account in VS, you will also receive a valid access token and CSOM calls will succeed. However, be aware that this is NOT what we want, we want to mimic an app making CSOM calls to have the same experience on dev vs deployed to avoid nasty bugs. You can log out of that account in account settings to make sure the EnvironmentCredential works as expected.

## Use managed identity (MI) for deployed app

Just follow the steps from [Rick's blog](https://www.rickvanrousselt.com/stop-using-clientid-and-secret-to-access-your-office-365-services/):

- App service -> Identity -> Enable system assigned MI to app service -> copy the MI app id
- Add permissions to the MI AAD instance via O365 CLI

      Connect-AzureAD

      #Get the Service Principal that was created when you created a Managed Identity from your Web App
      $MyMI = Get-AzureADServicePrincipal -ObjectId <ID(GUID) of your managed identity shown in the Azure portal>

      #Get the Office 365 SharePoint Online API
      $SharePointAPI = Get-AzureADServicePrincipal -SearchString “Office 365 SharePoint”

      #Select the User.ReadWrite.All App-Only Permissions. If you want another App-Only permission then search for another value
      $UserReadWrite = $SharePointAPI.AppRoles | where Value -like 'Sites.FullControl.All'

      #Add the User.ReadWrite.All permissions from the Office 365 SharePoint Online API 
      New-AzureADServiceAppRoleAssignment -Id $UserReadWrite.Id -ObjectId $MyMI.ObjectId -PrincipalId $MyMI.ObjectId -ResourceId $SharePointAPI.ObjectId

- We will not be needing AzureServiceTokenProvider() to fetch access tokens
- The code using AzureDefaultCredential will use the MI credential to authenticate towards SharePoint when doing CSOM requests using the permissions added earlier

# UserCredential

This access token provider is also provided in the sample. You can easily use this implementation of IAccessTokenProvider instead of the AzureDefaultCredential implementation. You only need to add some app settings such as UserName & Password.