/*
 *      Project Name: Mirror Image function
 *      Date Written: October 17, 2014
 *      Created by: Hiroyuki Plumlee. Rewritten by Gabriel Del Pino.
 */ 

#include "stdafx.h"

extern "C"
{
		// Function requires a 1 dimentional buffer of an image, along as various parameters
	// The image array buffer is later converted to a 3 dimentional dynamic array in the format of inputImage[Samples][Height][Width]
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
				}
			}
		}

		// copy columns work! =]
		for (int k = 0; k < samples; k++) // 0~3 Samples per pixel of mirrorImge
		{
			for (int col = offsetHeight; col < offsetHeight + height; col++) // 3~514 Height of mirrorImage
			{
				for (int i = 0; i < offsetWidth; i++) // 0~2 Width of mirrorImage
				{
					// copy left columns
					mirrorImage[k][col][i] = inputImage[k][col - offsetHeight][offsetWidth - i];
					// copy right columns
					mirrorImage[k][col][i + width + offsetWidth] = inputImage[k][col - offsetHeight][width - i - 2];
				}
			} // end for
		} // end for

		for (int k = 0; k < samples; k++) // 0~3 Samples per pixel of mirrorImge
		{
			for (int i = 0; i < offsetHeight; i++) // 0~2 Height of mirrorImage
			{
				for (int row = 0; row < width + (offsetWidth * 2); row++) // 0~514 Width if mirrorImage
				{
            
					// copy top rows
					mirrorImage[k][i][row] = mirrorImage[k][(offsetHeight * 2) - i][row];
					// copy bottom rows
					mirrorImage[k][i + height + offsetHeight][row] = mirrorImage[k][height + offsetHeight - 2 - i][row];
				}
			}
		}
		
		// Initializing the 1 dimentional array buffer
		unsigned char* mirrorImageBuffer = new unsigned char[samples * MirroredWidth * MirroredHeight];

		// converting the 3 dimentional array to a 1 dimentional buffer for C#
		for (int k = 0; k < samples; k++) // the samples per pixel
		{
			for (int i = 0; i < MirroredHeight; i++) // the height (or column) of an image
			{
				for (int j = 0; j < MirroredWidth; j++) // the width (or row) of an image
				{            
					mirrorImageBuffer[((MirroredWidth * samples) * i) + (samples * j) + k] = mirrorImage[k][i][j]; // assigining all values
				}
			}
		}

		// outputs the 1 dimentional array buffer.
		return mirrorImageBuffer;
	}
}
