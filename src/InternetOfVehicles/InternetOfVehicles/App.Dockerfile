# escape=`
FROM microsoft/windowsservercore:1803

LABEL maintainer="kevin6535.lin@etatung.com"

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN Set-ItemProperty -path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord

RUN New-Item -Path 'C:\App' -Type Directory;

COPY .\publish\InternetOfVehicles\. C:\App\

RUN New-Item -Path 'C:\App\Log' -Type Directory;

EXPOSE 8888

ENTRYPOINT C:\App\InternetOfVehicles.exe