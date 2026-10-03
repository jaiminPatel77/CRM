using System.Text.Json.Serialization;

namespace Crm.Tests.Common;

public class TestResult<T>
{
    [JsonPropertyName("succeeded")]
    public bool Succeeded { get; set; }

    [JsonPropertyName("errors")]
    public string[]? Errors { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }
}
