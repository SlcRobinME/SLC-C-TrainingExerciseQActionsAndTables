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
	public static void Run(SLProtocol protocol)
	{
        protocol.Log("QA4|Run|BUTTON CLICKED", LogType.Allways, LogLevel.NoLogging);

        try
        {
            protocol.Log("QA4|Run|Force poll triggered by operator.",LogType.Allways,LogLevel.NoLogging);

            DataPoller.PollData(protocol);
        }
		catch (Exception ex)
		{
            protocol.Log($"QA4|Run|Unexpected exception: {ex}",LogType.Error,LogLevel.NoLogging);
        }
	}
}
