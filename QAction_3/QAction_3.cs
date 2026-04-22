using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.SecureCoding.SecureIO;
using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

/// <summary>
/// DataMiner QAction Class: Parse Transport Streams JSON.
/// </summary>
public static class QAction
{
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocolExt protocol)
    {
        try
        {
            string filePath = Root.JsonPath;

            string jsonRaw = File.ReadAllText(SecurePath.CreateSecurePath(filePath));

            var root = SecureNewtonsoftDeserialization.DeserializeObject<Root>(jsonRaw);

            var tsRows = new List<TransportstreamsQActionRow>();
            var svcRows = new List<ServicesQActionRow>();

            // Generate random bitrate for services with missing or zero bitrate, to better visualize them in the UI.
            // Remove this logic if not needed.
            Random rng = new Random();
            double maxBitrate = 100;
            int decimals = 3;

            foreach (var ts in root.TransportStreams)
            {
                string tsKey = ts.TsId.ToString();
                tsRows.Add(new TransportstreamsQActionRow
                {
                    Transportstreamsid_1001 = tsKey,
                    Transportstreamsname_1002 = ts.TsName,
                    Transportstreamsmulticast_1003 = ts.Multicast,
                    Transportstreamsip_1004 = ts.SourceIp,
                    Transportstreamsnetworkid_1005 = ts.NetworkId.ToString(),
                    Transportstreamsnumberofservices_1006 = (double)(ts.Services?.Count ?? 0),
                    Transportstreamslastpolled_1007 = DateTime.Now.ToOADate(),
                });

                if (ts.Services == null)
                    continue;

                foreach (var svc in ts.Services)
                {
                    string svcKey = $"{ts.TsId}/{svc.ServiceId}";

                    svcRows.Add(new ServicesQActionRow
                    {
                        Servicesinstanceid_2001 = svcKey,
                        Servicesid_2002 = (double)svc.ServiceId,
                        Servicesname_2003 = svc.ServiceName,
                        Servicestype_2004 = svc.ServiceType,
                        Servicesprovider_2005 = svc.ServiceProvider,
                        Servicesbitrate_2006 = svc.ServiceBitrate <= 0 ? Math.Round(rng.NextDouble() * maxBitrate, decimals) : svc.ServiceBitrate,
                        Servicestransportstreamid_2007 = tsKey,
                        Serviceslastpolled_2008 = DateTime.Now.ToOADate(),
                        Servicestransportstreamnameservice_2009 = ts.TsName,
                    });
                }
            }

            protocol.FillArray(Parameter.Transportstreams.tablePid, tsRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
            protocol.FillArray(Parameter.Services.tablePid, svcRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
        }
        catch (Exception ex)
        {
            protocol.Log($"QAction3|Exception: {ex.Message}\n{ex.StackTrace}", LogType.Error, LogLevel.NoLogging);
        }
    }
}