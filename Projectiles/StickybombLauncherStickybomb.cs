using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static TerrariaFortress.TerrariaFortress;
using Terraria.ModLoader;
using System;

namespace TerrariaFortress.Projectiles
{
	public class StickybombLauncherStickybomb : TFProjectile
	{
		public int armingTimer;
		public bool detonated = false;
		public int detonationTimer;
		public bool detonationSoundCheck = false;
		public bool tileCollided = false;	
		public bool tileCollisionSoundCheck = false;

		public override void SetStaticDefaults()
        {
			DisplayName.SetDefault("Stickybomb");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			projectile.width = 16;
			projectile.height = 16;
			projectile.tileCollide = false;
			projectile.ranged = true;
			projectile.penetrate = -1;
            projectile.friendly = true;
			explosiveDamage = true;
        }

        public override void Explode(int radius = 150, float strength = 20f)
        {
			radius = 292;
			strength *= 0.8f;
			base.Explode(radius, strength);
        }

        public override bool SwitchOwnerOnAirblast()
        {
			return false;
        }

        public override void TFAI()
		{
			Player player = Main.player[projectile.owner];

			if (player.dead || !player.active)
            {
				projectile.Kill();
            }

			void PastLimitExplosion()
			{
                #region Old Code
                //int stickybombsLimit = 8;
                //int[] placedStickybombs = new int[Main.maxPlayers];
                //for (int i = Main.maxProjectiles; i > 0; i--)
                //{
                //	Projectile stickybomb = Main.projectile[i];

                //	if (!stickybomb.active)
                //	{
                //		continue;
                //	}

                //	if (player.active && stickybomb.active && stickybomb.modProjectile is StickybombLauncherStickybomb stickybomb2)
                //	{
                //		if (placedStickybombs[player.whoAmI] + 1 >= stickybombsLimit)
                //		{
                //			if (stickybomb.owner == player.whoAmI)
                //			{
                //				stickybomb2.Explode();
                //			}
                //		}
                //		placedStickybombs[player.whoAmI]++;
                //	}
                //}
                #endregion

                int stickybombsLimit = 8;
                int placedStickybombs = 0;
                int oldestStickybombIndex = -1;
                int oldestTimeLeft = 100000;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
					Projectile stickybomb = Main.projectile[i];
                    if (stickybomb.active && stickybomb.owner == player.whoAmI && stickybomb.type == projectile.type)
                    {
                        placedStickybombs++;
                        if (stickybomb.timeLeft < oldestTimeLeft)
                        {
                            oldestStickybombIndex = i;
                            oldestTimeLeft = stickybomb.timeLeft;
                        }
                    }
                }
                if (placedStickybombs > stickybombsLimit)
                {
					Projectile oldestStickybomb = Main.projectile[oldestStickybombIndex];
                    if (oldestStickybomb.modProjectile is StickybombLauncherStickybomb stickybomb2)
                    {
						stickybomb2.Explode();
                    }
                }
            }

            PastLimitExplosion();

            float gravity = 0.3f;
			int radius = 0;

			if (projectile.velocity != new Vector2(0f, 0f))
			{
				projectile.rotation = projectile.velocity.ToRotation();
				if (projectile.velocity.Y <= 10f)
				{
					projectile.velocity.Y += gravity;
				}
			}

			for (int i = radius; i < projectile.width - radius; i++)
			{
				for (int j = radius; j < projectile.height - radius; j++)
				{
					Point point = (projectile.position + new Vector2(i, j)).ToTileCoordinates();
					if (WorldGen.SolidOrSlopedTile(point.X, point.Y))
					{
						projectile.velocity = new Vector2(0f, 0f);
						tileCollided = true;
					}
				}
			}

			if (armingTimer > 0)
            {
				armingTimer--;
            }

			if (detonated)
            {
				if (!detonationSoundCheck)
                {
					Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/StickybombLauncherDetonate"));
					detonationSoundCheck = true;
				}
				if (detonationTimer > 0)
                {
					detonationTimer--;
                }
            }
			if (detonationTimer == 0)
            {
				Explode();
            }

			if (tileCollided && !tileCollisionSoundCheck)
            {
				Main.PlaySound(SoundLoader.customSoundType, (int)projectile.Center.X, (int)projectile.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/PlasticImpactHard" + Main.rand.Next(1, 5)));
				tileCollisionSoundCheck = true;
            }
        }
        public override void CritEffects()
        {
            Dust dust1 = Dust.NewDustPerfect(projectile.Center, 182, projectile.velocity * 0.25f, 0, default, 1f);
            dust1.noGravity = true;
            base.CritEffects();
        }

        public override bool CanDamage()
        {
			return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            projectile.velocity = new Vector2(0f, 0f);

            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {	
			DrawProjectileTemplate(spriteBatch, Color.White, 1, 4, 0, 2, DrawMode.TrailOnly);
			float rotation = projectile.rotation;
			projectile.rotation += MathHelper.ToRadians(45f);
			DrawProjectileTemplate(spriteBatch, lightColor, 1, 4, 0, 1, DrawMode.ProjectileOnly);
			projectile.rotation = rotation;
			int frames = 6;
			if (armingTimer <= frames * 3)
			{
				float colorFormula = (frames - 1) * ((armingTimer % frames != 0 ? armingTimer % frames : frames) - 1);
				DrawProjectileTemplate(spriteBatch, Color.White * (255f / colorFormula * (1 / 255f)), 1, 4, 0, 3, DrawMode.ProjectileOnly);
			}
			DrawProjectileTemplate(spriteBatch, lightColor, 1, 4, 0, 0, DrawMode.ProjectileOnly);
			return false;
		}
    }
}