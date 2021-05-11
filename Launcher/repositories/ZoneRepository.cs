using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public static class ZoneRepository
    {
        public static List<Zone> GetZones()
        {
            return new List<Zone>
            {
                //Convert booleans to bitfield?
                new Zone(101, 128, locationNameId: 0, classNameId: 0, musicSetId: 1, mapName: "sea0Field01"),
                new Zone(101, 129, locationNameId: 1, classNameId: 0, musicSetId: 1, mapName: "sea0Field02"),
                new Zone(101, 130, locationNameId: 2, classNameId: 0, musicSetId: 1, mapName: "sea0Field03"),
                new Zone(101, 131, locationNameId: 3, classNameId: 0, musicSetId: 0, mapName: "sea0Dungeon01"),
                new Zone(101, 132, locationNameId: 4, classNameId: 0, musicSetId: 0, mapName: "sea0Dungeon03"),
                new Zone(101, 133, locationNameId: 5, classNameId: 0, musicSetId: 2, mapName: "sea0Town01"),
                new Zone(101, 137, locationNameId: 6, classNameId: 0, musicSetId: 0, mapName: "sea0Dungeon06"),
                //new Zone(101, 138, locationNameId: 7, musicSetId: 1),
                //new Zone(101, 140, locationNameId: 8, musicSetId: 0),
                new Zone(101, 141, locationNameId: 0, classNameId: 0, musicSetId: 1, mapName: "sea0Field01a"),
                new Zone(101, 135, locationNameId: 9, classNameId: 0, musicSetId: 1, mapName: "sea0Field04"),
                //new Zone(101, 204, locationNameId: 1, musicSetId: 1, mapName: "sea0Field02a"),
                //new Zone(101, 205, locationNameId: 2, musicSetId: 1, mapName: "sea0Field03a"),
                new Zone(101, 230, locationNameId: 5, classNameId: 0, musicSetId: 2, mapName: "sea0Town01a"),
                //new Zone(101, 235, locationNameId: 10, musicSetId: 0),
                new Zone(101, 236, locationNameId: 11, classNameId: 18, musicSetId: 0, mapName: "sea1Field01"),
                //new Zone(101, 237, locationNameId: 12, musicSetId: 0),
                //new Zone(101, 248, locationNameId: 1, musicSetId: 0),
                //new Zone(101, 260, locationNameId: 1, musicSetId: 0),
                //new Zone(101, 261, locationNameId: 1, musicSetId: 0),
                //new Zone(101, 269, locationNameId: 11, musicSetId: 0),
                //new Zone(101, 270, locationNameId: 12, musicSetId: 0),
                new Zone(102, 143, locationNameId: 13, classNameId: 2, musicSetId: 3, mapName: "roc0Field01"),
                new Zone(102, 144, locationNameId: 14, classNameId: 2, musicSetId: 3, mapName: "roc0Field02"),
                new Zone(102, 145, locationNameId: 15, classNameId: 2, musicSetId: 3, mapName: "roc0Field03"),
                //new Zone(102, 146, locationNameId: 16, musicSetId: 3),
                new Zone(102, 147, locationNameId: 17, classNameId: 2, musicSetId: 3, mapName: "roc0Field04"),
                new Zone(102, 148, locationNameId: 18, classNameId: 2, musicSetId: 3, mapName: "roc0Field05"),
                new Zone(102, 231, locationNameId: 19, classNameId: 2, musicSetId: 0, mapName: "roc0Dungeon01"),
                new Zone(102, 239, locationNameId: 20, classNameId: 2, musicSetId: 0, mapName: "roc0Field02a"),
                new Zone(102, 245, locationNameId: 21, classNameId: 2, musicSetId: 0, mapName: "roc0Dungeon04"),
                new Zone(102, 250, locationNameId: 20, classNameId: 2, musicSetId: 0, mapName: "roc0Field02a"),
                new Zone(102, 252, locationNameId: 21, classNameId: 2, musicSetId: 0, mapName: "roc0Dungeon04"),
                new Zone(102, 253, locationNameId: 21, classNameId: 2, musicSetId: 0, mapName: "roc0Dungeon04"),
                new Zone(102, 256, locationNameId: 20, classNameId: 2, musicSetId: 0, mapName: "roc0Field02a"),

                new Zone(103, 150, locationNameId: 22, classNameId: 3, musicSetId: 4, mapName: "fst0Field01"),
                new Zone(103, 151, locationNameId: 23, classNameId: 3, musicSetId: 4, mapName: "fst0Field02"),
                new Zone(103, 152, locationNameId: 24, classNameId: 3, musicSetId: 4, mapName: "fst0Field03"),
                new Zone(103, 153, locationNameId: 25, classNameId: 3, musicSetId: 4, mapName: "fst0Field04"),
                new Zone(103, 154, locationNameId: 26, classNameId: 3, musicSetId: 4, mapName: "fst0Field05"),
                new Zone(103, 155, locationNameId: 27, classNameId: 3, musicSetId: 5, mapName: "fst0Town01"),
                //new Zone(103, 156, locationNameId: 28, musicSetId: 0),
                new Zone(103, 157, locationNameId: 29, classNameId: 3, musicSetId: 0, mapName: "fst0Dungeon01"),
                new Zone(103, 158, locationNameId: 30, classNameId: 3, musicSetId: 0, mapName: "fst0Dungeon02"),
                new Zone(103, 159, locationNameId: 31, classNameId: 3, musicSetId: 0, mapName: "fst0Dungeon03"),
                //new Zone(103, 161, locationNameId: 32, musicSetId: 0),
                new Zone(103, 162, locationNameId: 22, classNameId: 3, musicSetId: 4, mapName: "fst0Field01a"),
                new Zone(103, 206, locationNameId: 27, classNameId: 3, musicSetId: 16, mapName: "fst0Town01a"),
                new Zone(103, 207, locationNameId: 24, classNameId: 3, musicSetId: 4, mapName: "fst0Field03a"),
                new Zone(103, 208, locationNameId: 26, classNameId: 3, musicSetId: 4, mapName: "fst0Field05a"),
                //new Zone(103, 238, locationNameId: 33, musicSetId: 0, mapName: "fst0Field04"),
                //new Zone(103, 247, locationNameId: 24, musicSetId: 0),
                //new Zone(103, 258, locationNameId: 24, musicSetId: 0),
                //new Zone(103, 259, locationNameId: 24, musicSetId: 0),
                new Zone(104, 170, locationNameId: 34, classNameId: 6, musicSetId: 7, mapName: "wil0Field01"),
                new Zone(104, 171, locationNameId: 35, classNameId: 6, musicSetId: 7, mapName: "wil0Field02"),
                new Zone(104, 172, locationNameId: 36, classNameId: 6, musicSetId: 7, mapName: "wil0Field03"),
                new Zone(104, 173, locationNameId: 37, classNameId: 6, musicSetId: 7, mapName: "wil0Field04"),
                new Zone(104, 174, locationNameId: 38, classNameId: 6, musicSetId: 7, mapName: "wil0Field05"),
                new Zone(104, 175, locationNameId: 39, classNameId: 6, musicSetId: 11, mapName: "wil0Town01"),
                new Zone(104, 176, locationNameId: 40, classNameId: 6, musicSetId: 0, mapName: "wil0Dungeon02"),
                new Zone(104, 178, locationNameId: 41, classNameId: 6, musicSetId: 0, mapName: "wil0Dungeon04"),
                //new Zone(104, 179, locationNameId: 42, musicSetId: 0),
                //new Zone(104, 181, locationNameId: 43, musicSetId: 0),
                //new Zone(104, 182, locationNameId: 34, musicSetId: 0),
                new Zone(104, 186, locationNameId: 39, classNameId: 8, musicSetId: 0, mapName: "wil0Battle02"),
                new Zone(104, 187, locationNameId: 39, classNameId: 8, musicSetId: 0, mapName: "wil0Battle03"),
                new Zone(104, 188, locationNameId: 39, classNameId: 8, musicSetId: 0, mapName: "wil0Battle04"),
                new Zone(104, 209, locationNameId: 39, classNameId: 5, musicSetId: 11, mapName: "wil0Town01a"),
                //new Zone(104, 210, locationNameId: 35, musicSetId: 7),
                //new Zone(104, 211, locationNameId: 36, musicSetId: 7),
                //new Zone(104, 240, locationNameId: 44, musicSetId: 0, mapName: "wil0Field05a"),
                //new Zone(104, 246, locationNameId: 45, musicSetId: 0),
                //new Zone(104, 249, locationNameId: 35, musicSetId: 0),
                //new Zone(104, 254, locationNameId: 45, musicSetId: 0),
                //new Zone(104, 255, locationNameId: 45, musicSetId: 0),
                //new Zone(104, 262, locationNameId: 35, musicSetId: 0),
                //new Zone(104, 263, locationNameId: 35, musicSetId: 0),
                //new Zone(104, 265, locationNameId: 44, musicSetId: 0),
                new Zone(105, 190, locationNameId: 46, classNameId: 9, musicSetId: 8, mapName: "lak0Field01"),
                //new Zone(105, 251, locationNameId: 47, musicSetId: 0),
                //new Zone(105, 264, locationNameId: 47, musicSetId: 0, mapName: "lak0Field01"),
                new Zone(105, 266, locationNameId: 46, classNameId: 9, musicSetId: 8, mapName: "lak0Field01a"),

                new Zone(106, 164, locationNameId: 22, classNameId: 5, musicSetId: 6, mapName: "fst0Battle01"),
                new Zone(106, 165, locationNameId: 22, classNameId: 5, musicSetId: 6, mapName: "fst0Battle02"),
                new Zone(106, 166, locationNameId: 22, classNameId: 5, musicSetId: 6, mapName: "fst0Battle03"),
                new Zone(106, 167, locationNameId: 22, classNameId: 5, musicSetId: 6, mapName: "fst0Battle04"),
                new Zone(106, 168, locationNameId: 22, classNameId: 5, musicSetId: 6, mapName: "fst0Battle05"),
                new Zone(107, 184, locationNameId: 39, classNameId: 8, musicSetId: 0, mapName: "wil0Battle01"),
                new Zone(107, 185, locationNameId: 39, classNameId: 8, musicSetId: 0, mapName: "wil0Battle01"),
                new Zone(109, 257, locationNameId: 48, classNameId: 20, musicSetId: 0, mapName: "roc1Field01"),
                new Zone(109, 267, locationNameId: 48, classNameId: 20, musicSetId: 0, mapName: "roc1Field02"),
                new Zone(109, 268, locationNameId: 48, classNameId: 20, musicSetId: 0, mapName: "roc1Field03"),
                new Zone(111, 193, locationNameId: 49, classNameId: 12, musicSetId: 9, mapName: "ocn0Battle02"),
                new Zone(112, 139, locationNameId: 50, classNameId: 0, musicSetId: 0, mapName: "sea0Field01a"),
                new Zone(112, 192, locationNameId: 49, classNameId: 10, musicSetId: 0, mapName: "ocn1Battle01"),
                new Zone(112, 194, locationNameId: 49, classNameId: 10, musicSetId: 0, mapName: "ocn1Battle03"),
                new Zone(112, 195, locationNameId: 49, classNameId: 10, musicSetId: 0, mapName: "ocn1Battle04"),
                new Zone(112, 196, locationNameId: 49, classNameId: 10, musicSetId: 0, mapName: "ocn1Battle05"),
                new Zone(112, 198, locationNameId: 49, classNameId: 10, musicSetId: 0, mapName: "ocn1Battle06"),
                new Zone(202, 134, locationNameId: 51, classNameId: 1, musicSetId: 0, mapName: "sea0Market01"),
                new Zone(202, 232, locationNameId: 52, classNameId: 15, musicSetId: 13, mapName: "sea0Office01"),
                new Zone(204, 234, locationNameId: 53, classNameId: 17, musicSetId: 12, mapName: "fst0Office01"),
                new Zone(204, 160, locationNameId: 51, classNameId: 4, musicSetId: 0, mapName: "fst0Market01"),
                new Zone(205, 233, locationNameId: 54, classNameId: 16, musicSetId: 14, mapName: "wil0Office01"),
                new Zone(205, 180, locationNameId: 51, classNameId: 7, musicSetId: 0, mapName: "wil0Market01"),
                new Zone(207, 177, locationNameId: 57, classNameId: 11, musicSetId: 1, mapName: "_jail"),
                new Zone(208, 201, locationNameId: 58, classNameId: 14, musicSetId: 0, mapName: "prv0Cottage00"),
                new Zone(209, 244, locationNameId: 55, classNameId: 19, musicSetId: 15, mapName: "prv0Inn01"),
                new Zone(805, 200, locationNameId: 56, classNameId: 13, musicSetId: 10, mapName: "sea1Cruise01"),
            };
        }

        /// <summary>
        /// Get the contents of a XML file configured as a resource.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetXmlResource(string fileName)
        {
            //From https://social.msdn.microsoft.com/Forums/vstudio/en-US/6990068d-ddee-41e9-86fc-01527dcd99b5/how-to-embed-xml-file-in-project-resources?forum=csharpgeneral
            string result = string.Empty;
            Stream stream = typeof(ZoneRepository).Assembly.GetManifestResourceStream("Launcher.Resources.xml.zones.z" + fileName);
            if (stream != null)
                using (stream)
                using (StreamReader sr = new StreamReader(stream))
                    result = sr.ReadToEnd();

            return result;
        }
    }
}
