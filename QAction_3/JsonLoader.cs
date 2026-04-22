using System.IO;
using Skyline.DataMiner.Utils.SecureCoding.SecureIO;
using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

public class JsonLoader : IJsonLoader
{
    public Root Load(string filePath)
    {
        string jsonRaw = File.ReadAllText(SecurePath.CreateSecurePath(filePath));
        return SecureNewtonsoftDeserialization.DeserializeObject<Root>(jsonRaw);
    }
}