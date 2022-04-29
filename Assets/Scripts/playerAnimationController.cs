using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerAnimationController : MonoBehaviour
{
    PlayerController playerController;
    CharacterController2D characterController;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        playerController = gameObject.GetComponent<PlayerController>();
        characterController = gameObject.GetComponent<CharacterController2D>();
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("horizontalMovement", Mathf.Abs(playerController.MoveDirection.x));
        animator.SetFloat("verticalMovement", playerController.MoveDirection.y);

        if (characterController.below)
            animator.SetBool("isGrounded", true);
        else
            animator.SetBool("isGrounded", false);

        if ((characterController.left || characterController.right) && !characterController.below)
            animator.SetBool("onWall", true);
        else
            animator.SetBool("onWall", false);

        if (playerController.isGliding)
            animator.SetBool("isGliding", true);
        else
            animator.SetBool("isGliding", false);

        if (playerController.isDucking)
            animator.SetBool("isCrouching", true);
        else
            animator.SetBool("isCrouching", false);

        if (characterController.isSubmerged)
            animator.SetBool("inWater", true);
        else
            animator.SetBool("inWater", false);

    }
}