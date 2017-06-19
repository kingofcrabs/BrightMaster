#ifndef UAContoller  
#define UAContoller  
#if (defined WIN32 || defined _WIN32 || defined WINCE) && defined MYDLL_EXPORTS  
#  define CS_EXPORTS __declspec(dllexport)  
#else  
#  define CS_EXPORTS  
#endif  

#include "ua_core2.h"
#include "ua_core2_util.h"

#include <direct.h>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <sstream>
#include <string>



extern "C"  __declspec(dllexport) bool Init();
extern "C"  __declspec(dllexport) void AcquireImage(float* x, float *y, float *z, char* path);
extern "C"  __declspec(dllexport) void UnInit();


#endif