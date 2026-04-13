using Newtonsoft.Json;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.SecureCoding.SecureIO;
using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            string jsonRaw = @"
            {
                ""transport_streams"": [
                    {
                        ""ts_id"": 1,
                        ""ts_name"": ""RTL HD"",
                        ""multicast"": ""232.101.1.1"",
                        ""sourceIp"": ""10.15.1.1"",
                        ""network_id"": 1,
                        ""services"": [
                            { ""service_id"": 52006, ""service_name"": ""Service 1"", ""service_type"": ""digital_television"", ""service_provider"": ""Provider A"", ""service_bitrate"": 27.8 },
                            { ""service_id"": 52007, ""service_name"": ""Service 2"", ""service_type"": ""digital_television"", ""service_provider"": ""Provider A"", ""service_bitrate"": 25.3 }
                        ]
                    },
                    {
                        ""ts_id"": 2,
                        ""ts_name"": ""Das Erste SD"",
                        ""multicast"": ""232.101.1.2"",
                        ""sourceIp"": ""10.15.1.2"",
                        ""network_id"": 1,
                        ""services"": [
                            { ""service_id"": 101, ""service_name"": ""Service 3"", ""service_type"": ""digital_television"", ""service_provider"": ""Provider B"", ""service_bitrate"": 5.0 },
                            { ""service_id"": 102, ""service_name"": ""Service 4"", ""service_type"": ""digital_radio"", ""service_provider"": ""Provider B"", ""service_bitrate"": 5.4 }
                        ]
                    },
                    {
                        ""ts_id"": 3,
                        ""ts_name"": ""Comedy Central HD"",
                        ""multicast"": ""232.101.1.3"",
                        ""sourceIp"": ""10.15.1.3"",
                        ""network_id"": 2,
                        ""services"": [
                            { ""service_id"": 2003, ""service_name"": ""Service 5"", ""service_type"": ""digital_television"", ""service_provider"": ""Provider C"", ""service_bitrate"": 8.9 },
                            { ""service_id"": 2004, ""service_name"": ""Service 6"", ""service_type"": ""digital_radio"", ""service_provider"": ""Provider C"", ""service_bitrate"": 10.6 }
                        ]
                    }
                ]
            }";

            //string filePath = @"C:\Users\AmerMO\Documents\training\SLC-C-TrainingExerciseQActionsAndTables\Documentation\Data.json";
            //string filePath = SecurePath.ConstructSecurePath(
            //    AppDomain.CurrentDomain.BaseDirectory,
            //    "Documentation",
            //    "Data.json"
            //);

            //jsonRaw = File.ReadAllText(filePath);

            var root = SecureNewtonsoftDeserialization.DeserializeObject<Root>(jsonRaw);
            protocol.Log($"QAction3|Deserialized root={(root != null ? "OK" : "NULL")}", LogType.Allways, LogLevel.NoLogging);

            string lastPolled = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            protocol.ClearAllKeys(1000);
            protocol.ClearAllKeys(2000);

            foreach (var ts in root.TransportStreams)
            {
                string tsKey = ts.TsId.ToString();

                protocol.AddRow(1000, new object[]
                {
                    tsKey,                              // 1001
                    ts.TsName,                          // 1002
                    ts.Multicast,                       // 1003
                    ts.SourceIp,                        // 1004
                    (double)ts.NetworkId,               // 1005
                    (double)(ts.Services?.Count ?? 0),  // 1006
                    lastPolled,                         // 1007
                });

                if (ts.Services == null)
                    continue;

                foreach (var svc in ts.Services)
                {
                    string svcKey = $"{ts.TsId}/{svc.ServiceId}";

                    protocol.AddRow(2000, new object[]
                    {
                        svcKey,                          // 2001
                        (double)svc.ServiceId,           // 2002
                        svc.ServiceName,                 // 2003
                        svc.ServiceType,                 // 2004
                        svc.ServiceProvider,             // 2005
                        svc.ServiceBitrate,              // 2006
                        tsKey,                           // 2007 (FK)
                        lastPolled,                      // 2008
                    });
                }
            }

            protocol.Log("QAction3|Rows successfully added (AddRow)", LogType.Allways, LogLevel.NoLogging);
        }
        catch (Exception ex)
        {
            protocol.Log($"QAction3|Exception: {ex.Message}\n{ex.StackTrace}", LogType.Error, LogLevel.NoLogging);
        }
    }
}