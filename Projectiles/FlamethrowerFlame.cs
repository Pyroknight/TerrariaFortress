using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaFortress.Buffs;
using TerrariaFortress.Dusts;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Projectiles
{
	public class FlamethrowerFlame : TFProjectile
	{
		public int dustScaleTimer = 0;

		public override void SetStaticDefaults()
        {
			DisplayName.SetDefault("Flames");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			projectile.width = 14;
			projectile.height = 14;
			projectile.tileCollide = true;
			projectile.ranged = true;
			projectile.penetrate = -1;
			projectile.alpha = 255;
			projectile.timeLeft = 120;
			projectile.extraUpdates = 3;
            projectile.friendly = true;
			fireDamage = true;
        }

		public override void AI()
        {
			projectile.rotation = projectile.velocity.ToRotation();
			Player player = Main.player[projectile.owner];

			dustScaleTimer++;

			if (projectile.wet && !projectile.lavaWet)
            {
				for (int i = 0; i < 4; i++)
				{
					Dust dust1 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, ModContent.DustType<AirblastBubble>(), projectile.velocity.X * 0.5f, projectile.velocity.Y * 0.5f, 0, default, 1f);
					dust1.noGravity = true;
				}
				projectile.Kill();
            }

			if (projectile.alpha > 95)
            {
				projectile.alpha -= 20;
            }

			if (++projectile.frameCounter >= 5)
			{
				projectile.damage = (int)(projectile.damage * 0.965936335f);

				projectile.frameCounter = 0;
				if (projectile.frame < 19)
				{
					projectile.frame++;
				}
			}

			if (projectile.frame >= 4 && projectile.frame <= 15)
            {
				projectile.velocity.Y -= 0.03f;
			}

			projectile.velocity *= 0.97f;

			Color color = new Color(255, 0, 0);
			int dustType = 64;
			int alpha = 240;
			if (projectile.frame >= 0 && projectile.frame <= 1)
			{
				color = default;
				dustType = 211;
				alpha = 0;
			}
			else if (projectile.frame == 2)
			{
				color = new Color(210, 150, 210);
				dustType = 211;
				alpha = 0;

			}
			else if (projectile.frame >= 3 && projectile.frame <= 4)
			{
				color = new Color(220, 60, 60);
				dustType = 64;
				alpha = 127;

			}
			else if (projectile.frame >= 5 && projectile.frame <= 11)
			{
				color = new Color(255, 0, 0);
				dustType = 64;
				alpha = 200;

			}
			else if (projectile.frame >= 12 && projectile.frame <= 15)
			{
				color = new Color(200, 50, 50);
				dustType = 64;
				alpha = 230;
			}
			else if (projectile.frame >= 16 && projectile.frame <= 19)
			{
				color = new Color(200, 50, 50);
				dustType = 54;
				alpha = 252;
			}

			Point point = projectile.Center.ToTileCoordinates();
			if (!projectile.wet && !WorldGen.SolidOrSlopedTile(point.X, point.Y))
			{
				int R = 255, G = 127, B = 0;
				float brightness = 1.2f;
				Lighting.AddLight(projectile.Center, R / 255f * brightness, G / 255f * brightness, B / 255f * brightness);

				if (Main.rand.Next(4) == 0)
                {
                    Dust dust1 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, dustType, projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, alpha, color, MathHelper.Clamp(dustScaleTimer * 0.2f, 0f, 7f));
                    dust1.noGravity = true;
                    dust1.noLight = true;
					Dust dust2 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Smoke, projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, 200, new Color(255, 0, 0), 3f);
					dust2.noGravity = true;
					dust2.noLight = true;
				}

				Dust dust3 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.FlameBurst, projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, 200, default, 0.5f);
				dust3.noGravity = true;
				dust3.noLight = true;
			}
		}

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
			if (!target.buffImmune[BuffID.OnFire])
			{
				if (!target.HasBuff(ModContent.BuffType<Afterburn>()))
				{
					target.AddBuff(ModContent.BuffType<Afterburn>(), 210, true);
				}
				else if (target.buffTime[target.FindBuffIndex(ModContent.BuffType<Afterburn>())] < 600)
				{
					target.buffTime[target.FindBuffIndex(ModContent.BuffType<Afterburn>())] += 60;
				}
			}
		}

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
			if (!target.buffImmune[BuffID.OnFire])
			{
				if (!target.HasBuff(ModContent.BuffType<Afterburn>()))
				{
					target.AddBuff(ModContent.BuffType<Afterburn>(), 210, true);
				}
				else if (target.buffTime[target.FindBuffIndex(ModContent.BuffType<Afterburn>())] < 600)
				{
					target.buffTime[target.FindBuffIndex(ModContent.BuffType<Afterburn>())] += 60;
				}
			}
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			Texture2D texture = Main.projectileTexture[projectile.type];
			SpriteEffects spriteEffect = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Rectangle rectangle = texture.Frame(1, 20, 0, projectile.frame);

			for (int i = 0; i < projectile.oldPos.Length; i++)
            {
				Color drawColor = Color.White * ((float)(projectile.oldPos.Length - i) / (float)projectile.oldPos.Length);
				spriteBatch.Draw(texture, projectile.oldPos[i] + (projectile.Center - projectile.position) - Main.screenPosition + new Vector2(-30f, 0f).RotatedBy(projectile.rotation), rectangle, drawColor, projectile.oldRot[i], rectangle.Size() * 0.5f, projectile.scale, spriteEffect, 0f);
			}
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(-30f, 0f).RotatedBy(projectile.rotation), rectangle, Color.White, projectile.rotation, rectangle.Size() * 0.5f, projectile.scale, spriteEffect, 0f);

            return false;
        }
    }
}