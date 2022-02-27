using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Item {
    public override string name { get => (this.color == Color.Red ? "Red key" : "Blue key") + (singleUse ? " (once)" : ""); }
    public bool singleUse = true;

    public enum Color { Red, Blue };

    public Color color = Color.Red;
}
