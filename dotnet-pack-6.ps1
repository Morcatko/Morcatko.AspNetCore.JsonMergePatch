Param (
[Parameter(Mandatory=$True, HelpMessage='eg. "0-Preview123"')]
[ValidateNotNull()]
$versionSuffix)

& dotnet pack "$PSScriptRoot\src\6.0-JsonMergePatch.Document" --configuration Release --version-suffix $versionSuffix
& dotnet pack "$PSScriptRoot\src\6.0-JsonMergePatch.NewtonsoftJson" --configuration Release --version-suffix $versionSuffix
& dotnet pack "$PSScriptRoot\src\6.0-JsonMergePatch.SystemText" --configuration Release --version-suffix $versionSuffix

New-Item -ItemType Directory -Force -Path "$PSScriptRoot\.nugets"
& copy "$PSScriptRoot\src\6.0-JsonMergePatch.Document\bin\Release\*.*nupkg" "$PSScriptRoot\.nugets"
& copy "$PSScriptRoot\src\6.0-JsonMergePatch.NewtonsoftJson\bin\Release\*.*nupkg" "$PSScriptRoot\.nugets"
& copy "$PSScriptRoot\src\6.0-JsonMergePatch.SystemText\bin\Release\*.*nupkg" "$PSScriptRoot\.nugets"