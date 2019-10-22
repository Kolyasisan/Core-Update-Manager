//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------
//  
//Core Update Manager is a fairly performant update manager designed to mitigate the issues with Unity's marhsalled calls (Update, Fixed Update, etc.).
//For more background info read this (https://blogs.unity3d.com/2015/12/23/1k-update-calls/).
//Read the repo wiki for documentation.

//Uncomment this define in order to make the loop objects visible in the editor and inspect their values.
#define UPDATEMANAGER_SHOWLOOPOBJECTS

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public class CoreUpdateManager : MonoBehaviour
{
    #region general variables

    public const int arrayInitLength = 4096;

    public static CoreUpdateManager instance { get; private set; }
    public static GameObject coreUpdateManagerGO { get; private set; }
    private static bool isInited = false;

    /// <summary>
    /// Holds all the behaviour queues that the CoreMonoBehs can subscribe to.
    /// </summary>
    private List<BehaviourLoopInstance> behaviourQueues = new List<BehaviourLoopInstance>(16);

    private bool removalsRequired = false;

    #endregion

    #region worker queues

    private static BehaviourLoopInstance additionQueue = null;


    #endregion

    #region init

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void Init()
    {
        if (isInited)
            return;

        isInited = true;

        //Update manager init
        coreUpdateManagerGO = new GameObject();
        coreUpdateManagerGO.name = "CoreUpdateManager";
        DontDestroyOnLoad(coreUpdateManagerGO);

#if UNITY_EDITOR && UPDATEMANAGER_SHOWLOOPOBJECTS

#else
        coreUpdateManagerGO.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
#endif

        instance = coreUpdateManagerGO.AddComponent<CoreUpdateManager>();

        additionQueue = (WorkerLoop)CreateComponentAsGO<WorkerLoop>("additionQueue");
        //removalQueue = (WorkerLoop)CreateComponentAsGO<WorkerLoop>("removalQueue");
    }

    //For those nice Domain Reloading features introduced in 2019.3
    private void OnDestroy()
    {
        //We do not clear Instance and worker queues because other scripts may be working with them, which may produce errors.

        isInited = false;
        coreUpdateManagerGO = null;
        behaviourQueues.Clear();
    }

    /// <summary>
    /// Registers the supplied loop as an UpdateLoop so CoreMonoBehs will interact with it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BehaviourLoopInstance RegisterBehaviourQueue<T>(string name) where T : BehaviourLoopInstance
    {
        BehaviourLoopInstance queue = (BehaviourLoopInstance)CreateComponentAsGO<T>(name);
        instance.behaviourQueues.Add(queue);
        return queue;
    }

    /// <summary>
    /// Creates a GameObject with a dedicated component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MonoBehaviour CreateComponentAsGO<T>(string name) where T : MonoBehaviour
    {
        Init();
        GameObject go = new GameObject();
        DontDestroyOnLoad(go);

#if UNITY_EDITOR && UPDATEMANAGER_SHOWLOOPOBJECTS

#else
        go.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
#endif

        go.name = name;
        MonoBehaviour comp = (MonoBehaviour)go.AddComponent(typeof(T));
        go.transform.parent = coreUpdateManagerGO.transform;
        return comp;
    }

    #endregion

    #region core functions

    /// <summary>
    /// Tracks and adds the behaviours to the loops.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessAdditions()
    {
        if (additionQueue.HasEntries)
        {
#if UNITY_EDITOR
            Profiler.BeginSample("Entries Additions");
#endif

            for (int i = additionQueue.LowerBound + 1; i < additionQueue.UpperBound; i++)
            {
                CoreMonoBeh workerBeh = additionQueue.queue[i];

                for (int j = 0; j < behaviourQueues.Count; j++)
                {
                    if (behaviourQueues[j].GetLoopSettings(workerBeh).isInited)
                    {
                        behaviourQueues[j].AddBehaviourAndSort(workerBeh);
                    }
                }
            }

            additionQueue.WipeQueue();

#if UNITY_EDITOR
            Profiler.EndSample();
#endif
        }
    }

    /// <summary>
    /// Tracks and removes the behaviours from the loops.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessRemovals()
    {
        if (removalsRequired)
        {
#if UNITY_EDITOR
            Profiler.BeginSample("Entries Removals");
#endif

            for (int i = 0; i < behaviourQueues.Count; i++)
            {
                behaviourQueues[i].RemoveBehavioursBatched();
            }

            removalsRequired = false;

#if UNITY_EDITOR
            Profiler.EndSample();
#endif
        }
    }

    /// <summary>
    /// Sorts the behaviours on all of the loops by their update orders.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessSortsOnLoops()
    {
#if UNITY_EDITOR
        Profiler.BeginSample("Entries Sorts On Loops");
#endif

        for (int i = 0; i < behaviourQueues.Count; i++)
        {
            behaviourQueues[i].SortBehaviours();
        }

#if UNITY_EDITOR
        Profiler.EndSample();
#endif
    }

    /// <summary>
    /// A set of cheap and trivial functions that track and perform changes in the manager.
    /// Should be called before and after a loop poll.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PerformUpdateManagerRoutine()
    {
        Profiler.BeginSample("Update Manager Routine");

        instance.ProcessAdditions();
        instance.ProcessRemovals();
        instance.ProcessSortsOnLoops();

        Profiler.EndSample();
    }

    /// <summary>
    /// Will add the specified behaviour to the update loops at the end of the current frame.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ScheduleBehaviourRegister(CoreMonoBeh beh)
    {
        Init();
        additionQueue.AddBehaviourOnTop(beh);
    }

    /// <summary>
    /// Will remove the specified behaviour from the update loops at the end of the current frame.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ScheduleBehaviourRemoval(CoreMonoBeh beh)
    {
        instance.removalsRequired = true;
    }

    #endregion
}