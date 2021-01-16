using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaFortress.Dusts
{
    public class BirthdayBalloon : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.frame = new Rectangle(Main.rand.Next(8) * 22, 0, 22, 70);
            //switch (Main.rand.Next(4))
            //{
            //    case 0:
            //        dust.frame = new Rectangle(Main.rand.Next(8) * 22, 0, 22, 70);
            //        break;
            //    case 1:
            //    case 2:
            //    case 3:
            //        switch (Main.rand.Next(2))
            //        {
            //            case 0:
            //                dust.frame = new Rectangle(44, 0, 22, 70);
            //                break;
            //            case 1:
            //                dust.frame = new Rectangle(154, 0, 22, 70);
            //                break;
            //        }
            //        break;
            //}
        }
        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.rotation = dust.velocity.X * 0.2f;
            dust.velocity.X *= 0.97f;
            dust.scale -= 0.01f;
            dust.alpha += 3;

            if (dust.scale <= 0 || dust.alpha > 255)
            {
                dust.active = false;
            }

            if (!dust.noGravity)
            {
                dust.velocity.Y -= 0.03f;
            }
            return false;
        }
    }
}