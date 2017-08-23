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
	
	double CalculateGlare(std::string sFile,std::vector<Rect2f> rc);
	void Convert2PesudoColor(std::string srcFile, std::string destFile);
	void FindRect(std::string sFile, std::vector<std::pair<int, int>>& pts);
	int AdaptiveThreshold(uchar*, int width, int height,std::vector<uchar>& vector);
	int SearchLights(uchar* pdata, int width, int height, int min, int max, std::vector<std::vector<cv::Point>>& contours);
	std::vector<std::vector<Point>> contours;
	int max, min;
private:
	double CalculateOmega(int x, int y);
	double CalculateGuthPosition(int x, int y);
	double  GetDistance(double x1, double y1, double x2, double y2);
	void  FindContours(std::string sFile,
		std::vector<std::vector<cv::Point>
		>& contours,
		int min, int max, int cnt2Find);

	void  FindContoursRaw(uchar* pdata, int width, int height,
		std::vector<std::vector<cv::Point>
		>& contours,
		int min, int max, int cnt2Find);
	//void on_trackbar(int val, void*);

	void GothroughImage(Mat& src);
	void SaveHistogram(Mat& src);
	std::vector<Point2f> GetQuadRangle(std::vector<Point2f> hullPts);

	bool PointIsNear(Point2f ptA, Point2f ptB);
	
	//float angleBetween(const Point &v1, const Point &v2);
	bool HasNear(std::vector<LineSegment>& lines, int curIndex);
	float GetDistance(LineSegment& line, Point2f pt);

	bool IsClockWise(const Point2f v1, const Point2f& v2);
	LineSegment FindNearest(std::vector<LineSegment>& lines, Point2f ptCenter);
	void FilterOutLines(std::vector<LineSegment>& lines, RotatedRect& boundingRect, Mat& img);
	void ProcessLines(std::vector<LineSegment>&);
	std::vector<LineSegment> MergeNear(std::vector<LineSegment>& lines, int curIndex);
	bool Intersection(Point2f o1, Point2f p1, Point2f o2, Point2f p2, Point2f &r);
	std::vector<Point2f> IntersectLines(std::vector<LineSegment>& lines);
	int thresholdVal;
	
};

