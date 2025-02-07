using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace XRSpatiotemopralAuthoring
{
    public class DataManager : MonoBehaviour
    {
        private static DataManager _Instance;
        public static DataManager Instance { get { return _Instance; } }

        private DBNetworkingManager _dbNetworkingManager;
        [SerializeField] private Image _UIBorder;
        [SerializeField] private Image _UIBorderAR;
        [SerializeField] private TMP_Dropdown _projectDropdown;

        private List<Project> _projectList;
        private List<string> _projectNames;
        public List<MuseumExhibit> _MuseumComponents { private set; get; }

        public string dataFileCSV { private set; get; }

        void Awake()
        {
            if (_Instance == null)
            {
                _Instance = this;
            }
            DontDestroyOnLoad(this.gameObject);
        }

        public void Start()
        {
            _dbNetworkingManager = DBNetworkingManager.Instance;
            _projectNames = new List<string>();
        }

        // Start is called before the first frame update
        public void StartConnecting()
        {
            try
            {
                _dbNetworkingManager.Connect();
                //Get Project List
                _projectList = _dbNetworkingManager.RetrieveProjectCollection("Projects");
                if (_projectList != null)
                {
                    Debug.Log("[DataManager]: Retrieve project list successful");
                    _projectNames.Add("Select Project");

                    foreach (Project project in _projectList)
                    {
                        _projectNames.Add(project.ProjectName);
                    }
                    _projectDropdown.AddOptions(_projectNames);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

        }

        

        public void RetrieveProjectData()
        {
            try
            {
                //string projectName = _projectDropdown.options[_projectDropdown.value].text;

                _MuseumComponents = _dbNetworkingManager.RetrieveCollection("VRDemoRoom");
                if (_MuseumComponents != null)
                {
                    ConvertCSV(_MuseumComponents);
                    _UIBorder.color = Color.green;
                    Debug.Log("[DataManager]: Retrieve project data successful");
                }
                else
                {
                    Debug.Log("[DataManager]: Retrieve project data failed");
                    _UIBorder.color = Color.red;
                }


            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }


        public void ConvertCSV(List<MuseumExhibit> list)
        {
            try
            {
                // Construct CSV string
                dataFileCSV = "ExhibitID,ExhibitName,ConditionStatus,PopularityScore,YearCreated,YearAcquired,DisplayArea,Location,Category\n"; // Header
                foreach (MuseumExhibit obj in list)
                {
                    dataFileCSV += $"{obj.ExhibitID},{obj.ExhibitName},{obj.ConditionStatus},{obj.PopularityScore},{obj.YearCreated},{obj.YearAcquired},{obj.DisplayArea},{obj.Location},{obj.Category}\n"; // Data rows

                }
                Debug.Log("[DataManager]: Convert to CSV successful");
                //2d<0>3d<1>

            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _UIBorder.color = Color.red;
            }



        }
        public string ConvertCSV()
        {
            if (_MuseumComponents == null)
            {
                _MuseumComponents = _dbNetworkingManager.RetrieveCollection("VRDemoRoom");
            }



            try
            {
                // Construct CSV string
                dataFileCSV = "ExhibitID,ExhibitName,ConditionStatus,PopularityScore,YearCreated,YearAcquired,DisplayArea,Location,Category\n"; // Header
                foreach (MuseumExhibit obj in _MuseumComponents)
                {
                    dataFileCSV += $"{obj.ExhibitID},{obj.ExhibitName},{obj.ConditionStatus},{obj.PopularityScore},{obj.YearCreated},{obj.YearAcquired},{obj.DisplayArea},{obj.Location},{obj.Category}\n"; // Data rows

                }
                Debug.Log("[DataManager]: Convert to CSV successful");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            return dataFileCSV;

        }

    }
        
}

