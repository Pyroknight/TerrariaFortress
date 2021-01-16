using Terraria;
using static TerrariaFortress.TerrariaFortress;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace TerrariaFortress.Items.Weapons
{
    public class Bottle : TFWeapon
    {
        public bool broken = false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bottle");
        }

        public override void TFDefaults()
        {
            TFItemType = "Bottle";
            isSwingMelee = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 65;
            item.knockBack = 10;
            item.width = 28;
            item.height = 28;
            item.useTime = 49;
            item.useAnimation = 49;
        }

        public override void TFUpdateInventory(Player player)
        {
            if (player.HeldItem.modItem != this || player.dead)
            {
                broken = false;
            }
        }

        public override void OnHitTile(Player player)
        {
            broken = true;
        }

        public override void SetConstantDefaults()
        {
            basicUseSound = "MeleeSwing";
            meleeEntityCollisionSound = "AxeFleshHit" + Main.rand.Next(1, 4);
            meleeWorldCollisionSound = "BottleWorldHit" + Main.rand.Next(1, 4);
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
                DrawSimpleMeleeHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/Bottle2"), new Vector2(0f, 0f), new Vector2(-2f, -2f), MeleeRotation(player), LightColor(player), player, 1, 2, 0, broken ? 1 : 0);
            }
        }
    }
}