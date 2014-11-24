using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Filter_GUI
{
    /// <summary>
    /// Interaction logic for Preview.xaml
    /// </summary>
    public partial class Preview : Window
    {
        public Preview(BitmapSource previewimage)
        {
            InitializeComponent();

            ImageBox.Source = previewimage;

            var titleHeight = SystemParameters.WindowCaptionHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
            var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;

            this.Height = titleHeight + verticalBorderWidth + ImageBox.Source.Height;
            this.Width =  verticalBorderWidth + verticalBorderWidth+ ImageBox.Source.Width;
            
        }
    }
}
