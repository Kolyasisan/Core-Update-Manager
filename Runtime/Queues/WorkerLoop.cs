//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

//The update manager executes calls inside Try-Catch blocks in order to deal with exceptions.
//You can comment-out this line to gain minor performance, but any exception will halt the loop entirely, which can lead to a softlock.

using System;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class WorkerLoop : BehaviourLoopInstance
{
    public sealed override LoopUpdateSettings GetLoopSettings(CoreMonoBeh beh)
    {
        throw new Exception("A WorkerLoop abstract method has been called! You should not inherit from it!");
    }

    public sealed override void Perform()
    {
        throw new Exception("A WorkerLoop abstract method has been called! You should not inherit from it!");
    }

    public sealed override void WriteLoopSettings(CoreMonoBeh beh, LoopUpdateSettings set)
    {
        throw new Exception("A WorkerLoop abstract method has been called! You should not inherit from it!");
    }
}
