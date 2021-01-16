using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static TerrariaFortress.TerrariaFortress;
using Terraria.ModLoader;
using System;

namespace TerrariaFortress.Projectiles
{
	public class GrenadeLauncherGrenade : TFProjectile
	{
		public bool canExplode = true;
		public int explosionTimer;
		public int bounceCounter;
		public int bounceCounterMax = 5;
		public override void SetStaticDefaults()
        {
			DisplayName.SetDefault("Grenade");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			projectile.width = 8;
			projectile.height = 8;
			projectile.tileCollide = true;
			projectile.ranged = true;
			projectile.penetrate = -1;
            projectile.friendly = true;
			explosiveDamage = true;
        }

        public override void Explode(int radius = 150, float strength = 20f)
        {
			strength *= 0.5f;
			base.Explode(radius, strength);
        }

        public override void AI()
        {
			Player player = Main.player[projectile.owner];

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(Math.Abs(projectile.velocity.X + projectile.velocity.Y) * 0.5f * explosionTimer);

			float gravity = 0.3f;
			if (projectile.velocity.Y <= 13f)
			{
				projectile.velocity.Y += gravity;
			}

			for (int i = 0; i < projectile.width; i++)
            {
				for (int j = 0; j < projectile.height; j++)
                {
					Point point = (projectile.position + new Vector2(i, j)).ToTileCoordinates();

					if (WorldGen.SolidOrSlopedTile(point.X, point.Y))
                    {
						canExplode = false;
                    }
                }
            }

			if (explosionTimer > 0)
			{
				explosionTimer--;
			}
			else if (explosionTimer == 0)
            {
				Explode();
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (canExplode)
            {
				Explode();
            }
        }

		public override void OnHitPvp(Player target, int damage, bool crit)
		{
			if (canExplode)
			{
				Explode();
			}
		}

		public override bool CanDamage()
        {
			return canExplode;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
			float bounceMultiplier = 0.3f;
			if (projectile.velocity.X != oldVelocity.X)
			{
				projectile.velocity.X = -oldVelocity.X * bounceMultiplier;
			}
			if (projectile.velocity.Y != oldVelocity.Y)
			{
				projectile.velocity.Y = -oldVelocity.Y * bounceMultiplier * 2f;
			}

			float soundCheckVelocity = (Math.Abs(projectile.velocity.X) + Math.Abs(projectile.velocity.Y)) * 0.5f;
			if (soundCheckVelocity >= 2f && bounceCounter < bounceCounterMax)
			{
				bounceCounter++;
				Main.PlaySound(SoundLoader.customSoundType, (int)projectile.Center.X, (int)projectile.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/GrenadeLauncherImpact" + Main.rand.Next(1, 4)));
			}
					
			canExplode = false;
			return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			DrawProjectileTemplate(spriteBatch, Color.White, 1, 3, 0, 2, DrawMode.TrailOnly);
			DrawProjectileTemplate(spriteBatch, lightColor, 1, 3, 0, 0, DrawMode.ProjectileOnly);
			DrawProjectileTemplate(spriteBatch, lightColor, 1, 3, 0, 1, DrawMode.ProjectileOnly);
			return false;
		}
    }
}