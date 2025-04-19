using System.Text.Json.Serialization;

namespace GraphicsTools.Alundra;

public class EditorConfiguration
{
    [JsonPropertyName("psy_q_sdk_folder")]
    public string PsyqSdkFolder { get; set; }
}