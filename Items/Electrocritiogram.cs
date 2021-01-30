using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items
{
    public class Electrocritiogram : TFItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Electrocritiogram");
        }

        public override void TFDefaults()
        {
            TFItemType = "Critical Synapse Detector";
            TFItemQuality = (int)TFColorID.Unique;
            TFItemLevel = 5;
            item.width = 44;
            item.height = 40;
            item.value = Item.buyPrice(gold: 20);
            item.autoReuse = true;
        }

        public override void TFDescription(List<TooltipLine> tooltips)
        {
            Player player = Main.player[item.owner];
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributePositive], "Shows your chance of landing");
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributePositive], "a random critical hit");
            Color color = Color.White;
            float multiplier = (player.GetModPlayer<TFModPlayer>().storedRandomCritChance - player.GetModPlayer<TFModPlayer>().randomCritMinMax[0]) / 10f;
            if (TFUtils.InRange(multiplier, 0f, 0.5f))
            {
                color = TFUtils.Gradient(new Color(255, 0, 0), new Color(255, 255, 0), multiplier * 2f);
            }
            if (TFUtils.InRange(multiplier, 0.5f, 1f))
            {
                color = TFUtils.Gradient(new Color(255, 255, 0), new Color(0, 255, 0), multiplier * 2f);
            }
            float chance = (float)Math.Floor(player.GetModPlayer<TFModPlayer>().storedRandomCritChance * 10f) / 10f;
            string chanceText = chance + "% " + (chance == player.GetModPlayer<TFModPlayer>().randomCritMinMax[1] ? "(MAX)" : "");
            if (item.buy)
            {
                chanceText = "(Purchase Electriocritiogram to show chance)";
                color = new Color(255, 0, 0);
            }   
            string text = "RCC: " + chanceText;
            AddTFAttribute(tooltips, color, text);
        }
    }
}