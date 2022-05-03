using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    /// <summary>
    /// delta is the distance that the ortho camera moved.
    /// The layers will move with parallaxFactor 
    /// ( if parallaxFactor is 1 then the layer will move 
    /// at the same speed as the camera, if it's 0.5 then 
    /// it will move 2 times slower, 2 for 2 times faster
    /// </summary>
    public float parallaxFactor;
    public void Move(float delta)
    {
        Vector3 newPos = transform.localPosition;
        newPos.x -= delta * parallaxFactor;
        transform.localPosition = newPos;
    }
}