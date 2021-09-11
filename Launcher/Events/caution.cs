using System.Net.Sockets;

namespace PrimalLauncher
{
    public class caution : EventRequest
    {
        public caution(byte[] data) : base(data) { }

        public override void Execute(Socket sender)
        {
            base.Execute(sender);
            Finish(sender);
        }
    }
}
