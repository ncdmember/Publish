using ManagerCrawl.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagerCrawl.DAL.Interfaces
{
    public interface IManager
    {
        List<InitObjectColumn> InitObjectColumns();
        void GetData(object sender, EventArgs e);
    }
}
