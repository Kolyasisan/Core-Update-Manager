//* ---------------------------------------------------------------
//* "THE BEERWARE LICENSE" (Revision 42):
//* Nikolai "Kolyasisan" Ponomarev @ PCHK Studios wrote this code.
//* As long as you retain this notice, you can do whatever you
//* want with this stuff. If we meet someday, and you think this
//* stuff is worth it, you can buy me a beer in return.
//* ---------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFramework.Tests
{
    //First, you inherit from CoreMonoBeh
    public class TestBehaviour : CoreMonoBeh
    {
        public float UpdateValue;
        public float UpdateRand;

        public float FixedValue;
        public float FixedRand;

        //Then, you override this method and setup your configs
        public override void CoreInitSetup()
        {
            //Using default will create a config with, well, default values, but you can provide your own.
            //Look up the docs for settings if you don't understand something.

            UM_SETTINGS_UPDATE = new LoopUpdateSettings(default);

            //Here, for example, we've changed the execution order from 128 to 32, which will make this script execute earlier in a queue than those with a higher number.
            UM_SETTINGS_FIXEDUPDATE = new LoopUpdateSettings(32);
        }

        //Additionally, we can use the new Awake, Start, OnDisable, etc. methods that all start with Core and are overriden from the base.
        //There are CoreAwake, CoreStart, CoreOnDestroy, CoreOnDisable and CoreOnEnable.
        public override void CoreAwake()
        {
            UpdateRand = Random.Range(5f, 10f);
            FixedRand = Random.Range(5f, 10f);
        }

        public override void CoreUpdate()
        {
            UpdateValue = Mathf.Repeat(UpdateValue + UpdateRand * Time.deltaTime, 4096f);
        }

        public override void CoreGameplayUpdate()
        {
            //This function will not be called because the config for this function was not initialized in the CoreInitSetup method.
        }

        public override void CoreFixedUpdate()
        {
            FixedValue = Mathf.Repeat(FixedValue + FixedRand * Time.fixedDeltaTime, 4096f);
        }
    }
}