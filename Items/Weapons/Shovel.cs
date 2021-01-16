using Terraria;
using static TerrariaFortress.TerrariaFortress;
using Microsoft.Xna.Framework;

namespace TerrariaFortress.Items.Weapons
{
    public class Shovel : TFWeapon
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shovel");
        }

        public override void TFDefaults()
        {
            TFItemType = "Shovel";
            isSwingMelee = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 65;
            item.knockBack = 10;
            item.width = 40;
            item.height = 40;
            item.useTime = 49;
            item.useAnimation = 49;
        }

        public override void SetConstantDefaults()
        {
            basicUseSound = "MeleeSwing";
            meleeEntityCollisionSound = "AxeFleshHit" + Main.rand.Next(1, 4);
            meleeWorldCollisionSound = "WorldMetalHit" + Main.rand.Next(1, 3);
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            hitbox = TFMeleeHitbox(player, 0f, -36f, 52, 56);
            base.UseItemHitbox(player, ref hitbox, ref noHitbox);
        }

        public override void DrawWeaponHoldout(Player player)
        {
            if (player.HeldItem.modItem == this)
            {
                DrawSimpleMeleeHoldout(ItemTexture(player), new Vector2(-2f, 4f), new Vector2(0f, -8f), MeleeRotation(player), LightColor(player), player);
            }
        }
    }
}