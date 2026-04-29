namespace Skyline.DataMiner.Utils.UnitTestingFramework.Tests.Protocol
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using FluentAssertions;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;
	using Skyline.DataMiner.Scripting;
	using Skyline.DataMiner.Utils.UnitTestingFramework.Protocol;

	/// <summary>
	/// Unit tests for <see cref="TransportStreamService"/>, covering mapping logic,
	/// bitrate generation, service count calculation, and protocol integration.
	/// </summary>
	[TestClass]
	public class TransportStreamServiceTests
	{
		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Map"/> correctly maps all fields
		/// of a transport stream and its services to the corresponding row properties.
		/// </summary>
		[TestMethod]
		public void Should_Map_TransportStreams_Correctly()
		{
			// Arrange
			ITransportStreamService service = new TransportStreamService();
			var root = new Root
			{
				TransportStreams = new List<TransportStream>
				{
					new ()
					{
						TsId = 1,
						TsName = "TS1",
						Multicast = "239.0.0.1",
						SourceIp = "192.168.1.1",
						NetworkId = 100,
						Services = new List<Service>
						{
							new ()
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
			var expectedTs = new TransportstreamsQActionRow
			{
				Transportstreamsid_1001 = "1",
				Transportstreamsname_1002 = "TS1",
				Transportstreamsmulticast_1003 = "239.0.0.1",
				Transportstreamsip_1004 = "192.168.1.1",
				Transportstreamsnetworkid_1005 = "100",
				Transportstreamsnumberofservices_1006 = 1.0,
			};

			tsRows.Should().HaveCount(1);
			tsRows[0].Should().BeEquivalentTo(expectedTs, options => options
				.ExcludingMissingMembers()
				.Excluding(r => r.Columns)
				.Excluding(r => r.Transportstreamslastpolled_1007)
				.Excluding(r => r.Transportstreamslastpolled));

			// Assert – Service
			var expectedSvc = new ServicesQActionRow
			{
				Servicesinstanceid_2001 = "1/10",
				Servicesid_2002 = 10.0,
				Servicesname_2003 = "Service1",
				Servicestype_2004 = "TV",
				Servicesprovider_2005 = "Provider1",
				Servicesbitrate_2006 = 50.0,
				Servicestransportstreamid_2007 = "1",
				Servicestransportstreamnameservice_2009 = "TS1",
			};

			svcRows.Should().HaveCount(1);
			svcRows[0].Should().BeEquivalentTo(expectedSvc, options => options
				.ExcludingMissingMembers()
				.Excluding(r => r.Columns)
				.Excluding(r => r.Serviceslastpolled_2008)
				.Excluding(r => r.Serviceslastpolled));
		}

		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Map"/> sets the service count to zero
		/// and returns an empty service row list when the stream has no services (null or empty list).
		/// </summary>
		/// <param name="useEmptyList">
		/// <c>true</c> to pass an empty <see cref="List{Service}"/>;
		/// <c>false</c> to pass <c>null</c>.
		/// </param>
		[TestMethod]
		[DataRow(false, DisplayName = "services is null")]
		[DataRow(true, DisplayName = "services is empty list")]
		public void Should_Set_ServiceCount_To_Zero_When_No_Services(bool useEmptyList)
		{
			var services = useEmptyList ? new List<Service>() : null;
			var root = BuildRoot(new List<TransportStream> { BuildTs(services: services) });
			var (tsRows, svcRows) = new TransportStreamService().Map(root);

			tsRows.Should().HaveCount(1);
			tsRows[0].Transportstreamsnumberofservices_1006.Should().Be(0.0);
			svcRows.Should().BeEmpty();
		}

		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Map"/> returns the correct number of
		/// transport stream rows and service rows for various stream/service configurations.
		/// </summary>
		/// <param name="tsCount">Number of transport streams to create.</param>
		/// <param name="totalServices">Number of services to assign to the first stream.</param>
		/// <param name="expectedTs">Expected number of transport stream rows.</param>
		/// <param name="expectedSvc">Expected number of service rows.</param>
		[TestMethod]
		[DataRow(1, 0, 1, 0, DisplayName = "single stream, no services")]
		[DataRow(1, 3, 1, 3, DisplayName = "single stream, 3 services")]
		[DataRow(3, 3, 3, 3, DisplayName = "3 streams, mixed (2+1+null)")]
		[DataRow(5, 0, 5, 0, DisplayName = "5 streams, all null")]
		public void Should_Produce_Correct_Row_Count(int tsCount, int totalServices, int expectedTs, int expectedSvc)
		{
			var streams = Enumerable.Range(1, tsCount)
				.Select((id, index) => BuildTs(
					tsId: id,
					services: index == 0 && totalServices > 0
						? Enumerable.Range(1, totalServices).Select(s => BuildService(s)).ToList()
						: null))
				.ToList();

			var (tsRows, svcRows) = new TransportStreamService().Map(BuildRoot(streams));

			tsRows.Should().HaveCount(expectedTs);
			svcRows.Should().HaveCount(expectedSvc);
		}

		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Map"/> generates unique composite keys
		/// (tsId/serviceId) across all transport streams, even when services share the same ID
		/// in different streams.
		/// </summary>
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

			keys.Should().OnlyHaveUniqueItems();
			keys.Should().Contain(new[] { "1/10", "1/20", "2/10" });
		}

		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Map"/> returns empty lists for both
		/// transport streams and services when the root contains no streams.
		/// </summary>
		[TestMethod]
		public void Should_Return_Empty_Lists_When_No_Streams()
		{
			var root = BuildRoot(new List<TransportStream>());

			var (tsRows, svcRows) = new TransportStreamService().Map(root);

			tsRows.Should().BeEmpty();
			svcRows.Should().BeEmpty();
		}

		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Map"/> throws a
		/// <see cref="NullReferenceException"/> when <c>null</c> is passed as the root object.
		/// </summary>
		[TestMethod]
		public void Should_Throw_When_Root_Is_Null()
		{
			// Act
			Action act = () => new TransportStreamService().Map(null);

			// Assert
			act.Should().Throw<NullReferenceException>();
		}

		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Map"/> preserves a positive bitrate
		/// value as-is without applying any random generation.
		/// </summary>
		[TestMethod]
		public void Should_Use_Provided_Bitrate_When_Positive()
		{
			var root = BuildRoot(new List<TransportStream>
			{
				BuildTs(services: new List<Service> { BuildService(bitrate: 12.345) }),
			});

			var (_, svcRows) = new TransportStreamService().Map(root);

			svcRows[0].Servicesbitrate_2006.Should().Be(12.345);
		}

		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Map"/> generates a random bitrate
		/// within <c>[0, maxBitrate]</c> when the service bitrate is zero or negative.
		/// </summary>
		/// <param name="inputBitrate">The non-positive bitrate value to set on the service.</param>
		/// <param name="maxBitrate">The upper bound passed to <see cref="TransportStreamService"/>.</param>
		[TestMethod]
		[DataRow(0, 100, DisplayName = "zero bitrate")]
		[DataRow(-1, 50, DisplayName = "negative bitrate")]
		[DataRow(-999, 75, DisplayName = "large negative bitrate")]
		public void Should_Generate_Random_Bitrate_When_NonPositive(double inputBitrate, int maxBitrate)
		{
			var root = BuildRoot(new List<TransportStream>
			{
				BuildTs(services: new List<Service> { BuildService(bitrate: inputBitrate) }),
			});
			var (_, svcRows) = new TransportStreamService(maxBitrate: maxBitrate).Map(root);
			double bitrate = (double)svcRows[0].Servicesbitrate_2006;
			bitrate.Should().BeInRange(0, maxBitrate, $"Bitrate {bitrate} is not in the range [0, {maxBitrate}]");
		}

		/// <summary>
		/// Verifies that the randomly generated bitrate is rounded to exactly the number of
		/// decimal places specified via the <c>decimals</c> constructor parameter.
		/// </summary>
		/// <param name="decimals">The number of decimal places to enforce.</param>
		[TestMethod]
		[DataRow(0, DisplayName = "0 decimal places")]
		[DataRow(1, DisplayName = "1 decimal place")]
		[DataRow(2, DisplayName = "2 decimal places")]
		[DataRow(4, DisplayName = "4 decimal places")]
		public void Should_Respect_Decimal_Places_For_Random_Bitrate(int decimals)
		{
			var root = BuildRoot(new List<TransportStream>
			{
				BuildTs(services: new List<Service> { BuildService(bitrate: 0) }),
			});

			var (_, svcRows) = new TransportStreamService(
				rng: new Random(42), decimals: decimals).Map(root);
			double bitrate = (double)svcRows[0].Servicesbitrate_2006;
			bitrate.Should().Be(Math.Round(bitrate, decimals), $"Bitrate {bitrate} has more than {decimals} decimal places");
		}

		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Execute"/> calls
		/// <c>FillArray</c> exactly once for each of the two tables
		/// (transport streams and services).
		/// </summary>
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

			var protocolMock = new SLProtocolMock();

			ITransportStreamService sut = new TransportStreamService(loader: mockLoader.Object);

			// Act
			sut.Execute(protocolMock.Object);

			// Assert
			protocolMock.Assert().Table(Parameter.Transportstreams.tablePid).RowCount.Should().Be(1);
			protocolMock.Assert().Table(Parameter.Services.tablePid).RowCount.Should().Be(1);
		}

		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Execute"/> forwards the provided
		/// file path to the <see cref="IJsonLoader"/> without modification.
		/// </summary>
		[TestMethod]
		public void Execute_Should_Pass_FilePath_To_Loader()
		{
			// Arrange
			var mockLoader = new Mock<IJsonLoader>();
			mockLoader
				.Setup(l => l.Load(It.IsAny<string>()))
				.Returns(BuildRoot(new List<TransportStream>()));

			var protocolMock = new SLProtocolMock();
			ITransportStreamService sut = new TransportStreamService(loader: mockLoader.Object);
			string customPath = @"C:\custom\path.json";

			// Act
			sut.Execute(protocolMock.Object, customPath);

			// Assert
			mockLoader.Verify(l => l.Load(customPath), Times.Once);
		}

		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Execute"/> correctly loads, maps,
		/// and fills both tables using the real <see cref="JsonLoader"/> with a valid JSON file.
		/// </summary>
		[TestMethod]
		public void Execute_Should_Load_And_Fill_Tables_Via_Real_Loader()
		{
			// Arrange
			string tempFile = Path.GetTempFileName();
			try
			{
				// Use the same Root/TransportStream/Service objects to serialize —
				// this guarantees JSON property names always match the model exactly
				var fakeRoot = BuildRoot(new List<TransportStream>
				{
					BuildTs(tsId: 1, services: new List<Service> { BuildService() }),
				});

				string json = Newtonsoft.Json.JsonConvert.SerializeObject(fakeRoot);
				File.WriteAllText(tempFile, json);

				var protocolMock = new SLProtocolMock();
				var sut = new TransportStreamService(); // real JsonLoader

				// Act
				sut.Execute(protocolMock.Object, tempFile);

				// Assert
				protocolMock.Assert().Table(Parameter.Transportstreams.tablePid).RowCount.Should().Be(1);
				protocolMock.Assert().Table(Parameter.Services.tablePid).RowCount.Should().Be(1);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Execute"/> throws when the file
		/// does not exist, propagating the error from the real <see cref="JsonLoader"/>.
		/// </summary>
		[TestMethod]
		public void Execute_Should_Throw_When_File_Does_Not_Exist()
		{
			// Arrange
			var protocolMock = new SLProtocolMock();
			var sut = new TransportStreamService(); // real JsonLoader
			string badPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

			// Act
			Action act = () => sut.Execute(protocolMock.Object, badPath);

			// Assert
			act.Should().Throw<Exception>();
		}

		/// <summary>
		/// Verifies that <see cref="TransportStreamService.Execute"/> throws when the file
		/// contains invalid JSON, propagating the deserialization error from <see cref="JsonLoader"/>.
		/// </summary>
		[TestMethod]
		public void Execute_Should_Throw_When_Json_Is_Invalid()
		{
			// Arrange
			string tempFile = Path.GetTempFileName();
			try
			{
				File.WriteAllText(tempFile, "not valid json {{{}}}");
				var protocolMock = new SLProtocolMock();
				var sut = new TransportStreamService(); // real JsonLoader

				// Act
				Action act = () => sut.Execute(protocolMock.Object, tempFile);

				// Assert
				act.Should().Throw<Exception>();
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

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
	}
}