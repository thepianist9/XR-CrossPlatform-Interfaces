using MongoDB.Bson;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DataObject : MonoBehaviour
{
    [SerializeField] private Material SelectedMaterial;
    private MeshRenderer meshRenderer;
    [SerializeField] public bool IsSelected;
    private GameObject dataInfoPanel;

    [SerializeField] private string id;
    [SerializeField] private string name;

    [SerializeField] private string milestone;
    [SerializeField] private int size;
    [SerializeField] private string type;
    [SerializeField] private string material;
    [SerializeField] private string location;
    [SerializeField] private double height;


    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Select()
    {
        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
        }
        else
        {
            Debug.LogError("MeshRenderer is null");
        }
    }

    public void UnSelect()
    {
        if (meshRenderer != null)
        {
           meshRenderer.enabled = false;
        }
        else
        {
            Debug.LogError("MeshRenderer is null");
        }
    }

    public void ToggleSelect(bool isSelected)
    {
        if (isSelected)
        {
            Select();
        }
        else
        {
            UnSelect();
        }
    }

    public void DisplayDataInfo()
    {
        dataInfoPanel.SetActive(true);
        //add info about the data object in the panel
        TMP_Text textContainer = dataInfoPanel.transform.GetChild(0).GetComponentInChildren<TMP_Text>();
        textContainer.text = $"ID: {id}\nName: {name}\nMilestone: {milestone}\nSize: {size}\nType: {type}\nMaterial: {material}\nLocation: {location}\nHeight: {height}";
    }
}
