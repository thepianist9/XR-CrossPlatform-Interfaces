using Assets.David.Scripts.DesignPatterns.StateMachine;
using RuntimeInspectorNamespace;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using XRSpatiotemopralAuthoring;
using static Assets.David.Scripts.DesignPatterns.StateMachine.ExperimentStateManager;

namespace Game.ServerAuthoritativeSynchronousSpawning
{
    /// <summary>
    /// A dynamic prefab loading use-case where the server instructs all clients to load a single network prefab, and
    /// will only invoke a spawn once all clients have successfully completed their respective loads of said prefab. The
    /// server will initially send a ClientRpc to all clients, begin loading the prefab on the server, will await
    /// acknowledgement of a load via ServerRpcs from each client, and will only spawn the prefab over the network once
    /// it has received an acknowledgement from every client, within m_SynchronousSpawnTimeoutTimer seconds.
    /// </summary>
    /// <remarks>
    /// This use-case is recommended for scenarios where you'd want to guarantee the same world version across all
    /// connected clients. Since the server will wait until all clients have loaded the same dynamic prefab, the spawn
    /// of said dynamic prefab will be synchronous. Thus, we recommend using this technique for spawning game-changing
    /// gameplay elements, assuming you'd want all clients to be able to interact with said gameplay elements from the
    /// same point forward. For example, you wouldn't want to have an enemy only be visible (network side and/or
    /// visually) to some clients and not others -- you'd want to delay the enemy's spawn until all clients have
    /// dynamically loaded it and are able to see it before spawning it server side.
    /// </remarks>
    /// 

    public sealed class XRServerAuthoritativeSynchronousSpawning : NetworkBehaviour
    {
        [SerializeField]
        NetworkManager m_NetworkManager;
        [SerializeField] private float offsetDistance;

        [SerializeField] List<AssetReferenceGameObject> m_DynamicPrefabReferences;
        const int k_MaxConnectedClientCount = 4;

        const int k_MaxConnectPayload = 1024;
        private GameObject SpawnedNetworkObject = null;

        float m_SynchronousSpawnTimeoutTimer;
        [SerializeField] private Transform PlatformButtonsGroup;

        int m_SynchronousSpawnAckCount = 0;
        [SerializeField] private GameObject NetworkExperimentObject;

        [SerializeField] List<GameObject> m_DynamicSpawnedPrefabs;
        [SerializeField] private Color m_AuthorityColor;
        [SerializeField] private Color m_NoAuthorityColor;
        [SerializeField] private Image UIBorder;
        [SerializeField] SelectTransformGizmoXR m_SelectTransformGizmo;
        [SerializeField] private GameObject m_messageBox;
        [SerializeField] private XRNetworkAppController m_AppController;
        [SerializeField] private ExperimentStateManager stateManager;
        [SerializeField] private GameObject TaskCompleted;
        [SerializeField] private GameObject TaskCompleted3D;
        [SerializeField] private TMP_Text TaskCompleted3DDescription;
        private HashSet<string> spawnedAssets = new HashSet<string>();
        [SerializeField] List<Vector3> OriginalPositions;

        private Vector3 cachedSharedSpacePosition;

        [SerializeField]private GameObject _networkSpace = null;
        [SerializeField] private GameObject _AuthorityStatus;



        void Start()
        {
            DynamicPrefabLoadingUtilities.Init(m_NetworkManager);

            // In the use-cases where connection approval is implemented, the server can begin to validate a user's
            // connection payload, and either approve or deny connection to the joining client.
            // Note: we will define a very simplistic connection approval below, which will effectively deny all
            // late-joining clients unless the server has not loaded any dynamic prefabs. You could choose to not define
            // a connection approval callback, but late-joining clients will have mismatching NetworkConfigs (and  
            // potentially different world versions if the server has spawned a dynamic prefab).
            m_NetworkManager.NetworkConfig.ConnectionApproval = true;

            // Here, we keep ForceSamePrefabs disabled. This will allow us to dynamically add network prefabs to Netcode
            // for GameObject after establishing a connection.
            m_NetworkManager.NetworkConfig.ForceSamePrefabs = false;

            // This is a simplistic use-case of a connection approval callback. To see how a connection approval should
            // be used to validate a user's connection payload, see the connection approval use-case, or the
            // APIPlayground, where all post-connection techniques are used in harmony.
            m_NetworkManager.ConnectionApprovalCallback += ConnectionApprovalCallback;

            if(OriginalPositions.Count == 0)
            {
                CacheLocalPositions();
            }


        }

        private void CacheLocalPositions()
        {

            foreach(Transform child in _networkSpace.transform)
            {
                OriginalPositions.Add(child.localPosition);

            }
            
        }

        public override void OnDestroy()
        {
            m_NetworkManager.ConnectionApprovalCallback -= ConnectionApprovalCallback;
            DynamicPrefabLoadingUtilities.UnloadAndReleaseAllDynamicPrefabs();
            base.OnDestroy();
        }

        void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            Debug.Log("Client is trying to connect " + request.ClientNetworkId);
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;

            if (clientId == m_NetworkManager.LocalClientId)
            {
                // allow the host to connect
                Approve();
                return;
            }

            // A sample-specific denial on clients after k_MaxConnectedClientCount clients have been connected
            if (m_NetworkManager.ConnectedClientsList.Count >= k_MaxConnectedClientCount)
            {
                ImmediateDeny();
                return;
            }

            if (connectionData.Length > k_MaxConnectPayload)
            {
                // If connectionData is too big, deny immediately to avoid wasting time on the server. This is intended as
                // a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
                ImmediateDeny();
                return;
            }

            // simple approval if the server has not loaded any dynamic prefabs yet
            if (DynamicPrefabLoadingUtilities.LoadedPrefabCount == 0)
            {
                Approve();
            }
            else
            {
                ImmediateDeny();
            }

            void Approve()
            {
                Debug.Log($"Client {clientId} approved");
                response.Approved = true;
                response.CreatePlayerObject = false; //we're not going to spawn a player object for this sample
            }

            void ImmediateDeny()
            {
                Debug.Log($"Client {clientId} denied connection");
                response.Approved = false;
                response.CreatePlayerObject = false;
            }
        }
        public void ShareAllVariantPrefabs()
        {
            //get private objects from variant and spawn them with index
            for (int i = 0; i <= 3; i++)
            {
                OnClickedTrySpawnSynchronously(i);
            }
        }
        private bool CheckIfSpawned(int prefabIndex)
        {
            return spawnedAssets.Contains(m_DynamicPrefabReferences[prefabIndex].AssetGUID);

        }

        // invoked by UI
        public void OnClickedTrySpawnSynchronously(int index)
        {

            if (!CheckIfSpawned(index))
            {
               
                //index = 0; variant = 1; calc index = 0, index = 0; variant = 2; calc index = 3, index = 0; variant = 3; calc index = 6
                
                if (index == m_DynamicPrefabReferences.Count)
                {
                    index = m_DynamicPrefabReferences.Count - 1;
                }

                Debug.Log($"AppController private space: {m_AppController.privateSpace}");

                if (!m_AppController.privateSpace)
                {
                    var position = _networkSpace.transform.GetChild(index).localPosition;
                    var rotation = _networkSpace.transform.GetChild(index).localRotation;

                    //calculate offset and send to all clients
                    if (!m_NetworkManager.IsServer)
                    {
                        TrySpawnServerRpc
                            (index, position, rotation);
                    }


                    TrySpawnSynchronously(index, position, rotation);
                    spawnedAssets.Add(m_DynamicPrefabReferences[index].AssetGUID);
                }
                else
                {
                    m_messageBox.SetActive(true);
                    m_messageBox.GetComponentInChildren<TMP_Text>().text = "You are in private space. Please switch to shared space to spawn objects";
                }
            }
            else
            {
                DisplayMessageBox("Object already spawned");
            }

        }


        [ServerRpc(RequireOwnership = false)]
        void TrySpawnServerRpc(int index, Vector3 position, Quaternion rotation)
        {
            Debug.Log("Runnning ServerRpc on Server");
            if (!IsServer)
            {
                return;
            }

            TrySpawnSynchronously(index, position, rotation);
        }
        public override void OnNetworkSpawn()
        {
            if(IsLocalPlayer)
            {
                UIBorder.color = Color.green;
            }

            spawnedAssets.Clear();
        }         
        public override void OnNetworkDespawn()
        {
            if(IsLocalPlayer)
            {
                UIBorder.color = Color.white;
            }

            spawnedAssets.Clear();
        }        



        public void ResetPositions()
        {

            //for client and server

            //get the variant child 
            //get the index of relvant position and set the position
            if(_networkSpace != null)
            {
                Transform tr = _networkSpace.transform;


                for(int i = 0; i < tr.childCount; i++)
                {
                    tr.GetChild(i).localPosition = OriginalPositions[i];
                    
                }

            }
            else
            {
                DisplayMessageBox("private space or shared space is null");
            }
        }



        



        async void TrySpawnSynchronously(int index, Vector3 position, Quaternion rotation)
        {
            var randomPrefab = m_DynamicPrefabReferences[index];
            await TrySpawnDynamicPrefabSynchronously(randomPrefab.AssetGUID, position, rotation);
        }




        /// <summary>
        /// This call attempts to spawn a prefab by it's addressable guid - it ensures that all the clients have loaded the prefab before spawning it,
        /// and if the clients fail to acknowledge that they've loaded a prefab - the spawn will fail.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        async Task<(bool Success, NetworkObject Obj)> TrySpawnDynamicPrefabSynchronously(string guid, Vector3 position, Quaternion rotation)
        {
            if (IsServer)
            {
                var assetGuid = new AddressableGUID()
                {
                    Value = guid
                };

                if (DynamicPrefabLoadingUtilities.IsPrefabLoadedOnAllClients(assetGuid))
                {
                    Debug.Log("Prefab is already loaded by all peers, we can spawn it immediately");
                    var obj = Spawn(assetGuid, position, rotation);
                    return (true, obj);
                }

                m_SynchronousSpawnAckCount = 0;
                m_SynchronousSpawnTimeoutTimer = 0;

                Debug.Log("Loading dynamic prefab on the clients...");
                LoadAddressableClientRpc(assetGuid);


                //load the prefab on the server, so that any late-joiner will need to load that prefab also
                await DynamicPrefabLoadingUtilities.LoadDynamicPrefab(assetGuid, 0);

                // server loaded a prefab, update UI with the loaded asset's name
                DynamicPrefabLoadingUtilities.TryGetLoadedGameObjectFromGuid(assetGuid, out var loadedGameObject);

                var requiredAcknowledgementsCount = IsHost ? m_NetworkManager.ConnectedClients.Count - 1 :
                    m_NetworkManager.ConnectedClients.Count;

                while (m_SynchronousSpawnTimeoutTimer < 3000)
                {
                    if (m_SynchronousSpawnAckCount >= requiredAcknowledgementsCount)
                    {
                        Debug.Log($"All clients have loaded the prefab in {m_SynchronousSpawnTimeoutTimer} seconds, spawning the prefab on the server...");
                        var obj = Spawn(assetGuid, position, rotation);
                        return (true, obj);
                    }

                    m_SynchronousSpawnTimeoutTimer += Time.deltaTime;
                    await Task.Yield();
                }

                // left to the reader: you'll need to be reactive to clients failing to load -- you should either have
                // the offending client try again or disconnect it after a predetermined amount of failed attempts
                Debug.LogError("Failed to spawn dynamic prefab - timeout");
                return (false, null);
            }

            return (false, null);

            NetworkObject Spawn(AddressableGUID assetGuid, Vector3 position, Quaternion rotation)
            {
                Transform variantParent = GameObject.FindGameObjectWithTag("Shared Space").transform;
               /* foreach (Transform tr in variantParent)
                {
                    if (tr.name == $"Variant {variant}(Clone)")
                    {
                        variantParent = tr;
                        break;
                    }
                }*/
                if (variantParent == null)
                {
                    DisplayMessageBox("Shared Space object not found");
                }
                if (!DynamicPrefabLoadingUtilities.TryGetLoadedGameObjectFromGuid(assetGuid, out var prefab))
                {
                    Debug.LogWarning($"GUID {assetGuid} is not a GUID of a previously loaded prefab. Failed to spawn a prefab.");
                    return null;
                }
                var obj = Instantiate(prefab.Result);

                var networkObj = obj.GetComponent<NetworkObject>();
                networkObj.Spawn();
                obj.transform.SetParent(variantParent.transform, false);
                obj.transform.localPosition = position;
                obj.transform.localRotation = rotation;

                m_DynamicSpawnedPrefabs.Add(obj);


                Debug.Log("Spawned dynamic prefab");

                // every client loaded dynamic prefab, their respective ClientUIs in case they loaded first

                return networkObj;
            }
        }

        public void DisplayMessageBox(string msg)
        {
            m_messageBox.SetActive(true);
            m_messageBox.GetComponentInChildren<TMP_Text>().text = msg;
        }




        [ClientRpc]
        void LoadAddressableClientRpc(AddressableGUID guid, ClientRpcParams rpcParams = default)
        {
            if (!IsHost)
            {
                Load(guid);
            }

            async void Load(AddressableGUID assetGuid)
            {
                Debug.Log("Loading dynamic prefab on the client...");
                await DynamicPrefabLoadingUtilities.LoadDynamicPrefab(assetGuid, 0);
                Debug.Log("Client loaded dynamic prefab");

                DynamicPrefabLoadingUtilities.TryGetLoadedGameObjectFromGuid(assetGuid, out var loadedGameObject);

                AcknowledgeSuccessfulPrefabLoadServerRpc(assetGuid.GetHashCode());
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void AcknowledgeSuccessfulPrefabLoadServerRpc(int prefabHash, ServerRpcParams rpcParams = default)
        {
            m_SynchronousSpawnAckCount++;
            Debug.Log($"Client acknowledged successful prefab load with hash: {prefabHash}");
            DynamicPrefabLoadingUtilities.RecordThatClientHasLoadedAPrefab(prefabHash,
                rpcParams.Receive.SenderClientId);

            // a quick way to grab a matching prefab reference's name via its prefabHash
            var loadedPrefabName = "Undefined";
            foreach (var prefabReference in m_DynamicPrefabReferences)
            {
                var prefabReferenceGuid = new AddressableGUID() { Value = prefabReference.AssetGUID };
                if (prefabReferenceGuid.GetHashCode() == prefabHash)
                {
                    // found the matching prefab reference
                    if (DynamicPrefabLoadingUtilities.LoadedDynamicPrefabResourceHandles.TryGetValue(
                            prefabReferenceGuid,
                            out var loadedGameObject))
                    {
                        // if it is loaded on the server, update the name on the ClientUI
                        loadedPrefabName = loadedGameObject.Result.name;
                    }
                    break;
                }
            }

        }


        public bool RequestOwnership(ulong clientId)
        {
            if (!IsServer && clientId != null)
            {
                Debug.Log("Requesting ownership from server");
                GrantOwnershipServerRPC(clientId);
                m_SelectTransformGizmo.ActivateEdit();
                return true;

            }
            else
                return false;

        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnObjectfromClientServerRpc(Vector3 position, Quaternion rotation)
        {



            // Instantiate the object and set its position and rotation
            SpawnedNetworkObject = Instantiate(NetworkExperimentObject, position, rotation);



/*            SpawnedNetworkObject = Instantiate(NetworkExperimentObject, Vector3.zero, rotation);*/
            NetworkObject no = SpawnedNetworkObject.GetComponent<NetworkObject>();
            no.Spawn();
            no.TrySetParent(_networkSpace, true);
            no.ChangeOwnership(_networkSpace.GetComponent<NetworkObject>().OwnerClientId);
        }


        public void RevokeOwnership()
        {
            if (IsServer)
            {
                foreach (var obj in m_DynamicSpawnedPrefabs)
                {
                    NetworkObject no = obj.GetComponent<NetworkObject>();
                    no.RemoveOwnership();
                }
                m_SelectTransformGizmo.ActivateEdit();
                if(_AuthorityStatus != null)
                {
                    _AuthorityStatus.GetComponent<MeshRenderer>().material.color = m_AuthorityColor;

                }
                
                DisableEditClientRPC();

            }

        }

        [ClientRpc]
        private void DisableEditClientRPC()
        {
            if (!IsServer)
            {
                m_SelectTransformGizmo.DeactivateEdit();
                if (_AuthorityStatus != null)
                {
                    _AuthorityStatus.GetComponent<MeshRenderer>().material.color = m_NoAuthorityColor;

                }
                
            }
            
        }

        [ServerRpc(RequireOwnership = false)]

        private void GrantOwnershipServerRPC(ulong clientId)
        {
            if (IsServer)
            {
                Debug.Log($"Granting ownership to client: {clientId}");

                _networkSpace.GetComponent<NetworkObject>().ChangeOwnership(clientId);
                foreach (Transform obj in _networkSpace.transform)
                {

                    NetworkObject no = obj.GetComponent<NetworkObject>();
                    if(no)
                        no.ChangeOwnership(clientId);

                }
                if(SpawnedNetworkObject != null)
                {
                    NetworkObject no = SpawnedNetworkObject.GetComponent<NetworkObject>();
                    no.ChangeOwnership(clientId);
                }
                AuthorityStatusClientRpc(clientId);
            }
        }

        [ClientRpc]
        private void AuthorityStatusClientRpc(ulong authoritativeCLientId)
        {
            if(NetworkManager.Singleton.LocalClientId == authoritativeCLientId)
            {
                if(_AuthorityStatus != null)
                {
                    _AuthorityStatus.GetComponent<MeshRenderer>().material.color = m_AuthorityColor;
                    Debug.Log("Setting Authority Status to green");

                }
                
            }
            else
            {
                if (_AuthorityStatus != null)
                {
                    _AuthorityStatus.GetComponent<MeshRenderer>().material.color = m_NoAuthorityColor;
                    Debug.Log("Setting Authority Status to red");

                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void HideObjectfromClientServerRpc(string gameObject)
        {
            if(gameObject != null)
            {
                HideOnAllClientRpc(gameObject);
            }
        }
        [ClientRpc]
        private void HideOnAllClientRpc(string gameObject)
        {
            _networkSpace.transform.Find(gameObject).gameObject.SetActive(false);
       
        }


        public void SetTaskGraphOnClients(string task)
        {
            if (IsServer)
            {
                SetTaskGraphClientRpc(task);
            }
        }

        [ClientRpc]
        private void SetTaskGraphClientRpc(string task)
        {
            GraphManager.Instance.SetGraph(task);
        }

        [ClientRpc]
        public void DisplayTaskCompletedClientRPC(string StatusMessage)
        {
            if (!IsHost)
            {
                if (PlatformManager.Instance.platform == PlatformManager.Platform.Desktop || PlatformManager.Instance.platform == PlatformManager.Platform.Editor || PlatformManager.Instance.platform == PlatformManager.Platform.AR)
                {

                    TaskCompleted.SetActive(true);
                    TaskCompleted.GetComponentInChildren<TMP_Text>().text = StatusMessage;
                }
                else
                {
                    TaskCompleted3D.SetActive(true);
                    TaskCompleted3DDescription.text = StatusMessage;
                }
            }

        }


        public void SetSharedSpacePlatformAuthority(string platform)
        {
            PlatformManager.Platform pltform;
            if (Enum.TryParse(platform, out pltform))
            {
                SetSharedSpacePlatformAuthorityClientRpc(pltform);
                foreach(Transform tr in PlatformButtonsGroup)
                {
                    if(tr.name == platform)
                    {
                        tr.GetChild(1).gameObject.SetActive(true);
                    }
                    else
                    {
                        tr.GetChild(1).gameObject.SetActive(false);
                    }
                }

            }
        }
        //if the platform is the same as the client's platform, grant ownership
        [ClientRpc]
        private void SetSharedSpacePlatformAuthorityClientRpc(PlatformManager.Platform platform)
        {
            if (!IsHost)
            {
                if (PlatformManager.Instance.platform == platform)
                {
                    GrantOwnershipServerRPC(NetworkManager.Singleton.LocalClientId);
                }
            }
        }

    }
}
