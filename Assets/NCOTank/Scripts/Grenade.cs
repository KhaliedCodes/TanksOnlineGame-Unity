using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGOTank
{
    public class Grenade : MonoBehaviour
    {
        [SerializeField] private int Damage = 20;
        ulong OwnerId; 
        Rigidbody rb;
        List<Collider> hitColliders = new List<Collider>();
        public void Init(ulong shooterId, Material material)
        {
            OwnerId = shooterId;
            GetComponent<Renderer>().material = material; // Set the bullet's material
        }
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.linearVelocity = transform.up * 5f;
            // if (TryGetComponent(out rb))
            // {
                
            // }
            StartCoroutine(Explode());
        }

        IEnumerator Explode()
        {
            yield return new WaitForSeconds(5f); // Wait for 2 seconds before exploding
            // Add explosion logic here (e.g., apply damage to players in range)
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent(out NetworkPlayer player))
                {
                    // Apply damage to the player
                    player.ApplyDamage(Damage, OwnerId);
                }
            }
            Destroy(gameObject); // Destroy the grenade
            
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                bool IsSameTeam = NetworkingManager.Instance.GetPlayer(OwnerId).pData.Value.TeamId == other.GetComponent<NetworkPlayer>().pData.Value.TeamId;
                if (IsSameTeam)
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
            if(hitColliders.Contains(other))
            {
                hitColliders.Remove(other);
                Debug.Log("Exit player: " + other.name);
            }
        }
    }
}
