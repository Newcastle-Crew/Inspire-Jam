using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Item {
    public override string name { get => this.color == Color.Red ? "Red key" : "Blue key"; }

    public enum Color { Red, Blue };

    public Color color = Color.Red;
}
