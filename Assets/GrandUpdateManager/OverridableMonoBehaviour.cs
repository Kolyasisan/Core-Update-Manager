using UnityEngine;

public class OverridableMonoBehaviour : MonoBehaviour
{
	[Tooltip("Controls whether the behaviour should get update calls. By default OnEnable and OnDisable are used to control it")]
    public bool internal_Enabled;
    [Tooltip("Will disable OnEnable and OnDisable so functions would still execute if the behaviour is disabled")]
    public bool internal_DisableEnablingEvents;
	[Tooltip("Execution order of a script. See UpdateManager script for details")]
    public int internal_ExecutionOrder;

	protected virtual void Awake()
	{
        internal_Enabled = isActiveAndEnabled;
	}

	protected virtual void Start()
	{
		TrySubscribing();
	}
	
	public virtual void TrySubscribing()
	{
		UpdateManager.SubscribeItem(this);
	}
	
	protected virtual void OnDestroy()
	{
		UpdateManager.UnsubscribeItem(this);
	}

	protected virtual void OnEnable()
    {
        if (!internal_Enabled && !internal_DisableEnablingEvents)
        internal_Enabled = true;
    }

    protected virtual void OnDisable()
    {
        if (internal_Enabled && !internal_DisableEnablingEvents)
        internal_Enabled = false;
    }



    /// <summary>
	/// Usual Update loop.
	/// internal_ScriptExecutionOrder is used for controlling the execution order.
    /// </summary>
    public virtual void UpdateMe() {

    }

	/// <summary>
	/// Usual FixedUpdate loop
	/// </summary>
	public virtual void FixedUpdateMe() {}

	/// <summary>
	/// Usual LateUpdate loop
	/// </summary>
	public virtual void LateUpdateMe() {}

    public void DisableScriptInternally()
    {
        internal_Enabled = false;
    }

    public void EnableScriptInternally()
    {
        internal_Enabled = true;
    }

    public void SetScriptActive(bool val)
    {
        internal_Enabled = val;
    }

	/// <summary>
	/// If set to false, then OnEnable and OnDisable will not do anything and functions will be called and performed even when disabled.
	/// </summary>
	public void SetDisableEventAllowance(bool val)
	{
		internal_DisableEnablingEvents = val;
	}

}
