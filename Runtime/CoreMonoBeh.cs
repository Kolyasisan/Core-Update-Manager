//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public partial class CoreMonoBeh : MonoBehaviour
{
    //Add your own settings defines here or use a partial class.
    #region loopsettings

    [System.NonSerialized] public LoopUpdateSettings UM_SETTINGS_UPDATE;
    [System.NonSerialized] public LoopUpdateSettings UM_SETTINGS_GAMEPLAYUPDATE;
    [System.NonSerialized] public LoopUpdateSettings UM_SETTINGS_FIXEDUPDATE;

    #endregion

    //Add your own function defines here or use a partial class.
    #region loopfunctions

    public virtual void CoreUpdate() { }
    public virtual void CoreGameplayUpdate() { }
    public virtual void CoreFixedUpdate() { }

    #endregion

    #region behaviourfunctions

    /// <summary>
    /// Works the same as Awake(). Use this instead of the Awake method.
    /// </summary>
    public virtual void CoreAwake() { }

    /// <summary>
    /// Works the same as Start(). Use this instead of the Start method.
    /// </summary>
    public virtual void CoreStart() { }

    /// <summary>
    /// Works the same as OnDestroy(). Use this instead of the OnDestroy method.
    /// </summary>
    public virtual void CoreOnDestroy() { }

    /// <summary>
    /// Called right after OnAwakeMethod() in order to initialize your UM_SETTINGS for the update manager
    /// </summary>
    public virtual void CoreInitSetup() { }

    /// <summary>
    /// Works the same as OnEnable().
    /// </summary>
    public virtual void CoreOnEnable() { }

    /// <summary>
    /// Works the same as OnDisable().
    /// </summary>
    public virtual void CoreOnDisable() { }

    #endregion

    #region cachedcomponents

    public Transform _transform { get; private set; }

    #endregion

    #region internalfunctions

    protected void Awake()
    {
        _transform = (Transform)GetComponent(typeof(Transform));

        CoreInitSetup();
        CoreUpdateManager.ScheduleBehaviourRegister(this);

        CoreAwake();
    }

    protected void Start()
    {
        CoreStart();
    }

    protected void OnDestroy()
    {
        CoreOnDestroy();
        CoreUpdateManager.ScheduleBehaviourRemoval(this);
    }

    protected void OnEnable()
    {
        UM_SETTINGS_UPDATE.PerformEnableDisableRoutine(true);
        UM_SETTINGS_GAMEPLAYUPDATE.PerformEnableDisableRoutine(true);
        UM_SETTINGS_FIXEDUPDATE.PerformEnableDisableRoutine(true);

        CoreOnEnable();
    }

    protected void OnDisable()
    {
        UM_SETTINGS_UPDATE.PerformEnableDisableRoutine(false);
        UM_SETTINGS_GAMEPLAYUPDATE.PerformEnableDisableRoutine(false);
        UM_SETTINGS_FIXEDUPDATE.PerformEnableDisableRoutine(false);

        CoreOnDisable();
    }

    #endregion
}