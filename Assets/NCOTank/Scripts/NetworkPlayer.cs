using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace NGOTank
{
    public class NetworkPlayer : NetworkBehaviour
    {

        [SerializeField] private float moveSpeed = 5f; // Speed of the player movement
        [SerializeField] private float rotationSpeed = 720f; // Speed of the player rotation
        [SerializeField] private Transform cannonTransform; // Reference to the cannon transform
                                                            // Start is called once before the first execution of Update after the MonoBehaviour is created
                                                            // NetworkVariable<FixedString64Bytes> pName = new NetworkVariable<FixedString64Bytes>();
        public NetworkVariable<PlayerData> pData = new NetworkVariable<PlayerData>();
        NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
        private Rigidbody rb;
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private Transform PlayerUI;
        [SerializeField] private int MaxHealth = 100;
        [SerializeField] private int Damage = 10;
        [SerializeField] private Transform img_health;
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform bulletSpawnPoint;
        public bool isDead = false;
        [SerializeField] private Material RedMaterial;

        [SerializeField] private Material BlueMaterial;
        private Transform Panel_KillsLog;
        [SerializeField] private TMP_Text Text_KillLog;

        private GameObject Panel_Result;
        [SerializeField] private TMP_Text Text_ResultPrefab;
        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            PlayerUI.LookAt(Camera.main.transform.position);
            if (IsLocalPlayer && !isDead)
            {
                // Get input from the player
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");
                bool right = Input.GetKey(KeyCode.RightArrow);

                bool left = Input.GetKey(KeyCode.LeftArrow);
                bool shoot = Input.GetKeyDown(KeyCode.Space);

                // Calculate the movement direction
                Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;

                // Move the player
                rb.MovePosition(transform.position + moveSpeed * Time.deltaTime * moveDirection);

                if (right || left)
                {
                    float rotationAngle = 0;
                    if (right)
                    {
                        rotationAngle += rotationSpeed * Time.deltaTime;
                    }
                    if (left)
                    {

                        rotationAngle -= rotationSpeed * Time.deltaTime;
                    }
                    cannonTransform.Rotate(0, rotationAngle, 0);
                }
                if (shoot)
                {
                    ShootServerRPC();
                }
            }


        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            pData.OnValueChanged += OnPlayerDataUpdated;
            CurrentHealth.OnValueChanged += OnHealthUpdated;
            if (IsLocalPlayer)
            {
                UpdatePlayerDataServerRpc(NetworkingManager.Instance.playerData);
            }
            else
            {
                InitilatizePlayer();
            }
            if (IsServer)
            {
                CurrentHealth.Value = MaxHealth;
            }
            NetworkingManager.Instance.AddPlayer(this);
        }

        #region Server RPCs
        [ServerRpc]
        void UpdatePlayerDataServerRpc(PlayerData playerData)
        {
            // pName.Value = value;
            pData.Value = playerData;
        }
        [ServerRpc]
        void ShootServerRPC()
        {
            Bullet bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.Init(OwnerClientId, Damage);
            ShootClientRPC(bullet.transform.position, bullet.transform.rotation);
        }
        #endregion
        #region Client RPCs
        [ClientRpc]
        void ShootClientRPC(Vector3 position, Quaternion rotation)
        {
            if (!NetworkManager.Singleton.IsHost)
            {
                Bullet bullet = Instantiate(bulletPrefab, position, rotation);
                bullet.Init(OwnerClientId, Damage);
            }
        }

        [ClientRpc]
        void KillPlayerClientRpc(ulong OwnerId)
        {
            NetworkPlayer killer = NetworkingManager.Instance.GetPlayer(OwnerId);
            isDead = true;
            Debug.Log($"Player {pData.Value.PlayerName} has been killed by {killer.pData.Value.PlayerName}");
            TMP_Text Text_KillLogInstance = Instantiate(Text_KillLog, Vector3.zero, Quaternion.identity);
            Text_KillLogInstance.text = $"{killer.pData.Value.PlayerName} killed {pData.Value.PlayerName}";
            Text_KillLogInstance.transform.SetParent(Panel_KillsLog);
            Destroy(Text_KillLogInstance.gameObject, 5f); // Destroy the text after 3 seconds
        }
        #endregion
        private void OnPlayerDataUpdated(PlayerData previousValue, PlayerData newValue)
        {
            InitilatizePlayer();
            //update in ui
            Debug.Log($"client{OwnerClientId} update playerData to {pData}");
        }
        void InitilatizePlayer()
        {
            // Set the player name text to the player's name

            playerNameText.text = pData.Value.PlayerName.ToString();
            OnHealthUpdated(0, CurrentHealth.Value);
            Panel_KillsLog = GameObject.Find("Panel_KillsLog").transform;
            GetComponent<Renderer>().material = (int)pData.Value.TeamId == 1 ? BlueMaterial : RedMaterial;

            Panel_Result = GameObject.Find("Panel_Result");
        }

        private void OnHealthUpdated(int previousValue, int newValue)
        {
            // Update the health UI or perform any other actions based on health changes
            Debug.Log($"CurrentHealth.Value/ MaxHealth: {CurrentHealth.Value / MaxHealth}");
            img_health.localScale = new Vector3((float)CurrentHealth.Value / MaxHealth, 1, 1);

        }





        public void ApplyDamage(int damage, ulong OwnerId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("ApplyDamage can only be called on the server.");
                return;
            }

            CurrentHealth.Value -= damage;
            CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value); // Ensure health doesn't go below 0

            if (CurrentHealth.Value <= 0)
            {
                // Destroy the player object or perform any other actions when health reaches zero
                KillPlayerClientRpc(OwnerId);
                ShowResultUiClientRpc(NetworkingManager.Instance.CheckAllPlayersOnOneTeamDead());
                NetworkObject.Despawn();
                isDead = true;
            }
        }
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            pData.OnValueChanged -= OnPlayerDataUpdated;
            CurrentHealth.OnValueChanged -= OnHealthUpdated;


            // NetworkingManager.Instance.RemovePlayer(OwnerClientId);
        }
        [ClientRpc]
        void ShowResultUiClientRpc(GameState gameState)
        {
            Debug.Log($"Game state from the client side: {gameState}");
            if (gameState == GameState.BlueWins)
            {
                // Handle Blue Team Lose logic here
                Panel_Result.gameObject.GetComponent<Image>().color = new Color(0, 0, 1, 0.5f);
                TMP_Text Text_Result = Instantiate(Text_ResultPrefab, Vector3.zero, Quaternion.identity);
                Text_Result.transform.SetParent(Panel_Result.transform);
                Text_Result.transform.localPosition = new Vector3(0, 0, 0);
                Text_Result.text = "Blue Team Wins";
                NetworkingManager.Instance.EndGame();
            }
            else if (gameState == GameState.RedWins)
            {
                // Handle Red Team Lose logic here
                Panel_Result.gameObject.GetComponent<Image>().color = new Color(1, 0, 0, 0.5f);
                TMP_Text Text_Result = Instantiate(Text_ResultPrefab, Vector3.zero, Quaternion.identity);
                Text_Result.transform.SetParent(Panel_Result.transform);
                Text_Result.transform.localPosition = new Vector3(0, 0, 0);
                Text_Result.text = "Red Team Wins";
                NetworkingManager.Instance.EndGame();
            }

        }


    }


}
