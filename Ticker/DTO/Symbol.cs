using System.Text.Json.Serialization;

namespace Ticker.DTO
{
    public class Symbol
    {
        public string e { get; set; }
        public long E { get; set; }
        public string s { get; set; }
        [JsonPropertyName("k")]
        public SymbolValue Value { get; set; }
    }
}
