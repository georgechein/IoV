using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Sockets;
using System.Text;

namespace InternetOfVehicles
{
    class BaseProcess
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BaseProcess));
        //public static string[] positioningData = new string[18];
        public List<string[]> obdData = new List<string[]> ();
        public string[] extendedData = new string[6];
        public string[] TPMSData = new string[3];
        static Dictionary<string, long> timeDic = new Dictionary<string, long>();
        static bool instruction0516 = true;
        static bool instruction8792 = true;
        static bool instruction9201 = true;
        static bool instruction9195 = true;
        //public int vibration, testclose = 10;
        //public double obdvolt;
        //public string wifistatus;
        //public static string latitude, longitude, eg_load, eg_close;
        //DateTime date = DateTime.MinValue, time = DateTime.MinValue, localDate = DateTime.Now;

        public idkey[] getIdKey = new idkey[10];

        public struct idkey
        {
            public string trackerId;
            public string deviceId;
            public string deviceKey;
            public DateTime time;
        }
        public void putIDKEY()
        {
            //getIdKey[0].trackerId = "100170110019";
            getIdKey[0].trackerId = "104170829004";
            getIdKey[0].deviceId = "TP-G1---RAY-9201";
            getIdKey[0].deviceKey = "pxPJ+pSbgvLjDytSsvTB2LUP+eQnN/ZrkLgZB0LKUQI=";
            getIdKey[0].time = DateTime.UtcNow;

            //getIdKey[1].trackerId = "100170110018";
            getIdKey[1].trackerId = "104170829006";
            getIdKey[1].deviceId = "TP-G1---RAR-0516";
            getIdKey[1].deviceKey = "dYCDu3gkPoC3g5P7pDs00Q==";
            getIdKey[1].time = DateTime.UtcNow;

            //getIdKey[2].trackerId = "100170110015";
            getIdKey[2].trackerId = "104170829001";
            getIdKey[2].deviceId = "TP-G1---RAY-9195";
            getIdKey[2].deviceKey = "cExfkIyQOvjxu3iQm/amxQ==";
            getIdKey[2].time = DateTime.UtcNow;

            //getIdKey[3].trackerId = "100170110014";
            getIdKey[3].trackerId = "104170829002";            
            getIdKey[3].deviceId = "TP-G3---RBS-8792";
            getIdKey[3].deviceKey = "n/HouTERpQmG1POR9lDb7A==";
            getIdKey[3].time = DateTime.UtcNow;

            getIdKey[4].trackerId = "100170110013";
            getIdKey[4].deviceId = "DCH_OBD_Device";
            getIdKey[4].deviceKey = "iKzOitn61/o8R0rH1yHmO4bLcWwnBFYz+/nOHGOCOTg=";
            getIdKey[4].time = DateTime.UtcNow;
            //微軟用
        }
        public void parserDATA(string data, Socket handler)
        {
            //將資料轉回原始狀態
            data = data.Replace("7D02", "7E");
            data = data.Replace("7D01", "7D");
            //parser 基本資訊,以下均為切割字串後,對應文件上的型態做轉碼
            string[] positioningData = new string[18];
            DateTime date = DateTime.MinValue, time = DateTime.MinValue;
            string latitude = null, longitude = null, eg_load = null, eg_close = null;
            int start = 2, i = 0, j = 0;
            string str = null;
            int[] cutTimes = { 4, 4, 12, 2, 6, 6, 8, 9, 1, 2, 2, 2, 2, 8, 2, 8, 4, 10 };
            string[] cutName = { "id", "attributes", "deviceid", "serial_number", "date", "time", "latitude", "longitude", "位指示", "speed", "direction", "GSM", "GPS", "mileage", "sensor_power", "CELL ID", "LAC ID", "Reserved" };
            int[] cutType = { 16, 2, 16, 10, 16, 16, 16, 16, 2, 10, 10, 10, 10, 10, 10, 10, 16, 16, 16 };
            double latitudenumber = 0.0, longitudenumber = 0.0;
            StringBuilder messageString = new StringBuilder("");
            Random test = new Random();
            foreach (var ct in cutTimes)
            {
                if (cutType[i] != 16)
                {
                    str = hexToConvert(data.Substring(start, cutTimes[i]), cutType[i]);
                }
                //
                if (i == 4)
                {
                    if (DateTime.TryParseExact(data.Substring(start, cutTimes[i]), "ddMMyy", null, System.Globalization.DateTimeStyles.None, out date))
                    {
                        //TryParseExact轉換成功
                        //positioningData[j++] = data.Substring(start, cutTimes[i]);
                        positioningData[j++] = date.ToString("yyyy/MM/dd");
                        messageString.Append("\"" + cutName[i] + "\":\"" + date.ToString("yyyy/MM/dd") + "\",");
                    }
                }
                else if (i == 5)
                {
                    if (DateTime.TryParseExact(data.Substring(start, cutTimes[i]), "HHmmss", null, System.Globalization.DateTimeStyles.None, out time))
                    {
                        //TryParseExact轉換成功
                        //positioningData[j++] = data.Substring(start, cutTimes[i]);
                        positioningData[j++] = time.ToString("HH:mm:ss");
                        messageString.Append("\"" + cutName[i] + "\":\"" + time.ToString("HH:mm:ss") + "\",");
                    }
                }
                else if (i == 6)
                {
                    if (double.Parse(data.Substring(start, cutTimes[i])) != 0)
                        latitude = data.Substring(start, cutTimes[i]);
                    else
                    {
                        positioningData[j++] = null;
                    }
                }
                else if (i == 7)
                {
                    if (double.Parse(data.Substring(start, cutTimes[i])) != 0)
                        longitude = data.Substring(start, cutTimes[i]);
                    else
                    {
                        positioningData[j++] = null;
                    }
                }
                else if (i == 8 && longitude != null && latitude != null)
                {
                    latitudenumber = int.Parse(latitude.Substring(0, 2)) + ((int.Parse(latitude.Substring(2, 6)) * 0.0001) / 60);
                    longitudenumber = int.Parse(longitude.Substring(0, 3)) + ((int.Parse(longitude.Substring(3, 6)) * 0.0001) / 60);
                    //
                    str = hexToConvert(data.Substring(start, cutTimes[i]), cutType[i]);
                    str = str.PadLeft(4, '0');

                    if (str.Substring(1, 1) == "1")
                    {
                        positioningData[j++] = longitudenumber.ToString("#0.000000");
                        messageString.Append("\"longitude\":\"").Append(longitudenumber.ToString("#0.000000") + "\",");
                    }
                    else if (str.Substring(1, 1) == "0")
                    {
                        positioningData[j++] = "-" + longitudenumber.ToString("#0.000000");
                        messageString.Append("\"longitude\":\"-").Append(longitudenumber.ToString("#0.000000") + "\",");
                    }
                    //
                    if (str.Substring(2, 1) == "1")
                    {
                        positioningData[j++] = latitudenumber.ToString("#0.000000");
                        messageString.Append("\"latitude\":\"").Append(latitudenumber.ToString("#0.000000") + "\",");
                    }
                    else if (str.Substring(2, 1) == "0")
                    {
                        positioningData[j++] = "-" + latitudenumber.ToString("#0.000000");
                        messageString.Append("\"latitude\":\"-").Append(latitudenumber.ToString("#0.000000") + "\",");
                    }
                }
                else if (i == 9)
                {
                    double spend = int.Parse(str) * 1.85;
                    positioningData[j++] = spend.ToString();
                    messageString.Append("\"" + cutName[i] + "\":\"" + spend + "\",");
                }
                else if (i == 10)
                {
                    int direction = int.Parse(str) * 2;
                    positioningData[j++] = direction.ToString();
                    messageString.Append("\"" + cutName[i] + "\":\"" + direction + "\",");
                }
                else
                {
                    if (cutType[i] != 16)
                    {
                        if (i == 1)
                            str = str.PadLeft(16, '0');
                        positioningData[j++] = str;
                        messageString.Append("\"" + cutName[i] + "\":\"" + str + "\",");
                    }
                    else
                    {
                        try
                        {
                            positioningData[j++] = data.Substring(start, cutTimes[i]);
                            messageString.Append("\"" + cutName[i] + "\":\"" + data.Substring(start, cutTimes[i]) + "\",");
                        }
                        catch (Exception e)
                        {
                            log.Fatal(e.ToString());
                            log.Fatal(data + "->" + start + "," + cutTimes[i]);
                        }
                    }
                }
                start += cutTimes[i++];
            }
            if (positioningData[1].Substring(0, 1) == "1")
            {
                int NumberChars = 0;
                string msg = "";
                try
                {
                    int number = int.Parse(positioningData[3]) + 1;
                    if (number.ToString("X").Length > 2)
                    {
                        number = 0;
                    }
                    msg = "44010003" + positioningData[2] + number.ToString("X").PadLeft(2, '0') + positioningData[0] + "00";
                    string XOR = checkEORCode(msg);
                    
                    msg = msg + XOR.Substring(XOR.Length - 2, 2);
                    msg = msg.Replace("7D", "7D01");
                    msg = msg.Replace("7E", "7D02");
                    msg = "7E" + msg + "7E";

                    NumberChars = msg.Length;
                    byte[] bytes = new byte[NumberChars / 2];
                    for (int ii = 0; ii < NumberChars; ii += 2)
                    {
                        bytes[ii / 2] = Convert.ToByte(msg.Substring(ii, 2), 16);
                    }
                    Program.Send(handler, bytes);
                    //Program.Send(handler, "(700160818000,1,001,BASE,11,123)");
                }
                catch (Exception e)
                {
                    log.Fatal("NumberChars:" + NumberChars);
                    log.Fatal("msg:" + msg);
                    log.Fatal(e.ToString());
                }
                //Program.Send(handler, "(700160818000,1,001,BASE,11,123)");
            }
            //下指令
            if (positioningData[2] == getIdKey[0].trackerId && instruction9201)
            {
                instruction9201 = false;
                log.Debug("send 9201, " + getIdKey[0].trackerId);
                Program.Send(handler, "(700160818000,1,001,GSENS,2,0,500,80)");
            }
            else if (positioningData[2] == getIdKey[1].trackerId && instruction0516)
            {
                instruction0516 = false;
                log.Debug("send 0516, " + getIdKey[1].trackerId);
                Program.Send(handler, "(700160818000,1,001,GSENS,2,0,500,80)");
            }
            else if (positioningData[2] == getIdKey[2].trackerId && instruction9195)
            {
                instruction9195 = false;
                log.Debug("send 9195, " + getIdKey[2].trackerId);
                Program.Send(handler, "(700160818000,1,001,GSENS,2,0,500,80)");
            }
            else if (positioningData[2] == getIdKey[3].trackerId && instruction8792)
            {
                instruction8792 = false;
                log.Debug("send 8792, " + getIdKey[3].trackerId);
                Program.Send(handler, "(700160818000,1,001,GSENS,2,0,500,100)");
            }

            if (positioningData[0] == "5501")
            {
                //string[] extendedData = new string[6];
                messageString.Append(parserSplitData(data.Substring(start), eg_load, eg_close));
                //parser 其他擴充內容
                messageString.Remove(messageString.Length - 1, 1);
                Console.WriteLine("parser sensor data:" + messageString);
                Random rnd = new Random();

                obdData.Reverse();
                //由舊至新
                foreach (string[] obd in obdData)
                {
                    try
                    {
                        if (int.Parse(obd[6]) > 0)//轉速大於0表示引擎運作中
                        {
                            eg_load = "on";
                        }
                        else
                        {
                            eg_load = "off";
                        }
                    }
                    catch (Exception e)
                    {
                        eg_load = "error";
                        log.Fatal(e.ToString());
                    }
                    if (positioningData[6] == null)
                        positioningData[6] = "null";

                    if (positioningData[7] == null)
                        positioningData[7] = "null";

                    string location = "fail";
                    if (positioningData[0] == "5501")
                        location = "success";
                    
                    //如果OBD時間為0,改傳裝置速度
                    if (double.Parse(obd[8])<0)
                    {
                        log.Debug("really Intake Air Temperature:" + obd[8] );
                        obd[8] = "0.0";
                    }
                    if (int.Parse(obd[3]) < 0) {
                        log.Debug("really Engine Coolant Temperature:" + obd[3]);
                        obd[3] = "0";
                    }

                    string telemetry = "{" +
                        "\"Device Speed\":\"" + positioningData[8] +
                        "\",\"EG_RPM\":\"" + obd[6] +
                        "\",\"mileage\":\"" + positioningData[12] +
                        "\",\"GPS Location\":\"" + location +
                        "\",\"DeviceID\":\"" + positioningData[2] +
                        "\",\"direction\":\"" + positioningData[9] +
                        "\",\"GSM\":\"" + positioningData[10] +
                        "\",\"GPS\":\"" + positioningData[11] +
                        "\",\"sensor_power\":\"" + positioningData[13] +
                        //"\",\"CELL ID\":\"" + positioningData[14] +
                        //"\",\"LAC ID\":\"" + positioningData[15] +
                        "\",\"Fuel System Status\":\"" + obd[1] +
                        "\",\"Calculated Engine Load\":\"" + obd[2] +
                        "\",\"Engine Coolant Temperature\":\"" + obd[3] +
                        "\",\"Fuel Pressure\":\"" + obd[4] +
                        "\",\"Intake Manifold Pressure\":\"" + obd[5] +
                        "\",\"Vehicle Speed\":\"" + obd[7] +
                        "\",\"Intake Air Temperature\":\"" + obd[8] +
                        "\",\"Air Flow Rate\":\"" + obd[9] +
                        "\",\"Throttle Position\":\"" + obd[10] +
                        "\",\"Battery voltage\":\"" + obd[11] +
                        //"\",\"SS\":\"" + obd[12] +
                        "\",\"Distance\":\"" + obd[13] +
                        "\",\"LONGITUDE\":\"" + (positioningData[6] ?? "0") +
                        "\",\"LATITUDE\":\"" + (positioningData[7] ?? "0") +
                        "\",\"Device Date\":\"" + positioningData[4] +
                        "\",\"Device Time\":\"" + positioningData[5] +
                        "\",\"Voltage Connect\":\"" + (extendedData[5] ?? null) +
                        "\"}";
                    //
                    Console.WriteLine("parser obd data:" + telemetry);
                    log.Debug("parser sensor data:" + messageString);
                    log.Debug("parser obd data:" + telemetry);

                    //DateTime now = DateTime.UtcNow;
                    DateTime deviceTime = DateTime.Parse(positioningData[4] + " " + positioningData[5]);
                    //TimeSpan ts = now - deviceTime;

                    //log.Debug("Minutes:" + ts.TotalMinutes);
                    if (!string.IsNullOrEmpty(positioningData[6]) && !string.IsNullOrEmpty(positioningData[7]))
                    {
                        string deviceId = getDeviceId(positioningData[2]);

                        if (checkDeviceTime(deviceId, deviceTime))
                        {
                            log.Debug("move:" + deviceId + "->" + positioningData[2] + "," + positioningData[6] + "," + positioningData[7]);
                            IoTProcess.moveLocation(deviceId, double.Parse(positioningData[6]), double.Parse(positioningData[7]));
                        }
                    }
                    //改變DocumentDB

                    long tt = Convert.ToInt64((DateTime.Parse(positioningData[4] + " " + positioningData[5])).ToString("yyyyMMddHHmmss"));
                    if (timeDic.ContainsKey(positioningData[2]))
                    {
                        long nn = timeDic[positioningData[2]];
                        if (tt > nn)
                        {
                            if (obd[6] != "0" || (obd[6] == "0" && obd[7] == "0"))
                            {
                                //if (ts.TotalMinutes <= int.Parse(ConfigurationManager.AppSettings["timedeviation"]))
                                {
                                    sendToIoTHub(positioningData[2], telemetry);
                                    // 傳送data至iot hub
                                    Console.WriteLine("sendToIoTHub.");
                                }
                            }
                            timeDic.Remove(positioningData[2]);
                            timeDic.Add(positioningData[2], tt);
                        }
                    }
                    else
                    {
                        timeDic.Add(positioningData[2], tt);
                    }                   
                }
            }
        }
        private string checkEORCode(string message)
        {
            int temp = Convert.ToInt32(message.Substring(0, 2), 16);
            int j = message.Length;

            for (int i = 2; i < j - 1; i = i + 2)
            {
                temp = temp ^ Convert.ToInt32(message.Substring(i, 2), 16);
            }
            return temp.ToString("X").PadLeft(2, '0');
        }
        public string parserSplitData(string input, string eg_load, string eg_close)
        {
            //parser 擴展資料清單
            string id, length, data, result = "";
            try
            {
                for (int i = 0; i < input.Length - 4;)
                {
                    id = input.Substring(i, 2);
                    i += 2;
                    if (id == "07")
                    {
                        //擴展資料
                        length = hexToConvert(input.Substring(i, 2), 10);
                        i += 2;
                        data = input.Substring(i, int.Parse(length) * 2);
                        i += int.Parse(length) * 2;
                        extendedData[0] = id;
                        extendedData[1] = length;
                        extendedData[2] = data;
                        result += parserExtended(int.Parse(length), data);
                    }
                    else if (id == "08")
                    {
                        //OBD資料
                        length = hexToConvert(input.Substring(i, 2), 10);
                        i += 2;
                        data = input.Substring(i, int.Parse(length) * 2);
                        i += int.Parse(length) * 2;
                        string whichmodule = data.Substring(0, 2);
                        string obdtype = data.Substring(2, 2);//01表示當前資料為前10秒的OBD資料,若為0則表示只有一條OBD資料
                        string obdUpLoad = hexToConvert(data.Substring(4, 4), 2).PadLeft(16, '0');//表示obd那些資料有被上傳,詳細請查閱pdf 
                        string obdErrorCode = data.Substring(8, 32);

                        //最多可能有10條OBD資料
                        for (int j = 40; j < (int.Parse(length) * 2); j = j + 36)
                        {
                            result += parserobd(data.Substring(j, 36), eg_load, eg_close);
                        }
                    }
                    else if (id == "09")
                    {
                        //TPMS資料,需要加購裝置
                        length = input.Substring(i, 2);
                        i += 2;
                        data = input.Substring(i, int.Parse(length) * 2);
                        i += int.Parse(length) * 2;
                        TPMSData[0] = id;
                        TPMSData[1] = length;
                        TPMSData[2] = data;
                        result += ("\"TPMSData\":\"" + TPMSData[2] + "\",");
                    }
                    else
                    {
                        log.Error("wrong length, " + id + "," + input);
                        i = input.IndexOf("07");
                    }
                }
            }
            catch (Exception e)
            {
                log.Fatal("input:" + input);
                log.Fatal(e.ToString());
            }
            return result;
        }
        private string parserobd(string data,string eg_load,string eg_close)
        {
            StringBuilder messageString = new StringBuilder("");
            int testclose;
            string[] obd = new string[18];
            string[] obdParser = new string[14];
            int obdnum = 0;
            DateTime localDate = DateTime.Now;
            try
            {
                for (int j = 0; j < data.Length; j = j + 2)
                {
                    obd[obdnum] = data.Substring(j, 2);
                    obdnum++;
                }
                //parser OBD資料
                if (obd[0] == "50")
                {
                    obdParser[0] = "P";
                    //messageString.Append("Passenger car\",");
                    //obdData[14] = (double.Parse(hexToConvert(NormalMessage.Substring(24, 2), 10)) / 10).ToString();
                }
                else if (obd[0] == "48")
                {
                    obdParser[0] = "H";
                    //messageString.Append("Heavy duty vehicles\",");
                    //obdData[14] = ((double.Parse(hexToConvert(NormalMessage.Substring(24, 2), 10)) + 100) / 10).ToString();
                }
                else
                {
                    Console.WriteLine("Other obd Data.");
                    log.Error("Other obd Data." + obd[0]);
                    obdParser[0] = "null";
                }
                messageString.Append("\"Cartype\":\"" + obdParser[0] + ",");

                string fuelSystemStatus = hexToConvert(obd[1], 2).PadLeft(8, '0');
                obdParser[1] = fuelSystemStatus;
                messageString.Append("\"Fuel System Status\":\"" + obdParser[1] + "\",");

                obdParser[2] = (double.Parse(hexToConvert(obd[2], 10)) * 100 / 255).ToString("#0.0");
                messageString.Append("\"Calculated Engine Load\":\"" + obdParser[2] + "\",");

                if (int.Parse(hexToConvert(obd[3], 10)) == 0)
                    obdParser[3] = "0";
                else
                    obdParser[3] = (int.Parse(hexToConvert(obd[3], 10)) - 40).ToString();
                messageString.Append("\"Engine Coolant Temperature\":\"" + obdParser[3] + "\",");

                obdParser[4] = (int.Parse(hexToConvert(obd[4], 10)) * 3).ToString();
                messageString.Append("\"Fuel Pressure\":\"" + obdParser[4] + "\",");

                obdParser[5] = hexToConvert(obd[5], 10);
                messageString.Append("\"Intake Manifold Pressure\":\"" + obdParser[5] + "\",");

                int RPMA = int.Parse(hexToConvert(obd[6], 10));
                int RPMB = int.Parse(hexToConvert(obd[7], 10));
                obdParser[6] = (RPMA * 256 + RPMB).ToString();
                messageString.Append("\"EG_RPM\":\"" + obdParser[6] + "\",");

                obdParser[7] = hexToConvert(obd[8], 10);
                messageString.Append("\"Vehicle Speed\":\"" + obdParser[7] + "\",");


                if (int.Parse(hexToConvert(obd[9], 10)) == 0)
                    obdParser[8] = "0.0";
                else
                    obdParser[8] = (int.Parse(hexToConvert(obd[9], 10)) - 40).ToString("#0.0");
                messageString.Append("\"Intake Air Temperature\":\"" + obdParser[8] + "\",");

                obdParser[9] = (int.Parse(hexToConvert(obd[10], 10)) * 3).ToString();
                messageString.Append("\"Air Flow Rate\":\"" + obdParser[9] + "\",");

                obdParser[10] = (double.Parse(hexToConvert(obd[11], 10)) * 100 / 255).ToString("#0.00");
                messageString.Append("\"Throttle Position\":\"" + obdParser[10] + "\",");

                if (obdParser[0] == "P")
                {
                    obdParser[11] = (int.Parse(hexToConvert(obd[12], 10)) / 10).ToString();
                    messageString.Append("\"Battery voltage\":\"" + obdParser[11] + "\",");

                    obdParser[12] = hexToConvert(obd[13], 2).PadLeft(8, '0');
                    messageString.Append("\"SS\":\"" + obdParser[12] + "\",");
                }
                else if (obdParser[0] == "H")
                {
                    obdParser[11] = ((int.Parse(hexToConvert(obd[12], 10)) + 100) / 10).ToString();
                    messageString.Append("\"Battery voltage\":\"" + obdParser[11] + "\",");

                    obdParser[12] = hexToConvert(obd[13], 2).PadLeft(8, '0');
                    messageString.Append("\"SS\":\"" + obdParser[12] + "\",");
                }
                else//例外先以一般車處理
                {
                    obdParser[11] = (int.Parse(hexToConvert(obd[12], 10)) / 10).ToString();
                    messageString.Append("\"Battery voltage\":\"" + obdParser[11] + "\",");

                    obdParser[12] = hexToConvert(obd[13], 2).PadLeft(8, '0');
                    messageString.Append("\"SS\":\"" + obdParser[12] + "\",");
                }

                int DH = int.Parse(hexToConvert(obd[14], 10));
                int DM = int.Parse(hexToConvert(obd[15], 10));
                int DL = int.Parse(hexToConvert(obd[16], 10));
                obdParser[13] = (DH * 256 * 256 + DM * 256 + DL).ToString();
                messageString.Append("\"Distance\":\"" + obdParser[13] + "\",");

                obdData.Add(obdParser);

                Random rnd = new Random();
                testclose = rnd.Next(0, 4);
                if (eg_load == null)
                    eg_load = localDate.ToString("yyyy/MM/dd HH:mm:ss.fff");

                if (obdParser[12].Substring(0, 1) == "1" || testclose == 1) //如果引擎故障
                {
                    eg_close = localDate.ToString("yyyy/MM/dd HH:mm:ss.fff");
                    eg_load = localDate.ToString("yyyy/MM/dd HH:mm:ss.fff");
                }
                else
                {
                    eg_close = null;
                }
            }
            catch (Exception e)
            {
                log.Fatal(e.ToString());
            }
            return messageString.ToString();
        }
        private string parserExtended(int length, string data)
        {
            //parser擴展狀態資料
            StringBuilder messageString = new StringBuilder("");
            string wifistatus=null;
            double obdvolt;
            int vibration;

            if (length == 5)
            {
                vibration = int.Parse(hexToConvert(data.Substring(0, 4), 10));
                messageString.Append("\"vibration\":\"" + vibration + "\",");
                extendedData[2] = vibration.ToString();
                obdvolt = int.Parse(hexToConvert(data.Substring(4, 2), 10)) * 0.1;
                messageString.Append("\"obdvolt\":\"" + obdvolt + "\",");
                extendedData[3] = obdvolt.ToString();
                wifistatus = data.Substring(6, 2);
                if (wifistatus == "00")
                {
                    messageString.Append("\"wifistatus\":\"off\",");
                }
                else if (wifistatus == "01")
                {
                    messageString.Append("\"wifistatus\":\"on\",");
                }
                else
                {
                    Console.WriteLine("Wrong wifi statc!");
                    log.Fatal("Wrong wifi statc!");
                }
                string IOtype = hexToConvert(data.Substring(8, 2), 2).PadLeft(8, '0');
                messageString.Append("\"GPIO Input1\":\"" + IOtype.Substring(0, 1) + "\",");
                messageString.Append("\"GPIO Input2\":\"" + IOtype.Substring(1, 1) + "\",");
                messageString.Append("\"GPIO Onput1\":\"" + IOtype.Substring(2, 1) + "\",");
                if (IOtype.Substring(3, 1) == "0")
                    messageString.Append("\"Engine switch\":\"off\",");
                else if (IOtype.Substring(3, 1) == "1")
                    messageString.Append("\"Engine switch\":\"on\",");
                else
                    log.Fatal("Wrong Engine switch." + IOtype.Substring(3, 1));

                if (IOtype.Substring(4, 1) == "0")
                    messageString.Append("\"Emergency brakes\":\"off\",");
                else if (IOtype.Substring(4, 1) == "1")
                    messageString.Append("\"Emergency brakes\":\"on\",");
                else
                    log.Fatal("Wrong Emergency brakes." + IOtype.Substring(3, 1));

                extendedData[4] = IOtype.Substring(5, 1); //外部電低電壓告警
                messageString.Append("\"External voltage Low\":" + extendedData[4] + ",");
                extendedData[5] = IOtype.Substring(6, 1);//外部電斷開告警
                messageString.Append("\"External voltage connect\":" + extendedData[5] + ",");
            }
            else
            {
                Console.WriteLine("New Extended Data.");
                log.Fatal("New Extended Data." + data);
            }
            return messageString.ToString();
        }
        public void sendToIoTHub(string trackerId, string data)
        {
            //string deviceId = ConfigurationManager.AppSettings["deviceId"];
            //string deviceKey = ConfigurationManager.AppSettings["deviceKey"];
            string deviceId = getDeviceId(trackerId);
            string deviceKey = getDeviceKey(trackerId);
            IoTProcess process = new IoTProcess(deviceId, deviceKey, data);
            log.Debug(" sendToIoTHub: " + trackerId + "," + deviceId + "," + deviceKey + "," + data);
            process.sendMessage();
        }
        public string getDeviceId(string TrackerId)
        {
            foreach (var i in getIdKey)
            {
                if (i.trackerId == TrackerId)
                    return i.deviceId;
            }
            return null;
        }
        public string getDeviceKey(string TrackerId)
        {
            foreach (var i in getIdKey)
            {
                if (i.trackerId == TrackerId)
                    return i.deviceKey;
            }
            return null;
        }
        public bool checkDeviceTime(string deviceId, DateTime time)
        {
            for (int i = 0; i < getIdKey.Length; i++)
            {
                if (getIdKey[i].deviceId == deviceId)
                {
                    TimeSpan deviation = time - getIdKey[i].time;
                    if (deviation.TotalMinutes <= 1)
                    {
                        getIdKey[i].time = time;
                        return true;
                    }
                    else
                    {
                        log.Debug("device time:" + time + ",old time:" + getIdKey[i].time);
                    }
                }
            }
            return false;
        }
        public static string hexToConvert(string strHex, int number)
        {
            if (string.IsNullOrEmpty(strHex))
            {
                Console.Write("hexToConvert no input!");
                log.Fatal("hexToConvert no input!");
                return null;
            }
            try
            {
                int intNum = Convert.ToInt32(strHex, 16);
                string strBinary = Convert.ToString(intNum, number);
                return strBinary;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                log.Fatal(ex.ToString());
                return null;
            }
        }
        public static string bytesToHexString(byte[] data)
        {
            string hex = BitConverter.ToString(data);
            hex = BitConverter.ToString(data).Replace("-", string.Empty);
            return hex;
        }
    }
}
