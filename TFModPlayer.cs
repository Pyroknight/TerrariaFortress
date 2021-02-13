using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaFortress.Items;
using TerrariaFortress.Items.Weapons;
using TerrariaFortress.Projectiles;

namespace TerrariaFortress
{
    public class TFModPlayer : ModPlayer
    {
        public float randomCritCheck = Main.rand.NextFloat();
        public bool randomCrit = false;
        public int randomCritTimer = 0;
        public bool canBeRandomCritted = false;

        /// <summary>
        /// Recently dealt damage by the player.
        /// </summary>
        public float recentDamage = 0;
        /// <summary>
        /// The player's random crit chance.
        /// </summary>
        public float storedRandomCritChance = 0f;
        /// <summary>
        /// An array containing the minimum and maximum possible random crit chance.
        /// </summary>
        public float[] randomCritMinMax = new float[] { 2f, 12f };
        /// <summary>
        /// The max amount of damage stored for determining random crit chance.
        /// </summary>
        public float randomCritStorageCap = 2060f;
        /// <summary>
        /// If the player has crits active.
        /// </summary>
        public bool critBoosted = false;
        /// <summary>
        /// If the player has mini-crits active.
        /// </summary>
        public bool miniCritBoosted = false;
        /// <summary>
        /// If the player is rocket jumping.
        /// </summary>
        public bool isRocketJumping = false;
        /// <summary>
        /// If the player is ubercharged with the Medi Gun.
        /// </summary>
        public bool mediGunUber = false;
        /// <summary>
        /// If the player is ubercharged with the Vaccinator.
        /// </summary>
        public bool vaccinatorUber = false;
        /// <summary>
        /// Different vaccinator resistance modes.
        /// </summary>
        public enum VaccinatorModes
        {
            BulletResist,
            ExplosiveResist,
            FireResist
        }
        /// <summary>
        /// If the player is being ubercharged with the Quick-Fix.
        /// </summary>
        public bool quickFixUber = false;
        /// <summary>
        /// If the player is being ubercharged with the Kritzkrieg.
        /// </summary>
        public bool kritzkriegUber = false;
        /// <summary>
        /// If the player's cursor is over a building.
        /// </summary>
        public bool mouseBuilding = false;

        public virtual bool ItemCritCheck(Player player, TFItem TFItem)
        {
            if (TFItem.canRandomCrit)
            {
                if (player.GetModPlayer<TFModPlayer>().randomCrit)
                {
                    return true;
                }

                if (player.GetModPlayer<TFModPlayer>().critBoosted)
                {
                    return true;
                }
            }

            return false;
        }

        public override void OnEnterWorld(Player player)
        {
            if (player == Main.player[Main.myPlayer])
            {
                if (ModContent.GetInstance<TFConfig>().showTitleExtras)
                {
                    ModContent.GetInstance<TerrariaFortress>().TFUnload();
                }
            }
        }

        public override void PreUpdate()
        {
            #region Random Crit Calculation
            player.GetModPlayer<TFModPlayer>().randomCritCheck = Main.rand.NextFloat();
            if (player == Main.player[Main.myPlayer])
            {
                player.GetModPlayer<TFModPlayer>().canBeRandomCritted = ModContent.GetInstance<TFConfig>().enableRandomCrits;
            }
            if (!player.dead)
            {
                if (player.GetModPlayer<TFModPlayer>().recentDamage > 0f)
                {
                    player.GetModPlayer<TFModPlayer>().recentDamage -= 0.5f;
                }

                player.GetModPlayer<TFModPlayer>().storedRandomCritChance = MathHelper.Clamp((player.GetModPlayer<TFModPlayer>().recentDamage > 0 ? player.GetModPlayer<TFModPlayer>().recentDamage / 82.5f / 2f : 0f) + 2f, player.GetModPlayer<TFModPlayer>().randomCritMinMax[0], player.GetModPlayer<TFModPlayer>().randomCritMinMax[1]);
                
                if (player.GetModPlayer<TFModPlayer>().randomCritCheck <= (player.GetModPlayer<TFModPlayer>().storedRandomCritChance / 100f))
                {
                    player.GetModPlayer<TFModPlayer>().randomCritTimer = 2;
                }

                if (player.GetModPlayer<TFModPlayer>().randomCritTimer > 0)
                {
                    player.GetModPlayer<TFModPlayer>().randomCrit = true;
                    player.GetModPlayer<TFModPlayer>().randomCritTimer--;
                }
                else
                {
                    player.GetModPlayer<TFModPlayer>().randomCrit = false;
                }

                if (!ModContent.GetInstance<TFConfig>().enableRandomCrits)
                {
                    player.GetModPlayer<TFModPlayer>().randomCrit = false;
                }
            }
            #endregion

            #region Rocket Jumping
            if (player.velocity.Y == 0f)
            {
                if (player.GetModPlayer<TFModPlayer>().isRocketJumping)
                {
                    player.velocity.X *= 0.8f;
                    player.GetModPlayer<TFModPlayer>().isRocketJumping = false;
                }
            }
            #endregion
        }

        #region Random Crit Check
        public override void UpdateDead()
        {
            player.GetModPlayer<TFModPlayer>().randomCritTimer = 0;
            player.GetModPlayer<TFModPlayer>().storedRandomCritChance = 0f;
            player.GetModPlayer<TFModPlayer>().randomCrit = false;
            player.GetModPlayer<TFModPlayer>().isRocketJumping = false;
            player.GetModPlayer<TFModPlayer>().critBoosted = false;
            player.GetModPlayer<TFModPlayer>().miniCritBoosted = false;
        }

        public virtual void AddRecentDamage(float damage)
        {
            if (player.GetModPlayer<TFModPlayer>().recentDamage + damage <= player.GetModPlayer<TFModPlayer>().randomCritStorageCap)
            {
                player.GetModPlayer<TFModPlayer>().recentDamage += damage;
            }
            else
            {
                player.GetModPlayer<TFModPlayer>().recentDamage += player.GetModPlayer<TFModPlayer>().randomCritStorageCap - player.GetModPlayer<TFModPlayer>().recentDamage;
            }
        }

        #region Add Recent Damage
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            if (target.CanBeChasedBy())
            {
                if (item.modItem is TFItem)
                {
                    AddRecentDamage(damage);
                }
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (target.CanBeChasedBy())
            {
                if (proj.modProjectile is TFProjectile)
                {
                    AddRecentDamage(damage);
                }
            }
        }

        public override void OnHitPvp(Item item, Player target, int damage, bool crit)
        {
            if (item.modItem is TFItem)
            {
                AddRecentDamage(damage);
            }
        }

        public override void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit)
        {
            if (proj.modProjectile is TFProjectile)
            {
                AddRecentDamage(damage);
            }
        }
        #endregion

        #region Remove Vanilla Crits
        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (item.modItem is TFItem)
            {
                crit = false;
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (proj.modProjectile is TFProjectile)
            {
                crit = false;
            }
        }

        public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
        {
            if (item.modItem is TFItem)
            {
                crit = false;
            }
        }

        public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
        {
            if (proj.modProjectile is TFProjectile)
            {
                crit = false;
            }
        }
        #endregion
        #endregion
    }

    public class HoldoutDrawLayer : ModPlayer
    {
        public bool pilotSoundCheck = false;
        public bool meleeRotationBuildup = true;
        public float meleeRotationFactor = 0f;
        public float meleeSwingBackRotation = 180f;

        public override void PostUpdate()
        {
            if (player.HeldItem.modItem is TFItem TFItem)
            {
                if (TFItem.isMelee)
                {
                    void MeleeSwingAnimation()
                    {
                        float toMouse = 0f;
                        if (player == Main.player[Main.myPlayer])
                        {
                            if (ModContent.GetInstance<TFConfig>().shouldMeleeHoldoutRotate)
                            {
                                player.GetModPlayer<HoldoutDrawLayer>().meleeSwingBackRotation = 180f;
                                player.ChangeDir((Main.MouseWorld.X > player.MountedCenter.X).ToDirectionInt());
                                toMouse = ((Main.MouseWorld - player.MountedCenter).ToRotation() + MathHelper.ToRadians((player.direction == -1 ? 180f : 0f) * player.direction));
                            }
                            else
                            {
                                player.GetModPlayer<HoldoutDrawLayer>().meleeSwingBackRotation = 135f;
                            }
                        }

                        if (player.itemAnimation == player.HeldItem.useAnimation - 1 || player.itemAnimation == 1 || player.itemAnimation == 0)
                        {
                            player.GetModPlayer<HoldoutDrawLayer>().meleeRotationBuildup = true;
                            player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor = 0f;
                            player.itemRotation = player.fullRotation + (player.direction == -1 ? MathHelper.ToRadians(90f) : MathHelper.ToRadians(270f)) + toMouse + MathHelper.ToRadians(0f) * player.direction;
                        }
                        else
                        {
                            if (player == Main.player[Main.myPlayer])
                            {
                                if (ModContent.GetInstance<TFConfig>().shouldRangedHoldoutRotate)
                                {
                                    player.ChangeDir((Main.MouseWorld.X > player.MountedCenter.X).ToDirectionInt());
                                }
                            }

                            if (player.itemAnimation != player.HeldItem.useAnimation - 1)
                            {
                                if (player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor < player.GetModPlayer<HoldoutDrawLayer>().meleeSwingBackRotation)
                                {
                                    player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor += 5f + player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor * (100f / (player.HeldItem.useAnimation != 0f ? player.HeldItem.useAnimation : 1f) * 0.025f) * 3f;
                                }
                                else
                                {
                                    player.GetModPlayer<HoldoutDrawLayer>().meleeRotationBuildup = false;
                                }

                                if (!player.GetModPlayer<HoldoutDrawLayer>().meleeRotationBuildup)
                                {
                                    player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor -= (player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor - player.GetModPlayer<HoldoutDrawLayer>().meleeSwingBackRotation) * 0.2f;
                                }
                                player.itemRotation = (player.direction == -1 ? MathHelper.ToRadians(90f - player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor) : MathHelper.ToRadians(270f + player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor)) + toMouse;
                            }
                        }

                        if (player.itemAnimation != 0)
                        {
                            if (player.itemAnimation < player.itemAnimationMax * 0.777f)
                            {
                                player.bodyFrame.Y = player.bodyFrame.Height * 3;
                            }
                            else if (player.itemAnimation < player.itemAnimationMax * 0.888f)
                            {
                                player.bodyFrame.Y = player.bodyFrame.Height * 2;
                            }
                            else if (player.itemAnimation > player.itemAnimationMax * 0.888f)
                            {
                                player.bodyFrame.Y = player.bodyFrame.Height;
                            }
                        }

                        if (TFItem.isSwingMelee && player.itemAnimation == 0)
                        {
                            player.bodyFrame.Y = player.bodyFrame.Height * 1;
                        }
                    }
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        if (!Main.gamePaused)
                        {
                            MeleeSwingAnimation();
                        }
                    }
                    else
                    {
                        MeleeSwingAnimation();
                    }
                }

                if (TFItem.isRanged)
                {
                    void RangedHoldoutFrame()
                    {
                        if (player == Main.player[Main.myPlayer])
                        {
                            if (ModContent.GetInstance<TFConfig>().shouldRangedHoldoutRotate)
                            {
                                player.ChangeDir((Main.MouseWorld.X > player.MountedCenter.X).ToDirectionInt());
                                Vector2 toMouse = Main.MouseWorld - player.MountedCenter;
                                float rotation = -(float)Math.Atan2(toMouse.X * player.direction, toMouse.Y * player.direction) + MathHelper.ToRadians(90f) - player.fullRotation;
                                player.itemRotation = rotation;
                                rotation *= player.direction;
                                player.bodyFrame.Y = player.bodyFrame.Height * 3;
                                if (rotation < -0.75f)
                                {
                                    player.bodyFrame.Y = player.bodyFrame.Height * 2;
                                    if (player.gravDir == -1f)
                                    {
                                        player.bodyFrame.Y = player.bodyFrame.Height * 4;
                                    }
                                }
                                if (rotation > 0.6f)
                                {
                                    player.bodyFrame.Y = player.bodyFrame.Height * 4;
                                    if (player.gravDir == -1f)
                                    {
                                        player.bodyFrame.Y = player.bodyFrame.Height * 2;
                                    }
                                }
                            }
                            else if (player.itemAnimation == 0)
                            {
                                player.itemRotation = 0f;
                                player.bodyFrame.Y = player.bodyFrame.Height * 3;
                            }
                        }
                    }
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        if (!Main.gamePaused)
                        {
                            RangedHoldoutFrame();
                        }
                    }
                    else
                    {
                        RangedHoldoutFrame();
                    }
                }
            }
        }

        public static readonly PlayerLayer HeldItem = new PlayerLayer("TerrariaFortress", "HeldItem", PlayerLayer.HeldItem, delegate (PlayerDrawInfo drawInfo)
        {
            if (drawInfo.shadow != 0f)
            {
                return;
            }

            Player player = drawInfo.drawPlayer;
            bool drawConditions = !player.dead && player.active && !player.invis && !player.frozen && !player.stoned;

            void DrawHoldout()
            {
                if (player.HeldItem.modItem is TFItem TFItem)
                {
                    TFItem.DrawWeaponHoldout(player);
                }
            }

            void DrawAllHoldouts()
            {
                if (drawConditions && !player.pulley)
                {
                    DrawHoldout();
                }
                else if (player.pulley && (player.controlUseItem || player.altFunctionUse == 2))
                {
                    DrawHoldout();
                }
            }

            DrawAllHoldouts();
        });

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            HeldItem.visible = true;
            int index = layers.FindIndex(x => x == PlayerLayer.HeldItem);

            if (index != -1)
            {
                layers.Insert(index, HeldItem);
            }
        }
    }
}