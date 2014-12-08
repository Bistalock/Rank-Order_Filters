/*
 *      Project Name: Rank-Order EV filter
 *      Date Written: October 17, 2014
 *      Created by: Gabriel Del Pino. Portions inspired by Hiroyuki Plumlee.
 */  

#include "cuda.h"
#include <cmath>



extern "C"
{
	// **TODO: incorporate other transformation methods - currently only works with the mean values
	// Function requires a 1 dimentional buffer of an image, along as various parameters
	// The image array buffer is later converted to a 3 dimentional dynamic array in the format of inputImage[Samples][Height][Width]
	__declspec(dllexport) unsigned char* getImage(unsigned char* mirrorImageBuffer, int height, int width, int samples, int kernelHeight, int kernelWidth, int EV)
	{
		int offsetHeight = (kernelHeight - 3) / 2 + 1; // calculation of the center 
		int offsetWidth = (kernelWidth - 3) / 2 + 1; // calculation of the center  
		
		// Mirrorimage before the creation of the kernel
		int MirroredHeight = height + (offsetHeight * 2);
		int MirroredWidth = width + (offsetWidth * 2);
		
		// Initiallizing the 3 dimentional dynamic array as a series of pointers
		unsigned char*** mirrorImage = new unsigned char**[samples];
		
		// Converting the 1 dimentional array to a 3 dimentional array for C++
		for (int k = 0; k < samples; k++)
		{
			mirrorImage[k] = new unsigned char*[MirroredHeight];
			for (int i = 0; i < MirroredHeight; i++)
			{
				mirrorImage[k][i] = new unsigned char[MirroredWidth];
				for (int j = 0; j < MirroredWidth; j++)
				{            
					mirrorImage[k][i][j] = mirrorImageBuffer[((MirroredWidth * samples) * i) + (samples * j) + k];
				}
			}
		}

		
		// initiallizing the kernel widow
		unsigned char** window = new unsigned char*[kernelHeight]; // the kernel window for each sample
		// initiallizing the array values to zero
		for (int i = 0; i < kernelHeight; i++)
		{
			window[i] = new unsigned char[kernelWidth];
			for (int j = 0; j < kernelWidth; j++)
				{
					window[i][j] = 0;
				}
		}
		
		// initializing the resulting image
		unsigned char*** processedImage = new unsigned char**[samples, height, width]; // the resulting image after the filtering
		// initiallizing the array values to zero
		for (int k = 0; k < samples; k++)
		{
			processedImage[k] = new unsigned char*[height];
			for (int i = 0; i < height; i++)
			{
				processedImage[k][i] = new unsigned char[width];
				for (int j = 0; j < width; j++)
				{
					processedImage[k][i][j] = 0;
				}
			}
		}
		
		//  Perfect CUDA zone
		#pragma region Applying kernel to every pixel in image
		// Consider adding if statement for samples
		for (int k = 0; k < samples; k++)
		{ 
			for (int i = offsetHeight; i < MirroredHeight - offsetHeight; i++)
			{
				for (int j = offsetWidth; j < MirroredWidth - offsetWidth; j++)
				{
					
					// Begin creation of kernel
					double addedsum = 0;
					double selected = 0;
					
					int mean; // initializing the mean variable
					
					// the current pixel that is being processed. Also the center pixel of the kernel to be created.
					unsigned char temp = mirrorImage[k][i][j]; 
					
					// *** this loop is commented because we may create the kernel at the same time we calculate a tranformation method (mean, median) *** // 
					//for (int l = 0; l < kernelHeight; l++) // kernel height
					//{
					//    for (int m = 0; m < kernelWidth; m++) // kernel width
					//    {
					//        window[l, m] = mirrorImage[k][i - offsetHeight + l][j - offsetWidth + m]; // applying the intensity values to the kernel window
					//	  }
					//}
					
					// loops collecting the pixels within the kernel meeting the EV criterion and adding to addedsum temp var
					for (int l = 0; l < kernelHeight; l++) // kernel height
					{
						for (int m = 0; m < kernelWidth; m++) // kernel width
						{
							window[l][m] = mirrorImage[k][i - offsetHeight + l][j - offsetWidth + m]; // applying the intensity values to the kernel window
							/* this if statement is commented because of the fact that Project VEGA seems to take the 
							 * center pixel into account as well... */
							//if (l != offsetHeight || m != offsetWidth) // as long as the pixel to be processed in not the center pixel
							//{
							    // ***** obtaning the absolute difference of the pixel within the kernel to the offset ***** //
								if (abs(window[l][m] - temp) <= EV) 
								{
									// **TODO: maybe add switch/case statement for mean and median methods?
									
									// used for mean method:
									selected++; // incrementing the counter for the pixel within the kernel
									addedsum += window[l][m]; // adding the intenities within the kernel to one variable
								}
							//}
						}
					}
					
					// calulating the mean of kernel                     
					// if no pixels in kernel matched EV parameter, set to the center pixel. Else, add all values and divided by the number of elements
					selected == 0 ? mean = window[offsetHeight][offsetWidth] : mean = int(addedsum / selected); 
					
					//if (selected == 0)
					//{
					// mean = window[offsetHeight][offsetWidth]; // the center coordinate of a kernel
					//}
					//else
					//{
					// mean = int(addedsum / selected); // all added values, divided by the number of elements
					//}
					
					// tranfering the mean values to the resulting image
					processedImage[k][i - offsetHeight][j - offsetWidth] = unsigned char(mean);                      
				}
			}
		} // end of EV filtering
		#pragma endregion
		
		// Initializing the 1 dimentional array buffer
		unsigned char* processedImageBuffer = new unsigned char[samples * width * height];
		
		// converting the 3 dimentional array to a 1 dimentional buffer for C#
		for (int k = 0; k < samples; k++) // the samples per pixel
		{
			for (int i = 0; i < height; i++) // the height (or column) of an image
			{
				for (int j = 0; j < width; j++) // the width (or row) of an image
				{
					processedImageBuffer[((width * samples) * i) + (samples * j) + k] = processedImage[k][i][j]; // assigining all values
				}
			}
		}
		
		// outputs the 1 dimentional array buffer.
		return processedImageBuffer;
	}
}
