using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using TerrariaFortress.Projectiles;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items.Weapons
{
    public class Pistol : TFWeapon
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pistol");
        }

        public override void TFDefaults()
        {
            TFItemType = "Pistol";
            isRanged = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 15;
            item.knockBack = 5f;
            item.width = 38;
            item.height = 50;
            item.useTime = 9;
            item.useAnimation = 9;
            item.shootSpeed = 5f;
            item.shoot = ModContent.ProjectileType<TFBullet>();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 spawnPosition = ShootSpawnPos(player, 6f, -4f);
            if (Collision.CanHitLine(player.MountedCenter, 0, 0, spawnPosition, 0, 0))
            {
                if (player.CountItem(item.useAmmo, 1) >= 1)
                {
                    Projectile projectile = Projectile.NewProjectileDirect(spawnPosition, new Vector2(speedX, speedY), type, (int)(damage * 1.5f), knockBack, player.whoAmI);
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/PistolShoot"));
                    player.ConsumeItem(item.useAmmo);
                }
            }
            else if (player.CountItem(item.useAmmo, 1) >= 1)
            {
                spawnPosition = position;

                Projectile projectile = Projectile.NewProjectileDirect(spawnPosition, new Vector2(speedX, speedY), type, (int)(damage * 1.5f), knockBack, player.whoAmI);
                Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/PistolShoot"));
                player.ConsumeItem(item.useAmmo);
            }

            return false;
        }

        public override void DrawWeaponHoldout(Player player)
        {
            if (player.HeldItem.modItem == this)
            {
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/PistolHoldout"), new Vector2(0f, 2f), new Vector2(0f, 0f), player.itemRotation, RangedWeaponColor(player), player);
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/Details/PistolFlash1"), new Vector2(0f, 2f), new Vector2(-26f, 7f), player.itemRotation, GlowmaskColor(player), player, 1, 5, 0, MuzzleFrame(player, 5, 45));
            }
        }
    }
}   