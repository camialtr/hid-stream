using Input;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes
{
    public class Recorder : MonoBehaviour
    {
        public Text uiText;
        
        private void Start()
        {
            if (!Controllers.Initialized)
            {
                Controllers.InitializeJoyCon();
            }
        }

        private void Update()
        {
            for (var i = 0; i < Controllers.Players.Length; i++)
            {
                Controllers.Players[i].Update();

                IsPlayerConnectedAndAskingForControl(Controllers.Players[i]);
                
                if (Controllers.Players[i].IsMaster()) uiText.text = $"Joy-Con {i + 1} is set as the target controller";
                
                Controllers.Players[i].Clear();
            }
        }

        private static void IsPlayerConnectedAndAskingForControl(IJoyController joyController)
        {
            if (!joyController.IsConnected() || joyController.IsMaster()) return;

            if (!joyController.Select()) return;
            
            ResetMasters();
            joyController.SetAsMaster(true);
        }

        private static void ResetMasters()
        {
            foreach (var player in Controllers.Players)
            {
                player.SetAsMaster(false);
            }
        }
    }
}
