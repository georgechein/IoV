using Autofac;
using log4net;
using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;


namespace InternetOfVehicles
{
    public class IoTProcess
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(IoTProcess));
        public static string primaryKey = null;
        protected static IContainer appContainer = null;
        //static DeviceClient deviceClient;

        private static string iotHubHostName = ConfigurationManager.AppSettings["iotHub.HostName"];
        private static string iotHubConnectionString = ConfigurationManager.AppSettings["iotHub.ConnectionString"];

        private string _deviceId = null;
        private string _deviceKey = null;
        private string _messageString = null;

        private static string endpoint = ConfigurationManager.AppSettings["docdb.EndpointUrl"];   //"https://tsti-iov1d39d.documents.azure.com:443/";
        private static string authKey = ConfigurationManager.AppSettings["docdb.PrimaryAuthorizationKey"];  //"l5koEixrrTch3lZYK6qXzkra8ICH71SPgrta8RmXt9Kmb9MXi9TPHcEEz3DJ6pDms53XnL0vu1do5xAyGaPpZg ==";

        private static string databaseName = ConfigurationManager.AppSettings["docdb.DatabaseId"];  //"DevMgmtDB";
        private static string collectionName = ConfigurationManager.AppSettings["docdb.DocumentCollectionId"]; //"DevMgmtCollection";


        static void BuildContainer()
        {
            var builder = new ContainerBuilder();
            appContainer = builder.Build();
        }
        public static void moveLocation(string devicdID, double Longitude, double Latitude)
        {
            //string devicdID = "TP-G1-3";
            try
            {
                DocumentClient client = new DocumentClient(new Uri(endpoint), authKey);
                DeviceInfo device = ExecuteSimpleQuery(client, databaseName, collectionName, devicdID);
                device.DeviceProperties.Longitude = Longitude;
                device.DeviceProperties.Latitude = Latitude;

                //device.DeviceProperties.Longitude = 121.526188;
                //device.DeviceProperties.Latitude = 25.068871;

                doReplacedeviceDocument(client, databaseName, collectionName, device.id, device);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
                log.Fatal(e.ToString());
                Environment.Exit(0);
            }
            System.Threading.Thread.Sleep(3000);
        }
        public IoTProcess(string deviceId, string deviceKey, string messageString) : base()
        {
            if (appContainer == null)
            {
                BuildContainer();
            }

            _deviceId = deviceId;
            _deviceKey = deviceKey;
            _messageString = messageString;
        }

        public void sendMessage()
        {
            try
            {
                DeviceClient deviceClient = DeviceClient.Create(iotHubHostName, new DeviceAuthenticationWithRegistrySymmetricKey(_deviceId, _deviceKey));
                sendCloudToDeviceMessageAsync(deviceClient, _deviceId, _messageString);
            }
            catch (Exception e)
            {
                log.Fatal(e.ToString());
            }
        }
        private async void sendCloudToDeviceMessageAsync(DeviceClient deviceClient, string deviceId, string msg)
        {
            try
            {
                var commandMessage = new Message(Encoding.ASCII.GetBytes(msg));
                await deviceClient.SendEventAsync(commandMessage);
            }
            catch (Exception e)
            {
                log.Fatal(e.ToString());
            }
        }

        static DeviceInfo ExecuteSimpleQuery(DocumentClient client, string databaseName, string collectionName, string devicdID)
        {
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1 };


            // Here we find the Andersen device via its LastName
            IQueryable<DeviceInfo> deviceQuery = client.CreateDocumentQuery<DeviceInfo>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                    .Where(c => c.DeviceProperties.DeviceID == devicdID);

            // The query is executed synchronously here, but can also be executed asynchronously via the IDocumentQuery<T> interface
            //Console.WriteLine("Running LINQ query...");
            foreach (DeviceInfo device in deviceQuery)
            {
                //Console.WriteLine("\tRead {0}", device);
                return device;
            }

            //Console.ReadKey();
            return null;
        }

        static async void doReplacedeviceDocument(DocumentClient client, string databaseName, string collectionName, string documentId, DeviceInfo updatedDeviceInfo)
        {
            try
            {
                await ReplacedeviceDocument(client, databaseName, collectionName, documentId, updatedDeviceInfo);
            }
            catch (Exception e)
            {
                log.Fatal(e.ToString());
                Environment.Exit(0);
            }
        }

        // ADD THIS PART TO YOUR CODE
        static async Task ReplacedeviceDocument(DocumentClient client, string databaseName, string collectionName, string documentId, DeviceInfo updatedDeviceInfo)
        {
            try
            {
                await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, documentId), updatedDeviceInfo);
                //this.WriteToConsoleAndPromptToContinue("Replaced device {0}", deviceName);
            }
            catch (Exception e)
            {
                log.Fatal(e.ToString());
                Environment.Exit(0);
            }
        }

        static string ConvertStringToHex(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            var hexString = BitConverter.ToString(bytes);
            hexString = hexString.Replace("-", "");
            return hexString;
        }

        static string ConvertHexToString(string HexValue)
        {
            string StrValue = "";
            while (HexValue.Length > 0)
            {
                StrValue += System.Convert.ToChar(System.Convert.ToUInt32(HexValue.Substring(0, 2), 16)).ToString();
                HexValue = HexValue.Substring(2, HexValue.Length - 2);
            }
            StrValue = StrValue.Replace("\0", "");
            return StrValue;
        }
    }
}