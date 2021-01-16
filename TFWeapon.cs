using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using TerrariaFortress.Items;

namespace TerrariaFortress
{
    public class TFWeapon : ModItem
    {
        public string basicUseSound = "";

        public string meleeEntityCollisionSound = "";
        public bool meleeEntityCollided = false;
        public bool meleeEntityCollisionSoundCheck = false;

        public string meleeWorldCollisionSound = "";
        public bool meleeWorldCollided = false;
        public bool meleeWorldCollisionSoundCheck = false;

        public string holsterSound = "";
        public bool holsterSoundCheck = false;

        public string deploySound = "";
        public bool deploySoundCheck = false;


        /// <summary>
        /// Item type, used in the tooltip.
        /// </summary>
        public string TFItemType = "";
        /// <summary>
        /// If this item is a swing melee. Sets damage for melee, used for most melees.
        /// </summary>
        public bool isSwingMelee = default;
        /// <summary>
        /// If this item is ranged. Sets damage to ranged, used for most primaries and secondaries.
        /// </summary>
        public bool isRanged = default;
        /// <summary>
        /// If this item is a SF spell. Sets damage to magic, among other things.
        /// </summary>
        public bool isScreamFortressSpell = default;
        /// <summary>
        /// If this item is strange. Counts kills.
        /// </summary>
        public bool isStrange = default;
        /// <summary>
        /// If this item has a killstreaker active.
        /// </summary>
        public bool killstreaksActive = default;

        /// <summary>
        /// If this item is from a contract or not. Used for determining sell prices.
        /// </summary>
        public bool contractItem = default;
        /// <summary>
        /// Item quality for name color. See <see cref="TerrariaFortress.TFColorID"/> enum.
        /// </summary>
        public int TFItemQuality = 0;
        /// <summary>
        /// Item level for tooltip.
        /// </summary>
        public int TFItemLevel;

        /// <summary>
        /// Type of strange rank, for specific items. See <see cref="TerrariaFortress.StrangeRankType"/> enum.
        /// </summary>
        public int strangeRankType = (int)TerrariaFortress.StrangeRankType.Generic;
        /// <summary>
        /// What each strange point is referred to as.
        /// </summary>
        public string strangeStat = "Kills";
        /// <summary>
        /// Type of strange rank as a string, shown in the tooltip.
        /// </summary>
        public string strangeRank = "Strange";
        /// <summary>
        /// Amount of strange points stored.
        /// </summary>
        public int strangePoints = 0;

        /// <summary>
        /// How long, in ticks, it takes for the item to deploy.
        /// </summary>
        public int deployTimeMax = 0;
        /// <summary>
        /// The item's remaining deploy time, in ticks.
        /// </summary>
        public int deployTime = 0;

        float tooltipHeight;
        string longestTooltipLine = "";

        /// Enable random crits.
        public bool canCrit = true;


        public Vector2 ShootSpawnPos(Player player, float horizontalOffset, float verticalOffset)
        {
            return player.MountedCenter + (new Vector2(player.direction * horizontalOffset, verticalOffset)).RotatedBy(player.itemRotation + player.fullRotation);
        }

        /// <summary>
        /// Generates a hitbox for TF melee weapons.
        /// </summary>
        /// <param name="player">Player parameter.</param>
        /// <param name="horizontalOffset">The hitbox's horizontal offset.</param>
        /// <param name="verticalOffset">The hitbox's vertical offset.</param>
        /// <param name="width">The hitbox's width.</param>
        /// <param name="height">The hitbox's height.</param>
        /// <returns></returns>
        public virtual Rectangle TFMeleeHitbox(Player player, float horizontalOffset, float verticalOffset, int width, int height)
        {
            Vector2 targetDirection = new Vector2(0f, 0f);
            if (player == Main.player[Main.myPlayer])
            {
                targetDirection = Vector2.Normalize(Main.MouseWorld - player.MountedCenter);
            }
            Rectangle hitbox = new Rectangle((int)(player.MountedCenter.X + (player.direction == -1 ? -width - horizontalOffset : horizontalOffset)), (int)(player.MountedCenter.Y + verticalOffset * player.gravDir + (player.gravDir == -1 ? (-height) : 0f)), width, height);
            if (player == Main.player[Main.myPlayer])
            {
                Vector2 position = player.MountedCenter + new Vector2(width * 0.5f - horizontalOffset, 0f).RotatedBy(targetDirection.ToRotation()) - hitbox.Size() * 0.5f;
                hitbox.X = (int)(position.X);
                hitbox.Y = (int)(position.Y);
            }
            return hitbox;
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

        /// <summary>
        /// System for TF tooltips.
        /// </summary>
        /// <param name="tooltips">Tooltips parameter.</param>
        /// <param name="attributeColor">The tooltip's color.</param>
        /// <param name="attributeText">The tooltip text.</param>
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

        public virtual void StrangeSystem()
        {
            switch (strangeRankType)
            {
                default:
                case (int)TerrariaFortress.StrangeRankType.Generic:
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
                case (int)TerrariaFortress.StrangeRankType.HolidayPunch:
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
                case (int)TerrariaFortress.StrangeRankType.InvisWatch:
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
                case (int)TerrariaFortress.StrangeRankType.Mantreads:
                    break;
                case (int)TerrariaFortress.StrangeRankType.Sapper:
                    break;
                case (int)TerrariaFortress.StrangeRankType.SpiritOfGiving:
                    break;
                case (int)TerrariaFortress.StrangeRankType.Cosmetic:
                    break;
                case (int)TerrariaFortress.StrangeRankType.DuckJournal:
                    break;
                case (int)TerrariaFortress.StrangeRankType.SoulGargoyle:
                    break;
            }
        }

        /// <summary>
        /// For setting constantly changing fields, i.e. variation for weapon sounds.
        /// </summary>
        public virtual void SetConstantDefaults()
        {

        }

        /// <summary>
        /// SetDefaults(), for TF items.
        /// </summary>
        public virtual void TFDefaults()
        {

        }

        public sealed override void SetDefaults()
        {
            SetConstantDefaults();
            TFDefaults();
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
                item.value = 300000;
                item.shoot = 10;
            }
            item.magic = isScreamFortressSpell;
            item.noUseGraphic = true;
            item.autoReuse = true;
            TFDefaults();
        }

        /// <summary>
        /// Allow things to happen while in the inventory.
        /// </summary>
        /// <param name="player">Player parameter.</param>
        public virtual void TFUpdateInventory(Player player)
        {

        }

        public override void UpdateInventory(Player player)
        {
            TFUpdateInventory(player);
            SetConstantDefaults();
            StrangeSystem();
            DrawWeaponHoldout(player);
            if (player.HeldItem.modItem == this)
            {
                if (deploySound != "" && !deploySoundCheck)
                {
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + deploySound));
                    deploySoundCheck = true;
                }
            }
            if (player.HeldItem.modItem != this)
            {
                if (holsterSound != "" && !holsterSoundCheck)
                {
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + holsterSound));
                    holsterSoundCheck = true;
                }
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

        /// <summary>
        /// For setting TF tooltips.
        /// </summary>
        /// <param name="tooltips">Tooltips parameter.</param>
        public virtual void TFDescription(List<TooltipLine> tooltips)
        {

        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TFDefaults();

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
                            line.overrideColor = TerrariaFortress.TFColor[(int)TerrariaFortress.TFColorID.Strange];
                            line.text = strangeRank + " " + line.text;
                        }
                        else
                        {
                            line.overrideColor = TerrariaFortress.TFColor[TFItemQuality];
                        }
                    }

                    for (int i = 0; i < linesToRecolor.Length; i++)
                    {
                        int index = tooltips.FindIndex(tooltip => tooltip.Name == linesToRecolor[i]);

                        if (index >= 0 && line.Name == linesToRecolor[i])
                        {
                            line.overrideColor = TerrariaFortress.TFColor[(int)TerrariaFortress.TFColorID.AttributeNeutral];
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
                        level.overrideColor = TerrariaFortress.TFColor[(int)TerrariaFortress.TFColorID.AttributeLevel];
                        tooltips.Add(level);
                    }
                    else
                    {
                        TooltipLine level = new TooltipLine(mod, "ItemName", "Level " + TFItemLevel + " " + TFItemType);
                        level.overrideColor = TerrariaFortress.TFColor[(int)TerrariaFortress.TFColorID.AttributeLevel];
                        tooltips.Add(level);
                    }
                }

            }

            TFDescription(tooltips);
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
            longestTooltipLine = longestLine;
            {
                foreach (TooltipLine line in lines)
                {
                    tooltipHeight += tooltip.MeasureString(line.text).Y;
                }
            }
            Texture2D texture = ModContent.GetTexture("TerrariaFortress/Items/TFItemTooltipUI");
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

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            DynamicSpriteFont tooltip = Main.fontMouseText;

            line.X = (int)(line.X + ((tooltip.MeasureString(longestTooltipLine).X - tooltip.MeasureString(line.text).X) * 0.5f));
            return true;
        }

        public virtual bool TFCanUseItem(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (isSwingMelee)
            {
                player.itemAnimation = item.useAnimation;
                meleeEntityCollided = false;
                meleeWorldCollided = false;
                meleeEntityCollisionSoundCheck = false;
                meleeWorldCollisionSoundCheck = false;
                Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + basicUseSound));
            }

            return base.CanUseItem(player) && TFCanUseItem(player);
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

        /// <summary>
        /// Allows you to make things happen when this item strikes a tile in the world.
        /// </summary>
        /// <param name="player">Player parameter.</param>
        public virtual void OnHitTile(Player player)
        {

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

                            Point point = new Vector2(hitbox.X + i, hitbox.Y + j).ToTileCoordinates();
                            if (WorldGen.SolidOrSlopedTile((int)point.X, (int)point.Y))
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
                        if (hitbox.Intersects(impactedNPC.Hitbox) && impactedNPC.active)
                        {
                            meleeEntityCollided = true;
                        }
                    }
                }

                if (meleeEntityCollided && !meleeEntityCollisionSoundCheck)
                {
                    meleeEntityCollisionSoundCheck = true;
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + meleeEntityCollisionSound));
                }
                if (!meleeEntityCollisionSoundCheck && meleeWorldCollided && !meleeWorldCollisionSoundCheck)
                {
                    meleeWorldCollisionSoundCheck = true;
                    OnHitTile(player);
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + meleeWorldCollisionSound));
                }
            }
            if (isRanged)
            {
                noHitbox = true;
            }
        }

        #region Drawcode Methods
        #region Draw Fields
        public virtual Texture2D ItemTexture(Player player)
        {
            return Main.itemTexture[player.HeldItem.type];
        }

        public virtual float MeleeRotation(Player player)
        {
            return player.itemRotation * player.gravDir;
        }

        public virtual Color LightColor(Player player)
        {
            return Lighting.GetColor((int)(player.MountedCenter.X / 16), (int)(player.MountedCenter.Y / 16));
        }

        public virtual Color RangedWeaponColor(Player player)
        {
            int alphaOffset = (int)(Math.Abs(player.stealth - 1) * 255);
            return new Color(LightColor(player).R - alphaOffset, LightColor(player).G - alphaOffset, LightColor(player).B - alphaOffset, LightColor(player).A - alphaOffset);
        }

        public virtual Color GlowmaskColor(Player player)
        {
            int alphaOffset = (int)(Math.Abs(player.stealth - 1) * 255);
            return new Color(255 - alphaOffset, 255 - alphaOffset, 255 - alphaOffset, 255 - alphaOffset);
        }

        public virtual SpriteEffects FlipEffect(Player player)
        {
            return (player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (player.gravDir == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None);
        }

        public virtual int MuzzleFrame(Player player, int frames = 0, float fps = 60)
        {
            return (int)MathHelper.Clamp(Math.Abs(player.itemTime - player.HeldItem.useTime) / (60 / fps) - 1, 0, frames - 1);
        }
        #endregion

        public virtual void DrawSimpleHoldout(Texture2D texture, Vector2 offset, Vector2 originOffset, float rotation, Color suppliedColor, Player player, int horizontalFrames = 1, int verticalFrames = 1, int frameX = 0, int frameY = 0, int shader = 0)
        {
            Rectangle rectangle = texture.Frame(horizontalFrames, verticalFrames, frameX, frameY);
            Vector2 drawPosition = (player.MountedCenter - Main.screenPosition + new Vector2(player.direction * offset.X, player.gfxOffY + player.gravDir * offset.Y));
            drawPosition = new Vector2((int)drawPosition.X, (int)drawPosition.Y);
            Vector2 origin = rectangle.Size() * 0.5f + new Vector2(0f - offset.X * player.direction + originOffset.X * player.direction - offset.X * player.direction + (rectangle.Width * -player.direction * 0.5f), player.gravDir * (-offset.Y + originOffset.Y));
            DrawData drawData = new DrawData(texture, drawPosition, rectangle, suppliedColor, rotation, origin, player.HeldItem.scale, FlipEffect(player), 0);
            drawData.shader = shader;
            Main.playerDrawData.Add(drawData);
        }

        public virtual void DrawSimpleMeleeHoldout(Texture2D texture, Vector2 offset, Vector2 originOffset, float rotation, Color suppliedColor, Player player, int horizontalFrames = 1, int verticalFrames = 1, int frameX = 0, int frameY = 0, int shader = 0)
        {
            Rectangle rectangle = texture.Frame(horizontalFrames, verticalFrames, frameX, frameY);
            originOffset += new Vector2(0f, rectangle.Height * 0.5f);
            offset = offset.RotatedBy(MathHelper.ToRadians(45f));
            offset = new Vector2((int)offset.X, (int)offset.Y);
            DrawSimpleHoldout(texture, offset, originOffset, rotation, suppliedColor, player, horizontalFrames, verticalFrames, frameX, frameY, shader);
        }

        public virtual void DrawWeaponHoldout(Player player)
        {

        }
        #endregion
    }
}