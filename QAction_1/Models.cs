using Newtonsoft.Json;
using System.Collections.Generic;

namespace Skyline.Protocol
{
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
}
