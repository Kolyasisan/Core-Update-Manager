//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

using System.Runtime.CompilerServices;
using UnityEngine;

public struct UpdateLoopSettings
{
    /// <summary>
    /// Represents the sorting order in the loops.
    /// Scripts with lower UpdateOrder will be excecuted earlier.
    /// </summary>
    public int UpdateOrder;

    /// <summary>
    /// True if a loop should call the method.
    /// </summary>
    public bool EligibleForUpdate;

    /// <summary>
    /// True if this script instance should be registered with the loop.
    /// False when it either shouldn't or should be removed from the loop.
    /// </summary>
    public bool ShouldBeRegistered;

    /// <summary>
    /// Automatically set to true upon properly creating a struct.
    /// If set to false, then this struct and its relative update function will not be used in the update manager.
    /// </summary>
    public bool IsValid { get; private set; }

    public UpdateLoopSettings(bool eligibleForUpdate = true, int updateOrder = 0)
    {
        EligibleForUpdate = eligibleForUpdate;
        UpdateOrder = updateOrder;
        ShouldBeRegistered = true;
        IsValid = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UpdateLoopSettings Create(MonoBehaviour beh, int updateOrder = 0)
    {
        return new UpdateLoopSettings(beh.isActiveAndEnabled, updateOrder);
    }
}
