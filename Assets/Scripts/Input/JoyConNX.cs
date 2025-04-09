#if UNITY_SWITCH
using System;
using nn.hid;
using nn.util;
using UnityEngine;

namespace Input
{
    public class JoyConNx : IJoyController
    {
        private readonly NpadId _npadId;
        private NpadStyle _npadStyle;
        private NpadState _npadState;
        private bool _connectedAtLeastOnce;
        private SixAxisSensorState _sixAxisSensorState;
        private readonly SixAxisSensorHandle[] _handle = new SixAxisSensorHandle[2];
        private Vector3 _currentAcceleration;
        private Vector3 _currentAngle;
        private Vector3 _lastAcceleration;
        private int _handleCount;
        private int _duplicatedAccelerationCount;
        private bool _isMaster;

        private JoyConNx(NpadId npadId)
        {
            _npadId = npadId;
            _isMaster = false;
        }

        public static bool Initialize(ref IJoyController[] players)
        {
            Npad.Initialize();
            Npad.SetSupportedIdType(new[]
                { NpadId.No1, NpadId.No2, NpadId.No3, NpadId.No4, NpadId.No5, NpadId.No6 });
            Npad.SetSupportedStyleSet(NpadStyle.JoyLeft | NpadStyle.JoyRight);

            players[0] = new JoyConNx(NpadId.No1);
            players[1] = new JoyConNx(NpadId.No2);
            players[2] = new JoyConNx(NpadId.No3);
            players[3] = new JoyConNx(NpadId.No4);
            players[4] = new JoyConNx(NpadId.No5);
            players[5] = new JoyConNx(NpadId.No6);
            return true;
        }

        public void Update()
        {
            if (GetState(NpadStyle.JoyLeft) || GetState(NpadStyle.JoyRight))
            {
            }

            var sixAxisSensor = UpdateSixAxisSensor();

            _currentAcceleration = sixAxisSensor.Item1;
            _currentAngle = sixAxisSensor.Item2;
        }

        private bool GetState(NpadStyle style)
        {
            var joyState = _npadState;
            Npad.GetState(ref joyState, _npadId, style);
            if (joyState.buttons == NpadButton.None) return false;
            GetSixAxisSensor(_npadId, style);
            _npadState = joyState;
            _npadStyle = style;
            return true;
        }

        private void GetSixAxisSensor(NpadId id, NpadStyle style)
        {
            for (var i = 0; i < _handleCount; i++)
            {
                SixAxisSensor.Stop(_handle[i]);
            }

            _handleCount = SixAxisSensor.GetHandles(_handle, 2, id, style);

            for (var i = 0; i < _handleCount; i++)
            {
                SixAxisSensor.Start(_handle[i]);
            }
        }

        private (Vector3, Vector3, Vector3) UpdateSixAxisSensor()
        {
            for (var i = 0; i < _handleCount;)
            {
                SixAxisSensor.GetState(ref _sixAxisSensorState, _handle[i]);
                var acceleration = new Vector3(_sixAxisSensorState.acceleration.x, _sixAxisSensorState.acceleration.y,
                    _sixAxisSensorState.acceleration.z);
                
                var angle = new Vector3(_sixAxisSensorState.angle.x, _sixAxisSensorState.angle.y, _sixAxisSensorState.angle.z);

                var angularVelocity = new Vector3(_sixAxisSensorState.angularVelocity.x, _sixAxisSensorState.angularVelocity.y, _sixAxisSensorState.angularVelocity.z);

                if (_lastAcceleration != acceleration)
                {
                    _duplicatedAccelerationCount = 0;
                    _lastAcceleration = acceleration;
                }
                else
                {
                    _duplicatedAccelerationCount++;
                }

                return (acceleration, angle, angularVelocity);
            }

            return (Vector3.zero, Vector3.zero, Vector3.zero);
        }

        public void Clear() => _npadState.Clear();

        public bool IsConnected()
        {
            var isConnected = (_npadState.attributes & NpadAttribute.IsConnected) != 0;
            if (isConnected)
            {
                _connectedAtLeastOnce = true;
                _duplicatedAccelerationCount = 0;
                return true;
            }

            if (!_connectedAtLeastOnce) return false;

            if (_duplicatedAccelerationCount > 60)
            {
                _connectedAtLeastOnce = false;
            }
            else
            {
                return true;
            }

            return false;
        }

        public string GetId() => _npadId.ToString();

        public bool IsMaster() => _isMaster;

        public bool SetAsMaster(bool isMaster) => _isMaster = isMaster;

        public Vector3 GetAcceleration() => _currentAcceleration;
        
        public Vector3 GetAngle() => _currentAngle;

        private bool GetKeyCodeFromId(int joyLeft, int joyRight)
        {
            KeyCode keyCode;
            var id = _npadId.ToString()[^1];
            return _npadStyle switch
            {
                NpadStyle.JoyLeft => UnityEngine.Input.GetKeyDown(
                    Enum.TryParse($"Joystick{id}Button{joyLeft}", out keyCode)
                        ? keyCode
                        : KeyCode.None),
                NpadStyle.JoyRight => UnityEngine.Input.GetKeyDown(
                    Enum.TryParse($"Joystick{id}Button{joyRight}", out keyCode)
                        ? keyCode
                        : KeyCode.None),
                _ => false
            };
        }

        public bool Undo() => GetKeyCodeFromId(12, 0); // Down or B

        public bool Select() => GetKeyCodeFromId(13, 1); // Right or A

        public bool Up()
        {
            return _npadStyle switch
            {
                NpadStyle.JoyLeft => _npadState.GetButton(NpadButton.StickLUp),
                NpadStyle.JoyRight => _npadState.GetButton(NpadButton.StickRUp),
                _ => false
            };
        }

        public bool Down()
        {
            return _npadStyle switch
            {
                NpadStyle.JoyLeft => _npadState.GetButton(NpadButton.StickLDown),
                NpadStyle.JoyRight => _npadState.GetButton(NpadButton.StickRDown),
                _ => false
            };
        }

        public bool Left()
        {
            return _npadStyle switch
            {
                NpadStyle.JoyLeft => _npadState.GetButton(NpadButton.StickLLeft),
                NpadStyle.JoyRight => _npadState.GetButton(NpadButton.StickRLeft),
                _ => false
            };
        }

        public bool Right()
        {
            return _npadStyle switch
            {
                NpadStyle.JoyLeft => _npadState.GetButton(NpadButton.StickLRight),
                NpadStyle.JoyRight => _npadState.GetButton(NpadButton.StickRRight),
                _ => false
            };
        }

        public bool CustomInteractionA() => GetKeyCodeFromId(14, 2); // Left or Y

        public bool CustomInteractionB() => GetKeyCodeFromId(15, 3); // Up or X
    }
}
#endif