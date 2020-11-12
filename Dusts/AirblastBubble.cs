using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaFortress.Dusts
{
    public class AirblastBubble : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.frame = new Rectangle(0, 0, 10, 10);
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.9f;
            dust.rotation += dust.velocity.X * 0.5f;
            dust.alpha += 2;
            dust.scale -= 0.05f;

            if (dust.scale <= 0)
            {
                dust.active = false;
            }

            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
            => default;
    }
}