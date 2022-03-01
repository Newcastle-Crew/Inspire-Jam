using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equippable : Item {
    // If this is not empty, it will override the normal name, in case you want to give things better names later.
    public string override_name = "";
    public override string name { get => (override_name.Length > 0) ? override_name : (this.kind == Player.ItemKind.Gun ? (single_use ? "Fruit bowl" : "Gun") : "None"); }
    public bool single_use = false;

    public AudioClip usageSound;

    public Player.ItemKind kind;
}
