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
        #region Networking Code Setup 

        private Server server;
        private List<Connection> clients;
        public event EventHandler<EventArgs> OnPauseScanningEventRecieved;
        public event EventHandler<EventArgs> OnContinueScanningEventRecieved;



        public void StartServer()
        {
            this.clients = new List<Connection>();
            // Create a new server, add name and port number
            this.server = new Server("MRIViz", 12345);
            this.server.IsDiscoverable = true;
            this.server.Connection += new ConnectionEventHandler(OnServerConnection);

            this.server.Start();
        }

        public void StopServer()
        {
            if (this.server != null && this.server.IsRunning)
            {
                this.server.Stop();
            }
        }

        #endregion
        
        // Handles the event that a message is received
        private void OnConnectionMessage(object sender, Message msg)
        {
            if (msg != null)
            {
                // Check message name...
                switch (msg.Name)
                {
                    case "PauseScan":
                        Console.Out.WriteLine("Scanning Paused");
                        this.server.BroadcastMessage(msg);
                        OnPauseScanningEventRecieved(this, new EventArgs());
                        break;
                    case "ContinueScan":
                        Console.Out.WriteLine("Scanning Continued");
                        this.server.BroadcastMessage(msg);
                        OnContinueScanningEventRecieved(this, new EventArgs());
                        break;
                    default:
                        //Do nothing
                        break;
                }
            }
        }

        // Handles who connected to server
        private void OnServerConnection(object sender, ConnectionEventArgs e)
        {
            if (e.ConnectionEvent == ConnectionEvents.Connect)
            {
                // Lock the list so that only the networking thread affects it
                lock (this.clients)
                {
                    if (!(this.clients.Contains(e.Connection)))
                    {
                        // Add to list and create event listener
                        this.clients.Add(e.Connection);
                        e.Connection.MessageReceived += new ConnectionMessageEventHandler(OnConnectionMessage);
                    }
                }
            }

            else if (e.ConnectionEvent == ConnectionEvents.Disconnect)
            {
                // Lock the list so that only the networking thread affects it
                lock (this.clients)
                {
                    if (this.clients.Contains(e.Connection))
                    {
                        // Clean up -- remove from list and remove event listener
                        this.clients.Remove(e.Connection);
                        e.Connection.MessageReceived -= new ConnectionMessageEventHandler(OnConnectionMessage);
                    }
                }
            }
        }
        
        public void DispatchSlice(int sliceNumber)
        {
            Message msg = new Message("ShowSlice");
            msg.AddField("sliceNumber", sliceNumber);
            this.server.BroadcastMessage(msg);
        }



    }
}
