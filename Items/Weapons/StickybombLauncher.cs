using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaFortress.Projectiles;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items.Weapons
{
    public class StickybombLauncher : TFWeapon
    {
        public int chargeTimer = 240;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Stickybomb Launcher");
        }

        public override void TFDefaults()
        {
            TFItemType = "Stickybomb Launcher";
            isRanged = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 120;
            item.knockBack = 10;
            item.width = 44;
            item.height = 54;
            item.useTime = 36;
            item.useAnimation = 36;
            item.shootSpeed = 8.12f;
            item.shoot = ModContent.ProjectileType<StickybombLauncherStickybomb>();
            item.value = Item.gold * 80;
        }

        public override bool TFCanUseItem(Player player)
        {
            if (player.ownedProjectileCounts[item.shoot] < 8)
            {
                return true;
            }

            return false;
        }

        public override void TFDescription(List<TooltipLine> tooltips)
        {
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeNeutral], "Alt-Fire: Detonate all Stickybombs");
        }

        public override void TFUpdateInventory(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];

                if (projectile.active && projectile.modProjectile is StickybombLauncherStickybomb stickyBomb)
                {
                    if (player == Main.player[Main.myPlayer])
                    {
                        if (stickyBomb.armingTimer == 0 && Main.mouseRight && !player.mouseInterface && !stickyBomb.detonated && !player.dead && player.active)
                        {
                            stickyBomb.detonated = true;
                        }
                    }
                }
            }
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player == Main.player[Main.myPlayer])
            {
                Vector2 spawnPosition = ShootSpawnPos(player, 23f, -5f);
                if (player.altFunctionUse != 2)
                {
                    if (Collision.CanHitLine(player.MountedCenter, 0, 0, spawnPosition, 0, 0))
                    {
                        if (player.CountItem(item.useAmmo, 1) >= 1)
                        {
                            Projectile projectile = Projectile.NewProjectileDirect(spawnPosition, new Vector2(speedX, speedY), type, (int)(damage * 1.2f), knockBack, player.whoAmI);
                            if (projectile.modProjectile is StickybombLauncherStickybomb stickyBomb)
                            {
                                stickyBomb.armingTimer = 42;
                                stickyBomb.detonationTimer = 12;
                            }
                            Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/StickybombLauncherShoot"));
                            player.ConsumeItem(item.useAmmo);
                        }
                    }
                    else if (player.CountItem(item.useAmmo, 1) >= 1)
                    {
                        spawnPosition = position;

                        Projectile projectile = Projectile.NewProjectileDirect(spawnPosition, new Vector2(speedX, speedY), type, (int)(damage * 1.2f), knockBack, player.whoAmI);
                        if (projectile.modProjectile is StickybombLauncherStickybomb stickyBomb)
                        {
                            stickyBomb.detonationTimer = 42;
                            stickyBomb.detonationTimer = 12;
                        }
                        Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/StickybombLauncherShoot"));
                        player.ConsumeItem(item.useAmmo);
                    }
                }
            }

            return false;
        }

        public override void DrawWeaponHoldout(Player player)
        {
            if (player.HeldItem.modItem == this)
            {
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/StickybombLauncherHoldout"), new Vector2(0f, 0f), new Vector2(0f, 0f), player.itemRotation, RangedWeaponColor(player), player);
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/Details/StickybombLauncherFlash1"), new Vector2(0f, 0f), new Vector2(-44f, 5f), player.itemRotation, GlowmaskColor(player), player, 1, 5, 0, MuzzleFrame(player, 5, 30));
            }
        }
    }
}