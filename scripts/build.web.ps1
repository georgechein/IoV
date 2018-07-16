$nuGetPath = "C:\Chocolatey\bin\nuget.bat"
$msBuildPath = "C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
$project1 = "Web"

cd C:\SOURCE\
& $nuGetPath restore .\RemoteMonitoring.sln
& $msBuildPath .\DeviceAdministration\$project1\$project1.csproj "/p:OutputPath=C:\BUILD\$project1;DeployOnBuild=true;VSToolsPath=C:\MSBuild.Microsoft.VisualStudio.Web.targets.14.0.0.3\tools\VSToolsPath;TargetFrameworkSDKToolsDirectory=C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.7 Tools"