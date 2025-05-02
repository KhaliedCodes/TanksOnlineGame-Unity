using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NCOTank
{
    public class NetworkPlayer : NetworkBehaviour
    {

        [SerializeField] private float moveSpeed = 5f; // Speed of the player movement
        [SerializeField] private float rotationSpeed = 720f; // Speed of the player rotation
        [SerializeField] private Transform cannonTransform; // Reference to the cannon transform
                                                            // Start is called once before the first execution of Update after the MonoBehaviour is created
        // NetworkVariable<FixedString64Bytes> pName = new NetworkVariable<FixedString64Bytes>();
        NetworkVariable<PlayerData> pData = new NetworkVariable<PlayerData>();
        NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();
        private Rigidbody rb;
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private Transform PlayerUI;
        private readonly int MaxHealth = 100;
        [SerializeField] private Transform img_health;
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform bulletSpawnPoint;
        bool isDead = false;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            PlayerUI.LookAt(Camera.main.transform.position);
            if(IsLocalPlayer && !isDead){
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
            }else {
                InitilatizePlayer();
            }
            if(IsServer){
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
        #endregion
        private void OnPlayerDataUpdated(PlayerData previousValue, PlayerData newValue)
        {
            InitilatizePlayer();
            //update in ui
            Debug.Log($"client{OwnerClientId} update playerData to {pData}");
        }
        void InitilatizePlayer(){
            // Set the player name text to the player's name

            playerNameText.text = pData.Value.PlayerName.ToString();
            OnHealthUpdated(0, CurrentHealth.Value);
            
        }

        private void OnHealthUpdated(int previousValue, int newValue)
        {
            // Update the health UI or perform any other actions based on health changes
            Debug.Log($"CurrentHealth.Value/ MaxHealth: {CurrentHealth.Value/ MaxHealth}");
            img_health.localScale =  new Vector3((float)CurrentHealth.Value/ MaxHealth , 1, 1);
            
        }
        [ServerRpc]
        void ShootServerRPC(){
            Bullet bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.Init(OwnerClientId);
            ShootClientRPC(bullet.transform.position, bullet.transform.rotation);
        }

        [ClientRpc]
        void ShootClientRPC(Vector3 position, Quaternion rotation){
            if(!NetworkManager.Singleton.IsHost){
                Bullet bullet = Instantiate(bulletPrefab, position, rotation);
                bullet.Init(OwnerClientId);
            }
        }

        [ClientRpc]
        void KillPlayerClientRpc(ulong OwnerId)
        {
            NetworkPlayer killer = NetworkingManager.Instance.GetPlayer(OwnerId);
            isDead = true;
            // Debug.Log($"Player {pName.Value} has been killed by {killer.pName.Value}");
        }


        public void ApplyDamage(int damage, ulong OwnerId){
            if(!IsServer){
                Debug.LogWarning("ApplyDamage can only be called on the server.");
                return;
            } 
            CurrentHealth.Value -= damage;
            CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value); // Ensure health doesn't go below 0

            if(CurrentHealth.Value <= 0){
                // Destroy the player object or perform any other actions when health reaches zero
                Debug.Log($"Player {OwnerClientId} has been destroyed.");
                KillPlayerClientRpc(OwnerId);
                NetworkObject.Despawn();
                // Destroy(gameObject);
            }
        }
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            pData.OnValueChanged -= OnPlayerDataUpdated;
            CurrentHealth.OnValueChanged -= OnHealthUpdated;


            NetworkingManager.Instance.RemovePlayer(OwnerClientId);
        }
    }

    
}
