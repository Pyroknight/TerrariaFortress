using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TerrariaFortress
{
    [Label("Terraria Fortress Settings")]
    public class TFConfig : ModConfig
    {
        [Header("Visuals")]
        [DefaultValue(true)]
        [Label("Ranged Weapon Holdout Rotation")]
        [Tooltip("If enabled, ranged weapons' holdouts will rotate\nto your cursor and change your player's direction.")]
        public bool shouldRangedHoldoutRotate;
        [DefaultValue(true)]
        [Label("Melee Weapon Holdout Rotation")]
        [Tooltip("If enabled, melee weapons' holdouts will rotate\nto your cursor and change your player's direction.")]
        public bool shouldMeleeHoldoutRotate;

        [Header("Gameplay")]
        [DefaultValue(true)]
        [Label("Random Crits")]
        [Tooltip("If enabled, you can deal random crits\nwith your weapons, as well as take damage from them.")]
        public bool enableRandomCrits;

        public override ConfigScope Mode => ConfigScope.ClientSide;
    }
}