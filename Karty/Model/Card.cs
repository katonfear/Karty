using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karty.Model
{
    internal class Card
    {
        [JsonProperty("serialnumber")]
        public string? SerialNumber { get; set; }
        [JsonProperty("accesscode")]
        public string? AccessCode { get; set; }
    }
}
