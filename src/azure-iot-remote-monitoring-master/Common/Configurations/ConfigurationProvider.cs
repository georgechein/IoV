using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations
{
    public class ConfigurationProvider : IConfigurationProvider, IDisposable
    {
        readonly Dictionary<string, string> configuration = new Dictionary<string, string>();
        EnvironmentDescription environment = null;
        const string ConfigToken = "config:";
        bool _disposed = false;
        private Dictionary<string, string> _defaultConfig = null;

        public string GetConfigurationSettingValue(string configurationSettingName)
        {
            return this.GetConfigurationSettingValueOrDefault(configurationSettingName, string.Empty);
        }

        public string GetConfigurationSettingValueOrDefault(string configurationSettingName, string defaultValue)
        {
            if (!this.configuration.ContainsKey(configurationSettingName))
            {
                string configValue = CloudConfigurationManager.GetSetting(configurationSettingName);
                bool isEmulated = Environment.CommandLine.Contains("iisexpress.exe") ||
                    Environment.CommandLine.Contains("w3wp.exe") ||
                    Environment.CommandLine.Contains("WebJob.vshost.exe");

                if (isEmulated && (configValue != null && configValue.StartsWith(ConfigToken, StringComparison.OrdinalIgnoreCase)))
                {
                    //if (environment == null)
                    //{
                    //    LoadEnvironmentConfig();
                    //}
                    //configValue = environment.GetSetting(
                    //    configValue.Substring(configValue.IndexOf(ConfigToken, StringComparison.Ordinal) + ConfigToken.Length));
                    configValue = System.Environment.GetEnvironmentVariable(configurationSettingName);
                    if (string.IsNullOrEmpty(configValue))
                        configValue = GetDefaultValue(configurationSettingName);
                    Console.WriteLine(string.Format("env:{0}={1}", configurationSettingName, configValue));
                }
                try
                {
                    this.configuration.Add(configurationSettingName, configValue);
                    Console.WriteLine(string.Format("config:{0}={1}", configurationSettingName, configValue));
                }
                catch (ArgumentException)
                {
                    // at this point, this key has already been added on a different
                    // thread, so we're fine to continue
                }
            }

            return this.configuration[configurationSettingName];
        }

        void LoadEnvironmentConfig()
        {
            var executingPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            // Check for build_output
            int buildLocation = executingPath.IndexOf("Build_Output", StringComparison.OrdinalIgnoreCase);
            if (buildLocation >= 0)
            {
                string fileName = executingPath.Substring(0, buildLocation) + "local.config.user";
                if (File.Exists(fileName))
                {
                    this.environment = new EnvironmentDescription(fileName);
                    return;
                }
            }

            // Web roles run in there app dir so look relative
            int location = executingPath.IndexOf("Web\\bin", StringComparison.OrdinalIgnoreCase);

            if (location == -1)
            {
                location = executingPath.IndexOf("WebJob\\bin", StringComparison.OrdinalIgnoreCase);
            }
            if (location >= 0)
            {
                string fileName = executingPath.Substring(0, location) + "..\\local.config.user";
                if (File.Exists(fileName))
                {
                    this.environment = new EnvironmentDescription(fileName);
                    return;
                }
            }

            throw new ArgumentException("Unable to locate local.config.user file.  Make sure you have run 'build.cmd local'.");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (environment != null)
                {
                    environment.Dispose();
                }
            }

            _disposed = true;
        }

        ~ConfigurationProvider()
        {
            Dispose(false);
        }
        private string GetDefaultValue(string key)
        {
            if (_defaultConfig == null)
                _defaultConfig = GetDefaultSetting();
            return _defaultConfig[key];
        }
        private Dictionary<string, string> GetDefaultSetting()
        {
            var dic = new Dictionary<string, string>();
            dic.Add("ida.AADClientId", "4e0a344b-57b8-45ed-aa63-f0293ad66f69");
            dic.Add("ida.AADInstance", "https://login.microsoftonline.com/{0}");
            dic.Add("ida.AADTenant", "2597e416-2f81-408e-9e2e-b6780898d718");
            dic.Add("iotHub.HostName", "tsti-iov96b3b.azure-devices.net");
            dic.Add("iotHub.ConnectionString", "HostName=tsti-iov96b3b.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=BUvDQDO1a0sm8N61Kqqoo+amia8uK/SD5Iv6awPq6Tc=");
            dic.Add("docdb.EndpointUrl", "https://tsti-iov1d39d.documents.azure.com:443/");
            dic.Add("docdb.PrimaryAuthorizationKey", "l5koEixrrTch3lZYK6qXzkra8ICH71SPgrta8RmXt9Kmb9MXi9TPHcEEz3DJ6pDms53XnL0vu1do5xAyGaPpZg==");
            dic.Add("eventHub.ConnectionString", "Endpoint=sb://tsti-iov.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=IOBWkKurWl75FMwjzEBmh3YmzxmyH52UAC0A2VbX8DE=");
            dic.Add("eventHub.StorageConnectionString", "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=tstiiov;AccountKey=M7qtqLlLLvkr2CR4whzv4ZteE7P1d+p4VR5jrlnBHHkzruzRmARNdc1b5a8b0o9X1JSNMOdMQlfrA+a6F8SqeA==");
            dic.Add("RulesEventHub.Name", "tsti-iov-ehruleout");
            dic.Add("RulesEventHub.ConnectionString", "Endpoint=sb://tsti-iov.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=IOBWkKurWl75FMwjzEBmh3YmzxmyH52UAC0A2VbX8DE=");
            dic.Add("device.TableName", "DeviceList");
            dic.Add("device.StorageConnectionString", "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=tstiiov;AccountKey=M7qtqLlLLvkr2CR4whzv4ZteE7P1d+p4VR5jrlnBHHkzruzRmARNdc1b5a8b0o9X1JSNMOdMQlfrA+a6F8SqeA==");
            dic.Add("ObjectTypePrefix", "");
            dic.Add("SolutionName", "tsti-IoV");
            dic.Add("MapApiQueryKey", "As1Cmu0Rd9jFWtA1lK0G-NbifnW9mVrYdCg-fvK-jotc1q5rIbGv7jqWdvzU-J9G");
            dic.Add("AzureAccountName", "");
            dic.Add("WEBSITE_HOSTNAME", "iov.tsti.local");
            return dic;
        }
    }
}
