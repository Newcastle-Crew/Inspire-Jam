using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InventoryButton : MonoBehaviour
{
    public Item item;

    void Awake() {
        GetComponent<Button>().onClick.AddListener(Click);
    }

    public void UpdateName() {
        // Hack!
        GetComponentInChildren<Text>().text = item.name;
    }

    public void Click() {
        if (item is Equippable equipItem) {
            if (equipItem.kind == Player.ItemKind.None) {
                GameState.Instance.PutMessage("Unequipped", 1.5f);
            } else {
                GameState.Instance.PutMessage("Equipped " + equipItem.name, 1.5f);
            }
            Player.Equip(equipItem.kind);
            GameState.CloseInventory();
        } else if (item is Key key) {
            GameState.Instance.PutMessage("Use keys to unlock doors", 1.5f);
        } else if (item is NoteItem note) {
            GameState.OpenNote(note);
        } else if (item == null) {
            Debug.LogError("Null item type!!!!");
        } else {
            Debug.LogWarning("Unhandled item type");
        }
    }
}
