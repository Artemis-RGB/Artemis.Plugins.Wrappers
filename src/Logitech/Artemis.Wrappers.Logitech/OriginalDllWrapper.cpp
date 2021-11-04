#include "pch.h"
#include "OriginalDllWrapper.hpp"
#include "Logger.hpp"
#include "Utils.hpp"

void OriginalDllWrapper::LoadDll() {
	if (IsDllLoaded()) {
		LOG("Tried to load original dll again, returning true");
		return;
	}

	HKEY registryKey;
	LSTATUS result = RegOpenKeyExW(HKEY_LOCAL_MACHINE, REGISTRY_PATH, 0, KEY_QUERY_VALUE, &registryKey);
	if (result != ERROR_SUCCESS) {
		LOG(std::format("Failed to open registry key \'{}\'. Error: {}}", utf8_encode(REGISTRY_PATH), result));
		return;
	}

	LOG(std::format("Opened registry key \'{}\'", utf8_encode(REGISTRY_PATH)));
	WCHAR buffer[255] = { 0 };
	DWORD bufferSize = sizeof(buffer);
	LSTATUS resultB = RegQueryValueExW(registryKey, ARTEMIS_REG_NAME, 0, NULL, (LPBYTE)buffer, &bufferSize);
	if (resultB != ERROR_SUCCESS) {
		LOG(std::format("Failed to query registry name \'{}\'. Error: {}", utf8_encode(ARTEMIS_REG_NAME), resultB));
		return;
	}

	LOG(std::format("Queried registry name \'{}\' and got value \'{}\'", utf8_encode(ARTEMIS_REG_NAME), utf8_encode(buffer)));
	if (GetFileAttributesW(buffer) == INVALID_FILE_ATTRIBUTES) {
		LOG(std::format("Dll file \'{}\' does not exist. Failed to load original dll.", utf8_encode(buffer)));
		return;
	}

	dll.Load(buffer);

	if (!IsDllLoaded()) {
		LOG("Failed to load original dll");
		return;
	}

	LOG("Loaded original dll");

	LoadFunctions();

	LOG("Loaded original dll functions");
}

void OriginalDllWrapper::LoadFunctions() {
	LogiLedInit = dll["LogiLedInit"];
	LogiLedInitWithName = dll["LogiLedInitWithName"];

	LogiLedGetConfigOptionNumber = dll["LogiGetConfigOptionNumber"];
	LogiLedGetConfigOptionBool = dll["LogiGetConfigOptionBool"];
	LogiLedGetConfigOptionColor = dll["LogiGetConfigOptionColor"];
	LogiLedGetConfigOptionRect = dll["LogiGetConfigOptionRect"];
	LogiLedGetConfigOptionString = dll["LogiGetConfigOptionString"];
	LogiLedGetConfigOptionKeyInput = dll["LogiGetConfigOptionKeyInput"];
	LogiLedGetConfigOptionSelect = dll["LogiGetConfigOptionSelect"];
	LogiLedGetConfigOptionRange = dll["LogiGetConfigOptionRange"];
	LogiLedSetConfigOptionLabel = dll["LogiSetConfigOptionLabel"];

	LogiLedSetTargetDevice = dll["LogiLedSetTargetDevice"];
	LogiLedSaveCurrentLighting = dll["LogiLedSaveCurrentLighting"];
	LogiLedSetLighting = dll["LogiLedSetLighting"];
	LogiLedRestoreLighting = dll["LogiLedRestoreLighting"];
	LogiLedFlashLighting = dll["LogiLedFlashLighting"];
	LogiLedPulseLighting = dll["LogiLedPulseLighting"];
	LogiLedStopEffects = dll["LogiLedStopEffects"];

	LogiLedSetLightingFromBitmap = dll["LogiLedSetLightingFromBitmap"];
	LogiLedSetLightingForKeyWithScanCode = dll["LogiLedSetLightingForKeyWithScanCode"];
	LogiLedSetLightingForKeyWithHidCode = dll["LogiLedSetLightingForKeyWithHidCode"];
	LogiLedSetLightingForKeyWithQuartzCode = dll["LogiLedSetLightingForKeyWithQuartzCode"];
	LogiLedSetLightingForKeyWithKeyName = dll["LogiLedSetLightingForKeyWithKeyName"];
	LogiLedSaveLightingForKey = dll["LogiLedSaveLightingForKey"];
	LogiLedRestoreLightingForKey = dll["LogiLedRestoreLightingForKey"];
	LogiLedExcludeKeysFromBitmap = dll["LogiLedExcludeKeysFromBitmap"];

	LogiLedFlashSingleKey = dll["LogiLedFlashSingleKey"];
	LogiLedPulseSingleKey = dll["LogiLedPulseSingleKey"];
	LogiLedStopEffectsOnKey = dll["LogiLedStopEffectsOnKey"];

	LogiLedSetLightingForTargetZone = dll["LogiLedSetLightingForTargetZone"];

	LogiLedShutdown = dll["LogiLedShutdown"];
}

bool OriginalDllWrapper::IsDllLoaded(){
	return dll.IsLoaded();
}