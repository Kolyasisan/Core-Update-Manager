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
using UnityEngine;

[ExecuteAlways]
public class CoreLateUpdateLoop : BehaviourLoopInstance<ICoreLateUpdatable>
{
    public static CoreLateUpdateLoop Instance { get; private set; }

    public static bool IsInited { get; set; }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void TryInitialize()
    {
        if (IsInited)
            return;

        IsInited = true;
        Instance = new CoreLateUpdateLoop();
        Instance.RegisterLoopToUpdateManager();

        PollingLoops.CoreLateUpdatePoller.Hook();
    }

    public override UpdateLoopSettings GetSettings(ICoreLateUpdatable beh)
    {
        return beh.UpdateLoopSettings_CoreLateUpdate;
    }

    public override void WriteSettings(UpdateLoopSettings config, ICoreLateUpdatable beh)
    {
        beh.UpdateLoopSettings_CoreLateUpdate = config;
    }

    public override void UpdateLoop()
    {
        CoreUpdateManager.PerformManagingRoutineOnLoops();

        if (HasEntries)
        {
            int cnt = UpperBound;
            for (int i = LowerBound + 1; i < cnt; i++)
            {
                if (Queue[i].UpdateLoopSettings_CoreLateUpdate.EligibleForUpdate)
                {
#if UPDATEMANAGER_USETRYCATCH
                    try
                    {
                        Queue[i].CoreLateUpdate();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
#else
                    Queue[i].CoreLateUpdate();
#endif
                }
            }
        }

        CoreUpdateManager.PerformManagingRoutineOnLoops();
    }
}
