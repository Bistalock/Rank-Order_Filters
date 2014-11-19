/*
 *      Date Written: October 17, 2014
 *      Code Written by: Mr. Gabriel Del Pino. Portions inspired by Hiroyuki Plumlee.
 */  

#include "stdafx.h"
#include <cmath>


extern "C"
{
	__declspec(dllexport) int EV_Filter()
	{
		int test = 0;
		return test;
	}

	__declspec(dllexport) int EV_Filter2()
	{
		int test = 99;
		return test;
	}


	__declspec(dllexport) int getImage(int height, int width, int samples, int kernel, int EV)//wchar_t *phrase)
	{
		int offset = (kernel - 3) / 2 + 1; // calculation of the center 
           

		#pragma region Mirroring the image
		// Mirrorimage before the creation of the kernel
		int MirroredHeight = height + (offset * 2);
		int MirroredWidth = width + (offset * 2);
		unsigned char* inputImage = new unsigned char[samples, MirroredHeight, MirroredWidth]();

		unsigned char* mirrorImage = new unsigned char[samples, MirroredHeight, MirroredWidth]();

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

		int* window = new int[kernel, kernel](); // the kernel window for each sample


		int* processedImage = new int[samples, height, width](); // the resulting image after the filtering

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
					processedImage[k, i - offset, j - offset] = int(mean);                      
				}   
			}
		} // end of EV filtering   
		#pragma endregion

		int lol = 99;
		return lol;
	}



	//__declspec(dllexport) int getImage(unsigned char*** inputImage, int height, int width, int samples, int kernel, int EV)//wchar_t *phrase)
	//{
	//	int offset = (kernel - 3) / 2 + 1; // calculation of the center 
	//	//inputImage = new int[];

	//	#pragma region Mirroring the image
	//	// Mirrorimage before the creation of the kernel
	//	int MirroredHeight = height + (offset * 2);
	//	int MirroredWidth = width + (offset * 2);

	//	//array<Byte,3>^ inputImage = gcnew array<Byte,3>(samples, height, width);
	//	//array<Byte,3>^ mirrorImage = gcnew array<Byte,3>(samples, MirroredHeight, MirroredWidth);

	//	unsigned char*** mirrorImage = new unsigned char**[samples];

	//	// copying the original image to the larger mirrored image
	//	for (int k = 0; k < samples; k++)
	//	{
	//		mirrorImage[samples] = new unsigned char*[height];
	//		for (int i = 0; i < height; i++)
	//		{
	//			mirrorImage[samples][height] = new unsigned char[width];
	//			for (int j = 0; j < width; j++)
	//			{          
	//				mirrorImage[k, i + offset, j + offset] = inputImage[k, i, j];
	//			}
	//		}
	//	}

	//	// copy columns work! =]
	//	for (int k = 0; k < samples; k++)
	//	{
	//		for (int i = 0; i < offset; i++) // 0~2
	//		{
	//			for (int row = offset; row < offset + height; row++) // 3~514
	//			{

	//				// copy left columns
	//				mirrorImage[k, row, i] = inputImage[k, row - offset, offset - i];
	//				// copy right columns
	//				mirrorImage[k, row, i + width + offset] = inputImage[k, row - offset, width - i - 2];
	//			}
	//		} // end for
	//	} // end for

	//	for (int k = 0; k < samples; k++)
	//	{
	//		for (int i = 0; i < offset; i++)
	//		{
	//			for (int col = 0; col < width + (offset * 2); col++)
	//			{
 //           
	//				// copy top rows
	//				mirrorImage[k, i, col] = mirrorImage[k, (offset * 2) - i, col];
	//				// copy bottom rows
	//				mirrorImage[k, i + height + offset, col] = mirrorImage[k, height + offset - 2 - i, col];
	//			}
	//		}
	//	}
	//	#pragma endregion

	//	//array<Byte,2>^ window = gcnew array<Byte,2>(kernel, kernel); // the kernel window for each sample

	//	unsigned char** window = new unsigned char*[kernel];
	//	for (int i = 0; i < height; i++)
	//	{
	//		window[kernel] = new unsigned char[kernel];
	//	}

	//	//array<Byte,3>^ processedImage = gcnew array<Byte,3>(samples, height, width); // the resulting image after the filtering

	//	unsigned char*** processedImage = new unsigned char**[samples];

	//	// Allocation of the processed image
	//	for (int k = 0; k < samples; k++)
	//	{
	//		processedImage[samples] = new unsigned char*[height];
	//		for (int i = 0; i < height; i++)
	//		{
	//			processedImage[samples][height] = new unsigned char[width];
	//		}
	//	}
	//	#pragma region Applying kernel to every pixel in image
	//	// Consider adding if statement for samples
	//	for (int k = 0; k < samples; k++)
	//	{ 
	//		for (int i = offset; i < MirroredHeight - offset; i++)
	//		{
	//			for (int j = offset; j < MirroredWidth - offset; j++)
	//			{                    
 //                                  
	//				#pragma region Kernel
	//				// Begin creation of kernel
	//				double addedsum = 0;
	//				double selected = 0;

	//				int mean;

	//				int temp = mirrorImage[k, i, j];

	//				//for (int l = 0; l < kernel; l++) // kernel height
	//				//{
	//				//    for (int m = 0; m < kernel; m++) // kernel width
	//				//    {
	//				//        window[l, m] = mirrorImage[k, i - offset + l, j - offset + m]; // applying the intensity values to the kernel window
	//				//	  }
	//				//}

	//				// loops collecting the pixels within the kernel meeting the EV criterion and adding to addedsum temp var
	//				for (int l = 0; l < kernel; l++) // kernel height
	//				{
	//					for (int m = 0; m < kernel; m++) // kernel width
	//					{
	//						window[l, m] = mirrorImage[k, i - offset + l, j - offset + m]; // applying the intensity values to the kernel window
	//						//if (l != offset || m != offset) // as long as the pixel to be processed in not the center pixel
	//						//{
	//							if (abs(window[l, m] - temp) <= EV) // obtaning the absolute difference of the pixel within the kernel to the offset
	//							{                                    
	//								selected++; // incrementing the counter for the pixel within the kernel
	//								addedsum += window[l, m]; // adding the intenities within the kernel to one variable
	//							}
	//						//}
	//					}
	//				}

	//				// calulating the mean of kernel                      
	//				// if no pixels in kernel matched EV parameter, set to the center pixel
	//				if (selected == 0)
	//				{
	//					mean = window[offset, offset];
	//				}
	//				else
	//				{
	//					mean = int(addedsum / selected);
	//				}

	//				// tranfering the mean values to the resulting image
	//				processedImage[k, i - offset, j - offset] = int(mean);

	//				#pragma endregion                        
	//			}   
	//		}
	//	} // end of EV filtering   
	//	#pragma endregion

	//	//  Deallocate 3D array
	//	for(int i = 0; i < samples; i++)
	//	{
	//		for(int j = 0; j < width; j++)
	//		{
	//			delete[] mirrorImage[i][j];
	//		}
 //
	//		delete[] mirrorImage[i];
	//	}
	//	delete[] mirrorImage;

	//	//return processedImage;
	//	int lol = 99;
	//	return lol;
	//}
}
