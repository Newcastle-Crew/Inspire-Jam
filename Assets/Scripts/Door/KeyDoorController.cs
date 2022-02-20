#region 'Using' information
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace KeySystem
{
    public class KeyDoorController : MonoBehaviour
    {
        private Animator doorAnim;
        private bool doorOpen = false;

        [SerializeField] private string openAnimationName = "DoorOpen";
        [SerializeField] private string closeAnimationName = "DoorClose";

        [SerializeField] private int timeToShowUI = 1;
        [SerializeField] private GameObject showDoorLockedUI = null;

        [SerializeField] private KeyInventory keyInventory = null; // Keeps track of keys the player's grabbed so that doors correspond to their keys.

        [SerializeField] private int waitTimer = 1; // Gives a 1-second timer between showing the "LOCKED" message.
        [SerializeField] private bool pauseInteraction = false; // Stops the player from spamming interaction.

        private void Awake()
        {
            doorAnim = gameObject.GetComponent<Animator>();
        }

        private IEnumerator PauseDoorInteraction() // prevents door spamming open / shut
        {
            pauseInteraction = true;
            yield return new WaitForSeconds(waitTimer);
            pauseInteraction = false;
        }

        public void PlayAnimation()
        {
            if(keyInventory.hasRedKey)
            { OpenDoor(); }

            else if(keyInventory.hasBlueKey)
            { OpenDoor(); }

            else 
            { StartCoroutine(ShowDoorLocked()); }
        }

        void OpenDoor()
        {
            if (!doorOpen && !pauseInteraction)
            {
                doorAnim.Play(openAnimationName, 0, 0.0f);
                doorOpen = true; // Opens the door.
                StartCoroutine(PauseDoorInteraction()); // Stops the player from spamming the door.
            }

            else if (doorOpen && !pauseInteraction)
            {
                doorAnim.Play(closeAnimationName, 0, 0.0f);
                doorOpen = false; // Closes the door.
                StartCoroutine(PauseDoorInteraction()); // Stops  the player from spamming the doors.
            }
        }

        IEnumerator ShowDoorLocked()
        {
            showDoorLockedUI.SetActive(true);
            yield return new WaitForSeconds(timeToShowUI);
            showDoorLockedUI.SetActive(false);
        }
    }
}