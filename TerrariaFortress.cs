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
            downloadCount = (int)(Math.Floor(downloadCount / 100f) * 100f);
            
            return downloadCount;
        }

        public static int downloads = 0;
    }

    public class TerrariaFortress : Mod
    {
        public override void Load()
        {
            TFUtils.downloads = TFUtils.GetDownloadCount();
            On.Terraria.Main.DrawMenu += Main_DrawMenu;
        }

        internal static string changeLogsString;
        internal static string changeLogsStringMessage;
        internal static bool changelogsOpened;
        internal static Rectangle logsBox;
        internal static Color hoverColor;
        private void Main_DrawMenu(On.Terraria.Main.orig_DrawMenu orig, Main self, GameTime gameTime)
        {
            if (!changelogsOpened)
            {
                changeLogsString = "Open Terraria Fortress Changelogs";
                changeLogsStringMessage = "";

            }
            else
            {
                changeLogsString = "Close Terraria Fortress Changelogs";
                changeLogsStringMessage = "v0.5 - What Valve Couldn't Promise"
+ "\n- Brought back everyone's favorite Soviet giant for the mod icon"
 + "\n- Bug fixes and changes, specificaly:"
                + "\n* Cleaned up tons of inconsistencies in code"
               + "\n* Minor visual tweaks all around, i.e.afterburn flames looking more like fire and renaming \"Mann Co. Ammo Box\" to \n\"Mann Co. Medium Ammo Crate\" and updating its sprite"
  + "\n*A lot of visual changes, including:"
      + "\n* The player's hand not rotating to the cursor for weapon holdouts"
     + "\n* The actual sprite not rotating / acting weird with muzzle flashes"
      + "\n* Sprite repositioning for a lot of holdouts"
     + "\n* Fixed inconsistencies in the holdouts for melees"
     + "\n* The Flame Thrower's small blue flame at the end now emits actual light "
+ "\nand flame dust which reacts to motion / airblast"
     + "\n* The names of TF items are now in all caps, just like in TF2"
 + "\n* Fixing inconsistencies with weapons, including:\n"

     + "\n* Fixed the Flame Thrower firing without consuming ammo at times"
     + "\n* A lot of tweaks to airblast, should no longer fire in uncertain directions, extinguish allies properly, and push enemies at a reasonable force"
     + "\n* Grenade Launcher pills no longer make nonstop impact noises"
     + "\n* Stickybomb Launcher stickies will now stack damage, so that you can't survive mass explosions and traverse the skies"
 + "\n* Ignited enemies will now play a burning fwoosh sound upon ignition(Flame Thrower)\n"

+ "\n- Reached 8000 + downloads! Thank you so much for your non-stop support!"
 + "\n- Added Heavy's Minigun, which can rev up and down with right click as well, and stay revved"
 + "\n- Added Heavy's Fists, whose hand's skin color change to your player's while in the inventory"
 + "\n- Added the very infamous random crits! Mini - crits were also added, but are currently unused.\nBuild damage overtime to reach your minimum of 2 % to 12 % to land a random critical hit when using weapons.\nYour stored chance deteriorates overtime.";

                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontDeathText, changeLogsStringMessage, new Vector2(10, 25), Color.White, 0f, Vector2.Zero, new Vector2(0.25f, 0.25f));
            }

            Vector2 letsFuckingMeasure = Main.fontDeathText.MeasureString(changeLogsString);

            float x = (int)letsFuckingMeasure.X * 0.25f;
            float y = (int)letsFuckingMeasure.Y * 0.25f;
            logsBox = new Rectangle(10, 10, (int)x, (int)y);

            if (logsBox.Contains(Main.MouseScreen.ToPoint()))
            {
                hoverColor = Color.Yellow;

                if (TFUtils.CanDetectClick(true))
                {
                    changelogsOpened = !changelogsOpened;
                    Main.PlaySound(!changelogsOpened ? SoundID.MenuClose : SoundID.MenuOpen);
                }
            }
            else
            {
                hoverColor = Color.White;
            }

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, Main.fontDeathText, changeLogsString, new Vector2(10, 10), hoverColor, 0f, Vector2.Zero, new Vector2(0.25f, 0.25f));

            orig(self, gameTime);
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