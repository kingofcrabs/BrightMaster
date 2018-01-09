#include "stdafx.h"
#include "EngineImpl.h"

using namespace std;
using namespace cv;
static string dbgFolder = "d:\\temp\\";
static int innerRadius = 30;
static Mat globalImg;
static vector<pair<int, int>> globalPts;
static vector<pair<int, int>> globalHullPts;

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

static void on_trackbar(int val, void* parent)
{
	globalPts.clear();
	EngineImpl* pEngine = (EngineImpl*)parent;
	pEngine->FindRectImpl(globalImg, globalPts,globalHullPts);
}


void EngineImpl::FindRectImpl(Mat& img, vector<pair<int, int>>& ptPairs, std::vector<std::pair<int, int>>&hullPairs, bool mannualThreshold)
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
		//line(drawing, pt, ptEnd, color);
		hullPts.push_back(pt);
	}
		
	double epsilon = 1;
	vector<Point2f> veryApprox;
	hullPairs.clear();
	approxPolyDP(hullPts, veryApprox, epsilon, true);
	for (int i = 0; i < veryApprox.size(); i++)
	{
		Point2f ptStart = veryApprox[i];
		Point2f ptEnd = veryApprox[(i + 1) % veryApprox.size()];
		line(drawing, ptStart, ptEnd, color, 1);
		hullPairs.push_back(make_pair(ptStart.x, ptStart.y));
	}
	if (!mannualThreshold)
	{
		imshow("threshold", drawing);
		//imwrite("d:\\test.jpg", drawing);
	}
	
	//cvWaitKey(0);
	epsilon = 0.1*arcLength(hullPts, true);
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
	
		

}

void EngineImpl::FindRect(std::string sFile, int& defaultThreshold, vector<pair<int, int>>& ptPairs,
	std::vector<std::pair<int, int>>&hullPts,bool mannualThreshold)
{
	thresholdVal = defaultThreshold;
	auto img = cv::imread(sFile, 0);
	Mat thresholdImg;
	globalImg = img.clone();
	cv::threshold(img, thresholdImg, thresholdVal, 255, CV_THRESH_BINARY);
	if (!mannualThreshold)
	{
		FindRectImpl(img, ptPairs, hullPts, mannualThreshold);
		return;
	}
	string winName = "threshold";
	string sliderName = "slider";
	namedWindow(winName, WINDOW_NORMAL);
	imshow(winName, thresholdImg);
	//resizeWindow(winName, 800, 800 * img.rows / img.cols);
	createTrackbar(sliderName, winName, &thresholdVal, 255, on_trackbar, this);
	FindRectImpl(img, ptPairs, hullPts);
	globalPts = ptPairs;
	globalHullPts = hullPts;
	waitKey(0);
	defaultThreshold = thresholdVal;
	ptPairs = globalPts;
	hullPts = globalHullPts;
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






