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

namespace TerrariaFortress
{
    public static class TFUtils
    {
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