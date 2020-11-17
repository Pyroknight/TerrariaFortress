using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaFortress.Dusts;
using Terraria.Enums;
using static TerrariaFortress.TerrariaFortress;
using System;
using Terraria.DataStructures;

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
        }

		public override void AI()
        {
			Player player = Main.player[projectile.owner];

			if (projectile.timeLeft % 10 == 0)

			projectile.damage = (int)(MathHelper.Clamp(projectile.damage * 0.965936335f, 24, projectile.damage));

			projectile.rotation = projectile.velocity.ToRotation();

            Dust dust1 = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Smoke, -projectile.velocity.X * 0.01f, -projectile.velocity.Y * 0.0f, 100, new Color(200, 200, 200), 1.5f)];
            dust1.noGravity = true;
            dust1.noLight = true;
			if (Main.rand.Next(5) == 0)
			{
				Dust.NewDustPerfect(projectile.Center + new Vector2(-7.5f, -7.5f), ModContent.DustType<AirblastFlash>(), new Vector2(0f, 0f), 0, new Color(255, 200, 0, 255), 0.5f);
			}
        }

		public void Explode()
        {
			Player player = Main.player[projectile.owner];

			Main.PlaySound(SoundLoader.customSoundType, (int)projectile.Center.X, (int)projectile.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/RocketLauncherExplode" + Main.rand.Next(1, 4)));
			int R = 255, G = 64, B = 0;
			float brightness = 1.4f;
			Lighting.AddLight(projectile.Center, R / 255f * brightness, G / 255f * brightness, B / 255f * brightness);

			Dust.NewDustPerfect(projectile.Center + new Vector2(-45f, -45f), ModContent.DustType<LargeFlash>(), new Vector2(0f, 0f), 0, new Color(255, 240, 0, 255), 1f);

			int circleAmount = Main.rand.Next(30, 61);
			for (int k = 0; k < circleAmount; k++)
			{
				if (Main.rand.Next(3) == 0)
				{
					for (int i = 0; i < 5; i++)
					{
						if (Main.rand.Next(3) == 0)
						{
							Dust dust1 = Dust.NewDustPerfect(projectile.Center + new Vector2(0f, -i).RotatedBy(MathHelper.ToRadians(k * (360f / circleAmount))), DustID.Smoke, new Vector2(0f, -i - 1f).RotatedBy(MathHelper.ToRadians(k * (360f / circleAmount))), 50, new Color(200, 200, 200), 3f);
							dust1.noGravity = true;
							dust1.noLight = true;
						}
					}

					Dust dust2 = Dust.NewDustPerfect(projectile.Center + new Vector2(0f, 0f).RotatedBy(MathHelper.ToRadians(k * (360f / circleAmount))), DustID.Fire, new Vector2(0f, -5f).RotatedBy(MathHelper.ToRadians(k * (360f / circleAmount))) + new Vector2(0f, -1f), 100, default, 3f);
					dust2.noGravity = true;
					dust2.noLight = true;

					if (Main.rand.Next(2) == 0)
					{
						Dust dust3 = Dust.NewDustPerfect(projectile.Center + new Vector2(0f, -4f).RotatedBy(MathHelper.ToRadians(k * (360f / circleAmount))), ModContent.DustType<AirblastLines>(), new Vector2(0f, -8f).RotatedBy(MathHelper.ToRadians(k * (360f / circleAmount))) + new Vector2(0f, -1f), 0, default, 1f);
						dust3.noGravity = true;
						dust3.noLight = true;
					}
				}
			}
			Rectangle explosion = new Rectangle((int)projectile.Center.X - 125, (int)projectile.Center.Y - 125, 250, 250);

			if (explosion.Intersects(player.Hitbox) && player.active)
			{
				player.jump = 1;
				player.velocity.X += (projectile.DirectionTo(player.Center) / (projectile.Distance(player.Center) * 0.03f) * projectile.velocity.Length() * 3f).X;
				player.velocity.Y += (projectile.DirectionTo(player.Center) / (projectile.Distance(player.Center) * 0.03f)* projectile.velocity.Length() * 4f).Y;
				player.GetModPlayer<TFModPlayer>().isRocketJumping = true;
				player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was blown up."), (int)(projectile.damage * 0.5f - Math.Abs(projectile.Distance(player.Center) * 0.2f)), 0, false, true, false, -1);
			}

			for (int i = 0; i < Main.maxPlayers; i++)
            {
				Rectangle hitbox = explosion;
				Player playerToIntersect = Main.player[i];
				if (hitbox.Intersects(playerToIntersect.Hitbox) && playerToIntersect.whoAmI != player.whoAmI && playerToIntersect.active)
				{
					if (player.team != (int)Team.None && player.team != playerToIntersect.team && player.hostile == playerToIntersect.hostile && player.hostile)
					{
						playerToIntersect.jump = 1;
						playerToIntersect.velocity.X += (projectile.DirectionTo(playerToIntersect.Center) / (projectile.Distance(playerToIntersect.Center) * 0.03f) * projectile.velocity.Length() * 2f).X * (player.noKnockback ? 0f : 1f);
						playerToIntersect.velocity.Y += (projectile.DirectionTo(playerToIntersect.Center) / (projectile.Distance(playerToIntersect.Center) * 0.03f) * projectile.velocity.Length() * 3.5f).Y * (player.noKnockback ? 0f : 1f);
						playerToIntersect.GetModPlayer<TFModPlayer>().isRocketJumping = true;
						playerToIntersect.Hurt(PlayerDeathReason.ByCustomReason(playerToIntersect.name + " was blown up by " + player.name + "."), (int)(projectile.damage * 0.5f - Math.Abs(projectile.Distance(playerToIntersect.Center) * 0.2f)), 0, false, true, false, -1);
					}
					else if (player.team == (int)Team.None && player.hostile == playerToIntersect.hostile && player.hostile)
					{
						playerToIntersect.jump = 1;
						playerToIntersect.velocity.X += (projectile.DirectionTo(playerToIntersect.Center) / (projectile.Distance(playerToIntersect.Center) * 0.03f) * projectile.velocity.Length() * 2f).X * (player.noKnockback ? 0f : 1f);
						playerToIntersect.velocity.Y += (projectile.DirectionTo(playerToIntersect.Center) / (projectile.Distance(playerToIntersect.Center) * 0.03f) * projectile.velocity.Length() * 3.5f).Y * (player.noKnockback ? 0f : 1f);
						playerToIntersect.GetModPlayer<TFModPlayer>().isRocketJumping = true;
						playerToIntersect.Hurt(PlayerDeathReason.ByCustomReason(playerToIntersect.name + " was blown up by " + player.name + "."), (int)(projectile.damage * 0.5f - Math.Abs(projectile.Distance(playerToIntersect.Center) * 0.2f)), 0, false, false, false, -1);
					}
				}
			}

			for (int i = 0; i < Main.maxNPCs; i++)
            {
				Rectangle hitbox = explosion;
				NPC npcToIntersect = Main.npc[i];
				if (hitbox.Intersects(npcToIntersect.Hitbox) && npcToIntersect.active)
				{
					if (!npcToIntersect.friendly)
					{
						if (npcToIntersect.CanBeChasedBy())
						{
							npcToIntersect.velocity.X += (projectile.DirectionTo(npcToIntersect.Center) / (projectile.Distance(npcToIntersect.Center) * 0.03f) * projectile.velocity.Length() * 2f).X * npcToIntersect.knockBackResist;
							npcToIntersect.velocity.Y += (projectile.DirectionTo(npcToIntersect.Center) / (projectile.Distance(npcToIntersect.Center) * 0.03f) * projectile.velocity.Length() * 3.5f).Y * npcToIntersect.knockBackResist;
						}
						if (npcToIntersect.CanBeChasedBy() || npcToIntersect.type == NPCID.TargetDummy)
                        {
							npcToIntersect.StrikeNPC((int)(projectile.damage * 0.5f - Math.Abs(projectile.Distance(npcToIntersect.Center) * 0.2f)), 0, 0, false, false, false);
						}
					}
				}
			}

			projectile.Kill();
		}

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
			Explode();
			return true;
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
			Texture2D texture = Main.projectileTexture[projectile.type];
			SpriteEffects spriteEffect = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Rectangle rectangle = texture.Frame(1, 1, 0, 0);

			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition + new Vector2(-16f, -1f).RotatedBy(projectile.rotation), rectangle, Color.White, projectile.rotation, rectangle.Size() * 0.5f, projectile.scale, spriteEffect, 0f);
			return false;
		}
    }
}