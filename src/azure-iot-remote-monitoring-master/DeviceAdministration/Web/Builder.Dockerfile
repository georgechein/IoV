# escape=`
FROM microsoft/aspnet:4.7.1-windowsservercore-1709

LABEL maintainer="kevin6535.lin@etatung.com"

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN Install-PackageProvider -Name chocolatey -RequiredVersion 2.8.5.130 -Force; `
	Install-Package -Name microsoft-build-tools -RequiredVersion 14.0.25420.1 -Force; `
	Install-Package -Name webdeploy -RequiredVersion 3.6.20170627 -Force; `
	Install-Package -Name nuget.commandline -RequiredVersion 4.6.2 -Force;

RUN C:\Chocolatey\bin\nuget install MSBuild.Microsoft.VisualStudio.Web.targets -Version 14.0.0.3

RUN Install-Package -Name netfx-4.5.1-devpack -RequiredVersion 4.5.50932 -Force;
RUN Install-Package -Name windows-sdk-10.1 -RequiredVersion 10.1.15063.468 -Force;

RUN New-Item -Path 'C:\SOURCE' -Type Directory; `
	New-Item -Path 'C:\BUILD' -Type Directory;
