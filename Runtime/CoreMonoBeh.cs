//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

using UnityEngine;

public abstract class CoreMonoBeh : MonoBehaviour, ICoreUpdatable, ICoreLateUpdatable, ICoreFixedUpdatable
{
    #region LoopConfigs

    public UpdateLoopSettings UpdateLoopSettings_CoreUpdate { get; set; }
    public UpdateLoopSettings UpdateLoopSettings_CoreFixedUpdate { get; set; }
    public UpdateLoopSettings UpdateLoopSettings_CoreLateUpdate { get; set; }

    #endregion

    #region LoopFunctions

    public virtual void CoreUpdate() { }
    public virtual void CoreFixedUpdate() { }
    public virtual void CoreLateUpdate() { }

    #endregion

    #region BehaviourFunctions

    /// <summary>
    /// Called right after OnAwakeMethod() in order to initialize your UM_SETTINGS for the update manager
    /// </summary>
    public virtual void CoreInitSetup() { }

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
    /// Works the same as OnEnable().
    /// </summary>
    public virtual void CoreOnEnable() { }

    /// <summary>
    /// Works the same as OnDisable().
    /// </summary>
    public virtual void CoreOnDisable() { }

    #endregion

    #region CachedComponentsAndMisc

    public Transform _transform { get; private set; }

    #endregion

    #region InternalFunctions

    protected void Awake()
    {
        _transform = GetComponent<Transform>();

#if UNITY_EDITOR
        if (Application.isPlaying)
            RegisterRoutine();
#else
        RegisterRoutine();
#endif

        CoreAwake();
    }

    protected void Start()
    {
        CoreStart();
    }

    protected void OnDestroy()
    {
        UnregisterRoutine();
        CoreOnDestroy();
    }

    protected void OnEnable()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
#endif
            OnEnableManagementRoutine();
#if UNITY_EDITOR
        }
        else
        {
            RegisterRoutine();
        }
#endif

        CoreOnEnable();
    }

    protected void OnDisable()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
#endif
            OnDisableManagementRoutine();
#if UNITY_EDITOR
        }
        else
        {
            UnregisterRoutine();
        }
#endif


        CoreOnDisable();
    }

    protected virtual void RegisterRoutine()
    {
        CoreInitSetup();

        //For editor lazy initialization.
#if UNITY_EDITOR
        CoreUpdateLoop.TryInitialize();
        CoreFixedUpdateLoop.TryInitialize();
        CoreLateUpdateLoop.TryInitialize();
#endif

        CoreUpdateLoop.Instance.EnqueueBehaviour(this);
        CoreLateUpdateLoop.Instance.EnqueueBehaviour(this);
        CoreFixedUpdateLoop.Instance.EnqueueBehaviour(this);
    }

    protected virtual void UnregisterRoutine()
    {
        UpdateLoopSettings settings;

        settings = UpdateLoopSettings_CoreUpdate;
        settings.ShouldBeRegistered = false;
        UpdateLoopSettings_CoreUpdate = settings;

        settings = UpdateLoopSettings_CoreLateUpdate;
        settings.ShouldBeRegistered = false;
        UpdateLoopSettings_CoreLateUpdate = settings;

        settings = UpdateLoopSettings_CoreFixedUpdate;
        settings.ShouldBeRegistered = false;
        UpdateLoopSettings_CoreFixedUpdate = settings;

        //For editor lazy initialization.
#if UNITY_EDITOR
        CoreUpdateLoop.TryInitialize();
        CoreFixedUpdateLoop.TryInitialize();
        CoreLateUpdateLoop.TryInitialize();
#endif

        CoreUpdateLoop.Instance.NeedsRemovals |= UpdateLoopSettings_CoreUpdate.IsValid;
        CoreLateUpdateLoop.Instance.NeedsRemovals |=  UpdateLoopSettings_CoreLateUpdate.IsValid;
        CoreFixedUpdateLoop.Instance.NeedsRemovals |= UpdateLoopSettings_CoreFixedUpdate.IsValid;
    }

    protected virtual void OnEnableManagementRoutine()
    {
        UpdateLoopSettings settings;

        settings = UpdateLoopSettings_CoreUpdate;
        settings.EligibleForUpdate = true;
        UpdateLoopSettings_CoreUpdate = settings;

        settings = UpdateLoopSettings_CoreLateUpdate;
        settings.EligibleForUpdate = true;
        UpdateLoopSettings_CoreLateUpdate = settings;

        settings = UpdateLoopSettings_CoreFixedUpdate;
        settings.EligibleForUpdate = true;
        UpdateLoopSettings_CoreFixedUpdate = settings;
    }

    protected virtual void OnDisableManagementRoutine()
    {
        UpdateLoopSettings settings;

        settings = UpdateLoopSettings_CoreUpdate;
        settings.EligibleForUpdate = false;
        UpdateLoopSettings_CoreUpdate = settings;

        settings = UpdateLoopSettings_CoreLateUpdate;
        settings.EligibleForUpdate = false;
        UpdateLoopSettings_CoreLateUpdate = settings;

        settings = UpdateLoopSettings_CoreFixedUpdate;
        settings.EligibleForUpdate = false;
        UpdateLoopSettings_CoreFixedUpdate = settings;
    }

#endregion
}