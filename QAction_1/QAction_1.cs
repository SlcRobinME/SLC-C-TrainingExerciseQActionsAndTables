namespace Skyline.Protocol
{
    using Newtonsoft.Json;
    using Skyline.DataMiner.Scripting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

    public class TransportStreamRoot
    {
        [JsonProperty("transport_streams")]
        public List<TransportStreamModel> TransportStreams { get; set; }
    }

    public class TransportStreamModel
    {
        [JsonProperty("ts_id")]
        public int TsId { get; set; }

        [JsonProperty("ts_name")]
        public string TsName { get; set; }

        [JsonProperty("multicast")]
        public string Multicast { get; set; }

        [JsonProperty("sourceIp")]
        public string SourceIp { get; set; }

        [JsonProperty("network_id")]
        public int NetworkId { get; set; }

        [JsonProperty("services")]
        public List<ServiceModel> Services { get; set; }
    }

    public class ServiceModel
    {
        [JsonProperty("service_id")]
        public int ServiceId { get; set; }

        [JsonProperty("service_name")]
        public string ServiceName { get; set; }

        [JsonProperty("service_type")]
        public string ServiceType { get; set; }

        [JsonProperty("service_provider")]
        public string ServiceProvider { get; set; }

        [JsonProperty("service_bitrate")]
        public double ServiceBitrate { get; set; }
    }

    public static class DataPoller
    {
        private const string JsonFilePath =@"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\Data.json";

        public static void PollData(SLProtocol protocol)
        {
            try
            {
                if (!File.Exists(JsonFilePath))
                {
                    protocol.Log($"QA|DataPoller|PollData|File not found: {JsonFilePath}", LogType.Error, LogLevel.NoLogging);
                    return;
                }
                string jsonContent = File.ReadAllText(JsonFilePath);
                var root = SecureNewtonsoftDeserialization.DeserializeObject<TransportStreamRoot>(jsonContent);

                if (root?.TransportStreams == null || root.TransportStreams.Count == 0)
                {
                    protocol.Log("QA|DataPoller|PollData|No transport stream data.", LogType.Allways, LogLevel.NoLogging);
                    return;
                }

                string pollTimestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                var tsRows = new List<object[]>();
                var svcRows = new List<object[]>();

                foreach (TransportStreamModel ts in root.TransportStreams)
                {
                    tsRows.Add(new object[]
                    {
                        ts.TsId.ToString(),
                        ts.TsName,
                        ts.Multicast,
                        ts.SourceIp,
                        (double)ts.NetworkId,
                        pollTimestamp,
                    });

                    if (ts.Services == null)
                        continue;

                    foreach (ServiceModel service in ts.Services)
                    {
                        svcRows.Add(new object[]
                        {
                            $"{ts.TsId}/{service.ServiceId}",
                            service.ServiceName,
                            service.ServiceType,
                            service.ServiceProvider,
                            service.ServiceBitrate,
                            ts.TsId.ToString(),
                            pollTimestamp
                        });
                    }
                }
                protocol.FillArray(Parameter.Transportstreams.tablePid, tsRows, NotifyProtocol.SaveOption.Full);
                protocol.FillArray(Parameter.Services.tablePid, svcRows, NotifyProtocol.SaveOption.Full);
            }
            catch (Exception ex)
            {
                protocol.Log($"QA|DataPoller|PollData|Unexpected exception: {ex}", LogType.Error, LogLevel.NoLogging);
            }
        }
    }
}