using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items
{
    public class AmmoBox : TFItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mann Co. Medium Ammo Crate");
        }

        public override void TFDefaults()
        {
            item.width = 28;
            item.height = 20;
            item.maxStack = 999;
            item.value = Item.buyPrice(copper: 7);
            item.ammo = item.type;
        }

        public override bool OnPickup(Player player)
        {
            for (int i = 0; i < Main.maxInventory; i++)
            {
                Item inventoryItem = player.inventory[i];
                if (!inventoryItem.IsAir) // iirc this is the method, dont really remember
                {
                    Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/AmmoPickup"));
                }
            }
            return base.OnPickup(player);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.GetTexture("TerrariaFortress/Items/AmmoBoxWorld");
            spriteBatch.Draw(texture, item.Center - Main.screenPosition, texture.Frame(1, 1, 0, 0), alphaColor, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void TFDescription(List<TooltipLine> tooltips)
        {
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeNeutral], "Inside lie universal ammunitions for");
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeNeutral], "all weapons shipped from Mann Co.\n");
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeUseLimit], "This is a limited use item. Uses: " + item.stack);
        }
    }
}