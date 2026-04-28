using System.Collections.Generic;
using Skyline.DataMiner.Scripting;

/// <summary>
/// Defines the contract for loading, mapping, and pushing transport stream data to the protocol.
/// </summary>
public interface ITransportStreamService
{
    /// <summary>
    /// Loads transport stream data from the specified file, maps it to row objects,
    /// and pushes the result to the protocol via <c>FillArray</c>.
    /// </summary>
    /// <param name="protocol">The <see cref="SLProtocol"/> instance used to write table data.</param>
    /// <param name="filePath">
    /// Path to the JSON file. Defaults to the standard DMA documents location when not specified.
    /// </param>
    void Execute(SLProtocol protocol, string filePath = @"C:\Skyline DataMiner\Documents\DMA_COMMON_DOCUMENTS\Data.json");

    /// <summary>
    /// Maps a <see cref="Root"/> object to transport stream and service row collections.
    /// Services with a non-positive bitrate receive a randomly generated value.
    /// </summary>
    /// <param name="root">The deserialized root object containing transport streams and their services.</param>
    /// <returns>
    /// A tuple of transport stream rows and service rows ready to be written to the protocol tables.
    /// </returns>
    (List<TransportstreamsQActionRow>, List<ServicesQActionRow>) Map(Root root);
}