using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaFortress.Dusts;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Projectiles
{
	public class RocketLauncherRocket : TFProjectile
	{
		public override void SetStaticDefaults()
        {
			DisplayName.SetDefault("Rocket");
		}

		public override void SetDefaults()
		{
			projectile.width = 14;
			projectile.height = 14;
			projectile.tileCollide = true;
			projectile.ranged = true;
			projectile.penetrate = -1;
			projectile.alpha = 255;
            projectile.friendly = true;
			projectile.extraUpdates = 1;
			explosiveDamage = true;
        }

        public override void Explode(int radius = 150, float strength = 20)
        {
			strength *= 0.8f;
            base.Explode(radius, strength);
        }

        public override void AI()
        {
			Player player = Main.player[projectile.owner];

			if (projectile.timeLeft % 10 == 0)
			{
				projectile.damage = (int)(MathHelper.Clamp(projectile.damage * 0.965936335f, 24, projectile.damage));
			}

			projectile.rotation = projectile.velocity.ToRotation();

            Dust dust1 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Smoke, -projectile.velocity.X * 0.01f, -projectile.velocity.Y * 0.0f, 100, new Color(200, 200, 200), 1.5f);
            dust1.noGravity = true;
            dust1.noLight = true;
			if (Main.rand.Next(5) == 0)
			{
				Dust dust2 = Dust.NewDustPerfect(projectile.Center + new Vector2(-7.5f, -7.5f), ModContent.DustType<SmallFlash>(), new Vector2(0f, 0f), 0, new Color(255, 200, 0, 255), 0.5f);
			}

			bool runExplosion = false;

			for (int i = 0; i < projectile.width; i++)
            {
				for (int j = 0; j < projectile.height; j++)
                {
					Point point = (projectile.position + new Vector2(i, j)).ToTileCoordinates();

					if (WorldGen.SolidOrSlopedTile(point.X, point.Y))
                    {
						runExplosion = true;
                    }
                }
            }

			if (runExplosion)
            {
				Explode();
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
			Explode();
			return false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
			Explode();
		}

        public override void OnHitPvp(Player target, int damage, bool crit)
        {
			Explode();
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			DrawProjectileTemplate(spriteBatch, Color.White);
			return false;
		}
    }
}