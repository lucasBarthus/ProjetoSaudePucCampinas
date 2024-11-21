using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Scroller : MonoBehaviour
{

    public RawImage rawImag;
    public float x, y;

    void Update()
    {
        rawImag.uvRect = new Rect(rawImag.uvRect.position + new Vector2(x, y) * Time.deltaTime, rawImag.uvRect.size);
    }
}
