using UnityEngine;

namespace Input
{
    public interface IJoyController
    {
        public bool IsConnected();

        public void Update();

        public void Clear();

        public string GetId();

        public bool IsMaster();

        public bool SetAsMaster(bool isMaster);
    
        public Vector3 GetAcceleration();
    
        public bool Undo();

        public bool Select();

        public bool Up();

        public bool Down();

        public bool Left();

        public bool Right();

        public bool CustomInteractionA();

        public bool CustomInteractionB();
    }
}