using Input;
using nn.swkbd;
using System.Net;
using UnityEngine;
using System.Text;
using UnityEngine.UI;

namespace Scenes
{
    public class Recorder : MonoBehaviour
    {
        public Text joyInfo;
        public Text connectionInfo;
        public Text console;
        public Text connectOrDisconnectInfo;
        private string _currentIp;
        private Client _client;

        private Color _regularColor;
        private Color _errorColor;

        private void Start()
        {
            if (!Controllers.Initialized) Controllers.InitializeJoyCon();
            ColorUtility.TryParseHtmlString("#FFF2C4", out _regularColor);
            ColorUtility.TryParseHtmlString("#FF9D96", out _errorColor);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.JoystickButton2) ||
                UnityEngine.Input.GetKeyDown(KeyCode.JoystickButton14))
            {
                var stringBuilder = new StringBuilder();
                var keyboardArg = new ShowKeyboardArg();
                keyboardArg.keyboardConfig.keyboardMode = KeyboardMode.Alphabet;
                Swkbd.Initialize(ref keyboardArg);
                Swkbd.ShowKeyboard(stringBuilder, keyboardArg);

                var currentIp = stringBuilder.ToString();
                string connectionInfoText;

                if (IPAddress.TryParse(currentIp, out var _))
                {
                    _currentIp = currentIp;
                    connectionInfoText = $"IP: {currentIp}";
                    
                    console.text = "Console: The informed IP was set successfully";
                    console.color = _regularColor;
                }
                else
                {
                    _currentIp = string.Empty;
                    connectionInfoText = "IP: none";
                    
                    console.text = "Console: The informed IP is invalid";
                    console.color = _errorColor;
                }

                connectionInfoText += $"\n\nDisconnected";

                connectionInfo.text = connectionInfoText;
            }
            
            if (UnityEngine.Input.GetKeyDown(KeyCode.Joystick1Button0) || 
                UnityEngine.Input.GetKeyDown(KeyCode.Joystick1Button12))
            {
                if (_client is null || !_client.Connected)
                {
                    _client = new Client(_currentIp, 14444);
                    if (_client.Connected)
                    {
                        connectionInfo.text = $"IP: {_currentIp}\n\nConnected";
                        console.text = "Console: Connected to the server";
                        console.color = _regularColor;
                        connectOrDisconnectInfo.text = "B / Down  -  Disconnect from server";
                    }
                    else
                    {
                        connectionInfo.text = $"IP: {_currentIp}\n\nDisconnected";
                        console.text = "Console: Failed when trying to connect to the server";
                        console.color = _errorColor;
                        connectOrDisconnectInfo.text = "B / Down  -  Connect to server";
                    }
                }
                else
                {
                    _client.Disconnect();
                    connectionInfo.text = $"IP: {_currentIp}\n\nDisconnected";
                    console.text = "Console: Disconnected from the server";
                    console.color = _regularColor;
                    connectOrDisconnectInfo.text = "B / Down  -  Connect to server";
                }
            }

            for (var i = 0; i < Controllers.Players.Length; i++)
            {
                Controllers.Players[i].Update();

                IsPlayerConnectedAndAskingForControl(Controllers.Players[i]);

                if (Controllers.Players[i].IsMaster())
                {
                    var joyInfoText = string.Empty;

                    joyInfoText += $"Joy-Con {i + 1} is set as the target controller";
                    
                    var acceleration = Controllers.Players[i].GetAcceleration();
                    
                    var accelX = $"{acceleration.x:F9}".PadRight(9, '0')[..9];
                    var accelY = $"{acceleration.y:F9}".PadRight(9, '0')[..9];
                    var accelZ = $"{acceleration.z:F9}".PadRight(9, '0')[..9];
                    
                    var angle = Controllers.Players[i].GetAngle();
                    
                    var angleX = $"{angle.x:F9}".PadRight(9, '0')[..9];
                    var angleY = $"{angle.y:F9}".PadRight(9, '0')[..9];
                    var angleZ = $"{angle.z:F9}".PadRight(9, '0')[..9];
                    
                    joyInfoText += $"\n\nAcceleration: {accelX}, {accelY}, {accelZ}";
                    joyInfoText += $"\n\nAngle: {angleX}, {angleY}, {angleZ}";

                    if (_client is not null && _client.Connected)
                    {
                        var rawData = $"{accelX}|{accelY}|{accelZ}|{angleX}|{angleY}|{angleZ}"; // Fixed length - 59
                        
                        _client.SendMessage(rawData);
                    }

                    joyInfo.text = joyInfoText;
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