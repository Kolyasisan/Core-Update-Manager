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

using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using System;

/// <summary>
/// A general queue for storing the behaviours.
/// </summary>
[System.Serializable] //System.Serializable is put here for debugging purpouses.
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public class BehaviourLoopInstance : MonoBehaviour
{
    #region variables

    public static bool isInited = false;

    public CoreMonoBeh[] queue
    {
        get { return m_queue; }
    }
    private CoreMonoBeh[] m_queue = new CoreMonoBeh[512];

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

    private bool NewEntriesPresent = false;

    private bool NeedsSorting = false;

    #endregion

    #region internalfunctions

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

        NewEntriesPresent = true;

        if (!HasEntries)
        {
            m_LowerUpdateQueueBound = m_UpperUpdateQueueBound;
            m_HasEntries = true;
        }
    }

    public void AddBehaviourOnTop(CoreMonoBeh beh)
    {
        //Safety check for overflow
        CheckForOverflowAndExpand();

        queue[m_UpperBound] = beh;
        m_UpperBound++;
        NewEntriesPresent = true;


        if (!HasEntries)
        {
            m_LowerUpdateQueueBound = m_UpperUpdateQueueBound;
            m_HasEntries = true;
        }
    }

    public void AddBehaviourAndSort(CoreMonoBeh beh, LoopUpdateSettings settings)
    {
        //Safety check for overflow
        CheckForOverflowAndExpand();

        if (settings.UpdateOrder >= UpperUpdateQueueBound)
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
        //It's somewhere inbetween. Sigh, sorting...
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

        NewEntriesPresent = true;
    }

    public void CheckForOverflowAndExpand()
    {
        if (UpperBound >= queue.Length)
        {
            CoreMonoBeh[] newqueue = new CoreMonoBeh[queue.Length * 2];
            m_queue.CopyTo(newqueue, 0);
            m_queue = newqueue;

#if UNITY_EDITOR
            Debug.LogError("Queue overflow detected in an update manager queue! You might wanna raise the default value a bit.");
#endif
        }
    }

    public void RemoveBehaviour(CoreMonoBeh beh, LoopUpdateSettings settings)
    {
        int virtI = 0;
        int cnt = m_UpperBound;
        bool foundOne = false;

        for (int i = m_LowerBound + 1; i < m_UpperBound; i++)
        {
            if (queue[i] == beh)
            {
                virtI++;
                m_UpperBound--;
                foundOne = true;
            }

            queue[i] = queue[i + virtI];
        }

        if (foundOne)
        {
            queue[m_UpperBound] = null;

            if (m_LowerBound + 1 >= m_UpperBound)
            {
                m_LowerBound = -1;
                m_UpperBound = 0;
                m_HasEntries = false;
                NewEntriesPresent = false;
            }
            else
            {
                m_UpperUpdateQueueBound = GetLoopSettings(queue[m_UpperBound - 1]).UpdateOrder;
            }
        }
    }

    public void SortBehaviours()
    {
        if (!NeedsSorting)
            return;

        NeedsSorting = false;

        for (int i = LowerBound + 1; i < UpperBound; i++)
        {
            var temp = queue[i];
            int j = i - 1;

            while (j >= 0 && GetLoopSettings(queue[j]).UpdateOrder > GetLoopSettings(temp).UpdateOrder)
            {
                queue[j + 1] = queue[j];
                j--;
            }

            queue[j + 1] = temp;
        }
    }

    public void RemoveLowerBehaviour()
    {
        m_LowerBound++;
        queue[m_LowerBound] = null;
        if (m_LowerBound + 1 >= m_UpperBound)
        {
            m_LowerBound = -1;
            m_UpperBound = 0;
            m_HasEntries = false;
            NewEntriesPresent = false;
        }
        else
        {
            m_LowerUpdateQueueBound = GetLoopSettings(queue[m_LowerUpdateQueueBound + 1]).UpdateOrder;
        }
    }

    public void RemoveUpperBehaviour()
    {
        queue[m_UpperBound] = null;
        m_UpperBound--;
        if (m_LowerBound + 1 >= m_UpperBound)
        {
            m_LowerBound = -1;
            m_UpperBound = 0;
            m_HasEntries = false;
            NewEntriesPresent = false;
        }
        else
        {
            m_UpperUpdateQueueBound = GetLoopSettings(queue[m_UpperBound - 1]).UpdateOrder;
        }
    }

    public void WipeQueue()
    {
        for (int i = m_LowerBound + 1; i < m_UpperBound; i++)
        {
            queue[i] = null;
        }

        m_LowerBound = -1;
        m_UpperBound = 0;
        m_HasEntries = false;
        NewEntriesPresent = false;
    }

    public bool HasNewEntries()
    {
        if (NewEntriesPresent)
        {
            NewEntriesPresent = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion

    #region stubfunctions

    public virtual LoopUpdateSettings GetLoopSettings(CoreMonoBeh beh) { throw new System.Exception("Usage of base undefined getter"); }

    public virtual void WriteLoopSettings(CoreMonoBeh beh, LoopUpdateSettings set) { throw new System.Exception("Usage of base undefined setter"); }

    /// <summary>
    /// Performs the entire queue poll and processess additions and removals for queues.
    /// </summary>
    public virtual void Perform() { { throw new System.Exception("Usage of base undefined method"); } }

    #endregion
}