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
public class GameplayUpdateQueue : BehaviourQueueBase
{
    public override LoopUpdateSettings GetLoopSettings(CoreMonoBeh beh)
    {
        return beh.UM_SETTINGS_GAMEPLAYUPDATE;
    }

    public override void WriteLoopSettings(CoreMonoBeh beh, LoopUpdateSettings set)
    {
        beh.UM_SETTINGS_GAMEPLAYUPDATE = set;
    }

    new public void Perform()
    {
        if (HasEntries)
        {
            int cnt = UpperBound;
            for (int i = LowerBound + 1; i < cnt; i++)
            {
                if (queue[i].UM_SETTINGS_GAMEPLAYUPDATE.eligibleForUpdate)
                {
#if UPDATEMANAGER_USETRYCATCH
                    try
                    {
                        queue[i].CoreGameplayUpdate();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
#else
                        queue[i].CoreGameplayUpdate();
#endif

                }
            }
        }
    }
}
