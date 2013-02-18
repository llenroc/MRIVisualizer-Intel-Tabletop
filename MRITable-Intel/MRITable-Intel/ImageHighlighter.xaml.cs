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

namespace MRITable_Intel
{
    /// <summary>
    /// Interaction logic for ImageHighlighter.xaml
    /// </summary>
    public partial class ImageHighlighter : UserControl
    {

        List<Rectangle> myRectangles; 
        int numberOfSlices;

        public ImageHighlighter()
        {
            InitializeComponent();
            myRectangles = new List<Rectangle>();
        }

        public void showSlice(int slicenumber)
        {
            // Remove last selected bar
            foreach (Rectangle r in this.myRectangles)
            {
                r.Opacity = 0.0;
            }

            if (slicenumber > 0 && slicenumber <= numberOfSlices)
            {
                Rectangle rect = this.myRectangles[slicenumber];
                rect.Opacity = 0.7;
            }
        }

        public void SetupGrid(int numberOfSlices)
        {
            this.numberOfSlices = numberOfSlices;

            for (int i = 0; i < numberOfSlices; i++)
            {
                innerGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j < numberOfSlices; j++)
            {
                Rectangle r = new Rectangle();
                r.SetValue(Grid.RowProperty, j);
                r.Fill = Brushes.DodgerBlue;
                r.Opacity = 0.0;
                myRectangles.Add(r);

                innerGrid.Children.Add(r);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            innerGrid.Width = BodyImage.ActualWidth;
        }
    }
}
