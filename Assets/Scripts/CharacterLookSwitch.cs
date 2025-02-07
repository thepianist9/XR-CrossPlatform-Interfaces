using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using XRSpatiotemopralAuthoring;

public class CharacterLookSwitch : MonoBehaviour
{
    private StarterAssetsInputs starterAssetsInputs;
    private PlayerInput player;
    // Start is called before the first frame update
    void Start()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            starterAssetsInputs.cursorInputForLook = !starterAssetsInputs.cursorInputForLook;
            Debug.Log("ping");
        }

    }

    public void SwitchLook()
    {
        starterAssetsInputs.cursorInputForLook = !starterAssetsInputs.cursorInputForLook;
    }

}
