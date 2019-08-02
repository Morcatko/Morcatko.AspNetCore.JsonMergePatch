Param (
[Parameter(Mandatory=$True)]
[ValidateNotNull()]
$versionSuffix)

& dotnet pack "$PSScriptRoot\src\3.0-JsonMergePatch" --configuration Release --version-suffix $versionSuffix
& dotnet pack "$PSScriptRoot\src\3.0-JsonMergePatch.NewtonsoftJson" --configuration Release --version-suffix $versionSuffix

New-Item -ItemType Directory -Force -Path "$PSScriptRoot\.nugets"
& copy "$PSScriptRoot\src\3.0-JsonMergePatch\bin\Release\*.*nupkg" "$PSScriptRoot\.nugets"
& copy "$PSScriptRoot\src\3.0-JsonMergePatch.NewtonsoftJson\bin\Release\*.*nupkg" "$PSScriptRoot\.nugets"