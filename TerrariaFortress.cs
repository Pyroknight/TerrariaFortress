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
using System.Net;
using System.Threading;
using Terraria.UI.Chat;

namespace TerrariaFortress
{
    public static class TFUtils
    {
        public static int downloads = 0;

        public struct UITextures
        {
            public static TerrariaFortress mod => ModContent.GetInstance<TerrariaFortress>();
            public static Texture2D Beige;
            public static Texture2D Tooltip;
            public static Texture2D White;
            public static Texture2D Logo;
        }

        /// <summary>
        /// Numerous building statistics.
        /// </summary>
        public struct BuildingStats
        {
            public struct Types
            {
                public static int MiniSentry = 0;
                public static int Sentry = 1;
                public static int Dispenser = 2;
                public static int Teleporter = 3;
            }

            public struct Levels
            {
                public static int MiniSentryOne = 0;
                public static int One = 1;
                public static int Two = 2;
                public static int Three = 3;
            }

            public const float upgradeTime = 96f;

            public static int[] maxHealth =
            {
                100,
                150,
                180,
                216
            };

            public static int[] buildCost =
            {
                100,
                130,
                100,
                50
            };

            public static float[] constructionTime =
            {
                252f,
                630f,
                1260f,
                1260f
            };
        }

        /// <summary>
        /// Detects a click from either Mouse1 or Mouse2.
        /// </summary>
        /// <param name="detectLeft">If true, will try and detect a left click.</param>
        /// <returns>Whether or not it has been clicked.</returns>
        public static bool CanDetectClick(bool detectLeft)
        {
            return detectLeft ? Main.mouseLeft && Main.mouseLeftRelease : Main.mouseRight && Main.mouseRightRelease;
        }

        /// <summary>
        /// Generates a color from a gradient.
        /// </summary>
        /// <param name="min">The color at the beginning at the gradient. Present when multiplier is 0f.</param>
        /// <param name="max">The color at the end of the gradient. Present when multiplier is 1f.</param>
        /// <param name="multiplier">The multiplier to shift the gradient.</param>
        /// <returns></returns>
        public static Color Gradient(Color min, Color max, float multiplier)
        {
            Color[] gradient = new Color[] { min, max };
            Color color = new Color(
                (int)(gradient[0].R - ((gradient[0].R - gradient[1].R) * multiplier)), 
                (int)(gradient[0].G - ((gradient[0].G - gradient[1].G) * multiplier)),
                (int)(gradient[0].B - ((gradient[0].B - gradient[1].B) * multiplier)));
            return color;
        }

        /// <summary>
        /// Checks if a value is in the range of two values.
        /// </summary>
        /// <param name="value">The value to check range for.</param>
        /// <param name="min">The minimum value check.</param>
        /// <param name="max">The maximum value check.</param>
        /// <param name="inclusive">If set to true, also count if equal to.</param>
        /// <returns></returns>
        public static bool InRange(float value, float min, float max, bool inclusive = true)
        {
            if (inclusive)
            {
                if (min <= value && value <= max)
                {
                    return true;
                }
            }
            else
            {
                if (min <= value && value < max)
                {
                    return true;
                }
            }
            return false;
        }

        public static string Between(string text, string start, string end)
        {
            if (text.Contains(start) && text.Contains(end))
            {
                int stringStart;
                int stringEnd;
                stringStart = text.IndexOf(start, 0) + start.Length;
                stringEnd = text.IndexOf(end, stringStart);
                return text.Substring(stringStart, stringEnd - stringStart);
            }
            return "";
        }

        public static int GetDownloadCount()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            WebClient client = new WebClient();
            string URL = "https://mirror7.sgkoi.dev/Mods/Details/TerrariaFortress";
            string htmlCode = client.DownloadString(URL);

            htmlCode = Between(htmlCode, "Downloads", "</dd>");
            htmlCode = htmlCode.Remove(0, 58); 
            int downloadCount = int.Parse(htmlCode);
            float round = 100f;
            round = round * (float)Math.Pow(10f, downloadCount.ToString().Length - 4);
            downloadCount = (int)(Math.Floor(downloadCount / round) * round);
            
            return downloadCount;
        }

        public static string DownloadCountFormatted()
        {
            return string.Format("{0:n0}", downloads);
        }

        /// <summary>   
        /// For drawing tiled backgrounds, such as like in TF Item tooltips. It is recommended to use a square for the texture.
        /// </summary>
        public static void DrawBackground(Texture2D texture, Vector2 position, Vector2 dimensions, Vector2 padding, Color color, int cornerSize = 16)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Vector2 sideLength = new Vector2(texture.Width - cornerSize * 2, texture.Height - cornerSize * 2);
            Vector2 sidePosition = new Vector2(texture.Width - cornerSize, texture.Height - cornerSize);
            int sideLengthX = (int)sideLength.X;
            int sideLengthY = (int)sideLength.Y;
            int sidePositionX = (int)sidePosition.X;
            int sidePositionY = (int)sidePosition.Y;
            Rectangle topLeft = new Rectangle(0, 0, cornerSize, cornerSize);
            Rectangle topMiddle = new Rectangle(cornerSize, 0, sideLengthX, cornerSize);
            Rectangle topRight = new Rectangle(sidePositionX, 0, cornerSize, cornerSize);
            Rectangle middleLeft = new Rectangle(0, cornerSize, cornerSize, sideLengthY);
            Rectangle center = new Rectangle(cornerSize, cornerSize, sideLengthX, sideLengthY);
            Rectangle middleRight = new Rectangle(sidePositionX, cornerSize, cornerSize, sideLengthY);
            Rectangle bottomLeft = new Rectangle(0, sidePositionY, cornerSize, cornerSize);
            Rectangle bottomMiddle = new Rectangle(cornerSize, sidePositionY, sideLengthX, cornerSize);
            Rectangle bottomRight = new Rectangle(sidePositionX, sidePositionY, cornerSize, cornerSize);
            float drawScale = 1f;

            spriteBatch.Draw(texture, position + new Vector2(-padding.X, -padding.Y), topLeft, color, default, default, drawScale, SpriteEffects.None, 0f);
            for (int i = 0; i < (dimensions.X - topLeft.Width - topRight.Width + padding.X * 2) / topMiddle.Width; i++)
            {
                spriteBatch.Draw(texture, position + new Vector2(-padding.X + topLeft.Width + i * topMiddle.Width, -padding.Y), i > (dimensions.X - topLeft.Width - topRight.Width + padding.X * 2) / topMiddle.Width - 1 ? new Rectangle(topMiddle.X, topMiddle.Y, (int)(dimensions.X - topLeft.Width - topRight.Width + padding.X * 2) % topMiddle.Width, topMiddle.Height) : topMiddle, color, default, default, drawScale, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(texture, position + new Vector2(dimensions.X - topRight.Width + padding.X, -padding.Y), topRight, color, default, default, drawScale, SpriteEffects.None, 0f);

            for (int i = 0; i < (dimensions.Y - topLeft.Height - bottomLeft.Height + padding.Y * 2) / center.Height; i++)
            {
                spriteBatch.Draw(texture, position + new Vector2(-padding.X, -padding.Y + topLeft.Height + i * middleLeft.Height), i > (dimensions.Y - topLeft.Height - bottomLeft.Height + padding.Y * 2) / center.Height - 1 ? new Rectangle(middleLeft.X, middleLeft.Y, middleLeft.Width, (int)(dimensions.Y - topLeft.Height - bottomLeft.Height + padding.Y * 2) % middleLeft.Height) : middleLeft, color, default, default, drawScale, SpriteEffects.None, 0f);
            }
            for (int i = 0; i < (dimensions.X - middleLeft.Width - middleRight.Width + padding.X * 2) / center.Width; i++)
            {
                for (int j = 0; j < (dimensions.Y - topMiddle.Height - bottomMiddle.Height + padding.Y * 2) / center.Height; j++)
                {
                    spriteBatch.Draw(texture, position + new Vector2(-padding.X + middleLeft.Width + i * center.Width, -padding.Y + topMiddle.Height + j * center.Height), new Rectangle(center.X, center.Y, i > (dimensions.X - middleLeft.Width - middleRight.Width + padding.X * 2) / center.Width - 1 ? (int)(dimensions.X - middleLeft.Width - middleRight.Width + padding.X * 2) % center.Width : center.Width, j > (dimensions.Y - topMiddle.Height - bottomMiddle.Height + padding.Y * 2) / center.Height - 1 ? (int)(dimensions.Y - topMiddle.Height - bottomMiddle.Height + padding.Y * 2) % center.Height : center.Height), color, default, default, drawScale, SpriteEffects.None, 0f);
                }
            }
            for (int i = 0; i < (dimensions.Y - topRight.Height - bottomRight.Height + padding.Y * 2) / center.Height; i++)
            {
                spriteBatch.Draw(texture, position + new Vector2(dimensions.X - middleLeft.Width + padding.X, -padding.Y + topRight.Height + i * middleRight.Height), i > (dimensions.Y - topRight.Height - topRight.Height + padding.Y * 2) / center.Height - 1 ? new Rectangle(middleRight.X, middleRight.Y, middleRight.Width, (int)(dimensions.Y - topRight.Height - bottomRight.Height + padding.Y * 2) % middleRight.Height) : middleRight, color, default, default, drawScale, SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(texture, position + new Vector2(-padding.X, dimensions.Y - bottomRight.Height + padding.Y), bottomLeft, color, default, default, drawScale, SpriteEffects.None, 0f);
            for (int i = 0; i < (dimensions.X - bottomLeft.Width - bottomRight.Width + padding.X * 2) / bottomMiddle.Width; i++)
            {
                spriteBatch.Draw(texture, position + new Vector2(-padding.X + bottomLeft.Width + i * bottomMiddle.Width, dimensions.Y - bottomRight.Height + padding.Y), i > (dimensions.X - bottomLeft.Width - bottomRight.Width + padding.X * 2) / bottomMiddle.Width - 1 ? new Rectangle(bottomMiddle.X, bottomMiddle.Y, (int)(dimensions.X - bottomLeft.Width - bottomRight.Width + padding.X * 2) % bottomMiddle.Width, bottomMiddle.Height) : bottomMiddle, color, default, default, drawScale, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(texture, position + new Vector2(dimensions.X - bottomRight.Width + padding.X, dimensions.Y - bottomRight.Height + padding.Y), bottomRight, color, default, default, drawScale, SpriteEffects.None, 0f);
        }
    }

    public class TerrariaFortress : Mod
    {
        public static TerrariaFortress mod => ModContent.GetInstance<TerrariaFortress>();
        public Texture2D cachedLogo1;
        public Texture2D cachedLogo2;
        public SoundEffect cachedTickSound;
        public SoundEffect cachedOpenedSound;
        public SoundEffect cachedClosedSound;

        public void TFUnload()
        {
            Main.logoTexture = cachedLogo1;
            Main.logo2Texture = cachedLogo2;
            Main.soundMenuTick = cachedTickSound;
            Main.soundMenuOpen = cachedOpenedSound;
            Main.soundMenuClose = cachedClosedSound;
            lock (TFUtils.UITextures.Beige)
            {
                TFUtils.UITextures.Beige = null;
            }
            lock (TFUtils.UITextures.Logo)
            {
                TFUtils.UITextures.Logo = null;
            }
            lock (TFUtils.UITextures.Tooltip)
            {
                TFUtils.UITextures.Tooltip = null;
            }
            lock (TFUtils.UITextures.White)
            {
                TFUtils.UITextures.White = null;
            }
        }

        public void TFLoad()
        {
            TFUtils.downloads = TFUtils.GetDownloadCount();
            cachedLogo1 = Main.logoTexture;
            cachedLogo2 = Main.logo2Texture;
            cachedTickSound = Main.soundMenuTick;
            cachedOpenedSound = Main.soundMenuOpen;
            cachedClosedSound = Main.soundMenuClose;

            TFUtils.UITextures.Beige = mod.GetTexture("Extras/UI1");
            TFUtils.UITextures.Tooltip = mod.GetTexture("Items/TFItemTooltipUI");
            TFUtils.UITextures.White = mod.GetTexture("Extras/UI2");
            TFUtils.UITextures.Logo = mod.GetTexture("Extras/Logo");

            if (ModContent.GetInstance<TFConfig>().showTitleExtras)
            {
                Main.logoTexture = TFUtils.UITextures.Logo;
                Main.logo2Texture = TFUtils.UITextures.Logo;
                Main.soundMenuTick = GetSound("Sounds/Custom/UIHover1");
                Main.soundMenuOpen = GetSound("Sounds/Custom/UIClickFull1");
                Main.soundMenuClose = GetSound("Sounds/Custom/UIClickFull1");
            }
        }

        public override void PreSaveAndQuit()
        {
            TFLoad();
        }

        public override void Load()
        {
            TFLoad();
            if (ModContent.GetInstance<TFConfig>().showTitleExtras)
            {
                On.Terraria.Main.DrawMenu += Main_DrawMenu;
            }
            On.Terraria.Main.DrawInterface_39_MouseOver += Main_DrawInterface_39_MouseOver;
        }

        private void TFDrawBuildingMouseText(Main self)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, self.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player == Main.player[Main.myPlayer])
                {
                    bool mouseOverProjectile = false;
                    string text = "";
                    for (int j = 0; j < Main.maxProjectiles; j++)
                    {
                        Projectile projectile = Main.projectile[i];

                        player.GetModPlayer<TFModPlayer>().mouseBuilding = false;
                        if (projectile.active && projectile.Hitbox.Contains(Main.MouseWorld.ToPoint()) && !player.GetModPlayer<TFModPlayer>().mouseBuilding)
                        {
                            if (projectile.Name != "" && projectile.Name != null)
                            {
                                text = projectile.Name + ": " + projectile.timeLeft + "/" + projectile.timeLeft;
                            }
                            mouseOverProjectile = true;
                            player.GetModPlayer<TFModPlayer>().mouseBuilding = true;
                        }
                    }
                    if (mouseOverProjectile)
                    {
                        Vector2 dimensions = Main.fontMouseText.MeasureString(text);
                        Color color = new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor);
                        Vector2 position = new Vector2(0f, 0f);
                        if (Main.ThickMouse)
                        {
                            position += new Vector2(6f, 6f);
                        }
                        position += Main.MouseScreen;
                        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontMouseText, text, position, color, 0f, Vector2.Zero, Vector2.One);
                    }
                }
            }
            spriteBatch.End();
        }

        private void Main_DrawInterface_39_MouseOver(On.Terraria.Main.orig_DrawInterface_39_MouseOver orig, Main self)
        {
            TFDrawBuildingMouseText(self);
            orig(self);
        }

        public override void Unload()
        {
            TFUnload();
            // Should work. Sadly It is written inefficiently.
        }

        internal static string changelogsString;
        internal static string changelogsStringMessage;
        internal static bool changelogsOpened;
        internal static bool changelogsHovering;
        internal static Rectangle changelogsBox;
        internal static Color hoverColor;
        private void Main_DrawMenu(On.Terraria.Main.orig_DrawMenu orig, Main self, GameTime gameTime)
        {
            if (Main.gameMenu)
            {
                if (Main.menuMode == 0 || Main.menuMode == -5)
                {
                    Vector2 changelogsPosition = new Vector2(30f, 90f);
                    Vector2 changelogsScale = new Vector2(0.35f, 0.35f);
                    DynamicSpriteFont changelogsFont = Main.fontDeathText;
                    SpriteBatch spriteBatch = Main.spriteBatch;
                    Vector2 letsFuckingMeasure;
                    string openString = "Open Terraria Fortress Changelog";
                    string closeString = "Close Terraria Fortress Changelog";
                    changelogsString = "Currently at " + TFUtils.DownloadCountFormatted() + " downloads.\n";

                    if (changelogsBox.Contains(Main.MouseScreen.ToPoint()))
                    {
                        if (TFUtils.CanDetectClick(true))
                        {
                            Main.menuMode = changelogsOpened ? 0 : -5;
                        }
                    }

                    if (!changelogsOpened)
                    {
                        Main.logoTexture = TFUtils.UITextures.Logo;
                        Main.logo2Texture = TFUtils.UITextures.Logo;
                        changelogsString += openString;
                        changelogsStringMessage = "";

                    }
                    else
                    {
                        Main.logoTexture = GetTexture("Extras/FUCK");
                        Main.logo2Texture = GetTexture("Extras/FUCK");
                        changelogsString += closeString;
                        #region The Actual Fucking Log
                        changelogsStringMessage =
"v0.5 - What Valve Couldn't Promise"
+ "\n- Brought back everyone's favorite Soviet giant for the mod icon"
+ "\n- Added this changelog, replaced the main menu logo with the TF logo,"
+ "\nand changed UI sounds to match TF2's. Thanks to Stevie / Ryan for those changes' code"
+ "\n- Bug fixes and changes, specifically:"
+ "\n   * Cleaned up tons of inconsistencies in code"
+ "\n   * Minor visual tweaks all around, i.e. afterburn flames looking more like fire and renaming \"Mann Co. Ammo Box\" to \n\"Mann Co. Medium Ammo Crate\" and updating its sprite"
+ "\n   * A lot of visual changes, including:"
+ "\n       * The player's hand not rotating to the cursor for weapon holdouts"
+ "\n       * The actual sprite not rotating / acting weird with muzzle flashes"
+ "\n       * Sprite repositioning for a lot of holdouts"
+ "\n       * Fixed inconsistencies in the holdouts for melees"
+ "\n       * The Flame Thrower's small blue flame at the end now emits actual light "
+ "\n   and flame dust which reacts to motion / airblast"
+ "\n       * The names of TF items are now in all caps, just like in TF2"
+ "\n   * Fixing inconsistencies with weapons, including:"
+ "\n       * Fixed the Flame Thrower firing without consuming ammo at times"
+ "\n       * A lot of tweaks to airblast, should no longer fire in uncertain directions, extinguish allies properly, and push enemies at a reasonable force"
+ "\n       * Grenade Launcher pills no longer make nonstop impact noises"
+ "\n       * Stickybomb Launcher stickies will now stack damage, so that you can't survive mass explosions and traverse the skies"
+ "\n       * Ignited enemies will now play a burning fwoosh sound upon ignition (Flame Thrower)\n"

+ "\n- Reached 8000+ downloads! Thank you so much for your non-stop support!"
+ "\n- Added Heavy's Minigun, which can rev up and down with right click as well, and stay revved"
+ "\n- Added Heavy's Fists, whose hand's skin color change to your player's while in the inventory"
+ "\n- Added the very infamous random crits! Mini-crits were also added, but are currently unused.\nBuild damage overtime to reach your minimum of 2% to 12% to land a random critical hit when using weapons.\nYour stored chance deteriorates overtime."
+ "\n- Added the Electrocritiogram, a device sold by the Arms Dealer which lets you view your chance to land a random crit"
+ "\n- The mod's download count now updates live each time you load it up"
+ "\n- Added a few Mod Config options to the settings menu:"
+ "\n    * Melee weapon holdout rotation can be toggled"
+ "\n    * Ranged weapon holdout rotation can be toggled"
+ "\n    * Random crits can be toggled; if enabled, you can deal and suffer random crits; if disabled, you can't deal nor suffer random crits";
                        #endregion
                        letsFuckingMeasure = changelogsFont.MeasureString(changelogsStringMessage) * changelogsScale;
                        TFUtils.DrawBackground(TFUtils.UITextures.Beige, new Vector2((int)changelogsPosition.X, (int)changelogsPosition.Y), new Vector2((int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - changelogsPosition.X * 2), (int)letsFuckingMeasure.Y), new Vector2(18f, 0f), Color.White);
                        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, changelogsFont, changelogsStringMessage, changelogsPosition, TFColor[(int)TFColorID.Strange], 0f, new Vector2(0f, 0f), changelogsScale);
                    }

                    letsFuckingMeasure = changelogsFont.MeasureString(closeString);
                    Vector2 logsBoxDimensions = letsFuckingMeasure * changelogsScale;
                    changelogsBox = new Rectangle((int)changelogsPosition.X, 16, (int)logsBoxDimensions.X, (int)logsBoxDimensions.Y);

                    if (changelogsBox.Contains(Main.MouseScreen.ToPoint()))
                    {
                        hoverColor = TFColor[(int)TFColorID.CombatMiniCrit];

                        if (!changelogsHovering)
                        {
                            changelogsHovering = true;
                            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/UIHover1"));
                        }
                        if (TFUtils.CanDetectClick(true))
                        {
                            changelogsOpened = !changelogsOpened;
                            Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/UIClickFull1"));
                        }
                    }
                    else
                    {
                        changelogsHovering = false;
                        hoverColor = TFColor[(int)TFColorID.Strange];
                    }

                    letsFuckingMeasure = changelogsFont.MeasureString(changelogsString) * changelogsScale;
                    TFUtils.DrawBackground(TFUtils.UITextures.Tooltip, new Vector2((int)changelogsPosition.X, changelogsBox.Y), new Vector2((int)letsFuckingMeasure.X, (int)letsFuckingMeasure.Y - 4), new Vector2(18f, 6f), changelogsHovering ? Color.White : new Color(255, 110, 110));
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, changelogsFont, changelogsString, new Vector2(changelogsPosition.X, changelogsBox.Y), hoverColor, 0f, Vector2.Zero, changelogsScale);
                }
                orig(self, gameTime);
            }
        }

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
                            ModContent.ItemType<Minigun>(),
                            ModContent.ItemType<Fists>(),
                        };

                        foreach (int item in shopItems)
                        {
                            AddPurchasable(item, ref nextSlot);
                        }
                    }
                }

                if (type == NPCID.ArmsDealer)
                {
                    if (NPC.downedBoss3)
                    {
                        AddPurchasable(ModContent.ItemType<Electrocritiogram>(), ref nextSlot);
                    }
                }

                if (type == NPCID.PartyGirl)
                {
                    if (NPC.downedBoss3)
                    {
                        AddPurchasable(ModContent.ItemType<NoiseMakerBirthday>(), ref nextSlot);
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