using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace NCOTank
{
    public class NetworkingManager : NetworkManager
    {
        static NetworkingManager instance;
        public static NetworkingManager Instance => instance;
        public const string SceneName_MainMenu = "MainMenu";
        public const string SceneName_Gameplay = "Gameplay";
        public string LocalPlayerName;
        Dictionary<ulong, NetworkPlayer> players = new Dictionary<ulong, NetworkPlayer>();

        public PlayerData playerData {get; private set; } = new PlayerData();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

            OnServerStarted += NetMgr_ServerStarted;
        }

        // Update is called once per frame
        void Update()
        {
        
        }



        void NetMgr_ServerStarted()
        {
            SceneManager.LoadScene(SceneName_Gameplay, UnityEngine.SceneManagement.LoadSceneMode.Single);
                // SpawnPlayer(clientId);
        }

        private void OnDestroy()
        {
            OnServerStarted -= NetMgr_ServerStarted;
        }
        public void UpdatePlayerName(string pName)
        {
            LocalPlayerName = pName;

        }

        public void AddPlayer(NetworkPlayer networkPlayer)
        {
            if (!players.ContainsKey(networkPlayer.OwnerClientId))
            {

                players.Add(networkPlayer.OwnerClientId, networkPlayer);
            }
        }

        public void RemovePlayer(ulong clientId)
        {
            if (players.ContainsKey(clientId))
            {
                players.Remove(clientId);
            }
        }

        public NetworkPlayer GetPlayer(ulong clientId)
        {
            if (players.ContainsKey(clientId))
            {
                return players[clientId];
            }

            return null;
        }

        public void UpdatePlayerData(PlayerData playerData)
        {
            this.playerData = playerData;
        }
    }
}
