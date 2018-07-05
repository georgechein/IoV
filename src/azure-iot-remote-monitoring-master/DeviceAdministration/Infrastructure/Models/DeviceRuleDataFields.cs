using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public static class DeviceRuleDataFields
    {
        public static string TEMP
        {
            get
            {
                return "TEMP";
            }
        }

        //原為濕度
        public static string REAR_TPM
        {
            get
            {
                return "REAR_TPM";
            }
        }
        //insert DATA FIELD
        public static string FRONT_TPM
        {
            get
            {
                return "FRONT_TPM";
            }
        }
        //insert DATA FIELD
        public static string BETTERY_VOLT
        {
            get
            {
                return "BETTERY_VOLT";
            }
        }
        //public static string UpperTEMP
        //{
        //    get
        //    {
        //        return "UpperTEMP";
        //    }
        //}
        //insert DATA FIELD
        private static List<string> _availableDataFields = new List<string>
        {
            TEMP, REAR_TPM, FRONT_TPM,BETTERY_VOLT//,UpperTEMP 
        };

        public static List<string> GetListOfAvailableDataFields()
        {
            return _availableDataFields;
        }
    }
}
