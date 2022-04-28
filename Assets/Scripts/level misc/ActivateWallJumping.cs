using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateWallJumping : MonoBehaviour
{
    public Vector2 triggerBoxSize;
    public LayerMask player;

    bool triggered = false;

    // Update is called once per frame
    void Update()
    {
        if (triggered == false)
        {
            RaycastHit2D triggerBox = Physics2D.BoxCast(transform.position, triggerBoxSize, 0, Vector2.zero, 0, player);
            if(triggerBox.collider != null)
            {
                V2PlayerController pC = triggerBox.collider.gameObject.GetComponent<V2PlayerController>();
                pC.canWallJump = true;
                pC.canWallSlide = true;

                triggered = true;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, triggerBoxSize);
    }
}
