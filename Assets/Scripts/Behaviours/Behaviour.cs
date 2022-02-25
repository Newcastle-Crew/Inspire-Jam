using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Behaviour: MonoBehaviour {
    public bool wantsToExit = false;

    /// Every frame, a `Run` runs. Hooray!
    public abstract void Run(TalkativeNpc self); 

    /// Requests the behaviour to handle some impulse. Return true if it is handled by the behaviour.
    /// This can both happen on a behaviour that is currently running, but also on one that isn't.
    public virtual bool HandleImpulse(ImpulseInfo info, Impulse impulse, bool is_source_impulse = false) => false; 

    /// Is called when the behaviour is stopped
    public virtual void Stop() { }
    /// Is called when the behaviour is yielded(stopped temporarily). Return false to make the behaviour
    /// get removed when it's stopped.
    public virtual bool Yield() {
        return true;
    }
}