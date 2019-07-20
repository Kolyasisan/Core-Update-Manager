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
public class CoreMonoBeh : MonoBehaviour
{
    #region loopsettings
    [System.NonSerialized] public LoopUpdateSettings UM_SETTINGS_UPDATE;
    [System.NonSerialized] public LoopUpdateSettings UM_SETTINGS_GAMEPLAYUPDATE;
    [System.NonSerialized] public LoopUpdateSettings UM_SETTINGS_FIXEDUPDATE;
    #endregion

    #region cachedcomponents
    public Transform _transform { get; private set; }
    #endregion

    public virtual void CoreUpdate() { }
    public virtual void CoreGameplayUpdate() { }
    public virtual void CoreFixedUpdate() { }

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
        if (UM_SETTINGS_UPDATE.isInited && UM_SETTINGS_UPDATE.AutoManageEnableDisableEvents)
            UM_SETTINGS_UPDATE.eligibleForUpdate = true;

        if (UM_SETTINGS_GAMEPLAYUPDATE.isInited && UM_SETTINGS_GAMEPLAYUPDATE.AutoManageEnableDisableEvents)
            UM_SETTINGS_GAMEPLAYUPDATE.eligibleForUpdate = true;

        if (UM_SETTINGS_FIXEDUPDATE.isInited && UM_SETTINGS_FIXEDUPDATE.AutoManageEnableDisableEvents)
            UM_SETTINGS_FIXEDUPDATE.eligibleForUpdate = true;

        CoreOnEnable();
    }

    protected void OnDisable()
    {      
        if (UM_SETTINGS_UPDATE.isInited && UM_SETTINGS_UPDATE.AutoManageEnableDisableEvents)
            UM_SETTINGS_UPDATE.eligibleForUpdate = false;

        if (UM_SETTINGS_GAMEPLAYUPDATE.isInited && UM_SETTINGS_GAMEPLAYUPDATE.AutoManageEnableDisableEvents)
            UM_SETTINGS_GAMEPLAYUPDATE.eligibleForUpdate = false;

        if (UM_SETTINGS_FIXEDUPDATE.isInited && UM_SETTINGS_FIXEDUPDATE.AutoManageEnableDisableEvents)
            UM_SETTINGS_FIXEDUPDATE.eligibleForUpdate = false;

        CoreOnDisable();
    }

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
}
