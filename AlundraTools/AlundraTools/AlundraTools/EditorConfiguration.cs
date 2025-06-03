using System.Text.Json.Serialization;

namespace AlundraTools.AlundraTools;

public class EditorConfiguration
{
    [JsonPropertyName("psy_q_sdk_folder")]
    public string PsyqSdkFolder { get; set; }
}