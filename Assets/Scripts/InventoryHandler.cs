using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
    public static InventoryHandler Instance;

    public GameObject itemPrefab;

    void Awake() {
        Instance = this;
    }

    public static (InventoryButton, Item) FindItemByPredicate(Func<Item, bool> predicate) {
        var childCount = Instance.transform.childCount;
        for (int i=0; i<childCount; i++) {
            var inventoryButton = Instance.transform.GetChild(i).GetComponent<InventoryButton>();
            var item = inventoryButton.item;
            if (predicate(item)) {
                return (inventoryButton, item);
            }
        }

        return (null, null);
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
