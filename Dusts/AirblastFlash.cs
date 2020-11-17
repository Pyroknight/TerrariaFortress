using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace TerrariaFortress.Dusts
{
    public class AirblastFlash : ModDust
    {
        private int flashTimer = 0;
        public override void OnSpawn(Dust dust)
        {
            dust.frame = new Rectangle(0, 0, 30, 30);
            dust.shader = GameShaders.Armor.GetSecondaryShader(1, Main.LocalPlayer);
        }
        public override bool Update(Dust dust)
        {
            //dust.rotation = dust.velocity.ToRotation();
            flashTimer++;

            if (flashTimer <= 5)
            {
                dust.alpha -= 52;
            }
            if (flashTimer >= 6)
            {
                dust.alpha += 26;
            }

            if (dust.alpha > 255)
            {
                dust.active = false;
            }

            if (!dust.noLight)
            {
                int R = 255, G = 255, B = 255;
                float brightness = 0.4f;
                Lighting.AddLight(dust.position, R / 255f * brightness, G / 255f * brightness, B / 255f * brightness);
            }
            return false;
        }

        public override Color? GetAlpha(Dust dust, Color lightColor)
            => new Color(255 - dust.alpha, 255 - dust.alpha, 255 - dust.alpha, 255 - dust.alpha);
    }
}