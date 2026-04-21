using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skyline.Protocol;

using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class: After Startup.
/// </summary>
public static class QAction
{
    /// <summary>
    /// The QAction entry point.
    /// </summary>
    /// <param name="protocol">Link with SLProtocol process.</param>
    public static void Run(SLProtocol protocol)
    {
        try
        {
            protocol.Log("QA2|Run|Start",LogType.Allways,LogLevel.NoLogging);
            DataPoller.PollData(protocol);
        }
        catch (Exception ex)
        {
            protocol.Log($"QA2|Run|Unexpected exception: {ex}",LogType.Error,LogLevel.NoLogging);
        }
    }
}
