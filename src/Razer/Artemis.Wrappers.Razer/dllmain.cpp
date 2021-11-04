// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#define LOGGER_FILE "ArtemisWrapperRazer.log"
#include <objbase.h>

#include "RzChromaSDK.h"
#include "ArtemisPipeClient.hpp"
#include "ArtemisBuffer.hpp"
#include "Utils.hpp"
#include "RazerCommand.h"
#include "Logger.hpp"

static const RZEFFECTID GUID_NULL = { 0, 0, 0, { 0, 0, 0, 0, 0, 0, 0, 0 } };

static ArtemisPipeClient artemisPipeClient;
static bool isInitialized = false;
static std::string program_name = "";

/// <summary>
/// Shorter version of ArtemisBuffer<RazerCommand>
/// </summary>
class RazerBuffer : public ArtemisBuffer<RazerCommand> { };

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	{
		if (program_name.empty()) {
			std::string program_path = GetCallerPath();
			size_t lastBackslashIndex = program_path.find_last_of('\\') + 1;
			program_name = program_path.substr(lastBackslashIndex, program_path.length() - lastBackslashIndex);

			LOG(std::format("Main called, DLL loaded into {} ({} bits)", program_name, _BITS));

		}
		break;
	}
	case DLL_THREAD_ATTACH:
		break;
	case DLL_THREAD_DETACH:
		break;
	case DLL_PROCESS_DETACH:
		//shutdown when detaching, sometimes games don't do this
		LOG("DLL_PROCESS_DETACH, shutting down.");

		//UnInit();
		break;
	}
	return TRUE;
}

#ifdef __cplusplus
extern "C" {
#endif	
	void EnsureValidGuid(RZEFFECTID* pEffectId)
	{
		if (pEffectId != nullptr && *pEffectId == GUID_NULL) {
			CoCreateGuid(pEffectId);
		}
	}

	RZRESULT Init()
	{
		if (isInitialized) {
			LOG("Program tried to initialize twice, returning true");
			return RZRESULT_SUCCESS;
		}

		LOG("Init Called");
		if (program_name != "Artemis.UI.exe") {
			artemisPipeClient.Connect(L"\\\\.\\pipe\\Artemis\\Razer");

			if (artemisPipeClient.IsConnected()) {
				const char* name = program_name.c_str();
				const uint32_t nameLength = (uint32_t)strlen(name) + 1;

				const auto buffer = RazerBuffer::create_dynamic<RazerCommand::CommandInit>(name, nameLength);

				artemisPipeClient.Write(buffer.data(), buffer.size());

				isInitialized = true;
				return RZRESULT_SUCCESS;
			}
		}
		else {
			LOG(std::format("Program name {} blacklisted.", program_name));
		}

		isInitialized = false;
		return RZRESULT_SERVICE_NOT_ACTIVE;
	}

	RZRESULT InitSDK(ChromaSDK::APPINFOTYPE* pAppInfo)
	{
		return Init();
		if (!artemisPipeClient.IsConnected())
			return RZRESULT_SERVICE_NOT_ACTIVE;

		const auto buffer = RazerBuffer::create<RazerCommand::CommandInitSDK>(*pAppInfo);
		artemisPipeClient.Write(buffer.data(), buffer.size());
		return RZRESULT_SUCCESS;
	}

	RZRESULT UnInit()
	{
		LOG("UnInit Called");

		if (!artemisPipeClient.IsConnected())
			return RZRESULT_SERVICE_NOT_ACTIVE;

		const auto buffer = RazerBuffer::create<RazerCommand::CommandUnInit>();
		artemisPipeClient.Write(buffer.data(), buffer.size());
		artemisPipeClient.Disconnect();
		isInitialized = false;
		return RZRESULT_SUCCESS;
	}

	RZRESULT CreateEffect(RZDEVICEID DeviceId, ChromaSDK::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
	{
		LOG("CreateEffect");

		if (!artemisPipeClient.IsConnected())
			return RZRESULT_SERVICE_NOT_ACTIVE;

		EnsureValidGuid(pEffectId);

		if (pParam == nullptr) {
			LOG("pParam was null, returning RZRESULT_INVALID_PARAMETER");
			return RZRESULT_INVALID_PARAMETER;
		}

		switch (Effect)
		{
		case ChromaSDK::CHROMA_WAVE:
		{
			const auto eff = static_cast<ChromaSDK::WAVE_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateEffect>(
				DeviceId,
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::CHROMA_SPECTRUMCYCLING:
		{
			const auto eff = static_cast<ChromaSDK::SPECTRUMCYCLING_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateEffect>(
				DeviceId,
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::CHROMA_BREATHING:
		{
			const auto eff = static_cast<ChromaSDK::BREATHING_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateEffect>(
				DeviceId,
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::CHROMA_BLINKING:
		{
			const auto eff = static_cast<ChromaSDK::BLINKING_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateEffect>(
				DeviceId,
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::CHROMA_REACTIVE:
		{
			const auto eff = static_cast<ChromaSDK::REACTIVE_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateEffect>(
				DeviceId,
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::CHROMA_STATIC:
		{
			const auto eff = static_cast<ChromaSDK::STATIC_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateEffect>(
				DeviceId,
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::CHROMA_CUSTOM:
		{
			const auto eff = static_cast<ChromaSDK::CUSTOM_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateEffect>(
				DeviceId,
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		default:
		{
			return RZRESULT_SUCCESS;
		}
		}

		return RZRESULT_SUCCESS;
	}

	RZRESULT CreateKeyboardEffect(ChromaSDK::Keyboard::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
	{
		LOG("CreateKeyboardEffect");

		if (!artemisPipeClient.IsConnected())
			return RZRESULT_SERVICE_NOT_ACTIVE;

		if (pParam == nullptr) {
			LOG("pParam was null, returning RZRESULT_INVALID_PARAMETER");
			return RZRESULT_INVALID_PARAMETER;
		}

		EnsureValidGuid(pEffectId);

		switch (Effect)
		{
		case ChromaSDK::Keyboard::CHROMA_BREATHING:
		{
			const auto eff = static_cast<ChromaSDK::Keyboard::BREATHING_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateKeyboardEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Keyboard::CHROMA_CUSTOM:
		{
			const auto eff = static_cast<ChromaSDK::Keyboard::CUSTOM_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateKeyboardEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Keyboard::CHROMA_REACTIVE:
		{
			const auto eff = static_cast<ChromaSDK::Keyboard::REACTIVE_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateKeyboardEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Keyboard::CHROMA_STATIC:
		{
			const auto eff = static_cast<ChromaSDK::Keyboard::STATIC_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateKeyboardEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Keyboard::CHROMA_WAVE:
		{
			const auto eff = static_cast<ChromaSDK::Keyboard::WAVE_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateKeyboardEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Keyboard::CHROMA_CUSTOM_KEY:
		{
			const auto eff = static_cast<struct ChromaSDK::Keyboard::CUSTOM_KEY_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateKeyboardEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Keyboard::CHROMA_CUSTOM2:
		{
			const auto eff = static_cast<ChromaSDK::Keyboard::v2::CUSTOM_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateKeyboardEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		default:
		{
			return RZRESULT_SUCCESS;
		}
		}

		return RZRESULT_SUCCESS;
	}

	RZRESULT CreateHeadsetEffect(ChromaSDK::Headset::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
	{
		LOG("CreateHeadsetEffect");

		if (!artemisPipeClient.IsConnected())
			return RZRESULT_SERVICE_NOT_ACTIVE;

		EnsureValidGuid(pEffectId);

		if (pParam == nullptr) {
			LOG("pParam was null, returning RZRESULT_INVALID_PARAMETER");
			return RZRESULT_INVALID_PARAMETER;
		}

		switch (Effect)
		{
		case ChromaSDK::Headset::CHROMA_STATIC:
		{
			const auto eff = static_cast<ChromaSDK::Headset::STATIC_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateHeadsetEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Headset::CHROMA_BREATHING:
		{
			const auto eff = static_cast<ChromaSDK::Headset::BREATHING_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateHeadsetEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Headset::CHROMA_CUSTOM:
		{
			const auto eff = static_cast<ChromaSDK::Headset::CUSTOM_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateHeadsetEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		default:
		{
			return RZRESULT_SUCCESS;
		}
		}

		return RZRESULT_SUCCESS;
	}

	RZRESULT CreateMousepadEffect(ChromaSDK::Mousepad::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
	{
		LOG("CreateMousepadEffect");

		if (!artemisPipeClient.IsConnected())
			return RZRESULT_SERVICE_NOT_ACTIVE;

		EnsureValidGuid(pEffectId);

		if (pParam == nullptr) {
			LOG("pParam was null, returning RZRESULT_INVALID_PARAMETER");
			return RZRESULT_INVALID_PARAMETER;
		}

		switch (Effect)
		{
		case ChromaSDK::Mousepad::CHROMA_BREATHING:
		{
			const auto eff = static_cast<ChromaSDK::Mousepad::BREATHING_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMousepadEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Mousepad::CHROMA_CUSTOM:
		{
			const auto eff = static_cast<ChromaSDK::Mousepad::CUSTOM_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMousepadEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Mousepad::CHROMA_STATIC:
		{
			const auto eff = static_cast<ChromaSDK::Mousepad::STATIC_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMousepadEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Mousepad::CHROMA_WAVE:
		{
			const auto eff = static_cast<ChromaSDK::Mousepad::WAVE_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMousepadEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Mousepad::CHROMA_CUSTOM2:
		{
			const auto eff = static_cast<ChromaSDK::Mousepad::v2::CUSTOM_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMousepadEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		default:
		{
			return RZRESULT_SUCCESS;
		}
		}

		return RZRESULT_SUCCESS;
	}

	RZRESULT CreateMouseEffect(ChromaSDK::Mouse::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
	{
		LOG("CreateMouseEffect");

		if (!artemisPipeClient.IsConnected())
			return RZRESULT_SERVICE_NOT_ACTIVE;

		EnsureValidGuid(pEffectId);

		if (pParam == nullptr) {
			LOG("pParam was null, returning RZRESULT_INVALID_PARAMETER");
			return RZRESULT_INVALID_PARAMETER;
		}

		switch (Effect)
		{
		case ChromaSDK::Mouse::CHROMA_BLINKING:
		{
			const auto eff = static_cast<ChromaSDK::Mouse::BLINKING_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMouseEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Mouse::CHROMA_BREATHING:
		{
			const auto eff = static_cast<ChromaSDK::Mouse::BREATHING_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMouseEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Mouse::CHROMA_CUSTOM:
		{
			const auto eff = static_cast<ChromaSDK::Mouse::CUSTOM_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMouseEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Mouse::CHROMA_REACTIVE:
		{
			const auto eff = static_cast<ChromaSDK::Mouse::REACTIVE_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMouseEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Mouse::CHROMA_SPECTRUMCYCLING:
		{
			const auto eff = static_cast<ChromaSDK::Mouse::SPECTRUMCYCLING_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMouseEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Mouse::CHROMA_STATIC:
		{
			const auto eff = static_cast<ChromaSDK::Mouse::STATIC_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMouseEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Mouse::CHROMA_WAVE:
		{
			const auto eff = static_cast<ChromaSDK::Mouse::WAVE_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMouseEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Mouse::CHROMA_CUSTOM2:
		{
			const auto eff = static_cast<ChromaSDK::Mouse::CUSTOM_EFFECT_TYPE2*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateMouseEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		default:
		{
			return RZRESULT_SUCCESS;
		}
		}

		return RZRESULT_SUCCESS;
	}

	RZRESULT CreateKeypadEffect(ChromaSDK::Keypad::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
	{
		LOG("CreateKeypadEffect");

		if (!artemisPipeClient.IsConnected())
			return RZRESULT_SERVICE_NOT_ACTIVE;

		EnsureValidGuid(pEffectId);

		if (pParam == nullptr) {
			LOG("pParam was null, returning RZRESULT_INVALID_PARAMETER");
			return RZRESULT_INVALID_PARAMETER;
		}

		switch (Effect)
		{
		case ChromaSDK::Keypad::CHROMA_BREATHING:
		{
			const auto eff = static_cast<ChromaSDK::Keypad::BREATHING_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateKeypadEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Keypad::CHROMA_CUSTOM:
		{
			const auto eff = static_cast<ChromaSDK::Keypad::CUSTOM_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateKeypadEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Keypad::CHROMA_REACTIVE:
		{
			const auto eff = static_cast<ChromaSDK::Keypad::REACTIVE_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateKeypadEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Keypad::CHROMA_STATIC:
		{
			const auto eff = static_cast<ChromaSDK::Keypad::STATIC_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateKeypadEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::Keypad::CHROMA_WAVE:
		{
			const auto eff = static_cast<ChromaSDK::Keypad::WAVE_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateKeypadEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		default:
		{
			return RZRESULT_SUCCESS;
		}
		}

		return RZRESULT_SUCCESS;
	}

	RZRESULT CreateChromaLinkEffect(ChromaSDK::ChromaLink::EFFECT_TYPE Effect, PRZPARAM pParam, RZEFFECTID* pEffectId)
	{
		LOG("CreateChromaLinkEffect");

		if (!artemisPipeClient.IsConnected())
			return RZRESULT_SERVICE_NOT_ACTIVE;

		EnsureValidGuid(pEffectId);

		if (pParam == nullptr) {
			LOG("pParam was null, returning RZRESULT_INVALID_PARAMETER");
			return RZRESULT_INVALID_PARAMETER;
		}

		switch (Effect)
		{
		case ChromaSDK::ChromaLink::CHROMA_CUSTOM:
		{
			const auto eff = static_cast<ChromaSDK::ChromaLink::CUSTOM_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateChromaLinkEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		case ChromaSDK::ChromaLink::CHROMA_STATIC:
		{
			const auto eff = static_cast<ChromaSDK::ChromaLink::STATIC_EFFECT_TYPE*>(pParam);
			const auto buffer = RazerBuffer::create<RazerCommand::CommandCreateChromaLinkEffect>(
				Effect,
				*eff,
				pEffectId == nullptr ? GUID_NULL : *pEffectId);
			artemisPipeClient.Write(buffer.data(), buffer.size());
			break;
		}
		default:
		{
			return RZRESULT_SUCCESS;
		}
		}

		return RZRESULT_SUCCESS;
	}

	RZRESULT SetEffect(RZEFFECTID EffectId)
	{
		LOG("SetEffect");

		if (!artemisPipeClient.IsConnected())
			return RZRESULT_SERVICE_NOT_ACTIVE;

		const auto buffer = RazerBuffer::create<RazerCommand::CommandSetEffect>(EffectId);
		artemisPipeClient.Write(buffer.data(), buffer.size());

		return RZRESULT_SUCCESS;
	}

	RZRESULT DeleteEffect(RZEFFECTID EffectId)
	{
		LOG("DeleteEffect");

		if (!artemisPipeClient.IsConnected())
			return RZRESULT_SERVICE_NOT_ACTIVE;

		const auto buffer = RazerBuffer::create<RazerCommand::CommandDeleteEffect>(EffectId);
		artemisPipeClient.Write(buffer.data(), buffer.size());

		return RZRESULT_SUCCESS;
	}

	RZRESULT RegisterEventNotification(HWND hWnd)
	{
		LOG("RegisterEventNotification");

		if (!isInitialized)
			return RZRESULT_NOT_VALID_STATE;

		return RZRESULT_SUCCESS;
	}

	RZRESULT UnregisterEventNotification()
	{
		LOG("UnregisterEventNotification");

		if (!isInitialized)
			return RZRESULT_NOT_VALID_STATE;

		return RZRESULT_SUCCESS;
	}

	RZRESULT QueryDevice(RZDEVICEID DeviceId, ChromaSDK::DEVICE_INFO_TYPE& DeviceInfo)
	{
		LOG("QueryDevice");
		if (!isInitialized)
			return RZRESULT_SERVICE_NOT_ACTIVE;

		if (DeviceId == ChromaSDK::BLACKWIDOW_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYBOARD;
		}
		else if (DeviceId == BLACKWIDOW_CHROMA_TE)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYBOARD;
		}
		else if (DeviceId == DEATHSTALKER_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYBOARD;
		}
		else if (DeviceId == OVERWATCH_KEYBOARD)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYBOARD;
		}
		else if (DeviceId == BLACKWIDOW_X_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYBOARD;
		}
		else if (DeviceId == BLACKWIDOW_X_TE_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYBOARD;
		}
		else if (DeviceId == ORNATA_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYBOARD;
		}
		else if (DeviceId == BLADE_STEALTH)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYBOARD;
		}
		else if (DeviceId == BLADE)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYBOARD;
		}
		else if (DeviceId == BLADE_PRO)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYBOARD;
		}
		else if (DeviceId == HUNTSMAN)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYBOARD;
		}
		else if (DeviceId == BLACKWIDOW_ELITE)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYBOARD;
		}
		else if (DeviceId == ChromaSDK::DEATHADDER_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_MOUSE;
		}
		else if (DeviceId == MAMBA_CHROMA_TE)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_MOUSE;
		}
		else if (DeviceId == DIAMONDBACK_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_MOUSE;
		}
		else if (DeviceId == MAMBA_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_MOUSE;
		}
		else if (DeviceId == NAGA_EPIC_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_MOUSE;
		}
		else if (DeviceId == NAGA_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_MOUSE;
		}
		else if (DeviceId == OROCHI_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_MOUSE;
		}
		else if (DeviceId == NAGA_HEX_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_MOUSE;
		}
		else if (DeviceId == DEATHADDER_ELITE_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_MOUSE;
		}
		else if (DeviceId == KRAKEN71_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_HEADSET;
		}
		else if (DeviceId == MANOWAR_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_HEADSET;
		}
		else if (DeviceId == KRAKEN71_REFRESH_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_HEADSET;
		}
		else if (DeviceId == KRAKEN_KITTY)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_HEADSET;
		}
		else if (DeviceId == FIREFLY_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_MOUSEPAD;
		}
		else if (DeviceId == TARTARUS_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYPAD;
		}
		else if (DeviceId == ORBWEAVER_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_KEYPAD;
		}
		else if (DeviceId == LENOVO_Y900)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_SYSTEM;
		}
		else if (DeviceId == LENOVO_Y27)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_SYSTEM;
		}
		else if (DeviceId == CORE_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_SYSTEM;
		}
		else if (DeviceId == CHROMABOX)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_SYSTEM;
		}
		else if (DeviceId == NOMMO_CHROMA)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_SPEAKERS;
		}
		else if (DeviceId == NOMMO_CHROMA_PRO)
		{
			DeviceInfo.Connected = 1;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_SPEAKERS;
		}
		else
		{
			LOG("Unknown deviceId requested, returning Connected = 0");
			DeviceInfo.Connected = 0;
			DeviceInfo.DeviceType = ChromaSDK::DEVICE_INFO_TYPE::DEVICE_INVALID;
		}

		return RZRESULT_SUCCESS;
	}

#ifdef __cplusplus
}
#endif