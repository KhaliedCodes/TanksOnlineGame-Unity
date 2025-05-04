using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace NGOTank
{
    public class NetworkingManager : NetworkManager
    {
        static NetworkingManager instance;
        public static NetworkingManager Instance => instance;
        public const string SceneName_MainMenu = "MainMenu";
        public const string SceneName_Gameplay = "Gameplay";
        public string LocalPlayerName;
        public Dictionary<ulong, NetworkPlayer> players = new Dictionary<ulong, NetworkPlayer>();

        public PlayerData playerData {get; private set; } = new PlayerData();
        public Dictionary<ulong, PlayerData> clientData = new Dictionary<ulong, PlayerData>();
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
            ConnectionApprovalCallback += ApprovalCheck;
            OnServerStarted += NetMgr_ServerStarted;
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

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
            if(instance != null)
            {
                OnServerStarted -= NetMgr_ServerStarted;
            }
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            Debug.Log($"Connection approval check for client: {request.ClientNetworkId}");
            byte[] connectionData = request.Payload; // Use Payload directly


            if (connectionData == null || connectionData.Length == 0)
            {
                Debug.LogError($"Client {request.ClientNetworkId} provided no connection data.");
                response.Approved = false;
                response.Reason = "Missing connection data.";
                return;
            }


            // Deserialize the PlayerData from the payload
            try
            {
                using var reader = new FastBufferReader(connectionData, Allocator.Temp);
                reader.ReadNetworkSerializable(out PlayerData data);


                // Store the data on the server, associated with the client ID
                clientData[request.ClientNetworkId] = data;
                Debug.Log($"Stored PlayerData for client {request.ClientNetworkId}: {data}");


                // --- Approval Decision ---
                response.Approved = true;
                response.CreatePlayerObject = false; // Important: We will manually spawn the player in GameplayManager
                response.Pending = false; // Process immediately
                // Optionally: Set PlayerPrefabHash = null if you're not using it for selection here.
                // Optionally: Set Position/Rotation if you want to override default spawn, but GameplayManager handles it.


                // Log approval
                Debug.Log($"Approved connection for client: {request.ClientNetworkId} with data: {data}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize PlayerData for client {request.ClientNetworkId}: {e.Message}");
                response.Approved = false;
                response.Reason = "Invalid connection data format.";
            }


        }

        public bool TryGetPlayerData(ulong clientId, out PlayerData playerData)
        {
            // Ensure this runs only on server/host where the data is stored
            if (IsServer || IsHost)
            {
                return clientData.TryGetValue(clientId, out playerData);
            }
            else
            {
                playerData = default; // Return default if called on a client
                Debug.LogWarning("TryGetPlayerData called on a client. Data is only available on the server/host.");
                return false;
            }
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


        public GameState CheckAllPlayersOnOneTeamDead(){
            bool BlueWins = players.Values.Where(player => player.pData.Value.TeamId == Team.Red).All(player => player.isDead);
            bool RedWins = players.Values.Where(player => player.pData.Value.TeamId == Team.Blue).All(player => player.isDead);
            Debug.Log($"BlueWins: {BlueWins}, RedWins: {RedWins}");
            if (BlueWins)
            {
                Debug.Log("Red Team Lose");
                // Handle Red Team Lose logic here
                return GameState.BlueWins;
            }
            else if (RedWins)
            {
                Debug.Log("Blue Team Lose");
                // Handle Blue Team Lose logic here
                return GameState.RedWins;
            }
            return GameState.InProgress;
        }


    
        public void EndGame(){
            StartCoroutine(ShutDownDelay(5f));
        }
        IEnumerator ShutDownDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Debug.Log("Game Over! Returning to Main Menu.");
            if (!IsHost)
            {
                // NetworkingManager.Instance.SceneManager.LoadScene(NetworkingManager.SceneName_MainMenu, UnityEngine.SceneManagement.LoadSceneMode.Single);
                Shutdown();
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName_MainMenu, UnityEngine.SceneManagement.LoadSceneMode.Single);
            
            }
            if (IsServer || IsHost)
            {
                NetworkingManager.Instance.clientData.Clear();
                NetworkingManager.Instance.players.Clear();
                NetworkingManager.Instance.Shutdown();
                NetworkingManager.Instance.SceneManager.LoadScene(NetworkingManager.SceneName_MainMenu, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }

        }
    }
}
