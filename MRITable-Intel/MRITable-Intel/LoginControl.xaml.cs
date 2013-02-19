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
using System.Speech.Synthesis;

using IntelDepth;
using System.Windows.Threading;

namespace MRITable_Intel
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {

        public event EventHandler<EventArgs> OnLoginCompleteSuccessfully;

        public LoginControl()
        {
            InitializeComponent();
        }

        List<String> gesturalPassword = new List<String> {"Thumbs Up", "Thumbs Down", "Flat Hand", "Peace Symbol" };
        int passwordIndex = 0; 

        private void RevealPasswordImage(int index)
        {
            switch (index)
            {
                case 0:
                    ThumbsUpImage.Visibility = System.Windows.Visibility.Visible;
                    break;
                case 1:
                    ThumbsDownImage.Visibility = System.Windows.Visibility.Visible;
                    break;
                case 2:
                    HandsFiveImage.Visibility = System.Windows.Visibility.Visible;
                    break;
                case 3:
                    PeaceImage.Visibility = System.Windows.Visibility.Visible;
                    break;
                default:
                    break;
            }                   
        }

        private void ApplyGestureToPassword(String gesture)
        {
            //Gesture Correct
            if(gesturalPassword.ElementAt(passwordIndex).Equals(gesture)) {
                
                RevealPasswordImage(passwordIndex);

                //Password Complete
                if (passwordIndex == (gesturalPassword.Count - 1))
                {
                    Speak("Password Correct.");
                    OnLoginCompleteSuccessfully(this, new EventArgs());

                    return;
                }
                Speak("Gesture Correct.");
                passwordIndex++;
            }

            //Gesture Incorrect
            else 
            {
                Speak("Gesture Incorrect");
            }
        }

        public void Speak(String stringToSpeak)
        {
            List<String> sentences = stringToSpeak.Split('.').ToList<String>();

            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();
            PromptBuilder builder = new PromptBuilder();
            foreach (String sentence in sentences)
            {
                builder.StartSentence();
                builder.AppendText(sentence);
                builder.EndSentence();
            }

            synthesizer.SpeakAsync(builder);
            
        }

        public void SetupLoginControl(IntelCameraPipeline intelCamera)
        {
            Speak("Welcome Doctor. Please enter your gesture password."); 
            SetupCameraForGestures(intelCamera);
        }

        public void ShutdownLoginController(IntelCameraPipeline camera)
        {
            camera.OnFlatHandGestureDetected -= Gesture_FlatHandGestureDetected;
            camera.OnThumbsDownGestureDetected -= Gesture_OnThumbsDownGestureDetected;
            camera.OnThumbsUpGestureDetected -= Gesture_OnThumbsUpGestureDetected;
            camera.OnPeaceSymbolGestureDetected -= Gesture_OnPeaceSymbolGestureDetected;
        }
        #region Gesture Methods
        private void SetupCameraForGestures(IntelCameraPipeline camera)
        {
            camera.OnFlatHandGestureDetected += Gesture_FlatHandGestureDetected;
            camera.OnThumbsDownGestureDetected += Gesture_OnThumbsDownGestureDetected;
            camera.OnThumbsUpGestureDetected += Gesture_OnThumbsUpGestureDetected;
            camera.OnPeaceSymbolGestureDetected += Gesture_OnPeaceSymbolGestureDetected;
        }

        private void ProcessGesture(String gesture)
        {
            Console.WriteLine("{0} Gesture Detected", gesture);
            ApplyGestureToPassword(gesture);
        }

        private void Gesture_OnPeaceSymbolGestureDetected(object sender, GestureEventArgs e)
        {
            ProcessGesture("Peace Symbol"); 
        }

        private void Gesture_OnThumbsDownGestureDetected(object sender, GestureEventArgs e)
        {
            ProcessGesture("Thumbs Down");
        }

        private void Gesture_OnThumbsUpGestureDetected(object sender, GestureEventArgs e)
        {
            ProcessGesture("Thumbs Up"); 
        }

        private void Gesture_FlatHandGestureDetected(object sender, GestureEventArgs e)
        {
            ProcessGesture("Flat Hand"); 
        }

        #endregion
    }
}
