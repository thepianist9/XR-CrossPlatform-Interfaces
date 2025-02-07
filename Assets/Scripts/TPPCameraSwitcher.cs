using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TPPCameraSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject tppCamera;
    [SerializeField] private GameObject tppMenuCamera;
    [SerializeField] private PlayerInput starterAssetsInputs;

    [SerializeField] private TMP_Text text;
    private bool focus = false;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            SetTPPView();
        }

    }

    public void SetTPPView()
    {
        focus = !focus;

        if (focus)
        {
            starterAssetsInputs.SwitchCurrentActionMap("UI");
            //set Camera lock ui
            StartCoroutine(Delay(true));
        }

        else
        {
            starterAssetsInputs.SwitchCurrentActionMap("Player");
            StartCoroutine(Delay(false));
        }

    }

    private IEnumerator Delay(bool lockCamera)
    {
        string cameralock;
        //show delay and hide
        if (lockCamera)
        {
            cameralock = "locked";
            text.color = Color.red;
        }
        else
        {
            cameralock = "free";
            text.color = Color.green;
        }
        text.text = "Camera " + cameralock;
        text.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        text.gameObject.SetActive(false);

    }
}
