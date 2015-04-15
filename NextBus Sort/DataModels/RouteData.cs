using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextBus_Sort.DataModels
{
    public class RouteData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; }

        public RouteData(string name)
        {
            Title = name;
        }
    }
}
