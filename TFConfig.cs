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
        public bool ShouldRangedHoldoutRotate { get; set; }

        [DefaultValue(true)]
        [Label("Melee Weapon Holdout Rotation")]
        [Tooltip("If enabled, melee weapons' holdouts will rotate\nto your cursor and change your player's direction.")]
        public bool ShouldMeleeHoldoutRotate { get; set; }


        public override ConfigScope Mode => ConfigScope.ClientSide;
    }
}