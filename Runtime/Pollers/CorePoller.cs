//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

using UnityEngine;

/// <summary>
/// This MonoBehaviour is used to receive proper functions and then distribute them to the Perform() methods on the UpdateQueues.
/// </summary>
public class CorePoller : MonoBehaviour
{
    public static CorePoller instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        instance = (CorePoller)CoreUpdateManager.CreateComponentAsGO<CorePoller>("CorePoller");
    }

    private void Update()
    {
        UpdateLoop.instance.Perform();
        GameplayUpdateLoop.instance.Perform();
    }

    private void LateUpdate()
    {
        CoreUpdateManager.PerformUpdateManagerRoutine();   
    }

    private void FixedUpdate()
    {
        FixedUpdateLoop.instance.Perform();
    }
}