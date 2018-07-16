#取得專案原始碼
#git clone https://github.com/georgechein/IoV.git

#切換至專案目錄下
#cd .\IoV

#定義專案絕對路徑
#$projectPath = "D:\source\NEW_EO10002"

#產生建置原始碼環境
docker-compose build builder

#產生建置結果目錄
#New-Item -Path $projectPath'\publish' -Type Directory;

#複製建置執行檔到publish目錄
#Copy-Item -Path $projectPath\build.ps1 -Destination $projectPath\publish\build.ps1 -Force;

#建置原始碼
#docker run --rm -v $projectPath\src\AP\TSTI:C:\SOURCE -v $projectPath\publish:C:\BUILD tsti/authweb-builder:latest C:\BUILD\build.ps1 

#產生資料庫映像檔
#docker-compose build db
#docker build -f .\Db.Dockerfile -t tsti/authweb-db:latest .

#準備好production環境的web.config for web and web service
#Copy-Item -Path $projectPath\Web.Web.config -Destination $projectPath\publish\TstiAuthWeb\_PublishedWebsites\TstiAuthWeb\Web.config -Force
#Copy-Item -Path $projectPath\WebService.Web.config -Destination $projectPath\publish\TstiAuthWebService\_PublishedWebsites\TstiAuthWebService\Web.config -Force

#產生網站映像檔
#docker-compose build authweb
#docker build -f .\Web.Dockerfile -t tsti/authweb-web:latest .

#因映像檔已完成，刪除建置產生的檔案
#Remove-Item -Path $projectPath'\publish' -Recurse;

#產生 mdf& ldf 檔存放位置
#New-Item -Path $projectPath'\data' -Type Directory;

#執行資料庫映像檔
#docker-compose up -d db
#docker run --name TstiAuthWeb-DB --ip 172.21.192.11 -d -p 1433:1433 -v "D:/source/NEW_EO10002/data:C:/DATA" -e sa_password=P@ssw0rd -e ACCEPT_EULA=Y tsti/authweb-db:latest
#docker exec TstiAuthWeb-DB SqlCmd -E -i C:\dbscript\1.CREATE_DB.sql
#docker exec TstiAuthWeb-DB SqlCmd -E -i C:\dbscript\2.CREATE_TABLES.sql
#docker exec TstiAuthWeb-DB SqlCmd -E -i C:\dbscript\3.CREATE_RECORDS.sql
#docker exec TstiAuthWeb-DB Sqlcmd -E -S 172.21.192.11 -Q "ALTER LOGIN SA WITH PASSWORD='P@ssw0rd'"

#產生 web log 檔存放位置
#New-Item -Path $projectPath'\logs' -Type Directory;

#執行網站映像檔
#docker-compose up -d authweb
#docker run --name TstiAuthWeb-WEB --ip 172.21.192.10 -d -p 80:80 tsti/authweb-web:latest
