using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading; 

using MathNet.Numerics.Statistics;
using System.ComponentModel;


namespace IntelDepth
{
    //Subclassing UtilMPipeline for IntelDepth
    public class IntelCameraPipeline : UtilMPipeline
    {
        public event EventHandler<DepthEventArgs> OnDepthDetected;
        public event EventHandler<SpeechEventArgs> getSpeechEvent;

        public event EventHandler<GestureEventArgs> OnFlatHandGestureDetected;
        public event EventHandler<GestureEventArgs> OnPeaceSymbolGestureDetected;
        public event EventHandler<GestureEventArgs> OnThumbsDownGestureDetected;
        public event EventHandler<GestureEventArgs> OnThumbsUpGestureDetected;

        public event EventHandler<SpeechEventArgs> OnSpeechDetected; 

        const int ErrorCode = -1;
        const short screenLength = 500;
        public int numFramesToAverage = 5;
        private short distanceFromScreenEdge = 200; // ~20 cm from the edge

        private Dispatcher mainDispatcher; 

        public IntelCameraPipeline(Dispatcher dispatcher): base() 
        {
            mainDispatcher = dispatcher; 
        }

        private BackgroundWorker bw; 


        public void Start()
        {
            bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(Start_OnBackgroundThread);
            bw.RunWorkerAsync();
        }

        private void Start_OnBackgroundThread(object sender, DoWorkEventArgs e)
        {
            //Depth Camera Enabling
            base.EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH);


            //Voice Recognition Enabling
            //base.EnableVoiceRecognition();

            ////Gesture Recognition Enabling
            base.EnableGesture();

            base.Init();
            if (AcquireFrame(true))
                doEvent(new DepthEventArgs(0));
            
        }


        public void Stop()
        {
            base.Close();
            base.Dispose();
        }

        /// <summary>
        /// Calibration Function
        /// </summary>
        /// <param name="depth"></param>
        public void Calibrate(float depth)
        {
            distanceFromScreenEdge = (short)depth;
        }

        /// <summary>
        /// Function that returns what was spoken
        /// </summary>
        /// <param name="data"></param>
        public override void OnRecognized(ref PXCMVoiceRecognition.Recognition data)
        {  
            OnSpeechDetected(this, new SpeechEventArgs(data.dictation));
            //if (data.)
                //getSpeechEvent(this, new SpeechEventArgs(data.dictation));            //base.OnRecognized(ref data);
        }

        /// <summary>
        /// This method is triggered by the Intel Library, it supplies a struct describing the gesture
        /// which is processed into the specific gesture events. 
        /// Supported gestures Flat Hand, Peace Symbol, Thumbs Down, Thumbs Up
        /// </summary>
        /// <param name="gesture"></param>
        public override void OnGesture(ref PXCMGesture.Gesture gesture)
        {
            if (!gesture.active)
                return;

            //We dispatch the event handling to the main thread
            PXCMGesture.Gesture inputGesture = gesture; //Copy the gesture for some reason ... 

            mainDispatcher.Invoke(
                new Action(
                    delegate()
                    {
                        ProcessGestureEvents(inputGesture);
                    }
                )
            );
     
        }

        private void ProcessGestureEvents(PXCMGesture.Gesture gesture)
        {
            switch (gesture.label)
            {
                //Flat Hand
                case PXCMGesture.Gesture.Label.LABEL_POSE_BIG5:
                    if (OnFlatHandGestureDetected != null)
                        OnFlatHandGestureDetected(this, new GestureEventArgs(gesture));
                    break;
                //Peace Symbol 
                case PXCMGesture.Gesture.Label.LABEL_POSE_PEACE:
                    if (OnPeaceSymbolGestureDetected != null)
                        OnPeaceSymbolGestureDetected(this, new GestureEventArgs(gesture));
                    break;
                //Thumbs Down
                case PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_DOWN:
                    if (OnThumbsDownGestureDetected != null)
                        OnThumbsDownGestureDetected(this, new GestureEventArgs(gesture));
                    break;
                //Thumbs Up
                case PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_UP:
                    if (OnThumbsUpGestureDetected != null)
                        OnThumbsUpGestureDetected(this, new GestureEventArgs(gesture));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Event for returning Depth
        /// </summary>
        /// <param name="e"></param>
        protected unsafe virtual void doEvent(DepthEventArgs e)
        {
            Console.Write("doEvent called");
            //copy event to avoid race condition
            EventHandler<DepthEventArgs> handler = OnDepthDetected;


            if (handler == null) Console.Write("no subscriber");

            if (handler != null) //if there are subscribers
            {
                for (; ; )
                {
                    //NewGestureFrame();

                    double[] compareArray = new double[numFramesToAverage];

                    for (int i = 0; i < numFramesToAverage; i++)
                    {
                        if (!base.AcquireFrame(true)) break;
                        {
                            PXCMImage image = base.QueryImage(PXCMImage.ImageType.IMAGE_TYPE_DEPTH);
                            PXCMImage.ImageData ddata;


                            pxcmStatus sts = image.AcquireAccess(PXCMImage.Access.ACCESS_READ, out ddata);
                            if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR)
                            {
                                //Console.WriteLine("breaking");
                                break;
                            }
                            else
                            {
                                short* depth = (short*)ddata.buffer.planes[0];

                                //Console.WriteLine("{0:N}", depth[0]);

                                short minimumValue = (short)(screenLength + distanceFromScreenEdge);
                                //short minimmValue = 33000; 
                                //int minx = 0;
                                //int miny = 0;
                                float zRatio = 0;
                                float discardedX = 0;
                                float discardedY = 0;
                                short rawDepth;

                                for (int y = 0; y < 240; y++)
                                {
                                    for (int x = 0; x < 320; x++)
                                    {
                                        rawDepth = depth[(y * 320) + x];
                                        if (rawDepth < distanceFromScreenEdge) rawDepth = distanceFromScreenEdge;
                                        if (rawDepth > distanceFromScreenEdge + screenLength) rawDepth = (short)(screenLength + distanceFromScreenEdge);

                                        if (rawDepth < minimumValue && rawDepth >= distanceFromScreenEdge)
                                        {
                                            int originRelativeX = x - 160;
                                            int originRelativeY = y - 160;

                                            zRatio = (rawDepth - distanceFromScreenEdge) / rawDepth;
                                            discardedX = zRatio * 160;
                                            discardedY = zRatio * 120;

                                            if ((rawDepth < minimumValue) && (Math.Abs(originRelativeX) < (160 - discardedX)) && (Math.Abs(originRelativeY) < (120 - discardedY)))
                                            {
                                                minimumValue = rawDepth;
                                            }
                                        }
                                    }
                                }

                                //Console.WriteLine("Minimum Value: " + string.Format("{0:N}", minimumValue));

                                image.ReleaseAccess(ref ddata);
                                base.ReleaseFrame();


                                compareArray[i] = minimumValue;
                                //Console.WriteLine(compareArray[i].ToString());

                                //e.Depth = string.Format("{0:N}", minimumValue);
                                //e.Depth = (float)Convert.ToDecimal(string.Format("{0:N}", minimumValue));//this was working

                                //if (minimumValue < 890) {
                                //  handler(this, new CustomEventArgs(minimumValue)); 
                            }
                        }
                    }
                    var sampleFrames = new DescriptiveStatistics(compareArray);

                    var stdDev = sampleFrames.StandardDeviation;
                    var sampleMean = sampleFrames.Mean;

                    var filteredFrames =
                        (from n in compareArray
                         where Math.Abs(n - sampleMean) < stdDev
                         select n);

                    double filteredResult = (filteredFrames.Sum() / filteredFrames.Count()) - distanceFromScreenEdge;

                    if (filteredResult < screenLength && filteredResult != - distanceFromScreenEdge)
                    {
                        //handler(this, new DepthEventArgs(filteredResult));
                        OnDepthDetected(this, new DepthEventArgs(filteredResult));
                    }
                }


            }
        }

        // <summary>
        // Function for returning what the positions of the left and right hand are
        // </summary>
        // <returns></returns>
        //public void NewGestureFrame()
        //{
        //    HandInformation returnedHandInformation = new HandInformation();

        //    PXCMGesture gesture = QueryGesture();

        //    PXCMGesture.GeoNode leftNodeData;
        //    pxcmStatus leftNodeStatus;

        //    PXCMGesture.GeoNode rightNodeData;
        //    pxcmStatus rightNodeStatus;

        //    leftNodeStatus = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_LEFT, out leftNodeData);
        //    rightNodeStatus = gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_RIGHT, out rightNodeData);

        //    if (leftNodeStatus >= pxcmStatus.PXCM_STATUS_NO_ERROR)
        //        returnedHandInformation.LeftHand = leftNodeData;
        //    if (rightNodeStatus >= pxcmStatus.PXCM_STATUS_NO_ERROR)
        //        returnedHandInformation.RightHand = rightNodeData;

        //    if (leftNodeStatus >= pxcmStatus.PXCM_STATUS_NO_ERROR || rightNodeStatus >= pxcmStatus.PXCM_STATUS_NO_ERROR)
        //        getHandsEvent(this, new HandEventArgs(returnedHandInformation));

        //}
        
    }
}




