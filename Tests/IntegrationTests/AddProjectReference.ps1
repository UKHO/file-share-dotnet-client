#param (
#    [Parameter(Mandatory = $true)] [string] $SourcesDirectory,
#    [Parameter(Mandatory = $true)] [string] $NuGetVersion,
#    [Parameter(Mandatory = $true)] [string] $PackagePath
#)

$SourcesDirectory = "C:\Code\file-share-dotnet-client"
$NuGetVersion = "2.0.41003.1"
$PackagePath = "C:\Users\ahugob\Desktop\FSS\packages"

$csprojPath = $SourcesDirectory + "\Tests\IntegrationTests\FileShareClientIntegrationTests\FileShareClientIntegrationTests.csproj"
$xmlContent = [xml](Get-Content $csprojPath)
$propertyGroup = $xmlContent.Project.PropertyGroup

if ($propertyGroup -is [array]) {
    $propertyGroup = $propertyGroup[0]
}

$newChild = $xmlContent.CreateElement("RestoreSources", $xmlContent.DocumentElement.NamespaceURI)
$newChild.InnerText = '$(RestoreSources);' + $PackagePath
$propertyGroup.AppendChild($newChild)
$xmlContent.Save($csprojPath)

Write-Host "Finished"
