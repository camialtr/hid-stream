namespace Input
{
    public static class Controllers
    {
        public static bool Initialized { get; private set; } = false;
        public static IJoyController[] Players { get; private set; }

#if UNITY_SWITCH
        public static bool InitializeJoyCon()
        {
            var controllers = new IJoyController[6];
            Initialized = JoyConNx.Initialize(ref controllers);
            Players = controllers;
            return Initialized;
        }
#endif
    }
}