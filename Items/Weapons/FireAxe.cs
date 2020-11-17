using Terraria;
using static TerrariaFortress.TerrariaFortress;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace TerrariaFortress.Items.Weapons
{
    public class FireAxe : TFWeapon
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fire Axe");
        }

        public override void SafeSetDefaults()
        {
            TFItemType = "Fire Axe";
            isSwingMelee = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 65;
            item.knockBack = 10;
            item.width = 52;
            item.height = 46;
            item.useTime = 49;
            item.useAnimation = 49;
            item.value = 200000;
        }

        public override void SetConstantDefaults()
        {
            basicUseSound = "MeleeSwing";
            meleeEntityCollisionSound = "AxeFleshHit" + Main.rand.Next(1, 4);
            meleeWorldCollisionSound = "WorldMetalHit" + Main.rand.Next(1, 3);
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            hitbox = TFMeleeHitbox(player, 0f, -36f, 36, 56);
            base.UseItemHitbox(player, ref hitbox, ref noHitbox);
        }
    }
}