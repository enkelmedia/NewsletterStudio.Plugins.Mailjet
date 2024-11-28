Function setVersion($filePath, $versionPrefix, $versionSuffix)
{
    $assemblyVersion = $versionPrefix
    $packageVersion = $versionPrefix + $versionSuffix

    Write-Host "Settings Versions for: $filePath"
    Write-Host "AssemblyVersion: $assemblyVersion"
    Write-Host "PackageVersion: $packageVersion"

    if(!(Test-Path $filePath)) {
        Write-Host "File not found"
        exit;
    }
    else {
        Write-Host "File found, updating version elements."
    }

    $xml=New-Object XML
    $xml.Load($filePath)
    $versionNode = $xml.Project.PropertyGroup.Version
    if ($versionNode -eq $null) {
        # If you have a new project and have not changed the version number the Version tag may not exist
        $versionNode = $xml.CreateElement("Version")
        $xml.Project.PropertyGroup.AppendChild($versionNode)
        Write-Host "Version-node XML tag added to the csproj"
    }

    $packageVersionNode = $xml.Project.PropertyGroup.PackageVersion
    if ($packageVersionNode -eq $null) {
        # If you have a new project and have not changed the version number the Version tag may not exist
        $packageVersionNode = $xml.CreateElement("PackageVersion")
        $xml.Project.PropertyGroup.AppendChild($packageVersionNode)
        Write-Host "PackageVersion-node XML tag added to the csproj"
    }

    $informationalVersionNode = $xml.Project.PropertyGroup.InformationalVersion
    if ($informationalVersionNode -eq $null) {
        # If you have a new project and have not changed the version number the Version tag may not exist
        $informationalVersionNode = $xml.CreateElement("InformationalVersion")
        $xml.Project.PropertyGroup.AppendChild($informationalVersionNode)
        Write-Host "InformationalVersion-node XML tag added to the csproj"
    }

    # Settings Properties
    $xml.Project.PropertyGroup.Version = $assemblyVersion
    $xml.Project.PropertyGroup.PackageVersion = $packageVersion
    $xml.Project.PropertyGroup.InformationalVersion = $packageVersion

    $xml.Save($filePath)

    Write-Host "Version updated in csproj"
}

Function exitIfNotSuccess()
{
    if($LASTEXITCODE -eq 1) {
        Write-Host "Last action failed, aborting." -ForegroundColor Red
        exit;
    }
}

Function cleanNuGetCache($packageId,$fullVersion) {

    Write-Host "Cleaning NuGet-cache for package $packageId | $fullVersion"

    $packageNuGetCacheFolder = "$($env:USERPROFILE)\.nuget\packages\$packageId\$fullVersion"
    if(Test-Path $packageNuGetCacheFolder){
        Remove-Item $packageNuGetCacheFolder -Recurse
        WriteHost "Folder $packageNugetCacheFolder cleaned."
    }
}
