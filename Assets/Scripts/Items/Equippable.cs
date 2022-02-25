using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equippable : Item {
    public override string name { get => this.kind == Player.ItemKind.Gun ? "Gun" : "None"; }

    public Player.ItemKind kind;
}
