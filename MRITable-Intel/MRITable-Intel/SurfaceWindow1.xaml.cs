using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

using IntelDepth;
using System.ComponentModel;

using GroupLab.iNetwork;
using GroupLab.iNetwork.Tcp;
using System.Windows.Threading;



namespace MRITable_Intel
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class SurfaceWindow1 : SurfaceWindow
    {
        const double SIZE_OF_TABLE = 500.0;
        const int NUM_OF_SLICES = 100;
        IntelCameraPipeline pub;

        BackgroundWorker bw = new BackgroundWorker(); 

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SurfaceWindow1()
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            bw.DoWork += new DoWorkEventHandler(bw_DoWork);

        }

        public void ShowSlice(int slicenumber)
        {
            this.ImageHighlighter.showSlice(slicenumber);
        }


        /// <summary>
        ///  To compute the slice number which corresponds to the distance provided we must consider (a) the size of the table and (b) the number of slices 
        ///  which we have to display. The calculation is to determine what percentage of distance we are into the table (distance/SIZE_OF_TABLE). Then we multiply this 
        ///  by the number of slices %INTO * NUM_OF_SLICES. 
        /// </summary>
        /// <example>
        /// If the sensor reports a distance of 200 mm and the table is 890 mm then we are 0.224719... through the table. If we have 100 slices then we want to display slice 22.
        /// </example>
        /// <param name="distance"> The distance provided by the sensor in mm. Assumes the distance is from the Bezel. </param>
        /// <returns></returns>
        public int ComputeSlice(double distance)
        {
            double percentageIntoTable = (distance / SIZE_OF_TABLE);
            int chosenSlice = (int)(percentageIntoTable * NUM_OF_SLICES);

            return chosenSlice; 
        }

        #region iNetwork stuff

        private Server _server;
        private List<Connection> _clients;

        public void InitializeServer()
        {
            this._clients = new List<Connection>();

            // Create a new server, add name and port number
            this._server = new Server("MRIViz", 12345);
            this._server.IsDiscoverable = true;
            this._server.Connection += new ConnectionEventHandler(OnServerConnection);

            this._server.Start();
        }


        private void OnServerConnection(object sender, ConnectionEventArgs e)
        {

            if (e.ConnectionEvent == ConnectionEvents.Connect)
            {
                // new client connected
                lock (this._clients)
                {
                    if (!(this._clients.Contains(e.Connection)))
                    {
                        this._clients.Add(e.Connection);
                        e.Connection.MessageReceived += new ConnectionMessageEventHandler(OnMessageReceived);

                        this.Dispatcher.Invoke(
                            new Action(
                                delegate()
                                {
                                    //this.clientList.Items.Add(e.Connection.ToString());
                                }));
                    }
                }
            }
            else if (e.ConnectionEvent == ConnectionEvents.Disconnect)
            {
                // client disconnected
                lock (this._clients)
                {
                    if (this._clients.Contains(e.Connection))
                    {
                        this._clients.Remove(e.Connection);
                        e.Connection.MessageReceived -= new ConnectionMessageEventHandler(OnMessageReceived);

                        this.Dispatcher.Invoke(
                            new Action(
                                delegate()
                                {
                                    //this.clientList.Items.Remove(e.Connection.ToString());
                                }));
                    }
                }
            }
        }

        private void OnMessageReceived(object sender, Message msg)
        {
            this.Dispatcher.Invoke(
                new Action(
                    delegate()
                    {
                        if (msg != null)
                        {
                            //this.messages.Text += msg.Name + "\n";
                            switch (msg.Name)
                            {
                                default:
                                    Console.Out.WriteLine(msg.Name);
                                    // don't do anything
                                    break;
                                // add cases with the message names
                            }
                        }
                    }));
        }

        void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Ensure that the server terminates when the window is closed.
            if (this._server != null && this._server.IsRunning)
            {
                this._server.Stop();
            }
        }

        #endregion

        #region Generated code

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        #endregion


        /// <summary>
        /// This method is responsible for sending the updated slice method to attached devices using iNetworking of all things. 
        /// </summary>
        /// <param name="sliceNumber">
        /// The slice number corresponding to the slice being displayed. 
        /// </param>
        private void DispatchSlice(int sliceNumber)
        {
            Message msg = new Message("ShowSlice");
            msg.AddField("sliceNumber", sliceNumber);
            this._server.BroadcastMessage(msg);

        }

        private void SurfaceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Setup networking server
            InitializeServer();

            //Setup grid for displaying slices 
            this.ImageHighlighter.SetupGrid(NUM_OF_SLICES);

            //Setup depth camera 
            pub = new IntelCameraPipeline();
            pub.getDepth += pub_getDepth;
            //pub.getHandsEvent += new EventHandler<HandEventArgs>(pub_getHandsEvent);

            //Start depth camera
            bw.RunWorkerAsync();

        }

        void pub_getHandsEvent(object sender, HandEventArgs e)
        {
            Console.WriteLine("LEFT HAND ({0},{1})", e.Hands.LeftHand.positionImage.x, e.Hands.LeftHand.positionImage.y);
            Console.WriteLine("RIGHT HAND ({0},{1})", e.Hands.RightHand.positionImage.x, e.Hands.RightHand.positionImage.y);
        }
        void pub_getDepth(object sender, DepthEventArgs e)
        {
            this.Dispatcher.Invoke(
                new Action(
                    delegate() {

                        int slice = ComputeSlice(e.Depth); //TODO Why doesn't the library just return it as a double?

                        //Update UI of reference body
                        this.ImageHighlighter.showSlice(slice);
                        this.label1.Content = String.Format("{0} mm, {1} slice", e.Depth, slice);

                        //Dispatch slice to attached devices
                        DispatchSlice(slice);


                    }
                )
            );
                

        }

        public delegate void DelegateUpdateDepth(float depth);
        public void UpdateDepth(float depth)
        {
            this.ImageHighlighter.showSlice((int)depth);
        }

        private void SurfaceWindow_Closed(object sender, EventArgs e)
        {
            pub.Stop();
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            pub.Start();
        }
    }
}