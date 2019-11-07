Param (
[Parameter(Mandatory=$True, HelpMessage='eg. "0-Preview123"')]
[ValidateNotNull()]
$versionSuffix)

& dotnet pack "$PSScriptRoot\src\Morcatko.AspNetCore.JsonMergePatch" --configuration Release --version-suffix $versionSuffix