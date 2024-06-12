using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerCrawl.Entities.Indeed
{
    public class ObjectResult
    {
        public int stt { get; set; }
        public string link { get; set; }
        public int page { get; set; }
        public string name { get; set; }
        public string name_company { get; set; }
        public string profile_company { get; set; }
        public string tags { get; set; }
        public string description { get; set; }
        public string detail { get; set; }
        public string tel { get; set; }
        public string email { get; set; }
        public string summary { get; set; }
        public string website { get; set; }
        public string recruitment_form { get; set; }
        public string salary { get; set; }
    }
}
