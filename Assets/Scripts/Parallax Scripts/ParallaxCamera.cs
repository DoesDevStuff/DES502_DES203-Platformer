using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParallaxCamera : MonoBehaviour
{
    /// <summary>
    /// We've separated this so that there is a different
    /// handler for how the camera processes the various 
    /// backgrounds. That way we don't need to set up a
    /// new camera.
    /// 
    /// We're basically looking to translate the camera 
    /// position and simulate the next physical position.
    /// </summary>
    /// <param name="Parallax Camera"></param>

    public delegate void ParallaxCameraDelegate(float deltaMovement);
    public ParallaxCameraDelegate onCameraTranslate;
    private float oldPosition;
    void Start()
    {
        oldPosition = transform.position.x;
    }
    void FixedUpdate()
    {
        if (transform.position.x != oldPosition)
        {
            if (onCameraTranslate != null)
            {
                float delta = oldPosition - transform.position.x;
                onCameraTranslate(delta);
            }
            oldPosition = transform.position.x;
        }
    }
}