using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int healthPlayer;
    public int numOfHeartsPlayer;

    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    void Update(){

      if(healthPlayer > numOfHeartsPlayer) {
        healthPlayer = numOfHeartsPlayer;
      }

      for (int i = 0; i < hearts.Length; i++) {

        if(i < healthPlayer) {
          hearts[i].sprite = fullHeart;
        } else {
          hearts[i].sprite = emptyHeart;
        }

        if(i < numOfHeartsPlayer) {
          hearts[i].enabled = true;
        } else {
          hearts[i].enabled = false;
        }
      }
    }
}
