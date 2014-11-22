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
        //[DllImport("EV_Filter.dll", CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Auto)]
        //public static extern int[,,] getImage([MarshalAs(UnmanagedType.LPArray)]int[, ,] inputImage, int height, int width, int samples, int kernel, int EV);

        // Importing the C++ EV Filter dynamic link library with a P/Invoke call
        [DllImport("EV_Filter.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern IntPtr getImage([In, Out] int[] inputImage, int height, int width, int samples, int kernel, int EV);
        
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
            byte samples = image.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToByte();

            // store the image information in 2d byte array
            // reserve memory for storing the size of 1 line
            byte[] scanline = new byte[image.ScanlineSize()];
            // reserve memory for the size of image

            // creating the 1 dimentional buffer containing information for ALL 3 DIMENTIONS
            int[] inputImageBuffer = new int[samples * height * width];

            byte[] test = new byte[samples * height * width];

            // loop gathering the values from a single scanline at a time
            for (int i = 0; i < height; i++)
            {
                image.ReadScanline(scanline, i); // read the scanline for each column
                for (int j = 0; j < samples * width; j++)
                {
                    // writing the entire image as a 1 dimentional integer buffer.
                    inputImageBuffer[image.ScanlineSize() * i + j] = scanline[j];
                    test[image.ScanlineSize() * i + j] = Convert.ToByte(inputImageBuffer[image.ScanlineSize() * i + j]);
                }
            } // end grabbing intensity values

            // read bytes of an image
            // byte[] ByteImageBuffer = File.ReadAllBytes(textBox1.Text);

            #region Initialization and processing
            // the kernel window to be used
            int kernel = 3;

            // the paremeter to be used for the filter
            int Filter_Parameter = 35;

            // initiallizing the 1 dimentional resulting image
            int[] processedImageBuffer = new int[samples * width * height];

            // Calling the native C++ function via a P/Invoke in C#
            IntPtr unmanagedArray = getImage(inputImageBuffer, height, width, samples, kernel, Filter_Parameter);

            // Marshaling the resulting interger pointer unmanaged array into a managed 1 dimentional array.
            Marshal.Copy(unmanagedArray, processedImageBuffer, 0, samples * width * height);
            #endregion

            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = new MemoryStream(test);
            imageSource.EndInit();

            var Preview = new Preview(imageSource);
            Preview.Show();

        }

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

            // creating the 1 dimentional buffer containing information for ALL 3 DIMENTIONS
            int[] inputImageBuffer = new int[samples * height * width];

            // loop gathering the values from a single scanline at a time
            for (int i = 0; i < height; i++)
            {
                image.ReadScanline(scanline, i); // read the scanline for each column
                for (int j = 0; j < samples * width; j++)
                {
                    // writing the entire image as a 1 dimentional integer buffer.
                    inputImageBuffer[image.ScanlineSize() * i + j] = Convert.ToInt32(scanline[j]);
                }
            } // end grabbing intensity values

            /* --- Old implementation. writes the scanlines into a 3 dimentional array. ---
             *
             *   int[, ,] inputImage = new int[samples, height, width];
             *
             *   for (int k = 0; k < samples; k++)
             *   {
             *       for (int i = 0; i < height; i++)
             *       {
             *           for (int j = 0; j < width; j++)
             *           {
             *               inputImage[k, i, j] = inputImageBuffer[(image.ScanlineSize() * i) + (3 * j) + k];
             *           }
             *       }
             *   } // end grabbing intensity values
             */
            #endregion

            #region Initialization and processing
            // the kernel window to be used
            int kernel = 3;

            // the paremeter to be used for the filter
            int Filter_Parameter = 35;

            // initiallizing the 1 dimentional resulting image
            int[] processedImageBuffer = new int[samples * width * height];

            // Calling the native C++ function via a P/Invoke in C#
            IntPtr unmanagedArray = getImage(inputImageBuffer, height, width, samples, kernel, Filter_Parameter);

            // Marshaling the resulting interger pointer unmanaged array into a managed 1 dimentional array.
            Marshal.Copy(unmanagedArray, processedImageBuffer, 0, samples * width * height);
            #endregion

            #region Save image
            string fileName = "processed.tif";

            // Create OpenFileDialog 
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set the dialog box variables
            dlg.InitialDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @"Resources");
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
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);
                    output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                    output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);
                    if (samples == 1) output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    else if (samples == 3) output.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
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
                                buffer[samples * j + k] = Convert.ToByte(processedImageBuffer[(image.ScanlineSize() * i) + (samples * j) + k]); // saving the resulting image to file
                            }

                        }
                        // write
                        if (samples == 1) output.WriteScanline(buffer, i);
                        else if (samples == 3) output.WriteEncodedStrip(i, buffer, image.ScanlineSize());
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
