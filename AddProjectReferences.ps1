param (
    [Parameter(Mandatory = $true)] [string] $SourcesDirectory,
    [Parameter(Mandatory = $true)] [string] $NuGetVersion,
    [Parameter(Mandatory = $true)] [string] $PackagePath
)

$csprojPath = $SourcesDirectory + "\Tests\IntegrationTests\FileShareClientIntegrationTests\FileShareClientIntegrationTests.csproj"
Write-Host "Updating " $csprojPath
Write-Host "Using version " $NuGetVersion
Write-Host "Package source " $PackagePath

$xmlContent = [xml](Get-Content $csprojPath)

$propertyGroup = $xmlContent.Project.PropertyGroup

if ($propertyGroup -is [array]) {
    $propertyGroup = $propertyGroup[0]
}

$newRestoreSources = $xmlContent.CreateElement("RestoreAdditionalProjectSources", $xmlContent.DocumentElement.NamespaceURI)
$newRestoreSources.InnerText = $PackagePath
$propertyGroup.AppendChild($newRestoreSources) | Out-Null

$itemGroup = $xmlContent.Project.ItemGroup

if ($itemGroup -is [array]) {
    $itemGroup = $itemGroup[0]
}

$packageNode1 = $itemGroup.PackageReference | Where-Object { $_.Include -eq 'UKHO.FileShareClient' }

if ($packageNode1 -eq $null) {
    throw "Error - unable to find UKHO.FileShareClient reference"
} else {
    $packageNode1.SetAttribute("Version", $NuGetVersion)
}

$packageNode2 = $itemGroup.PackageReference | Where-Object { $_.Include -eq 'UKHO.FileShareAdminClient' }

if ($packageNode2 -eq $null) {
    throw "Error - unable to find UKHO.FileShareAdminClient reference"
} else {
    $packageNode2.SetAttribute("Version", $NuGetVersion)
}

$xmlContent.Save($csprojPath)

Write-Host "Updated project file:"
Get-Content $csprojPath
