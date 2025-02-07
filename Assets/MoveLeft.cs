using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLeft : MonoBehaviour
{

    public void MoveLeftFunc()
    {
        GameObject.FindGameObjectWithTag("Shared Space").transform.position += new Vector3(0.05f, 0, 0);
    }
}
