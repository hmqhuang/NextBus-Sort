using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace NextBus_Sort.DataModels
{
    [Table("BusStops")]
    public class StopData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string StopName { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string Routes { get; set; }
        public string StopTags { get; set; }

        public StopData(string name, double lon, double lat, string routes, string tags)
        {
            StopName = name;
            Longitude = lon;
            Latitude = lat;
            Routes = routes;
            StopTags = tags;
        }
    }
}
