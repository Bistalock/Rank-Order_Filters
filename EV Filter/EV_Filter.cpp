/*
 *      Date Written: October 17, 2014
 *      Code Written by: Mr. Gabriel Del Pino. Portions inspired by Hiroyuki Plumlee.
 */  

#include "stdafx.h"
#include "EV_Filter.h"
#include <cmath>

// Definition of EV Filter constructor
EV_Filter::EV_Filter(): EV(35), kernel(3)
{
	//array<Byte,3>^ inputImage = gcnew array<Byte,3>(samples, height, width);
}

// Definition of EV Filter destructor
EV_Filter::~EV_Filter(void)
{
	//delete[] inputImage;
}

array<Byte,3>^ EV_Filter::getImage(array<Byte,3>^ inputImage, int height, int width, int samples, int kernel, int EV)//wchar_t *phrase)
{
	int offset = (kernel - 3) / 2 + 1; // calculation of the center 
    //inputImage = new int[];

    #pragma region Mirroring the image
    // Mirrorimage before the creation of the kernel
    int MirroredHeight = height + (offset * 2);
    int MirroredWidth = width + (offset * 2);

	array<Byte,3>^ mirrorImage = gcnew array<Byte,3>(samples, MirroredHeight, MirroredWidth);

    // copying the original image to the larger mirrored image
	for (int k = 0; k < samples; k++)
    {
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
            
                mirrorImage[k, i + offset, j + offset] = inputImage[k, i, j];
            }
        }
    }

    // copy columns work! =]
	for (int k = 0; k < samples; k++)
    {
		for (int i = 0; i < offset; i++) // 0~2
		{
			for (int row = offset; row < offset + height; row++) // 3~514
			{

                // copy left columns
                mirrorImage[k, row, i] = inputImage[k, row - offset, offset - i];
                // copy right columns
                mirrorImage[k, row, i + width + offset] = inputImage[k, row - offset, width - i - 2];
            }
        } // end for
    } // end for

	for (int k = 0; k < samples; k++)
    {
		for (int i = 0; i < offset; i++)
		{
			for (int col = 0; col < width + (offset * 2); col++)
			{
            
                // copy top rows
                mirrorImage[k, i, col] = mirrorImage[k, (offset * 2) - i, col];
                // copy bottom rows
                mirrorImage[k, i + height + offset, col] = mirrorImage[k, height + offset - 2 - i, col];
            }
        }
    }
    #pragma endregion

    array<Byte,2>^ window = gcnew array<Byte,2>(kernel, kernel); // the kernel window for each sample

    array<Byte,3>^ processedImage = gcnew array<Byte,3>(samples, height, width); // the resulting image after the filtering

    #pragma region Applying kernel to every pixel in image
	// Consider adding if statement for samples
    for (int k = 0; k < samples; k++)
    { 
		for (int i = offset; i < MirroredHeight - offset; i++)
		{
			for (int j = offset; j < MirroredWidth - offset; j++)
			{                    
                                   
                #pragma region Kernel
                // Begin creation of kernel
                double addedsum = 0;
                double selected = 0;

                int mean;

                int temp = mirrorImage[k, i, j];

                //for (int l = 0; l < kernel; l++) // kernel height
                //{
                //    for (int m = 0; m < kernel; m++) // kernel width
                //    {
                //        window[l, m] = mirrorImage[k, i - offset + l, j - offset + m]; // applying the intensity values to the kernel window
				//	  }
                //}

                // loops collecting the pixels within the kernel meeting the EV criterion and adding to addedsum temp var
                for (int l = 0; l < kernel; l++) // kernel height
                {
                    for (int m = 0; m < kernel; m++) // kernel width
                    {
                        window[l, m] = mirrorImage[k, i - offset + l, j - offset + m]; // applying the intensity values to the kernel window
                        //if (l != offset || m != offset) // as long as the pixel to be processed in not the center pixel
                        //{
                            if (abs(window[l, m] - temp) <= EV) // obtaning the absolute difference of the pixel within the kernel to the offset
                            {                                    
                                selected++; // incrementing the counter for the pixel within the kernel
                                addedsum += window[l, m]; // adding the intenities within the kernel to one variable
                            }
                        //}
                    }
                }

                // calulating the mean of kernel                      
                // if no pixels in kernel matched EV parameter, set to the center pixel
                if (selected == 0)
                {
                    mean = window[offset, offset];
                }
                else
                {
                    mean = int(addedsum / selected);
                }

                // tranfering the mean values to the resulting image
                processedImage[i - offset, j - offset, k] = int(mean);

                #pragma endregion                        
            }   
        }
    } // end of EV filtering   
    #pragma endregion

	return processedImage;
}

//int FilterTest()
//{
//	int A = 5;
//	return A;
//}