using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Skyline.DataMiner.Scripting;

namespace Skyline.Protocol.SLProtocolDependency.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class QActionRunTests
    {
        private const int TransportstreamsTable = 100;
        private const int ServiceTable = 102;

        private const string SampleJson = @"{
            ""transport_streams"": [
                { ""ts_id"": 1, ""ts_name"": ""RTL HD"", ""multicast"": ""232.101.1.1"", ""sourceIp"": ""10.15.1.1"", ""network_id"": 1,
                  ""services"": [
                    { ""service_id"": 52006, ""service_name"": ""Service 1"", ""service_type"": ""digital_television"", ""service_provider"": ""Provider A"", ""service_bitrate"": 27.8 },
                    { ""service_id"": 52007, ""service_name"": ""Service 2"", ""service_type"": ""digital_television"", ""service_provider"": ""Provider A"", ""service_bitrate"": 25.3 } ] },
                { ""ts_id"": 2, ""ts_name"": ""Das Erste SD"", ""multicast"": ""232.101.1.2"", ""sourceIp"": ""10.15.1.2"", ""network_id"": 1,
                  ""services"": [
                    { ""service_id"": 101, ""service_name"": ""Service 3"", ""service_type"": ""digital_radio"", ""service_provider"": ""Provider B"", ""service_bitrate"": 5.0 } ] }
            ]
        }";

        private string _originalPath;
        private string _testFilePath;

        [TestInitialize]
        public void UseTemporaryDataFile()
        {
            _originalPath = QAction.DataPath;
            _testFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
            QAction.DataPath = _testFilePath;
        }

        [TestCleanup]
        public void RestoreDataPath()
        {
            QAction.DataPath = _originalPath;
            if (File.Exists(_testFilePath)) File.Delete(_testFilePath);
        }

        private void WriteDataFile(string content) => File.WriteAllText(_testFilePath, content);

        /// <summary>Rows passed to AddRow for the given table, in call order.</summary>
        private static object[][] AddedRows(Mock<SLProtocol> fakeprotocol, int tablePid)
            => fakeprotocol.Invocations
                   .Where(i => i.Method.Name == "AddRow" && (int)i.Arguments[0] == tablePid)
                   .Select(i => (object[])i.Arguments[1])
                   .ToArray();

        [TestMethod]
        public void Run_FileDoesNotExist_AddsNoRows()
        {
            // Arrange (no file is written, so DataPath points to a missing file)
            var fakeprotocol = new Mock<SLProtocol>();

            // Act
            QAction.Run(fakeprotocol.Object);

            // Assert
            Assert.AreEqual(0, AddedRows(fakeprotocol, TransportstreamsTable).Length);
            Assert.AreEqual(0, AddedRows(fakeprotocol, ServiceTable).Length);
        }

        [TestMethod]
        public void Run_ValidFile_AddsOneRowPerStreamAndService()
        {
            // Arrange
            WriteDataFile(SampleJson);
            var fakeprotocol = new Mock<SLProtocol>();

            // Act
            QAction.Run(fakeprotocol.Object);

            // Assert
            Assert.AreEqual(2, AddedRows(fakeprotocol, TransportstreamsTable).Length);
            Assert.AreEqual(3, AddedRows(fakeprotocol, ServiceTable).Length);
        }

        [TestMethod]
        public void Run_ValidFile_AddsTransportStreamRowInColumnOrder()
        {
            // Arrange
            WriteDataFile(SampleJson);
            var fakeprotocol = new Mock<SLProtocol>();

            // Act
            QAction.Run(fakeprotocol.Object);
            object[] row = AddedRows(fakeprotocol, TransportstreamsTable)[0];

            // Assert
            Assert.AreEqual("1", Convert.ToString(row[0]));
            Assert.AreEqual("RTL HD", Convert.ToString(row[1]));
            Assert.AreEqual("232.101.1.1", Convert.ToString(row[2]));
            Assert.AreEqual("10.15.1.1", Convert.ToString(row[3]));
            Assert.AreEqual(1, Convert.ToInt32(row[4]));
            Assert.IsTrue(Convert.ToDouble(row[5]) > 0, "Last update should be an OADate.");
        }

        [TestMethod]
        public void Run_ValidFile_AddsServiceRowInColumnOrder()
        {
            // Arrange
            WriteDataFile(SampleJson);
            var fakeprotocol = new Mock<SLProtocol>();

            // Act
            QAction.Run(fakeprotocol.Object);
            object[] row = AddedRows(fakeprotocol, ServiceTable)[0];

            // Assert
            Assert.AreEqual("52006", Convert.ToString(row[0]));
            Assert.AreEqual("Service 1", Convert.ToString(row[1]));
            Assert.AreEqual("digital_television", Convert.ToString(row[2]));
            Assert.AreEqual("Provider A", Convert.ToString(row[3]));
        }

        [TestMethod]
        public void Run_ValidFile_SetsEachBitrateBetween0And30()
        {
            // Arrange
            WriteDataFile(SampleJson);
            var fakeprotocol = new Mock<SLProtocol>();

            // Act
            QAction.Run(fakeprotocol.Object);

            // Assert
            foreach (object[] row in AddedRows(fakeprotocol, ServiceTable))
            {
                double bitrate = Convert.ToDouble(row[4]);
                Assert.IsTrue(bitrate >= 0 && bitrate < 30, $"Bitrate {bitrate} is out of range.");
            }
        }

        [TestMethod]
        public void Run_RowAlreadyExists_CallsSetRowInsteadOfAddRow()
        {
            // Arrange
            WriteDataFile(SampleJson);
            var fakeprotocol = new Mock<SLProtocol>();
            fakeprotocol.Setup(p => p.Exists(TransportstreamsTable, "1")).Returns(true);

            // Act
            QAction.Run(fakeprotocol.Object);

            // Assert
            string[] addedKeys = AddedRows(fakeprotocol, TransportstreamsTable)
                .Select(r => Convert.ToString(r[0]))
                .ToArray();
            CollectionAssert.AreEqual(new[] { "2" }, addedKeys);

            bool setRowCalledForKey1 = fakeprotocol.Invocations.Any(i =>
                i.Method.Name == "SetRow"
                && (int)i.Arguments[0] == TransportstreamsTable
                && (string)i.Arguments[1] == "1");
            Assert.IsTrue(setRowCalledForKey1);
        }

        [TestMethod]
        public void Run_FileWithoutTransportStreams_AddsNoRows()
        {
            // Arrange
            WriteDataFile("{}");
            var fakeprotocol = new Mock<SLProtocol>();

            // Act
            QAction.Run(fakeprotocol.Object);

            // Assert
            Assert.AreEqual(0, AddedRows(fakeprotocol, TransportstreamsTable).Length);
        }

        [TestMethod]
        public void Run_InvalidJson_DoesNotThrowAndAddsNoRows()
        {
            // Arrange
            WriteDataFile("{ not valid json");
            var fakeprotocol = new Mock<SLProtocol>();

            // Act (Run catches the exception and logs it)
            QAction.Run(fakeprotocol.Object);

            // Assert
            Assert.AreEqual(0, AddedRows(fakeprotocol, TransportstreamsTable).Length);
        }
    }
}