using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPS
{

    public enum PlayerMove{
        None = 0,
        Rock = 1,
        Paper = 2,
        Scissors = 3
    }
    public enum EndResult
    {
        None = 0,
        Draw = 1,
        Win = 2,
        Lose = 3,
    }
    public class GameplayUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI txt_localPlayerName;
        [SerializeField] TextMeshProUGUI txt_localplayerScore;
        [SerializeField] TextMeshProUGUI txt_otherPlayerName;
        [SerializeField] TextMeshProUGUI txt_otherPlayerScore;
        [SerializeField] Button btn_Rock;
        [SerializeField] Button btn_Paper;
        [SerializeField] Button btn_Scissors;
        [SerializeField] TextMeshProUGUI txt_Result;

        public void UpdateName(bool isLocal, string pName)
        {
            if (isLocal)
            {
                txt_localPlayerName.text = pName;
            }
            else
            {
                txt_otherPlayerName.text = pName;
            }
        }
        public void OnMoveSelected(int Move)
        {
            NetworkingManager.Instance.LocalPlayer.CmdUpdatePlayerMove((PlayerMove)Move);
        }
        public void UpdateScore(bool islocal, int score)
        {
            if (islocal)
            {
                txt_localplayerScore.text = "Score : " + score.ToString();
            }
            else
            {
                txt_otherPlayerScore.text = "Score : " + score.ToString();
            }
        }
        public void DisplayEndResult(EndResult result)
        {
            Debug.Log("End Result: " + result.ToString());
            txt_Result.text = result.ToString();
            Invoke("ResetButtons", 2f);
        }
        void ResetButtons()
        {
            btn_Rock.interactable = true;
            btn_Paper.interactable = true;
            btn_Scissors.interactable = true;
        }

    }
}
