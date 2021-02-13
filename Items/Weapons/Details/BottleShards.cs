using Terraria;
using static TerrariaFortress.TerrariaFortress;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TerrariaFortress.Items.Weapons.Details
{
    public class BottleShards : ModGore
    {
        public override void OnSpawn(Gore gore)
        {
            gore.numFrames = 4;
            gore.timeLeft = 360;
        }

        public override bool Update(Gore gore)
        {
            gore.velocity.Y += 0.1f;
            gore.rotation += gore.velocity.ToRotation() * 0.1f;

            if (gore.timeLeft <= 255)
            {
                gore.alpha++;
            }
            if (gore.alpha >= 255 || gore.scale <= 0f)
            {
                gore.active = false;
            }

            if (gore.alpha >= 100)
            {
                gore.scale -= 0.01f;
            }
            return false;
        }

        public override Color? GetAlpha(Gore gore, Color lightColor)
        {
            return lightColor * Math.Abs(gore.alpha - 255) * (1 / 255f);
        }
    }
}