using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OnSpawn : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log("Variatn on spawn called");
        gameObject.transform.localPosition = new Vector3(0, 0, 0);
    }
}

