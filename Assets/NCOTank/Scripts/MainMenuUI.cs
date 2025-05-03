using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NGOTank
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
            string[] classNames = System.Enum.GetNames(typeof(Class));
            List<TMP_Dropdown.OptionData> ddTeamOptions = new();

            foreach (var teamName in teamNames)
            {
                ddTeamOptions.Add(new TMP_Dropdown.OptionData{text = teamName});
            }
            DD_TeamId.options = ddTeamOptions;

            List<TMP_Dropdown.OptionData> ddClassOptions = new();

            foreach (var className in classNames)
            {
                ddClassOptions.Add(new TMP_Dropdown.OptionData { text = className });
            }
            DD_ClassId.options = ddClassOptions;

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
            System.Enum.TryParse(typeof(Class), DD_ClassId.captionText.text, out var classId);
            PlayerData playerData = new(){
                PlayerName = IF_PlayerName.text,
                TeamId = (Team)teamId,
                ClassId = (Class)classId
            };
            NetworkingManager.Instance.UpdatePlayerData(playerData);

            using var writer = new FastBufferWriter(FixedString64Bytes.UTF8MaxLengthInBytes + sizeof(Team) + sizeof(Class), Allocator.Temp);
            writer.WriteNetworkSerializable(playerData);

            // Set the connection data in NetworkConfig
            NetworkingManager.Singleton.NetworkConfig.ConnectionData = writer.ToArray();
        }
    }
}
