#region 'using' information
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#endregion

namespace KeySystem
{
    public class KeyRaycast : MonoBehaviour
    {
        [SerializeField] private int rayLength = 3; // The distance at which you can interact with an object.
        [SerializeField] private LayerMask layerMaskInteract;
        [SerializeField] private string excludeLayerName = null;

        private KeyItemController raycastedObject;
        [SerializeField] private KeyCode openDoorKey = KeyCode.E; // The 'open door' button is "E" (and can be changed.)
        [SerializeField] private Image crosshair = null;
        private bool isCrosshairActive;
        private bool doOnce;

        private string interactableTag = "InteractiveObject";

        private void Start()
        { crosshair.color = Color.clear; } // Stops the crosshair from appearing when it shouldn't by making it invisible as the game starts.

        private void Update()
        {
            RaycastHit hit;
            Vector3 fwd = transform.TransformDirection(Vector3.forward);

            int mask = 1 << LayerMask.NameToLayer(excludeLayerName) | layerMaskInteract.value;

            if(Physics.Raycast(transform.position, fwd, out hit, rayLength, mask))
            {
                if(hit.collider.CompareTag(interactableTag))
                {
                    if(!doOnce)
                    {
                        raycastedObject = hit.collider.gameObject.GetComponent<KeyItemController>();
                        CrosshairChange(true);
                    }

                    isCrosshairActive = true;
                    doOnce = true;

                    if(Input.GetKeyDown(openDoorKey))
                    {
                        raycastedObject.ObjectInteraction();
                    }
                }
            }
            else
            {
                if(isCrosshairActive)
                {
                    CrosshairChange(false);
                    doOnce = false;
                }
            }
        }

        void CrosshairChange(bool on)
        {
            if (on && !doOnce)
            { crosshair.color = Color.red; } // Makes the crosshair appear when player's looking at something interactable.

            else
            {
                crosshair.color = Color.clear; // Otherwise, makes the crosshair invisible.
                isCrosshairActive = false;
            }
        }
    }
}
