using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVisible : MonoBehaviour
{
    public GameObject activeGameObject;

    /* Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    */

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            activeGameObject.SetActive(true);
        }
        else
        {
            activeGameObject.SetActive(false);
        }
    }
}
