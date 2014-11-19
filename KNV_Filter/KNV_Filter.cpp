//
//  KNVFilter.cpp
//  KNV Filter
//
//  Created by Lucas Allgood and Daniel Contreras on 11/6/14.
//
//


#include "stdafx.h"
#include <windows.h>
#include <cstdio>
#include <cmath>


//Simple global enum to demonstrate a way of implementing various methods of filtering.
enum Method {
    MED, MEAN
};

//This function assumes the img array to be in the format of img[CHANNEL][COLUMN][ROW]
 __declspec(dllexport) unsigned char*** processKNV(unsigned char img[][maxCol][maxRow], int kernelSize, int maxRow, int maxCol, int numChannels) { //, Method m) { This is an optional method parameter incase we want to implement a global enumeration as demonstrated above.
    
    int offset = (kernelSize - 1) / 2;
    
    for(int i = 0; i < numChannels; i++) {
        //For the next two for loops, I believe I have implemented the offset correctly, but I could be wrong. Please let me know if you find an error.
        for(int j = offset; j < maxRow - offset; j++) {
            for(int k = offset; k < maxCol - offset; k++) {
                
                //BUILD THE KERNEL//
                int currentKernelValues[kernelSize*kernelSize]; //Holds all values of the kernel
                int currentX = k - offset; //currentX is in the top left pixel of the kernel
                int rightCol = k + offset; //rightCol is in reference to the kernel's right col
                
                int currentY = j - offset; //currentY is in the top left pixel of the kernel
                int bottomRow = j + offset; //bottomRow is in reference to the kernel's bottom row
                
                int kv = 0; // kv stands for "Kernel Value"; indicates the value of each pixel in the currentKernalValues array
                
                for(currentX; currentX < rightCol; currentX++) {
                    for(currentY; currentY < bottomRow; currentY++) {
                        currentKernelValues[kv++] = img[i][currentX][currentY];
                    }
                }
                //KERNEL STORED IN currentKernelValues//
                
                //The next two lines are what would be inside a switch case based on the desired method for the KNV filter to work.
                int median = getMedian(currentKernelValues);
                
                img[i][k][j] = abs(img[i][k][j] - median); // Notice the appearance of j and k being backwards, but k and j correspond to x and y, respectively.
            }
        }
    }
    return img;
}

//This function was, admittedly, found online, and it uses the algorithm include, I believe.
int getMedian(int currentKernalValues[]) {
    int elements = sizeof(currentKernalValues) / sizeof(currentKernalValues[0]);
    std::sort(currentKernalValues, currentKernalValues + elements);
    
    return currentKernalValues[(sizeof(currentKernalValues)- 1) / 2 + 1];
}


int main() {
	int kernelSize = 3; // 3x3 kernel size
	int maxrow = 10; // width of 10
	int maxcol = 10; // height of 10
	int numChannels = 3; // 3 samples per pixel
	
	unsigned char img[numChannels][maxcol][maxrow]; // declaring the managed 3 dimentional array

	for (int k = 0; k < numChannels; k++){
		for (int i = 0; i < maxcol; i++){
			for (int j = 0; j < maxrow; j++){        
				img[k][i][j] = 30; // allocating the intensity value of 30 for each element
			}
		}
	}

	// calling the KNV processing function and assigning the value
	array<Byte,3>^ result = processKNV(img,kernelSize,maxrow,maxcol,numChannels);

	return 0;
};