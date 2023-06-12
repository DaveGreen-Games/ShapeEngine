
using Audio;
using Lib;

namespace Demo
{
    public static class SoundIDs
    {
        public static readonly uint UI_Click =      SID.NextID;
        public static readonly uint UI_Hover = SID.NextID;
        public static readonly uint PLAYER_Boost =  SID.NextID;
        public static readonly uint PLAYER_Slow =   SID.NextID;
        public static readonly uint PLAYER_Hurt =   SID.NextID;
        public static readonly uint PLAYER_Die =    SID.NextID;
        public static readonly uint PLAYER_StunEnded = SID.NextID;
        public static readonly uint PLAYER_PowerDown = SID.NextID;
        public static readonly uint PLAYER_PowerUp = SID.NextID;
        public static readonly uint PLAYER_Healed = SID.NextID;
        public static readonly uint PROJECTILE_Pierce = SID.NextID;
        public static readonly uint PROJECTILE_Bounce = SID.NextID;
        public static readonly uint PROJECTILE_Impact = SID.NextID;
        public static readonly uint PROJECTILE_Explosion = SID.NextID;
        public static readonly uint PROJECTILE_Crit = SID.NextID;
        public static readonly uint PROJECTILE_Shoot = SID.NextID;
        public static readonly uint ASTEROID_Die = SID.NextID;

        
        public static readonly uint BUS_SOUND   = SID.NextID;
        public static readonly uint BUS_MUSIC   = SID.NextID;
    }
}
