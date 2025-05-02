using Mirror;
using UnityEngine;

namespace RPS
{
    public class NetworkingPlayer : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnNameUpdated))] string pName; 
        GameplayUI gameUI;

        [SyncVar(hook = nameof(OnMoveUpdated))] public PlayerMove playerMove;
        [SyncVar(hook = nameof(OnScoreUpdated))] int score;

        private void Awake()
        {
            gameUI = FindFirstObjectByType<GameplayUI>();
        }

        public override void OnStartLocalPlayer(){
            base.OnStartLocalPlayer(); 
            string playerName = NetworkingManager.Instance.LocalPlayerName;
            CmdUpdatePlayerName(playerName);
        }

        [Command]
        void CmdUpdatePlayerName(string playerName){
            pName = playerName;
        }

        void OnNameUpdated(string oldVal, string newVal ){
            pName = newVal;
            Debug.Log("Netplayer of netId: "+ netId+  "Player name updated to: " + pName);
            gameUI.UpdateName(isLocalPlayer, pName);
        }
        void OnMoveUpdated(PlayerMove oldMove, PlayerMove newMove)
        {
            playerMove = newMove;
        }
        public override void OnStopClient()
        {
            base.OnStopClient();
            NetworkingManager.Instance.RemovePlayer(this);
        }

        [Command]
        public void CmdUpdatePlayerMove(PlayerMove newMove)
        {
            playerMove = newMove;
            NetworkingManager.Instance.CheckToCalculateResult();
        }
        void OnScoreUpdated(int oldScore, int newScore)
        {
            score = newScore;
            gameUI.UpdateScore(isLocalPlayer, score);
        }
        [Server]
        public void UpdateScore(bool increment)
        {
            score += increment ? 1 : 0;
        }
        [TargetRpc]
        public void TargetSetEndResult(EndResult result)
        {
            gameUI.DisplayEndResult(result);
            CmdUpdatePlayerMove(PlayerMove.None);
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            NetworkingManager.Instance.AddPlayer(this);
        }
    }
}
