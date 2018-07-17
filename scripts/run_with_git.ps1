#取得專案原始碼
git clone https://github.com/georgechein/IoV.git

#切換至專案目錄下
cd .\IoV

#定義專案絕對路徑
$projectPath = (Get-Item -Path ".\").FullName

#切換至scripts目錄下
cd scripts

#產生建置原始碼環境
docker-compose build IoVWebBuilder

#產生建置結果目錄
New-Item -Path $projectPath'\scripts\publish' -Type Directory;

#複製建置執行檔到publish目錄
Copy-Item -Path $projectPath\scripts\build.web.ps1 -Destination $projectPath\scripts\publish\build.web.ps1 -Force;

#建置原始碼
docker run --rm -v $projectPath\src\azure-iot-remote-monitoring-master:C:\SOURCE -v $projectPath\scripts\publish:C:\BUILD tsti/iovwebbuilder:latest C:\BUILD\build.web.ps1 

#準備好production環境的web.config for web and web service
Remove-Item -Path .\publish\Web\_PublishedWebsites\Web\Web.Debug.config -Force
Remove-Item -Path .\publish\Web\_PublishedWebsites\Web\Web.Release.config -Force

#產生網站映像檔
docker-compose build IoVWeb
#docker build -f .\Web.Dockerfile -t tsti/authweb-web:latest .

#產生 web log 檔存放位置
#New-Item -Path $projectPath'\logs' -Type Directory;

#執行網站映像檔
docker-compose run -d --name IoVWeb --service-ports IoVWeb
#docker run --name TstiAuthWeb-WEB --ip 172.21.192.10 -d -p 80:80 tsti/authweb-web:latest

#因網站成功啟動，刪除建置所產生的檔案
Remove-Item -Path $projectPath'\scripts\publish' -Recurse;

#產生建置原始碼環境
docker-compose build IoVGatewayBuilder

#產生建置結果目錄
New-Item -Path $projectPath'\scripts\publish' -Type Directory;

#複製建置執行檔到publish目錄
Copy-Item -Path $projectPath\scripts\build.gateway.ps1 -Destination $projectPath\scripts\publish\build.gateway.ps1 -Force;

#建置原始碼
docker run --rm -v $projectPath\src\InternetOfVehicles:C:\SOURCE -v $projectPath\scripts\publish:C:\BUILD tsti/iovgwbuilder:latest C:\BUILD\build.gateway.ps1 

#產生gateway映像檔
docker-compose build IoVGateway

#執行gateway映像檔
docker-compose run -d --name IoVGateway --service-ports IoVGateway

#因gateway成功啟動，刪除建置所產生的檔案
Remove-Item -Path $projectPath'\scripts\publish' -Recurse;
