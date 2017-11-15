
#include "stdafx.h"
#include "IEngine.h"
#include <msclr\marshal_cppstd.h>
using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;

namespace EngineDll
{
	IEngine::IEngine()
	{
		m_EngineImpl = new EngineImpl();
	}

	IEngine::~IEngine()
	{
		delete m_EngineImpl;
	}

	std::string IEngine::WStringToString(const std::wstring &wstr)
	{
		std::string str(wstr.length(), ' ');
		std::copy(wstr.begin(), wstr.end(), str.begin());
		return str;
	}

	cv::Rect2f IEngine::Convert2Rect2f(MRect^ rc)
	{
		cv::Point2f ptStart(rc->ptStart->x, rc->ptStart->y);
		cv::Point2f ptEnd(rc->ptEnd->x, rc->ptEnd->y);
		return cv::Rect2f(ptStart, ptEnd);
	}

	List<MPoint^>^ IEngine::FindRect(System::String^ sFile, int% threshold, bool autoFindBoundary)
	{
		std::string nativeFileName = msclr::interop::marshal_as< std::string >(sFile);
		std::vector<std::pair<int,int>> pts;
		int nativeThreshold = threshold;
		m_EngineImpl->FindRect(nativeFileName, nativeThreshold, pts, autoFindBoundary);
		threshold = nativeThreshold;
		List<MPoint^>^ managedPts = gcnew List<MPoint^>(pts.size());
		for (int i = 0; i < pts.size(); i++)
		{
			managedPts->Add(gcnew MPoint(pts[i].first, pts[i].second));
		}
		return managedPts;
	}

	void IEngine::Convert2PseudoColor(System::String^ sOrgFile, System::String^ sDestFile)
	{
		std::string nativeSourceFileName = msclr::interop::marshal_as< std::string >(sOrgFile);
		std::string nativeDestFileName = msclr::interop::marshal_as< std::string >(sDestFile);
		m_EngineImpl->Convert2PesudoColor(nativeSourceFileName, nativeDestFileName);
	}

	template<typename T>  List<T>^  IEngine::Copy2List(std::vector<T> vector)
	{
		List<T>^ arrayList = gcnew List<T>();
		for (int i = 0; i < vector.size(); i++)
		{
			arrayList->Add(vector[i]);
		}
		return arrayList;
	}

	/*void IEngine::FindContours(System::String^ sFile, int cnt2Find)
	{
		std::string nativeSourceFileName = msclr::interop::marshal_as< std::string >(sFile);
		std::vector<std::vector<cv::Point>> contours;
		m_EngineImpl->FindContours(nativeSourceFileName, contours, 100, 1000, 3);
	}*/

}
