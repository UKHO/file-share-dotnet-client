param (
    [Parameter(Mandatory = $true)] [string] $SourcesDirectory,
    [Parameter(Mandatory = $true)] [string] $NuGetVersion,
    [Parameter(Mandatory = $true)] [string] $PackagePath
)

$csprojPath = $SourcesDirectory + "\Tests\IntegrationTests\FileShareClientIntegrationTests\FileShareClientIntegrationTests.csproj"
Write-Host "Updating " + $csprojPath
Write-Host "Using version " + $NuGetVersion
Write-Host "Package source " + $PackagePath

$xmlContent = [xml](Get-Content $csprojPath)

$propertyGroup = $xmlContent.Project.PropertyGroup

if ($propertyGroup -is [array]) {
    $propertyGroup = $propertyGroup[0]
}

$newRestoreSources = $xmlContent.CreateElement("RestoreSources", $xmlContent.DocumentElement.NamespaceURI)
$newRestoreSources.InnerText = '$(RestoreSources);' + $PackagePath
$propertyGroup.AppendChild($newRestoreSources)

$itemGroup = $xmlContent.Project.ItemGroup

if ($itemGroup -is [array]) {
    $itemGroup = $itemGroup[0]
}

$newPackageReference1 = $xmlContent.CreateElement("PackageReference", $xmlContent.DocumentElement.NamespaceURI)
$newPackageReference1.SetAttribute("Include", "UKHO.FileShareClient")
$newPackageReference1.SetAttribute("Version", $NuGetVersion)
$itemGroup.AppendChild($newPackageReference1)

$newPackageReference2 = $xmlContent.CreateElement("PackageReference", $xmlContent.DocumentElement.NamespaceURI)
$newPackageReference2.SetAttribute("Include", "UKHO.FileShareAdminClient")
$newPackageReference2.SetAttribute("Version", $NuGetVersion)
$itemGroup.AppendChild($newPackageReference2)

$xmlContent.Save($csprojPath)

Write-Host "Finished"
