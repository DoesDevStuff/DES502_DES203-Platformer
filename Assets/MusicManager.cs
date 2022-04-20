using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    //Wwise Changes state of Play_Music_Tracks Event
    static public void SetBehemothCombat()
    {
        AkSoundEngine.SetState("Current_Location", "Boss_Room60");
    }
    static public void SetBehemothIntro()
    {
        AkSoundEngine.SetState("Current_Location", "Boss_Intro60");
    }
    static public void SetNomadMain()
    {
        AkSoundEngine.SetState("Current_Location", "Exploration77");
    }

}