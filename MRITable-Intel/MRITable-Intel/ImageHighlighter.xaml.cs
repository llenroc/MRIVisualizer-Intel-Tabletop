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

using IntelDepth; 
using GroupLab.iNetwork;

namespace MRITable_Intel
{
    /// <summary>
    /// Interaction logic for ImageHighlighter.xaml
    /// </summary>
    public partial class ImageHighlighter : UserControl
    {
        const double SIZE_OF_TABLE = 500.0;
        const int NUM_OF_SLICES = 100;

        List<Rectangle> rectangles;
        Network network; 

        private void Depth_DepthValueDetected(object sender, DepthEventArgs e)
        {
            this.Dispatcher.Invoke(
                new Action(
                    delegate()
                    {
                        int slice = ComputeSlice(e.Depth);

                        //Update UI of reference body
                        ShowSlice(slice);
                        Console.WriteLine(String.Format("{0} mm, {1} slice", e.Depth, slice));

                        //Dispatch slice to attached devices
                        network.DispatchSlice(slice); 
                    }
                )
            );
        }


        public ImageHighlighter()
        {
            InitializeComponent();
            rectangles = new List<Rectangle>();
        }

        public void ShowSlice(int slicenumber)
        {
            // Remove last selected bar
            foreach (Rectangle r in this.rectangles)
            {
                r.Opacity = 0.0;
            }

            if (slicenumber > 0 && slicenumber <= NUM_OF_SLICES)
            {
                Rectangle rect = this.rectangles[slicenumber];
                rect.Opacity = 0.7;
            }
        }

        private int ComputeSlice(double distance)
        {
            double percentageIntoTable = (distance / SIZE_OF_TABLE);
            int chosenSlice = (int)(percentageIntoTable * NUM_OF_SLICES);

            return chosenSlice;
        }


        #region Setup & Utility Methods
        public void SetupImageHighlighter(IntelCameraPipeline camera)
        {
            SetupGrid();
            SetupCamera(camera);

            //Setup networking
            network = new Network();
            network.StartServer();
        }
        private void SetupCamera(IntelCameraPipeline camera)
        {
            camera.OnDepthDetected += Depth_DepthValueDetected;
        }

        private void SetupGrid()
        {

            for (int i = 0; i < NUM_OF_SLICES; i++)
            {
                innerGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j < NUM_OF_SLICES; j++)
            {
                Rectangle r = new Rectangle();
                r.SetValue(Grid.RowProperty, j);
                r.Fill = Brushes.DodgerBlue;
                r.Opacity = 0.0;
                rectangles.Add(r);

                innerGrid.Children.Add(r);
            }
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            innerGrid.Width = BodyImage.ActualWidth;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            network.StopServer(); 
        }

        #endregion

    }
}
