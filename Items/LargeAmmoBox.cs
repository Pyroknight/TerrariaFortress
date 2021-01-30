using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items
{
    public class LargeAmmoBox : TFItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mann Co. Large Ammo Crate");
        }

        public override void TFDefaults()
        {
            item.width = 30;
            item.height = 30;
            item.maxStack = 1;
            item.value = Item.sellPrice(gold: 1);
            item.ammo = ModContent.ItemType<AmmoBox>();
            item.consumable = false;
        }

        public override bool OnPickup(Player player)
        {
            Main.PlaySound(SoundLoader.customSoundType, (int)item.Center.X, (int)item.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/AmmoPickup"));

            return true;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.GetTexture("TerrariaFortress/Items/LargeAmmoBoxWorld");
            spriteBatch.Draw(texture, item.Bottom - Main.screenPosition + new Vector2(0f, texture.Height * -0.5f), texture.Frame(1, 1, 0, 0), alphaColor, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void TFDescription(List<TooltipLine> tooltips)
        {
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributePositive], "Has infinite uses");
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeNeutral], "Inside lie universal ammunitions for");
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeNeutral], "all weapons shipped from Mann Co.");
        }
    }
}