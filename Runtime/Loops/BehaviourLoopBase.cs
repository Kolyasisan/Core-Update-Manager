//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

using System.Runtime.CompilerServices;

public abstract class BehaviourLoopBase
{
    /// <summary>
    /// Represents the highest unoccupied slot.
    /// </summary>
    public int UpperBound { get; protected set; } = 0;

    /// <summary>
    /// Represents the lowest unoccupied slot. If -1, then there are no such slots.
    /// </summary>
    public int LowerBound { get; protected set; } = -1;

    /// <summary>
    /// Represents the highest execution order value that one of the scripts may have in this queue.
    /// </summary>
    public int UpperUpdateQueueBound { get; protected set; } = 0;

    /// <summary>
    /// Represents the lowest execution order value that one of the scripts may have in this queue.
    /// </summary>
    public int LowerUpdateQueueBound { get; protected set; } = 0;

    public int AdditionQueueAmount { get; protected set; } = 0;
    public bool HasEnqueuedEntries { get; protected set; } = false;
    public bool NeedsSorting { get; protected set; } = false;
    public bool NeedsRemovals { get; set; } = false;
    public bool HasEntries { get; protected set; } = false;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void UpdateLoop();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void RemoveBehavioursMarkedForRemoval();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void ProcessEnqueuedBehaviours();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void Reinitialize();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void PerformManagingRoutine();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract IUpdatableBase[] GetQueue();
    public abstract UpdateLoopSettings GetSettings(IUpdatableBase beh);
}
