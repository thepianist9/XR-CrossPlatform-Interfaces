using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SharedSpaceOnNetworkSpawn : NetworkBehaviour
{
    [SerializeField] private List<GameObject> m_ARObjects;

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (GameObject obj in m_ARObjects)
            {
                //Instantiate the object
                GameObject gameObject = Instantiate(obj);
                //Spawn Network object for corresponding object
                NetworkObject no = gameObject.GetComponent<NetworkObject>();

                //Set the parent of the object to the Shared Space


                no.Spawn();
                no.TrySetParent(transform);
                gameObject.transform.localPosition = new Vector3(0, 0, 0);
                
            }

        }
    }

}
