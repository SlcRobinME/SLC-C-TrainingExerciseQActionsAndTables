using Newtonsoft.Json;
using Skyline.DataMiner.Net.ResponseErrorData;
using Skyline.DataMiner.Scripting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using static Skyline.DataMiner.Scripting.Parameter;

/// <summary>
/// DataMiner QAction Class: getTransportstreamData.
/// </summary>
public static class QAction
{
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    /// 
    private static readonly Random Random = new Random();
    public static string DataPath { get; set; } =
        @"C:\Skyline DataMiner\Documents\Exercise Skyline Encoder Decoder RME\Data.json";

    public static void Run(SLProtocol protocol)
	{
		try
		{
            var path = DataPath;

            if (!File.Exists(path))
			{
				protocol.Log($"QA{protocol.QActionID}| File not found", LogType.Error, LogLevel.NoLogging);
				return;
			}

		    var data = JsonConvert.DeserializeObject<TransportStreamData>(File.ReadAllText(path));

            if (data.TransportStreams == null)
            {
                protocol.Log($"QA{protocol.QActionID}|Run|No transportstreams found in file.", LogType.Error, LogLevel.NoLogging);
                return;
            }

            foreach (var ts in data.TransportStreams)
            {
                TransportstreamsQActionRow row = new TransportstreamsQActionRow
                {
                    Transportstreamsid_1 = ts.TsId.ToString(),
                    Transportstreamsname_2 = ts.TsName,
                    Transportstreamsmulticast_3 = ts.Multicast,
                    Transportstreamssourceip_4 = ts.SourceIp,
                    Transportstreamsnetworkid_5 = ts.NetworkId,
                    Transportstreamslastupdate_11 = DateTime.Now.ToOADate(),
                };

                UpsertRow(protocol, Parameter.Transportstreams.tablePid, row.Key, row.ToObjectArray());

                foreach (var s in ts.Services)
                {
                    ServiceQActionRow sr = new ServiceQActionRow
                    {
                        Serviceid_6 = s.ServiceId.ToString(),
                        Servicename_7 = s.ServiceName,
                        Servicetype_8 = s.ServiceType,
                        Serviceprovider_9 = s.ServiceProvider,
                        Servicebitrate_10 = Random.NextDouble() * 30,
                    };

                    UpsertRow(protocol, Parameter.Service.tablePid, sr.Key, sr.ToObjectArray());
                }
            }

        }
		catch (Exception ex)
		{
			protocol.Log($"QA{protocol.QActionID}|{protocol.GetTriggerParameter()}|Run|Exception thrown:{Environment.NewLine}{ex}", LogType.Error, LogLevel.NoLogging);
		}
	}

    private static void UpsertRow(SLProtocol protocol, int tableId, string key, object[] row)
    {
        if (protocol.Exists(tableId, key))
            protocol.SetRow(tableId, key, row);
        else
            protocol.AddRow(tableId, row);
    }
}

public class TransportStreamData
{
    [JsonProperty("transport_streams")]
    public List<TransportStream> TransportStreams { get; set; }
}

public class TransportStream
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

	public string LastUpdate { get; set; }

	[JsonProperty("services")]
	public List<Services> Services { get; set; }
}

public class Services
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