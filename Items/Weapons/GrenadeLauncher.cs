using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaFortress.Projectiles;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items.Weapons
{
    public class GrenadeLauncher : TFItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Grenade Launcher");
        }

        public override void TFDefaults()
        {
            TFItemType = "Grenade Launcher";
            isRanged = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 100;
            item.knockBack = 10;
            item.width = 44;
            item.height = 54;
            item.useTime = 36;
            item.useAnimation = 36;
            item.shootSpeed = 12.27f;
            item.shoot = ModContent.ProjectileType<GrenadeLauncherGrenade>();
            item.value = Item.buyPrice(gold: 80);
        }

        public override void SetConstantDefaults()
        {
            basicUseSound = TFUseSound("GrenadeLauncherShoot");
        }

        public override void TFShoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 spawnPosition = ShootSpawnPos(player, 27f, -2f);
            if (Collision.CanHitLine(player.MountedCenter, 0, 0, spawnPosition, 0, 0))
            {
                if (player.CountItem(item.useAmmo, 1) >= 1)
                {
                    Projectile projectile = Projectile.NewProjectileDirect(spawnPosition, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);
                    if (projectile.modProjectile is GrenadeLauncherGrenade grenade)
                    {
                        grenade.explosionTimer = 138;
                    }
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
                if (projectile.modProjectile is GrenadeLauncherGrenade grenade)
                {
                    grenade.explosionTimer = 138;
                }
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
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/GrenadeLauncherHoldout"), new Vector2(-4f, 0f), new Vector2(0f, 0f), player.itemRotation, RangedWeaponColor(player), player);
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/Details/GrenadeLauncherFlash1"), new Vector2(-4f, 0f), new Vector2(-56f, 2f), player.itemRotation, GlowmaskColor(player), player, 1, 5, 0, MuzzleFrame(player, 5, 30));
            }
        }
    }
}