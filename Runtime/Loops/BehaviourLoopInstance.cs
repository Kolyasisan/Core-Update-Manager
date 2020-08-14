//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class BehaviourLoopInstance<T> : BehaviourLoopBase where T : class, IUpdatableBase
{
    /// <summary>
    /// This array holds sorted behaviours and calls functions on them.
    /// </summary>
    public T[] Queue { get; private set; } = new T[512];

    /// <summary>
    /// This array holds yet to be processed behaviours.
    /// </summary>
    public T[] AdditionQueue { get; private set; } = new T[512];

    /// <summary>
    /// Register the loop instance to the CoreUpdateManager scripts. Used for debug purposes.
    /// </summary>
    protected void RegisterLoopToUpdateManager()
    {
        CoreUpdateManager.TryInitialize();
        CoreUpdateManager.Instance.BehaviourQueues.Add(this);
    }

    /// <summary>
    /// Enqueue a behaviour to the addition queue.
    /// <para/>The behaviour will be added to the loop on the next <see cref="ProcessEnqueuedBehaviours"/> call.
    /// <para/>See also <seealso cref="PerformManagingRoutine"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnqueueBehaviour(T beh)
    {
        CheckAdditionQueueForOverflowAndExpand();
        AdditionQueue[AdditionQueueAmount] = beh;
        AdditionQueueAmount++;
        HasEnqueuedEntries = true;
    }

    /// <summary>
    /// Instantly tries to add a behaviour to the queue.
    /// <para/>It is highly recommended to use <see cref="EnqueueBehaviour"/> instead.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBehaviourToQueue(T beh, UpdateLoopSettings settings)
    {
        if (!settings.IsValid || !settings.ShouldBeRegistered)
            return;

        //Safety check for overflow
        CheckForOverflowAndExpand();

        //Just throw it somewhere in case if the entire queue is already marked for sorting
        if (NeedsSorting)
        {
            Queue[UpperBound] = beh;
            UpperBound++;
        }
        else if (settings.UpdateOrder >= UpperUpdateQueueBound)
        {
            Queue[UpperBound] = beh;
            UpperBound++;
            UpperUpdateQueueBound = settings.UpdateOrder;
        }
        else if (settings.UpdateOrder <= LowerUpdateQueueBound && LowerBound > -1)
        {
            Queue[LowerBound] = beh;
            LowerBound--;
            LowerUpdateQueueBound = settings.UpdateOrder;
        }
        //It's somewhere inbetween. Sigh, sorting...
        else
        {
            Queue[UpperBound] = beh;
            UpperBound++;
            NeedsSorting = true;
        }

        if (!HasEntries)
        {
            LowerUpdateQueueBound = UpperUpdateQueueBound;
            HasEntries = true;
        }
    }

    /// <summary>
    /// Schedules the removal of behaviours that are present in the loop and have shouldberegistered variable in their structs set to false.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void RemoveBehavioursMarkedForRemoval()
    {
        if (!NeedsRemovals || !HasEntries)
            return;

        int virtI = 0;
        int initialUpperBound = UpperBound;

        for (int i = LowerBound + 1; i < initialUpperBound; i++)
        {
            UpdateLoopSettings settings = GetSettings(Queue[i]);
            Queue[i - virtI] = Queue[i];

            if (!settings.ShouldBeRegistered || !settings.IsValid)
            {
                virtI++;
                UpperBound--;
            }
        }

        for (int i = UpperBound; i < initialUpperBound; i++)
        {
            Queue[i] = null;
        }

        if (LowerBound + 1 >= UpperBound)
        {
            LowerBound = -1;
            UpperBound = 0;
            HasEntries = false;
        }
        else
        {
            UpperUpdateQueueBound = GetSettings(Queue[UpperBound - 1]).UpdateOrder;
        }

        NeedsRemovals = false;
    }

    /// <summary>
    /// Sorts the behaviours based on their update orders.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SortBehaviours()
    {
        if (!NeedsSorting)
            return;

        for (int i = LowerBound + 1; i < UpperBound; i++)
        {
            var temp = Queue[i];
            int j = i - 1;

            while (j >= 0 && GetSettings(Queue[j]).UpdateOrder > GetSettings(temp).UpdateOrder)
            {
                Queue[j + 1] = Queue[j];
                j--;
            }

            Queue[j + 1] = temp;
        }

        NeedsSorting = false;
    }

    /// <summary>
    /// Adds enqueued behaviours to the loop. Should not be called directly.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void ProcessEnqueuedBehaviours()
    {
        if (!HasEnqueuedEntries)
            return;

        for (int i = 0; i < AdditionQueueAmount; i++)
        {
            AddBehaviourToQueue(AdditionQueue[i], GetSettings(AdditionQueue[i]));
        }

        WipeAdditionQueue();
    }

    /// <summary>
    /// Performs basic routine with the loop, like adding enqueued behaviours, sorting and removals.
    /// Should be called before and after doing polling.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void PerformManagingRoutine()
    {
        RemoveBehavioursMarkedForRemoval();
        ProcessEnqueuedBehaviours();
        SortBehaviours();
    }

    /// <summary>
    /// Wipes the queue.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void WipeQueue()
    {
        for (int i = LowerBound + 1; i < UpperBound; i++)
        {
            Queue[i] = null;
        }

        LowerBound = -1;
        UpperBound = 0;
        HasEntries = false;
    }

    /// <summary>
    /// Wipes the addition queue. Called afer processing all the additions in the queue in a batch.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WipeAdditionQueue()
    {
        for (int i = 0; i < AdditionQueueAmount; i++)
        {
            AdditionQueue[i] = null;
        }

        AdditionQueueAmount = 0;
        HasEnqueuedEntries = false;
    }

    /// <summary>
    /// Expands the queue to be double the size in case if there's not enough space. Generates garbage!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CheckForOverflowAndExpand()
    {
        if (UpperBound >= Queue.Length)
        {
            T[] newqueue = new T[Queue.Length * 2];
            Queue.CopyTo(newqueue, 0);
            Queue = newqueue;
        }
    }

    /// <summary>
    /// Expands the addition queue to be double the size in case if there's not enough space. Generates garbage!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CheckAdditionQueueForOverflowAndExpand()
    {
        if (AdditionQueueAmount >= AdditionQueue.Length)
        {
            T[] newqueue = new T[AdditionQueue.Length * 2];
            AdditionQueue.CopyTo(newqueue, 0);
            AdditionQueue = newqueue;
        }
    }

    /// <summary>
    /// Gets relative settings of a behaviour.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract UpdateLoopSettings GetSettings(T beh);

    /// <summary>
    /// Writes relative settings to a behaviour.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void WriteSettings(UpdateLoopSettings config, T beh);

    /// <summary>
    /// Gets the settings of a boxed generic object. Mostly used for debugging.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override UpdateLoopSettings GetSettings(IUpdatableBase beh)
    {
        return GetSettings((T)beh);
    }

    /// <summary>
    /// Gets the generic queue. Mostly used for debugging.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override IUpdatableBase[] GetQueue()
    {
        return Queue;
    }
}
