using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
using VectorMod.Projectiles.BuffedLunarItems.BetterBeaterProj;

namespace VectorMod
{
    public class HoldoutDrawLayer : ModPlayer
    {
        public static readonly PlayerLayer HeldItem = new PlayerLayer("VectorMod", "HeldItem", PlayerLayer.HeldItem, delegate (PlayerDrawInfo drawInfo)
        {
            Mod mod = ModLoader.GetMod("VectorMod");
            Player player = drawInfo.drawPlayer;
            Color colorBase = Lighting.GetColor((int)(player.Center.X / 16), (int)(player.Center.Y / 16));
            int alphaOffset = (int)(Math.Abs(player.stealth - 1) * 255);
            Color color = new Color(colorBase.R - alphaOffset, colorBase.G - alphaOffset, colorBase.B - alphaOffset, colorBase.A - alphaOffset);
            SpriteEffects spriteEffect = (player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (player.gravDir == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None);
            bool drawConditions = !player.dead && player.active && !player.invis && !player.frozen && !player.stoned && player.HeldItem.modItem is ModItem item && item.CanUseItem(player);

            void DrawHoldoutSimple(Texture2D texture, float horizontalOffset, float verticalOffset, float rotation, Color suppliedColor)
            {
                Rectangle rectangle = texture.Frame(1, 12, 0, 0);
                Vector2 drawPos = (player.MountedCenter - Main.screenPosition + new Vector2(player.direction * horizontalOffset, player.gfxOffY + verticalOffset)).Floor();
                Vector2 origin = player.direction == -1 ? rectangle.Size() * 0.5f + new Vector2(horizontalOffset + player.direction + rectangle.Width * 0.5f, verticalOffset) : rectangle.Size() * 0.5f + new Vector2(-horizontalOffset + player.direction - rectangle.Width * 0.5f, verticalOffset);
                Main.playerDrawData.Add(new DrawData(texture, drawPos, rectangle, suppliedColor, rotation, origin, player.HeldItem.scale, spriteEffect, 0));
            }

            if (drawConditions)
            {
                if (player.HeldItem.type == ModContent.ItemType<BetterBeaterProj>() && player.controlUseItem)
                {
                    float xOffset = 0f, yOffset = 0f;
                    DrawHoldoutSimple(ModContent.GetTexture("VectorMod/Projectiles/BuffedLunarItems/BetterBeaterProj"), xOffset, yOffset, player.itemRotation, color);
                    DrawHoldoutSimple(ModContent.GetTexture("VectorMod/Projectiles/BuffedLunarItems/BetterBeaterProj_Glow"), xOffset, yOffset, player.itemRotation, Color.White);
                }
            }
        });

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            HeldItem.visible = true;
            int index = layers.FindIndex(heldItem => heldItem == PlayerLayer.HeldItem);

            if (index != -1)
            {
                layers.Insert(index, HeldItem);
            }
        }
    }
}