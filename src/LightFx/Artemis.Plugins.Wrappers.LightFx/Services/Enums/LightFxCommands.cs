namespace Artemis.Plugins.Wrappers.LightFx.Services
{
    public enum LightFxCommand : uint
    {
        Initialize = 1,
        Release,
        Reset,
        Update,
        UpdateDefault,
        SetLightColor,
        Light,
        SetLightActionColor,
        SetLightActionColorEx,
        ActionColor,
        ActionColorEx
    }
}
