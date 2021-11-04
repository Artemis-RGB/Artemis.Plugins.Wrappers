#pragma once
#include <cstdint>
#include <string>
#include "Windows.h"
#include "Logger.hpp"
#include "Utils.hpp"

class ArtemisPipeClient
{
private:
	bool isConnected = false;
	HANDLE _pipe = NULL;
public:
	bool IsConnected()
	{
		return isConnected;
	}

	void Connect(const wchar_t* pipeName)
	{
		LOG("Connecting to pipe...");

		_pipe = CreateFile(
			pipeName,
			GENERIC_READ | GENERIC_WRITE,
			0,
			NULL,
			OPEN_EXISTING,
			0,
			NULL);

		if (_pipe == INVALID_HANDLE_VALUE) {
			LOG("Failed connecting to pipe");
			return;
		}

		DWORD mode = PIPE_READMODE_MESSAGE;
		BOOL success = SetNamedPipeHandleState(_pipe, &mode, NULL, NULL);

		if (!success) {
			LOG("Failed setting pipe state");
			return;
		}

		isConnected = TRUE;

		LOG("Connected to pipe successfully");
	}

	void Disconnect()
	{
		LOG("Closing pipe...");
		CloseHandle(_pipe);
		isConnected = false;
		LOG("Closed pipe");
	}

	void Write(const uint8_t* data, const size_t length)
	{
		DWORD writtenLength;
		BOOL result = WriteFile(
			_pipe,
			data,
			length,
			&writtenLength,
			NULL);

		if ((!result) || (writtenLength < length)) {
			LOG(std::format("Error writing to pipe: \'{error}\'. Wrote {bytes} bytes out of {total}", result, writtenLength, length));
			Disconnect();
		}
	}
};

