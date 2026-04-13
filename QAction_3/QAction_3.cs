using Skyline.DataMiner.Net.ClientCompatibility.Helpers;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

/// <summary>
/// DataMiner QAction Class: Parse Transport Streams JSON.
/// </summary>
public static class QAction
{
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocol protocol)
    {
        try
        {
            string filePath = @"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\Data.json";
            string jsonRaw = File.ReadAllText(filePath);

            var root = SecureNewtonsoftDeserialization.DeserializeObject<Root>(jsonRaw);
            protocol.Log($"QAction3|Deserialized root={(root != null ? "OK" : "NULL")}", LogType.Allways, LogLevel.NoLogging);

            string lastPolled = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var tsRows = new List<TransportStreamsQActionRow>();
            var svcRows = new List<ServicesQActionRow>();

            Random rng = new Random();

            foreach (var ts in root.TransportStreams)
            {
                string tsKey = ts.TsId.ToString();
                tsRows.Add(new TransportStreamsQActionRow
                {
                    TransportStreamID_1001 = tsKey,
                    TransportStreamName_1002 = ts.TsName,
                    TransportStreamMulticast_1003 = ts.Multicast,
                    TransportStreamSourceIP_1004 = ts.SourceIp,
                    TransportStreamNetworkID_1005 = ts.NetworkId.ToString(),
                    TransportStreamNumberOfServices_1006 = (double)(ts.Services?.Count ?? 0),
                    TransportStreamLastPolled_1007 = lastPolled,
                });

                protocol.SetRow(Parameter.TransportStreams.tablePid, tsKey, tsRows.Last().ToObjectArray());

                if (ts.Services == null)
                    continue;

                foreach (var svc in ts.Services)
                {
                    string svcKey = $"{ts.TsId}/{svc.ServiceId}";

                    svcRows.Add(new ServicesQActionRow
                    {
                        ServiceInstanceID_2001 = svcKey,
                        ServiceID_2002 = (double)svc.ServiceId,
                        ServiceName_2003 = svc.ServiceName,
                        ServiceType_2004 = svc.ServiceType,
                        ServiceProvider_2005 = svc.ServiceProvider,
                        ServiceBitrate_2006 = svc.ServiceBitrate <= 0 ? Math.Round(rng.NextDouble() * 15, 3) : svc.ServiceBitrate,
                        ServiceTransportStreamID_2007 = tsKey,
                        ServiceLastPolled_2008 = lastPolled,
                    });
                    protocol.SetRow(Parameter.Services.tablePid, svcKey, svcRows.Last().ToObjectArray());
                }
            }

            //protocol.FillArray(Parameter.TransportStreams.tablePid, tsRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
            //protocol.FillArray(Parameter.Services.tablePid, svcRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);

            protocol.Log("QAction3|Rows successfully added (AddRow)", LogType.Allways, LogLevel.NoLogging);
        }
        catch (Exception ex)
        {
            protocol.Log($"QAction3|Exception: {ex.Message}\n{ex.StackTrace}", LogType.Error, LogLevel.NoLogging);
        }
    }
}