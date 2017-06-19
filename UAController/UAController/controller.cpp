#include "controller.h"



UaSystem* uasystem = NULL;
UaDevice* device = NULL;
int average_count;
bool Init()
{

	if (Initialize("..\\param", &uasystem, &device) == UA_FAILURE)
		return false;

	// Set measurement conditions manually.
	UaDeviceProperty* property = uaCreateDeviceProperty(device->type);
	if (uaIsUA10(device->type))
	{
		property->exposure_time[1] = 20.0;
	}
	else if (uaIsUA200(device->type))
	{
		int gain[3] = { 1, 5, 1 };
		UaNDFilterType nd_filter[3] =
		{ UA_NO_ND_FILTER, UA_ND_FILTER_ONE_TENTH, UA_NO_ND_FILTER };
		double exposure_time[3] = { 80.0, 40.0, 120.0 };
		for (int i = 0; i < 3; i++) {
			property->gain[i] = gain[i];
			property->nd_filter[i] = nd_filter[i];
			property->exposure_time[i] = exposure_time[i];
		}
	}
	CALL_UA_FUNC(uaSetDeviceProperty(device, property));
	CALL_UA_FUNC(uaDestroyDeviceProperty(property));
}

template <typename T>
void FillData(float* dst, int width, int height, T* data)
{
	for (int y = 0; y < height; y++)
	{
		for (int x = 0; x < width; x++) 
		{
			int index = y * width + x;
			dst[index] << data[index];
		}
	}
}


void AcquireImage(float* x, float *y, float *z,char* savePath)
{
	for (int i = 0; i < 100; i++)
		x[i] = i;
	//UaCaptureData* capture_data = uaCreateCaptureData(device->type);
	//UaXYZImage* xyz_image = uaCreateXYZImage(device->type,
	//	UA_DATA_TRISTIMULUS_XYZ);

	//CALL_UA_FUNC(uaStartCapture(device));
	//for (int i = 0; i < 5; i++)
	//{
	//	CALL_UA_FUNC(uaCaptureImage(device, UA_CAPTURE_FILTER_XYZ, average_count,
	//		capture_data));
	//	CALL_UA_FUNC(uaToXYZImage(device, capture_data, xyz_image));
	//	CALL_UA_FUNC(uaSaveMeasurementData(savePath, NULL, xyz_image, NULL));
	//}
	//CALL_UA_FUNC(uaStopCapture(device));

	//// delete buffer
	//CALL_UA_FUNC(uaDestroyXYZImage(xyz_image));
	//CALL_UA_FUNC(uaDestroyCaptureData(capture_data));
}

void UnInit()
{
	if (device != NULL)
		CALL_UA_FUNC(uaCloseDevice(device));
	CALL_UA_FUNC(uaFinalize(uasystem));
}

//
//template <typename T>
//bool WriteToCSV(const std::string& filename, T* data, int width, int height)
//{
//	std::ofstream ofs(filename.c_str(), std::ios_base::trunc);
//	for (int y = 0; y < height; y++) {
//		for (int x = 0; x < width; x++) {
//			ofs << data[y * width + x];
//			if (x != width - 1)
//				ofs << ",";
//		}
//		if (y != height - 1)
//			ofs << "\n";
//	}
//	return true;
//}
//
//template <typename T>
//std::string ToString(const T& value)
//{
//	std::stringstream s;
//	s << value;
//	return s.str();
//}
//
//std::string Zfill(const std::string& str, const int width)
//{
//	std::stringstream s;
//	s << std::setw(width) << std::setfill('0') << str;
//	return s.str();
//}
//
//int main(int argc, char* argv[])
//{
//	UaSystem* system = NULL;
//	UaDevice* device = NULL;
//
//	if (Initialize("d:\\param", &system, &device) == UA_FAILURE)
//		return 1;
//
//	// Optimize measurement condition.
//	UaDeviceProperty* property = uaCreateDeviceProperty(device->type);
//	UaOptimizationCondition cond;
//	if (uaIsUA10(device->type)) {
//		cond = UA_OPTIMIZE_COND_GAIN_FIX_ND_FIX;
//	}
//	else if (uaIsUA200(device->type)) {
//		cond = UA_OPTIMIZE_COND_GAIN_OPTIMUM_ND_OPTIMUM;
//	}
//	CALL_UA_FUNC(uaOptimizeDeviceProperty(device, cond, property));
//	CALL_UA_FUNC(uaSetDeviceProperty(device, property));
//
//	// Get optimal average count.
//	int average_count;
//	CALL_UA_FUNC(uaGetOptimumAverageCount(device, 0, property->exposure_time[1],
//		&average_count));
//	CALL_UA_FUNC(uaDestroyDeviceProperty(property));
//
//	UaCaptureData* capture_data = uaCreateCaptureData(device->type);
//	UaXYZImage* xyz_image = uaCreateXYZImage(device->type,
//		UA_DATA_TRISTIMULUS_XYZ);
//
//	CALL_UA_FUNC(uaStartCapture(device));
//	for (int i = 0; i < 5; i++)
//	{
//		CALL_UA_FUNC(uaCaptureImage(device, UA_CAPTURE_FILTER_XYZ, average_count,
//			capture_data));
//		CALL_UA_FUNC(uaToXYZImage(device, capture_data, xyz_image));
//
//		// Base color correction
//		//if (uaIsUA10(device->type))
//		//  CALL_UA_FUNC(uaCorrectColor(device, xyz_image, UA_COLOR_CORRECTION_LED));
//
//		// User color correction
//		// CALL_UA_FUN(uaCorrectXYZImageLevel(xyz_image, 1.04, 0.98, 1.09));
//		std::string dir = Zfill(ToString(i), 3);
//		_mkdir(dir.c_str());
//
//		WriteToCSV(dir + "\\X.csv", xyz_image->X, xyz_image->width, xyz_image->height);
//		WriteToCSV(dir + "\\Y.csv", xyz_image->Y, xyz_image->width, xyz_image->height);
//		WriteToCSV(dir + "\\Z.csv", xyz_image->Z, xyz_image->width, xyz_image->height);
//
//		CALL_UA_FUNC(uaSaveMeasurementData(dir.c_str(), NULL, xyz_image, NULL));
//	}
//	CALL_UA_FUNC(uaStopCapture(device));
//
//	// delete buffer
//	CALL_UA_FUNC(uaDestroyXYZImage(xyz_image));
//	CALL_UA_FUNC(uaDestroyCaptureData(capture_data));
//
//	CALL_UA_FUNC(uaCloseDevice(device));
//	CALL_UA_FUNC(uaFinalize(system));
//}
