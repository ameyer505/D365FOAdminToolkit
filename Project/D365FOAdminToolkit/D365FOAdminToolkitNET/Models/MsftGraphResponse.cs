using Newtonsoft.Json;
using System.Collections.Generic;

namespace D365FOAdminToolkitNET.Models
{
    public class MsftGraphResponse<T>
    {
        [JsonProperty("@odata.context")]
        public string context { get; set; }
        [JsonProperty("@odata.nextLink")]
        public string nextLink { get; set; }
        public List<T> value { get; set; }
    }
}
