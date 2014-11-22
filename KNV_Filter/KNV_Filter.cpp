//
//  KNV_Filter.cpp
//  KNV Filter
//
//  Created by Lucas Allgood and Daniel Contreras on 11/6/14.
//  - Modified by Gabriel Del Pino on 11/18/14.
//
//


#include "stdafx.h"
#include <windows.h>
#include <cstdio>
#include <algorithm>
#include <cmath>

using namespace std;

//Simple global enum to demonstrate a way of implementing various methods of filtering.
enum Method {
    MED, MEAN
};

extern "C" {
	//This function assumes the img array to be in the format of img[CHANNEL][COLUMN][ROW]
	 __declspec(dllexport) int* processKNV(int* imgBuffer, int kernelSize, int maxRow, int maxCol, int numChannels) { //, Method m) { This is an optional method parameter incase we want to implement a global enumeration as demonstrated above.
    
		int offset = (kernelSize - 1) / 2;
    
		int*** img = new int**[numChannels];

			// Converting the 1 dimentional array to a 3 dimentional array for C++
			for (int k = 0; k < numChannels; k++)
			{
				img[k] = new int*[maxCol];
				for (int i = 0; i < maxCol; i++)
				{
					img[k][i] = new int[maxRow];
					for (int j = 0; j < maxRow; j++)
					{            
						img[k][i][j] = imgBuffer[((maxRow * numChannels) * i) + (3 * j) + k];
					}
				}
			}

		for(int i = 0; i < numChannels; i++) {
			//For the next two for loops, I believe I have implemented the offset correctly, but I could be wrong. Please let me know if you find an error.
			for(int j = offset; j < maxRow - offset; j++) {
				for(int k = offset; k < maxCol - offset; k++) {
                
				//BUILD THE KERNEL//
				int** currentKernelValues = new int*[kernelSize];

				// initiallizing the array values to zero
				for (int i = 0; i < kernelSize; i++)
				{
					currentKernelValues[i] = new int[kernelSize];
					for (int j = 0; j < kernelSize; j++)
						{            
							currentKernelValues[i][j] = 0;
						}
				}
				int currentX = k - offset; //currentX is in the top left pixel of the kernel
				int rightCol = k + offset; //rightCol is in reference to the kernel's right col
                
				int currentY = j - offset; //currentY is in the top left pixel of the kernel
				int bottomRow = j + offset; //bottomRow is in reference to the kernel's bottom row
                
				int kv = 0; // kv stands for "Kernel Value"; indicates the value of each pixel in the currentKernalValues array
                
				// There is an error in this loop
				for(currentX; currentX < rightCol; currentX++) {
					for(currentY; currentY < bottomRow; currentY++) {
						currentKernelValues[kv++] = img[i][currentX][currentY]; // cannot assign currentKernelvalues[] from img[][][]. is it currentKernelValues[][]?
					}
				}
				//KERNEL STORED IN currentKernelValues//
                
				//The next two lines are what would be inside a switch case based on the desired method for the KNV filter to work.
				int median = getMedian(currentKernelValues);
                
				img[i][k][j] = abs(img[i][k][j] - median); // Notice the appearance of j and k being backwards, but k and j correspond to x and y, respectively.
				}
			}
		}

		int*** img = new int**[numChannels];

			// Converting the 1 dimentional array to a 3 dimentional array for C++
			for (int k = 0; k < numChannels; k++)
			{
				img[k] = new int*[maxCol];
				for (int i = 0; i < maxCol; i++)
				{
					img[k][i] = new int[maxRow];
					for (int j = 0; j < maxRow; j++)
					{            
						imgBuffer[((maxRow * numChannels) * i) + (3 * j) + k] = img[k][i][j];
					}
				}
			}

		return imgBuffer;
	}
}

//This function was, admittedly, found online, and it uses the algorithm include, I believe.
int getMedian(int** currentKernalValues[]) {
    int elements = sizeof(currentKernalValues) / sizeof(currentKernalValues[0]);
    std::sort(currentKernalValues, currentKernalValues + elements);
    
    return currentKernalValues[(sizeof(currentKernalValues)- 1) / 2 + 1];
}