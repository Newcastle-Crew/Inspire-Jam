using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
    public static InventoryHandler Instance;

    public GameObject itemPrefab;

    void Awake() {
        Instance = this;
    }

    public static void AddItem(Item item) {
        if (!Instance) Debug.LogError("No InventoryHandler instance created (please add this to the ui)!");

        var itemSlot = Instantiate(Instance.itemPrefab);
        itemSlot.transform.SetParent(Instance.transform);
        var inventoryButton = itemSlot.GetComponent<InventoryButton>();
        inventoryButton.item = item;
        inventoryButton.UpdateName();
    }
}
