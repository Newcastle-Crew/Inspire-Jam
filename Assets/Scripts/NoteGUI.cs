using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteGUI : MonoBehaviour
{
    public static NoteGUI Instance = null;

    public Text title;
    public Text body;

    void Awake() {
        Instance = this;
        this.gameObject.SetActive(false);
    }

    public void Close() {
        GameState.CloseNote();
    }
}
