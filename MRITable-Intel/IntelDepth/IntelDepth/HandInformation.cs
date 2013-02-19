using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelDepth
{
    public class HandInformation
    {
        private PXCMGesture.GeoNode _leftHand;
        public PXCMGesture.GeoNode LeftHand
        {
            get { return _leftHand; }
            set { _leftHand = value; }
        }

        private PXCMGesture.GeoNode _rightHand;
        public PXCMGesture.GeoNode RightHand
        {
            get { return _rightHand; }
            set { _rightHand = value; }
        }
    }
}
