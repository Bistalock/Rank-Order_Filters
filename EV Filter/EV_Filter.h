///*
// *      Date Written: October 17, 2014
// *      Code Written by: Mr. Gabriel Del Pino. Portions inspired by Hiroyuki Plumlee.
// */  
//
//#pragma once
//#using <mscorlib.dll>
//
//using namespace System;
////
////namespace EV_Filter {
////
////	public ref class EV_Filter
////	{
////		__declspec(dllexport) array<Byte,3>^ getImage(array<Byte,3>^ inputImage, int height, int width, int samples, int kernel, int EV); //wchar_t *phrase);
////	};
////}
//
//extern "C"
//{
//	__declspec(dllexport) class EV_Filter
//	{
//	public:
//		//EV_Filter(void);
//		//~EV_Filter(void);
//
//		// function to call the EV filter
//		static int getImage(int height, int width, int samples, int kernel, int EV);
//
//		int ffs(int wtf)
//		{
//			int omg;
//
//			omg = wtf + 1;
//
//			return omg;
//		}
//	//protected:
//	//	!EV_Filter();
//
//	private:
//		int samples; // the bits per pixel of an image
//		int height; // the height of an image
//		int width; // The width of an image	
//		int kernel; // the window to be used
//		int EV; // the specific EV parameter
//		//array<int> inputImage; // the image to be processed
//	};
//}