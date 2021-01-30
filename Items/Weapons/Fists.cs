using Terraria;
using static TerrariaFortress.TerrariaFortress;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.ID;

namespace TerrariaFortress.Items.Weapons
{
    public class Fists : TFItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fists");
        }

        public override void TFDefaults()
        {
            TFItemType = "Fists";
            isMelee = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 65;
            item.knockBack = 10;
            item.width = 42;
            item.height = 46;
            item.useTime = 49;
            item.useAnimation = 49;
            item.useStyle = ItemUseStyleID.EatingUsing;
            item.holdStyle = ItemHoldStyleID.Default;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override void SetConstantDefaults()
        {
            basicUseSound = "Sounds/Custom/" + (critting ? "FistsSwingCrit" : "FistsSwing" + Main.rand.Next(1, 4));
            meleeEntityCollisionSound = "FistsHit" + Main.rand.Next(1, 3);
            meleeWorldCollisionSound = "FistsHit" + Main.rand.Next(1, 3);
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Player player = Main.player[item.owner];
            Texture2D texture = ModContent.GetTexture("TerrariaFortress/Items/Weapons/FistsSkin");
            spriteBatch.Draw(texture, position, frame, player.skinColor * 1.2f, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            hitbox = TFMeleeHitbox(player, 0f, -36f, 52, 56);
            base.UseItemHitbox(player, ref hitbox, ref noHitbox);
        }
    }
}