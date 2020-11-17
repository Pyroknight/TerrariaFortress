using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaFortress.Buffs;
using TerrariaFortress.Dusts;

namespace TerrariaFortress.Projectiles
{

	public class ShotgunBullet : ModProjectile
	{
		public override void SetStaticDefaults()
        {
			DisplayName.SetDefault("Shotgun");
		}

		public override void SetDefaults()
		{
			projectile.width = 2;
			projectile.height = 2;
			projectile.tileCollide = true;
			projectile.ranged = true;
			projectile.penetrate = -1;
			projectile.alpha = 255;
			projectile.extraUpdates = 3;
            projectile.friendly = true;
			projectile.ignoreWater = true;
        }

		public override void AI()
        {
			projectile.rotation = projectile.velocity.ToRotation();
			Vector2 velocity = projectile.velocity;
			velocity.Normalize();
			projectile.velocity = velocity * (64f / projectile.extraUpdates);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Collision.HitTiles(projectile.position, -projectile.velocity, projectile.width, projectile.height);
			return true;
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			Texture2D texture = Main.projectileTexture[projectile.type];
			SpriteEffects spriteEffect = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Rectangle rectangle = texture.Frame(1, 1, 0, 0);

			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(-10, 0f).RotatedBy(projectile.rotation), rectangle, Color.White, projectile.rotation, rectangle.Size() * 0.5f, projectile.scale, spriteEffect, 0f);
			return false;
        }
    }
}