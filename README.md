# Grand Update Manager

An optimized, generally garbage-free Update Manager with custom execution order that works with Arrays.  
First used for Grand Dad Mania Revived (https://pechka-productions.itch.io/grand-dad-mania). If you're asking "why" then read this: https://blogs.unity3d.com/2015/12/23/1k-update-calls/

Long story short: having "magic" methods called from Unity has an overhead and thus, if you wish to gain performance or get more freedom/features at the expense of being a little more careful and using a little bit more memory, you can write your own Update Manager which will be the only thing that receives the magic Update method and distributes it across all the other objects. In Grand Dad Mania Revived we managed to save 0.5ms on average on Xiaomi Mi5S Plus.

# How To Use
-Derive your MonoBehaviour from OverridableMonoBehaviour.  
-Add Interfaces to notify what your script is using (IUpdatable, ILateUpdatable, IFixedUpdatable).  
-Use public override void UpdateMe and its variants to get Update ticks.  
-If you use Start, OnEnable and OnDisable methods then change them to Protected Override Void Start and call base.FunctionName() in them.  
# Some Notes
-The manager recognizes what functions you want to use based on Interfaces. We're using them exclusively for that to avoid garbage generation/additional RAM usage compared to other methods and save performance by not using reflection.  
-Garbage is generetad only upon Arrays overflow when trying to add more scripts than the array can hold.  
-Arrays are used and managed manually instead of lists for maximum performance.  
-The scripts subscribe and unsubscribe on Start and OnDestroy respectively to save on performance. Whether or not they receive update ticks is determined by internal_Enabled which you can also control manually by disabling its automatic control (yes, you can set it up to get ticks even if it's disabled).  
-internal_ExecutionOrder controls the order of when your scripts will be updated. All the scripts are sorted really fast before every update after the changes were made by subscribing. That means dynamic and effortless execution order.
-See detailed instructions in the UpdateManager script.  

Warning: may potentially enforce you to facepalm yourself upon seeing the code!

Please read the License.
