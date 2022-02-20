using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InventoryButton : MonoBehaviour
{
    public Player.ItemKind item = Player.ItemKind.None;

    void Awake() {
        GetComponent<Button>().onClick.AddListener(Click);
    }

    public void Click() {
        Player.Equip(item);
        GameState.CloseInventory();
    }
}
