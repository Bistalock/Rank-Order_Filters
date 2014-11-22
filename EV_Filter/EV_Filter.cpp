/*
 *		Project Name: Rank-Order EV filter
 *      Date Written: October 17, 2014
 *      Created by: Gabriel Del Pino. Portions inspired by Hiroyuki Plumlee.
 */  

#include "stdafx.h"
#include <cmath>

extern "C"
{
	// Function requires a 1 dimentional buffer of an image, along as various parameters
	// The image array buffer is later converted to a 3 dimentional dynamic array in the format of inputImage[Samples][Height][Width]
	__declspec(dllexport) int* getImage(int* inputImageBuffer, int height, int width, int samples, int kernel, int EV)
	{
		int offset = (kernel - 3) / 2 + 1; // calculation of the center 
        
		// ***TODO: mirrorImage to be place as separate function in the future
		#pragma region Mirroring the image
		// Mirrorimage before the creation of the kernel
		int MirroredHeight = height + (offset * 2);
		int MirroredWidth = width + (offset * 2);

		int*** inputImage = new int**[samples];

		// Converting the 1 dimentional array to a 3 dimentional array for C++
		for (int k = 0; k < samples; k++)
		{
			inputImage[k] = new int*[height];
			for (int i = 0; i < height; i++)
			{
				inputImage[k][i] = new int[width];
				for (int j = 0; j < width; j++)
				{            
					inputImage[k][i][j] = inputImageBuffer[((width * samples) * i) + (samples * j) + k];
				}
			}
		}

		int*** mirrorImage = new int**[samples];

		// initiallizing the array values to zero
		for (int k = 0; k < samples; k++)
		{
			mirrorImage[k] = new int*[MirroredHeight];
			for (int i = 0; i < MirroredHeight; i++)
			{
				mirrorImage[k][i] = new int[MirroredWidth];
				for (int j = 0; j < MirroredWidth; j++)
				{            
					mirrorImage[k][i][j] = 0;
				}
			}
		}

		// copying the original image to the larger mirrored image
		for (int k = 0; k < samples; k++)
		{
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					mirrorImage[k][i + offset][j + offset] = inputImage[k][i][j];
					//cout << "mirrorImage value of " << mirrorImage[k][i][j] << " at index: " <<"[" << k << "] [" << i << "] [" << j << "]" << endl; // printing out each pixel intensities
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
					mirrorImage[k][row][i] = inputImage[k][row - offset][offset - i];
					// copy right columns
					mirrorImage[k][row][i + width + offset] = inputImage[k][row - offset][width - i - 2];
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
					mirrorImage[k][i][col] = mirrorImage[k][(offset * 2) - i][col];
					// copy bottom rows
					mirrorImage[k][i + height + offset][col] = mirrorImage[k][height + offset - 2 - i][col];
				}
			}
		}
		#pragma endregion

		// initiallizing the kernel widow
		int** window = new int*[kernel]; // the kernel window for each sample
		// initiallizing the array values to zero
		for (int i = 0; i < kernel; i++)
		{
			window[i] = new int[kernel];
			for (int j = 0; j < kernel; j++)
				{            
					window[i][j] = 0;
				}
		}

		// initializing the resulting image
		int*** processedImage = new int**[samples, height, width]; // the resulting image after the filtering
		// initiallizing the array values to zero
		for (int k = 0; k < samples; k++)
		{
			processedImage[k] = new int*[height];
			for (int i = 0; i < height; i++)
			{
				processedImage[k][i] = new int[width];
				for (int j = 0; j < width; j++)
				{            
					processedImage[k][i][j] = 0;
				}
			}
		}

		#pragma region Applying kernel to every pixel in image
		// Consider adding if statement for samples
		for (int k = 0; k < samples; k++)
		{ 
			for (int i = offset; i < MirroredHeight - offset; i++)
			{
				for (int j = offset; j < MirroredWidth - offset; j++)
				{                    
                                   
					// Begin creation of kernel
					double addedsum = 0;
					double selected = 0;

					int mean; // initializing the mean variable

					// the current pixel that is being processed. Also the center pixel of the kernel to be created.
					int temp = mirrorImage[k][i][j]; 

					// *** this loop is commented because we may create the kernel at the same time we calculate a tranformation method (mean, median) *** // 
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
							window[l][m] = mirrorImage[k][i - offset + l][j - offset + m]; // applying the intensity values to the kernel window
							/* this if statement is commented because of the fact that Project VEGA seems to take the 
							 * center pixel into account as well... */
							//if (l != offset || m != offset) // as long as the pixel to be processed in not the center pixel
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
					// if no pixels in kernel matched EV parameter, set to the center pixel
					if (selected == 0)
					{
						mean = window[offset][offset]; // the center coordinate of a kernel
					}
					else
					{
						mean = int(addedsum / selected); // all added values, divided by the number of elements
					}

					// tranfering the mean values to the resulting image
					processedImage[k][i - offset][j - offset] = int(mean);                      
				}   
			}
		} // end of EV filtering   
		#pragma endregion

		// Initializing the 1 dimentional array buffer
		int* processedImageBuffer = new int[samples * width * height];

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
