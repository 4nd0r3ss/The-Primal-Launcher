using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace PrimalLauncher
{
    public class EventManager
    {
        private static EventManager _instance = null;
        private static readonly object _padlock = new object();
        public EventRequest CurrentEvent { get; set; } = null;

        public static EventManager Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new EventManager();

                    return _instance;
                }
            }
        }

        private EventManager() { }
             
        public void ProcessIncoming(Socket sender, byte[] data)
        {
            try
            {
                string eventName = GetEventType(data);
                Type type = Type.GetType(eventName);
                CurrentEvent = (EventRequest)Activator.CreateInstance(type, data);
                CurrentEvent.Execute(sender);
            }catch(Exception e)
            {
                Log.Instance.Error(e.Message);
                File.WriteAllBytes("test.txt", data);
                throw e;
            }                             
        }    
        
        private string GetEventType(byte[] data)
        {
            byte nameSize;

            for (nameSize = 1; nameSize < 0x20; nameSize++)
                if (data[0x20 + nameSize] == 0) break;

            byte[] nameBytes = new byte[nameSize - 1];
            Array.Copy(data, 0x21, nameBytes, 0, nameSize - 1);
           return "PrimalLauncher." + Encoding.ASCII.GetString(nameBytes);
        }
    }

    
      
    

    

    

    

    

    

    

    
}
