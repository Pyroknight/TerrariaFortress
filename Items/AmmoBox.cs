using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items
{
    public class AmmoBox : TFWeapon
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Mann Co. Ammo Box");
        }

        public override void SafeSetDefaults()
        {
            item.width = 28;
            item.height = 20;
            item.maxStack = 999;
            item.value = 1;
            item.consumable = true;
            item.ammo = item.type;
        }

        public override bool OnPickup(Player player)
        {
            Main.PlaySound(SoundLoader.customSoundType, (int)item.Center.X, (int)item.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/AmmoPickup"));
            return true;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.GetTexture("TerrariaFortress/Items/AmmoBoxWorld");

            spriteBatch.Draw(texture, item.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), item.GetAlpha(lightColor), rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeNeutral], "Inside lie universal ammunitions for");
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeNeutral], "all weapons shipped from Mann Co.");
        }
    }
}