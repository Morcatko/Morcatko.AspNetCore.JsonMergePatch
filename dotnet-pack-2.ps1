Param (
[Parameter(Mandatory=$True, HelpMessage='eg. "0-Preview123"')]
[ValidateNotNull()]
$versionSuffix)

& dotnet pack "$PSScriptRoot\src\2.1-JsonMergePatch" --configuration Release --version-suffix $versionSuffix

New-Item -ItemType Directory -Force -Path "$PSScriptRoot\.nugets"
& copy "$PSScriptRoot\src\2.1-JsonMergePatch\bin\Release\*.*nupkg" "$PSScriptRoot\.nugets"