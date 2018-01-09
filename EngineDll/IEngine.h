#pragma once
#include "EngineImpl.h"
using namespace System::Collections::Generic;

namespace EngineDll
{

	public ref class Circle
	{
	public:
		int x;
		int y;
		int radius;
		
		Circle(int xx, int yy, int rr)
		{
			x = xx;
			y = yy;
			radius = rr;
		}
	};
	public ref class MPoint
	{
	public:
		int x;
		int y;
		MPoint(int xx, int yy)
		{
			x = xx;
			y = yy;
		}

	};

	public ref class MRect
	{
	public:
		MPoint^ ptStart;
		MPoint^ ptEnd;
		MRect(MPoint^ ptS, MPoint^ ptE)
		{
			ptStart = gcnew MPoint(ptS->x,ptS->y);
			ptEnd = gcnew MPoint(ptE->x,ptE->y);
		}
	};

	public ref class MSize
	{
	public:
		int x;
		int y;
	
		MSize(int xx, int yy)
		{
			x = xx;
			y = yy;
		}
	};


	public ref class AnalysisResult
	{
	public: 
		double val;
		AnalysisResult(double v)
		{
			val = v;
		}
	};




	public ref class IEngine
	{
	public:
		IEngine();
		~IEngine();
		cv::Rect2f Convert2Rect2f(MRect^ rc);
		void Convert2PseudoColor(System::String^ sOrgFile, System::String^ sDestFile);
		List<MPoint^>^ IEngine::FindRect(System::String^ sFile, int% threshold, bool mannualThreshold, List<MPoint^>^ hullPts);
	private :
		std::string IEngine::WStringToString(const std::wstring &wstr);
		template<typename T>  List<T>^  Copy2List(std::vector<T> vector);
		EngineImpl* m_EngineImpl;
		
	};



	
	

}

