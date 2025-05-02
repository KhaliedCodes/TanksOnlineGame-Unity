using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace NCOTank
{
    public class MainMenuUI : MonoBehaviour
    {

        [SerializeField] private TMP_InputField IF_PlayerName;
        [SerializeField]private TMP_Dropdown DD_TeamId;
        [SerializeField] private TMP_Dropdown DD_ClassId;

        private void Start()
        {
            // Set the default value for the input field
            // IF_PlayerName.text = "Player" + Random.Range(0, 1000).ToString();
            // DD_TeamId.value = 0;
            // DD_ClassId.value = 0;
            IniliatizeUI();
        }

        void IniliatizeUI(){
            string[] teamNames = System.Enum.GetNames(typeof(Team));
            List<TMP_Dropdown.OptionData> ddoptions = new();

            foreach (var teamName in teamNames)
            {
                ddoptions.Add(new TMP_Dropdown.OptionData{text = teamName});
            }
            DD_TeamId.options = ddoptions;

        }
        public void OnStartServerClicked()
        {
            GetPlayerData();
            // Start the server
            Debug.Log("Server started");

            NetworkingManager.Instance.StartServer();
        }

        public void OnStartClientClicked()
        {
            GetPlayerData();
            // Start the client
            Debug.Log("Client started");
            if (!string.IsNullOrEmpty(IF_PlayerName.text))
                NetworkingManager.Instance.StartClient();
        }


        public void OnStartHostClicked(){
            // Start the host
            Debug.Log("Host started");

            GetPlayerData();
            if (!string.IsNullOrEmpty(IF_PlayerName.text))
                NetworkingManager.Instance.StartHost();
        }
        public void GetName()
        {
            NetworkingManager.Instance.UpdatePlayerName(IF_PlayerName.text);
        }
        public void GetPlayerData(){
            System.Enum.TryParse(typeof(Team), DD_TeamId.captionText.text, out var teamId);
            PlayerData playerData = new PlayerData(){
                PlayerName = IF_PlayerName.text,
                TeamId = (Team)teamId,
                // Class = (PlayerClass)DD_ClassId.value
            };
            NetworkingManager.Instance.UpdatePlayerData(playerData);
        }
    }
}
