using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiController : MonoBehaviour
{

    public FusionManager fusionManager;


    public void Start()
    {

        fusionManager = GameObject.Find("[Fusion-Manager]").GetComponent<FusionManager>();
    }


    public void OnClickButton()
    {

        fusionManager.OnStartGameButtonClicked();
    }

}



