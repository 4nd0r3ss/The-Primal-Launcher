using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml;

namespace Launcher
{
    class GameData
    {
        public Dictionary<string, uint> Index { get; set; }
        public string Language { get; set; } = "en"; //default lang is english. Opitons are: ja, en, de, fr, chs.

        private static GameData _instance = null;
        private static readonly object _padlock = new object();

        public static GameData Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new GameData();

                    return _instance;
                }
            }
        }

        private GameData()
        {
            LoadGameDataIndex();
        }

        private FileStream GetFilestream(string file) => new FileStream(GetFilePath(Convert.ToUInt32(file)), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        public DataTable GetGameData(string indexName)
        {
            XmlDocument infoFile = new XmlDocument();
            DataTable dataTable = new DataTable("table");
            DataRow dataRow;
            string file = Encoding.Default.GetString(DecodeFile(GetFilePath(Index[indexName])));

            if (file.IndexOf("<") != 0)
                file = file.Substring(file.IndexOf("<"));

            infoFile.LoadXml(file);
            string langSelector = "";

            if (infoFile.SelectNodes("ssd/sheet").Count > 1) //if there is more than 1 sheet, we need to select a language.
                langSelector = "[@lang = '" + Language + "']";

            var blockNode = infoFile.SelectNodes("ssd/sheet" + langSelector + "/block/file");
            var typeNode = infoFile.SelectNodes("ssd/sheet" + langSelector + "/type/param");

            //add first column for id#
            dataTable.Columns.Add(new DataColumn("id", typeof(uint)));

            //add columns with types to data table
            for (int i = 0; i < typeNode.Count; i++)
                dataTable.Columns.Add(new DataColumn(typeNode[i].InnerText + "c" + i.ToString(), GetNodeType(typeNode[i].InnerText))); //we have no column names yet so we use index as name...

            //each file node
            foreach (XmlNode node in blockNode)
            {
                FileStream enableStream = GetFilestream(node.Attributes["enable"].Value);
                FileStream dataStream = GetFilestream(node.InnerText);
                using (BinaryReader enable = new BinaryReader(enableStream))
                {
                    while (enable.BaseStream.Position != enable.BaseStream.Length)
                    {
                        uint startId = enable.ReadUInt32();
                        uint numIds = enable.ReadUInt32();
                        BinaryReader data = new BinaryReader(dataStream);

                        //each datatable line
                        for (int i = 0; i < numIds; i++)
                        {
                            dataRow = dataTable.NewRow();
                            dataRow["id"] = startId + i; //add item id to first column
                            //each datatable line column
                            for (int j = 0; j < typeNode.Count; j++)
                                AddColumnFromDataStream(ref data, ref dataRow, typeNode[j].InnerText, j);
                            dataTable.Rows.Add(dataRow);
                        }
                    }
                }
            }
            return dataTable;
        }
        /// <summary>
        /// Get the game data index file, decode it and parse the result xml.
        /// </summary>
        private void LoadGameDataIndex()
        {
            XmlDocument xmlDoc = new XmlDocument();
            string file = Encoding.Default.GetString(DecodeFile(GetFilePath(0x01030000)));
            if (file.IndexOf("<") != 0)
                file = file.Substring(file.IndexOf("<"));
            xmlDoc.LoadXml(file);
            var rootNode = xmlDoc.SelectNodes("ssd/sheet");
            Index = new Dictionary<string, uint>();
            foreach (XmlNode node in rootNode)
            {
                Index.Add(node.Attributes["name"].Value, Convert.ToUInt32(node.Attributes["infofile"].Value));
            }
        }
        /// <summary>
        /// Decode a game data file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>A buffer containing the decoded bytes.</returns>
        private byte[] DecodeFile(string filePath)
        {
            byte[] buffer = File.ReadAllBytes(filePath);
            int bufferLength = buffer.Length;
            if (bufferLength == 0 || (buffer[bufferLength - 1] != 0xf1))
                return buffer;
            Array.Resize(ref buffer, bufferLength - 1);
            ScrambleBuffer(ref buffer);
            bufferLength = buffer.Length;
            ushort xA = (ushort)(bufferLength * 7);
            ushort xB = (ushort)((buffer[0x07] << 8 | buffer[0x06]) ^ 0x6c6d);
            DecodeSequence(ref buffer, xA, 0);
            DecodeSequence(ref buffer, xB, 2);
            if (bufferLength % 2 > 0)
                buffer[bufferLength - 1] = (byte)(buffer[bufferLength - 1] ^ (byte)(xA >> 8));
            Array.Resize(ref buffer, bufferLength - 1); //remove the extra weird character at the end to avoid xml parse error
            return buffer;
        }
        private void ScrambleBuffer(ref byte[] buffer)
        {
            int ptr = 0;
            int end = buffer.Length - 1;
            while (ptr < end)
            {
                byte a = buffer[ptr];
                byte b = buffer[end];
                buffer[ptr] = b;
                buffer[end] = a;
                ptr += 2;
                end -= 2;
            }
        }
        private void DecodeSequence(ref byte[] buffer, ushort key, int offset)
        {
            while (offset < buffer.Length - 1)
            {
                ushort val = (ushort)(buffer[offset + 1] << 8 | buffer[offset]);
                ushort calc = (ushort)(val ^ key);
                buffer[offset] = (byte)calc;
                buffer[offset + 1] = (byte)(calc >> 8);
                offset += 4;
            }
        }
        private string GetFilePath(uint fileNumber)
        {
            return Preferences.Instance.Options.GameInstallPath + @"\data\" + ((byte)(fileNumber >> 24)).ToString("X2") + @"\" + ((byte)(fileNumber >> 16)).ToString("X2") + @"\" + ((byte)(fileNumber >> 8)).ToString("X2") + @"\" + ((byte)(fileNumber >> 32)).ToString("X2") + ".dat";
        }
        private Type GetNodeType(string typeText)
        {
            switch (typeText)
            {
                case "str":
                    return typeof(string);
                case "s32":
                    return typeof(int);
                case "s16":
                    return typeof(short);
                case "float":
                    return typeof(float);
                case "u32":
                    return typeof(uint);
                case "u16":
                    return typeof(ushort);
                case "s8":
                case "u8":
                    return typeof(byte);
                case "bool":
                    return typeof(bool);
                default:
                    return null;
            }
        }
        private void AddColumnFromDataStream(ref BinaryReader dataStream, ref DataRow dataRow, string typeText, int index)
        {
            index++; //we want to skip column index 0

            switch (typeText)
            {
                case "s32":
                    int int32 = dataStream.ReadInt32();
                    dataRow[index] = int32;
                    break;
                case "s16":
                    short int16 = dataStream.ReadInt16();
                    dataRow[index] = int16;
                    break;
                case "float":
                    float flt = dataStream.ReadSingle();
                    dataRow[index] = flt;
                    break;
                case "u32":
                    uint uint32 = dataStream.ReadUInt32();
                    dataRow[index] = uint32;
                    break;
                case "u16":
                    ushort uint16 = dataStream.ReadUInt16();
                    dataRow[index] = uint16;
                    break;
                case "s8":
                case "u8":
                    byte int8 = dataStream.ReadByte();
                    dataRow[index] = int8;
                    break;
                case "bool":
                    bool boolean = dataStream.ReadBoolean();
                    dataRow[index] = boolean;
                    break;
                case "str":
                    ushort stringSize = dataStream.ReadUInt16();
                    byte unknown = dataStream.ReadByte(); //the string size includes this unknown byte
                    byte[] stringBytes = dataStream.ReadBytes(stringSize - 1);

                    for (int i = 0; i < stringSize - 1; i++)
                        stringBytes[i] ^= 0x73;

                    dataRow[index] = Encoding.UTF8.GetString(stringBytes);
                    break;
                default:
                    break;
            }
        }
    }
}

