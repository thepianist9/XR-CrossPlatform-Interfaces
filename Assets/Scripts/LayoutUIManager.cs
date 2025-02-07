
using StarterAssets;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using XRSpatiotemopralAuthoring;
using Image = UnityEngine.UI.Image;

namespace Assets.David.UILayout
{
    public class LayoutUIManager : MonoBehaviour
    {
        [Header("UI Layout Mode")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool worldCanvasMode = false;
        [SerializeField] private bool fullScreenMode = false;
        [SerializeField] private bool modalMode = false;

        [Header("Layout Configuration")]
        [SerializeField] private bool removeHeader = false;
        [SerializeField] private bool removeFooter = false;
        [SerializeField] private bool removeLeftBar = false;
        [SerializeField] private bool removeRightBar = false;

        [Header("ExpertStudy Task Objects 2d")]
        [SerializeField] private GameObject Modal;
        [SerializeField] private TMP_Text header;
        [SerializeField] private TMP_Text DescriptionBody;
        [SerializeField] private GameObject StepIndicator;
        [Header("ExpertStudy Task Objects 3d")]
        [SerializeField] private GameObject Modal3D;
        [SerializeField] private TMP_Text header3D;
        [SerializeField] private TMP_Text DescriptionBody3D;
        [SerializeField] private GameObject StepIndicator3D;

        [SerializeField] private GameObject m_MenuControlObject;
        [SerializeField] private GameObject m_MainMenu;

        [Header("World Canvas Settings")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject UILayoutBG;
        [SerializeField] private Vector3 canvasPosition = Vector3.zero;
        [SerializeField] private Vector3 canvasScale = new Vector3(0.01f, 0.01f, 0.01f);

        [SerializeField] private PlayerInput playerInput;

        private Dictionary<int, string> taskDescriptions = new Dictionary<int, string>
        {
            { 1, "Identify the least popular exhibit and remove it from the museum." },
            { 2, "Fit the new exhibit Guimbala Warrior with a volume of 100 units into an exhibit space with the appropriate area." },
            { 3, "Check the exhibit with the condition bad and relocate the one next to the window away from the sunlight." }
        };


        [Header("Debug Objects")]
        [SerializeField] private List<GameObject> layoutContainerObjects;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnValidate()
        {


            //World Canvas Mode
            SetWorldCanvas(worldCanvasMode);




        }
        public void SwitchUI()
        {
            m_MainMenu.SetActive(!m_MainMenu.activeSelf);
            m_MenuControlObject.SetActive(!m_MenuControlObject.activeSelf);
            if(m_MainMenu.activeSelf)
            {
                playerInput.DeactivateInput();
            }
            else
            {
                playerInput.ActivateInput();
            }
        }

/*        private void SetDebugMode(bool mode)
        {
            foreach (GameObject layoutContainerObject in layoutContainerObjects)
            {
                if (layoutContainerObject is not null)
                {

                    layoutContainerObject.GetComponent<Image>().enabled = mode;
                }
            }
        }

        public void SetModalMode(bool modalMode)
        {
       
            Debug.Log("Modal Mode: " + modalMode);
            
            Modal.SetActive(modalMode);
            UILayoutBG.SetActive(modalMode);


            
        }*/

        public void DisplayTask(int taskNumber)
        {
            if(PlatformManager.Instance.platform == PlatformManager.Platform.Desktop || PlatformManager.Instance.platform == PlatformManager.Platform.Editor || PlatformManager.Instance.platform == PlatformManager.Platform.AR)
            {
                Modal.SetActive(true);
                header.text = "Task " + taskNumber;
                DescriptionBody.text = taskDescriptions[taskNumber];
                //Set step indicator
                for (int i = 0; i < StepIndicator.transform.childCount; i++)
                {
                    StepIndicator.transform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = (taskNumber - 1 == i);
                }
            }
            else
            {
                //Display VR Menu Description
                Modal3D.SetActive(true);
                header3D.text = "Task " + taskNumber;
                DescriptionBody3D.text = taskDescriptions[taskNumber];
                //Set step indicator
                for (int i = 0; i < StepIndicator3D.transform.childCount; i++)
                {
                    StepIndicator3D.transform.GetChild(i).GetChild(0).gameObject.SetActive(taskNumber - 1 == i);
                }

            }


        }

        public void SetWorldCanvas(bool mode)
        {
            if (canvas is not null)
            {
                if (mode)
                {
                    canvas.renderMode = RenderMode.WorldSpace;
                    canvas.transform.position = canvasPosition;
                    canvas.transform.localScale = canvasScale;
                    canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 1080);
                }
                else
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }
            else
            {
                Debug.Log("Canvas is not set");
            }

        }

/*        private void EditLayoutConfig()
        {
            if (header is null || footer is null || leftDrawer is null || rightDrawer is null)
            {
                Debug.Log("Layout Objects are not set");
                return;
            }
            else
            {
                if (fullScreenMode)
                {
                    header.SetActive(false);
                    footer.SetActive(false);
                    leftDrawer.SetActive(false);
                    rightDrawer.SetActive(false);

                }
                else
                {
                    header.SetActive(!removeHeader);
                    footer.SetActive(!removeFooter);
                    leftDrawer.SetActive(!removeLeftBar);
                    rightDrawer.SetActive(!removeRightBar);
                }

            }
        }*/

/*        public void SetUILayout(bool removeHeader, bool removeFooter, bool removeLeftBar, bool removeRightBar)
        {
            this.removeHeader = removeHeader;
            this.removeFooter = removeFooter;
            this.removeLeftBar = removeLeftBar;
            this.removeRightBar = removeRightBar;

            EditLayoutConfig();
        }*/
/*
        public void SetFullScreen()
        {
            fullScreenMode = !fullScreenMode;
            EditLayoutConfig();
        }*/

    }
}
