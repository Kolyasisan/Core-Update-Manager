# Note
This is the first version of Core Update Manager. It is highly recommended for you to check out the new rewritten version at master.

# Core Update Manager

An optimized, garbage-free Update Manager for Unity with custom execution order that works with Arrays.  
First used for Grand Dad Mania Revived (https://pchk.itch.io/grand-dad-mania). If you're asking "why" then read this: https://blogs.unity3d.com/2015/12/23/1k-update-calls/

Long story short: having magic methods called from Unity has an overhead and thus, if you wish to gain performance or get more freedom/features at the expense of being a little more careful and using a little bit more memory, you can write your own Update Manager which will be the only thing that receives the magic Update method and distributes it across all the other objects. In Grand Dad Mania Revived we managed to save 0.5ms on average on Xiaomi Mi5S Plus.

While Unity is doubling-down on their Entity Component System paradigm, the usual MonoBehaviour approach is not going away, so this project will most definitely be useful to you. Managed updates will be faster than Unity's marshalled calls in pretty much any project.

# Quickstart
```C#

//Inherit from CoreMonoBeh
public class MyMonobeh : CoreMonoBeh
{
    //Create loop settings for this MonoBehaviour's CoreUpdate method
    public override void CoreInitSetup()
    {
        UM_SETTINGS_UPDATE = new LoopUpdateSettings(default);
    }
    
    //Use for initialization
    public override void CoreAwake() { }   
    public override void CoreStart() { }
    
    //These ones replace MonoBehaviour methods
    public override void CoreOnEnable() { }
    public override void CoreOnDisable() { }
    public override void CoreOnDestroy() { }
    
    //This method will be called according to the settings created in CoreInitSetup()
    public override void CoreUpdate() { }
    
    //Will not be called because the settings for these methods were not created in CoreInitSetup()
    public override void CoreFixedUpdate() { }
    public override void CoreLateUpdate() { }
}

```
  
Read ```the wiki``` for instructions.

# License

This project is distributed under the ```Beerware License```  
![Beer](https://habrastorage.org/getpro/geektimes/post_images/78f/720/c75/78f720c75de7b8828353bc0cf8a254c4.png)
