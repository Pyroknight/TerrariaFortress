using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaFortress.Dusts
{
    public class AirblastLines : ModDust
    {
        int frame = 0;
        int frameCounter = 0;
        int fps = 60 / 2;
        public override void OnSpawn(Dust dust)
        {
            dust.frame = new Rectangle(0, 0, 10, 10);
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation = dust.velocity.ToRotation();
            dust.alpha++;
            dust.scale -= 0.02f;
            dust.velocity *= 0.92f;

            if (++frameCounter > fps)
            {
                dust.frame = new Rectangle(0, frame * 10, 10, 10);
                frameCounter = 0;
                if (++frame > 2)
                {
                    frame = 2;
                }
            }

            if (dust.scale <= 0 || dust.alpha >= 255)
            {
                dust.active = false;
            }

            return false;
        }
    }
}