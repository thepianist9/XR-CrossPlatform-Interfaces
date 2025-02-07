using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;

namespace Game
{
    /// <summary>
    /// A class to bind UI events to invocations from <see cref="OptionalConnectionManager"/>, where client and host
    /// connection requests are initiated. This class also listens for status updates from Netcode for GameObjects to
    /// then display the appropriate UI elements.
    /// </summary>
    public sealed class XRNetworkAppController : MonoBehaviour
    {
        [SerializeField]
        NetworkManager m_NetworkManager;
        [SerializeField]
        SelectTransformGizmoXR m_selectTransformGizmo;
        [SerializeField] private Color m_AuthorityColor;
        [SerializeField] private Color m_NoAuthorityColor;
        [SerializeField] private GameObject MenuObject;

        [SerializeField] private string WirelessIp = "192.168.188.65";


        [SerializeField]
        XROptionalConnectionManager m_ConnectionManager;
        ushort port = 7777;

        [SerializeField] private GameObject AuthorityStatusGO;
        [SerializeField] private Transform NetworkParentGO;
/*
        [SerializeField] private GameObject m_PrivateSpaceBtn;
        [SerializeField] private GameObject m_SharedSpaceBtn;*/


        //UI for input for network configuration
        [SerializeField] private TMP_InputField IpAddress;
        public bool privateSpace = true;

        [SerializeField] private Button HostButton;
        [SerializeField] private Button ClientButton;
        [SerializeField] private Button DisconnectButton;
        

        void Awake()
        {
            m_NetworkManager = FindObjectOfType<NetworkManager>();
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            HostButton.onClick.AddListener(StartHost);
            ClientButton.onClick.AddListener(StartClient);
            DisconnectButton.onClick.AddListener(Disconnect);

            
            m_NetworkManager.OnClientConnectedCallback += OnClientConnected;
            m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;

        }

        void OnDestroy()
        {
            HostButton.onClick.RemoveAllListeners();
            ClientButton.onClick.RemoveAllListeners();
            DisconnectButton.onClick.RemoveAllListeners();

            m_NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            m_NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;

        }

        void OnClientConnected(ulong clientId)
        {
            // for host
            if (clientId == m_NetworkManager.LocalClientId)    
            {

                if (m_NetworkManager.IsHost)
                {

                    //Get all object and spwawn them on the network
                    AuthorityStatusGO.GetComponent<MeshRenderer>().material.color = m_AuthorityColor;
                    //LoadAllNetworkDataObjects();
                }
                else if(m_NetworkManager.IsClient)
                {
                    MenuObject.SetActive(false);
                   
                    if (AuthorityStatusGO is not null)
                    {
                        AuthorityStatusGO.GetComponent<MeshRenderer>().material.color = m_NoAuthorityColor;
                    }


                }
            }
/*            else if (m_NetworkManager.IsClient)
            {
                // for clients that are not host
                if (clientId == m_NetworkManager.LocalClientId)
                {
                    // grab all locally loaded prefabs and represent that on local client
                    var loadedPrefabs = GetLoadedPrefabsHashesAndNames();


                }
            }*/
        }

        private void LoadAllNetworkDataObjects()
        {
            foreach(Transform DataObjectTr in NetworkParentGO)
            { 
                NetworkObject no = DataObjectTr.GetComponent<NetworkObject>();
                if(no != null)
                {
                    no.enabled = true;
                    DataObjectTr.GetComponent<ClientNetworkTransform>().enabled = true;
                }

            }
        }


        public void ResetAllNetworkObjects()
        {
            foreach (Transform DataObjectTr in NetworkParentGO)
            {
                if(DataObjectTr.GetComponent<MeshRenderer>() != null)
                    ToggleShowMesh(DataObjectTr);
            }

/*            if (m_NetworkManager.IsHost)
            {
                foreach (Transform DataObjectTr in NetworkParentGO)
                {
                    NetworkObject no = DataObjectTr.GetComponent<NetworkObject>();
                    no.enabled = false;
                    DataObjectTr.GetComponent<ClientNetworkTransform>().enabled = false;
                }
            }
            if(m_NetworkManager.IsClient)
            {
                foreach (Transform DataObjectTr in NetworkParentGO)
                {
                    NetworkObject no = DataObjectTr.GetComponent<NetworkObject>();
                    no.enabled = false;
                    DataObjectTr.GetComponent<ClientNetworkTransform>().enabled = false;
                }
            }*/

        }

        private void ToggleShowMesh(Transform tr)
        {
            tr.GetComponent<MeshRenderer>().enabled = false;
        }


        static Tuple<int[], string[]> GetLoadedPrefabsHashesAndNames()
        {
            var loadedHashes = new int[DynamicPrefabLoadingUtilities.LoadedDynamicPrefabResourceHandles.Keys.Count];
            var loadedNames = new string[DynamicPrefabLoadingUtilities.LoadedDynamicPrefabResourceHandles.Keys.Count];
            int index = 0;
            foreach (var loadedPrefab in DynamicPrefabLoadingUtilities.LoadedDynamicPrefabResourceHandles)
            {
                loadedHashes[index] = loadedPrefab.Key.GetHashCode();
                loadedNames[index] = loadedPrefab.Value.Result.name;
                index++;
            }

            return Tuple.Create(loadedHashes, loadedNames);
        }
        
        void OnClientDisconnect(ulong clientId)
        {
          
            m_selectTransformGizmo.DeactivateEdit();
            AuthorityStatusGO.GetComponent<MeshRenderer>().material.color = Color.gray;
            //ResetAllNetworkObjects();
        }
        /*
        public void SetUndo(bool shared)
        {
            privateSpace = shared;

            //set button color to green whichever is selected and reset other to white
            if (privateSpace)
            {
                m_PrivateSpaceBtn.GetComponent<Image>().color = Color.green;
                m_SharedSpaceBtn.GetComponent<Image>().color = Color.white;
            }
            else
            {
                m_PrivateSpaceBtn.GetComponent<Image>().color = Color.white;
                m_SharedSpaceBtn.GetComponent<Image>().color = Color.green;
            }
        }*/

        void StartHost()
        {
            //LOCAL HOST

            Debug.Log("[APP CONTROLLER]:Started Host on: " + nameof(StartHost));
            if (IpAddress.text != null)
            {
                m_ConnectionManager.StartHostIp(WirelessIp, port);
                Debug.Log("[APP CONTROLLER]:Started Host on: " + nameof(StartHost));
            }
            else
            {
                Debug.Log("[APP CONTROLLER]: Port entered not in correct format");
            }


        }

        public void ResetTransform(int index)
        {
            if (index != null)
            { 
                if(privateSpace)
                    GameObject.FindGameObjectWithTag("Private Space").transform.GetChild(index).GetComponent<ARObject>().UndoTransform();
                else
                    GameObject.FindGameObjectWithTag("Shared Space").transform.GetChild(index).GetComponent<ARObject>().UndoTransform();
            }
        }

        void StartClient()
        {
            //START CLIENT
            ushort port = 7777;
            Debug.Log("[APP CONTROLLER]:Started Host on: " + nameof(StartClient));
            if (IpAddress.text != null)
            {
                m_ConnectionManager.StartClientIp(WirelessIp, port);
                Debug.Log("[APP CONTROLLER]:Started Host on: " + nameof(StartClient));
            }
            else
            {
                Debug.Log("[APP CONTROLLER]: Port entered not in correct format");
            }
        }
        public void StartClientXR()
        {
            //START CLIENT
            ushort port = 7777;
            Debug.Log("[APP CONTROLLER]:Started Host on: " + nameof(StartClient));
            if (IpAddress.text != null)
            {
                m_ConnectionManager.StartClientIp(WirelessIp, port);
                Debug.Log("[APP CONTROLLER]:Started Host on: " + nameof(StartClient));
            }
            else
            {
                Debug.Log("[APP CONTROLLER]: Port entered not in correct format");
            }
        }

        public void Disconnect()
        {
            Debug.Log(nameof(Disconnect));
            m_ConnectionManager.RequestShutdown();
        }

        void OnApplicationQuit()
        {
            Disconnect();
        }
    }
}
