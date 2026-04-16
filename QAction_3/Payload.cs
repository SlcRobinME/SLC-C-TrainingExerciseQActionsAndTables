using System.Collections.Generic;
using Newtonsoft.Json;

public class Root
{
#pragma warning disable SA1401 // Fields should be private
    public static string JsonPath = @"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\Data.json";
#pragma warning restore SA1401 // Fields should be private

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

    [JsonProperty("services")]
    public List<Service> Services { get; set; }
}

public class Service
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