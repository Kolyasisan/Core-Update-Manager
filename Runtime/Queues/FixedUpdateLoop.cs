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
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[System.Serializable]
[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public class FixedUpdateLoop : BehaviourLoopInstance
{
    public static FixedUpdateLoop instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        instance = (FixedUpdateLoop)CoreUpdateManager.RegisterBehaviourQueue<FixedUpdateLoop>("FixedUpdateQueue");
    }

    void OnDestroy()
    {
        instance = null;
    }

    public override LoopUpdateSettings GetLoopSettings(CoreMonoBeh beh)
    {
        return beh.UM_SETTINGS_FIXEDUPDATE;
    }

    public override void WriteLoopSettings(CoreMonoBeh beh, LoopUpdateSettings set)
    {
        beh.UM_SETTINGS_FIXEDUPDATE = set;
    }

    public override void Perform()
    {
        CoreUpdateManager.PerformUpdateManagerRoutine();

        if (HasEntries)
        {
            int cnt = UpperBound;
            for (int i = LowerBound + 1; i < cnt; i++)
            {
                if (queue[i].UM_SETTINGS_FIXEDUPDATE.eligibleForUpdate)
                {
#if UPDATEMANAGER_USETRYCATCH
                    try
                    {
                        queue[i].CoreFixedUpdate();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
#else
                    queue[i].CoreFixedUpdate();
#endif
                }
            }
        }
    }
}