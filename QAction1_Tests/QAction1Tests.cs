using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.UnitTestingFramework.Protocol;
using Skyline.Protocol;
using System.Diagnostics.CodeAnalysis;

namespace QAction1_Tests
{
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1027:TabsMustNotBeUsed", Justification = "Reviewed.")]
    [TestClass]
    public class JsonParsingTests
    {

        [TestMethod]
        public void ParseJson_ValidJson_ReturnsCorrectTransportStreamCount()
        {
            //Arrange
            string json = @"{""transport_streams"": [{
                            ""ts_id"": 1,
                            ""ts_name"": ""RTL HD"",
                            ""multicast"": ""232.101.1.1"",
                            ""sourceIp"": ""10.15.1.1"",
                            ""network_id"": 1,
                            ""services"": []}
                            ]}";
            //Act
            var root = Newtonsoft.Json.JsonConvert.DeserializeObject<TransportStreamRoot>(json);
            //Assert
            Assert.AreEqual(1, root.TransportStreams.Count);
        }


        [TestMethod]
        public void ParseJson_ValidJson_ReturnsCorrectTransportStreamProperties()
        {
            //Arrange
            string json = @"{""transport_streams"": [{
                            ""ts_id"": 1, 
                            ""ts_name"": ""RTL HD"", 
                            ""multicast"": ""232.101.1.1"", 
                            ""sourceIp"": ""10.15.1.1"", 
                            ""network_id"": 1, 
                            ""services"": []}
                            ]}";

            //Act
            var root = Newtonsoft.Json.JsonConvert.DeserializeObject<TransportStreamRoot>(json);
            var ts = root.TransportStreams[0];
            //Assert
            Assert.AreEqual(1, ts.TsId);
            Assert.AreEqual("RTL HD", ts.TsName);
            Assert.AreEqual("232.101.1.1", ts.Multicast);
            Assert.AreEqual("10.15.1.1", ts.SourceIp);
            Assert.AreEqual(1, ts.NetworkId);
        }


        [TestMethod]
        public void ParseJson_ValidJson_ReturnsCorrectServiceCount()
        {
            //Arrange
            string json = @"{""transport_streams"": [{
                            ""ts_id"": 1, 
                            ""ts_name"": ""RTL HD"", 
                            ""multicast"": ""232.101.1.1"", 
                            ""sourceIp"": ""10.15.1.1"", 
                            ""network_id"": 1, 
                            ""services"": [{
                                    ""service_id"": 52006, 
                                    ""service_name"": ""Service 1"", 
                                    ""service_type"": ""digital_television"", 
                                    ""service_provider"": ""Provider A"", 
                                    ""service_bitrate"": 27.8}]
                                    }]}";
            //Act
            var root = Newtonsoft.Json.JsonConvert.DeserializeObject<TransportStreamRoot>(json);
            //Assert
            Assert.AreEqual(1, root.TransportStreams[0].Services.Count);
        }


        [TestMethod]
        public void ParseJson_ValidJson_ReturnsCorrectServiceProperties()
        {
            //Arrange
            string json = @"{""transport_streams"": [{
                                ""ts_id"": 1, 
                                ""ts_name"": ""RTL HD"", 
                                ""multicast"": ""232.101.1.1"", 
                                ""sourceIp"": ""10.15.1.1"", 
                                ""network_id"": 1, 
                                ""services"": [{
                                            ""service_id"": 52006, 
                                            ""service_name"": ""Service 1"", 
                                            ""service_type"": ""digital_television"", 
                                            ""service_provider"": ""Provider A"", 
                                            ""service_bitrate"": 27.8}]
                                            }]}";
            //Act
            var root = Newtonsoft.Json.JsonConvert.DeserializeObject<TransportStreamRoot>(json);
            var service = root.TransportStreams[0].Services[0];
            //Assert
            Assert.AreEqual(52006, service.ServiceId);
            Assert.AreEqual("Service 1", service.ServiceName);
            Assert.AreEqual("digital_television", service.ServiceType);
            Assert.AreEqual("Provider A", service.ServiceProvider);
            Assert.AreEqual(27.8, service.ServiceBitrate);
        }


        [TestMethod]
        public void ParseJson_EmptyTransportStreams_ReturnsEmptyList()
        {
            //Arrange
            string json = @"{""transport_streams"": []}";
            //Act
            var root = Newtonsoft.Json.JsonConvert.DeserializeObject<TransportStreamRoot>(json);
            //Assert
            Assert.AreEqual(0, root.TransportStreams.Count);
        }

        [TestMethod]
        public void ParseJson_NullServices_ReturnsNullServiceList()
        {
            //Arrange
            string json = @"{""transport_streams"": [{
                            ""ts_id"": 1, 
                            ""ts_name"": ""RTL HD"", 
                            ""multicast"": ""232.101.1.1"", 
                            ""sourceIp"": ""10.15.1.1"", 
                            ""network_id"": 1}
                            ]}";
            //Act
            var root = Newtonsoft.Json.JsonConvert.DeserializeObject<TransportStreamRoot>(json);
            //Assert
            Assert.IsNull(root.TransportStreams[0].Services);
        }
    }

    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1027:TabsMustNotBeUsed", Justification = "Reviewed.")]
    [TestClass]
    public class TablePopulationTests
    {
        [TestMethod]
        public void TransportStreamsTable_AddRow_RowKeyExists()
        {
            //Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            var row = new TransportstreamsQActionRow
            {
                Transportstreamsid = "1",
                Transportstreamsname = "RTL HD",
            };
            //Act
            protocolMock.Object.AddRow(Parameter.Transportstreams.tablePid, row.ToObjectArray());
            //Assert
            protocolMock.Assert()
                .Table(Parameter.Transportstreams.tablePid)
                .AllRows()
                .Should()
                .ContainKey("1");

        }

        [TestMethod]
        public void TransportStreamsTable_AddRow_RowCountIsCorrect()
        {
            //Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            var row1 = new TransportstreamsQActionRow { Transportstreamsid = "1", Transportstreamsname = "RTL HD" };
            var row2 = new TransportstreamsQActionRow { Transportstreamsid = "2", Transportstreamsname = "Das Erste SD" };
            var row3 = new TransportstreamsQActionRow { Transportstreamsid = "3", Transportstreamsname = "Comedy Central HD" };

            //Act
            protocolMock.Object.AddRow(Parameter.Transportstreams.tablePid, row1.ToObjectArray());
            protocolMock.Object.AddRow(Parameter.Transportstreams.tablePid, row2.ToObjectArray());
            protocolMock.Object.AddRow(Parameter.Transportstreams.tablePid, row3.ToObjectArray());
            //Assert
            protocolMock.Assert()
                .Table(Parameter.Transportstreams.tablePid)
                .RowCount
                .Should()
                .Be(3);
        }

        [TestMethod]
        public void TransportStreamsTable_AddRow_AllKeysExist()
        {
            //Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            var row1 = new TransportstreamsQActionRow { Transportstreamsid = "1", Transportstreamsname = "RTL HD" };
            var row2 = new TransportstreamsQActionRow { Transportstreamsid = "2", Transportstreamsname = "Das Erste SD" };
            var row3 = new TransportstreamsQActionRow { Transportstreamsid = "3", Transportstreamsname = "Comedy Central HD" };
            //Act
            protocolMock.Object.AddRow(Parameter.Transportstreams.tablePid, row1.ToObjectArray());
            protocolMock.Object.AddRow(Parameter.Transportstreams.tablePid, row2.ToObjectArray());
            protocolMock.Object.AddRow(Parameter.Transportstreams.tablePid, row3.ToObjectArray());
            //Assert
            protocolMock.Assert()
                .Table(Parameter.Transportstreams.tablePid)
                .AllRows()
                .Should()
                .ContainKeys("1", "2", "3");
        }

        [TestMethod]
        public void TransportStreamsTable_AddRow_TransportStreamName()
        {
            //Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            var row = new TransportstreamsQActionRow
            {
                Transportstreamsid = "1",
                Transportstreamsname = "RTL HD",
            };
            //Act
            protocolMock.Object.AddRow(Parameter.Transportstreams.tablePid, row.ToObjectArray());
            //Assert
            Assert.AreEqual("RTL HD", protocolMock.Assert().Table(Parameter.Transportstreams.tablePid).Row<TransportstreamsQActionRow>("1").Transportstreamsname);
        }


        [TestMethod]
        public void TransportStreamsTable_AddRow_MulticastAddressCorrect()
        {
            //Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            var row = new TransportstreamsQActionRow
            {
                Transportstreamsid = "1",
                Transportstreamsmulticastaddress = "232.101.1.1",
            };
            //Act
            protocolMock.Object.AddRow(Parameter.Transportstreams.tablePid, row.ToObjectArray());
            //Assert
            Assert.AreEqual("232.101.1.1", protocolMock.Assert().Table(Parameter.Transportstreams.tablePid).Row<TransportstreamsQActionRow>("1").Transportstreamsmulticastaddress);
        }

        [TestMethod]
        public void TransportStreamsTable_AddRow_NetworkIDCorrect()
        {
            //Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            var row = new TransportstreamsQActionRow
            {
                Transportstreamsid = "1",
                Transportstreamsnetworkid = 1.0,
            };
            //Act
            protocolMock.Object.AddRow(Parameter.Transportstreams.tablePid, row.ToObjectArray());
            //Assert
            Assert.AreEqual(1.0, protocolMock.Assert().Table(Parameter.Transportstreams.tablePid).Row<TransportstreamsQActionRow>("1").Transportstreamsnetworkid);
        }


        [TestMethod]
        public void ServicesTable_AddRow_RowExistsWithCorrectValues()
        {
            //Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            var row = new ServicesQActionRow
            {
                Servicesid = "1/52006",
                Servicesname = "Service 1",
            };
            //Act
            protocolMock.Object.AddRow(Parameter.Services.tablePid, row.ToObjectArray());
            //Assert
            Assert.AreEqual("Service 1", protocolMock.Assert().Table(Parameter.Services.tablePid).Row<ServicesQActionRow>("1/52006").Servicesname);
        }


        [TestMethod]
        public void ServicesTable_AddRow_RowCountIsCorrect()
        {
            //Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            var row1 = new ServicesQActionRow { Servicesid = "1/52006", Servicesname = "Service 1" };
            var row2 = new ServicesQActionRow { Servicesid = "1/52007", Servicesname = "Service 2" };
            var row3 = new ServicesQActionRow { Servicesid = "2/101", Servicesname = "Service 3" };
            //Act
            protocolMock.Object.AddRow(Parameter.Services.tablePid, row1.ToObjectArray());
            protocolMock.Object.AddRow(Parameter.Services.tablePid, row2.ToObjectArray());
            protocolMock.Object.AddRow(Parameter.Services.tablePid, row3.ToObjectArray());
            //Assert
            protocolMock.Assert()
                .Table(Parameter.Services.tablePid)
                .RowCount
                .Should()
                .Be(3);
        }


        [TestMethod]
        public void ServicesTable_AddRow_CompositeKeyCorrect()
        {
            //Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            var row = new ServicesQActionRow
            {
                Servicesid = "1/52006",
                Servicesname = "Service 1",
            };
            //Act
            protocolMock.Object.AddRow(Parameter.Services.tablePid, row.ToObjectArray());
            //Assert
            protocolMock.Assert()
                .Table(Parameter.Services.tablePid)
                .AllRows()
                .Should()
                .ContainKey("1/52006");
        }

        [TestMethod]
        public void ServicesTable_AddRow_ServiceTypeCorrect()
        {
            //Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            var row = new ServicesQActionRow
            {
                Servicesid = "1/52006",
                Servicestype = "digital_television",
            };
            //Act
            protocolMock.Object.AddRow(Parameter.Services.tablePid, row.ToObjectArray());
            //Assert
            Assert.AreEqual("digital_television", protocolMock.Assert().Table(Parameter.Services.tablePid).Row<ServicesQActionRow>("1/52006").Servicestype);
        }
    }
}