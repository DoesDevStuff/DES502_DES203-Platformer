using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KillPlayer : MonoBehaviour
{
    [Header("Wwise Events")]
    public AK.Wwise.Event Death_Player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    /* Update is called once per frame
    void Update()
    {
        
    }*/

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Death_Player.Post(gameObject);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
