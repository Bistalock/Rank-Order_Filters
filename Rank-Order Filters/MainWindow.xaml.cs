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

namespace Rank_Order_Filters
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            #region GetTagInfo
            int width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            byte bits = image.GetField(TiffTag.BITSPERSAMPLE)[0].ToByte();
            byte pixel = image.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToByte();
            #endregion

            #region Grayscale Image
            if (pixel == 1)
            {
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
            #endregion

            #region Color Image
            if (pixel == 3)
            {
                //int imageSize = height * width * 3;
                //int[] raster = new int[imageSize];

                //byte[] scanline = new byte[image.ScanlineSize()];

                //// Initiallization of RGB values
                //int[,] red = new int[height, width];
                //int[,] green = new int[height, width];
                //int[,] blue = new int[height, width];

                //for (int i = 0; height > i; i++)
                //{
                //    image.ReadScanline(scanline, i); // EVIL BUG HERE
                //    for (int j = 0; j < width; j++)
                //    {
                //        red[i, j] = scanline[3 * j]; // PSNR: INFINITY, Channel is correct
                //        green[i, j] = scanline[3 * j + 1]; // PSNR: INFINITY, Channel is correct
                //        blue[i, j] = scanline[3 * j + 2]; // PSNR: INFINITY, Channel is correct
                //    }
                //}

                //byte[,] RGB = new byte[height, image.ScanlineSize()];

                //#region Merge RGB
                //for (int i = 0; i < height; i++)
                //{
                //    for (int j = 0; j < width; j++)
                //    {
                //        RGB[i, 3 * j] = Convert.ToByte(red[i, j]);
                //    }
                //}

                //for (int i = 0; i < height; i++)
                //{
                //    for (int j = 0; j < width; j++)
                //    {
                //        RGB[i, 3 * j + 1] = Convert.ToByte(green[i, j]);
                //    }
                //}
                //for (int i = 0; i < height; i++)
                //{
                //    for (int j = 0; j < width; j++)
                //    {
                //        RGB[i, 3 * j + 2] = Convert.ToByte(blue[i, j]);
                //    }
                //}
                //#endregion

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
            #endregion
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
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
            #region GetTagInfo
            int width = image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = image.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
            byte bits = image.GetField(TiffTag.BITSPERSAMPLE)[0].ToByte();
            byte pixel = image.GetField(TiffTag.SAMPLESPERPIXEL)[0].ToByte();
            #endregion

            #region Grayscale Image
            if (pixel == 1)
            {
                // store the image information in 2d byte array
                // reserve memory for storing the size of 1 line
                byte[] scanline = new byte[image.ScanlineSize()];
                // reserve memory for the size of image
                byte[,] gray = new byte[height, width];
                for (int i = 0; i < height; i++)
                {
                    image.ReadScanline(scanline, i);
                    {
                        for (int j = 0; j < width; j++)
                            gray[i, j] = scanline[j];
                    }
                } // end grabbing intensity values   

                // Create OpenFileDialog 
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

                // Set filter for file extension and default file extension 
                dlg.DefaultExt = ".tif";
                dlg.Filter = "TIFF Image (*.tif;*.tiff)|*.tif;.tiff|All files (*.*)|*.*";
                dlg.FileName = System.IO.Path.GetFileNameWithoutExtension(textBox1.Text) + "_Filtered" + ".tif";
                // Assigns the results value when Dialog is opened
                var dlgresult = dlg.ShowDialog();

                // Checks if value is true
                if (dlgresult == true)
                {
                    using (Tiff output = Tiff.Open(dlg.FileName, "w"))
                    {
                        output.SetField(TiffTag.IMAGEWIDTH, width);
                        output.SetField(TiffTag.IMAGELENGTH, height);
                        output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                        output.SetField(TiffTag.BITSPERSAMPLE, 8);
                        output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                        output.SetField(TiffTag.ROWSPERSTRIP, height);
                        output.SetField(TiffTag.XRESOLUTION, 88.0);
                        output.SetField(TiffTag.YRESOLUTION, 88.0);
                        output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);
                        output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                        output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                        output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                        output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);


                        byte[] im = new byte[width * sizeof(byte /*can be changed depending on the format of the image*/)];

                        for (int i = 0; i < height; ++i)
                        {
                            for (int j = 0; j < width; ++j)
                            {
                                im[j] = Convert.ToByte(gray[i, j]);
                            }
                            output.WriteScanline(im, i);
                        }
                        output.WriteDirectory();
                        output.Dispose();
                    }
                }
            }
            #endregion

            #region Color Image
            if (pixel == 3)
            {
                int imageSize = height * width * 3;
                int[] raster = new int[imageSize];

                byte[] scanline = new byte[image.ScanlineSize()];

                // Initiallization of RGB values
                int[,] red = new int[height, width];
                int[,] green = new int[height, width];
                int[,] blue = new int[height, width];

                for (int i = 0; height > i; i++)
                {
                    image.ReadScanline(scanline, i); // Potential fix needed here
                    for (int j = 0; j < width; j++)
                    {
                        red[i, j] = scanline[3 * j]; // PSNR: INFINITY, Channel is correct
                        green[i, j] = scanline[3 * j + 1]; // PSNR: INFINITY, Channel is correct
                        blue[i, j] = scanline[3 * j + 2]; // PSNR: INFINITY, Channel is correct
                    }
                }

                // new array holding color values
                byte[,] RGB = new byte[height, image.ScanlineSize()];

                #region Merge RGB
                // loop obtaining the red color values
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        RGB[i, 3 * j] = Convert.ToByte(red[i, j]);
                    }
                }
                // loop containing green color values
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        RGB[i, 3 * j + 1] = Convert.ToByte(green[i, j]);
                    }
                }
                // loop containing blue color values
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        RGB[i, 3 * j + 2] = Convert.ToByte(blue[i, j]);
                    }
                }
                #endregion

                // Create OpenFileDialog 
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

                // Set filter for file extension and default file extension 
                dlg.DefaultExt = ".tif";
                dlg.Filter = "TIFF Image (*.tif;*.tiff)|*.tif;.tiff|All files (*.*)|*.*";
                dlg.FileName = System.IO.Path.GetFileNameWithoutExtension(textBox1.Text) + "_Filtered" + ".tif";
                // Assigns the results value when Dialog is opened
                var dlgresult = dlg.ShowDialog();

                // Checks if value is true
                if (dlgresult == true)
                {
                    using (Tiff output = Tiff.Open(dlg.FileName, "w"))
                    {

                        // Write the tiff tags to the file
                        output.SetField(TiffTag.IMAGEWIDTH, width);
                        output.SetField(TiffTag.IMAGELENGTH, height);
                        output.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                        output.SetField(TiffTag.BITSPERSAMPLE, 8);
                        output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
                        output.SetField(TiffTag.ROWSPERSTRIP, height);
                        output.SetField(TiffTag.XRESOLUTION, 88.0);
                        output.SetField(TiffTag.YRESOLUTION, 88.0);
                        output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);
                        output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                        output.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                        output.SetField(TiffTag.COMPRESSION, Compression.NONE);
                        output.SetField(TiffTag.FILLORDER, FillOrder.MSB2LSB);


                        byte[] im = new byte[image.ScanlineSize() * sizeof(byte /*can be changed depending on the format of the image*/)];

                        for (int i = 0; i < height; i++)
                        {

                            for (int j = 0; j < image.ScanlineSize(); j++)
                            {
                                im[j] = RGB[i, j];
                            }
                            output.WriteEncodedStrip(i, im, image.ScanlineSize());
                        }
                        output.WriteDirectory();
                        output.Dispose();
                    }
                }
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
