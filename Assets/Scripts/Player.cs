using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SUPERCharacter.SUPERCharacterAIO))]
public class Player : MonoBehaviour
{
    public AudioSource susSoundTemp;
    SUPERCharacter.SUPERCharacterAIO cam;

    void Awake() {
        cam = GetComponent<SUPERCharacter.SUPERCharacterAIO>();
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.K)) {
            // Do sussy sound.
            GameState.SusSound(transform.position, 7f, 14f, Sussy.ActionKind.Fart);
            GameState.SusAction(Sussy.ActionKind.Fart);
            if(susSoundTemp) susSoundTemp.Play();
        }
    }
}
