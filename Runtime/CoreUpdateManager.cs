//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CoreUpdateManager
{
    private static bool isInited = false;
    public static CoreUpdateManager Instance { get; private set; } = null;
    public List<BehaviourLoopBase> BehaviourQueues { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void TryInitialize()
    {
        if (isInited)
            return;

        isInited = true;
        Instance = new CoreUpdateManager();
        Instance.BehaviourQueues = new List<BehaviourLoopBase>(16);
    }

    public CoreUpdateManager()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += PlayModeChanged;
#endif
    }

    ~CoreUpdateManager()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= PlayModeChanged;
#endif
    }

#if UNITY_EDITOR
    private static void PlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            for (int i = 0; i < Instance.BehaviourQueues.Count; i++)
            {
                Instance.BehaviourQueues[i].WipeQueue();
            }
        }
    }
#endif

    public static void PerformManagingRoutineOnLoops()
    {
#if UNITY_EDITOR
        Profiler.BeginSample("Core Update Manager Routine");
#endif

        for (int i = 0; i < Instance.BehaviourQueues.Count; i++)
        {
            Instance.BehaviourQueues[i].PerformManagingRoutine();
        }

#if UNITY_EDITOR
        Profiler.EndSample();
#endif
    }
}
