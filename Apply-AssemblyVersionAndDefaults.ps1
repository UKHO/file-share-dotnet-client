param (
    [Parameter(Mandatory = $true)] [string] $buildNumber,
    [Parameter(Mandatory = $true)] [string] $solutionDirectory,
    [Parameter(Mandatory = $true)] [string] $UKHOAssemblyCompany,
    [Parameter(Mandatory = $true)] [string] $UKHOAssemblyCopyright,
    [Parameter(Mandatory = $true)] [string] $UKHOAssemblyVersionPrefix,
    [Parameter(Mandatory = $true)] [string] $UKHOAssemblyProduct
)

#UKHO.file-share-dotnet-client_merge_20210428.5
$buildNumberRegex = "(.+)_202([0-9]{3,5})\.([0-9]{1,2})"
$validBuildNumber = $buildNumber -match $buildNumberRegex

if ($validBuildNumber -eq $false) {
    Write-Error "Build number passed in must be in the following format: (BuildDefinitionName)_.(date:yyyyMMdd)(rev:.r)"
    return
}

# Magic var $Matches comes from the above regex match statement: $buildNumber -match $buildNumberRegex
$buildRevisionNumber = $Matches.2 + "." + $Matches.3
$versionToApply = $UKHOAssemblyVersionPrefix + $buildRevisionNumber

$assemblyValues = @{
    "Company"         = $UKHOAssemblyCompany;
    "Copyright"       = $UKHOAssemblyCopyright;
    "Description"     = $UKHOAssemblyProduct;
    "Product"         = $UKHOAssemblyProduct;
    "AssemblyVersion" = $versionToApply;
    "FileVersion"     = $versionToApply;
    "Version"         = $versionToApply;
}

function UpdateOrAddAttribute($xmlContent, $assemblyKey, $newValue, $namespace) {
    $propertyGroup = $xmlContent.Project.PropertyGroup
    if ($propertyGroup -is [array]) {
        $propertyGroup = $propertyGroup[0]
    }

    $propertyGroupNode = $propertyGroup.$assemblyKey

    if ($null -ne $propertyGroupNode) {
        Write-Host "Assembly key $assemblyKey has been located in source file - updating with value: " + $newValue
        $propertyGroup.$assemblyKey = $newValue
        return $xmlContent
    }

    Write-Host "Assembly key $assemblyKey could not be located in source file - appending value " + $newValue

    $newChild = $xmlContent.CreateElement($assemblyKey, $namespace)
    $newChild.InnerText = $newValue
    $propertyGroup.AppendChild($newChild)

    return $propertyGroupNode
}

(Get-ChildItem -Path $solutionDirectory -File -Filter "*.csproj" -Recurse) | ForEach-Object {
    $file = $_

    Write-Host "Updating assembly file at path: $file"
    [xml]$xmlContent = (Get-Content $file.FullName)

    $assemblyValues.Keys | ForEach-Object {
        $key = $_

        UpdateOrAddAttribute $xmlContent $key $assemblyValues[$key] $xmlContent.DocumentElement.NamespaceURI
    }

    $xmlContent.Save($file.FullName)
}
