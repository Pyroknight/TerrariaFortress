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

        public class TFModPlayer : ModPlayer
        {
            public float recentDamage = 0;
            public float randomCritChance = 0f;
            public bool isRocketJumping = false;
            float randomCritStorageCap = 2060;
            public override void PreUpdate()
            {
                if (recentDamage > 0f)
                {
                    recentDamage -= 0.5f;
                }
                randomCritChance = MathHelper.Clamp((recentDamage > 0 ? recentDamage / 82.5f / 2f : 0) + 2, 2f, 12f);

                if (player.velocity.Y == 0)
                {
                    if (isRocketJumping)
                    {
                        player.velocity.X *= 0.8f;
                        isRocketJumping = false;
                    }
                }
            }

            public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
            {
                if (target.CanBeChasedBy())
                {
                    if (item.modItem is TFWeapon)
                    {
                        if (recentDamage + damage < randomCritStorageCap)
                        {
                            recentDamage += damage;
                        }
                        else
                        {
                            recentDamage += randomCritStorageCap - recentDamage;
                        }
                    }
                }
            }
            public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
            {
                if (target.CanBeChasedBy())
                {
                    if (proj.modProjectile is TFProjectile)
                    {
                        if (recentDamage + damage < randomCritStorageCap)
                        {
                            recentDamage += damage;
                        }
                        else
                        {
                            recentDamage += randomCritStorageCap - recentDamage;
                        }
                    }
                }
            }
            public override void OnHitPvp(Item item, Player target, int damage, bool crit)
            {
                if (item.modItem is TFWeapon)
                {
                    if (recentDamage + damage < randomCritStorageCap)
                    {
                        recentDamage += damage;
                    }
                    else
                    {
                        recentDamage += randomCritStorageCap - recentDamage;
                    }
                }
            }
            public override void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit)
            {
                if (proj.modProjectile is TFProjectile)
                {
                    if (recentDamage + damage < randomCritStorageCap)
                    {
                        recentDamage += damage;
                    }
                    else
                    {
                        recentDamage += randomCritStorageCap - recentDamage;
                    }
                }
            }
        }

        public abstract class TFProjectile : ModProjectile
        {
            public virtual bool HitboxCheckPlayer(Player player, Rectangle hitbox, Player playerToIntersect)
            {
                if (hitbox.Intersects(playerToIntersect.Hitbox) && playerToIntersect.whoAmI != player.whoAmI && playerToIntersect.active)
                {
                    if (player.team != (int)Team.None && player.team != playerToIntersect.team && player.hostile == playerToIntersect.hostile && player.hostile)
                    {
                        return true;
                    }
                    else if (player.team == (int)Team.None && player.hostile == playerToIntersect.hostile && player.hostile)
                    {
                        return true;
                    }
                }

                return false;
            }

            public virtual bool HitboxCheckNPC(Player player, Rectangle hitbox, NPC npcToIntersect)
            {
                if (hitbox.Intersects(npcToIntersect.Hitbox) && npcToIntersect.active)
                {
                    if (!npcToIntersect.friendly)
                    {
                        if (npcToIntersect.CanBeChasedBy())
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public bool willCrit = false;
            public bool critBoosted = false;
            public bool miniCritBoosted = false;
            public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
            {
                crit = willCrit;
            }

            public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
            {
                crit = willCrit;
            }
        }

        public abstract class TFWeapon : ModItem
        {
            public string basicUseSound = "";
            public string meleeEntityCollisionSound = "";
            public string meleeWorldCollisionSound = "";
            public string TFItemType = "";
            public bool meleeEntityCollided = false;
            public bool meleeWorldCollided = false;
            public bool meleeEntityCollisionSoundCheck = false;
            public bool meleeWorldCollisionSoundCheck = false;
            public bool isSwingMelee = default;
            public bool isRanged = default;
            public bool isScreamFortressSpell = default;
            public bool isStrange = default;
            public bool killstreaksActive = default;
            public bool contractItem = default;
            public int TFItemQuality = 0;
            public int TFItemLevel;
            public int strangeRankType = (int)StrangeRankType.Generic;
            public string strangeStat = "Kills";
            public string strangeRank = "Strange";
            public int strangePoints = 0;
            float tooltipHeight;
            string longestTooltipLine = "";

            public Vector2 ShootSpawnPos(Player player, float horizontalOffset, float verticalOffset)
            {
                return player.MountedCenter + (new Vector2(player.direction * horizontalOffset, verticalOffset) + new Vector2(4f, 0f)).RotatedBy(player.itemRotation + player.fullRotation);
            }

            public virtual Rectangle TFMeleeHitbox(Player player, float horizontalOffset, float verticalOffset, int width, int height)
            {
                return new Rectangle((int)(player.MountedCenter.X + (player.direction == -1 ? -width - horizontalOffset : horizontalOffset)), (int)(player.MountedCenter.Y + verticalOffset * player.gravDir + (player.gravDir == -1 ? (-height) : 0f)), width, height);
            }   

            public virtual bool HitboxCheckPlayer(Player player, Rectangle hitbox, Player playerToIntersect)
            {
                if (hitbox.Intersects(playerToIntersect.Hitbox) && playerToIntersect.whoAmI != player.whoAmI && playerToIntersect.active)
                {
                    if (player.team != (int)Team.None && player.team != playerToIntersect.team && player.hostile == playerToIntersect.hostile && player.hostile)
                    {
                        return true;
                    }
                    else if (player.team == (int)Team.None && player.hostile == playerToIntersect.hostile && player.hostile)
                    {
                        return true;
                    }
                }

                return false;
            }

            public virtual bool HitboxCheckNPC(Player player, Rectangle hitbox, NPC npcToIntersect)
            {
                if (hitbox.Intersects(npcToIntersect.Hitbox) && npcToIntersect.active)
                {
                    if (!npcToIntersect.friendly)
                    {
                        if (npcToIntersect.CanBeChasedBy())
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public virtual bool CanShootWithOffset(Player player, float horizontalOffset, float verticalOffset)
            {
                Vector2 spawningPosition = (player.MountedCenter + new Vector2(player.direction * horizontalOffset, verticalOffset).RotatedBy(player.itemRotation + player.fullRotation));

                if (Collision.CanHitLine(player.MountedCenter, 0, 0, spawningPosition, 0, 0))
                {
                    return true;
                }

                return false;
            }

            public virtual void AddTFAttribute(List<TooltipLine> tooltips, Color attributeColor, string attributeText)
            {
                int index = tooltips.FindIndex(tooltip => tooltip.Name == "ItemName");

                if (index >= 0 && attributeText != default)
                {
                    TooltipLine attribute = new TooltipLine(mod, "ItemName", attributeText);
                    attribute.overrideColor = attributeColor;
                    tooltips.Add(attribute);
                }
            }

            public virtual void SetConstantDefaults()
            {

            }

            public virtual void SafeSetDefaults()
            {

            }

            public sealed override void SetDefaults()
            {
                SetConstantDefaults();
                SafeSetDefaults();
                if (isSwingMelee)
                {
                    item.useStyle = ItemUseStyleID.SwingThrow;
                    item.melee = true;
                    item.useTurn = true;
                    item.holdStyle = ItemHoldStyleID.HoldingOut;
                    item.value = 200000;

                }
                if (isRanged)
                {
                    item.useStyle = ItemUseStyleID.HoldingOut;
                    item.useAmmo = ModContent.ItemType<AmmoBox>();
                    item.ranged = true;
                    item.noMelee = true;
                    item.holdStyle = ItemHoldStyleID.HoldingOut;
                    item.value = 300000;
                }
                item.magic = isScreamFortressSpell;
                item.noUseGraphic = true;
                item.autoReuse = true;
            }

            public override bool UseItem(Player player)
            {
                if (player.itemAnimation < player.itemAnimationMax * 0.777)
                {
                    player.bodyFrame.Y = player.bodyFrame.Height * 3;
                }
                else if (player.itemAnimation < player.itemAnimationMax * 0.888)
                {
                    player.bodyFrame.Y = player.bodyFrame.Height * 2;
                }
                else
                {
                    player.bodyFrame.Y = player.bodyFrame.Height;
                }
                return true;
            }

            public override void UpdateInventory(Player player)
            {
                SetConstantDefaults();

                switch (strangeRankType)
                {
                    default:
                    case (int)StrangeRankType.Generic:
                        if (strangePoints < 10)
                        {
                            strangeRank = "Strange";
                        }
                        else if (strangePoints >= 10 && strangePoints < 25)
                        {
                            strangeRank = "Unremarkable";
                        }
                        else if (strangePoints >= 25 && strangePoints < 45)
                        {
                            strangeRank = "Scarcely Lethal";
                        }
                        else if (strangePoints >= 45 && strangePoints < 70)
                        {
                            strangeRank = "Mildly Menacing";
                        }
                        else if (strangePoints >= 70 && strangePoints < 100)
                        {
                            strangeRank = "Somewhat Threatening";
                        }
                        else if (strangePoints >= 100 && strangePoints < 135)
                        {
                            strangeRank = "Uncharitable";
                        }
                        else if (strangePoints >= 135 && strangePoints < 175)
                        {
                            strangeRank = "Notably Dangerous";
                        }
                        else if (strangePoints >= 175 && strangePoints < 225)
                        {
                            strangeRank = "Sufficiently Lethal";
                        }
                        else if (strangePoints >= 225 && strangePoints < 275)
                        {
                            strangeRank = "Truly Feared";
                        }
                        else if (strangePoints >= 275 && strangePoints < 350)
                        {
                            strangeRank = "Spectacularly Lethal";
                        }
                        else if (strangePoints >= 350 && strangePoints < 500)
                        {
                            strangeRank = "Gore-Spattered";
                        }
                        else if (strangePoints >= 500 && strangePoints < 750)
                        {
                            strangeRank = "Wicked Nasty";
                        }
                        else if (strangePoints >= 750 && strangePoints < 999)
                        {
                            strangeRank = "Positively Inhumane";
                        }
                        else if (strangePoints >= 999 && strangePoints < 1000)
                        {
                            strangeRank = "Totally Ordinary";
                        }
                        else if (strangePoints >= 1000 && strangePoints < 1500)
                        {
                            strangeRank = "Face-Melting";
                        }
                        else if (strangePoints >= 1500 && strangePoints < 2500)
                        {
                            strangeRank = "Rage-Inducing";
                        }
                        else if (strangePoints >= 2500 && strangePoints < 5000)
                        {
                            strangeRank = "Server-Clearing";
                        }
                        else if (strangePoints >= 5000 && strangePoints < 7500)
                        {
                            strangeRank = "Epic";
                        }
                        else if (strangePoints >= 7500 && strangePoints < 7616)
                        {
                            strangeRank = "Legendary";
                        }
                        else if (strangePoints >= 7616 && strangePoints < 8500)
                        {
                            strangeRank = "Australian";
                        }
                        else if (strangePoints >= 8500)
                        {
                            strangeRank = "Hale's Own";
                        }
                        break;
                    case (int)StrangeRankType.HolidayPunch:
                        if (strangePoints < 10)
                        {
                            strangeRank = "Strange";
                        }
                        else if (strangePoints >= 10 && strangePoints < 25)
                        {
                            strangeRank = "Unremarkable";
                        }
                        else if (strangePoints >= 25 && strangePoints < 45)
                        {
                            strangeRank = "Almost Amusing";
                        }
                        else if (strangePoints >= 45 && strangePoints < 70)
                        {
                            strangeRank = "Mildly Mirthful";
                        }
                        else if (strangePoints >= 70 && strangePoints < 100)
                        {
                            strangeRank = "Somewhat Droll";
                        }
                        else if (strangePoints >= 100 && strangePoints < 135)
                        {
                            strangeRank = "Thigh-Slapping";
                        }
                        else if (strangePoints >= 135 && strangePoints < 175)
                        {
                            strangeRank = "Notably Cheery";
                        }
                        else if (strangePoints >= 175 && strangePoints < 225)
                        {
                            strangeRank = "Sufficiently Wry";
                        }
                        else if (strangePoints >= 225 && strangePoints < 275)
                        {
                            strangeRank = "Truly Feared";
                        }
                        else if (strangePoints >= 275 && strangePoints < 350)
                        {
                            strangeRank = "Spectacularly Jocular";
                        }
                        else if (strangePoints >= 350 && strangePoints < 500)
                        {
                            strangeRank = "Riotous";
                        }
                        else if (strangePoints >= 500 && strangePoints < 749)
                        {
                            strangeRank = "Wicked Funny";
                        }
                        else if (strangePoints >= 749 && strangePoints < 750)
                        {
                            strangeRank = "Totally Unamusing";
                        }
                        else if (strangePoints >= 750 && strangePoints < 1000)
                        {
                            strangeRank = "Positively Persiflagious";
                        }
                        else if (strangePoints >= 1000 && strangePoints < 1500)
                        {
                            strangeRank = "Frown-Annihilating";
                        }
                        else if (strangePoints >= 1500 && strangePoints < 2500)
                        {
                            strangeRank = "Grin-Inducing";
                        }
                        else if (strangePoints >= 2500 && strangePoints < 5000)
                        {
                            strangeRank = "Server-Clearing";
                        }
                        else if (strangePoints >= 5000 && strangePoints < 7500)
                        {
                            strangeRank = "Epic";
                        }
                        else if (strangePoints >= 7500 && strangePoints < 7923)
                        {
                            strangeRank = "Legendary";
                        }
                        else if (strangePoints >= 7923 && strangePoints < 8500)
                        {
                            strangeRank = "Australian";
                        }
                        else if (strangePoints >= 8500)
                        {
                            strangeRank = "Mann Co. Select";
                        }
                        break;
                    case (int)StrangeRankType.InvisWatch:
                        if (strangePoints < 200)
                        {
                            strangeRank = "Strange";
                        }
                        else if (strangePoints >= 200 && strangePoints < 500)
                        {
                            strangeRank = "Unremarkable";
                        }
                        else if (strangePoints >= 500 && strangePoints < 900)
                        {
                            strangeRank = "Scarcely Shocking";
                        }
                        else if (strangePoints >= 900 && strangePoints < 1337)
                        {
                            strangeRank = "Mildly Magnetizing";
                        }
                        else if (strangePoints >= 1337 && strangePoints < 2000)
                        {
                            strangeRank = "Somewhat Inducting";
                        }
                        else if (strangePoints >= 2000 && strangePoints < 2700)
                        {
                            strangeRank = "Unfortunate";
                        }
                        else if (strangePoints >= 2700 && strangePoints < 3500)
                        {
                            strangeRank = "Notably Deleterious";
                        }
                        else if (strangePoints >= 3500 && strangePoints < 4500)
                        {
                            strangeRank = "Sufficiently Ruinous";
                        }
                        else if (strangePoints >= 4500 && strangePoints < 5500)
                        {
                            strangeRank = "Truly Conducting";
                        }
                        else if (strangePoints >= 5500 && strangePoints < 7000)
                        {
                            strangeRank = "Spectacularly Psuedoful";
                        }
                        else if (strangePoints >= 7000 && strangePoints < 9000)
                        {
                            strangeRank = "Ion-Spattered";
                        }
                        else if (strangePoints >= 9000 && strangePoints < 12000)
                        {
                            strangeRank = "Wickedly Dynamizing";
                        }
                        else if (strangePoints >= 12000 && strangePoints < 16000)
                        {
                            strangeRank = "Positively Plasmatic";
                        }
                        else if (strangePoints >= 16000 && strangePoints < 21337)
                        {
                            strangeRank = "Totally Ordinary";
                        }
                        else if (strangePoints >= 21337 && strangePoints < 35000)
                        {
                            strangeRank = "Circuit-Melting";
                        }
                        else if (strangePoints >= 35000 && strangePoints < 58007)
                        {
                            strangeRank = "Nullity-Inducing";
                        }
                        else if (strangePoints >= 58007 && strangePoints < 90000)
                        {
                            strangeRank = "Server-Clearing";
                        }
                        else if (strangePoints >= 90000 && strangePoints < 120000)
                        {
                            strangeRank = "Epic";
                        }
                        else if (strangePoints >= 120000 && strangePoints < 140000)
                        {
                            strangeRank = "Legendary";
                        }
                        else if (strangePoints >= 140000 && strangePoints < 160000)
                        {
                            strangeRank = "Australian";
                        }
                        else if (strangePoints >= 160000)
                        {
                            strangeRank = "Mann Co. Select";
                        }
                        break;
                    case (int)StrangeRankType.Mantreads:
                        break;
                    case (int)StrangeRankType.Sapper:
                        break;
                    case (int)StrangeRankType.SpiritOfGiving:
                        break;
                    case (int)StrangeRankType.Cosmetic:
                        break;
                    case (int)StrangeRankType.DuckJournal:
                        break;
                    case (int)StrangeRankType.SoulGargoyle:
                        break;
                }
            }

            public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
            {
                crit = false;
            }

            public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
            {
                crit = false;
            }

            public override void ModifyTooltips(List<TooltipLine> tooltips)
            {
                SafeSetDefaults();
                string[] linesToRemove =
                {
                "CritChance",
                "Speed",
                "Knockback",
                };
                string[] linesToRecolor =
                {
                "Damage",
                "Ammo",
                "Consumable",
                };

                foreach (TooltipLine line in tooltips)
                {
                    if (line.mod == "Terraria")
                    {
                        if (line.Name == "ItemName")
                        {
                            if (isStrange)
                            {
                                line.overrideColor = TFColor[(int)TFColorID.Strange];
                                line.text = strangeRank + " " + line.text;
                            }
                            else
                            {
                                line.overrideColor = TFColor[TFItemQuality];
                            }
                        }

                        for (int i = 0; i < linesToRecolor.Length; i++)
                        {
                            int index = tooltips.FindIndex(tooltip => tooltip.Name == linesToRecolor[i]);

                            if (index >= 0 && line.Name == linesToRecolor[i])
                            {
                                line.overrideColor = TFColor[(int)TFColorID.AttributeNeutral];
                            }
                        }
                    }
                }

                for (int i = 0; i < linesToRemove.Length; i++)
                {
                    int index = tooltips.FindIndex(tooltip => tooltip.Name == linesToRemove[i]);

                    if (index >= 0)
                    {
                        tooltips.RemoveAt(index);
                    }
                }

                int index2 = tooltips.FindIndex(tooltip => tooltip.Name == "ItemName");
                if (index2 >= 0)
                {
                    if (TFItemType != "")
                    {
                        if (isStrange)
                        {
                            TooltipLine level = new TooltipLine(mod, "ItemName", strangeRank + " " + TFItemType + " - " + strangeStat + ": " + strangePoints);
                            level.overrideColor = TFColor[(int)TFColorID.AttributeLevel];
                            tooltips.Add(level);
                        }
                        else
                        {
                            TooltipLine level = new TooltipLine(mod, "ItemName", "Level " + TFItemLevel + " " + TFItemType);
                            level.overrideColor = TFColor[(int)TFColorID.AttributeLevel];
                            tooltips.Add(level);
                        }
                    }

                }
            }

            public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
            {
                SpriteBatch spriteBatch = Main.spriteBatch;
                DynamicSpriteFont tooltip = Main.fontMouseText;
                Vector2 drawPos = new Vector2(x, y);
                Rectangle topLeft = new Rectangle(0, 0, 16, 16);
                Rectangle topMiddle = new Rectangle(16, 0, 20, 16);
                Rectangle topRight = new Rectangle(36, 0, 16, 16);
                Rectangle middleLeft = new Rectangle(0, 16, 16, 20);
                Rectangle center = new Rectangle(16, 16, 20, 20);
                Rectangle middleRight = new Rectangle(36, 16, 16, 20);
                Rectangle bottomLeft = new Rectangle(0, 36, 16, 16);
                Rectangle bottomMiddle = new Rectangle(16, 36, 20, 16);
                Rectangle bottomRight = new Rectangle(36, 36, 16, 16);
                float horizontalPadding = 18f;
                float verticalPadding = 6f;

                string longestLine = "";
                foreach (TooltipLine tooltipLine in lines)
                {
                    if (tooltip.MeasureString(tooltipLine.text).X > tooltip.MeasureString(longestLine).X)
                    {
                        longestLine = tooltipLine.text;
                    }
                }
                Vector2 tooltipDimensions = tooltip.MeasureString(longestLine);
                {
                    foreach (TooltipLine line in lines)
                    {
                        tooltipHeight += tooltip.MeasureString(line.text).Y;
                    }
                }
                Texture2D texture = ModContent.GetTexture("TerrariaFortress/Items/Weapons/TFItemTooltipUI");
                Color drawColor = Color.White;
                float drawScale = 1f;
                bool TrueForNormalDrawing_FalseForNewDrawing = true;

                switch (TrueForNormalDrawing_FalseForNewDrawing)
                {
                    case true:
                        #region Normal UI Drawing
                        spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding, -verticalPadding), topLeft, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        for (int i = 0; i < (tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) / topMiddle.Width; i++)
                        {
                            spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + topLeft.Width + i * topMiddle.Width, -verticalPadding), i > (tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) / topMiddle.Width - 1 ? new Rectangle(topMiddle.X, topMiddle.Y, (int)(tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) % topMiddle.Width, topMiddle.Height) : topMiddle, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        }
                        spriteBatch.Draw(texture, drawPos + new Vector2(tooltipDimensions.X - topRight.Width + horizontalPadding, -verticalPadding), topRight, drawColor, default, default, drawScale, SpriteEffects.None, 0f);

                        for (int i = 0; i < (tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) / center.Height; i++)
                        {
                            spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding, -verticalPadding + topLeft.Height + i * middleLeft.Height), i > (tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) / center.Height - 1 ? new Rectangle(middleLeft.X, middleLeft.Y, middleLeft.Width, (int)(tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) % middleLeft.Height) : middleLeft, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        }
                        for (int i = 0; i < (tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) / center.Width; i++)
                        {
                            for (int j = 0; j < (tooltipHeight - topMiddle.Height - bottomMiddle.Height + verticalPadding * 2) / center.Height; j++)
                            {
                                spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + middleLeft.Width + i * center.Width, -verticalPadding + topMiddle.Height + j * center.Height), new Rectangle(center.X, center.Y, i > (tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) / center.Width - 1 ? (int)(tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) % center.Width : center.Width, j > (tooltipHeight - topMiddle.Height - bottomMiddle.Height + verticalPadding * 2) / center.Height - 1 ? (int)(tooltipHeight - topMiddle.Height - bottomMiddle.Height + verticalPadding * 2) % center.Height : center.Height), drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                            }
                        }
                        for (int i = 0; i < (tooltipHeight - topRight.Height - bottomRight.Height + verticalPadding * 2) / center.Height; i++)
                        {
                            spriteBatch.Draw(texture, drawPos + new Vector2(tooltipDimensions.X - middleLeft.Width + horizontalPadding, -verticalPadding + topRight.Height + i * middleRight.Height), i > (tooltipHeight - topRight.Height - topRight.Height + verticalPadding * 2) / center.Height - 1 ? new Rectangle(middleRight.X, middleRight.Y, middleRight.Width, (int)(tooltipHeight - topRight.Height - bottomRight.Height + verticalPadding * 2) % middleRight.Height) : middleRight, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        }

                        spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding, tooltipHeight - bottomRight.Height + verticalPadding), bottomLeft, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        for (int i = 0; i < (tooltipDimensions.X - bottomLeft.Width - bottomRight.Width + horizontalPadding * 2) / bottomMiddle.Width; i++)
                        {
                            spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + bottomLeft.Width + i * bottomMiddle.Width, tooltipHeight - bottomRight.Height + verticalPadding), i > (tooltipDimensions.X - bottomLeft.Width - bottomRight.Width + horizontalPadding * 2) / bottomMiddle.Width - 1 ? new Rectangle(bottomMiddle.X, bottomMiddle.Y, (int)(tooltipDimensions.X - bottomLeft.Width - bottomRight.Width + horizontalPadding * 2) % bottomMiddle.Width, bottomMiddle.Height) : bottomMiddle, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        }
                        spriteBatch.Draw(texture, drawPos + new Vector2(tooltipDimensions.X - bottomRight.Width + horizontalPadding, tooltipHeight - bottomRight.Height + verticalPadding), bottomRight, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        #endregion
                        break;
                    case false:
                        #region Proper UI Drawing
                        spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding, -verticalPadding), topLeft, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        for (int i = 0; i < (tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) / topMiddle.Width; i++)
                        {
                            spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + topLeft.Width + i * topMiddle.Width, -verticalPadding), i > (tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) / topMiddle.Width - 1 ? new Rectangle(topMiddle.X, topMiddle.Y, (int)(tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) % topMiddle.Width, topMiddle.Height) : topMiddle, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                            if (i > (tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) / topMiddle.Width - 1)
                            {
                                spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + topLeft.Width + i * topMiddle.Width + ((int)(tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) % topMiddle.Width), -verticalPadding), topRight, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                            }
                        }

                        for (int i = 0; i < (tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) / center.Height; i++)
                        {
                            spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding, -verticalPadding + topLeft.Height + i * middleLeft.Height), i > (tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) / center.Height - 1 ? new Rectangle(middleLeft.X, middleLeft.Y, middleLeft.Width, (int)(tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) % middleLeft.Height) : middleLeft, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                            if (i > (tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) / center.Height - 1)
                            {
                                spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding, -verticalPadding + topLeft.Height + i * middleLeft.Height + ((int)(tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) % middleLeft.Height)), bottomLeft, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                            }
                        }
                        for (int i = 0; i < (tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) / center.Width; i++)
                        {
                            for (int j = 0; j < (tooltipHeight - topMiddle.Height - bottomMiddle.Height + verticalPadding * 2) / center.Height; j++)
                            {
                                spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + middleLeft.Width + i * center.Width, -verticalPadding + topMiddle.Height + j * center.Height), new Rectangle(center.X, center.Y, i > (tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) / center.Width - 1 ? (int)(tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) % center.Width : center.Width, j > (tooltipHeight - topMiddle.Height - bottomMiddle.Height + verticalPadding * 2) / center.Height - 1 ? (int)(tooltipHeight - topMiddle.Height - bottomMiddle.Height + verticalPadding * 2) % center.Height : center.Height), drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                            }
                            for (int k = 0; k < (tooltipHeight - topRight.Height - bottomRight.Height + verticalPadding * 2) / center.Height; k++)
                            {
                                if (i > (tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) / center.Width - 1)
                                {
                                    spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + middleLeft.Width + i * center.Width + ((int)(tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) % center.Width), -verticalPadding + topRight.Height + k * middleRight.Height), new Rectangle(middleRight.X, middleRight.Y, middleRight.Width, k > (tooltipHeight - topRight.Height - bottomRight.Height + verticalPadding * 2) / center.Height - 1 ? (int)(tooltipHeight - topRight.Height - bottomRight.Height + verticalPadding * 2) % middleRight.Height : middleRight.Height), drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                                }
                            }
                        }

                        //for (int i = 0; i < (tooltipDimensions.X - bottomLeft.Width - bottomRight.Width + horizontalPadding * 2) / bottomMiddle.Width; i++)
                        //{
                        //    spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + bottomLeft.Width + i * bottomMiddle.Width, tooltipHeight - bottomRight.Height + verticalPadding), i > (tooltipDimensions.X - bottomLeft.Width - bottomRight.Width + horizontalPadding * 2) / bottomMiddle.Width - 1 ? new Rectangle(bottomMiddle.X, bottomMiddle.Y, (int)(tooltipDimensions.X - bottomLeft.Width - bottomRight.Width + horizontalPadding * 2) % bottomMiddle.Width, bottomMiddle.Height) : bottomMiddle, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        //}
                        //spriteBatch.Draw(texture, drawPos + new Vector2(tooltipDimensions.X - bottomRight.Width + horizontalPadding, tooltipHeight - bottomRight.Height + verticalPadding), bottomRight, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        #endregion
                        break;
                }
                return true;
            }

            //public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
            //{
            //    DynamicSpriteFont tooltip = Main.fontMouseText;
            //    if (tooltip.MeasureString(line.text).X > tooltip.MeasureString(longestTooltipLine).X)
            //    {
            //        longestTooltipLine = line.text;
            //    }
            //    centeredText = (int)tooltip.MeasureString(longestTooltipLine).X;

            //    //Main.NewText(Main.mouseX - line.X);
            //    return base.PreDrawTooltipLine(line, ref yOffset);
            //}

            public override bool CanUseItem(Player player)
            {
                if (isSwingMelee)
                {
                    player.itemAnimation = item.useAnimation;
                    meleeEntityCollided = false;
                    meleeWorldCollided = false;
                    meleeEntityCollisionSoundCheck = false;
                    meleeWorldCollisionSoundCheck = false;
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.Center.X, (int)player.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + basicUseSound));
                }

                return base.CanUseItem(player);
            }

            
            public override bool ConsumeAmmo(Player player)
            {
                return false;
            }

            public override bool NewPreReforge()
            {
                return false;
            }

            public override bool? PrefixChance(int pre, UnifiedRandom rand)
            {
                return pre == -3 || pre == -1 ? false : true;
            }

            public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
            {
                if (isSwingMelee)
                {
                    if (player.itemAnimation == (int)(item.useTime * 0.7f))
                    {
                        noHitbox = false;
                    }
                    else
                    {
                        noHitbox = true;
                    }

                    if (!noHitbox)
                    {
                        for (int i = 0; i < hitbox.Width;)
                        {
                            if (i + 16 < hitbox.Width)
                            {
                                i += 16;
                            }
                            else
                            {
                                i += hitbox.Width - i;
                            }
                            for (int j = 0; j < hitbox.Height;)
                            {
                                if (j + 16 < hitbox.Height)
                                {
                                    j += 16;
                                }
                                else
                                {
                                    j += hitbox.Height - j;
                                }

                                Point point = (new Vector2(hitbox.X + i, hitbox.Y + j)).ToTileCoordinates();
                                if (WorldGen.SolidOrSlopedTile(point.X, point.Y))
                                {
                                    meleeWorldCollided = true;
                                }
                            }
                        }

                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            Player impactedPlayer = Main.player[i];

                            if (HitboxCheckPlayer(player, hitbox, impactedPlayer))
                            {
                                meleeEntityCollided = true;
                            }
                        }

                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC impactedNPC = Main.npc[i];

                            if (HitboxCheckNPC(player, hitbox, impactedNPC))
                            {
                                meleeEntityCollided = true;
                            }
                            if (hitbox.Intersects(impactedNPC.Hitbox))
                            {
                                meleeEntityCollided = true;
                            }
                        }
                    }

                    if (meleeWorldCollided && !meleeWorldCollisionSoundCheck)
                    {
                        meleeWorldCollisionSoundCheck = true;
                        Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + meleeWorldCollisionSound));
                    }
                    if (meleeEntityCollided && !meleeEntityCollisionSoundCheck)
                    {
                        meleeEntityCollisionSoundCheck = true;
                        Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + meleeEntityCollisionSound));
                    }
                }
                if (isRanged)
                {
                    noHitbox = true;
                }
            }
        }

        public static readonly int[] airblastReflectBlacklist =
        {
            ModContent.ProjectileType<FlamethrowerAirblast>(),
            ModContent.ProjectileType<FlamethrowerFlame>(),
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
            static bool pilotSoundCheck = false;
            static bool meleeRotationBuildup = true;
            static float meleeRotationFactor = 0f;

            public static readonly PlayerLayer HeldItem = new PlayerLayer("TerrariaFortress", "HeldItem", PlayerLayer.HeldItem, delegate (PlayerDrawInfo drawInfo)
            {
                Mod mod = ModLoader.GetMod("TerrariaFortress");
                Player player = drawInfo.drawPlayer;
                Color colorBase = Lighting.GetColor((int)(player.Center.X / 16), (int)(player.Center.Y / 16));
                int alphaOffset = (int)(Math.Abs(player.stealth - 1) * 255);
                Color rangedHoldoutColor = new Color(colorBase.R - alphaOffset, colorBase.G - alphaOffset, colorBase.B - alphaOffset, colorBase.A - alphaOffset/*(int)(MathHelper.Clamp(colorBase.A - alphaOffset, 8, 255))*/);
                Texture2D flamethrowerTexture = ModContent.GetTexture("TerrariaFortress/Items/Weapons/FlamethrowerHoldout");
                Texture2D flamethrowerTexture2 = ModContent.GetTexture("TerrariaFortress/Items/Weapons/FlamethrowerHoldoutGlowmask");
                Rectangle flamethrowerRectangle = flamethrowerTexture.Frame(1, 1, 0, 0);
                Vector2 flamethrowerOrigin = player.direction == -1 ? flamethrowerRectangle.Size() * 0.5f + new Vector2(player.direction + flamethrowerRectangle.Width * 0.5f, 0f) : flamethrowerRectangle.Size() * 0.5f + new Vector2(player.direction - flamethrowerRectangle.Width * 0.5f, 0f);
                SpriteEffects spriteEffect = (player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (player.gravDir == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None);
                bool drawConditions = !player.dead && player.active && !player.invis && !player.frozen && !player.stoned;
                Vector2 flamethrowerDrawPos = (player.MountedCenter - Main.screenPosition + new Vector2(0f, player.gfxOffY)).Floor();

                void DrawHoldoutSimple(Texture2D texture, float horizontalOffset, float verticalOffset, float rotation, Color suppliedColor, bool meleeOrigin)
                {
                    Rectangle rectangle = texture.Frame(1, 1, 0, 0);
                    Vector2 drawPos = (player.MountedCenter - Main.screenPosition + new Vector2(player.direction * horizontalOffset, player.gfxOffY + verticalOffset)).Floor();
                    Vector2 origin = player.direction == -1 ? rectangle.Size() * 0.5f + new Vector2(horizontalOffset + rectangle.Width * 0.5f, verticalOffset) : rectangle.Size() * 0.5f + new Vector2(-horizontalOffset - rectangle.Width * 0.5f, verticalOffset);
                    Main.playerDrawData.Add(new DrawData(texture, drawPos, rectangle, suppliedColor, rotation, origin, player.HeldItem.scale, spriteEffect, 0));
                }

                void DrawHoldoutMelee(Texture2D texture, float horizontalOffset, float verticalOffset, float rotation, Color suppliedColor)
                {
                    Rectangle rectangle = texture.Frame(1, 1, 0, 0);
                    Vector2 drawPos = (player.MountedCenter - Main.screenPosition + new Vector2(player.direction * (horizontalOffset + (player.gravDir == -1 ? verticalOffset : 0f)), player.gfxOffY + verticalOffset + (player.gravDir == -1 ? horizontalOffset : 0f))).Floor();
                    Vector2 origin = player.direction == -1 ? rectangle.Size() * 0.5f + new Vector2(rectangle.Width * 0.5f + horizontalOffset, rectangle.Height * 0.5f - verticalOffset + (player.gravDir == -1 ? -rectangle.Height + verticalOffset : 0f)) : rectangle.Size() * 0.5f + new Vector2(-rectangle.Width * 0.5f - horizontalOffset, rectangle.Height * 0.5f - verticalOffset + (player.gravDir == -1 ? -rectangle.Height + verticalOffset : 0f));
                    Main.playerDrawData.Add(new DrawData(texture, drawPos, rectangle, suppliedColor, rotation, origin, player.HeldItem.scale, spriteEffect, 0));
                }

                void DrawAllHoldouts()
                {
                    float swingBackPoint = 135f;
                    if (player.HeldItem.modItem is TFWeapon item)
                    {
                        if (player.HeldItem.type == ModContent.ItemType<Flamethrower>())
                        {
                            Main.playerDrawData.Add(new DrawData(flamethrowerTexture, flamethrowerDrawPos, flamethrowerRectangle, rangedHoldoutColor, player.itemRotation, flamethrowerOrigin, player.HeldItem.scale, spriteEffect, 0));
                            Main.playerDrawData.Add(new DrawData(flamethrowerTexture2, flamethrowerDrawPos, flamethrowerRectangle, new Color(255 - alphaOffset, 255 - alphaOffset, 255 - alphaOffset, 255 - alphaOffset), player.itemRotation, flamethrowerOrigin, player.HeldItem.scale, spriteEffect, 0));

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
                        if (player.HeldItem.type == ModContent.ItemType<Shotgun>())
                        {
                            DrawHoldoutSimple(ModContent.GetTexture("TerrariaFortress/Items/Weapons/ShotgunHoldout"), -4f, 2f, player.itemRotation, rangedHoldoutColor, false);
                        }
                        if (player.HeldItem.type == ModContent.ItemType<RocketLauncher>())
                        {
                            DrawHoldoutSimple(ModContent.GetTexture("TerrariaFortress/Items/Weapons/RocketLauncherHoldout"), -16f, 0f, player.itemRotation, rangedHoldoutColor, false);
                        }

                        if (item.isSwingMelee)
                        {
                            if (player.itemAnimation == player.HeldItem.useAnimation - 1 || player.itemAnimation == 0)
                            {
                                meleeRotationBuildup = true;
                                meleeRotationFactor = 0f;
                                player.itemRotation = player.fullRotation + (player.direction == -1 ? MathHelper.ToRadians(90f) : MathHelper.ToRadians(270f));

                            }
                            else if (player.itemAnimation != player.HeldItem.useAnimation - 1)
                            {
                                if (meleeRotationFactor < swingBackPoint)
                                {
                                    meleeRotationFactor += 5f + meleeRotationFactor * (100 / player.HeldItem.useAnimation * 0.025f) * 3;
                                }
                                else
                                {
                                    meleeRotationBuildup = false;
                                }

                                if (!meleeRotationBuildup)
                                {
                                    meleeRotationFactor -= (meleeRotationFactor - swingBackPoint) * 0.2f;
                                }
                                player.itemRotation = (player.direction == -1 ? MathHelper.ToRadians(90f - meleeRotationFactor) : MathHelper.ToRadians(270f + meleeRotationFactor));
                            }

                            if (player.HeldItem.type == ModContent.ItemType<Bat>())
                            {
                                DrawHoldoutMelee(Main.itemTexture[player.HeldItem.type], 0f, 2f, player.itemRotation * player.gravDir, colorBase);
                            }
                            if (player.HeldItem.type == ModContent.ItemType<Shovel>())
                            {
                                DrawHoldoutMelee(Main.itemTexture[player.HeldItem.type], -2f, 2f, player.itemRotation * player.gravDir, colorBase);
                            }
                            if (player.HeldItem.type == ModContent.ItemType<FireAxe>())
                            {
                                DrawHoldoutMelee(Main.itemTexture[player.HeldItem.type], 0f, 2f, player.itemRotation * player.gravDir, colorBase);
                            }
                        }
                    }
                }

                if (!Main.gameMenu)
                {
                    if (drawConditions && !player.pulley)
                    {
                        DrawAllHoldouts();
                    }
                    else if (player.pulley && (player.controlUseItem || player.altFunctionUse == 2))
                    {
                        DrawAllHoldouts();
                    }
                }

                if (player.HeldItem.type != ModContent.ItemType<Flamethrower>())
                {
                    pilotSoundCheck = false;
                }
            });
            public static readonly PlayerLayer Skin = new PlayerLayer("TerrariaFortress", "Skin", PlayerLayer.Skin, delegate (PlayerDrawInfo drawInfo)
            {
                Player player = drawInfo.drawPlayer;

                if (!Main.gameMenu && player.HeldItem.modItem is TFWeapon item && item.isSwingMelee)
                {
                    if (player.itemAnimation < player.itemAnimationMax * 0.777)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 3;
                    }
                    else if (player.itemAnimation < player.itemAnimationMax * 0.888)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 2;
                    }
                    else if (player.itemAnimation > player.itemAnimationMax * 0.888)
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
                if (type == NPCID.Demolitionist)
                {
                    if (NPC.downedBoss3)
                    {
                        shop.item[nextSlot].SetDefaults(ModContent.ItemType<AmmoBox>());
                        nextSlot++;
                        shop.item[nextSlot].SetDefaults(ModContent.ItemType<Flamethrower>());
                        nextSlot++;
                        shop.item[nextSlot].SetDefaults(ModContent.ItemType<Shotgun>());
                        nextSlot++;
                        shop.item[nextSlot].SetDefaults(ModContent.ItemType<FireAxe>());
                        nextSlot++;
                        shop.item[nextSlot].SetDefaults(ModContent.ItemType<RocketLauncher>());
                        nextSlot++;
                        shop.item[nextSlot].SetDefaults(ModContent.ItemType<Shovel>());
                        nextSlot++;
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