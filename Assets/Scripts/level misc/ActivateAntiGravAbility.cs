using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAntiGravAbility : MonoBehaviour
{
    public List<GameObject> suspendedObjects = new List<GameObject>();
    [Space]
    public Vector2 triggerBoxSize;
    public LayerMask player;

    public GravityReducer gravityReducer;

    bool triggered = false;

    // Update is called once per frame
    void Update()
    {
        if (triggered == false)
        {
            RaycastHit2D triggerBox = Physics2D.BoxCast(transform.position, triggerBoxSize, 0, Vector2.zero, 0, player);
            if (triggerBox.collider != null)
            {
                AntiGravAbility antiGrav = triggerBox.collider.gameObject.GetComponent<AntiGravAbility>();
                antiGrav.active = true;

                gravityReducer.active = false;

                triggered = true;

                foreach(GameObject suspendedObject in suspendedObjects)
                {
                    Rigidbody2D suspendedBody = suspendedObject.GetComponent<Rigidbody2D>();
                    suspendedBody.bodyType = RigidbodyType2D.Dynamic;
                    suspendedBody.mass = 90000;
                    suspendedBody.gravityScale = 2;
                }

                Destroy(gameObject);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, triggerBoxSize);
    }
}
