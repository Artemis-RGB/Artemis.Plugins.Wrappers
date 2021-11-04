#pragma once

#ifdef _DEBUG
#include <fstream>
#include <string>
#include <mutex>
#include <format>
#include <chrono>
#ifndef LOGGER_FILE
#define LOGGER_FILE "ArtemisWrapper.log"
#endif

static std::mutex fileMutex;
inline void log_to_file(const std::string& data) {
	fileMutex.lock();
	std::ofstream logFile;
	logFile.open(LOGGER_FILE, std::ios::out | std::ios::app);

	auto now = std::chrono::system_clock::now();
	std::string timeHeader = std::format("[{:%Y-%m-%d %H:%M:%OS}] ", now);

	logFile << timeHeader << data << '\n';
	logFile.close();
	fileMutex.unlock();
}
#define LOG(x) log_to_file(x)
#else
#define LOG(x) ((void)0)
#endif 