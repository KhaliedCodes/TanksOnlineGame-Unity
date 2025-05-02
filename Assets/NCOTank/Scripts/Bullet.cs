using UnityEngine;

namespace NCOTank
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 20f; // Speed of the bullet
        [SerializeField] private int Damage = 10;
        ulong OwnerId;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        Rigidbody rb;

        public void Init(ulong shooterId)
        {
            OwnerId = shooterId;
        }
        void Start()
        {
            if(TryGetComponent(out rb)){
                rb.linearVelocity = transform.forward * speed;
            }
            Destroy(gameObject, 1.5f); // Destroy the bullet after 5 seconds to prevent memory leaks
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void OnTriggerEnter(Collider other)
        {
            if(NetworkingManager.Instance.IsServer)
                if(other.CompareTag("Player"))
                {
                    // Deal damage to the player
                    if (other.TryGetComponent<NetworkPlayer>(out var player))
                    {
                        player.ApplyDamage(Damage, OwnerId);
                    }
                    Destroy(gameObject); // Destroy the bullet on impact
                }
        }
    }
}
