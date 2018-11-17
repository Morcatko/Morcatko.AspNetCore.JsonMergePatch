Param (
[Parameter(Mandatory=$True)]
[ValidateNotNull()]
$versionSuffix)

& dotnet pack "$PSScriptRoot\src\Morcatko.AspNetCore.JsonMergePatch" --configuration Release --version-suffix $versionSuffix