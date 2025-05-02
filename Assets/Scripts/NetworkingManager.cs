using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq;

namespace RPS
{
    public class NetworkingManager : NetworkManager
    {
        public static NetworkingManager Instance { get; private set; }
        public bool IsServer {get; private set; }
        public bool IsClient {get; private set; }
        List<NetworkingPlayer> netPlayers = new List<NetworkingPlayer>();
        public bool IsHost => IsServer && IsClient;
        public string LocalPlayerName { get; private set; }
        public NetworkingPlayer LocalPlayer => netPlayers.First(x => x.isLocalPlayer);
        public NetworkingPlayer OtherPlayer => netPlayers.First(x => !x.isLocalPlayer);

        public override void Awake()
        {
            base.Awake();
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created


        public override void OnStartServer()
        {
            base.OnStartServer();
            IsServer = true;
            Debug.Log("Server started");
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            IsClient = true;
            Debug.Log("Client started");
        }

        public void UpdatePlayerName(string pName){
            LocalPlayerName = pName;
        }

        public void AddPlayer(NetworkingPlayer player)
        {
            if (!netPlayers.Contains(player))
            {
                netPlayers.Add(player);
                Debug.Log($"Player {player.netId} added to the list.");
            }
            else
            {
                Debug.LogWarning($"Player {player.netId} is already in the list.");
            }
        }
        public void RemovePlayer(NetworkingPlayer player)
        {
            if (netPlayers.Contains(player))
            {
                netPlayers.Remove(player);
                Debug.Log($"Player {player.netId} removed from the list.");
            }
            else
            {
                Debug.LogWarning($"Player {player.netId} is not in the list.");
            }
        }

        [Server]
        bool AllPlayersPlayed()
        {
            return netPlayers.Count == 2 && netPlayers.All(x => x.playerMove != PlayerMove.None);
        }
        [Server]
        public void CheckToCalculateResult()
        {
            if (!AllPlayersPlayed()) return;
            PlayerMove p1Move = netPlayers[0].playerMove;
            PlayerMove p2Move = netPlayers[1].playerMove;
            EndResult p1Result = (EndResult)(((int)p1Move - (int)p2Move + 3) % 3 + 1);
            EndResult p2Result = (EndResult)(((int)p2Move - (int)p1Move + 3) % 3 + 1);
            netPlayers[0].UpdateScore(p1Result == EndResult.Win);
            netPlayers[1].UpdateScore(p2Result == EndResult.Win);
            netPlayers[0].TargetSetEndResult(p1Result);
            netPlayers[1].TargetSetEndResult(p2Result);
        }
    }
}
