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
    public partial class MainWindow : SurfaceWindow
    {
        IntelCameraPipeline intelCamera;

        private void SurfaceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Setup Camera & Gesture
            intelCamera = new IntelCameraPipeline(this.Dispatcher); 
            
            //Start Camera
            //TODO ... gesture detection is not working when depth is not found ... fix this later
            intelCamera.OnDepthDetected += intelCamera_OnDepthDetected;
            intelCamera.Start();
        }
       
        public void intelCamera_SpeechDetected(object sender, SpeechEventArgs e)
        {
            Console.WriteLine(e.Sentence);
            string nameUpperCase = e.Sentence.ToUpper();
            if (nameUpperCase.Contains("FISH"))
            {
                intelCamera.PauseVoiceRecognition(true);
                loginControl.SetupLoginControl(intelCamera);
                loginControl.OnLoginCompleteSuccessfully += loginControl_OnLoginCompleteSuccessfully;
            }
            else
            {
               loginControl.Speak("Name incorrect");
            }
        }

        void intelCamera_OnDepthDetected(object sender, DepthEventArgs e)
        {
            Console.WriteLine("{0}", e.Depth);
        }

        private void SurfaceWindow_Closed(object sender, EventArgs e)
        {
            intelCamera.Stop();
        }

        private void LoginControl_Loaded(object sender, RoutedEventArgs e)
        {
            intelCamera.EnableVoiceRecognition();
            loginControl.Speak("Please say your name.");
            intelCamera.OnSpeechDetected += intelCamera_SpeechDetected;
        }

        private void loginControl_OnLoginCompleteSuccessfully(object sender, EventArgs e)
        {
            //Remove login controller & turn off events
            loginControl.ShutdownLoginController(intelCamera); 
            MainGridView.Children.Remove(loginControl);

            ImageHighlighter imgHighlighter = new ImageHighlighter();
            imgHighlighter.SetupImageHighlighter(intelCamera);
            MainGridView.Children.Add(imgHighlighter);
        }

        #region Generated code

        public MainWindow()
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