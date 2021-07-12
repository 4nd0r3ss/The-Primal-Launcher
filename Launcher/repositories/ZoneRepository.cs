using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PrimalLauncher
{
    public static class ZoneRepository
    {
        public static List<Zone> GetZones()
        {
            List<Zone> zones = new List<Zone>();
            XmlDocument zonesXml = new XmlDocument();
            string file = GetXmlResource();

            if (!string.IsNullOrEmpty(file))
            {
                zonesXml.LoadXml(file);
                XmlElement root = zonesXml.DocumentElement;
                               
                foreach (XmlNode node in root.ChildNodes)
                {
                    Type type = Type.GetType("PrimalLauncher." + node.Name);
                    Zone zone = (Zone)Activator.CreateInstance(type);
                  
                    zone.ClassName = "ZoneMaster" + node.Attributes["className"].Value;
                    zone.RegionId = Convert.ToUInt32(node.Attributes["regionId"].Value);
                    zone.Id = Convert.ToUInt32(node.Attributes["id"].Value);
                    zone.LocationName = node.Attributes["locationName"].Value;
                    zone.MusicSet = MusicSet.Get(Convert.ToInt32(node.Attributes["musicSetId"].Value));                    
                    zone.MapName = node.Attributes["mapName"].Value;
                    zone.Type = node.Attributes["type"] != null ? (ZoneType)Enum.Parse(typeof(ZoneType), node.Attributes["type"].Value) : ZoneType.Default;
                    zones.Add(zone);
                }
            }

            Log.Instance.Success("Loaded " + zones.Count + " zones.");

            return zones;
        }       

        /// <summary>
        /// Get the contents of a XML file configured as a resource.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetXmlResource()
        {
            //From https://social.msdn.microsoft.com/Forums/vstudio/en-US/6990068d-ddee-41e9-86fc-01527dcd99b5/how-to-embed-xml-file-in-project-resources?forum=csharpgeneral
            string result = string.Empty;
            Stream stream = typeof(ZoneRepository).Assembly.GetManifestResourceStream("Launcher.Resources.xml.ZoneList.xml");
            if (stream != null)
                using (stream)
                using (StreamReader sr = new StreamReader(stream))
                    result = sr.ReadToEnd();

            return result;
        }
    }
}
