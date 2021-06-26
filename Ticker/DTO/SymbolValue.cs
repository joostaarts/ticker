using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ticker.DTO
{    
    public class SymbolValue
    {
        public long t { get; set; }
        public long T { get; set; }
        public string s { get; set; }
        public string i { get; set; }
        public int f { get; set; }
        public int L { get; set; }
        public string o { get; set; }
        [JsonPropertyName("c")]
        public string ClosingValue { get; set; }
        public string h { get; set; }
        public string l { get; set; }
        public string v { get; set; }
        public int n { get; set; }
        public bool x { get; set; }
        public string q { get; set; }
        public string V { get; set; }
        public string Q { get; set; }
        public string B { get; set; }
    }


}
