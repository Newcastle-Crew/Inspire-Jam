#region 'using' information
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace KeySystem
{
    public class KeyItemController : MonoBehaviour
    {
        [SerializeField] private bool redDoor = false;
        [SerializeField] private bool blueDoor = false;

        [SerializeField] private bool redKey = false;
        [SerializeField] private bool blueKey = false;

        [SerializeField] private KeyInventory keyInventory = null;

        private KeyDoorController doorObject;

        private void Start()
        {
            if(redDoor || blueDoor)
            {
                doorObject = GetComponent<KeyDoorController>();
            }
        }

        public void ObjectInteraction()
        {
            if(redDoor || blueDoor)
            {
                doorObject.PlayAnimation();
            }

            else if(redKey)
            {
                keyInventory.hasRedKey = true;
                gameObject.SetActive(false);
            }

            else if (blueKey)
            {
                keyInventory.hasBlueKey = true;
                gameObject.SetActive(false);
            }
        }
    }
}