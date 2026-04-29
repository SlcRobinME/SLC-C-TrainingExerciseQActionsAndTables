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
        private const string SingleTransportStreamJson = @"{""transport_streams"": [{
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
                                            ""service_bitrate"": 27.8
                                                         }]
                                        }]}";

        private const string MultipleTransportStreamsJson = @"{""transport_streams"": [{
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
                                            ""service_bitrate"": 27.8
                                        }]
                                         }, {
                                        ""ts_id"": 2,
                                        ""ts_name"": ""Das Erste SD"",
                                        ""multicast"": ""232.101.1.2"",
                                        ""sourceIp"": ""10.15.1.2"",
                                        ""network_id"": 1,
                                        ""services"": [{
                                            ""service_id"": 101,
                                            ""service_name"": ""Service 3"",
                                            ""service_type"": ""digital_television"",
                                            ""service_provider"": ""Provider B"",
                                            ""service_bitrate"": 5.0
                                            }]
                                        }]}";

        [TestMethod]
        public void PollData_ValidJson_TransportStreamsTableRowCountCorrect()
        {
            // Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            // Act
            DataPoller.PollData(protocolMock.Object, SingleTransportStreamJson);

            // Assert
            protocolMock.Assert()
                .Table(Parameter.Transportstreams.tablePid)
                .RowCount
                .Should()
                .Be(1);
        }

        [TestMethod]
        public void PollData_ValidJson_ServicesTableRowCountCorrect()
        {
            // Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            // Act
            DataPoller.PollData(protocolMock.Object, SingleTransportStreamJson);

            // Assert
            protocolMock.Assert()
                .Table(Parameter.Transportstreams.tablePid)
                .RowCount
                .Should()
                .Be(1);
        }

        [TestMethod]
        public void PollData_ValidJson_TransportStreamRowContainsCorrectName()
        {
            // Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            // Act
            DataPoller.PollData(protocolMock.Object, SingleTransportStreamJson);

            // Assert
            Assert.AreEqual("RTL HD", protocolMock.Assert().Table(Parameter.Transportstreams.tablePid).Row<TransportstreamsQActionRow>("1").Transportstreamsname);
        }

        [TestMethod]
        public void PollData_ValidJson_TransportStreamRowContainsCorrectMulticast()
        {
            // Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            // Act
            DataPoller.PollData(protocolMock.Object, SingleTransportStreamJson);

            // Assert
            Assert.AreEqual("232.101.1.1", protocolMock.Assert().Table(Parameter.Transportstreams.tablePid).Row<TransportstreamsQActionRow>("1").Transportstreamsmulticastaddress);
        }

        [TestMethod]
        public void PollData_ValidJson_TransportStreamRowContainsCorrectNetworkId()
        {
            // Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            // Act
            DataPoller.PollData(protocolMock.Object, SingleTransportStreamJson);

            // Assert
            Assert.AreEqual(1.0, protocolMock.Assert().Table(Parameter.Transportstreams.tablePid).Row<TransportstreamsQActionRow>("1").Transportstreamsnetworkid);
        }

        [TestMethod]
        public void PollData_ValidJson_ServiceRowContainsCorrectName()
        {
            // Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            // Act
            DataPoller.PollData(protocolMock.Object, SingleTransportStreamJson);

            // Assert
            Assert.AreEqual("Service 1", protocolMock.Assert().Table(Parameter.Services.tablePid).Row<ServicesQActionRow>("1/52006").Servicesname);
        }
        [TestMethod]
        public void PollData_ValidJson_ServiceRowContainsCorrectType()
        {
            // Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            // Act
            DataPoller.PollData(protocolMock.Object, SingleTransportStreamJson);

            // Assert
            Assert.AreEqual("digital_television", protocolMock.Assert().Table(Parameter.Services.tablePid).Row<ServicesQActionRow>("1/52006").Servicestype);
        }

        [TestMethod]
        public void PollData_ValidJson_ServiceRowContainsCorrectForeignKey()
        {
            // Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            // Act
            DataPoller.PollData(protocolMock.Object, SingleTransportStreamJson);

            // Assert
            Assert.AreEqual("1", protocolMock.Assert().Table(Parameter.Services.tablePid).Row<ServicesQActionRow>("1/52006").Servicestransportstreamid);
        }

        [TestMethod]
        public void PollData_ValidJson_ServiceRowContainsCorrectCompositeKey()
        {
            // Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            // Act
            DataPoller.PollData(protocolMock.Object, SingleTransportStreamJson);

            // Assert
            protocolMock.Assert().Table(Parameter.Services.tablePid).AllRows().Should().ContainKey("1/52006");
        }

        [TestMethod]
        public void PollData_MultipleTransportStreams_TransportStreamsTableRowCountCorrect()
        {
            // Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            // Act
            DataPoller.PollData(protocolMock.Object, MultipleTransportStreamsJson);

            // Assert
            protocolMock.Assert().Table(Parameter.Transportstreams.tablePid).RowCount.Should().Be(2);
        }


        [TestMethod]
        public void PollData_MultipleTransportStreams_ServicesTableRowCountCorrect()
        {
            // Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();

            // Act
            DataPoller.PollData(protocolMock.Object, MultipleTransportStreamsJson);

            // Assert
            protocolMock.Assert().Table(Parameter.Services.tablePid).RowCount.Should().Be(2);
        }

        [TestMethod]
        public void PollData_EmptyTransportStreams_TablesAreEmpty()
        {
            // Arrange
            var protocolMock = new SLProtocolMock<ConcreteSLProtocolExt>();
            string json = @"{""transport_streams"": []}";

            // Act
            DataPoller.PollData(protocolMock.Object, json);

            // Assert
            protocolMock.Assert().Table(Parameter.Transportstreams.tablePid).RowCount.Should().Be(0);
        }
    }
}