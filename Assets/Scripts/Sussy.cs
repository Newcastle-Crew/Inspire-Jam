using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sussy : MonoBehaviour
{
    public const int NONE = 0;
    public const int ATTENTION_GRAB = 1;
    public const int RUDE           = 2;
    public const int MURDER         = 3;

    // TODO: ActionKind should give the sus level
    public enum ActionKind {
        Run,
        Fart,
        Indistinct,
        Reading,
        HoldingGun,
        ShotScream,
    }

    public static int ActionToSus(ActionKind action) {
        switch (action) {
            case ActionKind.Run: return RUDE;
            case ActionKind.Fart: return RUDE;
            case ActionKind.Reading: return RUDE;
            case ActionKind.Indistinct: return ATTENTION_GRAB;
            case ActionKind.HoldingGun: return MURDER;
            case ActionKind.ShotScream: return MURDER;
            default: {
                Debug.LogError("Invalid action");
                return NONE;
            }
        }
    }
}