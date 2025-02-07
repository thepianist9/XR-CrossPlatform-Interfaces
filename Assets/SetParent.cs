using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParent : MonoBehaviour
{
    public void SetParentNetwork()
    {
        GameObject parent = GameObject.FindGameObjectWithTag("Shared Space");
        GameObject child = GameObject.FindGameObjectWithTag("ARSpawnable");
        if (parent != null && child != null)
        {
            Debug.Log("Found network objects parent and child");
        }
        child.transform.SetParent(parent.transform);


    }
}
