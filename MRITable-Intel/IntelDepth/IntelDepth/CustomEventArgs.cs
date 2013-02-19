using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelDepth
{
    // Custom Class Definition for Depth Value Event
    public class DepthEventArgs : EventArgs
    {
        public DepthEventArgs(double s)
        {
            _depth = s;
        }
        private double _depth;

        public double Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }
    }

    // Custom Class Definition for Speech Value Event
    public class SpeechEventArgs : EventArgs
    {
        private String _sentence;

        public String Sentence
        {
            get { return _sentence; }
            set { _sentence = value; }
        }

        public SpeechEventArgs(String s)
        {
            _sentence = s;
        }
    }

    //Custom Class Definition for Speech Value Event
    public class GestureEventArgs : EventArgs
    {
        private PXCMGesture.Gesture _gesture;

        public PXCMGesture.Gesture Gesture
        {
            get { return _gesture; }
            set { _gesture = value; }
        }

        public GestureEventArgs(PXCMGesture.Gesture gesture)
        {
            _gesture = gesture;
        }
    }

    //Custom Class Definition for Hand Value Event
    public class HandEventArgs : EventArgs
    {
        private HandInformation _hands;

        public HandInformation Hands
        {
            get { return _hands; }
            set { _hands = value; }
        }

        public HandEventArgs(HandInformation hands)
        {
            _hands = hands;
        }
    }
}
