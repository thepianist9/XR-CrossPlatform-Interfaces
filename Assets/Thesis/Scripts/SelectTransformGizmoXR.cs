//MIT License
//Copyright (c) 2023 DA LAB (https://www.youtube.com/@DA-LAB)
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
using UnityEngine;
using UnityEngine.EventSystems;
using RuntimeHandle;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
using Game.ServerAuthoritativeSynchronousSpawning;
using Unity.Netcode;
using Game;
using XRSpatiotemopralAuthoring;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SelectTransformGizmoXR : MonoBehaviour
{
    [Header("Material for edit")]
    public Material highlightMaterial;
    public Material selectionMaterial;
    [SerializeField] private Image editSpaceImage;
    [SerializeField] private XRServerAuthoritativeSynchronousSpawning serverAuthoritativeSpawning;
    [SerializeField] private NetworkManager networkManager;

    [SerializeField] private XRNetworkAppController m_AppController;

    [SerializeField] private Image EditImage;
    [SerializeField] private Image m_PositionBtnImage;
    [SerializeField] private Image m_RotationBtnImage;
    [SerializeField] private Image m_ScaleBtnImage;

    [Header("UI Colors")]
    [SerializeField] private Color NonEditColor;
    [SerializeField] private Color EditColor;
    [SerializeField]
    InputActionReference TriggerActionReference; // Action for spawn
    [SerializeField] private XRRayInteractor _rightRayInteractor;

    private Transform highlight;
    private Transform selection;
    private RaycastHit raycastHit;
    private RaycastHit raycastHitHandle;
    private GameObject runtimeTransformGameObj;
    private RuntimeTransformHandle runtimeTransformHandle;
    private int runtimeTransformLayer = 6;
    private int runtimeTransformLayerMask;

    private bool isEditing = false;

    private void Start()
    {
        Reset();
    }
    private void Reset()
    {
        runtimeTransformGameObj = new GameObject();
        runtimeTransformHandle = runtimeTransformGameObj.AddComponent<RuntimeTransformHandle>();
        runtimeTransformGameObj.layer = runtimeTransformLayer;
        runtimeTransformLayerMask = 1 << runtimeTransformLayer; //Layer number represented by a single bit in the 32-bit integer using bit shift
        runtimeTransformHandle.type = HandleType.POSITION;
        runtimeTransformHandle.autoScale = true;
        runtimeTransformHandle.autoScaleFactor = 1.0f;
        runtimeTransformGameObj.SetActive(false);
    }

    void Update()
    {

        if (isEditing)
        {
            // Highlight
            if (highlight != null)
            {
                var renderer = highlight.GetComponent<MeshRenderer>();
                renderer.materials = new Material[] { (renderer.materials[0]) };
                highlight = null;
            }
            Ray ray;
            if((PlatformManager.Instance.platform == PlatformManager.Platform.Desktop)|| (PlatformManager.Instance.platform == PlatformManager.Platform.Editor))
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            }
            else if (PlatformManager.Instance.platform == PlatformManager.Platform.AR)
            {
                ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            }
            else
            {
                // If no platform matches, set a default ray (optional)
                ray = new Ray(); // Initialize to prevent unassigned variable errors
            }



            if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit)) //Make sure you have EventSystem in the hierarchy before using EventSystem
            {
                if (raycastHit.transform.CompareTag("ARSpawnable"))
                    highlight = raycastHit.transform;
                else
                    highlight = null;
            }

            // Selection
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {

                ApplyLayerToChildren(runtimeTransformGameObj);
                if (Physics.Raycast(ray, out raycastHit))
                {
                    if (Physics.Raycast(ray, out raycastHitHandle, Mathf.Infinity, runtimeTransformLayerMask)) //Raycast towards runtime transform handle only
                    {
                    }
                    else if (highlight)
                    {
                        selection = raycastHit.transform;
                        selection.GetComponent<DataObject>().Select();
                        runtimeTransformHandle.target = selection;
                        runtimeTransformGameObj.SetActive(true);
                        highlight = null;
                    }
                    else
                    {
                        if (selection)
                        {
                            selection.GetComponent<DataObject>().UnSelect();
                            selection = null;

                            runtimeTransformGameObj.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (selection)
                    {
                        selection.GetComponent<DataObject>().UnSelect();
                        selection = null;

                        runtimeTransformGameObj.SetActive(false);
                    }
                }
            }

            //Hot Keys for move, rotate, scale, local and Global/World transform
            if (runtimeTransformGameObj.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    runtimeTransformHandle.type = HandleType.POSITION;
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    runtimeTransformHandle.type = HandleType.ROTATION;
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    runtimeTransformHandle.type = HandleType.SCALE;
                }
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    if (Input.GetKeyDown(KeyCode.G))
                    {
                        runtimeTransformHandle.space = HandleSpace.WORLD;
                    }
                    if (Input.GetKeyDown(KeyCode.L))
                    {
                        runtimeTransformHandle.space = HandleSpace.LOCAL;
                    }
                }
            }
        }

    }

    public void ToggleEdit()
    {
        isEditing = !isEditing;

        if (isEditing)
        {
            ActivateEdit();
        }
        else
        {
            DeactivateEdit();
        }


    }

    public void ActivateEdit()
    {
        m_PositionBtnImage.color = Color.green;
        EditImage.color = Color.green;

    }
    public void DeactivateEdit()
    {

        runtimeTransformHandle.type = HandleType.POSITION;
        runtimeTransformGameObj.SetActive(false);
        ResettransformGroupColors();
        EditImage.color = Color.white;
    }

    private void ResettransformGroupColors()
    {
        // Reset button colors to white
        m_PositionBtnImage.color = Color.white;
        m_RotationBtnImage.color = Color.white;
        m_ScaleBtnImage.color = Color.white;
        m_AppController.ResetAllNetworkObjects();
    }


    public void ChangeHandleType(string handleType)
    {

        ResettransformGroupColors();
        // Change handle type and button color based on selection
        switch (handleType)
        {
            case "Position":
                runtimeTransformHandle.type = HandleType.POSITION;
                m_PositionBtnImage.color = Color.green;
                break;
            case "Rotation":
                runtimeTransformHandle.type = HandleType.ROTATION;
                m_RotationBtnImage.color = Color.green;
                break;
            case "Scale":
                runtimeTransformHandle.type = HandleType.SCALE;
                m_ScaleBtnImage.color = Color.green;
                break;
        }
    }


    private void ApplyLayerToChildren(GameObject parentGameObj)
    {
        foreach (Transform transform1 in parentGameObj.transform)
        {
            int layer = parentGameObj.layer;
            transform1.gameObject.layer = layer;
            foreach (Transform transform2 in transform1)
            {
                transform2.gameObject.layer = layer;
                foreach (Transform transform3 in transform2)
                {
                    transform3.gameObject.layer = layer;
                    foreach (Transform transform4 in transform3)
                    {
                        transform4.gameObject.layer = layer;
                        foreach (Transform transform5 in transform4)
                        {
                            transform5.gameObject.layer = layer;
                        }
                    }
                }
            }
        }
    }

}