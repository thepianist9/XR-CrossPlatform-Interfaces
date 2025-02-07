using System;
using System.Collections.Generic;
using IATK;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.David.UILayout;
using Unity.Netcode;

namespace XRSpatiotemopralAuthoring
{
    public class GraphManager : MonoBehaviour
    {
        private DataManager dataInstance;
        public Pose m_GraphPose;
        [SerializeField] private Vector3 GraphCenterOffset;

        private static GraphManager _Instance;
        [SerializeField] private TPPCameraSwitcher tPPCameraSwitcher;
        public static GraphManager Instance { get { return _Instance; } }

        [SerializeField] private TMP_Dropdown graphLabelNumberDropDown;
        [SerializeField] private GameObject dataStatusOutline;
        [SerializeField] private TMP_FontAsset legendFont;


        [SerializeField] private GameObject GraphControl;
        [SerializeField] private TMP_Text GraphControlPoperty;
        [SerializeField] private TMP_Text GraphControlMinText;
        [SerializeField] private TMP_Text GraphControlMaxText;
        [SerializeField] private Slider GraphControlSliderMin;
        [SerializeField] private Slider GraphControlSliderMax;


        [SerializeField] private GameObject graphDataLabeDropdownGOX;
        [SerializeField] private GameObject graphDataLabelDropdownGOY;
        [SerializeField] private GameObject graphDataLabeDropdownGOZ;

        [SerializeField] private Transform GraphParent1;

        private GameObject visualisationGO = null;

        [SerializeField] private BrushingAndLinking brushingAndLinking;

        private TMP_Dropdown dropDown = null;
        private Slider[] sliders;


        [SerializeField] private LayoutUIManager layoutUIManager;


        //Graph Control Panel
        [SerializeField] private Transform CanvasTransform;
        [SerializeField] private GameObject GraphControlPanelPrefab;


        [SerializeField] private GameObject visualisationObject;

        public Dictionary<Toggle, Visualisation> visualisations = new Dictionary<Toggle, Visualisation>();

        //AR
        private TMP_Dropdown graphDataLabeDropdownX;
        private TMP_Dropdown graphDataLabeDropdownY;
        private TMP_Dropdown graphDataLabeDropdownZ;
        internal GraphsControlManager currentGraphControlManager;

        private List<string> propertyNames = new List<string>();

        private Dictionary<Toggle, GameObject> togglePanelDictionary = new Dictionary<Toggle, GameObject>();

        public CSVDataSource myCSVDataSource;
        public bool isDataReady = false;

        private List<TMP_Dropdown> graphLabelDataDropdowns = new List<TMP_Dropdown>();
        private int graphAxisNumber = 0;
        private int graphNumber = 0;

        private void Awake()
        {
            if (_Instance == null)
            {
                _Instance = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log($"GraphNumber: {graphNumber}");
      

            //Set UI
            graphDataLabeDropdownX = graphDataLabeDropdownGOX.GetComponentInChildren<TMP_Dropdown>();
            graphDataLabeDropdownY = graphDataLabelDropdownGOY.GetComponentInChildren<TMP_Dropdown>();
            graphDataLabeDropdownZ = graphDataLabeDropdownGOZ.GetComponentInChildren<TMP_Dropdown>();

            graphLabelDataDropdowns.Add(graphDataLabeDropdownX);
            graphLabelDataDropdowns.Add(graphDataLabeDropdownY);
            graphLabelDataDropdowns.Add(graphDataLabeDropdownZ);
        

            dataInstance = DataManager.Instance;
       

        }


        public void setGraphLabels()
        {
            if(dataInstance == null)
                dataInstance = DataManager.Instance;
            if (dataInstance._MuseumComponents != null)
            {

                //convert and get csv data
                dataInstance.ConvertCSV(dataInstance._MuseumComponents);
                // Get the type of ConstructionBuilding
                Type buildingType = typeof(MuseumExhibit);

                // Get all properties of ConstructionBuilding
                buildingType.GetProperties().ForEach((prop => propertyNames.Add(prop.Name)));

                foreach (var dropDown in graphLabelDataDropdowns)
                {
                    dropDown.ClearOptions();
                    dropDown.AddOptions(propertyNames);
                }
                Debug.Log("[GraphManager]: Graph Labels Set...");
            }
        }

        public void SetDataSource()
        {
            if(gameObject.GetComponent<CSVDataSource>() == null)
            {
                myCSVDataSource = gameObject.AddComponent<CSVDataSource>();
                string result = dataInstance.ConvertCSV();
                myCSVDataSource.load(result, null);

                isDataReady = true;
                Debug.Log("[GraphManager]: Data source set for visualization");

                dataStatusOutline.GetComponent<Image>().color = Color.cyan;
                
            }
            
        }

/*        public void SetGraphAxisNumber()
        {
            Debug.Log("set axis called");
            switch (graphLabelNumberDropDown.options[graphLabelNumberDropDown.value].text)
            {
                case "Single Axis":
                    graphDataLabeDropdownGOX.SetActive(true);
                    graphDataLabelDropdownGOY.SetActive(false);
                    graphDataLabeDropdownGOZ.SetActive(false);
                    graphAxisNumber = 1;
                    break;
                case "Double Axis":
                    graphDataLabeDropdownGOX.SetActive(true);
                    graphDataLabelDropdownGOY.SetActive(true);
                    graphDataLabeDropdownGOZ.SetActive(false);
                    graphAxisNumber = 2;
                    break;
                case "Triple Axis":
                    graphDataLabeDropdownGOX.SetActive(true);
                    graphDataLabelDropdownGOY.SetActive(true);
                    graphDataLabeDropdownGOZ.SetActive(true);
                    graphAxisNumber = 3;
                    break;

            }
        }*/


        public void SetGraph()
        {
            switch (graphLabelNumberDropDown.options[graphLabelNumberDropDown.value].text)
            {
                case "Graph 1":
                    LoadGraph("Graph 1");
                    break;
                case "Graph 2":
                    LoadGraph("Graph 2");
                    break;
                case "Graph 3":
                    LoadGraph("Graph 3");
                    break;
                default:
                    Debug.Log($"[Graph Manager]: not a valid template selected");
                    break;

            }
        }

        public void SetGraph(string task)
        {
            switch (task)
            {
                case "task 1":
                    LoadGraph("Graph 1");
                    break;
                case "task 2":
                    LoadGraph("Graph 2");
                    break;
                case "task 3":
                    LoadGraph("Graph 3");
                    break;
                default:
                    Debug.Log($"[Graph Manager]: not a valid template selected");
                    break;

            }
        }








        public void LoadGraph(string NoOfAxis)
        {

            if(isDataReady)
            {
                ResetSliders();

                CSVDataSource csv = GetComponent<CSVDataSource>();

                GraphParent1.gameObject.SetActive(true);

                GameObject vGO = GraphParent1.transform.GetChild(0).GetChild(0).gameObject;

                Visualisation v = vGO.GetComponent<Visualisation>();


                switch (NoOfAxis)
                {

                    case "Graph 1":
                        

                        v.dataSource = csv;
                        v.geometry = AbstractVisualisation.GeometryType.Points;
                        v.xDimension.Attribute = "ExhibitID";
                        v.yDimension.Attribute = "ExhibitName";
                        v.zDimension.Attribute = "PopularityScore";
                        v.colourDimension = "Undefined";
                        v.colorPaletteDimension = "Undefined";
                        v.colour = Color.green;
                        v.size = 0.4f;

                        v.CreateVisualisation(AbstractVisualisation.VisualisationTypes.SCATTERPLOT);
                        v.updateProperties();
                        UpdateGraph3D(vGO, v, "z", "Popularity Score", "Task 1");

                        
                        brushingAndLinking.brushingVisualisations = new List<Visualisation> { v };
                        brushingAndLinking.isBrushing = true;

                        if (!NetworkManager.Singleton.IsHost)
                        {
                            layoutUIManager.DisplayTask(1);
                        }

                        break;
                    case "Graph 2":

                        v.dataSource = csv;
                        v.geometry = AbstractVisualisation.GeometryType.Points;
                        v.xDimension.Attribute = "DisplayArea";
                        v.yDimension.Attribute = "ExhibitName";
                        v.zDimension.Attribute = "YearAcquired";
                        v.colourDimension = "Undefined";
                        v.colorPaletteDimension = "Undefined";
                        v.colour = Color.green;
                        v.size = 0.4f;
                         
                        v.updateProperties();
                        UpdateGraph3D(vGO, v, "x", "Display Area", "Task 2");

                        brushingAndLinking.brushingVisualisations = new List<Visualisation> { v };
                        brushingAndLinking.isBrushing = true;
                        if (!NetworkManager.Singleton.IsHost)
                        {
                            layoutUIManager.DisplayTask(2);
                        }

                        break;
                    case "Graph 3":

                        v.dataSource = csv;
                        v.geometry = AbstractVisualisation.GeometryType.Points;
                        v.xDimension.Attribute = "ExhibitID";
                        v.yDimension.Attribute = "ConditionStatus";
                        v.zDimension.Attribute = "YearCreated";
                        v.colourDimension = "Undefined";
                        v.colorPaletteDimension = "Undefined";
                        v.colour = Color.green;
                        v.size = 0.4f;
                         
                        v.updateProperties();
                        UpdateGraph3D(vGO, v, "y", "Condition Status", "Task 3");
                        brushingAndLinking.brushingVisualisations = new List<Visualisation> { v };
                        brushingAndLinking.isBrushing = true; 

                        if (!NetworkManager.Singleton.IsHost)
                        {
                            layoutUIManager.DisplayTask(3);
                        }



                        break;

                    default:
                        Debug.Log($"[Graph Manager]: Load Graph failed");
                        break;
                }


                




            }
        }



        private void ResetSliders()
        {
            brushingAndLinking.isBrushing = false;
            GraphControlSliderMin.value = 0; GraphControlSliderMax.value = 1;

            GraphControlSliderMin.onValueChanged.RemoveAllListeners();
            GraphControlSliderMax.onValueChanged.RemoveAllListeners();


            brushingAndLinking.brushingVisualisations.Clear();

        }
        private void UpdateGraph3D(GameObject visualizationObject, Visualisation v, string axis, string GraphControlProperty, string task)
        {
            string attribute = "";
            GameObject control = null;
            foreach (Transform tr in visualizationObject.transform)
            {
                tr.localScale = new Vector3(1, 1, 1);
            }



            DataSource dataSource = v.dataSource;
            GraphControl.SetActive(true);
            GraphControlPoperty.text = GraphControlProperty;
            if (axis == "x")
            {
                SetMinMaxText(GraphControlSliderMin.value, "Min", dataSource, v.xDimension.Attribute);
                SetMinMaxText(GraphControlSliderMax.value, "Max", dataSource, v.xDimension.Attribute);
                GraphControlSliderMin.onValueChanged.AddListener(delegate { v.xDimension.minFilter = GraphControlSliderMin.value; v.updateProperties(); SetMinMaxText(GraphControlSliderMin.value, "Min", dataSource, v.xDimension.Attribute); });
                GraphControlSliderMax.onValueChanged.AddListener(delegate { v.xDimension.maxFilter = GraphControlSliderMax.value; v.updateProperties(); SetMinMaxText(GraphControlSliderMax.value, "Max", dataSource, v.xDimension.Attribute); });
            }                                                                                                                                                                                         
            else if(axis == "y")                                                                                                                                                                      
            {
                SetMinMaxText(GraphControlSliderMin.value, "Min", dataSource, v.yDimension.Attribute);
                SetMinMaxText(GraphControlSliderMax.value, "Max", dataSource, v.yDimension.Attribute);
                GraphControlSliderMin.onValueChanged.AddListener(delegate { v.yDimension.minFilter = GraphControlSliderMin.value; v.updateProperties(); SetMinMaxText(GraphControlSliderMin.value, "Min", dataSource, v.yDimension.Attribute); });
                GraphControlSliderMax.onValueChanged.AddListener(delegate { v.yDimension.maxFilter = GraphControlSliderMax.value; v.updateProperties(); SetMinMaxText(GraphControlSliderMax.value, "Max", dataSource, v.yDimension.Attribute); });
            }                                                                                                                                                                                           
            else if(axis == "z")                                                                                                                                                                        
            {
                SetMinMaxText(GraphControlSliderMin.value, "Min", dataSource, v.zDimension.Attribute);
                SetMinMaxText(GraphControlSliderMax.value, "Max", dataSource, v.zDimension.Attribute);
                GraphControlSliderMin.onValueChanged.AddListener(delegate { v.zDimension.minFilter = GraphControlSliderMin.value; v.updateProperties(); SetMinMaxText(GraphControlSliderMin.value, "Min", dataSource, v.zDimension.Attribute); });
                GraphControlSliderMax.onValueChanged.AddListener(delegate { v.zDimension.maxFilter = GraphControlSliderMax.value; v.updateProperties(); SetMinMaxText(GraphControlSliderMax.value, "Max", dataSource, v.zDimension.Attribute); });
            }
           




          
        }
        private void OnApplicationQuit()
        {
            ResetSliders();
        }

        private void SetMinMaxText(float normalizedValue, string minmax, DataSource datasource, string attribute)
        {
            string unnormalizedValue = datasource.getOriginalValue(normalizedValue, attribute).ToString();
            if(minmax == "Min")
            {
                GraphControlMinText.text = unnormalizedValue;
            }
            else
            {
                GraphControlMaxText.text = unnormalizedValue;
            }
        }


    }  
}

