using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria.Enums;
using TerrariaFortress.Buffs;
using TerrariaFortress.Dusts;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Projectiles
{
	public class FlamethrowerAirblast : TFProjectile
	{
		public int dustScaleTimer = 0;
		public bool runFunctions = true;
		public Rectangle airblastFunctionHitbox;

		public override void SetStaticDefaults()
        {
			DisplayName.SetDefault("Airblast");
		}

		public override void SetDefaults()
		{
			projectile.width = 14;
			projectile.height = 14;
			projectile.tileCollide = true;
			projectile.ranged = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 45;
			projectile.friendly = true;
			projectile.extraUpdates = 3;
		}

        public override void CritBoost()
        {

        }

        public override void MiniCritBoost()
        {

        }

        public override void TFAI()
        {
			Player player = Main.player[projectile.owner];
			projectile.rotation = projectile.velocity.ToRotation();

			if (projectile.wet)
            {
				projectile.Kill();
            }

			if (dustScaleTimer >= 1)
            {
				runFunctions = false;
            }
			projectile.velocity *= 0.97f;

			Point point = projectile.Center.ToTileCoordinates();

			if (!projectile.wet && !WorldGen.SolidOrSlopedTile(point.X, point.Y))
            {
                Dust dust1 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Smoke, projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, 200, new Color(220, 220, 220), MathHelper.Clamp(dustScaleTimer * 0.2f, 0f, 5f) + 1f);
                dust1.noGravity = true;
                dust1.noLight = true;
                Dust dust2 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Smoke, projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, 100, new Color(255, 255, 255), 1f);
                dust2.noGravity = true;
                dust2.noLight = true;
            }

			if (runFunctions)
			{
				Airblast(player);
			}

			dustScaleTimer++;
		}

        public override bool? CanCutTiles()
        {
			return false;
        }

        public override bool CanDamage()
        {
			return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            //spriteBatch.Draw(ModContent.GetTexture("TerrariaFortress/Projectiles/HitboxTest"), new Vector2(airblastFunctionHitbox.X, airblastFunctionHitbox.Y) - Main.screenPosition, airblastFunctionHitbox, Color.White, 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, 0f);
            return false;
        }
    }
}