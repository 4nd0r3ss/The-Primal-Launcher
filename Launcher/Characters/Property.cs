using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class Property
    {
        public byte[] _buffer;
        private MemoryStream _ms;
        private BinaryWriter _bw;
        private ushort _index;
        private bool _writeMore = true;

        private Socket _sender;
        private uint _actorId;
        
        private readonly byte _maxBytes = 0x7d;

        public byte[] Target { get; set; } = Encoding.ASCII.GetBytes(@"/_init");

        public Property(Socket sender, uint actorId)
        {
            _sender = sender;
            _actorId = actorId;

            InitWrite();
        }

        private void InitWrite()
        {
            _buffer = new byte[0x88];
            _ms = new MemoryStream(_buffer);
            _bw = new BinaryWriter(_ms);
            _bw.Seek(1, SeekOrigin.Begin);
            _index = 0;           
        }

        public void Add(string id, object value)
        {   
            
            uint hashedId = MurmurHash2(id, 0);     

            if(value is bool)
            {
                if (_index + 6 + 1 + Target.Length <= _maxBytes) //if there is space to write... | +1 = wrapper byte
                {
                    try
                    {
                        byte val = (byte)((bool)value ? 1 : 0);

                        _bw.Write((byte)1);
                        _bw.Write(hashedId);
                        _bw.Write(val);
                        _index += 6; //sum of all the type sizes.
                    }catch (Exception e) { throw e; }
            }
                else
                {
                    SendPacket(id, value);
                }
            }
            else if(value is byte || value is sbyte)
            {
                if (_index + 6 + 1 + Target.Length <= _maxBytes) 
                {
                    try
                    {
                        _bw.Write((byte)1);
                        _bw.Write(hashedId);
                        _bw.Write((byte)value);
                        _index += 6;
                    }
                    catch (Exception e) { throw e; }
                }
                else
                {
                    SendPacket(id, value);
                }
            }
            else if(value is ushort || value is short)
            {
                if (_index + 7 + 1 + Target.Length <= _maxBytes)
                {
                    try
                    {
                        _bw.Write((byte)2);
                        _bw.Write(hashedId);
                        if (value is ushort)
                            _bw.Write((ushort)value);
                        else
                            _bw.Write((short)value);
                        _index += 7;
                    }
                    catch (Exception e) { throw e; }
                }
                else
                {
                    SendPacket(id, value);
                }
            }           
            else if(value is uint || value is int)
            {
                if (_index + 9 + 1 + Target.Length <= _maxBytes)
                {
                    try
                    {
                        _bw.Write((byte)4);
                        _bw.Write(hashedId);
                        if (value is int)
                            _bw.Write((int)value);
                        else
                            _bw.Write((uint)value);
                        _index += 9;
                    }
                    catch (Exception e) { throw e; }
                }
                else
                {
                    SendPacket(id, value);
                }
            }
            else if(value is float)
            {
                byte[] val = BitConverter.GetBytes((float)value);

                if (_index + 5 + 1 + val.Length + Target.Length <= _maxBytes)
                {
                    try
                    {
                        _bw.Write((byte)(val.Length));
                        _bw.Write(hashedId);
                        _bw.Write(val);
                        _index += (ushort)(5 + val.Length);
                    }
                    catch (Exception e) { throw e; } 
                }
                else
                {
                    SendPacket(id, value);
                }
            }            
        }

        public void FinishWriting()
        {
            _writeMore = false;
            SendPacket();
        }

        private void SendPacket(string id = null, object value = null)
        {
            _bw.Write((byte)(_writeMore ? (0x60 + Target.Length) : (0x82 + Target.Length))); //write wrap byte
            _index += 1;            

            _bw.Write(Target);
            _index += (ushort)Target.Length;

            _bw.Seek(0, SeekOrigin.Begin);
            _bw.Write((byte)_index);            

            GamePacket gamePacket = new GamePacket
            {
                Opcode = 0x137,
                Data = _buffer
            };

            _bw.Dispose();
            _ms.Dispose();

            Packet packet = new Packet(new SubPacket(gamePacket) { SourceId = _actorId, TargetId = _actorId});
            _sender.Send(packet.ToBytes());

            if (_writeMore)
            {
                InitWrite();
                Add(id, value);
            }           
        }

        /// <summary>
        /// This is a generic murmurhash2 function. I copied this version from IonCannon's project, so the credit goes to him.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static uint MurmurHash2(string key, uint seed)
        {
            var data = Encoding.ASCII.GetBytes(key);
            const uint m = 0x5bd1e995;
            const int r = 24;
            var len = key.Length;
            var dataIndex = len - 4;
            var h = seed ^ (uint)len;

            while (len >= 4)
            {
                h *= m;
                var k = (uint)BitConverter.ToInt32(data, dataIndex);
                k = ((k >> 24) & 0xff) | ((k << 8) & 0xff0000) | ((k >> 8) & 0xff00) | ((k << 24) & 0xff000000);
                k *= m;
                k ^= k >> r;
                k *= m;
                h ^= k;
                dataIndex -= 4;
                len -= 4;
            }

            switch (len)
            {
                case 3:
                    h ^= (uint)data[0] << 16;
                    goto case 2;
                case 2:
                    h ^= (uint)data[len - 2] << 8;
                    goto case 1;
                case 1:
                    h ^= data[len - 1];
                    h *= m;
                    break;
            };

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return h;
        }
    }
}
