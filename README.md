# Core Update Manager

IMPORTANT NOTE: This branch is currently not under active development and is not considered to be suitable for production as-is. A greatly refactored version is currently being developed and battle-tested with a newer iteration of Grand Dad Mania: Revived and Know By Heart projects.

Core Update Manager is an optimized, extensible, garbage-free Update Manager for Unity.

First used for Grand Dad Mania Revived (https://pchk.itch.io/grand-dad-mania). If you're asking "why" then read this: https://blogs.unity3d.com/2015/12/23/1k-update-calls/

TL;DR: having magic methods called from Unity has a noticeable overhead with big amount of MonoBehaviours. If you wish to gain performance in this aspect or get more freedom/features at the expense of being a little more careful and using a little bit more memory, you can write your own Update Manager which will be the only thing that receives the magic Update method and distributes it across all the other objects. In Grand Dad Mania Revived we managed to save 0.5ms on average on Xiaomi Mi5S Plus.

While Unity is doubling-down on their Entity Component System paradigm, the usual MonoBehaviour approach is not going away, so this project can very well be useful to you. Managed updates will be faster than Unity's marshalled calls in almost any project.

# How to install
Simply copy-paste ```https://github.com/Kolyasisan/Core-Update-Manager.git``` into Unity's Package Manager git field or create a local package folder manually. You can also import it directly into the project.

# Quickstart
```C#

//Inherit from CoreMonoBeh
public class MyMonobeh : CoreMonoBeh
{
    //Create loop settings for this MonoBehaviour's CoreUpdate method
    public override void CoreInitSetup()
    {
        UpdateLoopSettings_CoreUpdate = UpdateLoopSettings.Create(this);
    }
    
    //These ones replace MonoBehaviour methods
    public override void CoreAwake() { }   
    public override void CoreStart() { }
    public override void CoreOnEnable() { }
    public override void CoreOnDisable() { }
    public override void CoreOnDestroy() { }
    
    //This method will be called according to the settings created in CoreInitSetup()
    public override void CoreUpdate() { }
    
    //Will not be called because the settings for these methods have not been created in CoreInitSetup()
    public override void CoreFixedUpdate() { }
    public override void CoreLateUpdate() { }
}

```
  
Read ```the wiki``` for instructions.

# License

This project is distributed under the ```Beerware License```  
![Beer](https://habrastorage.org/getpro/geektimes/post_images/78f/720/c75/78f720c75de7b8828353bc0cf8a254c4.png)
