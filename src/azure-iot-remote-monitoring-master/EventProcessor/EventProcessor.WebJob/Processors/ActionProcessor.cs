using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{

    public class ActionProcessor : IEventProcessor
    {
        private readonly IActionLogic _actionLogic;
        private readonly IActionMappingLogic _actionMappingLogic;
        private readonly IConfigurationProvider _configurationProvider;

        private int _totalMessages = 0;
        private Stopwatch _checkpointStopwatch;

        public ActionProcessor(
            IActionLogic actionLogic,
            IActionMappingLogic actionMappingLogic,
            IConfigurationProvider configurationProvider)
        {
            this.LastMessageOffset = "-1";
            _actionLogic = actionLogic;
            _actionMappingLogic = actionMappingLogic;
            _configurationProvider = configurationProvider;
        }

        public event EventHandler ProcessorClosed;

        public bool IsInitialized { get; private set; }

        public bool IsClosed { get; private set; }

        public bool IsReceivedMessageAfterClose { get; set; }

        public int TotalMessages
        {
            get { return _totalMessages; }
        }

        public CloseReason CloseReason { get; private set; }

        public PartitionContext Context { get; private set; }

        public string LastMessageOffset { get; private set; }

        public Task OpenAsync(PartitionContext context)
        {
            Trace.TraceInformation("ActionProcessor: Open : Partition : {0}", context.Lease.PartitionId);
            this.Context = context;
            _checkpointStopwatch = new Stopwatch();
            _checkpointStopwatch.Start();

            this.IsInitialized = true;

            return Task.Delay(0);
        }


        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            Trace.TraceInformation("ActionProcessor: In ProcessEventsAsync");

            foreach (EventData message in messages)
            {
                try
                {
                    Trace.TraceInformation("ActionProcessor: {0} - Partition {1}", message.Offset, context.Lease.PartitionId);
                    this.LastMessageOffset = message.Offset;

                    string jsonString = Encoding.UTF8.GetString(message.GetBytes());
                    IList<ActionModel> results = JsonConvert.DeserializeObject<List<ActionModel>>(jsonString);
                    if (results != null)
                    {
                        foreach (ActionModel item in results)
                        {
                            await ProcessAction(item);
                        }
                    }

                    ++_totalMessages;
                }
                catch (Exception e)
                {
                    Trace.TraceError("ActionProcessor: Error in ProcessEventAsync -- " + e.ToString());
                }
            }

            // checkpoint after processing batch
            try
            {
                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - ActionProcessor.ProcessEventsAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);
            }

            if (this.IsClosed)
            {
                this.IsReceivedMessageAfterClose = true;
            }
        }

        private async Task ProcessAction(ActionModel eventData)
        {
            if (eventData == null)
            {
                Trace.TraceWarning("Action event is null");
                return;
            }
            try
            {
                // NOTE: all column names from ASA come out as lowercase; see 
                // https://social.msdn.microsoft.com/Forums/office/en-US/c79a662b-5db1-4775-ba1a-23df1310091d/azure-table-storage-account-output-property-names-are-lowercase?forum=AzureStreamAnalytics 

                string deviceId = eventData.DeviceID;
                string ruleOutput = eventData.RuleOutput;
                
                if (ruleOutput.Equals("AlarmTEMP", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("ProcessAction: temperature rule triggered!");
                    double tempReading = eventData.Reading;

                    string tempActionId = await _actionMappingLogic.GetActionIdFromRuleOutputAsync(ruleOutput);

                    if (!string.IsNullOrWhiteSpace(tempActionId))
                    {
                        await _actionLogic.ExecuteLogicAppAsync(
                        tempActionId,
                        deviceId,
                        "TEMP",
                        tempReading);
                    }
                    else
                    {
                        Trace.TraceError("ActionProcessor: tempActionId value is empty for temperatureRuleOutput '{0}'", ruleOutput);
                    }
                }

                if (ruleOutput.Equals("AlarmREAR_TPM", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("ProcessAction: REAR_TPM rule triggered!");
                    double REAR_TPMReading = eventData.Reading;

                    string REAR_TPMActionId = await _actionMappingLogic.GetActionIdFromRuleOutputAsync(ruleOutput);

                    if (!string.IsNullOrWhiteSpace(REAR_TPMActionId))
                    {
                        await _actionLogic.ExecuteLogicAppAsync(
                            REAR_TPMActionId,
                            deviceId,
                            "REAR_TPM",
                            REAR_TPMReading);
                    }
                    else
                    {
                        Trace.TraceError("ActionProcessor: REAR_TPM value is empty for REAR_TPMRuleOutput '{0}'", ruleOutput);
                    }
                }
                //insert DATA FIELD
                if (ruleOutput.Equals("AlarmFRONT_TPM", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("ProcessAction: FRONT_TPM rule triggered!");
                    double FRONT_TPMReading = eventData.Reading;

                    string FRONT_TPMActionId = await _actionMappingLogic.GetActionIdFromRuleOutputAsync(ruleOutput);

                    if (!string.IsNullOrWhiteSpace(FRONT_TPMActionId))
                    {
                        await _actionLogic.ExecuteLogicAppAsync(
                            FRONT_TPMActionId,
                            deviceId,
                            "FRONT_TPM",
                            FRONT_TPMReading);
                    }
                    else
                    {
                        Trace.TraceError("ActionProcessor: FRONT_TPM ActionId value is empty for FRONT_TPMRuleOutput '{0}'", ruleOutput);
                    }
                }
                if (ruleOutput.Equals("AlarmBETTERY_VOLT", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("ProcessAction: BETTERY_VOLT rule triggered!");
                    double BETTERY_VOLTReading = eventData.Reading;

                    string BETTERY_VOLTActionId = await _actionMappingLogic.GetActionIdFromRuleOutputAsync(ruleOutput);

                    if (!string.IsNullOrWhiteSpace(BETTERY_VOLTActionId))
                    {
                        await _actionLogic.ExecuteLogicAppAsync(
                            BETTERY_VOLTActionId,
                            deviceId,
                            "BETTERY_VOLT",
                            BETTERY_VOLTReading);
                    }
                    else
                    {
                        Trace.TraceError("ActionProcessor: BETTERY_VOLT ActionId value is empty for BETTERY_VOLTRuleOutput '{0}'", ruleOutput);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("ActionProcessor: exception in ProcessAction:");
                Trace.TraceError(e.ToString());
            }
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation("ActionProcessor: Close : Partition : " + context.Lease.PartitionId);
            this.IsClosed = true;
            _checkpointStopwatch.Stop();
            this.CloseReason = reason;
            this.OnProcessorClosed();

            try
            {
                return context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - ActionProcessor.CloseAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);

                return Task.Run(() => { });
            }
        }

        public virtual void OnProcessorClosed()
        {
            EventHandler handler = this.ProcessorClosed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
