using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    public GameObject PlayerUI;
    public GameObject MovieUI;
    public GameObject MovieCam;

    void Start()
    {
        PlayerUI.SetActive(false);
        MovieUI.SetActive(true);
        MovieCam.SetActive(true);
        StartCoroutine(CutsceneDuration());
    }

    
    IEnumerator CutsceneDuration(){
        yield return new WaitForSeconds(5.0f);
        PlayerUI.SetActive(true);
        MovieUI.SetActive(false);
        MovieCam.SetActive(false);
    }
}
