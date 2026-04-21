using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skyline.Protocol;
using Skyline.DataMiner.Scripting;
using Skyline.DataMiner.Utils.Protocol.Extension;

/// <summary>
/// DataMiner QAction Class.
/// </summary>
public static class QAction
{
	/// <summary>
	/// The QAction entry point.
	/// </summary>
	/// <param name="protocol">Link with SLProtocol process.</param>
	public static void Run(SLProtocolExt protocol)
	{
		try
		{
            protocol.Log($"QA3|Running", LogType.Allways, LogLevel.NoLogging);
            DataPoller.PollData(protocol);
        }
        catch (Exception ex)
		{
            protocol.Log($"QA3|Run|Unexpected exception: {ex}",LogType.Error,LogLevel.NoLogging);
        }
	}
}
