using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaFortress.Dusts
{
    public class AfterburnFlames : ModDust
    {
        int frame = 0;
        int frameCounter = 0;
        int fps = 60 / 20;
        public override void OnSpawn(Dust dust)
        {
            dust.frame = new Rectangle(0, Main.rand.Next(4) * 14, 8, 14);
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.9f;
            dust.scale -= 0.05f;

            if (++frameCounter > fps)
            {
                dust.frame = new Rectangle(0, frame * 14, 8, 14);
                frameCounter = 0;
                if (++frame > 3)
                {
                    frame = 0;
                }
            }

            if (dust.scale <= 0)
            {
                dust.active = false;
            }

            if (!dust.noGravity)
            {
                dust.velocity.Y -= 0.1f;
            }

            if (!dust.noLight)
            {
                int R = 255, G = 127, B = 0;
                float brightness = 0.4f;
                Lighting.AddLight(dust.position, R / 255f * brightness, G / 255f * brightness, B / 255f * brightness);
            }
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
            => new Color(255 - dust.alpha, 255 - dust.alpha, 255 - dust.alpha, 255 - dust.alpha);
    }
}