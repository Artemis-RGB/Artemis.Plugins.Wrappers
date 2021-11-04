#pragma once
#include "LogitechLEDLib.h"
#include "DllHelper.hpp"
#include <string>

#define ARTEMIS_REG_NAME L"Artemis"
#define ARTEMIS_EXE_NAME "Artemis.UI.exe"

#ifdef _WIN64
#define REGISTRY_PATH L"SOFTWARE\\Classes\\CLSID\\{a6519e67-7632-4375-afdf-caa889744403}\\ServerBinary" 
#else
#define REGISTRY_PATH L"SOFTWARE\\Classes\\WOW6432Node\\CLSID\\{a6519e67-7632-4375-afdf-caa889744403}\\ServerBinary"
#endif

class OriginalDllWrapper {
private:
	DllHelper dll;
	void LoadFunctions();
public:
	void LoadDll();
	bool IsDllLoaded();

	decltype(LogiLedInit)* LogiLedInit = nullptr;
	decltype(LogiLedInitWithName)* LogiLedInitWithName = nullptr;

	decltype(LogiLedGetSdkVersion)* LogiLedGetSdkVersion = nullptr;
	decltype(LogiLedGetConfigOptionNumber)* LogiLedGetConfigOptionNumber = nullptr;
	decltype(LogiLedGetConfigOptionBool)* LogiLedGetConfigOptionBool = nullptr;
	decltype(LogiLedGetConfigOptionColor)* LogiLedGetConfigOptionColor = nullptr;
	decltype(LogiLedGetConfigOptionRect)* LogiLedGetConfigOptionRect = nullptr;
	decltype(LogiLedGetConfigOptionString)* LogiLedGetConfigOptionString = nullptr;
	decltype(LogiLedGetConfigOptionKeyInput)* LogiLedGetConfigOptionKeyInput = nullptr;
	decltype(LogiLedGetConfigOptionSelect)* LogiLedGetConfigOptionSelect = nullptr;
	decltype(LogiLedGetConfigOptionRange)* LogiLedGetConfigOptionRange = nullptr;
	decltype(LogiLedSetConfigOptionLabel)* LogiLedSetConfigOptionLabel = nullptr;

	decltype(LogiLedSetTargetDevice)* LogiLedSetTargetDevice = nullptr;
	decltype(LogiLedSaveCurrentLighting)* LogiLedSaveCurrentLighting = nullptr;
	decltype(LogiLedSetLighting)* LogiLedSetLighting = nullptr;
	decltype(LogiLedRestoreLighting)* LogiLedRestoreLighting = nullptr;
	decltype(LogiLedFlashLighting)* LogiLedFlashLighting = nullptr;
	decltype(LogiLedPulseLighting)* LogiLedPulseLighting = nullptr;
	decltype(LogiLedStopEffects)* LogiLedStopEffects = nullptr;

	decltype(LogiLedSetLightingFromBitmap)* LogiLedSetLightingFromBitmap = nullptr;
	decltype(LogiLedSetLightingForKeyWithScanCode)* LogiLedSetLightingForKeyWithScanCode = nullptr;
	decltype(LogiLedSetLightingForKeyWithHidCode)* LogiLedSetLightingForKeyWithHidCode = nullptr;
	decltype(LogiLedSetLightingForKeyWithQuartzCode)* LogiLedSetLightingForKeyWithQuartzCode = nullptr;
	decltype(LogiLedSetLightingForKeyWithKeyName)* LogiLedSetLightingForKeyWithKeyName = nullptr;
	decltype(LogiLedSaveLightingForKey)* LogiLedSaveLightingForKey = nullptr;
	decltype(LogiLedRestoreLightingForKey)* LogiLedRestoreLightingForKey = nullptr;
	decltype(LogiLedExcludeKeysFromBitmap)* LogiLedExcludeKeysFromBitmap = nullptr;

	decltype(LogiLedFlashSingleKey)* LogiLedFlashSingleKey = nullptr;
	decltype(LogiLedPulseSingleKey)* LogiLedPulseSingleKey = nullptr;
	decltype(LogiLedStopEffectsOnKey)* LogiLedStopEffectsOnKey = nullptr;

	decltype(LogiLedSetLightingForTargetZone)* LogiLedSetLightingForTargetZone = nullptr;

	decltype(LogiLedShutdown)* LogiLedShutdown = nullptr;
};