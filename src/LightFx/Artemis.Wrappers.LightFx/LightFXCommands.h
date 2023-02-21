#pragma once
enum LightFXCommand : unsigned int {
	Initialize = 1,
	Reset,
	Update,
	UpdateDefault,
	SetLightColor,
	Light,
	SetLightActionColor,
	SetLightActionColorEx,
	ActionColor,
	ActionColorEx,
	SetTiming
};