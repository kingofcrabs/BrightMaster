#pragma once
#include "stdafx.h"
#include <opencv2/opencv.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>
using namespace cv;


struct LineSegment
{
	cv::Point2f ptStart;
	cv::Point2f ptEnd;
	LineSegment(cv::Point2f start, cv::Point2f end)
	{
		ptStart = start;
		ptEnd = end;

	}

	LineSegment()
	{

	}
};

class EngineImpl
{
public:
	double f;         //len's focus
	double pixelUnit; //camera's pixel unit um

	EngineImpl();
	

	void Convert2PesudoColor(std::string srcFile, std::string destFile);
	void FindRect(std::string sFile, int& defaultThreshold, std::vector<std::pair<int, int>>& pts, bool mannualThreshold);
	void FindRectImpl(Mat& img, std::vector<std::pair<int, int>>& ptPairs, bool autoFindBoundary = false);
	std::vector<std::vector<Point>> contours;
	int max, min;
private:
	int FindThreshold(Mat& img);
	
	double  GetDistance(double x1, double y1, double x2, double y2);
	void GothroughImage(Mat& src);
	void SaveHistogram(Mat& src);
	int thresholdVal;
	
};

