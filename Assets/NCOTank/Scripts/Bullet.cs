using UnityEngine;

namespace NGOTank
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 20f; // Speed of the bullet
        public int Damage = 10;
        ulong OwnerId;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        Rigidbody rb;

        public void Init(ulong shooterId, int damage, Material material)
        {
            OwnerId = shooterId;
            Damage = damage;
            GetComponent<Renderer>().material = material; // Set the bullet's material
        }
        void Start()
        {
            if (TryGetComponent(out rb))
            {
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
            if (other.CompareTag("Player"))
            {
                bool IsSameTeam = NetworkingManager.Instance.GetPlayer(OwnerId).pData.Value.TeamId == other.GetComponent<NetworkPlayer>().pData.Value.TeamId;
                if (IsSameTeam)
                {
                    Debug.LogWarning("Cannot damage a player on the same team.");
                    return;
                }
                if (NetworkingManager.Instance.IsServer)

                {

                    // Deal damage to the player
                    if (other.TryGetComponent<NetworkPlayer>(out var player))
                    {
                        player.ApplyDamage(Damage, OwnerId);
                    }
                    // Destroy the bullet on impact
                }
            }


            // Destroy the bullet on impact with a wall
            Destroy(gameObject);

        }

    }
}
