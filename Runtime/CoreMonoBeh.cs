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
public abstract partial class CoreMonoBeh : MonoBehaviour
{
    //Add your own settings defines here or use a partial class.
    #region loop settings

    public LoopUpdateSettings UM_SETTINGS_UPDATE { get; set; }
    public LoopUpdateSettings UM_SETTINGS_GAMEPLAYUPDATE { get; set; }
    public LoopUpdateSettings UM_SETTINGS_FIXEDUPDATE { get; set; }

    #endregion

    //Add your own function defines here or use a partial class.
    #region loop functions

    public virtual void CoreUpdate() { }
    public virtual void CoreGameplayUpdate() { }
    public virtual void CoreFixedUpdate() { }

    #endregion

    //Insert routine functions for your custom settings defines here.
    #region OnEnable OnDisable callbacks

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





    //Internal stuff beyond this point.
    #region misc variables

    /// <summary>
    /// Returns true if this CoreMonoBeh is about to be destroyed.
    /// </summary>
    public bool isMarkedForDeletion { get; private set; }

    #endregion

    #region cached components

    public Transform _transform { get; private set; }

    #endregion

    #region behaviour functions

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

    #region internal functions

    protected void Awake()
    {
        _transform = (Transform)GetComponent(typeof(Transform));

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            CoreInitSetup();
            CoreUpdateManager.ScheduleBehaviourRegister(this);
        }
#else
        CoreInitSetup();
        CoreUpdateManager.ScheduleBehaviourRegister(this);
#endif

        CoreAwake();
    }

    protected void Start()
    {
        CoreStart();
    }

    protected void OnDestroy()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            isMarkedForDeletion = true;
            CoreOnDestroy();
            CoreUpdateManager.ScheduleBehaviourRemoval(this);
        }
        else
        {
            isMarkedForDeletion = true;
            CoreOnDestroy();
        }
#else
            isMarkedForDeletion = true;
            CoreOnDestroy();
            CoreUpdateManager.ScheduleBehaviourRemoval(this);
#endif

    }

    #endregion

}