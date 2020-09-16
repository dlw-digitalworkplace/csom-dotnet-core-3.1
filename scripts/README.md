## Description
This script generates a certificate that can be used for authentication. 

## Prerequisites
 - Azure AD PowerShell module needs to be installed.
 - PowerShell is run as administrator

## Parameters
| Variable name | Variable Description                                                   | Example                          |
|---------------|------------------------------------------------------------------------|----------------------------------|
| $dnsName      | The hostname of the application for which the certificate will be used | https://<my-app-service>.azurewebsites.net |
| $password     | The password of the certificate                                        | ***                    |
| $outputPath   | The output path where the generated certificate will be stored         | c:\                      |
| $fileName     | The name of the certificate file                                       | <customer>-<project>-<env>-cert         |
| $yearsValid   | The number of years that the certificate will be valid                 | 10                               |

## Example
```PowerShell
.\CreateCertificate.ps1 -dnsName "https://<my-app-service>.azurewebsites.net" -password "*******" -outputPath "c:\" -fileName "<customer>-<project>-<env>-cert" -yearsValid 10
```