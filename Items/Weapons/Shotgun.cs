using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
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

        public override void SafeSetDefaults()
        {
            TFItemType = "Shotgun";
            isRanged = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 60;
            item.knockBack = 10;
            item.width = 38;
            item.height = 50;
            item.useTime = 38;
            item.useAnimation = 38;
            item.shootSpeed = 64f;
            item.shoot = ProjectileID.Bullet;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 spawningPosition = (player.MountedCenter + new Vector2(player.direction * 42, -6f).RotatedBy(player.itemRotation + player.fullRotation));

            if (Collision.CanHitLine(player.MountedCenter, 0, 0, spawningPosition, 0, 0))
            {
                int desiredProjectiles = 10;
                float spreadFactor = 0.3f;
                switch (desiredProjectiles % 2 == 0)
                {
                    case true:
                        for (int i = 0; i < desiredProjectiles; i++)
                        {
                            Projectile.NewProjectile(spawningPosition, new Vector2(speedX, speedY).RotatedBy(MathHelper.ToRadians((-desiredProjectiles / 2 * spreadFactor) + i * spreadFactor)), type, (int)(damage * 1.5f) / desiredProjectiles, knockBack, player.whoAmI);
                        }
                        break;
                    case false:
                        for (int i = 0; i < desiredProjectiles; i++)
                        {
                            Projectile.NewProjectile(spawningPosition, new Vector2(speedX, speedY).RotatedBy(MathHelper.ToRadians((-desiredProjectiles / 2 * spreadFactor) + i * spreadFactor)), type, (int)(damage * 1.5f) / desiredProjectiles, knockBack, player.whoAmI);
                        }
                        break;
                }
            }

            return false;
        }

        public override bool CanUseItem(Player player)
        {
            Vector2 spawningPosition = (player.MountedCenter + new Vector2(player.direction * 42, 0f).RotatedBy(player.itemRotation + player.fullRotation));

            if (!Collision.CanHitLine(player.MountedCenter, 0, 0, spawningPosition, 0, 0))
            {
                return false;
            }

            if (player.CountItem(item.useAmmo, 1) >= 1)
            {
                if (Collision.CanHitLine(player.MountedCenter, 0, 0, spawningPosition, 0, 0))
                {
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.Center.X, (int)player.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/ShotgunShoot"));
                    player.ConsumeItem(item.useAmmo);
                }
            }

            return base.CanUseItem(player);
        }
    }
}