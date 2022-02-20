using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteGUI : MonoBehaviour
{
    public Text title;
    public Text body;

    void Awake() {
    }

    public void Close() {
        GameState.CloseNote();
    }
}
