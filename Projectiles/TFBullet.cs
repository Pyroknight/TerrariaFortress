using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;

namespace TerrariaFortress.Projectiles
{
	public class TFBullet : TFProjectile
	{
		public override void SetStaticDefaults()
        {
			DisplayName.SetDefault("Bullet");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			projectile.penetrate = 1;
			projectile.width = 2;
			projectile.height = 2;
			projectile.tileCollide = true;
			projectile.ranged = true;
			projectile.penetrate = 1;
            projectile.friendly = true;
			projectile.ignoreWater = true;
			projectile.extraUpdates = 4;
			//damageFalloffCounter = 20 * (projectile.extraUpdates + 1);
			rangedDamage = true;
		}

		public override void TFAI()
        {
			projectile.rotation = projectile.velocity.ToRotation();
            //if (projectile.timeLeft % 4 == 0 && damageFalloffCounter > 0)
            //{
            //    projectile.damage = (int)(projectile.damage * 0.9999f);
            //    damageFalloffCounter--;
            //}
            CastLight(new Color(255, 127, 0), 0.8f);
		}

		public override void TFModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			Player player = Main.player[projectile.owner];
			float distance = Vector2.Distance(player.MountedCenter, target.Center);
			float modifier = 192f;
			if (distance == 0f)
            {
				distance = modifier;
            }

			if (critting)
			{

			}
			else
            {
				damage = (int)(damage / (MathHelper.Clamp(distance, modifier, distance) / modifier));
			}
		}

        public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
			Main.PlaySound(SoundID.Dig, projectile.Center);
			projectile.Kill();
			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			DrawProjectileTemplate(spriteBatch, Color.White);
			return false;
		}
	}
}