using System.Collections;
using UnityEngine;

/// <summary>
/// Made by Nikolai "Kolyasisan" Ponomarev originally for Grand Dad Mania Revived (memes yay).
/// Base concept by Valentin Simonov, PlayDead and Feiko Joosten.
/// 
/// This system speeds up scripts by having their update ticks called from managed code rather than Unity's C++ code (thanks Unity).
/// The more update ticks you have the faster it gets compared to vanilla Updates.
/// Your scripts must derive from OverridableMonoBehaviour.
/// Your scripts must also contain one of the interfaces (IUpdatable, IFixedUpdatable, ILateUpdatable). They are used to let the manager know what your script uses (the calls are still performed through class inheritance).
/// Yeah, I know it's dumb to not have interfaces for calls, but this way we can reduce ram usage/garbage generation and also not use reflection.
/// public override void UpdateMe, FixedUpdateMe, LateUpdateMe and BatchedUpdateMe will update your scripts.
/// Awake, Start, OnEnable and OnDisable all need to be marked as protected override and have base.functionname in them in order to work.
///
/// Features include:
/// Works solely with arrays so it's super fast. Arrays are only resized when they are full, so we'll get no garbage at runtime from resizing if we're cafreful enough (at the cost of a bit more RAM usage).
/// Scripts automatically subscribe on Start and unsubscribe onDestroy. Those are not handled in Enable functions for performance reasons.
/// The reason we subscribe at Start is so that the script's update tick wouldn't be accidentally called before Start when the update manager performs its update sweep. Unity internally does the same (never subscribes on Awake)
/// No null checking at usual runtime for performance reasons. The only thing that is being checked during the sweep is internal_enabled (which is in managed space so it's quite fast anyway).
/// internal_enabled fully controls if your scripts should receive tick updates. That means you can still perform stuff when an object is disabled if you wish so. By default it's automatically managed by Enable events.
/// internal_executionOrder controls custom sorting of execution order (scripts are very quickly sorted when the arrays are changed, no countless for loops at usual runtime for obvious reasons. Generates a tiny bit of garbage, but that's going to be fixed).
/// </summary>

public class UpdateManager : MonoBehaviour
{
    #region config
    private int expandAmountOnOverflow = 100; //How much should we expand the arrays if, at one point, there would be an overflow.
    private float maxAllowedUpdateTime = 0.0175f; //Specifically for GDM we made this monstrocity to limit the amount of update calls per second to support high-fps monitors. Yep, we don't use delta time for some reason.

    //Defines the limits of execution order from minus x to x + 1.
    //No script should have their execution order to be outside of this range.
    //Setting this to crazy values would bring significant slowdown during the sorting process.
    private int maxExecutionOrderRange = 20;
    #endregion

    #region Variables

    //A singleton. Burn the heretic!
    private static UpdateManager instance;

    private int regularUpdateArrayCount;
    private int fixedUpdateArrayCount;
    private int lateUpdateArrayCount;

    private OverridableMonoBehaviour[] regularArray = new OverridableMonoBehaviour[0];
    private OverridableMonoBehaviour[] fixedArray = new OverridableMonoBehaviour[0];
    private OverridableMonoBehaviour[] lateArray = new OverridableMonoBehaviour[0];
    private OverridableMonoBehaviour[] workerArray = new OverridableMonoBehaviour[0];

    private bool arraysWereChanged;
    private float curFrameTime;
    
    #endregion

    #region InternalFunctionsAndApis

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else{
            instance = this;
            regularArray = new OverridableMonoBehaviour[750];
            fixedArray = new OverridableMonoBehaviour[150];
            lateArray = new OverridableMonoBehaviour[150];
            workerArray = new OverridableMonoBehaviour[1000];
        }
    }

    //Subscribes a behaviour
    public static void SubscribeItem(OverridableMonoBehaviour behaviour)
    {
        if (instance != null && behaviour != null)
        instance.ScheduleItemAddition(behaviour);
    }

    //Unsubscribes a behaviour
    public static void UnsubscribeItem(OverridableMonoBehaviour behaviour)
    {
        if (behaviour && instance)
            instance.ScheduleItemRemoval(behaviour);
        
    }

    private void ScheduleItemAddition(OverridableMonoBehaviour behaviour)
    {
        if (!CheckIfArrayContainsItem(regularArray, behaviour) && behaviour is IUpdatable)
        {
            ExtendAndAddItemToArray(ref regularArray, behaviour, ref regularUpdateArrayCount);
        }

        if (!CheckIfArrayContainsItem(fixedArray, behaviour) && behaviour is IFixedUpdatable)
        {
            ExtendAndAddItemToArray(ref fixedArray, behaviour, ref fixedUpdateArrayCount);
        }
        
        if (!CheckIfArrayContainsItem(lateArray, behaviour) && behaviour is ILateUpdatable)
        {
            ExtendAndAddItemToArray(ref lateArray, behaviour, ref lateUpdateArrayCount);
        }

        //ArrayExceptionSolver();
    }
    
    private void ScheduleItemRemoval(OverridableMonoBehaviour behaviour)
    {
        if (CheckIfArrayContainsItem(regularArray, behaviour))
        {
            ShrinkAndRemoveItemFromArray(ref regularArray, behaviour, ref regularUpdateArrayCount, ref regularUpdateArrayCount);
        }

        if (CheckIfArrayContainsItem(fixedArray, behaviour))
        {
            ShrinkAndRemoveItemFromArray(ref fixedArray, behaviour, ref fixedUpdateArrayCount, ref fixedUpdateArrayCount);
        }
        
        if (CheckIfArrayContainsItem(lateArray, behaviour))
        {
            ShrinkAndRemoveItemFromArray(ref lateArray, behaviour, ref lateUpdateArrayCount, ref lateUpdateArrayCount);
        }

        //ArrayExceptionSolver();
    }

    private void ExtendAndAddItemToArray(ref OverridableMonoBehaviour[] original, OverridableMonoBehaviour itemToAdd, ref int amount)
    {
        int size = original.Length;

        bool wasFound = false;

        //Fit our behavour in some empty space.
        for (int i = 0; i < size; i++)
        {
            if (original[i] == null)
            {
                original[i] = itemToAdd;
                arraysWereChanged = true;
                amount++;
                wasFound = true;
                break; //Return produces a stackoverflow exception here for some reason, so we're doing a bool check instead.
            }
        }

        //There was no empty space. Overflow!
        if (!wasFound)
        {
            regularArray = ExtendArraySize(regularArray);
            fixedArray = ExtendArraySize(fixedArray);
            lateArray = ExtendArraySize(lateArray);
            
            for (int i = 0; i < size; i++)
            {
                if (original[i] == null)
                {
                    original[i] = itemToAdd;
                    arraysWereChanged = true;
                    amount++;
                    wasFound = true;
                    break; //Return produces a stackoverflow exception here for some reason, so we're doing a bool check instead.
                }
            }
        }
    }

    private void ShrinkAndRemoveItemFromArray(ref OverridableMonoBehaviour[] original, OverridableMonoBehaviour itemToRemove, ref int amount, ref int arraySize)
    {
        //int size = GetTrueArrayLength(ref original);
        int size = arraySize;
        if (size == 0) size = 1;

        int virtI = 0;
        if (original.Length - 1 != 0)
        {
            for (int i = 0; i < size; i++)
            {
                if (original[i] == itemToRemove)
                {
                    virtI++;
                }
                else original[i - virtI] = original[i];
            }
        }

        amount--;
        arraysWereChanged = true;
        
        //WipeArrayFromToNull(ref original, size);
    }

    //Self-explanatory
    private bool CheckIfArrayContainsItem(OverridableMonoBehaviour[] arrayToCheck, OverridableMonoBehaviour objectToCheckFor)
    {
        int size = arrayToCheck.Length;

        for (int i = 0; i < size; i++)
        {
            if (objectToCheckFor == arrayToCheck[i]) return true;
        }

        return false;
    }

    //Sort all the items, starting from the ones that execute at the earliest stage
    private void SortItems(ref OverridableMonoBehaviour[] original)
    {
        int virtI = 0;
        int size = original.Length;

        if (workerArray.Length <= original.Length) ExtendArraySize(workerArray);
        WipeArray(ref workerArray, workerArray.Length);

        for (int iOrder = -maxExecutionOrderRange; iOrder < maxExecutionOrderRange + 1; iOrder++)
        {
            for (int i = 0; i < size; i++)
            {
                    if (original[i] == null) continue;

                    if (original[i].internal_ExecutionOrder == iOrder)
                    {
                        workerArray[virtI] = original[i];
                        virtI++;
                    }
            }
        }

        WipeArray(ref original, original.Length);
        CopyArray(ref workerArray, ref original, original.Length);
        //WipeArray(ref workerArray, workerArray.Length);
    }

    //Copies the array's elements starting from the frist one and ending on the desired location.
    public void CopyArray (ref OverridableMonoBehaviour[] copyFrom, ref OverridableMonoBehaviour[] copyTo, int amount){
        for (int i = 0; i < amount; i++){
            copyTo[i] = copyFrom[i];
        }
    }

    //Wipe the array's element starting from the first one up until the desired amount.
    public void WipeArray (ref OverridableMonoBehaviour[] arrayToWipe, int amount){
        for (int i = 0; i < amount; i++){
            arrayToWipe[i] = null;
        }
    }

    //Wipe the array's elements starting from a certain location and ending until hitting null or hitting the length of an arrays
    //Not used due to performance reasons
    private void WipeArrayFromToNull(ref OverridableMonoBehaviour[] arrayToWipe, int startLoc)
    {
        for (int i = startLoc; i < arrayToWipe.Length; i++)
        {
            if (arrayToWipe[i] != null) arrayToWipe[i] = null;
            else if (arrayToWipe[i] == null) return;
        }
    }

    //This is called to expand the array in case it got overflowed. Generates garbage and should be avoided by setting an adequate startArrayCount.
    private OverridableMonoBehaviour[] ExtendArraySize(OverridableMonoBehaviour[] original){
        int size = original.Length;
        OverridableMonoBehaviour[] finalArray = new OverridableMonoBehaviour[size + expandAmountOnOverflow];

        for (int i = 0; i < size; i++)
        {
            finalArray[i] = original[i];
        }
        
        arraysWereChanged = true;
        return finalArray;
    }

    private void TrySortingArrays()
    {
        if (arraysWereChanged)
        {
            SortItems(ref regularArray);
            SortItems(ref fixedArray);
            SortItems(ref lateArray);
            arraysWereChanged = false;
        }
    }
    
    #endregion

    #region BehaviourSweep
    
    private void Update()
    {
        //Now, Grand Dad Mania was (stupidly enough) built as old console games without Delta Time, just like Sonic Mania (which we're parodying, since GDM is a parody game).
        //In order to allow support for high-fps refresh rate (even if it won't do you good) we execute Update only a fixed amount of time per frame.
        //If you wish to have the same behaviour - uncomment this and comment-out the VirtUpdate() below this idiocy
        /*
        curFrameTime += Time.unscaledDeltaTime;
        if (curFrameTime >= 0.016f)
        {
            VirtUpdate();
            curFrameTime -= 0.016f;

            while (curFrameTime >= 0.016f) curFrameTime -= 0.016f;
        }
        */

        VirtUpdate();
    }

    private void VirtUpdate()
    {
        TrySortingArrays();

        if (regularUpdateArrayCount != 0){
        
            for (int i = 0; i < regularUpdateArrayCount; i++)
            {
                if (regularArray[i].internal_Enabled)
                {
                    regularArray[i].UpdateMe();
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (fixedUpdateArrayCount == 0) return;

        TrySortingArrays();

        for (int i = 0; i < fixedUpdateArrayCount; i++)
        {
            if (fixedArray[i].internal_Enabled)
                fixedArray[i].FixedUpdateMe();
        }
        
    }

    private void LateUpdate()
    {        
        if (lateUpdateArrayCount == 0) return;

        TrySortingArrays();

        for (int i = 0; i < lateUpdateArrayCount; i++)
        {
            if (lateArray[i].internal_Enabled)
                lateArray[i].LateUpdateMe();
        }
        
    }
    
    #endregion
    
}











