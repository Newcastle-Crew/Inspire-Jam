#region 'Using' information
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#endregion

public class OpenLinks : MonoBehaviour
{
    public void WestyCredit()
    { Application.OpenURL("https://westydesign.itch.io/"); }

    public void TrolledCredit()
    { Application.OpenURL("https://trolledwoods.itch.io/"); }

    public void LawtonCredit()
    { Application.OpenURL("https://www.instagram.com/lami_made_a_thing/"); }

    public void LeonCredit()
    { Application.OpenURL("https://dxrkest.itch.io"); }

    public void ScottCredit()
    { Application.OpenURL("https://itch.io/profile/thefancyevil"); }

    public void PaulCredit()
    { Application.OpenURL("https://www.youtube.com/channel/UCRohkWLVomg92U9FMbK3hTg/videos"); }
}