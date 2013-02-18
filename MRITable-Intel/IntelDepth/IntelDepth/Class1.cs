using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelDepth
{
    // Define a class to hold custom event info 
    public class CustomEventArgs : EventArgs
    {
        public CustomEventArgs(float d)
        {
            depth = d;
        }
        private float depth;

        public float Depth
        {
            get { return depth; }
            set { depth = value; }
        }
    }

    // Class that publishes an event 
    class depthStream
    {
        UtilMPipeline pp = new UtilMPipeline();
        const int ErrorCode = -1;
        float distanceFromScreenEdge = 350;

        // Declare the event using EventHandler<T> 
        public event EventHandler<CustomEventArgs> getDepth;

        public void start()
        {
            pp.EnableImage(PXCMImage.ColorFormat.COLOR_FORMAT_DEPTH);
            pp.Init();

            doEvent(new CustomEventArgs(0));
        }

        public void stop()
        {
            pp.Close();
            pp.Dispose();
        }

        public unsafe void calibrateStart()
        {
            for (; ; )
            {

                if (!pp.AcquireFrame(true)) break; //if no frames available, return error code

                else
                {
                    PXCMImage image = pp.QueryImage(PXCMImage.ImageType.IMAGE_TYPE_DEPTH);
                    PXCMImage.ImageData ddata;

                    pxcmStatus sts = image.AcquireAccess(PXCMImage.Access.ACCESS_READ, out ddata);
                    if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                    else
                    {
                        short* depth = (short*)ddata.buffer.planes[0];

                        Console.WriteLine("{0:N}", depth[0]);

                        short minimumValue = 32001;
                        int minx = 0;
                        int miny = 0;
                        int cabx = 0;
                        int caby = 0;

                        double shortestDistanceFromCenter = Math.Sqrt((160^2)+(120^2));

                        for (int y = 0; y < 240; y++)
                        {
                            Console.WriteLine("");
                            for (int x = 0; x < 320; x++)
                            {
                                if ((depth[(y * 320) + x] < minimumValue))
                                {
                                    minimumValue = depth[(y * 320) + x];
                                    minx = x;
                                    miny = y;
                                }
                            }
                        }

                        for (int y = 0; y < 240; y++)
                        {
                            for (int x = 0; x < 320; x++)
                            {
                                short thisDepth = depth[(y * 320) + x];
                                double distanceFromCenter = Math.Sqrt( ( (160 - x) ^ 2 ) + ( (120 - y) ^ 2) );
                                if((depth[(y*320)+x] - depth[(miny*320)+minx])< 600){
                                    if (distanceFromCenter < shortestDistanceFromCenter)
                                    {
                                        shortestDistanceFromCenter = distanceFromCenter;
                                        cabx = x;
                                        caby = y;
                                    }
                                }

                            }
                        }
                        distanceFromScreenEdge = depth[(caby * 320) + cabx];
                        Console.WriteLine("Minimum Value: " + minimumValue + "     X: " + minx + "     Y: " + miny);

                        image.ReleaseAccess(ref ddata);
                        pp.ReleaseFrame();

                    }
                }
            }
            return;
        }

        protected unsafe virtual void doEvent(CustomEventArgs e)
        {
            //copy event to avoid race condition
            EventHandler<CustomEventArgs> handler = getDepth;

            if (handler != null) //if there are subscribers
            {
                if (!pp.AcquireFrame(true)) e.Depth += ErrorCode; //if no frames available, return error code

                else
                {
                    PXCMImage image = pp.QueryImage(PXCMImage.ImageType.IMAGE_TYPE_DEPTH);
                    PXCMImage.ImageData ddata;

                    pxcmStatus sts = image.AcquireAccess(PXCMImage.Access.ACCESS_READ, out ddata);
                    if (sts < pxcmStatus.PXCM_STATUS_NO_ERROR) e.Depth = -1;
                    else
                    {
                        short* depth = (short*)ddata.buffer.planes[0];

                        Console.WriteLine("{0:N}", depth[0]);

                        short minimumValue = 32001;
                        int minx = 0;
                        int miny = 0;

                        for (int y = 0; y < 240; y++)
                        {
                            Console.WriteLine("");
                            for (int x = 0; x < 320; x++)
                            {
                                short thisDepth = depth[(y * 320) + x];
                                float zRatio = (thisDepth - distanceFromScreenEdge) / thisDepth;
                                float discardedX = zRatio * 160;
                                float discardedY = zRatio * 120;

                                if ((depth[(y * 320) + x] < minimumValue) && (Math.Abs(x) < (160-discardedX) ) && ( Math.Abs(y) < (120-discardedY) ))
                                {
                                    minimumValue = depth[(y * 320) + x];
                                    minx = x;
                                    miny = y;
                                }

                            }
                        }

                        Console.WriteLine("Minimum Value: " + minimumValue + "     X: " + minx + "     Y: " + miny);

                        image.ReleaseAccess(ref ddata);
                        pp.ReleaseFrame();

                        e.Depth = minimumValue;
                    }                    
                }

                handler(this, e);
            }
        }
    }
}