using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaFortress.Projectiles;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items.Weapons
{
    public class RocketLauncher : TFWeapon
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rocket Launcher");
        }

        public override void SafeSetDefaults()
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

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {

            Projectile.NewProjectile(ShootSpawnPos(player, 30f, 0f) + new Vector2(0f, -6f), new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);

            return false;
        }

        public override bool CanUseItem(Player player)
        {
            Vector2 spawningPosition = (player.MountedCenter + new Vector2(player.direction * 30f, 4f).RotatedBy(player.itemRotation + player.fullRotation) + new Vector2(0f, -10f));

            if (player.CountItem(item.useAmmo, 1) >= 1)
            {
                if (Collision.CanHitLine(player.MountedCenter, 0, 0, spawningPosition, 0, 0))
                {
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/RocketLauncherShoot"));
                    player.ConsumeItem(item.useAmmo);
                }
            }

            return base.CanUseItem(player);
        }
    }
}