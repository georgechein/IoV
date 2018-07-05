- 本專案主要目的在於將車聯網的原始碼，改為 container 版。
- 車聯網的web部分，原本是採用 Azure App Service 建立。
- 本原始碼包含兩大部分：車聯網的 web 畫面，與負責接收車載資料的socket server (即gateway)。
- web 部分，是由舊的 IoT remote monitoring 修改而成。整個方案中，只有 web 是最主要的部份，其餘多為測試用。故建置 image 時，已先將這些非必要的專案卸載。
- 由於 Windows 10 1704 的更新，提供了 multiple nat work 的功能。導致 docker for windows 會建立兩個名稱同樣為 nat 的 network 設定。這將造成 Visual Studio 發生 network nat is ambigous 的錯誤。此時，請將docker-compose.override.yml 中的名稱，改用 mynat ，並執行以下指令，另外建立一個 mynat 的 nat network。
```
docker network create -d nat mynat
```