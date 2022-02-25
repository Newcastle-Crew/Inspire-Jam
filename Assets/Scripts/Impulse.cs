using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpulseInfo {
    public Vector3 pos;
    public bool visible = false;
    public bool audible = false;
    public bool fromPlayer = false;
}

///
/// This is some sensory input that can cause enragement (or something).
/// Then, states of an NPC is usually either some default behaviour (like patrolling)   - should being scared also be a default behaviour, it just triggers in a certain emotional state?
/// We can then query this state of the NPC (along with the emotional state), to see if an impulse should be handled.
///
/// Probably using a similar system to window events in windows, you pass the impulse to the behaviour, see if it is handled (encompassed by the behaviour), like while being sadge
/// hearing as sound could just cause a twitch of fright but not actually change the state. Then, if it's not handled, then we go ahead and handle it in the default way, by adding a new
/// state onto the state stack and dealing with it that way. (this might replace a previous short-term state probably?)
///
/// Whether you interact with a state or not might also depend on the auditorial/emotional state of the room around you, so if you're in a loud room for example you're not going to react
/// to running footsteps. Last question is does impulses get generated from entities other than the player? Could be a cool immersive effect, but harder to deal with. However, could also
/// be more interesting to interact with.
///
[System.Serializable]
public abstract class Impulse {
    public string name = "<anonymous>";
    public int importance = 1;
    public bool isSuspicious = false;
    public float susLevel = 0f;

    public bool is_auditorial = true;
    public float audio_radius = 5f;

    public bool is_visual = true;
}

[System.Serializable]
public class GenericSound: Impulse {
}