using IATK;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using System.Net;
using System.Collections;
using TMPro;
using XRSpatiotemopralAuthoring;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BrushingAndLinking : MonoBehaviour {

    [SerializeField]
    public ComputeShader computeShader;
    [SerializeField]
    public Material myRenderMaterial;
    [SerializeField] public Material LineRenderMaterial;

    public Vector3 BrushedPosition;
    public bool Brushed;

[SerializeField]
    public List<Visualisation> brushingVisualisations;
    [SerializeField]
    public List<LinkingVisualisations> brushedLinkingVisualisations;
    [SerializeField] private GameObject DataObjects;
    [SerializeField] private GameObject DataCube;

    [SerializeField]
    public bool isBrushing;
    [SerializeField]
    public Color brushColor = Color.red;
    [SerializeField]
    InputActionReference TriggerActionReference; // Action for spawn
    [SerializeField] private XRRayInteractor _rightRayInteractor;
    [SerializeField]
    [Range(0f, 1f)]
    public float brushRadius;
    [SerializeField]
    public bool showBrush = false;

    [SerializeField] private TMP_Text debugText;
    [SerializeField]
    [Range (1f,10f)]
    public float brushSizeFactor = 1f;

    [SerializeField]
    public Transform input1;
    [SerializeField]
    public Transform input2;

    [SerializeField]
    public BrushType BRUSH_TYPE;
    public enum BrushType
    {
        SPHERE = 0,
        BOX,
        RAYCAST

    };
    private Vector3 projectedPointer1;

    [SerializeField]
    public SelectionType SELECTION_TYPE;
    public enum SelectionType
    {
        FREE = 0,
        ADD,
        SUBTRACT
    }

    [SerializeField]
    public List<int> brushedIndices;

    [SerializeField]
    public Material debugObjectTexture;

    private int kernelComputeBrushTexture;
    private int kernelComputeBrushedIndices;

    private RenderTexture brushedIndicesTexture;
    private int texSize;

    private ComputeBuffer dataBuffer;
    private ComputeBuffer filteredIndicesBuffer;
    private ComputeBuffer brushedIndicesBuffer;

    private bool hasInitialised = false;
    private bool hasFreeBrushReset = false;
    private AsyncGPUReadbackRequest brushedIndicesRequest;
    int layerMask = 1 << 6;

    string[] dataObjectNames = new string[] { "T-Rex", "Armour Helmet", "Typewriter", "Nefertitis Bust", "Revolver .38", "Guimbala Warrior", "Mortar", "The Sword of Victory", "Laughing Buddha", "Dino Skull" };
    private void Start()
    {
        InitialiseShaders();
    }

    /// <summary>
    /// Initialises the indices for the kernels in the compute shader.
    /// </summary>
    private void InitialiseShaders()
    {
        kernelComputeBrushTexture = computeShader.FindKernel("CSMain");
        kernelComputeBrushedIndices = computeShader.FindKernel("ComputeBrushedIndicesArray");
    }
    
    /// <summary>
    /// Initialises the buffers and textures necessary for the brushing and linking to work.
    /// </summary>
    /// <param name="dataCount"></param>
    private void InitialiseBuffersAndTextures(int dataCount)
    {
        dataBuffer = new ComputeBuffer(dataCount, 12);
        dataBuffer.SetData(new Vector3[dataCount]);
        computeShader.SetBuffer(kernelComputeBrushTexture, "dataBuffer", dataBuffer);

        filteredIndicesBuffer = new ComputeBuffer(dataCount, 4);
        filteredIndicesBuffer.SetData(new float[dataCount]);
        computeShader.SetBuffer(kernelComputeBrushTexture, "filteredIndicesBuffer", filteredIndicesBuffer);

        brushedIndicesBuffer = new ComputeBuffer(dataCount, 4);
        brushedIndicesBuffer.SetData(Enumerable.Repeat(-1, dataCount).ToArray());
        computeShader.SetBuffer(kernelComputeBrushedIndices, "brushedIndicesBuffer", brushedIndicesBuffer);

        texSize = NextPowerOf2((int)Mathf.Sqrt(dataCount));
        brushedIndicesTexture = new RenderTexture(texSize, texSize, 24);
        brushedIndicesTexture.enableRandomWrite = true;
        brushedIndicesTexture.filterMode = FilterMode.Point;
        brushedIndicesTexture.Create();

        myRenderMaterial.SetTexture("_MainTex", brushedIndicesTexture);

        computeShader.SetFloat("_size", texSize);
        computeShader.SetTexture(kernelComputeBrushTexture, "Result", brushedIndicesTexture);
        computeShader.SetTexture(kernelComputeBrushedIndices, "Result", brushedIndicesTexture);

        hasInitialised = true;
    }

    /// <summary>
    /// Updates the computebuffers with the values specific to the currently brushed visualisation.
    /// </summary>
    /// <param name="visualisation"></param>
    public void UpdateComputeBuffers(Visualisation visualisation)
    {
        if (visualisation.visualisationType == AbstractVisualisation.VisualisationTypes.SCATTERPLOT)
        {
            dataBuffer.SetData(visualisation.theVisualizationObject.viewList[0].BigMesh.getBigMeshVertices());
            computeShader.SetBuffer(kernelComputeBrushTexture, "dataBuffer", dataBuffer);

            filteredIndicesBuffer.SetData(visualisation.theVisualizationObject.viewList[0].GetFilterChannel());
            computeShader.SetBuffer(kernelComputeBrushTexture, "filteredIndicesBuffer", filteredIndicesBuffer);
        }
    }


    /// <summary>
    /// Finds the next power of 2 for a given number.
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    private int NextPowerOf2(int number)
    {
        int pos = 0;

        while (number > 0)
        {
            pos++;
            number = number >> 1;
        }
        return (int)Mathf.Pow(2, pos);
    }

    public void Update()
    {

        if (isBrushing && brushingVisualisations.Count > 0)
        {
            if (hasInitialised)
            {
                UpdateBrushTexture();

                UpdateBrushedIndices();

                DisplayBrushedIndices();
            }
            else
            {
                InitialiseBuffersAndTextures(brushingVisualisations[0].dataSource.DataCount);
            }
        }
    }

   
    private int previousIndex = -1;

    private void DisplayBrushedIndices()
    {
        DataSource dataSource = brushingVisualisations[0].dataSource;

        for (int i = 0; i < brushedIndices.Count; i++)
        {

            if (i != previousIndex && (brushedIndices[i] == 1))
            {
                var data = dataSource
                            .Where(item => item.Identifier == "ExhibitName")
                            .Select(item =>
                            {
                                var data = new Dictionary<string, object>();
                                data["ExhibitName"] = dataSource.getOriginalValue(item.Data[i], "ExhibitName");
                                data["ConditionStatus"] = dataSource.getOriginalValue(item.Data[i], "ConditionStatus");
                                data["PopularityScore"] = dataSource.getOriginalValue(item.Data[i], "PopularityScore");
                                data["YearCreated"] = dataSource.getOriginalValue(item.Data[i], "YearCreated");
                                data["YearAcquired"] = dataSource.getOriginalValue(item.Data[i], "YearAcquired");
                                data["DisplayArea"] = dataSource.getOriginalValue(item.Data[i], "DisplayArea");
                                data["Location"] = dataSource.getOriginalValue(item.Data[i], "Location");
                                data["Category"] = dataSource.getOriginalValue(item.Data[i], "Category");
                                return data;
                            })
                            .ToList();
                if (data.Count > 0)
                {
                    Debug.Log("Brushed: " + data[0].ToString() + " " + i);
                    //Select object in Scene
                    SelectDataObject(data[0]["ExhibitName"].ToString());
                    DebugInfoData(data[0]);
                    previousIndex = i;
                }
            }
        }
    }

    private void DebugInfoData(Dictionary<string, object> data)
    {
        debugText.text = "ExhibitName: " + data["ExhibitName"].ToString() + "\n" +
                         "ConditionStatus: " + data["ConditionStatus"].ToString() + "\n" +
                         "PopularityScore: " + data["PopularityScore"].ToString() + "\n" +
                         "YearCreated: " + data["YearCreated"].ToString() + "\n" +
                         "YearAcquired: " + data["YearAcquired"].ToString() + "\n" +
                         "DisplayArea: " + data["DisplayArea"].ToString() + "\n" +
                         "Location: " + data["Location"].ToString() + "\n"+
                         "Category: " + data["Category"].ToString() + "\n";
    }   

    public void ResetBrushedVertices()
    {
        for(int i=0; i <brushedIndices.Count; i++)
        {
            brushedIndices[i] = -1;
        }
        UpdateBrushTexture();
        foreach(Transform tr in DataObjects.transform)
        {
            if (tr != null)
            {
                DataObject dataObject = tr.GetComponent<DataObject>();
                if (dataObject != null)
                {
                    dataObject.UnSelect();
                }
            }
        }
    }

    private void SelectDataObject(string name)
    {

        if (DataObjects != null)
        {
            Transform dataObjectTransform = DataObjects.transform.Find(name);
            dataObjectTransform.GetComponent<MeshRenderer>().enabled = true;
            Debug.Log(name);
            if (dataObjectTransform != null)
            {
                // Cache the DataObject component
                DataObject dataObject = dataObjectTransform.GetComponent<DataObject>();

                if (dataObject != null)
                {
                    // Toggle selection based on the current state
                    if (!dataObject.IsSelected)
                    {
                        dataObject.Select();
                        Debug.Log($"Dataobject found drawing line to: {name}");
                        // DRAW LINE
                        // Add and configure LineRenderer to draw a line
                        LineRenderer lineRenderer = dataObjectTransform.gameObject.AddComponent<LineRenderer>();
                        lineRenderer.startWidth = 0.001f;
                        lineRenderer.endWidth = 0.001f;
                        lineRenderer.positionCount = 2;
                        lineRenderer.material = LineRenderMaterial;

                        // Set line positions between startPoint and endPoint
                        lineRenderer.SetPosition(0, BrushedPosition);
                        lineRenderer.SetPosition(1, dataObjectTransform.position);

                        // Start coroutine to automatically remove the line after 5 seconds
                        StartCoroutine(RemoveLineAfterSeconds(lineRenderer, 10f));
                    }
                    else
                    {
                        dataObject.UnSelect();
                    }
                }
            }
        }
    }

    private IEnumerator RemoveLineAfterSeconds(LineRenderer lineRenderer, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Check if the LineRenderer is still attached before removing
        if (lineRenderer != null)
        {
            Destroy(lineRenderer);
        }
    }


    /// <summary>
    /// Returns a list with all indices - if index > 0, index is brushed. It's not otherwise
    /// </summary>
    /// <returns></returns>
    public List<int> GetBrushedIndices()
    {

            UpdateBrushedIndices();
            List<int> indicesBrushed = new List<int>();

            for (int i = 0; i < brushedIndices.Count; i++)
            {
                if (brushedIndices[i] > 0)
                    indicesBrushed.Add(i);
            }


        return indicesBrushed;
    }

    /// <summary>
    /// Updates the brushedIndicesTexture using the visualisations set in the brushingVisualisations list.
    /// </summary>
    private void UpdateBrushTexture()
    {

        Vector3 projectedPointer2;

        computeShader.SetInt("BrushMode", (int)BRUSH_TYPE);
        computeShader.SetInt("SelectionMode", (int)SELECTION_TYPE);

        hasFreeBrushReset = false;

        foreach (var vis in brushingVisualisations)
        {
            UpdateComputeBuffers(vis);
            // Add a new method to handle the raycast brush
            void HandleRaycastBrush(Visualisation vis)
            {
                if ((PlatformManager.Instance.platform == PlatformManager.Platform.MR) || (PlatformManager.Instance.platform == PlatformManager.Platform.VR))
                {
                    if (_rightRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                    {
                        BrushedPosition = hit.point;
                        projectedPointer1 = vis.transform.InverseTransformPoint(hit.point);
                        Debug.Log("Pointer1: " + projectedPointer1.x + " " + projectedPointer1.y + " " + projectedPointer1.z);
                        computeShader.SetFloats("pointer1", projectedPointer1.x, projectedPointer1.y, projectedPointer1.z);
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, 10f, layerMask))
                        {
                            BrushedPosition = hit.point;
                            int childIndex = hit.transform.GetSiblingIndex();
                            Debug.Log("Collided with child index: " + dataObjectNames[childIndex-1]);
                            SelectDataObject(dataObjectNames[childIndex - 1]);
/*                            projectedPointer1 = vis.transform.InverseTransformPoint(hit.point);
                            Debug.Log("Pointer1: " + projectedPointer1.x + " " + projectedPointer1.y + " " + projectedPointer1.z);
                            computeShader.SetFloats("pointer1", projectedPointer1.x, projectedPointer1.y, projectedPointer1.z);*/
                        }
                    }
                }

            }

            // Modify the existing switch statement to include the raycast brush case
            switch (BRUSH_TYPE)
            {
                case BrushType.SPHERE:
                    HandleRaycastBrush(vis);
                    /*projectedPointer1 = vis.transform.InverseTransformPoint(input1.transform.position);
                    Debug.Log("Pointer1: " + projectedPointer1.x + " " + projectedPointer1.y + " " + projectedPointer1.z);
                    computeShader.SetFloats("pointer1", projectedPointer1.x, projectedPointer1.y, projectedPointer1.z);*/
                    break;
                case BrushType.BOX:
                    projectedPointer1 = vis.transform.InverseTransformPoint(input1.transform.position);
                    projectedPointer2 = vis.transform.InverseTransformPoint(input2.transform.position);
                    computeShader.SetFloats("pointer1", projectedPointer1.x, projectedPointer1.y, projectedPointer1.z);
                    computeShader.SetFloats("pointer2", projectedPointer2.x, projectedPointer2.y, projectedPointer2.z);
                    break;
                case BrushType.RAYCAST:
                    HandleRaycastBrush(vis);
                    break;
                default:
                    break;
            }

            //set the filters and normalisation values of the brushing visualisation to the computer shader
            computeShader.SetFloat("_MinNormX", vis.xDimension.minScale);
            computeShader.SetFloat("_MaxNormX", vis.xDimension.maxScale);
            computeShader.SetFloat("_MinNormY", vis.yDimension.minScale);
            computeShader.SetFloat("_MaxNormY", vis.yDimension.maxScale);
            computeShader.SetFloat("_MinNormZ", vis.zDimension.minScale);
            computeShader.SetFloat("_MaxNormZ", vis.zDimension.maxScale);

            computeShader.SetFloat("_MinX", vis.xDimension.minFilter);
            computeShader.SetFloat("_MaxX", vis.xDimension.maxFilter);
            computeShader.SetFloat("_MinY", vis.yDimension.minFilter);
            computeShader.SetFloat("_MaxY", vis.yDimension.maxFilter);
            computeShader.SetFloat("_MinZ", vis.zDimension.minFilter);
            computeShader.SetFloat("_MaxZ", vis.zDimension.maxFilter);

            computeShader.SetFloat("RadiusSphere", brushRadius);

            computeShader.SetFloat("width", vis.width);
            computeShader.SetFloat("height", vis.height);
            computeShader.SetFloat("depth", vis.depth);

            // Tell the shader whether or not the visualisation's points have already been reset by a previous brush, required to allow for
            // multiple visualisations to be brushed with the free selection tool
            if (SELECTION_TYPE == SelectionType.FREE)
                computeShader.SetBool("HasFreeBrushReset", hasFreeBrushReset);

            // Run the compute shader
            computeShader.Dispatch(kernelComputeBrushTexture, Mathf.CeilToInt(texSize / 32f), Mathf.CeilToInt(texSize / 32f), 1);

            foreach (var view in vis.theVisualizationObject.viewList)
            {
                view.BigMesh.SharedMaterial.SetTexture("_BrushedTexture", brushedIndicesTexture);
                view.BigMesh.SharedMaterial.SetFloat("_DataWidth", texSize);
                view.BigMesh.SharedMaterial.SetFloat("_DataHeight", texSize);
                view.BigMesh.SharedMaterial.SetFloat("_ShowBrush", Convert.ToSingle(showBrush));
                view.BigMesh.SharedMaterial.SetColor("_BrushColor", brushColor);
            }
           
            hasFreeBrushReset = true;
        }
        if(brushedLinkingVisualisations.Count > 0)
        {
            foreach (var linkingVis in brushedLinkingVisualisations)
            {
                linkingVis.View.BigMesh.SharedMaterial.SetTexture("_BrushedTexture", brushedIndicesTexture);
                linkingVis.View.BigMesh.SharedMaterial.SetFloat("_DataWidth", texSize);
                linkingVis.View.BigMesh.SharedMaterial.SetFloat("_DataHeight", texSize);
                linkingVis.View.BigMesh.SharedMaterial.SetFloat("_ShowBrush", Convert.ToSingle(showBrush));
                linkingVis.View.BigMesh.SharedMaterial.SetColor("_BrushColor", brushColor);
            }
        }

    }

   


    /// <summary>
    /// Updates the brushedIndices list with the currently brushed indices. A value of 1 represents brushed, -1 represents not brushed (boolean values are not supported).
    /// </summary>
    private void UpdateBrushedIndices()
    {
        // Wait for request to finish
        if (brushedIndicesRequest.done)
        {
            // Get values from request
            if (!brushedIndicesRequest.hasError)
            {
                brushedIndices = brushedIndicesRequest.GetData<int>().ToList();
            }

            // Dispatch again
            computeShader.Dispatch(kernelComputeBrushedIndices, Mathf.CeilToInt(brushedIndicesBuffer.count / 32f), 1, 1);
            brushedIndicesRequest = AsyncGPUReadback.Request(brushedIndicesBuffer);
        }
    }

    /// <summary>
    /// Releases the buffers on the graphics card.
    /// </summary>
    private void OnDestroy()
    {
        if (dataBuffer != null)
            dataBuffer.Release();

        if (filteredIndicesBuffer != null)
            filteredIndicesBuffer.Release();

        if (brushedIndicesBuffer != null)
            brushedIndicesBuffer.Release();
    }

    private void OnApplicationQuit()
    {
        if (dataBuffer != null)
            dataBuffer.Release();

        if (filteredIndicesBuffer != null)
            filteredIndicesBuffer.Release();

        if (brushedIndicesBuffer != null)
            brushedIndicesBuffer.Release();
    }
}