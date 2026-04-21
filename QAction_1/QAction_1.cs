namespace Skyline.Protocol
{
    using Skyline.DataMiner.Scripting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;
    using System.Linq;

    public static class DataPoller
    {
        private const string JsonFilePath =@"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\Data.json";

        public static void PollData(SLProtocolExt protocol)
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

                var tsRows = new List<TransportstreamsQActionRow>();
                var svcRows = new List<ServicesQActionRow>();

                foreach (TransportStreamModel ts in root.TransportStreams)
                {
                    tsRows.Add(new TransportstreamsQActionRow
                    {
                        Transportstreamsid = ts.TsId.ToString(),
                        Transportstreamsname = ts.TsName,
                        Transportstreamsmulticastaddress = ts.Multicast,
                        Transportstreamssourceip = ts.SourceIp,
                        Transportstreamsnetworkid = (double)ts.NetworkId,
                        Transportstreamslastpolled = pollTimestamp,
                    });

                    if (ts.Services == null)
                        continue;

                    foreach (ServiceModel service in ts.Services)
                    {
                        svcRows.Add(new ServicesQActionRow
                        {
                            Servicesid = $"{ts.TsId}/{service.ServiceId}",
                            Servicesname = service.ServiceName,
                            Servicestype = service.ServiceType,
                            Servicesprovider = service.ServiceProvider,
                            Servicesbitrate = service.ServiceBitrate,
                            Servicestransportstreamid = ts.TsId.ToString(),
                            Serviceslastpolled = pollTimestamp,
                        });
                    }
                }
                protocol.FillArray(Parameter.Transportstreams.tablePid, tsRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
                protocol.FillArray(Parameter.Services.tablePid, svcRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
            }
            catch (Exception ex)
            {
                protocol.Log($"QA|DataPoller|PollData|Unexpected exception: {ex}", LogType.Error, LogLevel.NoLogging);
            }
        }
    }
}