using System;
using Skyline.Protocol;
using Skyline.DataMiner.Scripting;


/// <summary>
/// DataMiner QAction Class.
/// </summary>
public static class QAction
{
	/// <summary>
	/// Manual poll button.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocolExt protocol)
	{
        try
        {
            string data = DataPoller.getData(protocol,DataPoller.JsonFilePath);
            DataPoller.PollData(protocol, data);
        }
		catch (Exception ex)
		{
            protocol.Log($"QA4|Run|Unexpected exception: {ex}",LogType.Error,LogLevel.NoLogging);
        }
	}
}
