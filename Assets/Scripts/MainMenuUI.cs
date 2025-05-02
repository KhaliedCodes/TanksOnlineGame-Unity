using TMPro;
using UnityEngine;

namespace RPS
{
    public class MainMenuUI : MonoBehaviour
    {

        [SerializeField] private TMP_InputField IF_PlayerName;
        public void OnStartServerClicked()
        {
            GetName();
            // Start the server
            Debug.Log("Server started");

            NetworkingManager.Instance.StartServer();
        }

        public void OnStartClientClicked()
        {
            GetName();
            // Start the client
            Debug.Log("Client started");
            if (!string.IsNullOrEmpty(IF_PlayerName.text))
                NetworkingManager.Instance.StartClient();
        }


        public void OnStartHostClicked(){
            // Start the host
            Debug.Log("Host started");

            GetName();
            if (!string.IsNullOrEmpty(IF_PlayerName.text))
                NetworkingManager.Instance.StartHost();
        }
        public void GetName()
        {
            NetworkingManager.Instance.UpdatePlayerName(IF_PlayerName.text);
        }
    }
}
