namespace Skyline.DataMiner.Utils.UnitTestingFramework.Tests.Protocol
{
	using System;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;
	using Skyline.DataMiner.Scripting;

	/// <summary>
	/// Unit tests for <see cref="QAction"/>, verifying that the entry point correctly
	/// delegates to <see cref="TransportStreamService"/> and handles exceptions gracefully.
	/// </summary>
	[TestClass]
	public class QActionTests
	{
		/// <summary>
		/// Verifies that <see cref="QAction.Run"/> calls <see cref="ITransportStreamService.Execute"/>
		/// once with the provided protocol instance on the happy path.
		/// </summary>
		[TestMethod]
		public void Run_Should_Call_Execute_Once()
		{
			// Arrange
			var mockProtocol = new Mock<SLProtocolExt>();
			var mockService = new Mock<ITransportStreamService>();

			// Act
			QAction.Run(mockProtocol.Object, mockService.Object);

			// Assert
			mockService.Verify(s => s.Execute(mockProtocol.Object, null), Times.Once);
		}

		/// <summary>
		/// Verifies that <see cref="QAction.Run"/> does not propagate exceptions thrown by
		/// <see cref="ITransportStreamService.Execute"/>, and instead logs the error via
		/// <see cref="SLProtocol.Log"/>.
		/// </summary>
		[TestMethod]
		public void Run_Should_Log_And_Not_Throw_On_Exception()
		{
			// Arrange
			var mockProtocol = new Mock<SLProtocolExt>();
			var mockService = new Mock<ITransportStreamService>();

			mockService	
				.Setup(s => s.Execute(It.IsAny<SLProtocolExt>(), It.IsAny<string>()))
				.Throws(new Exception("something went wrong"));

			// Act
			Action act = () => QAction.Run(mockProtocol.Object, mockService.Object);

			// Assert – no exception propagated
			act(); // would throw if catch block is missing

			mockProtocol.Verify(
				p => p.Log(
					It.Is<string>(msg => msg.Contains("something went wrong")),
					LogType.Error,
					LogLevel.NoLogging),
				Times.Once);
		}
	}
}