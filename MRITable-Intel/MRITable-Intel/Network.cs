using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GroupLab.iNetwork;
using GroupLab.iNetwork.Tcp;

namespace MRITable_Intel
{
    class Network
    {
        private Server server;
        
        public void DispatchSlice(int sliceNumber)
        {
            Message msg = new Message("ShowSlice");
            msg.AddField("sliceNumber", sliceNumber);
            this.server.BroadcastMessage(msg);
        }

        public void StartServer()
        {
            // Create a new server, add name and port number
            this.server = new Server("MRIViz", 12345);
            this.server.IsDiscoverable = true;

            this.server.Start();
        }


        public void StopServer()
        {
            this.server.Stop(); 

        }
    }
}
