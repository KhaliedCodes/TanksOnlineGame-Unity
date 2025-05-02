using TMPro;
using UnityEngine;

namespace RPS
{
    public class NetworkingIdentity : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [SerializeField] TMP_InputField IF_playerName;

        public void OnStartServerClicked()
        {
            NetworkingManager.Instance.StartServer();
        }

        public void OnStartHostClicked()
        {
            if (!string.IsNullOrEmpty(IF_playerName.text))
            {
                NetworkingManager.Instance.StartHost();
            }
        }

        public void OnStartClientClicked()
        {
            if (!string.IsNullOrEmpty(IF_playerName.text))
            {
                NetworkingManager.Instance.StartClient();
            }
        }

    }
}
