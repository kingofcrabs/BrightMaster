#include "stdafx.h"
#include "EngineImpl.h"

using namespace std;
using namespace cv;
static string dbgFolder = "d:\\temp\\";
static int innerRadius = 30;
static Mat img;

EngineImpl::EngineImpl()
{
	f = 20/1000; //mm
	thresholdVal = 128;
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




bool EngineImpl::PointIsNear(Point2f ptA, Point2f ptB)
{
	return norm(ptA - ptB) <= 2;

}





bool EngineImpl::HasNear(vector<LineSegment>& lines, int curIndex)
{
	float minDistance = 100000;
	int minIndex = 0;
	for (int i = curIndex + 1; i < lines.size(); i++)
	{
		LineSegment testLine = lines[i];
		LineSegment curLine = lines[curIndex];
		float d1 = norm(curLine.ptStart - testLine.ptStart);
		float d2 = norm(curLine.ptEnd - testLine.ptStart);
		float d3 = norm(curLine.ptEnd - testLine.ptEnd);
		float d4 = norm(curLine.ptStart - testLine.ptEnd);

		float dis = cv::min(cv::min(d1, d2), cv::min(d3, d4));
		if (minDistance > dis)
		{
			minDistance = dis;
			minIndex = i;
		}
	}
	LineSegment line1 = lines[curIndex];
	LineSegment line2 = lines[minIndex];
	return minDistance <= 2;
}


//d = (| (x_2 - x_1)x(x_1 - x_0) | ) / (| x_2 - x_1 | )
float EngineImpl::GetDistance(LineSegment& line, Point2f pt)
{
	float x = pt.x;
	float y = pt.y;
	float x1 = line.ptStart.x;
	float y1 = line.ptStart.y;
	float x2 = line.ptEnd.x;
	float y2 = line.ptEnd.y;

	float A = x - x1;
	float B = y - y1;
	float C = x2 - x1;
	float D = y2 - y1;

	float dot = A * C + B * D;
	float len_sq = C * C + D * D;
	float param = -1;
	if (len_sq != 0) //in case of 0 length line
		param = dot / len_sq;

	double xx, yy;

	if (param < 0) {
		xx = x1;
		yy = y1;
	}
	else if (param > 1) {
		xx = x2;
		yy = y2;
	}
	else {
		xx = x1 + param * C;
		yy = y1 + param * D;
	}

	double dx = x - xx;
	double dy = y - yy;
	return sqrt(dx * dx + dy * dy);
}


vector<LineSegment> EngineImpl::MergeNear(vector<LineSegment>& lines, int curIndex)
{
	vector<LineSegment> newLines;
	float minDistance = 100000;
	int minIndex = 0;
	for (int i = curIndex + 1; i < lines.size(); i++)
	{
		LineSegment& testLine = lines[i];
		LineSegment& curLine = lines[curIndex];
		for (int i = curIndex + 1; i < lines.size(); i++)
		{
			LineSegment testLine = lines[i];
			LineSegment curLine = lines[curIndex];
			float d1 = norm(curLine.ptStart - testLine.ptStart);
			float d2 = norm(curLine.ptEnd - testLine.ptStart);
			float d3 = norm(curLine.ptEnd - testLine.ptEnd);
			float d4 = norm(curLine.ptStart - testLine.ptEnd);

			float dis = cv::min(cv::min(d1, d2), cv::min(d3, d4));
			if (minDistance > dis)
			{
				minDistance = dis;
				minIndex = i;
			}
		}
	}
	LineSegment& closeLine = lines[minIndex];
	LineSegment& curLine = lines[curIndex];
	LineSegment newLine;

	float d1 = norm(curLine.ptStart - closeLine.ptStart);
	float d2 = norm(curLine.ptEnd - closeLine.ptStart);
	float d3 = norm(curLine.ptEnd - closeLine.ptEnd);
	float d4 = norm(curLine.ptStart - closeLine.ptEnd);
	

	
	if (minDistance == d1)
	{
		//return MLine(curLine.ptEnd, testLine.ptEnd);
		newLine = LineSegment(curLine.ptEnd, closeLine.ptEnd);
	}
	else if (minDistance == d2)
	{
		newLine = LineSegment(curLine.ptStart, closeLine.ptEnd);
	}
	else  if (minDistance == d3)
	{
		newLine = LineSegment(curLine.ptStart, closeLine.ptStart);
	}
	else  if (minDistance == d4)
	{
		newLine = LineSegment(curLine.ptEnd, closeLine.ptStart);
	}
			
	for (int j = 0; j < lines.size(); j++)
	{
		if (j != minIndex && j != curIndex)
		{
			newLines.push_back(lines[j]);
		}
	}
	newLines.push_back(LineSegment(newLine));
	return newLines;
}


LineSegment EngineImpl::FindNearest(vector<LineSegment>& lines, Point2f ptCenter)
{
	double minDistance = 100000;

	LineSegment nearestLine = lines[0];
	for each(LineSegment line in lines)
	{
		double distance = GetDistance(line, ptCenter);
		if (distance < minDistance)
		{
			nearestLine = line;
			minDistance = distance;
		}
	}
	return nearestLine;

}

bool EngineImpl::IsClockWise(const Point2f v1, const Point2f& v2)
{
	return v1.x*v2.y > v1.y*v2.x;
}

void EngineImpl::FilterOutLines(vector<LineSegment>& lines, RotatedRect& boundingRect,Mat& img)
{
	Canny(img, img, 50, 200, 3);
	vector<LineSegment> leftSideLines;
	vector<LineSegment> topSideLines;
	vector<LineSegment> bottomSideLines;
	vector<LineSegment> rightSideLines;
	vector<float> distances;
	Point2f pts[4];
	//The order is bottomLeft, topLeft, topRight, bottomRight. 
	boundingRect.points(pts);
	Point2f bottomLeft = pts[0];
	Point2f topLeft = pts[1];
	Point2f topRight = pts[2];
	Point2f bottomRight = pts[3];
	float xSum, ySum;
	xSum = ySum = 0;
	for (int i = 0; i < 4; i++)
	{
		xSum += pts[i].x;
		ySum += pts[i].y;
	}
	Point2f ptCenter = Point2f(xSum / 4, ySum / 4);

	RNG rng(12345);
	Scalar color = Scalar(rng.uniform(0, 255), rng.uniform(0, 255), rng.uniform(0, 255));
	Mat tmpDrawing(img.size(), CV_8UC3);
	cvtColor(img, tmpDrawing, CV_GRAY2BGR);
	for each(auto tmpline in lines)
	{
		Scalar color = Scalar(rng.uniform(0, 255), rng.uniform(0, 255), rng.uniform(0, 255));
		line(tmpDrawing, tmpline.ptStart, tmpline.ptEnd, color, 1);
		//imshow("lines", tmpDrawing);
		//waitKey(0);
	}
	imwrite("d:\\test.jpg", tmpDrawing);

	for each(auto curLine in lines)
	{
		if (norm(curLine.ptStart - curLine.ptEnd) < 100)
			continue;

		Point2f curLinePtCenter = (curLine.ptStart + curLine.ptEnd) / 2;
		
		if (!IsClockWise(topLeft - ptCenter, curLinePtCenter - topLeft) && IsClockWise(bottomLeft - ptCenter, curLinePtCenter - bottomLeft))
		{
			leftSideLines.push_back(curLine);
		}

		if (IsClockWise(topLeft - ptCenter, curLinePtCenter - topLeft) && !IsClockWise(topRight - ptCenter, curLinePtCenter - topRight))
		{
			topSideLines.push_back(curLine);
		}

		if (!IsClockWise(bottomRight - ptCenter, curLinePtCenter - bottomRight) && IsClockWise(topRight - ptCenter, curLinePtCenter - topRight))
		{
			rightSideLines.push_back(curLine);
		}

		if (IsClockWise(bottomRight - ptCenter, curLinePtCenter - bottomRight) && !IsClockWise(bottomLeft - ptCenter, curLinePtCenter - bottomLeft))
		{
			bottomSideLines.push_back(curLine);
		}
	}

	auto leftLine = FindNearest(leftSideLines,ptCenter);
	auto rightLine = FindNearest(rightSideLines, ptCenter);
	auto topLine = FindNearest(topSideLines, ptCenter);
	auto bottomLine = FindNearest(bottomSideLines, ptCenter);
	
	vector<LineSegment> filteredLines;
	filteredLines.push_back(topLine);
	filteredLines.push_back(rightLine);
	filteredLines.push_back(bottomLine);
	filteredLines.push_back(leftLine);
	
	lines = filteredLines;
}

void EngineImpl::ProcessLines(vector<LineSegment>& lines)
{
	bool hasNear;
	do
	{
		hasNear = false;
		for (int i = 0; i < lines.size(); i++)
		{
			if (HasNear(lines, i))
			{
				lines = MergeNear(lines, i);
				hasNear = true;
				break;
			}
		}
	} while (hasNear);

}


bool EngineImpl::Intersection(Point2f o1, Point2f p1, Point2f o2, Point2f p2, Point2f &r)
{
	Point2f x = o2 - o1;
	Point2f d1 = p1 - o1;
	Point2f d2 = p2 - o2;

	float cross = d1.x*d2.y - d1.y*d2.x;
	if (abs(cross) < /*EPS*/1e-8)
		return false;

	double t1 = (x.x * d2.y - x.y * d2.x) / cross;
	r = o1 + d1 * t1;
	return true;
}
vector<Point2f> EngineImpl::IntersectLines(vector<LineSegment>& lines)
{
	int cnt = lines.size();
	vector<Point2f> pts;
	for (int i = 0; i < cnt; i++)
	{
		LineSegment line1 = lines[i];
		LineSegment line2 = lines[(i + 1) % cnt];
		Point2f pt;
		Intersection(line1.ptStart, line2.ptStart, line1.ptEnd, line2.ptEnd, pt);
		pts.push_back(pt);
	}
	return pts;
}


vector<Point2f> EngineImpl::GetQuadRangle(vector<Point2f> hullPts)
{
	vector<pair<int, float>> index_Length;
	for (int i = 0; i < hullPts.size(); i++)
	{
		auto pt1 = hullPts[i];
		auto pt2 = hullPts[(i + 1) % hullPts.size()];
		index_Length.push_back(make_pair(i, norm(pt2 - pt1)));

	}

	
	sort(index_Length.begin(), index_Length.end(), [=](std::pair<int, float>& a, std::pair<int, float>& b)
	{
		return a.second > b.second;
	});

	vector<LineSegment> lines;
	for (int i = 0; i < 4; i++)
	{
		int index = index_Length[i].first;
		Point2f ptStart = hullPts[index];
		Point2f ptEnd = hullPts[(index + 1) % hullPts.size()];
		lines.push_back(LineSegment(ptStart, ptEnd));
	}

	
	for (int i = 0; i < 4; i++)
	{
		
	}
	return IntersectLines(lines);
}

void EngineImpl::FindRect(std::string sFile, vector<pair<int, int>>& ptPairs)
{
	auto img = cv::imread(sFile, 0);
	Mat thresholdImg;
	//Mat pseudoColor;
	//applyColorMap(img, pseudoColor, COLORMAP_JET);
	//sFile = sFile.substr(0, sFile.length() - 4) + "_pseudo.jpg";
	//imwrite(sFile, pseudoColor);
	std::vector< std::vector<cv::Point> > allContours;
	//cv::adaptiveThreshold(img, thresholdImg, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY_INV, 7, 5);
	cv::threshold(img, thresholdImg, 40, 255, CV_THRESH_BINARY);

	//imshow("threshold", thresholdImg);
	cv::findContours(thresholdImg, allContours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
	int width = img.size().width;
	int height = img.size().height;
	//assume rect length > with + height /2;
	int valid = (width + height) / 2;
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
	//pts.clear();
	vector<int> hull;
	convexHull(Mat(allContours[index]), hull, false);
	Mat drawing(img.size(), CV_8UC3);
	Mat drawing2(img.size(), CV_8UC3);
	RNG rng(12345);
	cvtColor(img, drawing, CV_GRAY2BGR);
	cvtColor(img, drawing2, CV_GRAY2BGR);
	int hullcount = (int)hull.size();
	vector<Point2f> hullPts;
	Scalar color = Scalar(255, 0, 0);
	for (int i = 0; i < hullcount; i++)
	{
		Point pt = allContours[index][hull[i]];
		Point ptEnd = allContours[index][hull[(i + 1) % hullcount]];
		line(drawing, pt, ptEnd, color);
		hullPts.push_back(pt);
	}
	imwrite("D:\\hull.jpg", drawing);
	imshow("hull", drawing);
	waitKey(0);
	double epsilon = 0.1*arcLength(hullPts, true);
	vector<Point2f> approx;
	approxPolyDP(hullPts, approx, epsilon, true);
	for (int i = 0; i < approx.size(); i++)
	{
		Point2f ptStart = approx[i];
		Point2f ptEnd = approx[(i + 1) % approx.size()];
		line(drawing2, ptStart, ptEnd, color);
		ptPairs.push_back(make_pair(ptStart.x, ptStart.y));
	}
	imwrite("D:\\Quad.jpg", drawing2);
	imshow("Quad", drawing2);
	waitKey(0);

}

void  EngineImpl::FindContours(string sFile,
	std::vector<std::vector<cv::Point>
	>& contours,
	int min, int max, int cnt2Find)
{
	std::vector< std::vector<cv::Point> > allContours;
	auto img = cv::imread(sFile,0);
	Mat thresholdImg;
	cv::adaptiveThreshold(img, thresholdImg, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, 7, 5);
	
	cv::findContours(thresholdImg, allContours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
	contours.clear();
	RNG rng(12345);
	for (size_t i = 0; i<allContours.size(); i++)
	{
		int contourSize = allContours[i].size();
		
		if (contourSize > min && contourSize < max)
		{
			contours.push_back(allContours[i]);
		}
	}
	Mat drawing = Mat::zeros(thresholdImg.size(), CV_8UC3);
	for (int i = 0; i< contours.size(); i++)
	{
		Scalar color = Scalar(rng.uniform(0, 255), rng.uniform(0, 255), rng.uniform(0, 255));
		drawContours(drawing, contours, i, color, 2, 8);
	}
	cv::imshow("test", drawing);
}


void EngineImpl::FindContoursRaw(uchar* pdata, int width, int height,
	std::vector<std::vector<cv::Point>
	>& contours,
	int min, int max, int cnt2Find)
{
	Mat img = Mat(height, width, CV_8UC1, pdata);
	
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



double EngineImpl::CalculateGuthPosition(int x, int y)
{
	//Ｈ　＝＞　ｙ
	//Ｔ　＝＞　ｘ
	//Ｒ　＝＞　ｆ
	double H2R = y / f;
	double T2R = x / f;
	return 1;
}



void EngineImpl::Convert2PesudoColor(std::string srcFile, std::string destFile)
{
	Mat orgImg = imread(srcFile);
	Mat color;
	applyColorMap(orgImg, color, COLORMAP_JET);
	imwrite(destFile,color);
}


double EngineImpl::CalculateOmega(int x, int y)
{
	int width = img.size().width;
	int height = img.size().height;
	double xDis = abs(x - width / 2) * pixelUnit;
	double yDis = abs(y - height / 2) * pixelUnit;
	double dis = sqrt(xDis*xDis + yDis * yDis);
	double r = sqrt(xDis*xDis + yDis * yDis + f*f);
	double cosθ = f / r;
	double Ap = pixelUnit * pixelUnit * cosθ;
	double ω = Ap / (r*r);
	return ω;
}

double EngineImpl::CalculateGlare(std::string sFile, std::vector<cv::Rect2f> rects)
{
	img = imread(sFile);
	imshow(sFile, img);
	return 5;
}

void on_trackbar(int val, void* parent)
{
	int thresholdVal = val;
	string winName = "threshold";
	Mat thresholdImg;
	std::vector< std::vector<cv::Point> > allContours;
	cv::threshold(img, thresholdImg, thresholdVal, 255, CV_THRESH_BINARY);
	cv::findContours(thresholdImg, allContours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
	EngineImpl* pEngine = (EngineImpl*)parent;
	pEngine->contours.clear();
	RNG rng(12345);
	for (size_t i = 0; i<allContours.size(); i++)
	{
		int contourSize = allContours[i].size();

		if (contourSize > pEngine->min && contourSize < pEngine->max)
		{
			std::vector<cv::Point> polygon;
			//double len = arcLength(allContours[i], true);
			approxPolyDP(allContours[i], polygon, 1, true);
			pEngine->contours.push_back(polygon);
		}
	}
	Mat drawing = Mat::zeros(thresholdImg.size(), CV_8UC3);
	for (int i = 0; i< pEngine->contours.size(); i++)
	{
		Scalar color = Scalar(rng.uniform(0, 255), rng.uniform(0, 255), rng.uniform(0, 255));
		drawContours(drawing, pEngine->contours, i, color, 2, 8);
	}
	
	imshow(winName, drawing);
	//imshow(winName, thresholdImg);
}

int EngineImpl::SearchLights(uchar* pdata, int width, int height, int min, int max, std::vector<std::vector<cv::Point>>& contours2Find)
{
	img = Mat(height, width, CV_8UC1, pdata);
	Mat thresholdImg;
	thresholdVal = cv::threshold(img, thresholdImg, 0, 255, CV_THRESH_BINARY | CV_THRESH_OTSU);
	//cv::adaptiveThreshold(img, thresholdImg, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, 7, 5);

	string winName = "threshold";
	namedWindow(winName, WINDOW_NORMAL);
	resizeWindow(winName, 800, 800 * img.cols / img.rows);
	imshow(winName, thresholdImg);

	//vector.resize(img.rows*img.cols);
	string sliderName = "slider";
	createTrackbar(sliderName, winName, &thresholdVal, 255, on_trackbar,this);
	waitKey(0);
	cv::threshold(img, thresholdImg, thresholdVal, 255, CV_THRESH_BINARY);
	//vector.assign(thresholdImg.datastart, thresholdImg.dataend);

	contours2Find = contours;
	return thresholdVal;
}

int EngineImpl::AdaptiveThreshold(uchar* pdata,int width, int height, std::vector<uchar>& vector)
{
	img = Mat(height, width, CV_8UC1, pdata);
	Mat thresholdImg;
	thresholdVal = cv::threshold(img, thresholdImg, 0, 255, CV_THRESH_BINARY | CV_THRESH_OTSU);
	//cv::adaptiveThreshold(img, thresholdImg, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY, 7, 5);
	
	string winName = "threshold";
	namedWindow(winName, WINDOW_NORMAL);
	resizeWindow(winName, 800, 800* img.cols/img.rows);
	imshow(winName, thresholdImg);
	
	vector.resize(img.rows*img.cols);
	string sliderName = "slider";
	createTrackbar(sliderName, winName, &thresholdVal, 255, on_trackbar);
	waitKey(0);
	cv::threshold(img, thresholdImg, thresholdVal, 255, CV_THRESH_BINARY);
	vector.assign(thresholdImg.datastart, thresholdImg.dataend);
	return thresholdVal;
}