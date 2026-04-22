using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Skyline.DataMiner.Scripting;

namespace Skyline.DataMiner.Utils.UnitTestingFramework.Tests.Protocol
{
	[TestClass]
	public class TransportStreamServiceTests
	{
		// ─────────────────────────────────────────────
		// Helpers
		// ─────────────────────────────────────────────

		private static Root BuildRoot(List<TransportStream> streams) =>
			new Root { TransportStreams = streams };

		private static TransportStream BuildTs(
			int tsId = 1, string name = "TS1", string multicast = "239.0.0.1", string sourceIp = "10.0.0.1", int networkId = 100, List<Service> services = null) =>
			new TransportStream
			{
				TsId = tsId,
				TsName = name,
				Multicast = multicast,
				SourceIp = sourceIp,
				NetworkId = networkId,
				Services = services,
			};

		private static Service BuildService(
			int serviceId = 10, string name = "SVC1", string type = "TV", string provider = "Provider1", double bitrate = 50.0) =>
			new Service
			{
				ServiceId = serviceId,
				ServiceName = name,
				ServiceType = type,
				ServiceProvider = provider,
				ServiceBitrate = bitrate,
			};

		[TestMethod]
		public void Should_Map_TransportStreams_Correctly()
		{
			// Arrange
			var service = new TransportStreamService();
			var root = new Root
			{
				TransportStreams = new List<TransportStream>
				{
					new TransportStream
					{
						TsId = 1,
						TsName = "TS1",
						Multicast = "239.0.0.1",
						SourceIp = "192.168.1.1",
						NetworkId = 100,
						Services = new List<Service>
						{
							new Service
							{
								ServiceId = 10,
								ServiceName = "Service1",
								ServiceType = "TV",
								ServiceProvider = "Provider1",
								ServiceBitrate = 50,
							},
						},
					},
				},
			};

			// Act
			var (tsRows, svcRows) = service.Map(root);

			// Assert – TS
			Assert.AreEqual(1, tsRows.Count);
			var ts = tsRows[0];
			Assert.AreEqual("1", ts.Transportstreamsid_1001);
			Assert.AreEqual("TS1", ts.Transportstreamsname_1002);
			Assert.AreEqual("239.0.0.1", ts.Transportstreamsmulticast_1003);
			Assert.AreEqual("192.168.1.1", ts.Transportstreamsip_1004);
			Assert.AreEqual("100", ts.Transportstreamsnetworkid_1005);
			Assert.AreEqual(1.0, ts.Transportstreamsnumberofservices_1006);

			// Assert – Service
			Assert.AreEqual(1, svcRows.Count);
			var svc = svcRows[0];
			Assert.AreEqual("1/10", svc.Servicesinstanceid_2001);
			Assert.AreEqual(10.0, svc.Servicesid_2002);
			Assert.AreEqual("Service1", svc.Servicesname_2003);
			Assert.AreEqual("TV", svc.Servicestype_2004);
			Assert.AreEqual("Provider1", svc.Servicesprovider_2005);
			Assert.AreEqual(50.0, svc.Servicesbitrate_2006);
			Assert.AreEqual("1", svc.Servicestransportstreamid_2007);
			Assert.AreEqual("TS1", svc.Servicestransportstreamnameservice_2009);
		}

		[TestMethod]
		public void Should_Set_ServiceCount_To_Zero_When_Services_Is_Null()
		{
			var root = BuildRoot(new List<TransportStream> { BuildTs(services: null) });

			var (tsRows, svcRows) = new TransportStreamService().Map(root);

			Assert.AreEqual(1, tsRows.Count);
			Assert.AreEqual(0.0, tsRows[0].Transportstreamsnumberofservices_1006);
			Assert.AreEqual(0, svcRows.Count);
		}

		[TestMethod]
		public void Should_Produce_Correct_Row_Count_For_Multiple_Streams()
		{
			var root = BuildRoot(new List<TransportStream>
			{
				BuildTs(tsId: 1, services: new List<Service> { BuildService(1), BuildService(2) }),
				BuildTs(tsId: 2, services: new List<Service> { BuildService(3) }),
				BuildTs(tsId: 3, services: null),
			});

			var (tsRows, svcRows) = new TransportStreamService().Map(root);

			Assert.AreEqual(3, tsRows.Count);
			Assert.AreEqual(3, svcRows.Count);
		}

		[TestMethod]
		public void Should_Produce_Unique_Service_Keys_Across_Streams()
		{
			var root = BuildRoot(new List<TransportStream>
			{
				BuildTs(tsId: 1, services: new List<Service> { BuildService(10), BuildService(20) }),
				BuildTs(tsId: 2, services: new List<Service> { BuildService(10) }),
			});

			var (_, svcRows) = new TransportStreamService().Map(root);

			var keys = svcRows.Select(r => r.Servicesinstanceid_2001).ToList();
			CollectionAssert.AllItemsAreUnique(keys);
			CollectionAssert.Contains(keys, "1/10");
			CollectionAssert.Contains(keys, "1/20");
			CollectionAssert.Contains(keys, "2/10");
		}

		[TestMethod]
		public void Should_Return_Empty_Lists_When_No_Streams()
		{
			var root = BuildRoot(new List<TransportStream>());

			var (tsRows, svcRows) = new TransportStreamService().Map(root);

			Assert.AreEqual(0, tsRows.Count);
			Assert.AreEqual(0, svcRows.Count);
		}

		[TestMethod]
		public void Should_Throw_When_Root_Is_Null()
		{
			// Act
			Action act = () => new TransportStreamService().Map(null);

			// Assert
			Assert.Throws<NullReferenceException>(act);
		}


		[TestMethod]
		public void Should_Use_Provided_Bitrate_When_Positive()
		{
			var root = BuildRoot(new List<TransportStream>
			{
				BuildTs(services: new List<Service> { BuildService(bitrate: 12.345) }),
			});

			var (_, svcRows) = new TransportStreamService().Map(root);

			Assert.AreEqual(12.345, svcRows[0].Servicesbitrate_2006);
		}

		[TestMethod]
		public void Should_Generate_Random_Bitrate_When_Bitrate_Is_Zero()
		{
			var root = BuildRoot(new List<TransportStream>
			{
				BuildTs(services: new List<Service> { BuildService(bitrate: 0) })
			});

			var (_, svcRows) = new TransportStreamService(maxBitrate: 100).Map(root);

			double bitrate = (double)svcRows[0].Servicesbitrate_2006;
			Assert.IsTrue(bitrate >= 0 && bitrate <= 100, $"Bitrate {bitrate} out of range");
		}

		[TestMethod]
		public void Should_Generate_Random_Bitrate_When_Bitrate_Is_Negative()
		{
			var root = BuildRoot(new List<TransportStream>
			{
				BuildTs(services: new List<Service> { BuildService(bitrate: -1) })
			});

			var (_, svcRows) = new TransportStreamService(maxBitrate: 50).Map(root);

			double bitrate = (double)svcRows[0].Servicesbitrate_2006;
			Assert.IsTrue(bitrate >= 0 && bitrate <= 50, $"Bitrate {bitrate} out of range");
		}

		[TestMethod]
		public void Should_Respect_Decimal_Places_For_Random_Bitrate()
		{
			var root = BuildRoot(new List<TransportStream>
			{
				BuildTs(services: new List<Service> { BuildService(bitrate: 0) }),
			});

			int decimals = 2;
			var (_, svcRows) = new TransportStreamService(rng: new Random(42), decimals: decimals).Map(root);

			double bitrate = (double)svcRows[0].Servicesbitrate_2006;
			Assert.AreEqual(Math.Round(bitrate, decimals), bitrate, $"Bitrate {bitrate} has more than {decimals} decimal places");
		}

		[TestMethod]
		public void Execute_Should_Call_FillArray_For_Both_Tables()
		{
			// Arrange
			var fakeRoot = BuildRoot(new List<TransportStream>
			{
				BuildTs(tsId: 1, services: new List<Service> { BuildService() }),
			});

			var mockLoader = new Mock<IJsonLoader>();
			mockLoader
				.Setup(l => l.Load(It.IsAny<string>()))
				.Returns(fakeRoot);

			var mockProtocol = new Mock<SLProtocol>();

			var sut = new TransportStreamService(loader: mockLoader.Object);

			// Act
			sut.Execute(mockProtocol.Object);

			// Assert
			mockProtocol.Verify(
				p => p.FillArray(
				Parameter.Transportstreams.tablePid,
				It.IsAny<List<object[]>>(),
				NotifyProtocol.SaveOption.Full), Times.Once);

			mockProtocol.Verify(
				p => p.FillArray(
				Parameter.Services.tablePid,
				It.IsAny<List<object[]>>(),
				NotifyProtocol.SaveOption.Full), Times.Once);
		}

		[TestMethod]
		public void Execute_Should_Pass_FilePath_To_Loader()
		{
			// Arrange
			var mockLoader = new Mock<IJsonLoader>();
			mockLoader
				.Setup(l => l.Load(It.IsAny<string>()))
				.Returns(BuildRoot(new List<TransportStream>()));

			var mockProtocol = new Mock<SLProtocol>();
			var sut = new TransportStreamService(loader: mockLoader.Object);
			string customPath = @"C:\custom\path.json";

			// Act
			sut.Execute(mockProtocol.Object, customPath);

			// Assert
			mockLoader.Verify(l => l.Load(customPath), Times.Once);
		}
	}
}