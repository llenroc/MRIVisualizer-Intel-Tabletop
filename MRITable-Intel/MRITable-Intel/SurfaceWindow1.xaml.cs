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
        IntelCameraPipeline intelCamera;

        private void SurfaceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Setup Camera & Gesture
            intelCamera = new IntelCameraPipeline();
            SetupCameraForGestures(intelCamera);
            
            //Start Camera
            intelCamera.Start();
        }

        private void SurfaceWindow_Closed(object sender, EventArgs e)
        {
            intelCamera.Stop();
        }



        #region Gesture Setup & Callback Methods
        private void SetupCameraForGestures(IntelCameraPipeline camera)
        {
            camera.OnFlatHandGestureDetected += Gesture_FlatHandGestureDetected;
            camera.OnThumbsDownGestureDetected += Gesture_OnThumbsDownGestureDetected;
            camera.OnThumbsUpGestureDetected += Gesture_OnThumbsUpGestureDetected;
            camera.OnPeaceSymbolGestureDetected += Gesture_OnPeaceSymbolGestureDetected;
        }

        private void Gesture_OnPeaceSymbolGestureDetected(object sender, GestureEventArgs e)
        {
            Console.WriteLine("Peace Symbol Gesture Detected");
        }

        private void Gesture_OnThumbsDownGestureDetected(object sender, GestureEventArgs e)
        {
            Console.WriteLine("Thumbs Down Gesture Detected");
        }

        private void Gesture_OnThumbsUpGestureDetected(object sender, GestureEventArgs e)
        {
            Console.WriteLine("Thumbs Up Gesture Detected");
        }

        private void Gesture_FlatHandGestureDetected(object sender, GestureEventArgs e)
        {
            Console.WriteLine("Flat Hand Gesture Detected");

        }

        #endregion

        #region Generated code

        public SurfaceWindow1()
        {
            InitializeComponent();
            AddWindowAvailabilityHandlers();
        }
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





    }
}