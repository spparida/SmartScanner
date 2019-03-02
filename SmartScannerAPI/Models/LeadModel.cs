using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartScannerAPI.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class LeadModel
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Company")]
        public string Company { get; set; }

        [JsonProperty(PropertyName = "Title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "CityStateZip")]
        public string CityStateZip { get; set; }

        [JsonProperty(PropertyName = "Email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "Website")]
        public string Website { get; set; }

        [JsonProperty(PropertyName = "Facebook")]
        public string Facebook { get; set; }

        [JsonProperty(PropertyName = "Twitter")]
        public string Twitter { get; set; }

        [JsonProperty(PropertyName = "Phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "Fax")]
        public string Fax { get; set; }

        [JsonProperty(PropertyName = "Cell")]
        public string Cell { get; set; }

        [JsonProperty(PropertyName = "Total")]
        public string Total { get; set; }

        [JsonProperty(PropertyName = "Date")]
        public string Date { get; set; }
    }
}