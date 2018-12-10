using Launcher.Characters;
using Launcher.packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Servers
{
    class GameServer : Server
    {           
        private User _user;

        public GameServer(World world, User user)
        {
            _user = user;
            Start(world.Name, world.Port);
        }        

        public override void ProcessIncoming(StateObject state)
        {
            //string request = Encoding.ASCII.GetString(state.buffer);


            //File.WriteAllBytes("gameserverconnect.txt", state.buffer);

            //state.workSocket.Send(response);
            //state.workSocket.Disconnect(true);
        }

        private static byte[] PackPage(string file) => new HtmlPacket(file).ToBytes();

        public override void ServerTransition()
        {
            throw new NotImplementedException();
        }
    }
}
