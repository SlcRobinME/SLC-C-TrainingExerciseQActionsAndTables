using System;
using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class: Parse Transport Streams JSON.
/// </summary>
public static class QAction
{
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    /// <param name="service">DI for the transport stream service.</param>
    public static void Run(SLProtocolExt protocol, ITransportStreamService service = null)
    {
        try
        {
            if (service == null)
                service = new TransportStreamService();
            service.Execute(protocol);
        }
        catch (Exception ex)
        {
            protocol.Log($"QAction3|Exception: {ex.Message}\n{ex.StackTrace}", LogType.Error, LogLevel.NoLogging);
        }
    }
}