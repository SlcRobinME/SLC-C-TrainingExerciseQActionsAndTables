using System;
using Skyline.Protocol;
using Skyline.DataMiner.Scripting;


/// <summary>
/// DataMiner QAction Class.
/// </summary>
public static class QAction
{
	/// <summary>
	/// Timer poll.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocol protocol)
	{
		try
		{
            DataPoller.PollData(protocol);
        }
        catch (Exception ex)
		{
            protocol.Log($"QA3|Run|Unexpected exception: {ex}",LogType.Error,LogLevel.NoLogging);
        }
	}
}
