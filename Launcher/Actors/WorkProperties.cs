using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace PrimalLauncher
{
    public class WorkProperties
    {
        public byte[] _buffer;
        private MemoryStream _ms;
        private BinaryWriter _bw;
        private ushort _index;
        private bool _writeMore = true;

        private Socket _sender;
        private uint _actorId;
        
        private readonly byte _maxBytes = 0x7d;

        public byte[] Command { get; set; }
        public Queue<byte[]> PacketQueue { get; set; }

        public WorkProperties(Socket sender, uint actorId, string command)
        {
            _sender = sender;
            _actorId = actorId;
            PacketQueue = new Queue<byte[]>();

            Command = Encoding.ASCII.GetBytes(command);

            InitWrite(1);
        }

        private void InitWrite(int startPosition)
        {
            _buffer = new byte[0x88];
            _ms = new MemoryStream(_buffer);
            _bw = new BinaryWriter(_ms);
            _bw.Seek(startPosition, SeekOrigin.Begin);
            _index = 0;           
        }

        public void Add(object key, object value, bool isLastItem = false)
        {
            uint hashedId = 0;

            //This is for writing unknown work values from packet traces while I don't get a working reverse murmur function. 
            //if key is a int we just bypass the murmur hashing.
            if (key is string)
                hashedId = MurmurHash2((string)key, 0);
            else
                hashedId = (uint)key;

            if (value is bool)
            {
                if (_index + 6 + 1 + Command.Length <= _maxBytes) //if there is space to write... | +1 = wrapper byte
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
                    SendPacket((string)key, value);
                }
            }
            else if(value is byte || value is sbyte)
            {
                if (_index + 6 + 1 + Command.Length <= _maxBytes) 
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
                    SendPacket((string)key, value);
                }
            }
            else if(value is ushort || value is short)
            {
                if (_index + 7 + 1 + Command.Length <= _maxBytes)
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
                    SendPacket((string)key, value);
                }
            }           
            else if(value is uint || value is int)
            {
                if (_index + 9 + 1 + Command.Length <= _maxBytes)
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
                    SendPacket((string)key, value);
                }
            }
            else if(value is float)
            {
                byte[] val = BitConverter.GetBytes((float)value);

                if (_index + 5 + 1 + val.Length + Command.Length <= _maxBytes)
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
                    SendPacket((string)key, value);
                }
            }  
            else if (value is Queue<short> buffer)
            {                
                int numBytesToWriteInitial = 0x73 - (1 + 1 + 4 + 2 + Command.Length);
                int numBytesToWriteFromBuffer = numBytesToWriteInitial; //size - arr start - hashkey - arr end - trailing - maxbytes [how many bytes from the buffer we can write now]
                int numBytesInBuffer = buffer.Count * sizeof(short); //note: number of bytes in buffer != array length  
                int pageCount = 0;

                numBytesToWriteFromBuffer = numBytesToWriteFromBuffer % 2 > 0 ? numBytesToWriteFromBuffer - 1 : numBytesToWriteFromBuffer;
                numBytesToWriteFromBuffer = numBytesInBuffer < numBytesToWriteFromBuffer ? numBytesInBuffer : numBytesToWriteFromBuffer;

                short wrapper = (short)(isLastItem ? 0x8f03 : 0xb103);

                while (buffer.Count > 0)
                {
                    InitWrite(0);

                    byte totalSize = (byte)(1 + 4 + 2 + Command.Length + numBytesToWriteFromBuffer);
                    _bw.Write((byte)totalSize); //total size
                    _bw.Write((byte)(pageCount == 0 ? 0x5f : 0x0b)); //array start/continue                   
                    _bw.Write(hashedId); //hashed key                  

                    for (int i = 0; i < (numBytesToWriteFromBuffer / 2); i++)
                        _bw.Write(buffer.Dequeue());

                    _bw.Write((short)(pageCount == 0 ? 0xb101 : wrapper)); //array wrap/continue
                    _bw.Write(Command);

                    pageCount++;

                    //enqueue this packet    
                    PacketQueue.Enqueue(new Packet(new GamePacket { Opcode = 0x137, Data = _buffer }).ToBytes());
                    //_sender.Send(new Packet(new GamePacket { Opcode = 0x137, Data = _buffer }).ToBytes());

                    //debug
                    //File.WriteAllBytes("levelTest_" + hashedId.ToString("X") + "_" + pageCount + ".txt", _buffer);

                    //reset stream
                    _bw.Dispose();
                    _ms.Dispose();

                    //recalculate bytes to write                   
                    numBytesInBuffer = buffer.Count * sizeof(short); //note: number of bytes in buffer != array length  
                    numBytesToWriteFromBuffer = numBytesToWriteInitial % 2 > 0 ? numBytesToWriteInitial - 1 : numBytesToWriteInitial;
                    numBytesToWriteFromBuffer = numBytesInBuffer < numBytesToWriteFromBuffer ? numBytesInBuffer : numBytesToWriteFromBuffer;
                }
            }
        }

        public void FinishWriting(uint actorId = 0)
        {
            _writeMore = false;
            SendPacket(actorId: actorId);
        }

        public void SendUpdate()
        {
            _writeMore = false;
            _bw.Write((byte) 0x94);
            _index += 1;
            SendPacket(wrapByte: false);
        }

        private void SendPacket(string key = null, object value = null, bool wrapByte = true, uint actorId = 0)
        {
            if (wrapByte)
            {
                _bw.Write((byte)(_writeMore ? (0x60 + Command.Length) : (0x82 + Command.Length))); //write wrap byte
                _index += 1;
            }                       

            _bw.Write(Command);
            _index += (ushort)Command.Length;

            _bw.Seek(0, SeekOrigin.Begin);
            _bw.Write((byte)_index);


            Packet.Send(_sender, ServerOpcode.ActorInit, _buffer, actorId);
            //_sender.Send(new Packet(new GamePacket { Opcode = (ushort)ServerOpcode.ActorInit, Data = _buffer }).ToBytes());

            _bw.Dispose();
            _ms.Dispose();

            if (_writeMore)
            {
                InitWrite(1);
                Add(key, value);
            }           
        }

        /// <summary>
        /// This is a generic murmurhash2 function. I've copied this version from Meteor project, the credit goes to them.
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
