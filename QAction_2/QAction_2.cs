using System;
using Skyline.Protocol;

using Skyline.DataMiner.Scripting;

/// <summary>
/// DataMiner QAction Class: After Startup.
/// </summary>
public static class QAction
{
    /// <summary>
    /// After startup poll.
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
            protocol.Log($"QA2|Run|Unexpected exception: {ex}",LogType.Error,LogLevel.NoLogging);
        }
    }
}
