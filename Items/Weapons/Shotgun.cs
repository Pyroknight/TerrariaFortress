using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using TerrariaFortress.Projectiles;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items.Weapons
{
    public class Shotgun : TFWeapon
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shotgun");
        }

        public override void TFDefaults()
        {
            TFItemType = "Shotgun";
            isRanged = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 60;
            item.knockBack = 5f;
            item.width = 38;
            item.height = 50;
            item.useTime = 38;
            item.useAnimation = 38;
            item.shootSpeed = 5f;
            item.shoot = ModContent.ProjectileType<TFBullet>();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 spawnPosition = ShootSpawnPos(player, 36f, -2f);
            if (Collision.CanHitLine(player.MountedCenter, 0, 0, spawnPosition, 0, 0))
            {
                if (player.CountItem(item.useAmmo, 1) >= 1)
                {
                    float projectiles = 10;
                    float rotation = 1f;
                    spawnPosition += Vector2.Normalize(new Vector2(speedX, speedY)) * rotation;
                    rotation = MathHelper.ToRadians(rotation);
                    for (int i = 0; i < projectiles; i++)
                    {
                        Vector2 perturbedSpeed = new Vector2(speedX, speedY).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (projectiles - 1)));
                        Projectile projectile = Projectile.NewProjectileDirect(spawnPosition, perturbedSpeed, type, (int)((damage * 1.5f) / projectiles), knockBack, player.whoAmI);
                    }
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/ShotgunShoot"));
                    player.ConsumeItem(item.useAmmo);
                }
            }
            else if (player.CountItem(item.useAmmo, 1) >= 1)
            {
                spawnPosition = position;

                float projectiles = 10;
                float rotation = 1f;
                spawnPosition += Vector2.Normalize(new Vector2(speedX, speedY)) * rotation;
                rotation = MathHelper.ToRadians(rotation);
                for (int i = 0; i < projectiles; i++)
                {
                    Vector2 perturbedSpeed = new Vector2(speedX, speedY).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (projectiles - 1)));
                    Projectile projectile = Projectile.NewProjectileDirect(spawnPosition, perturbedSpeed, ModContent.ProjectileType<TFBullet>(), (int)((damage * 1.5f) / projectiles), knockBack, player.whoAmI);
                }
                Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/ShotgunShoot"));
                player.ConsumeItem(item.useAmmo);
            }

            return false;
        }

        public override void DrawWeaponHoldout(Player player)
        {
            if (player.HeldItem.modItem == this)
            {
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/ShotgunHoldout"), new Vector2(-4f, 2f), new Vector2(0f, 0f), player.itemRotation, RangedWeaponColor(player), player);
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/Details/ShotgunFlash1"), new Vector2(-4f, 2f), new Vector2(-62f, 6f), player.itemRotation, GlowmaskColor(player), player, 1, 5, 0, MuzzleFrame(player, 5, 30));
            }
        }
    }
}