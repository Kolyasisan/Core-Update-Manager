//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

using System.Runtime.CompilerServices;

/// <summary>
/// A basic struct that holds the config data for the behaviour.
/// </summary>
public struct LoopUpdateSettings
{
    /// <summary>
    /// Generates automatically when you generate a proper config. Used as a suitability for addition to a certain queue in the IsBehaviourSuitable() method.
    /// </summary>
    public bool isInited { get; private set; }

    /// <summary>
    /// If true then the method will be called. Otherwise it'll be skipped.
    /// </summary>
    public bool eligibleForUpdate { get; private set; }

    /// <summary>
    /// Components with lower values will be updated locally at earlier stages, in bytes. 128 represents the default value.
    /// </summary>
    public byte UpdateOrder { get; private set; }

    /// <summary>
    /// If set to false, then the component will not manage itself when it's enabled or disabled so that management lies on you.
    /// </summary>
    public bool autoManageEnableDisableEvents { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PerformEnableDisableRoutine(bool enabled)
    {
        if (isInited && autoManageEnableDisableEvents)
            eligibleForUpdate = enabled;
    }

    public LoopUpdateSettings(byte order = 128, bool autoManage = true, bool eligible = true)
    {
        isInited = true;
        eligibleForUpdate = eligible;
        UpdateOrder = order;
        autoManageEnableDisableEvents = autoManage;
    }
}