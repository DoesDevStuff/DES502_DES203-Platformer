using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParallaxBackground : MonoBehaviour
{
    public ParallaxCamera parallaxCamera;
    List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();

    void Start()
    {
        if (parallaxCamera == null)
            parallaxCamera = Camera.main.GetComponent<ParallaxCamera>();
        if (parallaxCamera != null)
            parallaxCamera.onCameraTranslate += Move;
        SetLayers();
    }

    /// Sorts through and relabels all layers in object this
    /// script is attached, as Layer-0, Layer-1, Layer-2 etc
    /// This is done so that the individual parallax layers
    /// i.e the objects inside object the background script 
    /// is on can be given a parallax factor much more easily.
    /// 
    /// Layers are now also very easily sorted


    void SetLayers()
    {
        parallaxLayers.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();

            if (layer != null)
            {
                layer.name = "Layer-" + i;
                parallaxLayers.Add(layer);
            }
        }
    }
    void Move(float delta)
    {
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            layer.Move(delta);
        }
    }
}
