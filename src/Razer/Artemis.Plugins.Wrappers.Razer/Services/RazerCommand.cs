namespace Artemis.Plugins.Wrappers.Razer.Services
{
    internal enum RazerCommand : uint
    {
        Init = 1,
        InitSDK,
        UnInit,
        CreateEffect,
        CreateKeyboardEffect,
        CreateMouseEffect,
        CreateHeadsetEffect,
        CreateMousepadEffect,
        CreateKeypadEffect,
        CreateChromaLinkEffect,
        DeleteEffect,
        SetEffect,
        RegisterEventNotification,
        UnregisterEventNotification,
        QueryDevice
    }
}