using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaFortress.Projectiles;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items.Weapons
{
    public class Scattergun : TFWeapon
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scattergun");
        }

        public override void TFDefaults()
        {
            TFItemType = "Scattergun";
            isRanged = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 60;
            item.knockBack = 5f;
            item.width = 38;
            item.height = 50;
            item.useTime = 37;
            item.useAnimation = 37;
            item.shootSpeed = 5f;
            item.shoot = ModContent.ProjectileType<TFBullet>();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 spawnPosition = ShootSpawnPos(player, 38f, -4f) + new Vector2(player.direction * -6f, 0f); ;
            if (Collision.CanHitLine(player.MountedCenter, 0, 0, spawnPosition, 0, 0))
            {
                if (player.CountItem(item.useAmmo, 1) >= 1)
                {
                    float projectiles = 10; 
                    float rotation = 5f;
                    spawnPosition += Vector2.Normalize(new Vector2(speedX, speedY)) * rotation;
                    rotation = MathHelper.ToRadians(rotation);
                    for (int i = 0; i < projectiles; i++)
                    {
                        Vector2 perturbedSpeed = new Vector2(speedX, speedY).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (projectiles - 1)));
                        Projectile projectile = Projectile.NewProjectileDirect(spawnPosition, perturbedSpeed, type, (int)((damage * 1.75f) / projectiles), knockBack, player.whoAmI);
                    }
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/ScattergunShoot"));
                    player.ConsumeItem(item.useAmmo);
                }
            }
            else if (player.CountItem(item.useAmmo, 1) >= 1)
            {
                spawnPosition = position;

                float projectiles = 10;
                float rotation = 5f;
                spawnPosition += Vector2.Normalize(new Vector2(speedX, speedY)) * rotation;
                rotation = MathHelper.ToRadians(rotation);
                for (int i = 0; i < projectiles; i++)
                {
                    Vector2 perturbedSpeed = new Vector2(speedX, speedY).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (projectiles - 1)));
                    Projectile projectile = Projectile.NewProjectileDirect(spawnPosition, perturbedSpeed, type, (int)((damage * 1.75f) / projectiles), knockBack, player.whoAmI);
                }
                Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/ScattergunShoot"));
                player.ConsumeItem(item.useAmmo);
            }

            return false;
        }

        public override void DrawWeaponHoldout(Player player)
        {
            if (player.HeldItem.modItem == this)
            {
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/ScattergunHoldout"), new Vector2(-6f, 0f), new Vector2(0f, 0f), player.itemRotation, RangedWeaponColor(player), player);
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/Details/ScattergunFlash1"), new Vector2(-6f, 0f), new Vector2(-62f, 4f), player.itemRotation, GlowmaskColor(player), player, 1, 5, 0, MuzzleFrame(player, 5, 30));
            }
        }
    }
}