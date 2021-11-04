// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#define LOGGER_FILE "ArtemisWrapperLightFx.log"

#include "LFX2.h"
#include "LightFXCommands.h"
#include "Logger.hpp"
#include "Utils.hpp"
#include "ArtemisPipeClient.hpp"
#include "ArtemisBuffer.hpp"

#pragma region Static variables
static uint32_t maxDevices = 8;
static uint32_t maxLedsPerDevice = 8;
static ArtemisPipeClient artemisPipeClient;
static bool isInitialized = false;
static std::string program_name = "";
#pragma endregion

/// <summary>
/// Shorter version of ArtemisBuffer<LightFXCommand>
/// </summary>
class LightFxBuffer : public ArtemisBuffer<LightFXCommand> { };

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH: {
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

		LFX_Release();
		break;
	}
	return TRUE;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Initialize()
{
	LOG("LFX_Initialize");

	if (isInitialized) {
		LOG("Program tried to initialize twice, returning success");
		return LFX_SUCCESS;
	}
	if (program_name == "Artemis.UI.exe") {
		LOG("Program name blacklisted, returning failure");
		return LFX_FAILURE;
	}

	artemisPipeClient.Connect(L"\\\\.\\pipe\\Artemis\\LightFx");

	if (!artemisPipeClient.IsConnected()) {
		LOG("Failed connecting to Artemis pipe, returning failure");
		return LFX_FAILURE;
	}
	auto name = program_name.c_str();
	unsigned int nameLength = (int)strlen(name) + 1;

	const auto buffer = LightFxBuffer::create_dynamic<LightFXCommand::Initialize>(name, nameLength);

	artemisPipeClient.Write(buffer.data(), buffer.size());

	isInitialized = true;
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Release()
{
	LOG("LFX_Release");

	if (!isInitialized) {
		LOG("Tried to release when not initialized, returning");
		return LFX_SUCCESS;
	}

	if (artemisPipeClient.IsConnected()) {
		LOG("Informing artemis and closing pipe...");

		auto name = program_name.c_str();
		unsigned int nameLength = (int)strlen(name) + 1;

		const auto buffer = LightFxBuffer::create_dynamic<LightFXCommand::Release>(name, nameLength);

		artemisPipeClient.Write(buffer.data(), buffer.size());

		artemisPipeClient.Disconnect();
	}

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Reset()
{
	LOG("LFX_Reset");

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}

	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LightFxBuffer::create<LightFXCommand::Reset>();

		artemisPipeClient.Write(buffer.data(), buffer.size());
	}

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Update()
{
	LOG("LFX_Update");

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}

	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LightFxBuffer::create<LightFXCommand::Update>();

		artemisPipeClient.Write(buffer.data(), buffer.size());
	}

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_UpdateDefault()
{
	LOG("LFX_UpdateDefault");

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}

	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LightFxBuffer::create<LightFXCommand::UpdateDefault>();

		artemisPipeClient.Write(buffer.data(), buffer.size());
	}

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetNumDevices(unsigned int* const numDevices)
{
	LOG("LFX_GetNumDevices");

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}

	*numDevices = maxDevices;

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetDeviceDescription(const unsigned int devIndex, char* const devDesc, const unsigned int devDescSize, unsigned char* const devType)
{
	LOG(std::format("LFX_GetDeviceDescription - device {}", devIndex));

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}
	if (devIndex >= maxDevices) {
		LOG("Tried to get device description for non existing device.");
		return LFX_ERROR_NODEVS;
	}
	std::string deviceName = "Artemis Device " + std::to_string(devIndex + 1);

	if (devDescSize < deviceName.length()) {
		LOG("Buffer size error");
		return LFX_ERROR_BUFFSIZE;
	}

	strcpy_s(devDesc, devDescSize, deviceName.c_str());

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetNumLights(const unsigned int devIndex, unsigned int* const numLights)
{
	LOG(std::format("LFX_GetNumLights - device {}", devIndex));

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}
	if (devIndex >= maxDevices) {
		LOG("Tried to get device description for non existing device.");
		return LFX_ERROR_NODEVS;
	}

	*numLights = maxLedsPerDevice;

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightDescription(const unsigned int devIndex, const unsigned int lightIndex, char* const lightDesc, const unsigned int lightDescSize)
{
	LOG(std::format("LFX_GetLightDescription - device {} | light {}", devIndex, lightIndex));

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}
	if (devIndex >= maxDevices) {
		LOG("Tried to get device description for non existing device.");
		return LFX_ERROR_NODEVS;
	}
	if (lightIndex >= maxLedsPerDevice) {
		LOG("Tried to get led description for non existing led.");
		return LFX_ERROR_NOLIGHTS;
	}

	std::string lightName = "Artemis Light " + std::to_string(lightIndex + 1);

	if (lightDescSize < lightName.length()) {
		LOG("Buffer size error");
		return LFX_ERROR_BUFFSIZE;
	}

	strcpy_s(lightDesc, lightDescSize, lightName.c_str());

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightLocation(const unsigned int devIndex, const unsigned int lightIndex, PLFX_POSITION const lightLoc)
{
	LOG(std::format("LFX_GetLightLocation - device {} | light {}", devIndex, lightIndex));

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}
	if (devIndex >= maxDevices) {
		LOG("Tried to get device description for non existing device.");
		return LFX_ERROR_NODEVS;
	}
	if (lightIndex >= maxLedsPerDevice) {
		LOG("Tried to get led description for non existing led.");
		return LFX_ERROR_NOLIGHTS;
	}

	*lightLoc = {(unsigned char)devIndex,0,(unsigned char)lightIndex};
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetLightColor(const unsigned int devIndex, const unsigned int lightIndex, PLFX_COLOR const lightColor)
{
	LOG(std::format("LFX_GetLightColor - device {} | light {}", devIndex, lightIndex));

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}
	if (devIndex >= maxDevices) {
		LOG("Tried to get device description for non existing device.");
		return LFX_ERROR_NODEVS;
	}
	if (lightIndex >= maxLedsPerDevice) {
		LOG("Tried to get led description for non existing led.");
		return LFX_ERROR_NOLIGHTS;
	}

	*lightColor = { 255,255,255,255};//TODO
	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightColor(const unsigned int devIndex, const unsigned int lightIndex, PLFX_COLOR const lightColor)
{
	LOG(std::format("LFX_SetLightColor - device {} | light {} | color ({} {} {})", devIndex, lightIndex,lightColor->red, lightColor->green, lightColor->blue));

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}
	if (devIndex >= maxDevices) {
		LOG("Tried to get device description for non existing device.");
		return LFX_ERROR_NODEVS;
	}
	if (lightIndex >= maxLedsPerDevice) {
		LOG("Tried to get led description for non existing led.");
		return LFX_ERROR_NOLIGHTS;
	}

	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LightFxBuffer::create<LightFXCommand::SetLightColor>(
			lightColor->blue,
			lightColor->green,
			lightColor->red,
			lightColor->brightness
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());
	}

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_Light(const unsigned int locationMask, const unsigned int lightColor)
{
	LOG(std::format("LFX_Light - mask {} | color {}", locationMask, lightColor));

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}

	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LightFxBuffer::create<LightFXCommand::Light>(
			locationMask,
			lightColor
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());
	}

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightActionColor(const unsigned int devIndex, const unsigned int lightIndex, const unsigned int actionType, const PLFX_COLOR primaryColor)
{
	LOG(std::format("LFX_SetLightActionColor - device {} | light {} | actionType {} | color ({} {} {})", devIndex, lightIndex, actionType, primaryColor->red, primaryColor->green, primaryColor->blue));

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}

	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LightFxBuffer::create<LightFXCommand::SetLightActionColor>(
			devIndex,
			lightIndex,
			actionType,
			primaryColor->blue,
			primaryColor->green,
			primaryColor->red,
			primaryColor->brightness
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());
	}

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_SetLightActionColorEx(const unsigned int devIndex, const unsigned int lightIndex, const unsigned int actionType, const PLFX_COLOR primaryColor, const PLFX_COLOR secondaryColor)
{
	LOG(std::format("LFX_SetLightActionColorEx - device {} | light {} | actionType {} | PrimaryColor ({} {} {}) | SecondaryColor ({} {} {})", 
		devIndex, lightIndex, actionType, primaryColor->red, primaryColor->green, primaryColor->blue, secondaryColor->red, secondaryColor->green, secondaryColor->blue));

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}

	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LightFxBuffer::create<LightFXCommand::SetLightActionColorEx>(
			devIndex,
			lightIndex,
			actionType,
			primaryColor->blue,
			primaryColor->green,
			primaryColor->red,
			primaryColor->brightness,
			secondaryColor->blue,
			secondaryColor->green,
			secondaryColor->red,
			secondaryColor->brightness
		);
	}

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_ActionColor(const unsigned int locationMask, const unsigned int actionType, const unsigned int primaryCol)
{
	LOG(std::format("LFX_ActionColor - locationMask {} | actionType {} | color {}", locationMask, actionType, primaryCol));

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}

	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LightFxBuffer::create<LightFXCommand::ActionColor>(
			locationMask,
			actionType,
			primaryCol
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());
	}

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_ActionColorEx(const unsigned int locationMask, const unsigned int actionType, const unsigned int primaryColor, const unsigned int secondaryColor)
{
	LOG(std::format("LFX_ActionColorEx - locationMask {} | actionType {} | PrimaryColor {} | SecondaryColor {}", locationMask, actionType, primaryColor, secondaryColor));

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}

	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LightFxBuffer::create<LightFXCommand::ActionColorEx>(
			locationMask,
			actionType,
			primaryColor,
			secondaryColor
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());
	}

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_SetTiming(const int newTiming)
{
	LOG(std::format("LFX_SetTiming - newTiming {}", newTiming));

	if (!isInitialized) {
		LOG("Not initialized, returning NoInit");
		return LFX_ERROR_NOINIT;
	}

	if (artemisPipeClient.IsConnected()) {
		const auto buffer = LightFxBuffer::create<LightFXCommand::SetTiming>(
			newTiming
		);

		artemisPipeClient.Write(buffer.data(), buffer.size());
	}

	return LFX_SUCCESS;
}

FN_DECLSPEC LFX_RESULT STDCALL LFX_GetVersion(char* const version, const unsigned int versionSize)
{
	LOG("LFX_GetVersion");

	strcpy_s(version, versionSize, "2.2.0.0");

	return LFX_SUCCESS;
}