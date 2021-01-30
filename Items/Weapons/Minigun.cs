using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaFortress.Projectiles;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items.Weapons
{
    public class Minigun : TFItem
    {
        public int revTimer = 0;
        public int revTimerMax = 52;
        public bool fullyRevved = false;
        public bool revving = false;
        public float minigunRotation = 0f;
        public bool revUp = false;
        public int revFrame = 0;
        public bool windUpSoundCheck = false;
        public bool windDownSoundCheck = false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Minigun");
        }

        public override void TFDefaults()
        {
            TFItemType = "Minigun";
            isRanged = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 36;
            item.knockBack = 10;
            item.width = 60;
            item.height = 62;
            item.useTime = 5;
            item.useAnimation = 5;
            item.shootSpeed = 6f;
            item.shoot = ModContent.ProjectileType<TFBullet>();
            item.value = Item.buyPrice(gold: 80);
        }

        public override void TFUpdateInventory(Player player)
        {
            if (player.HeldItem.modItem == this)
            {
                fullyRevved = revTimer == revTimerMax;
                revving = false;
                if (player == Main.player[Main.myPlayer])
                {
                    if (!Main.blockMouse && !player.mouseInterface)
                    {
                        if (CanUseItem(player))
                        {
                            if (Main.mouseRight)
                            {
                                revving = true;
                            }
                            if (Main.mouseLeft)
                            {
                                revving = true;
                            }
                        }
                    }

                    if (revUp && Math.Abs(player.velocity.Y) <= 0.01f && (!player.mount.Active))
                    {
                        player.moveSpeed -= 0.63f;
                        player.velocity.X *= 0.98f;
                    }

                    if (revTimer == 0)
                    {
                        if (revving)
                        {
                            revUp = true;
                            if (!windUpSoundCheck)
                            {
                                Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/MinigunWindUp"));
                                windUpSoundCheck = true;
                            }
                            windDownSoundCheck = false;
                        }
                    }

                    if (revUp)
                    {
                        if (revTimer < revTimerMax)
                        {
                            revTimer++;
                        }
                    }
                    else
                    {
                        if (revving)
                        {
                            revTimer = 0;
                        }
                        if (revTimer > 0)
                        {
                            revTimer--;
                        }
                    }

                    if (fullyRevved)
                    {
                        if (!revving)
                        {
                            revUp = false;
                            if (!windDownSoundCheck)
                            {
                                Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/MinigunWindDown"));
                                windDownSoundCheck = true;
                            }
                            windUpSoundCheck = false;
                        }
                    }
                }
            }
            else
            {
                revTimer = 0;
                revUp = false;
                windDownSoundCheck = false;
                windUpSoundCheck = false;
            }
        }

        public override void SetConstantDefaults()
        {
            basicUseSound = TFUseSound("MinigunShoot");
        }

        public override void CritDamage(ref float value)
        {
            value *= 2;
        }
        public override void CritDamage(ref int value)
        {
            value *= 2;
        }

        public override bool ShootSoundConditions()
        {
            return fullyRevved;
        }

        public override void TFShoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 speed = new Vector2(speedX, speedY);
            if (fullyRevved)
            {
                Vector2 spawnPosition = ShootSpawnPos(player, 34f, 0f) + new Vector2(player.direction * -4f, 0f);
                if (Collision.CanHitLine(player.MountedCenter, 0, 0, spawnPosition, 0, 0))
                {
                    if (player.CountItem(item.useAmmo, 1) >= 1) 
                    {
                        float projectiles = 4;
                        float rotation = 35f;
                        rotation = MathHelper.ToRadians(rotation);
                        for (int i = 0; i < projectiles; i++)
                        {
                            Vector2 perturbedSpeed = new Vector2(speedX, speedY).RotatedBy(MathHelper.Lerp(-rotation, rotation, i / (projectiles - 1)));
                            Projectile projectile = Projectile.NewProjectileDirect(spawnPosition + perturbedSpeed, speed, type, (int)((damage * 1.5f) / projectiles), knockBack, player.whoAmI);
                            if (critting)
                            {
                                if (projectile.modProjectile is TFProjectile TFProjectile)
                                {
                                    TFProjectile.CritBoost();
                                    TFProjectile.critDamageMultiplier = 2f;
                                }
                            }
                        }
                        player.ConsumeItem(item.useAmmo);
                    }
                }
                else if (player.CountItem(item.useAmmo, 1) >= 1)
                {
                    spawnPosition = position;

                    float projectiles = 4;
                    for (int i = 0; i < projectiles; i++)
                    {
                        Projectile projectile = Projectile.NewProjectileDirect(spawnPosition, speed, type, (int)((damage * 1.5f) / projectiles), knockBack, player.whoAmI);
                        if (critting)
                        {
                            if (projectile.modProjectile is TFProjectile TFProjectile)
                            {
                                TFProjectile.CritBoost();
                                TFProjectile.critDamageMultiplier = 2f;
                            }
                        }
                    }
                    player.ConsumeItem(item.useAmmo);
                }
            }
        }

        public override void DrawWeaponHoldout(Player player)
        {
            if (player.HeldItem.modItem == this)
            {
                if (revTimer > 0)
                {
                    if (Main.time % 3 == 0)
                    {
                        revFrame = (int)Main.time % 4;
                    }
                }
                minigunRotation = MathHelper.Clamp(MathHelper.ToRadians((revTimer * (1f / revTimerMax) * (revUp ? 4f : 2f)) * 90f - 45f + (revUp ? 0f : -135f)), MathHelper.ToRadians(-45f), MathHelper.ToRadians(0f));
                DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/MinigunHoldout"), new Vector2(-2f, 2f), new Vector2(0f, 0f), ItemRotation(player, minigunRotation), RangedWeaponColor(player), player, 1, 4, 0, revFrame);
                if (fullyRevved)
                {
                    DrawSimpleHoldout(ModContent.GetTexture("TerrariaFortress/Items/Weapons/Details/MinigunFlash1"), new Vector2(-2f, 2f), new Vector2(-68f, 4f), ItemRotation(player, minigunRotation), GlowmaskColor(player), player, 1, 5, 0, (int)MathHelper.Clamp(Math.Abs(player.itemTime - item.useTime), 0f, 4f));
                }
            }
        }
    }
}