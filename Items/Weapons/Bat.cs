using Terraria;
using static TerrariaFortress.TerrariaFortress;
using Microsoft.Xna.Framework;

namespace TerrariaFortress.Items.Weapons
{
    public class Bat : TFItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bat");
        }

        public override void TFDefaults()
        {
            TFItemType = "Bat";
            isSwingMelee = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 35;
            item.knockBack = 6;
            item.width = 38;
            item.height = 38;
            item.useTime = 30;
            item.useAnimation = 30;
        }

        public override void SetConstantDefaults()
        {
            basicUseSound = TFUseSound("MeleeSwing");
            meleeEntityCollisionSound = "BatFleshHit";
            meleeWorldCollisionSound = "WorldMetalHit" + Main.rand.Next(1, 3);
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            hitbox = TFMeleeHitbox(player, 0f, -30f, 54, 50);
            base.UseItemHitbox(player, ref hitbox, ref noHitbox);
        }

        public override void DrawWeaponHoldout(Player player)
        {
            if (player.HeldItem.modItem == this)
            {
                DrawSimpleMeleeHoldout(ItemTexture(player), new Vector2(0f, 2f), new Vector2(3f, -5f), ItemRotation(player), LightColor(player), player);
            }
        }
    }
}