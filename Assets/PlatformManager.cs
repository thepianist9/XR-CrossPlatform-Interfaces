using Assets.David.UILayout;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


namespace XRSpatiotemopralAuthoring
{
    [ExecuteInEditMode]
    public class PlatformManager : MonoBehaviour
    {
        [Header("Platform Settings")]
        [Header("Camera Configuration")]
        //CameraRigs
        [SerializeField] private GameObject Desktop_CameraRig;
        [SerializeField] private GameObject AR_CameraRig;
        [SerializeField] private GameObject MR_CameraRig;
        [SerializeField] private Camera MR_Camera;
        [SerializeField] private ARCameraManager arCameraManager;
        [SerializeField] private Color TransparentColor;
        [SerializeField] private GameObject DataObjects;

        [Header("Environment Configuration")]

        [SerializeField] private GameObject Environment;
        [SerializeField] private GameObject ClientAvatar;

        [SerializeField] private GameObject UI;

 
        [Header("UI Configuration")]
        //UI

        [SerializeField] private LayoutUIManager layoutUIManager;


        //[SerializeField] private GameObject fpvcontroller;

        private static PlatformManager _Instance;
        public static PlatformManager Instance { get { return _Instance; } }




        public enum Platform
        {
            VR,
            AR,
            MR,
            Desktop,
            Editor
        }

        [SerializeField] public Platform platform;


        private void SetDataObjects(bool set)
        {
            foreach (Transform child in DataObjects.transform)
            {
                XRGrabInteractable xrgi = child.gameObject.GetComponent<XRGrabInteractable>();
                if (xrgi != null)
                {
                    xrgi.enabled = set;
                }
            }
        }


        private void Awake()
        {

            if (_Instance == null)
            {
                _Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }
        void OnValidate()
        {

            // Platform: Headset VR Oculus / COMPLETELY VIRTUAL
            if (platform == Platform.VR)
            {
                platform = Platform.VR;
                SetVRMode();
            }
            // Platform: Mobile / AUGMENTED REALITY
            else if (platform == Platform.AR)
            {   
                platform = Platform.AR;
                SetARMode();
            }
            else if (platform == Platform.MR)
            {   
                platform = Platform.MR;
                SetMRMode();
            }
            // Platform: Editor COMPLETELY VIRTUAL
            else if (platform == Platform.Editor)
            {
                platform = Platform.Editor;
                SetDesktopMode();

            }
            //  Platform: Standalone Desktop COMPLETELY VIRTUAL
            else if (platform == Platform.Desktop)
            {
                platform = Platform.Desktop;
                SetDesktopMode();
            }
        }
        private void SetARMode()
        {
            //Set 2DUI, AR Camera and Environment
            UI.SetActive(true);
            layoutUIManager.SetWorldCanvas(false);
            ClientAvatar.SetActive(false);  
            SetCamera();
            ActivateEnvironment(false);
            SetDataObjects(false);
        }
        private void SetMRMode()
        {
            //Set 2DUI, AR Camera and Environment
            UI.SetActive(false);
            ClientAvatar.SetActive(false);  
            SetCamera();
            ActivateEnvironment(false);
            SetDataObjects(true);

        }
        private void SetVRMode()
        {
            //Set 3DUI, AR Camera and Environment
            /*            UI.SetActive(false);
                        layoutUIManager.SetWorldCanvas(true);*/
            UI.SetActive(false);
            ClientAvatar.SetActive(false);
            SetCamera();
            ActivateEnvironment(true);
            SetDataObjects(true);

        }
        private void SetDesktopMode()
        {
            //Set 2DUI, Desktop Camera, Environment and avatar
            UI.SetActive(true);
            layoutUIManager.SetWorldCanvas(false);
            ClientAvatar.SetActive(true);
            SetCamera();
            ActivateEnvironment(true);
            SetDataObjects(false);
        }


        private void ActivateEnvironment(bool activate)
        {
            Environment.SetActive(activate);
        }

        public void ToggleVRMR(Toggle toggle)
        {

            if(toggle.isOn)
            {
                platform = Platform.VR;
                SetVRMode();
            }
            else
            {
                platform = Platform.MR;;
                SetMRMode();
            }
        }
        private void SetCamera()
        {
            switch (platform)
            {
                case Platform.AR:
                    AR_CameraRig.SetActive(true);
                    Desktop_CameraRig.SetActive(false);
                    MR_CameraRig.SetActive(false);
                    break;
                case Platform.MR:
                    AR_CameraRig.SetActive(false);
                    MR_CameraRig.SetActive(true);
                    MR_Camera.clearFlags = CameraClearFlags.SolidColor;
                    MR_Camera.backgroundColor = TransparentColor;
                    arCameraManager.enabled = true;
                    Desktop_CameraRig.SetActive(false);
                    break;
                case Platform.VR:
                    AR_CameraRig.SetActive(false);
                    MR_CameraRig.SetActive(true);
                    MR_Camera.clearFlags = CameraClearFlags.Skybox;
                    arCameraManager.enabled = false;
                    Desktop_CameraRig.SetActive(false);
                    break;
                case Platform.Desktop:
                    AR_CameraRig.SetActive(false);
                    MR_CameraRig.SetActive(false);
                    Desktop_CameraRig.SetActive(true);
                    break;
                case Platform.Editor:
                    AR_CameraRig.SetActive(false);
                    MR_CameraRig.SetActive(false);
                    Desktop_CameraRig.SetActive(true);
                    break;
            }
        }
      

    }
}

