# How to use?

To make your scripts work with the update manager, you'll need to inherit from CoreMonoBeh instead of MonoBehaviour.  

```C#
public class MyNiceBehaviour : CoreMonoBeh
{

}
```

This change binds you to a few requirements. most notably not using some of the marshalled calls. Update, Awake, Start, OnEnable, OnDisable and OnDestroy are affected. You'll need to use override methods that start with Core (CoreUpdate, CoreAwake, CoreOnDestroy, etc.).

```C#
public class MyNiceBehaviour : CoreMonoBeh
{
    public override void OnAwake()
    {
        //This is my new Awake.
    }
}
```

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

# How to extend?
CoreUpdateManager creates and manages queues. The base class is UpdateQueue, which stores and manages CoreMonoBehs. It's not enough by itself, because each queue needs to know what settings to pull and use. This is handled by the QueueOverrides, which serve to just retrieve and write the ```LoopUpdateSettings``` structs. See the source to understand it, it's not complex at all and is easily extendable.

So, to recap, in order to add a new loop type, you will need:  
-Create the dedicated UM_SETTINGS value in CoreMonoBeh to store the config (unless you wanna use the other loop's config)  
-Create an override for BehaviourQueue that gets and sets the configs for behaviours  
-Plug into the CoreUpdateManager and call necessary methods  

# Performance considerations
CoreUpdateManager is fairly fast: it generates no garbage during normal use, works with arraysm and sorts them only when necessary. However, there are 2 things that you need to be aware of.

First is that the size of the array is limited. By default arrays are initialized with a value of 512, so when the queue overflows a new array is generated with double the size, which will lead to the first array to be garbage collected, which produces a spike.

Second is that the update manager uses try/catch blocks in order to catch exceptions and function like marshalled methods. While the try block exhibits very little performance overhead, you can disable it by commenting out ```#define UPDATEMANAGER_USETRYCATCH``` on top of the script. This, however, will lead to a lot of problems if your code will encounter unmanaged exceptions, which most often leads to a softlock of the game (not technically one, but you get the point).
