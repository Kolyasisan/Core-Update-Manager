//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------
//  
//Core Update Manager is a performant update manager designed to mitigate the issues with Unity's marhsalled calls (Update, Fixed Update, etc.)
//
//For more background info read this (https://blogs.unity3d.com/2015/12/23/1k-update-calls/)
//
//Read the Documentation.md file for instructions.

using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using System.Collections.Generic;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public class CoreUpdateManager : MonoBehaviour
{
    public static CoreUpdateManager instance { get; private set; }
    public static GameObject coreUpdateManagerGO { get; private set; }
    private static bool isInited = false;

    #region WorkerQueues
    private static BehaviourLoopInstance additionQueue = null;
    private static BehaviourLoopInstance removalQueue = null;
    #endregion

    #region MainQueues
    /// <summary>
    /// Holds all the behaviour queues that the CoreMonoBehs can subscribe to.
    /// </summary>
    private List<BehaviourLoopInstance> behaviourQueues = new List<BehaviourLoopInstance>(16);

    #endregion

    #region init
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        if (isInited)
            return;

        isInited = true;

        //Update manager init
        coreUpdateManagerGO = new GameObject();
        coreUpdateManagerGO.name = "CoreUpdateManager";
        instance = coreUpdateManagerGO.AddComponent<CoreUpdateManager>();
        DontDestroyOnLoad(coreUpdateManagerGO);

        additionQueue = (BehaviourLoopInstance)CreateComponentAsGO<BehaviourLoopInstance>("additionQueue");
        removalQueue = (BehaviourLoopInstance)CreateComponentAsGO<BehaviourLoopInstance>("removalQueue");
    }

    //For those nice Domain Reloading features introduced in 2019.3
    private void OnDestroy()
    {
        isInited = false;
        instance = null;
    }

    public static BehaviourLoopInstance RegisterBehaviourQueue<T>(string name) where T : BehaviourLoopInstance
    {
        BehaviourLoopInstance queue = (BehaviourLoopInstance)CreateComponentAsGO<T>(name);
        instance.behaviourQueues.Add(queue);
        return queue;
    }

    public static MonoBehaviour CreateComponentAsGO<T>(string name) where T : MonoBehaviour
    {
        Init();
        GameObject go = new GameObject();
        DontDestroyOnLoad(go);
        go.name = name;
        MonoBehaviour comp = (MonoBehaviour)go.AddComponent(typeof(T));
        go.transform.parent = coreUpdateManagerGO.transform;
        return comp;
    }
    #endregion

    #region corefunctions

    private void ProcessAdditions()
    {
        if (additionQueue.HasNewEntries())
        {
            for (int i = additionQueue.LowerBound + 1; i < additionQueue.UpperBound; i++)
            {
                CoreMonoBeh workerBeh = additionQueue.queue[i];

                for (int j = 0; j < behaviourQueues.Count; j++)
                {
                    if (behaviourQueues[j].GetLoopSettings(workerBeh).isInited)
                    {
                        behaviourQueues[j].AddBehaviourAndSort(workerBeh, behaviourQueues[j].GetLoopSettings(workerBeh));
                    }
                }
            }

            additionQueue.WipeQueue();
        }
    }

    private void ProcessRemovals()
    {
        if (removalQueue.HasEntries)
        {
            for (int i = removalQueue.LowerBound + 1; i < removalQueue.UpperBound; i++)
            {
                CoreMonoBeh workerBeh = removalQueue.queue[i];

                for (int j = 0; j < behaviourQueues.Count; j++)
                {
                    if (behaviourQueues[j].GetLoopSettings(workerBeh).isInited)
                    {
                        behaviourQueues[j].RemoveBehaviour(workerBeh, behaviourQueues[j].GetLoopSettings(workerBeh));
                    }
                }
            }
        }
    }

    private void ProcessSortsOnLoops()
    {
        for (int i = 0; i < behaviourQueues.Count; i++)
        {
            behaviourQueues[i].SortBehaviours();
        }
    }

    public static void PerformUpdateManagerRoutine()
    {
        instance.ProcessAdditions();
        instance.ProcessRemovals();
        instance.ProcessSortsOnLoops();
    }

    /// <summary>
    /// Will add the specified behaviour to the update loops at the end of the current frame.
    /// </summary>
    public static void ScheduleBehaviourRegister(CoreMonoBeh beh)
    {
        Init();
        additionQueue.AddBehaviourOnTop(beh);
    }

    /// <summary>
    /// Will remove the specified behaviour from the update loops at the end of the current frame.
    /// </summary>
    public static void ScheduleBehaviourRemoval(CoreMonoBeh beh)
    {
        removalQueue.AddBehaviourOnTop(beh);
    }
    #endregion

}