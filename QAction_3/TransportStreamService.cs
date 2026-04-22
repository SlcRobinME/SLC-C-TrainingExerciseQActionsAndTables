using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Scripting;

public class TransportStreamService
{
    private const string DefaultFilePath = @"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\Data.json";

    private readonly IJsonLoader loader;
    private readonly Random rng;
    private readonly double maxBitrate;
    private readonly int decimals;

    public TransportStreamService(IJsonLoader loader = null, Random rng = null, double maxBitrate = 100, int decimals = 3)
    {
        this.loader = loader ?? new JsonLoader();
        this.rng = rng ?? new Random();
        this.maxBitrate = maxBitrate;
        this.decimals = decimals;
    }

    public void Execute(SLProtocol protocol, string filePath = DefaultFilePath)
    {
        var root = loader.Load(filePath);
        var (tsRows, svcRows) = Map(root);

        protocol.FillArray(Parameter.Transportstreams.tablePid, tsRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
        protocol.FillArray(Parameter.Services.tablePid, svcRows.Select(r => r.ToObjectArray()).ToList(), NotifyProtocol.SaveOption.Full);
    }

    public (List<TransportstreamsQActionRow>, List<ServicesQActionRow>) Map(Root root)
    {
        var tsRows = new List<TransportstreamsQActionRow>();
        var svcRows = new List<ServicesQActionRow>();

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
                    Servicesbitrate_2006 = svc.ServiceBitrate <= 0
                        ? Math.Round(rng.NextDouble() * maxBitrate, decimals)
                        : svc.ServiceBitrate,
                    Servicestransportstreamid_2007 = tsKey,
                    Serviceslastpolled_2008 = DateTime.Now.ToOADate(),
                    Servicestransportstreamnameservice_2009 = ts.TsName,
                });
            }
        }

        return (tsRows, svcRows);
    }
}