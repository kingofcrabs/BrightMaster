#include "stdafx.h"
#include "EngineImpl.h"

using namespace std;
using namespace cv;
static string dbgFolder = "d:\\temp\\";
static int innerRadius = 30;
static Mat globalImg;
static vector<pair<int, int>> globalPts;
EngineImpl::EngineImpl()
{
	f = 20/1000; //mm
	thresholdVal = 40;
	//1536 * 1024
	min = 20;
	max = 1000;
	pixelUnit = 9 / 1000000; //um
}

void EngineImpl::GothroughImage(Mat& src)
{
	int height = src.rows;
	int width = src.cols;
	int channels = src.channels();
	int nc = width * channels;

	for (int y = 0; y < height; y++)
	{
		uchar *data = src.ptr(y);
		for (int x = 0; x < width; x++)
		{
			int xStart = x*channels;
			for (int i = 0; i< channels; i++)
				data[xStart + i] = 0;
		}
	}
	return;
}




void EngineImpl::SaveHistogram(Mat& src)
{
	int height = src.rows;
	int width = src.cols;
	int channels = src.channels();
	int nc = width * channels;
	vector<int> grayCnts(255);
	for (int y = 0; y < height; y++)
	{
		uchar *data = src.ptr(y);
		for (int x = 0; x < width; x++)
		{
			int xStart = x*channels;
			for (int i = 0; i< channels; i++)
				grayCnts[data[xStart + i]]++;
		}
	}
	return;
}

#include "opencv2/core/utility.hpp"  












//d = (| (x_2 - x_1)x(x_1 - x_0) | ) / (| x_2 - x_1 | )



//static bool IsDimodal(vector<double> HistGram)       // 检测直方图是否为双峰的
//{
//	// 对直方图的峰进行计数，只有峰数位2才为双峰 
//	int Count = 0;
//	for (int Y = 1; Y < 255; Y++)
//	{
//		if (HistGram[Y - 1] < HistGram[Y] && HistGram[Y + 1] < HistGram[Y])
//		{
//			Count++;
//			if (Count > 2)
//				return false;
//		}
//	}
//	return Count == 2;
//}
//
//static int GetMinimumThreshold(vector<int>& HistGram)
//{
//	int Y, Iter = 0;
//	vector<double> HistGramC;           // 基于精度问题，一定要用浮点数来处理，否则得不到正确的结果
//	vector<double> HistGramCC;          // 求均值的过程会破坏前面的数据，因此需要两份数据
//	for (Y = 0; Y < 256; Y++)
//	{
//		HistGramC[Y] = HistGram[Y];
//		HistGramCC[Y] = HistGram[Y];
//	}
//
//	// 通过三点求均值来平滑直方图
//	while (!IsDimodal(HistGramCC))                                        // 判断是否已经是双峰的图像了      
//	{
//		HistGramCC[0] = (HistGramC[0] + HistGramC[0] + HistGramC[1]) / 3;                 // 第一点
//		for (Y = 1; Y < 255; Y++)
//			HistGramCC[Y] = (HistGramC[Y - 1] + HistGramC[Y] + HistGramC[Y + 1]) / 3;     // 中间的点
//		HistGramCC[255] = (HistGramC[254] + HistGramC[255] + HistGramC[255]) / 3;         // 最后一点
//		HistGramC.clear();
//		HistGramC.insert(HistGramC.end(),HistGramCC.begin(), HistGramCC.end());
//		Iter++;
//		if (Iter >= 1000) return -1;                                                   // 直方图无法平滑为双峰的，返回错误代码
//	}
//	// 阈值极为两峰之间的最小值 
//	bool Peakfound = false;
//	for (Y = 1; Y < 255; Y++)
//	{
//		if (HistGramCC[Y - 1] < HistGramCC[Y] && HistGramCC[Y + 1] < HistGramCC[Y]) Peakfound = true;
//		if (Peakfound == true && HistGramCC[Y - 1] >= HistGramCC[Y] && HistGramCC[Y + 1] >= HistGramCC[Y])
//			return Y - 1;
//	}
//	return -1;
//}
//
//int EngineImpl::FindThreshold(Mat& img)
//{
//	vector<int> histogram;
//	//256个，范围是0，255.  
//	const int histSize = 256;
//	float range[] = { 0, 255 };
//	const float *ranges[] = { range };
//	const int channels = 0;
//	Mat hist;
//	calcHist(&img, 1, 0, Mat(), hist, 1, &histSize, 0);
//	vector<int> vals;
//	for (int i = 0; i < 256; i++)
//	{
//		int val = saturate_cast<int>(hist.at<float>(i));
//		vals.push_back(val);
//	}
//	return GetMinimumThreshold(vals);
//}

static void on_trackbar(int val, void* parent)
{
	globalPts.clear();
	EngineImpl* pEngine = (EngineImpl*)parent;
	pEngine->FindRectImpl(globalImg, globalPts);
}


void EngineImpl::FindRectImpl(Mat& img, vector<pair<int, int>>& ptPairs, bool mannualThreshold)
{
	std::vector< std::vector<cv::Point> > allContours;
	Mat thresholdImg;
	Mat drawing;
	cvtColor(img, drawing, CV_GRAY2BGR);
	cv::threshold(img, thresholdImg, thresholdVal, 255, CV_THRESH_BINARY);
	//imshow("threshold", thresholdImg);
	//cvWaitKey(0);
	cv::findContours(thresholdImg, allContours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
	int width = img.size().width;
	int height = img.size().height;
	int valid = (width + height) / 3;
	int min = 0;
	int index = -1;
	int allmostFull = (width + height) * 2 - 50;

	for (size_t i = 0; i<allContours.size(); i++)
	{
		int contourSize = allContours[i].size();
		if (contourSize <= valid)
			continue;

		if (contourSize > allmostFull)
			continue;
		if (contourSize > min)
		{
			min = contourSize;
			index = i;
		}
	}
	if (index == -1)
		return;
	vector<int> hull;
	convexHull(Mat(allContours[index]), hull, false);
	int hullcount = (int)hull.size();
	vector<Point2f> hullPts;
	cvtColor(img, drawing, CV_GRAY2BGR);
	Scalar color = Scalar(255, 0, 0);
	for (int i = 0; i < hullcount; i++)
	{
		Point pt = allContours[index][hull[i]];
		Point ptEnd = allContours[index][hull[(i + 1) % hullcount]];
		line(drawing, pt, ptEnd, color);
		hullPts.push_back(pt);
	}
	/*imshow("hull", drawing);
	cvWaitKey(0);*/
	double epsilon = 0.1*arcLength(hullPts, true);
	vector<Point2f> approx;
	approxPolyDP(hullPts, approx, epsilon, true);
	for (int i = 0; i < approx.size(); i++)
	{
		Point2f ptStart = approx[i];
		Point2f ptEnd = approx[(i + 1) % approx.size()];
		line(drawing, ptStart, ptEnd, color, 1);
		ptPairs.push_back(make_pair(ptStart.x, ptStart.y));
	}
	//imshow("approx", drawing);
	
	if (mannualThreshold)
	{
		imshow("threshold", drawing);
#if _DEBUG
		imwrite("d:\\test.jpg", drawing);
#endif
	}
		

}

void EngineImpl::FindRect(std::string sFile, int& defaultThreshold, vector<pair<int, int>>& ptPairs, bool mannualThreshold)
{
	thresholdVal = defaultThreshold;
	auto img = cv::imread(sFile, 0);
	Mat thresholdImg;
	globalImg = img.clone();
	cv::threshold(img, thresholdImg, thresholdVal, 255, CV_THRESH_BINARY);
	if (!mannualThreshold)
	{
		FindRectImpl(img, ptPairs, mannualThreshold);
		return;
	}
	string winName = "threshold";
	string sliderName = "slider";
	namedWindow(winName, WINDOW_NORMAL);
	imshow(winName, thresholdImg);
	resizeWindow(winName, 800, 800 * img.rows / img.cols);
	createTrackbar(sliderName, winName, &thresholdVal, 255, on_trackbar, this);
	FindRectImpl(img, ptPairs);
	globalPts = ptPairs;
	waitKey(0);
	defaultThreshold = thresholdVal;
	ptPairs = globalPts;
}



std::string WStringToString(const std::wstring &wstr)
{
	std::string str(wstr.length(), ' ');
	std::copy(wstr.begin(), wstr.end(), str.begin());
	return str;
}


double  EngineImpl::GetDistance(double x1, double y1, double x2, double y2)
{
	double xx = (x1 - x2)*(x1 - x2);
	double yy = (y1 - y2)*(y1 - y2);
	return sqrt(xx + yy);
}




void EngineImpl::Convert2PesudoColor(std::string srcFile, std::string destFile)
{
	Mat orgImg = imread(srcFile);
	Mat color;
	applyColorMap(orgImg, color, COLORMAP_JET);
	//LUT()
	imwrite(destFile,color);
}






