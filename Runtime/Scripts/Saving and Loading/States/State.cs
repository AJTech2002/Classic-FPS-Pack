using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ClassicFPS.Saving_and_Loading.States
{
    //Base class for storing all your States
    public class State : MonoBehaviour
    {
        //A method that will be called when the Game Is Saved (returns the JSON of the State)
        public virtual string SaveState ()
        {
            return null;
        }

        //Loads the State by passing in JSON found from the file
        public virtual void LoadState (string loadedJSON)
        {
        }
    
        //Tells the Saving System whether or not this State needs to be saved (can save space)
        public virtual bool ShouldSave ()
        {
            return true;
        }

        //A Unique Id Generator that is able to identify which save files belong to which objects
        public virtual string GetUID ()
        {
            return (gameObject.scene.name+"_"+gameObject.name+"_"+(this.GetType()));
        }

        public virtual bool ShouldLoad ()
        {
            return true;
        }

    }
}