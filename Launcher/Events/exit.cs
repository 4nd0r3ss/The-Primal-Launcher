using System.Net.Sockets;

namespace PrimalLauncher
{
    public class exit : EventRequest
    {
        public exit(byte[] data) : base(data) { }

        public override void Execute(Socket sender)
        {
            base.Execute(sender);
            Finish(sender);
        }
    }
}
