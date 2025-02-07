using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XRSpatiotemopralAuthoring;

public class ExperimentManager : MonoBehaviour
{
    // Start is called before the first frame update
    public void Setup()
    {
        //set data
        DataSetup();
        GraphSetup();
    }

    private void DataSetup()
    {
        DataManager.Instance.StartConnecting();
        DataManager.Instance.RetrieveProjectData();
    }

    private void GraphSetup()
    {
        GraphManager.Instance.SetDataSource();
        GraphManager.Instance.setGraphLabels();

    }
}
