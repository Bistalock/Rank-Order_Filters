﻿using System;
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

        // Importing the C++ MirrorImage dynamic link library with a P/Invoke call
        [DllImport("MirrorImage.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern IntPtr mirrorImage([In, Out] byte[] inputImageBuffer, int height, int width, int samples, int kernelHeight, int kernelWidth);

        // Importing the C++ EV_Filter dynamic link library with a P/Invoke call
        [DllImport("EV_Filter.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern IntPtr getImage([In, Out] byte[] mirrorImageBuffer, int height, int width, int samples, int kernelHeight, int kernelWidth, int EV);
        
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

        private void loadImageButton_Click(object sender, RoutedEventArgs e)
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
                textBox.Text = dlg.FileName;
            }
        }

        private async void previewButton_Click(object sender, RoutedEventArgs e)
        {
            #region Initialization
            if (string.IsNullOrEmpty(textBox.Text))
            {
                MessageBoxResult result = MessageBox.Show("Please input the TIFF image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }

            Tiff image = Tiff.Open(textBox.Text, "r");

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
            byte[] inputImageBuffer = new byte[samples * height * width];

            // loop gathering the values from a single scanline at a time
            for (int i = 0; i < height; i++)
            {
                image.ReadScanline(scanline, i); // read the scanline for each column
                for (int j = 0; j < samples * width; j++)
                {
                    // writing the entire image as a 1 dimentional integer buffer.
                    inputImageBuffer[image.ScanlineSize() * i + j] = scanline[j];
                }
            } // end grabbing intensity values

            // the transformation method to be used for the filter            
            if (transComboBox.Text == "")
            {
                // Error Windows when no number of samples entered
                MessageBoxResult result = MessageBox.Show("No Transformation method entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }

            // the kernel window to be used
            int kernelHeight = 0;

            if (textBox1.Text == "")
            {
                // Error Windows when no number of samples entered
                MessageBoxResult result = MessageBox.Show("No kernel vertical size selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }
            else
            {
                char[] c = textBox1.Text.ToCharArray(); // seperates compbox elements into an array

                for (int i = 0; i < c.Length; i++)
                {
                    kernelHeight = Convert.ToInt32(textBox1.Text.Substring(0, i + 1));
                }

                // ************************  Let the user enter any odd number as size of the pixel
                if (kernelHeight % 2 != 1)
                {
                    MessageBoxResult result = MessageBox.Show("The kernel window only accepts odd integers.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (result == MessageBoxResult.OK)
                    {
                        return;
                    }
                }
            }

            int kernelWidth = 0;

            if (textBox2.Text == "")
            {
                // Error Windows when no number of samples entered
                MessageBoxResult result = MessageBox.Show("The kernel window only accepts odd integers.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }
            else
            {
                char[] c = textBox2.Text.ToCharArray(); // seperates compbox elements into an array

                for (int i = 0; i < c.Length; i++)
                {
                    kernelWidth = Convert.ToInt32(textBox2.Text.Substring(0, i + 1));
                }

                // ************************  Let the user enter any odd number as size of the pixel
                if (kernelWidth % 2 != 1)
                {
                    MessageBoxResult result = MessageBox.Show("The kernel window only accepts odd integers.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (result == MessageBoxResult.OK)
                    {
                        return;
                    }
                }
            }


            // the paremeter to be used for the filter            
            if (textBox3.Text == "")
            {
                // Error Windows when no number of samples entered
                MessageBoxResult result = MessageBox.Show("No filter parameter entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }

            #endregion

            #region Processing

            Title = "Rank-Order Filters (Working)";
            previewButton.IsEnabled = false;
            processImageButton.IsEnabled = false;

            int Filter_Parameter = Convert.ToInt32(textBox3.Text);

            int offsetHeight = (kernelHeight - 3) / 2 + 1; // calculation of the center 
            int offsetWidth = (kernelWidth - 3) / 2 + 1; // calculation of the center  

            // Mirrorimage before the creation of the kernelTh
            int MirroredHeight = height + (offsetHeight * 2);
            int MirroredWidth = width + (offsetWidth * 2);

            // initiallizing the 1 dimentional resulting image
            byte[] mirrorImageBuffer = new byte[samples * MirroredHeight * MirroredWidth];

            // initiallizing the 1 dimentional resulting image
            byte[] processedImageBuffer = new byte[samples * width * height];

            // Calling the native mirrorImage C++ function via a P/Invoke in C#
            IntPtr unmanagedMirrorBuffer = await Task.Run(() => mirrorImage(inputImageBuffer, height, width, samples, kernelHeight, kernelWidth));

            // Marshaling the resulting interger pointer unmanaged array into a managed 1 dimentional array.
            Marshal.Copy(unmanagedMirrorBuffer, mirrorImageBuffer, 0, samples * MirroredWidth * MirroredHeight);

            // Calling the native EV_Filter C++ function via a P/Invoke in C#
            IntPtr unmanagedArray = await Task.Run(() => getImage(mirrorImageBuffer, height, width, samples, kernelHeight, kernelWidth, Filter_Parameter));

            // Marshaling the resulting interger pointer unmanaged array into a managed 1 dimentional array.
            Marshal.Copy(unmanagedArray, processedImageBuffer, 0, samples * width * height);
            #endregion

            PixelFormat pixelFormat;

            if (samples == 1) pixelFormat = PixelFormats.Gray8;
            else if (samples == 3) pixelFormat = PixelFormats.Rgb24;
            else
            {
                pixelFormat = PixelFormats.Rgba64;
                MessageBoxResult result = MessageBox.Show("This program does not support alpha channels.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                previewButton.IsEnabled = true;
                processImageButton.IsEnabled = true;
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }

            var bytesPerPixel = (pixelFormat.BitsPerPixel + 7) / 8;

            var bitmap = BitmapImage.Create(width, height, 96, 96, pixelFormat, null, processedImageBuffer, samples * width);

            var Preview = new Preview(bitmap);
            Preview.Show();

            Title = "Rank-Order Filters";
            previewButton.IsEnabled = true;
            processImageButton.IsEnabled = true;
        }

        private async void processImageButton_Click(object sender, RoutedEventArgs e)
        {
            #region Initiallization
            if (string.IsNullOrEmpty(textBox.Text))
            {
                MessageBoxResult result = MessageBox.Show("Please input the TIFF image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }

            Tiff image = Tiff.Open(textBox.Text, "r");

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
            byte[] inputImageBuffer = new byte[samples * height * width];

            // loop gathering the values from a single scanline at a time
            for (int i = 0; i < height; i++)
            {
                image.ReadScanline(scanline, i); // read the scanline for each column
                for (int j = 0; j < samples * width; j++)
                {
                    // writing the entire image as a 1 dimentional integer buffer.
                    inputImageBuffer[image.ScanlineSize() * i + j] = scanline[j];
                }
            } // end grabbing intensity values

            ///* --- Old implementation. writes the scanlines into a 3 dimentional array. ---
            // *
            // *   byte[, ,] inputImage = new byte[samples, height, width];
            // *
            // *   for (int k = 0; k < samples; k++)
            // *   {
            // *       for (int i = 0; i < height; i++)
            // *       {
            // *           for (int j = 0; j < width; j++)
            // *           {
            // *               inputImage[k, i, j] = inputImageBuffer[(image.ScanlineSize() * i) + (3 * j) + k];
            // *           }
            // *       }
            // *   } // end grabbing intensity values
            // */

            // the transformation method to be used for the filter            
            if (transComboBox.Text == "")
            {
                // Error Windows when no number of samples entered
                MessageBoxResult result = MessageBox.Show("No Transformation method entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }

            // the kernel window to be used
            int kernelHeight = 0; // initializes the kernel height to zero

            if (textBox1.Text == "")
            {
                // Error Windows when no number of samples entered
                MessageBoxResult result = MessageBox.Show("No kernel vertical size selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }
            else
            {
                char[] c = textBox1.Text.ToCharArray(); // seperates compbox elements into an array

                for (int i = 0; i < c.Length; i++)
                {
                    kernelHeight = Convert.ToInt32(textBox1.Text.Substring(0, i + 1));
                }

                // ************************  Let the user enter any odd number as size of the pixel
                if (kernelHeight % 2 != 1)
                {
                    MessageBoxResult result = MessageBox.Show("The kernel window only accepts odd integers.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (result == MessageBoxResult.OK)
                    {
                        return;
                    }
                }
            }

            int kernelWidth = 0; // initializes the kernel width to zero

            if (textBox2.Text == "")
            {
                // Error Windows when no number of samples entered
                MessageBoxResult result = MessageBox.Show("No kernel horizontal size selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }
            else
            {
                char[] c = textBox2.Text.ToCharArray(); // seperates compbox elements into an array

                for (int i = 0; i < c.Length; i++)
                {
                    kernelWidth = Convert.ToInt32(textBox2.Text.Substring(0, i + 1));
                }

                // ************************  Let the user enter any odd number as size of the pixel
                if (kernelWidth % 2 != 1)
                {
                    MessageBoxResult result = MessageBox.Show("The kernel window only accepts odd integers.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (result == MessageBoxResult.OK)
                    {
                        return;
                    }
                }
            }
            

            // the paremeter to be used for the filter            
            if (textBox3.Text == "")
            {
                // Error Windows when no number of samples entered
                MessageBoxResult result = MessageBox.Show("No filter parameter entered.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (result == MessageBoxResult.OK)
                {
                    return;
                }
            }
            #endregion

            #region Processing

            Title = "Rank-Order Filters (Working)";
            previewButton.IsEnabled = false;
            processImageButton.IsEnabled = false;

            int Filter_Parameter = Convert.ToInt32(textBox3.Text);

            int offsetHeight = (kernelHeight - 3) / 2 + 1; // calculation of the center 
            int offsetWidth = (kernelWidth - 3) / 2 + 1; // calculation of the center  

            // Mirrorimage before the creation of the kernelTh
            int MirroredHeight = height + (offsetHeight * 2);
            int MirroredWidth = width + (offsetWidth * 2);

            // initiallizing the 1 dimentional resulting image
            byte[] mirrorImageBuffer = new byte[samples * MirroredHeight * MirroredWidth];

            // initiallizing the 1 dimentional resulting image
            byte[] processedImageBuffer = new byte[samples * width * height];

            // Calling the native mirrorImage C++ function via a P/Invoke in C#
            IntPtr unmanagedMirrorBuffer = await Task.Run(() => mirrorImage(inputImageBuffer, height, width, samples, kernelHeight, kernelWidth));

            // Marshaling the resulting interger pointer unmanaged array into a managed 1 dimentional array.
            Marshal.Copy(unmanagedMirrorBuffer, mirrorImageBuffer, 0, samples * MirroredWidth * MirroredHeight);

            // Calling the native EV_Filter C++ function via a P/Invoke in C#
            IntPtr unmanagedArray = await Task.Run(() => getImage(mirrorImageBuffer, height, width, samples, kernelHeight, kernelWidth, Filter_Parameter));

            // Marshaling the resulting interger pointer unmanaged array into a managed 1 dimentional array.
            Marshal.Copy(unmanagedArray, processedImageBuffer, 0, samples * width * height);
            #endregion

            #region Save image
            string fileName = System.IO.Path.GetFileNameWithoutExtension(textBox.Text) + "_Processed" + ".tif";

            // Create OpenFileDialog 
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            // Set the dialog box variables
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
                    output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                    output.SetField(TiffTag.ROWSPERSTRIP, 1);
                    output.SetField(TiffTag.XRESOLUTION, 96); //dpiX);
                    output.SetField(TiffTag.YRESOLUTION, 96); //dpiY);
                    if (samples == 1) output.SetField(TiffTag.PHOTOMETRIC, Photometric.MINISBLACK);
                    else if (samples == 3) output.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                    output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);
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
                                buffer[samples * j + k] = processedImageBuffer[((samples * width) * i) + (samples * j) + k]; // saving the resulting image to file
                            }
                        }
                        // write
                        if (samples == 1) output.WriteScanline(buffer, i);
                        else if (samples == 3) output.WriteEncodedStrip(i, buffer, samples * width);
                    }
                    // write to file
                    output.WriteDirectory();
                    output.Dispose();

                    //System.Diagnostics.Process.Start(dlg.FileName); // displays the result
                }// end inner using
            }

            Title = "Rank-Order Filters";
            previewButton.IsEnabled = true;
            processImageButton.IsEnabled = true;
            #endregion
        }

        // Closes all preview windows when main window is closed
        private void Window_Closed(object sender, EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void EV_RadioButton_Click(object sender, RoutedEventArgs e)
        {
            filterHeader.Header = "EV Filter Properties";
            filterLabel.Content = "EV Parameter:";
            transComboBox.Items.Clear();
            transComboBox.Items.Add("Mean");
            transComboBox.Items.Add("Median");
            transComboBox.Items.Add("Cut off");
            transComboBox.Items.Add("Multivalued");
            previewButton.IsEnabled = true;
            processImageButton.IsEnabled = true;

            #region Old code
            //var editor = sender as Style;

            //var s = new Style(typeof(ListBoxItem));
            //var enableSetter = new Setter { Property = IsEnabledProperty, Value = false };
            //s.Setters.Add(enableSetter);

            //editor.ItemContainerStyle = s; 

            //ComboBoxItem Mean = (ComboBoxItem)transComboBox.ItemContainerGenerator.ContainerFromIndex(0);
            //Mean.IsEnabled = false;
            //ComboBoxItem Median = (ComboBoxItem)transComboBox.ItemContainerGenerator.ContainerFromIndex(1);
            //Median.IsEnabled = true;
            //ComboBoxItem cutOff = (ComboBoxItem)transComboBox.ItemContainerGenerator.ContainerFromIndex(2);
            //cutOff.IsEnabled = false;
            //ComboBoxItem Multivalued = (ComboBoxItem)transComboBox.ItemContainerGenerator.ContainerFromIndex(3);
            //Multivalued.IsEnabled = false; 
            #endregion
        }

        private void ER_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            filterHeader.Header = "ER Filter Properties";
            filterLabel.Content = "ER Parameter:";
            transComboBox.Items.Clear();
            //previewButton.IsEnabled = false;
            //processImageButton.IsEnabled = false;

        }

        private void KNV_RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            filterHeader.Header = "KNV Filter Properties";
            filterLabel.Content = "KNV Parameter:";
            transComboBox.Items.Clear();
            transComboBox.Items.Add("Median");
            //previewButton.IsEnabled = false;
            //processImageButton.IsEnabled = false;
        } 
    }
}
