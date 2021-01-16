using Terraria.ModLoader;
using Terraria.ID;
using TerrariaFortress.Projectiles;
using Terraria;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TerrariaFortress.Items;
using Terraria.Enums;
using System.Collections.ObjectModel;
using Terraria.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria.DataStructures;
using TerrariaFortress.Items.Weapons;
using TerrariaFortress.Dusts;
using System.Linq;
using TerrariaFortress.Buffs;
using Microsoft.Xna.Framework.Audio;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TerrariaFortress
{
    public class TerrariaFortress : Mod
    {
        public static readonly Color[] TFColor =
        {
            new Color(178, 178, 178),
            new Color(255, 215, 0),
            new Color(71, 98, 145),
            new Color(77, 116, 85),
            new Color(207, 106, 50),
            new Color(134, 80, 172),
            new Color(56, 243, 171),
            new Color(170, 0, 0),
            new Color(250, 250, 250),
            new Color(112, 176, 74),
            new Color(165, 15, 121),
            new Color(117, 107, 94),
            new Color(153, 204, 255),
            new Color(255, 64, 64),
            new Color(235, 226, 202),
            new Color(0, 160, 0),
            new Color(22, 255, 29),
            new Color(255, 255, 16),
        };

        public enum TFColorID
        {
            Normal,
            Unique,
            Vintage,
            Genuine,
            Strange,
            Unusual,
            Haunted,
            Collectors,
            Decorated,
            CommunityAndSelfMade,
            Valve,
            AttributeLevel,
            AttributePositive,
            AttributeNegative,
            AttributeNeutral,
            AttributeUseLimit,
            CombatCrit,
            CombatMiniCrit,
        }

        public enum StrangeRankType
        {
            Generic,
            HolidayPunch,
            InvisWatch,
            Mantreads,
            Sapper,
            SpiritOfGiving,
            Cosmetic,
            DuckJournal,
            SoulGargoyle
        }

        public static readonly int[] airblastReflectBlacklist =
        {
            ModContent.ProjectileType<FlamethrowerAirblast>(),
            ModContent.ProjectileType<FlamethrowerFlame>(),
            ModContent.ProjectileType<TFBullet>(),
            ProjectileID.VilethornBase,
            ProjectileID.VilethornTip,
            ProjectileID.Bullet,
            ProjectileID.MagicMissile,
            ProjectileID.Flamelash,
            ProjectileID.NettleBurstRight,
            ProjectileID.NettleBurstLeft,
            ProjectileID.NettleBurstEnd,
            ProjectileID.PhantasmalDeathray,
            ProjectileID.LastPrism,
        };

        public class HoldoutDrawLayer : ModPlayer
        {
            public bool pilotSoundCheck = false;
            public bool meleeRotationBuildup = true;
            public float meleeRotationFactor = 0f;
            public float meleeSwingBackRotation = 180f;

            public override void PostUpdate()
            {
                if (player.HeldItem.modItem is TFWeapon TFWeapon)
                {
                    if (TFWeapon.isSwingMelee)
                    {
                        void MeleeSwingAnimation()
                        {
                            float toMouse = 0f;
                            if (player == Main.player[Main.myPlayer])
                            {
                                if (ModContent.GetInstance<TFConfig>().ShouldMeleeHoldoutRotate)
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
                                    if (ModContent.GetInstance<TFConfig>().ShouldRangedHoldoutRotate)
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

                    if (TFWeapon.isRanged)
                    {
                        void RangedHoldoutFrame()
                        {
                            if (player == Main.player[Main.myPlayer])
                            {
                                if (ModContent.GetInstance<TFConfig>().ShouldRangedHoldoutRotate)
                                {
                                    player.ChangeDir((Main.MouseWorld.X > player.MountedCenter.X).ToDirectionInt());
                                    Vector2 toPlayer = Main.MouseWorld - player.MountedCenter;
                                    float rotation = -(float)Math.Atan2(toPlayer.X * player.direction, toPlayer.Y * player.direction) + MathHelper.ToRadians(90f) - player.fullRotation;
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
                Player player = drawInfo.drawPlayer;
                bool drawConditions = !player.dead && player.active && !player.invis && !player.frozen && !player.stoned;

                void DrawAllHoldouts()
                {
                    if (player.HeldItem.modItem is TFWeapon TFWeapon)
                    {
                        if (player.HeldItem.type == ModContent.ItemType<Flamethrower>())
                        {
                            //int R = 255, G = 255, B = 255;
                            //float brightness = 0.15f;
                            //Vector2 blueFlamePos = player.Center + new Vector2(player.direction * 60, player.gfxOffY + 6f).RotatedBy(player.itemRotation + player.fullRotation);

                            //Lighting.AddLight(blueFlamePos, R / 255f * brightness, G / 255f * brightness, B / 255f * brightness);
                            //Dust dust1 = Main.dust[Dust.NewDust(blueFlamePos, 2, 2, 211, 0f, -0.2f, 100, default, 0.5f)];
                            //dust1.noGravity = true;
                            //dust1.noLight = true;

                            //if (!pilotSoundCheck)
                            //{
                            //    Main.PlaySound(SoundLoader.customSoundType, -1, -1, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerPilot"));
                            //    pilotSoundCheck = true;
                            //}
                        }

                        TFWeapon.DrawWeaponHoldout(player);
                    }
                }

                if (drawConditions && !player.pulley)
                {
                    DrawAllHoldouts();
                }
                else if (player.pulley && (player.controlUseItem || player.altFunctionUse == 2))
                {
                    DrawAllHoldouts();
                }

                if (player.HeldItem.type != ModContent.ItemType<Flamethrower>())
                {
                    player.GetModPlayer<HoldoutDrawLayer>().pilotSoundCheck = false;
                }
            });
            public static readonly PlayerLayer Skin = new PlayerLayer("TerrariaFortress", "Skin", PlayerLayer.Skin, delegate (PlayerDrawInfo drawInfo)
            {
                Player player = drawInfo.drawPlayer;
                if (player.HeldItem.modItem is TFWeapon TFWeapon && TFWeapon.isSwingMelee)
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

                    if (player.itemAnimation == 0)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 1;
                    }
                }
            });

            public override void ModifyDrawLayers(List<PlayerLayer> layers)
            {
                HeldItem.visible = true;
                Skin.visible = true;
                int index = layers.FindIndex(x => x == PlayerLayer.HeldItem);
                int index2 = layers.FindIndex(y => y == PlayerLayer.Skin);

                if (index != -1)
                {
                    layers.Insert(index, HeldItem);
                }
                if (index2 != -1)
                {
                    layers.Insert(index2, Skin);
                }
            }
        }

        public class TemporaryItemDeal : GlobalNPC
        {
            public override void SetupShop(int type, Chest shop, ref int nextSlot)
            {
                void AddPurchasable(int itemType, ref int slot)
                {
                    shop.item[slot].SetDefaults(itemType);
                    slot++;
                }

                if (type == NPCID.Demolitionist)
                {
                    if (NPC.downedBoss3)
                    {
                        int[] shopItems =
                        {
                            ModContent.ItemType<AmmoBox>(),
                            ModContent.ItemType<Flamethrower>(),
                            ModContent.ItemType<Shotgun>(),
                            ModContent.ItemType<FireAxe>(),
                            ModContent.ItemType<RocketLauncher>(),
                            ModContent.ItemType<Shovel>(),
                            ModContent.ItemType<Scattergun>(),
                            ModContent.ItemType<Pistol>(),
                            ModContent.ItemType<Bat>(),
                            ModContent.ItemType<GrenadeLauncher>(),
                            ModContent.ItemType<StickybombLauncher>(),
                            ModContent.ItemType<Bottle>(),
                        };

                        for (int i = 0; i < shopItems.Length; i++)
                        {
                            AddPurchasable(shopItems[i], ref nextSlot);
                        }
                    }
                }

                if (type == NPCID.PartyGirl)
                {
                    if (NPC.downedBoss3)
                    {
                        shop.item[nextSlot].SetDefaults(ModContent.ItemType<NoiseMakerBirthday>());
                        nextSlot++;
                    }
                }
            }
        }

        public class AIStats : GlobalItem
        {
            public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
            {
                //if (item.type == ItemID.GPS)
                //{
                //    NPC trackedNPC = Main.npc[NPC.FindFirstNPC(NPCID.Merchant)];
                //    if (trackedNPC.active)
                //    {
                //        tooltips.Add(new TooltipLine(mod, "ItemName", ("[c/FF0000:AI 0: ]" + trackedNPC.ai[0] + "[c/FF0000:,] ") + ("[c/FF0000:AI 1:] " + trackedNPC.ai[1] + "[c/FF0000:,] ") + ("[c/FF0000:AI 2:] " + trackedNPC.ai[2] + "[c/FF0000:,] ") + ("[c/FF0000:AI 3:] " + trackedNPC.ai[3])));
                //    }
                //}
            }
        }
    }
}