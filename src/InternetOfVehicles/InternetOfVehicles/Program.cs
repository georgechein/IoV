using log4net;
using log4net.Config;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace InternetOfVehicles
{
    class Program
    {
        static Socket listener;
        private static ManualResetEvent allDone = new ManualResetEvent(false);
        private static bool listening = true;
        static bool time = false;
        static int samenum = 0;
        static string tempinput = null;
        static string localIp = ConfigurationManager.AppSettings["ip"];
        static int port = int.Parse(ConfigurationManager.AppSettings["port"]);
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            try
            {
                XmlConfigurator.Configure(new System.IO.FileInfo("./log4net.config"));
                log.Info("Startup Socket Server.");
                log.Info("IP:" + localIp + ", port:" + port);
                Console.WriteLine("IP:" + localIp + ", port:" + port);
                new Thread(StartListening).Start();
                //Console.ReadLine();
            }
            catch (Exception e)
            {
                log.Fatal(e.ToString());
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// 開啟Socket監聽port
        /// </summary>
        private static void StartListening()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(localIp), port);
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(65535);
                while (listening)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();
                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                log.Fatal(e.ToString());
                Environment.Exit(0);
            }
        }
        private static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                allDone.Set();
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
            catch (Exception e)
            {
                log.Fatal(e.ToString());
                Environment.Exit(0);
            }
        }
        private static void ReadCallback(IAsyncResult ar)
        {
            StateObject state = null;
            Socket handler = null;
            try
            {
                state = (StateObject)ar.AsyncState;
                handler = state.workSocket;
                int bytesRead = handler.EndReceive(ar);
                int test = 0;
          
                if (bytesRead > 0)
                {
                    byte[] buffer = new byte[bytesRead];
                    Array.Copy(state.buffer, 0, buffer, 0, bytesRead);
                    string hexString = bytesToHexString(buffer);
                    log.Debug("raw data:" + hexString);
                    Console.Clear();
                    Console.WriteLine("input:" + hexString);
                    //
                    //Write(DateTime.Now.ToString() + " " + hexString);
                    //寫到記事本做紀錄
                    //RawDataToDB.Thread tws = new RawDataToDB.Thread(hexString);
                    //Thread myThread = new Thread(new ThreadStart(tws.ThreadProc));
                    //myThread.Start();
                    //寫到DB做紀錄

                    if (hexString != tempinput)
                    {
                        tempinput = hexString;
                        samenum = 0;
                    }
                    else
                    {
                        samenum++;
                    }

                    if (hexString.IndexOf("2C3030312C424153452C322C54494D4529", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        //AT100G要求授時
                        Console.WriteLine(fromHexString(hexString));
                        if (time == false && test < 1)
                        {
                            //之前授時過,不要短時間內重複授時,等候AT100G更新,除非沒更新反應,繼續要求授時
                            Send(handler, "(700160818000,1,001,BASE,2, " + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ")");
                        }
                        else if (test > 10)
                        {
                            test = 0;
                        }
                        test++;
                        //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }
                    else
                    {
                        if (hexString.IndexOf("2C322C3030312C424153452C313129", StringComparison.OrdinalIgnoreCase) < 0 || hexString.Substring(0, 5) == "7E550")
                        {
                            //排除當系統收到debug指令的回覆(不需回應)
                            //處理AT100G回應                            
                            dataProcess(bytesRead, buffer, hexString, handler);
                            //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);

                            //Send(handler, "(700160818000,1,001,BASE,11,123)");
                            //此為回應AG100T的debug指令(所有回應都可以回復此指令)
                        }
                    }
                    if (samenum > 10)//Debug用
                    {
                        Write("重複" + tempinput);
                        Send(handler, "(700160818000,1,001,BASE,11,123)");
                        samenum = 0;
                    }
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    //處理Client端"正常"斷線的事件
                    // No data received.
                    log.Debug("No data received.");
                    Console.WriteLine("No data received.");
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    //Console.WriteLine("Waiting for a connection...");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                log.Fatal(e.ToString());
                if (handler != null)
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    handler.Dispose();
                }
                Console.WriteLine("Clent Disconnected...");
            }
        }
        private static void dataProcess(int bytesRead,byte[] buffer, string hexString, Socket handler)
        {
            try
            {
                if (hexString== "283130303137303131303031392C322C3030312C424153452C322C2032303137")
                {
                    //收到成功授時
                    time = true;
                }
                if (clientReturn(buffer, bytesRead))
                {
                    //確認AG100G上傳資料是否為回應指令結果
                    Console.WriteLine("Get Client return." + hexString);
                    log.Fatal("Get Client return." + hexString);
                }
                else if (buffer[bytesRead - 1] == 0x7E && buffer[0] == 0x7E)
                {
                    if (hexString.Substring(2, 4) == "5501" || hexString.Substring(2, 4) == "5502")
                    {
                        //Console.WriteLine("定位資料.");
                        //log.Debug("定位資料.");
                        if (hexString.Substring(2, 4) == "5501")
                        {
                            Console.WriteLine("即時資料.");
                            log.Debug("5501.");
                        }
                        if (hexString.Substring(2, 4) == "5502")
                        {
                            Console.WriteLine("歷史資料資料.");
                            log.Debug("5502.");
                        }

                        if (hexString.Length >= 94)
                        {
                            BaseProcess myData = new BaseProcess();
                            if (hexString.Length >= 220)
                            {
                                string[] strs = hexString.Split(new string[] { "7E" }, StringSplitOptions.RemoveEmptyEntries);

                                if (strs[0].Length >= 94)
                                {
                                    if (strs[0] != null)
                                    {
                                        strs[0] = "7E" + strs[0] + "7E";
                                        //parser data
                                        myData.putIDKEY();
                                        myData.parserDATA(strs[0], handler);
                                        log.Debug("Paser Data End.");
                                    }
                                }
                            }
                            else
                            {
                                myData.putIDKEY();
                                myData.parserDATA(hexString, handler);
                                log.Debug("Paser Data End.");
                            }
                        }
                        else
                        {
                            //紀錄例外訊息
                            Console.WriteLine("Other data Length!");
                            log.Fatal("Other data Length!");
                        }
                    }
                    else if (hexString.Substring(2, 4) == "4001")
                    {
                        Console.WriteLine("終端心跳包.");
                        log.Debug("heart beat: " + hexString);
                    }
                    else
                    {
                        //不在協議中的輸入
                        Console.WriteLine("其他消息ID. " + hexString.Substring(2, 4));
                        log.Fatal("其他消息ID. " + hexString.Substring(2, 4));
                    }
                }
                else
                {
                    // Not all data received.
                    Console.WriteLine("Not all data received.");
                    log.Debug(buffer[0] + "," + buffer[bytesRead - 1]);
                    log.Debug("Not all data received.");
                    log.Debug(hexString);
                    Console.WriteLine(hexString);
                    //Write(hexString);
                    //額外寫到記事本做紀錄
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                log.Fatal(e.ToString());
            }
        }
        public static bool clientReturn(byte[] buffer,int bytesRead)
        {
            ////回復指令結果的訊息不會包7EXXXX7E,均為28XXXXX29,28為"(",29為")"
            if (buffer[bytesRead - 1] != 0x7E && buffer[0] != 0x7E)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void Write(string data)
        {            
            string day = DateTime.Now.ToString("yyyyMMdd");
            FileStream fs = new FileStream(ConfigurationManager.AppSettings["exception"] + day + ".txt", FileMode.Append);
            StreamWriter sw = new StreamWriter(fs);
            try
            {
                //開始寫入
                sw.WriteLine(data);
                //清空緩衝區
                sw.Flush();
            }
            catch (Exception e)
            {
                log.Fatal(e.ToString());
            }
            finally
            {
                //關閉流
                sw.Close();
                fs.Close();
            }
        }
        public static void Send(Socket handler, byte[] byteData)
        {
            try
            {
                //
                //Write(BitConverter.ToString(byteData));
                //額外寫到記事本做紀錄
                Console.WriteLine("Send:" + BitConverter.ToString(byteData));
                log.Debug("Send:" + BitConverter.ToString(byteData));
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch (Exception e)
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                handler.Dispose();
                log.Fatal(e.ToString());
            }
        }
        public static void Send(Socket handler, string data)
        {
            try
            {
                //Write(data);
                //額外寫到記事本做紀錄
                // Convert the string data to byte data using ASCII encoding.
                byte[] byteData = Encoding.ASCII.GetBytes(data);
                Console.WriteLine("Send:" + data);
                Console.WriteLine("to byte:" + BitConverter.ToString(byteData));
                log.Debug("Send:" + data);
                log.Debug("to byte:" + BitConverter.ToString(byteData));
                // Begin sending the data to the remote device.
                handler.BeginSend(byteData, 0, byteData.Length, 0,new AsyncCallback(SendCallback), handler);
            }
            catch (Exception e)
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                handler.Dispose();
                log.Fatal(e.ToString());
            }
        }
        private static void SendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            Socket handler = (Socket)ar.AsyncState;
            try
            {
                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception e)
            {
                log.Fatal(e.ToString());
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                handler.Dispose();
            }
        }
        public static string bytesToHexString(byte[] data)
        {
            string hex = BitConverter.ToString(data);
            hex = BitConverter.ToString(data).Replace("-", string.Empty);

            int start = hex.IndexOf("7E");
            int end = hex.LastIndexOf("7E") + 2;

            if (end > start && start >= 0 && end - start > 0 && end - start <= hex.Length)
            {
                return hex.Substring(start, end - start);
            }
            else
            {
                return hex;
            }
        }
        public static string fromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.ASCII.GetString(bytes);
        }
    }

    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 10240;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
    }
}