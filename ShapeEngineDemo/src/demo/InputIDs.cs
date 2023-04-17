
using ShapeInput;
using ShapeLib;

namespace ShapeEngineDemo
{
    public static class InputIDs
    {
        public static readonly uint INPUTMAP_Default = SID.NextID;

        public static readonly uint GROUP_UI = SID.NextID;
        public static readonly uint GROUP_Debug = SID.NextID;
        public static readonly uint GROUP_Settings = SID.NextID;
        public static readonly uint GROUP_Player = SID.NextID;
        public static readonly uint GROUP_Level = SID.NextID;


        public static readonly uint OPTIONS_Quit                    = SID.NextID;
        public static readonly uint OPTIONS_Fullscreen              = SID.NextID;
        public static readonly uint OPTIONS_CycleRes                = SID.NextID;
        public static readonly uint OPTIONS_NextMonitor             = SID.NextID;
        public static readonly uint OPTIONS_Vsync                   = SID.NextID;
        public static readonly uint OPTIONS_CycleFrameRateLimit     = SID.NextID;

        public static readonly uint GAME_Pause = SID.NextID;
        public static readonly uint GAME_SlowTime = SID.NextID;
               
        public static readonly uint PLAYER_RotateLeft = SID.NextID;
        public static readonly uint PLAYER_RotateRight = SID.NextID;
        public static readonly uint PLAYER_Rotate = SID.NextID;
        public static readonly uint PLAYER_Boost = SID.NextID;
        public static readonly uint PLAYER_Slow = SID.NextID;
        public static readonly uint PLAYER_CycleGuns = SID.NextID;
        public static readonly uint PLAYER_Shoot = SID.NextID;
        public static readonly uint PLAYER_DropAimPoint = SID.NextID;
               
        public static readonly uint DEBUG_HealPlayer = SID.NextID;
        public static readonly uint DEBUG_SpawnAsteroid = SID.NextID;
        public static readonly uint DEBUG_ToggleDrawHelpers = SID.NextID;
        public static readonly uint DEBUG_ToggleDrawColliders = SID.NextID;
        public static readonly uint DEBUG_CycleZoom = SID.NextID;

        public static readonly uint UI_Pressed = SID.NextID;
        public static readonly uint UI_MousePressed = SID.NextID;
        public static readonly uint UI_Cancel = SID.NextID;
        public static readonly uint UI_Up = SID.NextID;
        public static readonly uint UI_Down = SID.NextID;
        public static readonly uint UI_Left = SID.NextID;
        public static readonly uint UI_Right = SID.NextID;
    }
}
