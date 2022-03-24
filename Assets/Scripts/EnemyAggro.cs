using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAggro : MonoBehaviour
{
    #region Animation stuff
    /// <summary>
    /// In case we decide to do a wake up animation on just the face 
    /// 
    /// We have a GameObject face ensure this is inside the enemy object on top.
    /// This is where we have animation for eyes (eg: Sleeping and then annoyed )
    /// </summary>

    // [SerializeField]
    // GameObject face;

    // Animator faceAnimator; // so that you don't have a whole animator for face emotions

    #endregion

    #region Enemy accessible stuff
    [SerializeField]
    Transform player;

    [SerializeField]
    float aggroRange; // range to see player

    [SerializeField]
    float moveSpeed;
    #endregion

    Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
       // faceAnimator = face.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // check distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        //Debug.Log("distanceToPlayer : " + distanceToPlayer);

        if(distanceToPlayer < aggroRange)
        {
            //chase player
            ChasePlayer();
        }
        else
        {
            //stop chasing player
            StopChasingPlayer();
        }
    }


    private void ChasePlayer()
    {
        if (transform.position.x < player.position.x)
        {
            // enemy on left i.e move right
            rb2d.velocity = new Vector2(moveSpeed, 0);
            transform.localScale = new Vector2(-1, 1); // turn left
        }
        else if (transform.position.x > player.position.x) // counter for if we're on top
        {
            //enemy on right i.e move left
            rb2d.velocity = new Vector2(-moveSpeed, 0);
            transform.localScale = new Vector2(1, 1);// turn right
        }

       // faceAnimator.Play("Animation Name");
    }

    private void StopChasingPlayer()
    {
        rb2d.velocity = Vector2.zero;
        // faceAnimator.Play("Animation Name");
    }
}
