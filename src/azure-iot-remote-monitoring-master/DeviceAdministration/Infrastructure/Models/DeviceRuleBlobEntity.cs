namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceRuleBlobEntity
    {
        public DeviceRuleBlobEntity(string deviceId)
        {
            DeviceId = deviceId;
        }

        public string DeviceId { get; private set; }
        public double? TEMP { get; set; }
        //public double? UpperTEMP { get; set; }
        public double? REAR_TPM { get; set; }
        public string TEMPRuleOutput { get; set; }
        public string REAR_TPMRuleOutput { get; set; }
        //insert DATA FIELD
        public double? FRONT_TPM { get; set; }
        public double? BETTERY_VOLT { get; set; }
        public string FRONT_TPMRuleOutput { get; set; }
        public string BETTERY_VOLTRuleOutput { get; set; }
    }
}
