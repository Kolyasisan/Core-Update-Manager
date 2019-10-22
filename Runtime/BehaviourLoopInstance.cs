//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

//The update manager executes calls inside Try-Catch blocks in order to deal with exceptions.
//You can comment-out this line to gain minor performance, but any exception will halt the loop entirely, which can lead to a softlock.
#define UPDATEMANAGER_USETRYCATCH

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// A general queue for storing the behaviours.
/// </summary>
[System.Serializable] //System.Serializable is put here for debugging purpouses (see CoreUpdateManager script).
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public abstract class BehaviourLoopInstance : MonoBehaviour
{
    #region variables

    public CoreMonoBeh[] queue
    {
        get { return m_queue; }
    }
    private CoreMonoBeh[] m_queue = new CoreMonoBeh[CoreUpdateManager.arrayInitLength];

    /// <summary>
    /// Represents the highest unoccupied slot.
    /// </summary>
    public int UpperBound
    {
        get { return m_UpperBound; }
    }
    private int m_UpperBound = 0;

    /// <summary>
    /// Represents the lowest unoccupied slot. If -1, then there are no such slots.
    /// </summary>
    public int LowerBound
    {
        get { return m_LowerBound; }
    }
    private int m_LowerBound = -1;

    /// <summary>
    /// Represents the highest execution order value that one of the scripts may have in this queue.
    /// </summary>
    public byte UpperUpdateQueueBound
    {
        get { return m_UpperUpdateQueueBound; }
    }
    private byte m_UpperUpdateQueueBound = 0;

    /// <summary>
    /// Represents the lowest execution order value that one of the scripts may have in this queue.
    /// </summary>
    public byte LowerUpdateQueueBound
    {
        get { return m_LowerUpdateQueueBound; }
    }
    private byte m_LowerUpdateQueueBound = 0;

    /// <summary>
    /// Returns true if there is at least one behaviour present in the queue.
    /// </summary>
    public bool HasEntries
    {
        get { return m_HasEntries; }
    }
    private bool m_HasEntries = false;

    private bool NeedsSorting = false;

    #endregion

    #region internal functions

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBehaviour(CoreMonoBeh beh)
    {
        //Safety check for overflow
        CheckForOverflowAndExpand();

        //Lower empty fields take priority
        if (LowerBound > -1)
        {
            queue[LowerBound] = beh;
            m_LowerBound--;
        }
        else
        {
            queue[UpperBound] = beh;
            m_UpperBound++;
        }

        if (!HasEntries)
        {
            m_LowerUpdateQueueBound = m_UpperUpdateQueueBound;
            m_HasEntries = true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBehaviourOnTop(CoreMonoBeh beh)
    {
        //Safety check for overflow
        CheckForOverflowAndExpand();

        queue[m_UpperBound] = beh;
        m_UpperBound++;

        if (!HasEntries)
        {
            m_LowerUpdateQueueBound = m_UpperUpdateQueueBound;
            m_HasEntries = true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBehaviourAndSort(CoreMonoBeh beh)
    {
        //Safety check for overflow
        CheckForOverflowAndExpand();

        LoopUpdateSettings settings = GetLoopSettings(beh);

        //Just throw it somewhere if the queue is already marked for sorting.
        //A nice early-out for batched additions.
        if (NeedsSorting)
        {
            queue[m_UpperBound] = beh;
            m_UpperBound++;
            return;
        }
        else if (settings.UpdateOrder >= UpperUpdateQueueBound)
        {
            queue[m_UpperBound] = beh;
            m_UpperBound++;
            m_UpperUpdateQueueBound = settings.UpdateOrder;
        }
        else if (settings.UpdateOrder <= LowerUpdateQueueBound && m_LowerBound > -1)
        {
            queue[m_LowerBound] = beh;
            m_LowerBound--;
            m_LowerUpdateQueueBound = settings.UpdateOrder;
        }
        //It's somewhere inbetween or where we can't place legally. Mark it for sorting.
        else
        {
            queue[m_UpperBound] = beh;
            m_UpperBound++;
            NeedsSorting = true;
        }

        if (!HasEntries)
        {
            m_LowerUpdateQueueBound = m_UpperUpdateQueueBound;
            m_HasEntries = true;
        }
    }

    /// <summary>
    /// Check if the bounds fall inside the array size. If not, then the array will be expanded.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckForOverflowAndExpand()
    {
        if (UpperBound >= m_queue.Length)
        {
            Array.Resize(ref m_queue, m_queue.Length * 2);
        }
    }

    /// <summary>
    /// Removes a behaviour from the queue. Due to performance reasons it is heavily advised that you use RemoveBehavioursBatched().
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveBehaviourSingular(CoreMonoBeh beh)
    {
        int j = 0;
        int cnt = m_UpperBound;
        bool foundOne = false;

        for (int i = m_LowerBound + 1; i < m_UpperBound; i++)
        {
            if (queue[i] == beh)
            {
                j++;
                m_UpperBound--;
                foundOne = true;
            }

            queue[i] = queue[i + j];
        }

        if (foundOne)
        {
            queue[m_UpperBound] = null;

            if (m_LowerBound + 1 >= m_UpperBound)
            {
                m_LowerBound = -1;
                m_UpperBound = 0;
                m_HasEntries = false;
            }
            else
            {
                m_UpperUpdateQueueBound = GetLoopSettings(queue[m_UpperBound - 1]).UpdateOrder;
            }
        }
    }

    /// <summary>
    /// Sweeps the entire queue and removes behaviours that are marked as destroyed.
    /// Designed for automatic use with CoreUpdateManager script when the behaviours are removed when they are destroyed.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveBehavioursBatched()
    {
        int initialLowerBound = m_LowerBound;
        int initialTrueLowerBound = initialLowerBound + 1;
        int initialUpBound = m_UpperBound;
        int removalAmount = 0;

        //Sweep through the entire queue and check if behaviours are marked for deletion.
        for (int i = initialTrueLowerBound; i < initialUpBound; i++)
        {
            //If the behaviour is marked for deletion - offset the overwrite value so it would be removed later.
            if (queue[i].isMarkedForDeletion)
            {
                removalAmount++;
                m_UpperBound--;
            }
            //If we had no success removing the behaviour - leave it be, but offset its position in the array.
            else
                queue[i - removalAmount] = queue[i];
        }

        //Early-out if we had no success in finding suitable targets.
        if (removalAmount == 0)
            return;

        //Clear the fields that we've deemed to be removed.
        for (int i = m_UpperBound; i < initialUpBound; i++)
        {
            queue[i] = null;
        }

        //Update internal values.
        if (initialLowerBound + 1 >= m_UpperBound)
        {
            this.m_LowerBound = -1;
            this.m_UpperBound = 0;
            this.m_HasEntries = false;
        }
        else
        {
            this.m_UpperUpdateQueueBound = GetLoopSettings(queue[m_UpperBound - 1]).UpdateOrder;
        }

    }

    /// <summary>
    /// Sort the behaviours based on their execution order value in settings.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SortBehaviours()
    {
        if (!NeedsSorting)
            return;

#if UNITY_EDITOR
        Profiler.BeginSample("Entres Sorting");
#endif

        NeedsSorting = false;

        for (int i = LowerBound + 1; i < UpperBound; i++)
        {
            CoreMonoBeh temp = queue[i];
            int j = i - 1;

            while (j >= 0 && GetLoopSettings(queue[j]).UpdateOrder > GetLoopSettings(temp).UpdateOrder)
            {
                queue[j + 1] = queue[j];
                j--;
            }

            queue[j + 1] = temp;
        }

#if UNITY_EDITOR
        Profiler.EndSample();
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveLowerBehaviour()
    {
        m_LowerBound++;
        queue[m_LowerBound] = null;
        if (m_LowerBound + 1 >= m_UpperBound)
        {
            m_LowerBound = -1;
            m_UpperBound = 0;
            m_HasEntries = false;
        }
        else
        {
            m_LowerUpdateQueueBound = GetLoopSettings(queue[m_LowerUpdateQueueBound + 1]).UpdateOrder;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveUpperBehaviour()
    {
        queue[m_UpperBound] = null;
        m_UpperBound--;
        if (m_LowerBound + 1 >= m_UpperBound)
        {
            m_LowerBound = -1;
            m_UpperBound = 0;
            m_HasEntries = false;
        }
        else
        {
            m_UpperUpdateQueueBound = GetLoopSettings(queue[m_UpperBound - 1]).UpdateOrder;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WipeQueue()
    {
        Array.Clear(queue, m_LowerBound + 1, m_UpperBound - m_LowerBound - 1);

        m_LowerBound = -1;
        m_UpperBound = 0;
        m_HasEntries = false;
    }

    #endregion

    #region abstract functions

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract LoopUpdateSettings GetLoopSettings(CoreMonoBeh beh);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void WriteLoopSettings(CoreMonoBeh beh, LoopUpdateSettings set);

    /// <summary>
    /// Performs the entire queue poll and processess additions and removals for queues.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract void Perform();

    #endregion
}