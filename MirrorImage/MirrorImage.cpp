/*
 *		Project Name: Mirror Image function
 *      Date Written: October 17, 2014
 *      Created by: Hiroyuki Plumlee. Rewritten by Gabriel Del Pino.
 */ 

#include "stdafx.h"

extern "C"
{
	_declspec(dllexport) unsigned char* mirrorImage(unsigned char* inputImageBuffer, int height, int width, int samples, int kernelHeight, int kernelWidth)
	{
		int offsetHeight = (kernelHeight - 3) / 2 + 1; // calculation of the center 
		int offsetWidth = (kernelWidth - 3) / 2 + 1; // calculation of the center 

		// Mirrorimage before the creation of the kernel
		int MirroredHeight = height + (offsetHeight * 2);
		int MirroredWidth = width + (offsetWidth * 2);

		unsigned char*** inputImage = new unsigned char**[samples];

		// Converting the 1 dimentional array to a 3 dimentional array for C++
		for (int k = 0; k < samples; k++)
		{
			inputImage[k] = new unsigned char*[height];
			for (int i = 0; i < height; i++)
			{
				inputImage[k][i] = new unsigned char[width];
				for (int j = 0; j < width; j++)
				{            
					inputImage[k][i][j] = inputImageBuffer[((width * samples) * i) + (samples * j) + k];
				}
			}
		}

		unsigned char*** mirrorImage = new unsigned char**[samples];

		// initiallizing the array values to zero
		for (int k = 0; k < samples; k++)
		{
			mirrorImage[k] = new unsigned char*[MirroredHeight];
			for (int i = 0; i < MirroredHeight; i++)
			{
				mirrorImage[k][i] = new unsigned char[MirroredWidth];
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
					mirrorImage[k][i + offsetHeight][j + offsetWidth] = inputImage[k][i][j];
					//cout << "mirrorImage value of " << mirrorImage[k][i][j] << " at index: " <<"[" << k << "] [" << i << "] [" << j << "]" << endl; // printing out each pixel intensities
				}
			}
		}

		// copy columns work! =]
		for (int k = 0; k < samples; k++)
		{
			for (int i = 0; i < offsetHeight; i++) // 0~2
			{
				for (int row = offsetWidth; row < offsetWidth + height; row++) // 3~514
				{

					// copy left columns
					mirrorImage[k][row][i] = inputImage[k][row - offsetHeight][offsetWidth - i];
					// copy right columns
					mirrorImage[k][row][i + width + offsetWidth] = inputImage[k][row - offsetHeight][width - i - 2];
				}
			} // end for
		} // end for

		for (int k = 0; k < samples; k++)
		{
			for (int i = 0; i < offsetHeight; i++)
			{
				for (int col = 0; col < width + (offsetWidth * 2); col++)
				{
            
					// copy top rows
					mirrorImage[k][i][col] = mirrorImage[k][(offsetHeight * 2) - i][col];
					// copy bottom rows
					mirrorImage[k][i + height + offsetHeight][col] = mirrorImage[k][height + offsetHeight - 2 - i][col];
				}
			}
		}
		
		// Initializing the 1 dimentional array buffer
		unsigned char* mirrorImageBuffer = new unsigned char[samples * width * height];

		// converting the 3 dimentional array to a 1 dimentional buffer for C#
		for (int k = 0; k < samples; k++) // the samples per pixel
		{
			for (int i = 0; i < height; i++) // the height (or column) of an image
			{
				for (int j = 0; j < width; j++) // the width (or row) of an image
				{            
					mirrorImageBuffer[((width * samples) * i) + (samples * j) + k] = mirrorImage[k][i][j]; // assigining all values
				}
			}
		}

		// outputs the 1 dimentional array buffer.
		return mirrorImageBuffer;
	}
}
