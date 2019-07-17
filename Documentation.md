# How to use?

To make your scripts work with the update manager, you'll need to inherit from CoreMonoBeh instead of MonoBehaviour.  

```C#
public class MyNiceBehaviour : CoreMonoBeh
{

}
```

This change binds you to a few requirements. most notably not using some of the omarshalled calls. Update, Awake, Start, OnEnable, OnDisable and OnDestroy are affected. You'll need to use override methods that start with Core (CoreUpdate, CoreAwake, CoreOnDestroy, etc.).  

Update Manager consist of several loops, to which your CoreMonoBehs can subscribe. Internally, CoreMonoBeh contains a struct for each loop type, which defines options for them. That struct is called ```LoopUpdateSettings``` and by default it is initialized in a way that will not make your CoreMonoBeh subscribe to the Update Manager. To have your CoreMonoBeh subscribe to one or more update loops, you need to generate the settings.  

As an example, there are only 2 loops available here, CoreUpdate and CoreGameplayUpdate. Each CoreMonoBeh has a configuration struct defined for them, UM_SETTINGS_UPDATE and UM_SETTINGS_GAMEPLAYUPDATE respectively.

There are several options in the struct:
-isInited: generates automatically when you manually initialize the struct, e.g. when you want this CoreMonoBeh to be considered by the update manager regarding a particular loop.
-eligibleForUpdate: if it's set to false, then this CoreMonoBeh will not receive update calls for a loop (provided that it subscribed in the first place).
-updateOrder: defines the order where your script will be placed. The default value is 128.
-AutoManageDisableAndEnableEvents: if it's checked, the the CoreMonoBeh will automatically start and stop receiving update calls when it's enabled or disabled, just like the usual MonoBehaviour. You can set it to false and manage this value yourself, which has quite a bit of use cases.

So, by doing something like this:
```C#
public class MyNiceBehaviour : CoreMonoBeh {

    public override void CoreInitSetup()
    {
        UM_SETTINGS_UPDATE = new LoopUpdateSettings(default); //Replace defaults with your own values for something special
    }
    
    public override void CoreUpdate()
    {
        //Yay, code!
    }
}
```
We make ```MyNiceBehaviour``` receive custom update calls. Cool huh?
