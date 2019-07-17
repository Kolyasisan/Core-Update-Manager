# Core Update Manager

An optimized, garbage-free Update Manager for Unity with custom execution order that works with Arrays.  
First used for Grand Dad Mania Revived (https://pechka-productions.itch.io/grand-dad-mania). If you're asking "why" then read this: https://blogs.unity3d.com/2015/12/23/1k-update-calls/

Long story short: having "magic" methods called from Unity has an overhead and thus, if you wish to gain performance or get more freedom/features at the expense of being a little more careful and using a little bit more memory, you can write your own Update Manager which will be the only thing that receives the magic Update method and distributes it across all the other objects. In Grand Dad Mania Revived we managed to save 0.5ms on average on Xiaomi Mi5S Plus.

Read ```Documentation.md``` on how to use it.

Please read the ```License```.
