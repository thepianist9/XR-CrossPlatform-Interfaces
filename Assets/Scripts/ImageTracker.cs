using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class XRImageTracker : MonoBehaviour
{
    private ARTrackedImageManager trackedImages;
    [SerializeField] public List<GameObject> ArPrefabs; //0: private space 1:shared space
    [SerializeField] private XROrigin arSessionOrigin;
    [SerializeField] private GameObject DataObject;
    void Awake()
    {
        trackedImages = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImages.trackedImagesChanged += OnTrackedImagesChanged;
        Debug.Log("Compute Shader Support: " + SystemInfo.supportsComputeShaders);
    }

    void OnDisable()
    {
        trackedImages.trackedImagesChanged -= OnTrackedImagesChanged;
    }


    // Event Handler
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        //Create network object based on image tracked
        //only server can parent network objects ==>
        //1. send rpc to server to instantiate object at the position of the tracked image
        //2. server spawns the object on the network
        //3. server parents the shared object to the tracked image
        foreach (var trackedImage in eventArgs.added)
        {
            foreach (var arPrefab in ArPrefabs)
            {
                if (trackedImage.referenceImage.name == arPrefab.name)
                {
                    if (trackedImage.referenceImage.name == "Data Objects")
                    { 


                        if (DataObject != null)
                        {

                            DataObject.transform.SetParent(trackedImage.transform, false);
                            DataObject.transform.localPosition = new Vector3(0, 0, 0);  
                            DataObject.transform.localRotation = Quaternion.Euler(0, 0, 0);


                        }
                    }
                }

            }
        }
        foreach(var trackedImage in eventArgs.updated)
        {
            foreach(var arPrefab in ArPrefabs)
            {
                if(arPrefab.name == trackedImage.name)
                {
                    arPrefab.SetActive(trackedImage.trackingState == TrackingState.Tracking);
                }
            }
        }
    }

}

    

