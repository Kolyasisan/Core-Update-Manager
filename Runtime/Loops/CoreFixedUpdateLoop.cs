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
public class CoreFixedUpdateLoop : BehaviourLoopInstance<ICoreFixedUpdatable>
{
    public static CoreFixedUpdateLoop Instance { get; private set; }

    public static bool IsInited { get; set; }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void TryInitialize()
    {
        if (IsInited)
            return;

        IsInited = true;
        Instance = new CoreFixedUpdateLoop();
        Instance.RegisterLoopToUpdateManager();

        PollingLoops.CoreFixedUpdatePoller.Hook();
    }

    public override void Reinitialize()
    {
        if (IsInited)
        {
            IsInited = false;
            TryInitialize();
        }
    }

    public override UpdateLoopSettings GetSettings(ICoreFixedUpdatable beh)
    {
        return beh.UpdateLoopSettings_CoreFixedUpdate;
    }

    public override void WriteSettings(UpdateLoopSettings config, ICoreFixedUpdatable beh)
    {
        beh.UpdateLoopSettings_CoreFixedUpdate = config;
    }

    public override void UpdateLoop()
    {
        CoreUpdateManager.PerformManagingRoutineOnLoops();

        if (HasEntries)
        {
            int cnt = UpperBound;
            for (int i = LowerBound + 1; i < cnt; i++)
            {
                if (Queue[i].UpdateLoopSettings_CoreFixedUpdate.EligibleForUpdate)
                {
#if UPDATEMANAGER_USETRYCATCH
                    try
                    {
                        Queue[i].CoreFixedUpdate();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
#else
                    Queue[i].CoreFixedUpdate();
#endif
                }
            }
        }

        CoreUpdateManager.PerformManagingRoutineOnLoops();
    }
}
