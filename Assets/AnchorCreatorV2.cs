using Game.ServerAuthoritativeSynchronousSpawning;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Feedback;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.ARFoundation.Samples
{
    public class AnchorCreatorV2 : MonoBehaviour
    {
        [SerializeField]
        InputActionReference SpawnActionReference; // Action for spawn

        [SerializeField] private GameObject SpawnableObject;
        [SerializeField] private GameObject ObjectTable;

        [SerializeField] private XRRayInteractor _leftRayInteractor;

        [SerializeField] private Transform DataObjects;
        [SerializeField] private XRServerAuthoritativeSynchronousSpawning _sharedAuthoritativeSynchronousSpawning;

        [SerializeField]
        string targetTag = "ARSpawnable"; // Tag for raycast target validation
        [SerializeField]
        string deleteTag = "ARSpawnable"; // Tag for raycast target validation

        [SerializeField]private bool createFlag = false;
        [SerializeField] private bool deleteFlag = false;
        [SerializeField] private bool editFlag = false;

        [SerializeField] private Image createButton;
        [SerializeField] private Image deleteButton;
        [SerializeField] private Image editButton;
        void OnEnable()
        {
            SpawnActionReference.action.Enable();
            SpawnActionReference.action.performed += Spawn;
        }

        void OnDisable()
        {
            SpawnActionReference.action.Disable();
            SpawnActionReference.action.performed -= Spawn;
        }

        void Spawn(InputAction.CallbackContext context)
        {
            if (createFlag)
            {
                Debug.Log("Spawn");
                _sharedAuthoritativeSynchronousSpawning.SpawnObjectfromClientServerRpc(ObjectTable.transform.position + Vector3.up *0.2f, Quaternion.identity);
            }

            if (_leftRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                
                if (hit.collider.CompareTag(deleteTag) && deleteFlag)
                {
                    Debug.Log("Delete");
                    _sharedAuthoritativeSynchronousSpawning.HideObjectfromClientServerRpc(hit.collider.gameObject.name);
                }
               
            }
        }

        public void SetCreateFlag()
        {
            createFlag = true;
            deleteFlag = false;
            editFlag = false;

            createButton.color = Color.green;
            deleteButton.color = Color.white;
            editButton.color = Color.white;
        }

        public void SetDeleteFlag()
        {
            createFlag = false;
            deleteFlag = true;
            editFlag = false;

            createButton.color = Color.white;
            deleteButton.color = Color.green;
            editButton.color = Color.white;
        }
        public void SetEdit()
        {
            createFlag = false;
            deleteFlag = false;
            editFlag = true;

            createButton.color = Color.white;
            deleteButton.color = Color.white;
            editButton.color = Color.green;

            foreach (Transform tr in DataObjects.transform)
            {
                XRGrabInteractable grab = tr.GetComponent<XRGrabInteractable>();
                if (grab)
                {
                    grab.enabled = editFlag;
                }
            }
        }
    }
}
