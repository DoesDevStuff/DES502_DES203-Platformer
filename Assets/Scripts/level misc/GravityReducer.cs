using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityReducer : MonoBehaviour
{
    public GameObject player;
    [Space]
    public float minGravity;
    float defaultGravity;
    [Space]
    public float maxHeight;
    public float minHeight;
    [Space]
    public bool active;

    float peakGravityMultiplier;

    V2PlayerController playerScript;

    void Start()
    {
        playerScript = player.GetComponent<V2PlayerController>();
        defaultGravity = playerScript.gravity;
        peakGravityMultiplier = playerScript.peakGravity / defaultGravity;
    }

    void Update()
    {
        if(player.transform.position.y > minHeight && active == true)
        {
            float t = (player.transform.position.y - minHeight) / maxHeight;

            playerScript.gravity = Mathf.Lerp(defaultGravity, minGravity, t);
            playerScript.peakGravity = Mathf.Lerp(defaultGravity, minGravity, t) * peakGravityMultiplier;
        }
        else
        {
            playerScript.gravity = defaultGravity;
            playerScript.peakGravity = defaultGravity * peakGravityMultiplier;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawRay(new Vector3(transform.position.x, minHeight), Vector2.left);
        Gizmos.DrawRay(new Vector3(transform.position.x, maxHeight), Vector2.left);
    }
}
