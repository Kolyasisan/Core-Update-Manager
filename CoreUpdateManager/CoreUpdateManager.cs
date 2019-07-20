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
//Basic usage:
//-Use CoreMonoBeh instead of MonoBehaviour
//-Override the CoreInitSetup() method and generate a LoopUpdateSettings struct inside it with settings for your loops
//-Use override functions that start with Core instead of Unity's marshalled calls (for example CoreAwake instead of Awake)
//
//See some examples in the Examples folder.
//Also look at TestBehaviour for a code sample.
//
//Read the Documentation.md file for more info.

using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public class CoreUpdateManager : MonoBehaviour
{
    public static CoreUpdateManager instance { get; private set; }

    #region WorkerQueues
    private static BehaviourQueueBase additionQueue = new BehaviourQueueBase();
    private static BehaviourQueueBase removalQueue = new BehaviourQueueBase();
    #endregion

    #region MainQueues
    [SerializeField] private UpdateQueue updateQueue = new UpdateQueue();
    [SerializeField] private GameplayUpdateQueue gameplayUpdateQueue = new GameplayUpdateQueue();
    [SerializeField] private FixedUpdateQueue fixedUpdateQueue = new FixedUpdateQueue();
    #endregion

    #region init
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        //Update manager init
        GameObject go = new GameObject();
        go.name = "CoreUpdateManager";
        instance = go.AddComponent<CoreUpdateManager>();
        DontDestroyOnLoad(go);
    }
    #endregion

    #region corefunctions
    private void LateUpdate()
    {
        //Now, there may be some situations when we'll need to do additions and removals twice in a loop.
        //That's fairly normal since before every check there's an early-out bool check implemented.

        ProcessAdditions();
        ProcessRemovals();
        ProcessSortsOnLoops();

        updateQueue.Perform();
        gameplayUpdateQueue.Perform();
    }

    private void Update()
    {
        ProcessAdditions();
        ProcessRemovals();
        ProcessSortsOnLoops();

        fixedUpdateQueue.Perform();
    }

    private void ProcessAdditions()
    {
        if (additionQueue.HasNewEntries())
        {
            for (int i = additionQueue.LowerBound + 1; i < additionQueue.UpperBound; i++)
            {
                CoreMonoBeh workerBeh = additionQueue.queue[i];

                if (workerBeh.UM_SETTINGS_UPDATE.isInited)
                {
                    updateQueue.AddBehaviourAndSort(workerBeh, workerBeh.UM_SETTINGS_UPDATE);
                }

                if (workerBeh.UM_SETTINGS_GAMEPLAYUPDATE.isInited)
                {
                    gameplayUpdateQueue.AddBehaviourAndSort(workerBeh, workerBeh.UM_SETTINGS_GAMEPLAYUPDATE);
                }

                if (workerBeh.UM_SETTINGS_FIXEDUPDATE.isInited)
                {
                    fixedUpdateQueue.AddBehaviourAndSort(workerBeh, workerBeh.UM_SETTINGS_FIXEDUPDATE);
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

                if (workerBeh.UM_SETTINGS_UPDATE.isInited)
                {
                    updateQueue.RemoveBehaviour(workerBeh, workerBeh.UM_SETTINGS_UPDATE);
                }

                if (workerBeh.UM_SETTINGS_GAMEPLAYUPDATE.isInited)
                {
                    gameplayUpdateQueue.RemoveBehaviour(workerBeh, workerBeh.UM_SETTINGS_GAMEPLAYUPDATE);
                }

                if (workerBeh.UM_SETTINGS_FIXEDUPDATE.isInited)
                {
                    fixedUpdateQueue.RemoveBehaviour(workerBeh, workerBeh.UM_SETTINGS_FIXEDUPDATE);
                }
            }
        }
    }

    private void ProcessSortsOnLoops()
    {
        updateQueue.SortBehaviours();
        gameplayUpdateQueue.SortBehaviours();
    }

    /// <summary>
    /// Will add the specified behaviour to the update loops at the end of the current frame.
    /// </summary>
    public static void ScheduleBehaviourRegister(CoreMonoBeh beh)
    {
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