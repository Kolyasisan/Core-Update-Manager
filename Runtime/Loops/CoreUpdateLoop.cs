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
public class CoreUpdateLoop : BehaviourLoopInstance<ICoreUpdatable>
{
    public static CoreUpdateLoop Instance { get; private set; }

    public static bool IsInited { get; set; }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void TryInitialize()
    {
        if (IsInited)
            return;

        IsInited = true;
        Instance = new CoreUpdateLoop();
        Instance.RegisterLoopToUpdateManager();

        PollingLoops.CoreUpdatePoller.Hook();
    }

    public override void Reinitialize()
    {
        if (IsInited)
        {
            IsInited = false;
            TryInitialize();
        }
    }

    public override UpdateLoopSettings GetSettings(ICoreUpdatable beh)
    {
        return beh.UpdateLoopSettings_CoreUpdate;
    }

    public override void WriteSettings(UpdateLoopSettings config, ICoreUpdatable beh)
    {
        beh.UpdateLoopSettings_CoreUpdate = config;
    }

    public override void UpdateLoop()
    {
        CoreUpdateManager.PerformManagingRoutineOnLoops();

        if (HasEntries)
        {
            int cnt = UpperBound;
            for (int i = LowerBound + 1; i < cnt; i++)
            {
                if (Queue[i].UpdateLoopSettings_CoreUpdate.EligibleForUpdate)
                {
#if UPDATEMANAGER_USETRYCATCH
                    try
                    {
                        Queue[i].CoreUpdate();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
#else
                    Queue[i].CoreUpdate();
#endif
                }
            }
        }

        CoreUpdateManager.PerformManagingRoutineOnLoops();
    }
}
