// dllmain.cpp : Defines the entry point for the DLL application.
#pragma warning( disable : 6387 )
#pragma warning( disable : 26812 )
#include "pch.h"
#define LOGGER_FILE "ArtemisWrapperLogitech.log"

#include "LogitechLEDLib.h"
#include "LogiCommand.hpp"
#include "ArtemisBuffer.hpp"
#include "ArtemisPipeClient.hpp"
#include "Utils.hpp"
#include "OriginalDllWrapper.hpp"
#include "Logger.hpp"

#pragma region Static variables
static OriginalDllWrapper originalDllWrapper;
static ArtemisPipeClient artemisPipeClient;
static bool isInitialized = false;
static std::string program_name = "";
#pragma endregion

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	{
		std::string program_path = GetCallerPath();
		size_t lastBackslashIndex = program_path.find_last_of('\\') + 1;
		program_name = program_path.substr(lastBackslashIndex, program_path.length() - lastBackslashIndex);

		LOG(std::format("Main called, DLL loaded into {} ({} bits)", program_name, _BITS));
		break;
	}
	case DLL_THREAD_ATTACH:
		break;
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		//shutdown when detaching, sometimes games don't do this
		LOG("DLL_PROCESS_DETACH, shutting down.");

		LogiLedShutdown();
		break;
	}
	return TRUE;
}

/// <summary>
/// Shorter version of ArtemisBuffer<LogiCommand>
/// </summary>
class LogiBuffer : public ArtemisBuffer<LogiCommand> { };

#pragma region Variable length buffer functions
bool LogiLedInit()
{
	return LogiLedInitWithName(program_name.c_str());
}

bool LogiLedInitWithName(const char name[])
{
	if (isInitialized) {
		LOG("Program tried to initialize twice, returning true");
		return true;
	}

	LOG("LogiLedInit Called");
	if (program_name != ARTEMIS_EXE_NAME) {
		artemisPipeClient.Connect(L"\\\\.\\pipe\\Artemis\\Logitech");

		if (artemisPipeClient.IsConnected()) {
			const uint32_t nameLength = (uint32_t)strlen(name) + 1;

			const auto buffer = LogiBuffer::create_dynamic<LogiCommand::Init>(name, nameLength);

			artemisPipeClient.Write(buffer.data(), buffer.size());

			isInitialized = true;
			return true;
		}
	}
	else {
		LOG(std::format("Program name {} blacklisted.", program_name));
	}

	LOG("Trying to load original dll...");

	originalDllWrapper.LoadDll();

	if (originalDllWrapper.IsDllLoaded()) {
		isInitialized = originalDllWrapper.LogiLedInit();
		return isInitialized;
	}
	isInitialized = false;
	return false;
}

bool LogiLedExcludeKeysFromBitmap(LogiLed::KeyName* keyList, int listCount)
{
	if (listCount == 0)
		return false;

	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create_dynamic<LogiCommand::ExcludeKeysFromBitmap>(keyList, listCount);

		artemisPipeClient.Write(buffer.data(), buffer.size());
		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedExcludeKeysFromBitmap(keyList, listCount);
	}
	return false;
}

void LogiLedShutdown()
{
	if (!isInitialized)
		return;

	LOG("LogiLedShutdown called");

	isInitialized = false;

	if (artemisPipeClient.IsConnected()) {
		LOG("Informing artemis and closing pipe...");

		const char* name = program_name.data();
		const uint32_t nameLength = (int)strlen(name) + 1;

		const auto buffer = LogiBuffer::create_dynamic<LogiCommand::Shutdown>(name, nameLength);

		artemisPipeClient.Write(buffer.data(), buffer.size());

		artemisPipeClient.Disconnect();
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedShutdown();
	}
}
#pragma endregion

bool LogiLedSetTargetDevice(int targetDevice)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::SetTargetDevice>(targetDevice);

		artemisPipeClient.Write(buffer.data(), buffer.size());
		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedSetTargetDevice(targetDevice);
	}
	return false;
}

bool LogiLedSaveCurrentLighting()
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::SaveCurrentLighting>();

		artemisPipeClient.Write(buffer.data(), buffer.size());
		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedSaveCurrentLighting();
	}
	return false;
}

bool LogiLedSetLighting(int redPercentage, int greenPercentage, int bluePercentage, int unknown = 0)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::SetLighting>(
			percentage_to_byte(redPercentage),
			percentage_to_byte(greenPercentage),
			percentage_to_byte(bluePercentage),
			unknown
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());

		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedSetLighting(redPercentage, greenPercentage, bluePercentage);
	}
	return false;
}

bool LogiLedRestoreLighting()
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::RestoreLighting>();

		artemisPipeClient.Write(buffer.data(), buffer.size());

		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedRestoreLighting();
	}
	return false;
}

bool LogiLedFlashLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::FlashLighting>(
			percentage_to_byte(redPercentage),
			percentage_to_byte(greenPercentage),
			percentage_to_byte(bluePercentage),
			milliSecondsDuration,
			milliSecondsInterval
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());

		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedFlashLighting(redPercentage, greenPercentage, bluePercentage, milliSecondsDuration, milliSecondsInterval);
	}
	return false;
}

bool LogiLedPulseLighting(int redPercentage, int greenPercentage, int bluePercentage, int milliSecondsDuration, int milliSecondsInterval)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::PulseLighting>(
			percentage_to_byte(redPercentage),
			percentage_to_byte(greenPercentage),
			percentage_to_byte(bluePercentage),
			milliSecondsDuration,
			milliSecondsInterval
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());

		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedPulseLighting(redPercentage, greenPercentage, bluePercentage, milliSecondsDuration, milliSecondsInterval);
	}
	return false;
}

bool LogiLedStopEffects()
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::StopEffects>();

		artemisPipeClient.Write(buffer.data(), buffer.size());
		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedStopEffects();
	}
	return false;
}

bool LogiLedSetLightingFromBitmap(uint8_t bitmap[])
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::SetLightingFromBitmap, LOGI_LED_BITMAP_SIZE>(bitmap);

		artemisPipeClient.Write(buffer.data(), buffer.size());
		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedSetLightingFromBitmap(bitmap);
	}
	return false;
}

bool LogiLedSetLightingForKeyWithScanCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::SetLightingForKeyWithScanCode>(
			keyCode,
			percentage_to_byte(redPercentage),
			percentage_to_byte(greenPercentage),
			percentage_to_byte(bluePercentage)
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());

		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedSetLightingForKeyWithScanCode(keyCode, redPercentage, greenPercentage, bluePercentage);
	}
	return false;
}

bool LogiLedSetLightingForKeyWithHidCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::SetLightingForKeyWithHidCode>(
			keyCode,
			percentage_to_byte(redPercentage),
			percentage_to_byte(greenPercentage),
			percentage_to_byte(bluePercentage)
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());

		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedSetLightingForKeyWithHidCode(keyCode, redPercentage, greenPercentage, bluePercentage);
	}
	return false;
}

bool LogiLedSetLightingForKeyWithQuartzCode(int keyCode, int redPercentage, int greenPercentage, int bluePercentage)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::SetLightingForKeyWithQuartzCode>(
			keyCode,
			percentage_to_byte(redPercentage),
			percentage_to_byte(greenPercentage),
			percentage_to_byte(bluePercentage)
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());

		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedSetLightingForKeyWithQuartzCode(keyCode, redPercentage, greenPercentage, bluePercentage);
	}
	return false;
}

bool LogiLedSetLightingForKeyWithKeyName(LogiLed::KeyName keyName, int redPercentage, int greenPercentage, int bluePercentage)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::SetLightingForKeyWithKeyName>(
			keyName,
			percentage_to_byte(redPercentage),
			percentage_to_byte(greenPercentage),
			percentage_to_byte(bluePercentage)
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());

		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedSetLightingForKeyWithKeyName(keyName, redPercentage, greenPercentage, bluePercentage);
	}
	return false;
}

bool LogiLedSaveLightingForKey(LogiLed::KeyName keyName)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::SaveLightingForKey>(
			keyName
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());
		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedSaveLightingForKey(keyName);
	}
	return false;
}

bool LogiLedRestoreLightingForKey(LogiLed::KeyName keyName)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::RestoreLightingForKey>(
			keyName
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());
		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedRestoreLightingForKey(keyName);
	}
	return false;
}

bool LogiLedFlashSingleKey(LogiLed::KeyName keyName, int redPercentage, int greenPercentage, int bluePercentage, int msDuration, int msInterval)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::FlashSingleKey>(
			keyName,
			percentage_to_byte(redPercentage),
			percentage_to_byte(greenPercentage),
			percentage_to_byte(bluePercentage),
			msDuration,
			msInterval
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());

		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedFlashSingleKey(keyName, redPercentage, greenPercentage, bluePercentage, msDuration, msInterval);
	}
	return false;
}

bool LogiLedPulseSingleKey(LogiLed::KeyName keyName, int startRedPercentage, int startGreenPercentage, int startBluePercentage, int finishRedPercentage, int finishGreenPercentage, int finishBluePercentage, int msDuration, bool isInfinite)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::PulseSingleKey>(
			keyName,
			percentage_to_byte(startRedPercentage),
			percentage_to_byte(startGreenPercentage),
			percentage_to_byte(startBluePercentage),
			percentage_to_byte(finishRedPercentage),
			percentage_to_byte(finishGreenPercentage),
			percentage_to_byte(finishBluePercentage),
			msDuration,
			isInfinite
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());

		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedPulseSingleKey(keyName, startRedPercentage, startGreenPercentage, startBluePercentage, finishRedPercentage, finishGreenPercentage, finishBluePercentage, msDuration, isInfinite);
	}
	return false;
}

bool LogiLedStopEffectsOnKey(LogiLed::KeyName keyName)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::StopEffectsOnKey>(
			keyName
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());
		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedStopEffectsOnKey(keyName);
	}
	return false;
}

bool LogiLedSetLightingForTargetZone(LogiLed::DeviceType deviceType, int zone, int redPercentage, int greenPercentage, int bluePercentage)
{
	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LogiBuffer::create<LogiCommand::SetLightingForTargetZone>(
			deviceType,
			zone,
			percentage_to_byte(redPercentage),
			percentage_to_byte(greenPercentage),
			percentage_to_byte(bluePercentage)
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());

		return true;
	}
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedSetLightingForTargetZone(deviceType, zone, redPercentage, greenPercentage, bluePercentage);
	}
	return false;
}

#pragma region Useless methods
bool LogiGetConfigOptionNumber(const wchar_t* configPath, double* defaultValue)
{
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedGetConfigOptionNumber(configPath, defaultValue);
	}
	return false;
}
bool LogiGetConfigOptionBool(const wchar_t* configPath, bool* defaultValue)
{
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedGetConfigOptionBool(configPath, defaultValue);
	}
	return false;
}
bool LogiGetConfigOptionColor(const wchar_t* configPath, int* defaultRed, int* defaultGreen, int* defaultBlue)
{
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedGetConfigOptionColor(configPath, defaultRed, defaultGreen, defaultBlue);
	}
	return false;
}
bool LogiGetConfigOptionRect(const wchar_t* configPath, int* defaultX, int* defaultY, int* defaultWidth, int* defaultHeight)
{
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedGetConfigOptionRect(configPath, defaultX, defaultY, defaultWidth, defaultHeight);
	}
	return false;
}
bool LogiGetConfigOptionRange(const wchar_t* configPath, int* defaultValue, int min, int max)
{
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedGetConfigOptionRange(configPath, defaultValue, min, max);
	}
	return false;
}
bool LogiGetConfigOptionSelect(const wchar_t* configPath, wchar_t* defaultValue, int* valueSize, const wchar_t* values, int bufferSize)
{
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedGetConfigOptionSelect(configPath, defaultValue, valueSize, values, bufferSize);
	}
	return false;
}
bool LogiGetConfigOptionKeyInput(const wchar_t* configPath, wchar_t* defaultValue, int bufferSize)
{
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedGetConfigOptionKeyInput(configPath, defaultValue, bufferSize);
	}
	return false;
}
bool LogiSetConfigOptionLabel(const wchar_t* configPath, wchar_t* label)
{
	if (originalDllWrapper.IsDllLoaded()) {
		return originalDllWrapper.LogiLedSetConfigOptionLabel(configPath, label);
	}
	return false;
}
#pragma endregion
