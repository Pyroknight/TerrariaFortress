using Terraria;
using static TerrariaFortress.TerrariaFortress;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace TerrariaFortress.Items.Weapons
{
    public class Bottle : TFItem
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

        public void Break(Player player)
        {
            broken = true;
            for (int i = 0; i < 4; i++)
            {
                Gore shard = Gore.NewGorePerfect(player.MountedCenter, new Vector2(0f, -8f).RotatedByRandom(MathHelper.ToRadians(360f)), mod.GetGoreSlot<Details.BottleShards>());
                shard.frame = (byte)i;
            }
        }

        public override void OnHitTile(Player player)
        {
            if (critting)
            {
                if (!broken)
                {
                    Break(player);
                }
            }
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (critting)
            {
                if (!broken)
                {
                    Break(player);
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + meleeWorldCollisionSound));
                }
            }
        }

        public override void OnHitPvp(Player player, Player target, int damage, bool crit)
        {
            if (critting)
            {
                if (!broken)
                {
                    Break(player);
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + meleeWorldCollisionSound));
                }
            }
        }

        public override void SetConstantDefaults()
        {
            basicUseSound = TFUseSound("MeleeSwing");
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
                DrawSimpleMeleeHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/Bottle2"), new Vector2(0f, 0f), new Vector2(-2f, -2f), ItemRotation(player), LightColor(player), player, 1, 2, 0, broken ? 1 : 0);
            }
        }
    }
}