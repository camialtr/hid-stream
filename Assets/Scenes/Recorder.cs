using System.Net;
using Input;
using nn.swkbd;
using UnityEngine;
using System.Text;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace Scenes
{
    public class Recorder : MonoBehaviour
    {
        public Text uiText;
        public Text uiText2;
        bool isConnected = false;
        DancerClient client;
        
        private void Start()
        {
            if (!Controllers.Initialized)
            {
                Controllers.InitializeJoyCon();
            }
        }

        private void Update()
        {
            //if (UnityEngine.Input.GetKeyDown(KeyCode.J))
            //{
            //    var currentIp = "192.168.1.3";
            //    if (IPAddress.TryParse(currentIp, out var ip))
            //    {
            //        isConnected = true;
            //        uiText2.text = $"Connecting to [{currentIp}]...";
            //        client = new DancerClient();
            //        client.Connect(currentIp, 14444);
            //        uiText2.text = $"Connected to [{currentIp}]";
            //        Debug.Log($"Connected to [{currentIp}].");
            //    }
            //    else
            //    {
            //        uiText2.text = $"Invalid IP address: {currentIp}";
            //        return;
            //    }
            //}
            //if (client is not null && client.Connected)
            //{
            //    var ndata = new NetworkData()
            //    {
            //        AccelX = Random.value,
            //        AccelY = Random.value,
            //        AccelZ = Random.value,
            //        GyroX = Random.value,
            //        GyroY = Random.value,
            //        GyroZ = Random.value
            //    };
            //    Debug.Log($"Sending data: {JsonConvert.SerializeObject(ndata)}");
            //    client.SendMessage(JsonConvert.SerializeObject(ndata));
            //}
            if (!isConnected && UnityEngine.Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                var stringBuilder = new StringBuilder();
                var keyboardArg = new ShowKeyboardArg();
                keyboardArg.keyboardConfig.keyboardMode = KeyboardMode.Alphabet;
                Swkbd.Initialize(ref keyboardArg);
                Swkbd.ShowKeyboard(stringBuilder, keyboardArg);
                var currentIp = stringBuilder.ToString();
                if (IPAddress.TryParse(currentIp, out var ip))
                {
                    isConnected = true;
                    uiText2.text = $"Connecting to [{currentIp}]...";
                    client = new DancerClient();
                    client.Connect(currentIp, 14444);
                    uiText2.text = $"Connected to [{currentIp}]";
                }
                else
                {
                    uiText2.text = $"Invalid IP address: {currentIp}";
                    return;
                }
            }
            
            for (var i = 0; i < Controllers.Players.Length; i++)
            {
                Controllers.Players[i].Update();

                IsPlayerConnectedAndAskingForControl(Controllers.Players[i]);
                
                if (Controllers.Players[i].IsMaster())
                {
                    var uiTextString = "";
                    
                    uiTextString += $"Joy-Con {i + 1} is set as the target controller";
                    var acceleration = Controllers.Players[i].GetAcceleration();
                    uiTextString += $"\n\nAcceleration: {acceleration.x}, {acceleration.y}, {acceleration.z}";
                    var angle = Controllers.Players[i].GetAngle();
                    uiTextString += $"\n\nAngle: {angle.x}, {angle.y}, {angle.z}";

                    if (client.Connected)
                    {
                        var ndata = new NetworkData()
                        {
                            AccelX = acceleration.x,
                            AccelY = acceleration.y,
                            AccelZ = acceleration.z,
                            GyroX = angle.x,
                            GyroY = angle.y,
                            GyroZ = angle.z
                        };
                        client.SendMessage(JsonConvert.SerializeObject(ndata));
                    }
                    
                    uiText.text = uiTextString;
                }
                
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
