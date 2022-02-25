using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteItem : Item {
    public override string name { get => this.title; }

    public string title;

    // TODO: The nice way to do this(allow multiple lines of editing) would be to make a custom editor styling that uses the line editing thing, but I don't wanna bother.
    public List<string> bodyText;
}
