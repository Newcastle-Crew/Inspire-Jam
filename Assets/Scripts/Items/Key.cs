using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Item {
    public override string name { get => (this.color == Color.Red ? "Hall key" : "Employee key") + (singleUse ? " (Fragile!)" : ""); }
    public bool singleUse = true;

    public enum Color { Red, Blue };

    public Color color = Color.Red;
}
