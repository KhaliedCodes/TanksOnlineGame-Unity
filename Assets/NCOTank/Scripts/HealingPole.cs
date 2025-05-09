using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGOTank
{
    public class HealingPole : MonoBehaviour
    {
        [SerializeField] private int Heal = 20;
        ulong OwnerId;
        List<Collider> hitColliders = new List<Collider>();
        private float HealTime = 0f; // Time between heals in seconds
        public void Init(ulong shooterId, Material material)
        {
            OwnerId = shooterId;
            GetComponent<Renderer>().material = material; // Set the bullet's material
        }
        void Start()
        {
            Destroy(gameObject, 5f); // Destroy the grenade after 5 seconds
        }

        void Update()
        {
            HealTime += Time.deltaTime; // Increment the time since the grenade was created
            if (HealTime >= 1) // 60 FPS * 5 seconds
            {
                foreach (Collider player in hitColliders)
                {
                    if (player.TryGetComponent<NetworkPlayer>(out var networkPlayer))
                    {
                        networkPlayer.ApplyHeal(Heal); // Negative damage = healing
                    }
                }
                HealTime = 0;
            }
            
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                bool IsSameTeam = NetworkingManager.Instance.GetPlayer(OwnerId).pData.Value.TeamId == other.GetComponent<NetworkPlayer>().pData.Value.TeamId;
                if (!IsSameTeam)
                {
                    return;
                }
                if (NetworkingManager.Instance.IsServer)
                {
                    hitColliders.Add(other);
                    Debug.Log("Hit player: " + other.name);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (hitColliders.Contains(other))
            {
                hitColliders.Remove(other);
                Debug.Log("Exit player: " + other.name);
            }
        }
    }
}
