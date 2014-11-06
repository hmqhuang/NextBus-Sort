using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace NextBus_Sort
{
    class Program
    {
        static void Main(string[] args)
        {
            //Get XML listing all routes of SF Muni
            XmlTextReader reader = new XmlTextReader("http://webservices.nextbus.com/service/publicXMLFeed?command=routeList&a=sf-muni");
            List<string> routeNums = new List<string>();
            List<string> routeTitles = new List<string>();

            //Read each XML line and add the route number or letter to a list
            Console.WriteLine("Getting route list...");
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name == "body") break;
                        reader.MoveToFirstAttribute();
                        routeNums.Add(reader.Value);
                        reader.MoveToNextAttribute();
                        routeTitles.Add(reader.Value);
                        break;
                    default:
                        break;
                }
            }

            //Print out list of routes
            Console.WriteLine("Writing routes to text file...");
            StreamWriter output = new StreamWriter("routes.txt");
            for (int i = 0; i < routeTitles.Count; i++)
            {
                output.WriteLine(routeTitles[i].ToString());
            }
            output.Close();
            Console.WriteLine("---routes.txt created---");

            System.Console.WriteLine("Getting all bus stops...");
            List<Stop> allStopsList = new List<Stop>();
            string base_xml = "http://webservices.nextbus.com/service/publicXMLFeed?command=routeConfig&a=sf-muni&r=";
            string temp;
            int counter = 0;

            //Go through each route in RouteNum and append to the base URL. Get XML from each URL and parse
            for (int i = 0; i < routeNums.Count; i++)
            {
                temp = base_xml + routeNums[i];
                reader = new XmlTextReader(temp);

                //Parse XML of all stops for a specific route
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "body") break;
                            else if (reader.Name == "route") break;
                            else if (reader.Name == "direction") break;
                            else if (reader.Name == "path") break;
                            else if (reader.Name == "point") break;

                            //Add stop data to the allStopsList List
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name == "tag")
                                {
                                    allStopsList.Add(new Stop(reader.Value, routeNums[i], 0));
                                    counter++;
                                }
                                if (reader.Name == "title")
                                {
                                    allStopsList[counter - 1].title = reader.Value;
                                }
                                if (reader.Name == "lon")
                                {
                                    allStopsList[counter - 1].lon = reader.Value;
                                }
                                if (reader.Name == "lat")
                                {
                                    allStopsList[counter - 1].lat = reader.Value;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            //Prepare to sort all the stops for duplicates and output results to a text file
            System.Console.WriteLine("Merging bus stops...");
            output = new StreamWriter("sorted.txt");
            string key;

            List<Stop> firstSort = new List<Stop>();
            List<Stop> found = new List<Stop>();
            List<String> used = new List<String>();

            counter = 0;
            for (int j = 0; j < allStopsList.Count; j++)
            {
                key = allStopsList[j].title;
                if (key != null)
                {
                    if (firstSort.Exists(y => y.title == key) == false)
                    {
                        found = allStopsList.FindAll(x => x.title == key);
                        firstSort.Add(new Stop(found[0].tag, found[0].route, 1));
                        firstSort[counter].title = found[0].title;
                        firstSort[counter].lon = found[0].lon;
                        firstSort[counter].lat = found[0].lat;

                        for (int l = 1; l < found.Count; l++)
                        {
                            if (firstSort[counter].allRoutes.Contains(" " + found[l].route) == false)
                            {
                                firstSort[counter].AddRoute(found[l].route);
                                firstSort[counter].AddTag(found[l].route, found[l].tag);
                                firstSort[counter].AddStopTag(found[l].tag);
                            }
                            if (firstSort[counter].allRoutes.Contains(" " + found[l].route) == true)
                            {
                                if (firstSort[counter].allTags.Contains(found[l].route + "|" + found[l].tag) != true)
                                {
                                    firstSort[counter].AddTag(found[l].route, found[l].tag);
                                    firstSort[counter].AddStopTag(found[l].tag);
                                }
                            }
                        }
                        counter++;
                    }
                }
            }

            //Remove "Inbound/Outbound" from metro station names
            foreach (Stop s in firstSort)
            {
                if (s.title.Contains("Inbound"))
                {
                    s.title = s.title.Replace(" Inbound", "");
                }
                if (s.title.Contains("Outbound"))
                {
                    s.title = s.title.Replace(" Outbound", "");
                }
            }

            //Prepare for second sort to remove similar bus stops e.g. 5th & Mission St / Mission St. & 5th St
            System.Console.WriteLine("Sorting stops...");
            string[] titleSplit;
            found.Clear();
            used.Clear();
            List<Stop> secondSort = new List<Stop>();
            foreach (Stop s in firstSort)
            {
                titleSplit = s.title.Split('&');

                if (!used.Contains(s.title))
                {
                    if (titleSplit.Count() > 1) key = titleSplit[1].Substring(1) + " & " + titleSplit[0].Substring(0, (titleSplit[0].Length - 1));
                    else key = s.title;

                    if (firstSort.Any(x => x.title == key))
                    {
                        found = firstSort.FindAll(y => y.title == key);

                        foreach (Stop d in found)
                        {
                            foreach (string r in d.allRoutes)
                            {
                                s.AddRoute(r);
                            }

                            foreach (string t in d.allTags)
                            {
                                s.AddTag(t);
                            }

                            foreach (string t in d.StopTags)
                            {
                                s.AddStopTag(t);
                            }
                        }
                    }
                    used.Add(key);
                    secondSort.Add(s);
                }
            }

            System.Console.WriteLine("Writing sorted list to text file...");
            for (int k = 0; k < secondSort.Count; k++)
            {
                output.WriteLine(secondSort[k].title + "%" + secondSort[k].lon + "%" + secondSort[k].lat + "%" + secondSort[k].ListRoutes() + "%" + secondSort[k].ListStopTags() + "%" + secondSort[k].ListTags());
            }
            output.Close();
            Console.WriteLine("---sorted.txt created---");

            System.Console.WriteLine("Finished!\n\nPress any key to exit");
            Console.ReadKey();
        }
    }

    public class Stop
    {
        public string title { get; set; }
        public string route { get; set; }
        public List<String> allTags = new List<String>();
        public List<String> allRoutes { get; set; }
        public string tag { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public List<String> StopTags = new List<String>();

        public Stop()
        {

        }

        public Stop(string _tag, string r, int id)
        {

            if (id == 0)
            {
                //if (r.Equals("K_OWL")) r = "K OWL";
                //else if (r.Equals("L_OWL")) r = "L OWL";
                //else if (r.Equals("M_OWL")) r = "M OWL";
                //else if (r.Equals("N_OWL")) r = "N OWL";
                //else if (r.Equals("T_OWL")) r = "T OWL";

                this.tag = _tag;
                this.route = r;
            }
            else if (id == 1)
            {
                allTags = new List<String>();
                allRoutes = new List<String>();

                this.AddTag(r, _tag);
                allRoutes.Add(" " + r);
                AddStopTag(_tag);
            }
        }

        public void AddRoute(string rt)
        {
            string route;

            if (rt.Equals("K_OWL")) route = " K OWL";
            else if (rt.Equals("L_OWL")) route = " L OWL";
            else if (rt.Equals("M_OWL")) route = " M OWL";
            else if (rt.Equals("N_OWL")) route = " N OWL";
            else if (rt.Equals("T_OWL")) route = " T OWL";
            else if (rt.Contains(' ')) route = rt;
            else route = " " + rt;

            if (!allRoutes.Contains(route)) allRoutes.Add(route);
        }

        public string ListRoutes()
        {
            string text = string.Join(",", allRoutes);
            return text;
        }
        public void AddTag(string rt, string tag)
        {
            this.allTags.Add(rt + "|" + tag);
        }

        public void AddTag(string tag)
        {
            if (!allTags.Contains(tag)) this.allTags.Add(tag);
        }

        public void AddStopTag(string _tag)
        {
            if (!StopTags.Contains(_tag)) StopTags.Add(_tag);
        }

        public string ListTags()
        {
            string taglist = string.Join(",", allTags);
            return taglist;
        }

        public string ListStopTags()
        {
            string taglist = string.Join(",", StopTags);
            return taglist;
        }
    }
}

