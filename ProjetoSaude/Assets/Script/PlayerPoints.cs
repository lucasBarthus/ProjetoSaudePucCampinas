using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class PlayerPoints : MonoBehaviour
{
    private TextMeshPro PlayerPointText;
    private PlayerMovementFusion player;

    private void Start()
    {
        PlayerPointText = GameObject.Find("PointsText").GetComponent<TextMeshPro>();

    }

    private void Update()
    {

        //PlayerPointText.text += 
    }
}
