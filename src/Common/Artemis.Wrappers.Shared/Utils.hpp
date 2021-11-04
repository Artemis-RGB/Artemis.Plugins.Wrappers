#pragma once
#include <string>
#include <vector>
#include "Windows.h"

#ifdef _WIN64
#define _BITS "64"
#else
#define _BITS "32"
#endif

//https://stackoverflow.com/questions/215963
inline std::string utf8_encode(const std::wstring& wstr)
{
	if (wstr.empty()) return std::string();
	int size_needed = WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), NULL, 0, NULL, NULL);
	std::string strTo(size_needed, 0);
	WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), &strTo[0], size_needed, NULL, NULL);
	return strTo;
}

inline std::wstring utf8_decode(const std::string& str)
{
	if (str.empty()) return std::wstring();
	int size_needed = MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), NULL, 0);
	std::wstring wstrTo(size_needed, 0);
	MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), &wstrTo[0], size_needed);
	return wstrTo;
}

inline std::string trim(std::string str)
{
	size_t firstChar = str.find_first_not_of('\0');
	size_t lastChar = str.find_last_not_of('\0');
	size_t length = lastChar - firstChar + 1;
	return str.substr(firstChar, length);
}

inline std::string GetCallerPath()
{
	std::vector<wchar_t> pathBuf;
	DWORD copied = 0;
	do {
		pathBuf.resize(pathBuf.size() + MAX_PATH);
		copied = GetModuleFileName(0, &pathBuf.at(0), pathBuf.size());
	} while (copied >= pathBuf.size());

	pathBuf.resize(copied);

	std::wstring path(pathBuf.begin(), pathBuf.end());

	return trim(utf8_encode(path));
}

inline uint8_t percentage_to_byte(int percentage) {
	return (uint8_t)(percentage / 100.0 * 255.0);
}