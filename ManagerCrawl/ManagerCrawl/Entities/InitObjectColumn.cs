using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerCrawl.Entities
{
    public class InitObjectColumn
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public int? Width { get; set; }
        public bool IsWidthPercent { get; set; } = false;
    }
}
