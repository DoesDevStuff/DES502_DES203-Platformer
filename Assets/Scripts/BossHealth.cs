using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    public int healthBoss;
    public int numOfHeartsBoss;

    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    void Update(){

      if(healthBoss > numOfHeartsBoss) {
        healthBoss = numOfHeartsBoss;
      }

      for (int i = 0; i < hearts.Length; i++) {

        if(i < healthBoss) {
          hearts[i].sprite = fullHeart;
        } else {
          hearts[i].sprite = emptyHeart;
        }

        if(i < numOfHeartsBoss) {
          hearts[i].enabled = true;
        } else {
          hearts[i].enabled = false;
        }
      }
    }
}
