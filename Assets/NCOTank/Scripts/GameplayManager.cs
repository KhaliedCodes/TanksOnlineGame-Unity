using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NGOTank
{
    public enum GameState{
        RedWins,
        BlueWins,
        InProgress
    }
    public class GameplayManager : MonoBehaviour
    {
        [SerializeField] private Transform[] spawningPosList;
        [SerializeField] private NetworkObject playerDPSPrefab;
        [SerializeField] private NetworkObject playerTankPrefab;
        private int currentSpawningIndex = 0;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            NetworkingManager.Instance.SceneManager.OnLoadComplete += NetSceneMgr_LoadCompleted;
            // NetworkingManager.Instance.OnClientConnectedCallback += SpawnNextPlayer;
            if (NetworkingManager.Singleton.IsHost)
            {
                SpawnNextPlayer(NetworkingManager.Singleton.LocalClientId);
            }
        }

        private void NetSceneMgr_LoadCompleted(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (sceneName == NetworkingManager.SceneName_Gameplay)
            {
                SpawnNextPlayer(clientId);
            }
        }
        private void SpawnNextPlayer(ulong clientId)
        {
            // Spawn the player object for the client
            NetworkingManager.Instance.TryGetPlayerData(clientId, out PlayerData playerData);
            NetworkObject playerObject;
            if (playerData.ClassId == Class.DPS)
                playerObject = Instantiate(playerDPSPrefab, spawningPosList[currentSpawningIndex]);
            else
                playerObject = Instantiate(playerTankPrefab, spawningPosList[currentSpawningIndex]);



            // playerObject.GetComponent<NetworkObject>().Spawn();
            playerObject.SpawnAsPlayerObject(clientId);
            currentSpawningIndex++;
            currentSpawningIndex %= spawningPosList.Length; // Loop back to the first spawn point
        }

        // Update is called once per frame
        void Update()
        {

        }
        void OnDestroy()
        {
            if (NetworkingManager.Instance.SceneManager == null) return;
            NetworkingManager.Instance.SceneManager.OnLoadComplete -= NetSceneMgr_LoadCompleted;

            // NetworkingManager.Instance.OnClientConnectedCallback -= SpawnNextPlayer;
        }
        


    }
}
