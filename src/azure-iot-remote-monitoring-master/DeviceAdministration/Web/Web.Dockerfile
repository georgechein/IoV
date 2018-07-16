# escape=`
FROM microsoft/iis:latest

LABEL maintainer="kevin6535.lin@etatung.com"

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

RUN Set-ItemProperty -path 'HKLM:\SYSTEM\CurrentControlSet\Services\Dnscache\Parameters' -Name ServerPriorityTimeLimit -Value 0 -Type DWord

RUN Install-WindowsFeature NET-Framework-45-ASPNET; `
	Install-WindowsFeature Web-Asp-Net45

RUN Import-Module IISAdministration; Import-Module webadministration;`
	$cert=New-SelfSignedCertificate -DnsName 'iov.tsti.local' -CertStoreLocation Cert:\LocalMachine\My; `
	$rootStore=New-Object System.Security.Cryptography.X509Certificates.X509Store -ArgumentList Root, LocalMachine; `
	$rootStore.Open('MaxAllowed'); `
	$rootStore.Add($cert); `
	$rootStore.Close(); `
	$certHash=$cert.GetCertHash(); `
	$sm=Get-IISServerManager; `
	$sm.Sites['Default Web Site'].Bindings.Add('*:443:', $certHash, 'My','0'); `
	$sm.CommitChanges();

#RUN	["powershell","-Command","new-item -path IIS:\SslBindings\0.0.0.0!443 -value $cert;"]
	
#RUN New-WebBinding -Name 'Default Web Site' -IPAddress '*' -Port 443 -Protocol https
	
COPY .\publish\Web\_PublishedWebsites\Web\. C:\inetpub\wwwroot\

EXPOSE 80 443