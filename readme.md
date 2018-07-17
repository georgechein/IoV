簡介
-
- 本專案主要目的在於將車聯網的原始碼，改為 container 版。
- 車聯網的web部分，原本是採用 Azure App Service 建立。
- 本原始碼包含兩大部分：車聯網的 web 畫面，與負責接收車載資料的socket server (即gateway)。
- web 部分，是由舊的 IoT remote monitoring 修改而成。整個方案中，只有 web 是最主要的部份，其餘多為測試用。故建置 image 時，已先將這些非必要的專案卸載。

注意事項
-
- 若開發人員遇到 Windows docker 出現 2個 nat 網路設定，導致 Visual Studio 出現 network nat is ambigous 的錯誤時，請先嘗試由系統網卡中找看看是否有兩張網卡的名稱都為 nat。若有，請將重複的刪除。(在使用 NB 的正常情況下，應該要有五張網卡，Wi-Fi、乙太網路、vEthernet (DockerNAT)、vEthernet (nat)、vEthernet (預設切換))
- 若刪除 nat 網卡後，Visual Studio 編譯時發生 HNS failed with error : 嘗試在降級的物件上執行作業。先確認 C:\Windows\System32\HostNetSvc.dll 的版本為何(2018/4/13的應為10.0.17134.1)。接著於登錄檔 HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\hns 的 Parameters 中，確認CompatibleMajorVersion、CompatibleMinorVersion、CurrentMajorVersion、CurrentMinorVersion的值，是否與實體檔案相符。若不同，則改為相同。
- 由於 Windows 10 1704 的更新，提供了 multiple nat work 的功能。導致 docker for windows 可能會建立兩個名稱同樣為 nat 的 network 設定。這將造成 Visual Studio 發生 network nat is ambigous 的錯誤。單純用指令無法刪除系統預設的 nat 網路設定。
- 使用 WindowsContainerNetworking-LoggingAndCleanupAide.ps1 可以取得系統中 docker 與相關環境的資訊，其中 HNSRegistry.txt 就有環境中的 HNS 服務資訊。苦主的環境不知為何登錄檔設定變成 7.2，與實際檔案不符。
- 轉為 .net core 版本，工程浩大，原專案參考了一個 DeviceManagement.Infrustructure.Connectivity 的 Nuget 套件，該套件無 .net core 版本。
- 使用 AAD (Azure Active Directory)驗證時，登入的網址會受到 AAD 的設定影響，無法變更。解決方式只有不使用 AAD 。