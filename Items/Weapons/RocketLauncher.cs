using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaFortress.Projectiles;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items.Weapons
{
    public class RocketLauncher : TFItem
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rocket Launcher");
        }

        public override void TFDefaults()
        {
            TFItemType = "Rocket Launcher";
            isRanged = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 90;
            item.knockBack = 10;
            item.width = 50;
            item.height = 50;
            item.useTime = 48;
            item.useAnimation = 48;
            item.shootSpeed = 5f;
            item.shoot = ModContent.ProjectileType<RocketLauncherRocket>();
        }

        public override void SetConstantDefaults()
        {
            basicUseSound = TFUseSound("RocketLauncherShoot");
        }

        public override void TFShoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 spawnPosition = ShootSpawnPos(player, 26f, -6f) + new Vector2(player.direction * -12f, 0f);
            if (Collision.CanHitLine(player.MountedCenter, 0, 0, spawnPosition, 0, 0))
            {
                if (player.CountItem(item.useAmmo, 1) >= 1)
                {
                    Projectile projectile = Projectile.NewProjectileDirect(spawnPosition, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);
                    if (critting)
                    {
                        if (projectile.modProjectile is TFProjectile TFProjectile)
                        {
                            TFProjectile.CritBoost();
                        }
                    }
                    player.ConsumeItem(item.useAmmo);
                }
            }
            else if (player.CountItem(item.useAmmo, 1) >= 1)
            {
                spawnPosition = position;

                Projectile projectile = Projectile.NewProjectileDirect(spawnPosition, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);
                if (critting)
                {
                    if (projectile.modProjectile is TFProjectile TFProjectile)
                    {
                        TFProjectile.CritBoost();
                    }
                }
                player.ConsumeItem(item.useAmmo);
            }
        }

        public override void DrawWeaponHoldout(Player player)
        {
            if (player.HeldItem.modItem == this)
            {
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/RocketLauncherHoldout"), new Vector2(-12f, -2f), new Vector2(0f, 0f), player.itemRotation, RangedWeaponColor(player), player);
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/Details/RocketLauncherFlash1"), new Vector2(-12f, -2f), new Vector2(-62f, 3f), player.itemRotation, GlowmaskColor(player), player, 1, 5, 0, MuzzleFrame(player, 5, 30));
            }
        }
    }
}