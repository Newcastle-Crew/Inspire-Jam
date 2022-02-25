using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a workaround around the fact that unity seemingly doesn't have a way to run something on the start for a disabled gameobject, so we need to make it disabled only at the second frame.
// This means that there may be some screen flicker at the start of the game, but ithis is a jam so I honestly don't really care. The real fix is to not use singletons I presume?
public class FirstFrameDisable : MonoBehaviour {
    void Start() {
        gameObject.SetActive(false);
    }
}
