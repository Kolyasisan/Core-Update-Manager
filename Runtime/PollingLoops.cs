//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

public class PollingLoops
{
    public struct CoreUpdatePoller
    {
        static bool alreadyHooked = false;

        public static void Hook()
        {
            if (alreadyHooked)
                return;

            alreadyHooked = true;

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var update = playerLoop.subSystemList[4];
            var updateAsList = new List<PlayerLoopSystem>(update.subSystemList);

            int updateLoopIndex = -1;

            for (int i = 0; i < updateAsList.Count; i++)
            {
                if (updateAsList[i].type == typeof(Update.ScriptRunBehaviourUpdate))
                {
                    updateLoopIndex = i;
                    break;
                }
            }

            updateAsList.Insert(updateLoopIndex + 1, new PlayerLoopSystem()
            {
                type = typeof(CoreUpdatePoller),
                updateDelegate = UpdateFunction
            });

            update.subSystemList = updateAsList.ToArray();
            playerLoop.subSystemList[4] = update;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        public static void UpdateFunction()
        {
            CoreUpdateLoop.Instance.UpdateLoop();
        }
    }

    public struct CoreLateUpdatePoller
    {
        static bool alreadyHooked = false;

        public static void Hook()
        {
            if (alreadyHooked)
                return;

            alreadyHooked = true;

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var update = playerLoop.subSystemList[5];
            var updateAsList = new List<PlayerLoopSystem>(update.subSystemList);

            int updateLoopIndex = -1;

            for (int i = 0; i < updateAsList.Count; i++)
            {
                if (updateAsList[i].type == typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate))
                {
                    updateLoopIndex = i;
                    break;
                }
            }

            updateAsList.Insert(updateLoopIndex + 1, new PlayerLoopSystem()
            {
                type = typeof(CoreLateUpdatePoller),
                updateDelegate = UpdateFunction
            });

            update.subSystemList = updateAsList.ToArray();
            playerLoop.subSystemList[5] = update;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        public static void UpdateFunction()
        {
            CoreLateUpdateLoop.Instance.UpdateLoop();
        }
    }


    public struct CoreFixedUpdatePoller
    {
        static bool alreadyHooked = false;

        public static void Hook()
        {
            if (alreadyHooked)
                return;

            alreadyHooked = true;

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var update = playerLoop.subSystemList[2];
            var updateAsList = new List<PlayerLoopSystem>(update.subSystemList);

            int updateLoopIndex = -1;

            for (int i = 0; i < updateAsList.Count; i++)
            {
                if (updateAsList[i].type == typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate))
                {
                    updateLoopIndex = i;
                    break;
                }
            }

            updateAsList.Insert(updateLoopIndex, new PlayerLoopSystem()
            {
                type = typeof(CoreFixedUpdatePoller),
                updateDelegate = UpdateFunction
            });

            update.subSystemList = updateAsList.ToArray();
            playerLoop.subSystemList[2] = update;
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        public static void UpdateFunction()
        {
            CoreFixedUpdateLoop.Instance.UpdateLoop();
        }
    }
}
