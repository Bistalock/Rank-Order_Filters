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
using System.Windows.Navigation;
using System.Windows.Shapes;
/* ---------------------- Added Libraries ---------------------- */
using BitMiracle.LibTiff.Classic; // Use Tiff images
using System.Collections.Specialized; // String Collection
using System.IO; // Memory Stream
using System.Runtime.InteropServices; // DLL support

namespace Filter_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("EV_Filter.dll", CallingConvention= CallingConvention.Cdecl)]
        public static extern int getImage(int height, int width, int samples, int kernel, int EV);

        [DllImport("EV_Filter.dll")]
        public static extern int EV_Filter();

        [DllImport("EV_Filter.dll")]
        public static extern int EV_Filter2();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
        }

        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            // Overrides default behavior
            e.Handled = true;
        }

        private void TextBox_PreviewDrop(object sender, DragEventArgs e)
        {
            // Get data object
            var dataObject = e.Data as DataObject;

            // Check for file list
            if (dataObject.ContainsFileDropList())
            {
                // Clear values
                ((TextBox)sender).Text = string.Empty;

                // Process file names
                StringCollection fileNames = dataObject.GetFileDropList();
                StringBuilder bd = new StringBuilder();
                foreach (var fileName in fileNames)
                {
                    bd.Append(fileName);
                }

                // Set text
                ((TextBox)sender).Text = bd.ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //int A = FilterTest();
            //string B = Convert.ToString(A);
            //MessageBoxResult test = MessageBox.Show(B, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //if (test == MessageBoxResult.OK)
            //{
            //    return;
            //}

            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.InitialDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"Resources");
            dlg.DefaultExt = ".tif";
            dlg.Filter = "TIFF Image (*.tif;*.tiff)|*.tif;.tiff|All files (*.*)|*.*";

            // Assigns the results value when Dialog is opened
            var result = dlg.ShowDialog();

            // Checks if value is true
            if (result == true)
            {
                textBox1.Text = dlg.FileName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBoxResult result = MessageBox.Show("Please input the TIFF image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }

            Tiff image = Tiff.Open(textBox1.Text, "r");

            // Obtain basic tag information of the image
            int width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            byte bits = image.GetField(TiffTag.BITSPERSAMPLE)[0].ToByte();
            byte pixel = image.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToByte();

            // read bytes of an image
            byte[] buffer = File.ReadAllBytes(textBox1.Text);

            MemoryStream memoryStream = new MemoryStream(buffer);

            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = memoryStream;
            imageSource.EndInit();

            var Preview = new Preview(imageSource);
            Preview.Show();

        }

        //public unsafe IntPtr test(byte[,,] processedImage)
        //{
        //    fixed (byte* p = processedImage)
        //    {
        //        IntPtr test2 = (IntPtr)p;
        //        return test2;
        //    }
        //}

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            #region load image
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBoxResult result = MessageBox.Show("Please input the TIFF image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }

            Tiff image = Tiff.Open(textBox1.Text, "r");

            // Obtain basic tag information of the image
            int width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            byte bits = image.GetField(TiffTag.BITSPERSAMPLE)[0].ToByte();
            byte samples = image.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToByte();


            // store the image information in 2d byte array
            // reserve memory for storing the size of 1 line
            byte[] scanline = new byte[image.ScanlineSize()];
            // reserve memory for the size of image

            byte[, ,] inputImage = new byte[samples, height, width];
            for (int k = 0; k < samples; k++)
            {
                for (int i = 0; i < height; i++)
                {
                    image.ReadScanline(scanline, i);
                    for (int j = 0; j < width; j++)
                    {
                        inputImage[k, i, j] = scanline[3 * j + k];
                    }
                }
            } // end grabbing intensity values
            #endregion

            int kernel = 3;
            int EV = 35;

            Byte[, ,] processedImage = new Byte[samples, width, height];

            //IntPtr buffer = test(processedImage);

            //int wtf = 4;

            //int omg = EV_Filter2();
            int omg = getImage(height, width, samples, kernel, EV);
            string lol = Convert.ToString(omg);

            MessageBoxResult test = MessageBox.Show("Value succesfully returned from C++!\nThe value returned is: " + lol, "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
            if (test == MessageBoxResult.OK)
            {
                return;
            }

            

            byte[] test2 = new byte[samples * width * height];

            //Marshal.Copy(pointerarray, test2, 0, samples * width * height);





            for (int k = 0; k < samples; k++)
            {
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        processedImage[k, i, j] = test2[(3 * j) * i + k];
                    }
                }
            } // end grabbing intensity values

            #region Save image
            string fileName = "processed.tif";

            // Create OpenFileDialog 
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".tif";
            dlg.Filter = "TIFF Image (*.tif;*.tiff)|*.tif;.tiff|All files (*.*)|*.*";
            dlg.FileName = fileName;
            // Assigns the results value when Dialog is opened
            var dlgresult = dlg.ShowDialog();

            // Checks if value is true
            if (dlgresult == true)
            {
                // Recreation of the image from 3d byte array image
                using (Tiff output = Tiff.Open(dlg.FileName, "w"))
                {
                    #region SetTagInfo
                    // set tag information
                    output.SetField(TiffTag.IMAGEWIDTH, width);
                    output.SetField(TiffTag.IMAGELENGTH, height);
                    output.SetField(TiffTag.BITSPERSAMPLE, bits);
                    output.SetField(TiffTag.SAMPLESPERPIXEL, samples);
                    output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                    output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    output.SetField(TiffTag.ROWSPERSTRIP, height);
                    output.SetField(TiffTag.XRESOLUTION, 96); //dpiX);
                    output.SetField(TiffTag.YRESOLUTION, 96); //dpiY);
                    output.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.CENTIMETER);
                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
                    #endregion

                    // reserve buffer
                    byte[] buffer = new byte[width * samples];
                    // obtain each line of the final byte arrays and write them to a file

                    // loop is [height,width,samples] because of how Tiff scanlines work
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            for (int k = 0; k < samples; k++)
                            {
                                buffer[3 * j + k] = Convert.ToByte(processedImage[k, i, j]); // saving the resulting image to file
                            }

                        }
                        // write
                        //output.WriteScanline(buffer, i);
                        output.WriteEncodedStrip(i, buffer, image.ScanlineSize());
                    }
                    // write to file
                    output.WriteDirectory();
                    output.Dispose();

                    System.Diagnostics.Process.Start(dlg.FileName); // displays the result
                }// end inner using
            }
            #endregion
        }

        // Closes all preview windows when main window is closed
        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
