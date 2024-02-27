using System.IO;

namespace D365FOAdminToolkitNET.Models
{
    public class CsvFile
    {
        public string Name { get; set; }
        public MemoryStream Contents { get; set; }
    }
}
