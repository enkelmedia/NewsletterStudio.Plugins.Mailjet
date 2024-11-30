param ($version = "14.0.0",$versionSuffix = "")
# Params
# version = major.minor.patch
# versionSuffix = eg -beta1, -rc1 (should include leading -). Leave as empty string if not needed.

. ".\functions.ps1" #Includes functions used in the script

$versionFull = $version + $versionSuffix
$packageId = "NewsletterStudio.Plugins.Mailjet"

Write-Host "Version  :" $version
Write-Host "VersionFull : " $versionFull

cleanNuGetCache $packageId $versionFull

# Update cachebuster in umbraco-package.json
Update-UmbracoPackageJsonFile -JsonFilePath "../src/$packageId/wwwroot/App_Plugins/$packageId/umbraco-package.json" -NewVersion $versionFull
exitIfNotSuccess

setVersion ../src/NewsletterStudio.Plugins.Mailjet/NewsletterStudio.Plugins.Mailjet.csproj $version $versionSuffix

dotnet pack ../src/NewsletterStudio.Plugins.Mailjet/NewsletterStudio.Plugins.Mailjet.csproj --output Artifacts --configuration Release
