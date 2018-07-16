using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace InternetOfVehicles
{
    class DeviceInfo
    {
        public DeviceProperties DeviceProperties { get; set; }
        public SystemProperties SystemProperties { get; set; }
        public Commands[] Commands { get; set; }
        public CommandHistory[] CommandHistory { get; set; }

        public string IsSimulatedDevice { get; set; }
        public string id { get; set; }
        public Telemetry[] Telemetry { get; set; }
        public string Version { get; set; }
        public string ObjectType { get; set; }
        public string ObjectName { get; set; }

        public string IoTHub { get; set; }
        public Twin Twin { get; set; }

    }

    class DeviceProperties
    {
        public string DeviceID { get; set; }
        public string HubEnabledState { get; set; }
        public string CreatedTime { get; set; }
        public string DeviceState { get; set; }
        public string UpdatedTime { get; set; }
        public string Manufacturer { get; set; }
        public string ModelNumber { get; set; }
        public string SerialNumber { get; set; }
        public string FirmwareVersion { get; set; }
        public string AvailablePowerSources { get; set; }
        public string PowerSourceVoltage { get; set; }
        public string BatteryLevel { get; set; }
        public string MemoryFree { get; set; }
        public string HostName { get; set; }
        public string Platform { get; set; }
        public string Processor { get; set; }
        public string InstalledRAM { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

    }

    class SystemProperties
    {
        public string ICCID { get; set; }
    }

    class Commands
    {
        public string Command { get; set; }
    }
    class CommandHistory
    {
        public string commandHistory { get; set; }
    }


    class Telemetry
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Type { get; set; }
    }

    class Twin
    {
        public string deviceId { get; set; }
        public string etag { get; set; }
        public tags tags { get; set; }
        public properties properties { get; set; }
    }
    class tags
    {
        public string Building { get; set; }
        public string Floor { get; set; }
    }

    class properties
    {
        public desired desired { get; set; }
        public reported reported { get; set; }
    }
    class desired
    {
        public string desire { get; set; }
    }
    class reported
    {
        public string report { get; set; }
    }
}
