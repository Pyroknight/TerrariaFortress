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
		int dustScaleTimer = 0;
		bool runFunctions = true;
		Rectangle airblastFunctionHitbox;

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

        public override void AI()
        {
			Player player = Main.player[projectile.owner];

            if (projectile.wet)
            {
				projectile.Kill();
            }

			if (dustScaleTimer >= 1)
            {
				runFunctions = false;
            }
			projectile.velocity *= 0.97f;

			Point point = (projectile.Center).ToTileCoordinates();

			if (!projectile.wet && !WorldGen.SolidOrSlopedTile(point.X, point.Y))
            {
                Dust dust1 = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Smoke, projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, 200, new Color(220, 220, 220), MathHelper.Clamp(dustScaleTimer * 0.2f, 0f, 5f) + 1f)];
                dust1.noGravity = true;
                dust1.noLight = true;
                Dust dust2 = Main.dust[Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Smoke, projectile.velocity.X * 0.25f, projectile.velocity.Y * 0.25f, 100, new Color(255, 255, 255), /*MathHelper.Clamp(dustScaleTimer * 0.04f, 0f, 1f)*/ + 1f)];
                dust2.noGravity = true;
                dust2.noLight = true;
            }

			if (runFunctions)
			{
                int widthBase = 20, heightBase = 10;
                int width = widthBase * 16, height = heightBase * 16;
                Vector2 center = player.MountedCenter + player.DirectionTo(Main.MouseWorld) * width * 0.4f;
                Vector2 position = center - new Vector2(width * 0.5f, height * 0.5f);
                airblastFunctionHitbox = new Rectangle((int)position.X, (int)position.Y, width, height);

                Main.PlaySound(SoundLoader.customSoundType, (int)projectile.Center.X, (int)projectile.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerAirblast"));

                #region Extinguish
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player extinguishedPlayer = Main.player[i];

                    if (airblastFunctionHitbox.Intersects(extinguishedPlayer.Hitbox) && extinguishedPlayer.whoAmI != player.whoAmI && extinguishedPlayer.active)
                    {
                        if (player.team != (int)Team.None && player.team == extinguishedPlayer.team && player.hostile == extinguishedPlayer.hostile)
                        {
                            if (extinguishedPlayer.HasBuff(BuffID.OnFire) || extinguishedPlayer.HasBuff(ModContent.BuffType<Afterburn>()))
                            {
                                Main.PlaySound(SoundLoader.customSoundType, (int)extinguishedPlayer.MountedCenter.X, (int)extinguishedPlayer.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerExtinguish"));
                                extinguishedPlayer.ClearBuff(BuffID.OnFire);
                                extinguishedPlayer.ClearBuff(ModContent.BuffType<Afterburn>());
                                if (!player.moonLeech)
                                {
                                    player.statLife += 20;
                                    player.HealEffect(20, true);
                                }
                            }
                        }
                        else if (player.team == (int)Team.None && player.hostile == extinguishedPlayer.hostile && !player.hostile)
                        {
                            if (extinguishedPlayer.HasBuff(BuffID.OnFire) || extinguishedPlayer.HasBuff(ModContent.BuffType<Afterburn>()))
                            {
                                Main.PlaySound(SoundLoader.customSoundType, (int)extinguishedPlayer.MountedCenter.X, (int)extinguishedPlayer.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerExtinguish"));
                                extinguishedPlayer.ClearBuff(BuffID.OnFire);
                                extinguishedPlayer.ClearBuff(ModContent.BuffType<Afterburn>());
                                if (!player.moonLeech)
                                {
                                    player.statLife += 20;
                                    player.HealEffect(20, true);
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC extinguishedNPC = Main.npc[i];

                    if (airblastFunctionHitbox.Intersects(extinguishedNPC.Hitbox))
                    {
                        if (extinguishedNPC.friendly && extinguishedNPC.active)
                        {
                            if (extinguishedNPC.HasBuff(BuffID.OnFire) || extinguishedNPC.HasBuff(ModContent.BuffType<Afterburn>()))
                            {
                                Main.PlaySound(SoundLoader.customSoundType, (int)extinguishedNPC.Center.X, (int)extinguishedNPC.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerExtinguish"));
                                if (extinguishedNPC.HasBuff(BuffID.OnFire))
                                {
                                    extinguishedNPC.DelBuff(extinguishedNPC.FindBuffIndex(BuffID.OnFire));
                                }
                                if (extinguishedNPC.HasBuff(ModContent.BuffType<Afterburn>()))
                                {
                                    extinguishedNPC.DelBuff(extinguishedNPC.FindBuffIndex(ModContent.BuffType<Afterburn>()));
                                }

                                if (!player.moonLeech)
                                {
                                    player.statLife += 20;
                                    player.HealEffect(20, true);
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Reflect
                bool reflectedDust = false;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile reflectedProjectile = Main.projectile[i];

                    if (airblastFunctionHitbox.Intersects(reflectedProjectile.Hitbox))
                    {
                        if (reflectedProjectile.hostile && reflectedProjectile.active)
                        {
                            if (!TerrariaFortress.airblastReflectBlacklist.Contains(reflectedProjectile.type))
                            {
                                if (!projectile.wet && !WorldGen.SolidOrSlopedTile(point.X, point.Y))
                                {
                                    reflectedDust = true;
                                }
                                Main.PlaySound(SoundLoader.customSoundType, -1, -1, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerReflect"));

                                reflectedProjectile.friendly = true;
                                reflectedProjectile.owner = player.whoAmI;
                                reflectedProjectile.velocity = reflectedProjectile.DirectionTo(Main.MouseWorld) * projectile.velocity.Length();
                                //reflectedProjectile.damage = (int)(reflectedProjectile.damage * 1.35f);
                            }
                        }
                    }
                }
                if (reflectedDust)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        Dust.NewDust(projectile.position, projectile.width, projectile.height, ModContent.DustType<AirblastLines>(), projectile.velocity.X, projectile.velocity.Y, 0, default, 1.5f);
                    }
                }
                #endregion

                #region Push
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player pushedPlayer = Main.player[i];

                    if (airblastFunctionHitbox.Intersects(pushedPlayer.Hitbox) && pushedPlayer.whoAmI != player.whoAmI && pushedPlayer.active)
                    {
                        if (player.team != (int)Team.None && player.team != pushedPlayer.team && player.hostile == pushedPlayer.hostile && player.hostile)
                        {
                            Main.PlaySound(SoundLoader.customSoundType, (int)pushedPlayer.MountedCenter.X, (int)pushedPlayer.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerImpact" + Main.rand.Next(1, 5)));
                            pushedPlayer.velocity += new Vector2(Math.Abs(pushedPlayer.DirectionTo(Main.MouseWorld).X) * Math.Sign(projectile.velocity.X), pushedPlayer.DirectionTo(Main.MouseWorld).Y) * projectile.velocity.Length() * (pushedPlayer.noKnockback ? 0 : 1);
                        }
                        else if (player.team == (int)Team.None && player.hostile == pushedPlayer.hostile && player.hostile)
                        {
                            Main.PlaySound(SoundLoader.customSoundType, (int)pushedPlayer.MountedCenter.X, (int)pushedPlayer.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerImpact" + Main.rand.Next(1, 5)));
                            pushedPlayer.velocity += new Vector2(Math.Abs(pushedPlayer.DirectionTo(Main.MouseWorld).X) * Math.Sign(projectile.velocity.X), pushedPlayer.DirectionTo(Main.MouseWorld).Y) * projectile.velocity.Length() * (pushedPlayer.noKnockback ? 0 : 1);
                        }
                    }
                }

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC pushedNPC = Main.npc[i];

                    if (airblastFunctionHitbox.Intersects(pushedNPC.Hitbox) && pushedNPC.active)
                    {
                        if (!pushedNPC.friendly || pushedNPC.townNPC)
                        {
                            if (pushedNPC.type != NPCID.TargetDummy && pushedNPC.knockBackResist > 0f)
                            {
                                Main.PlaySound(SoundLoader.customSoundType, (int)pushedNPC.Center.X, (int)pushedNPC.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerImpact" + Main.rand.Next(1, 5)));
                                pushedNPC.velocity += new Vector2(Math.Abs(pushedNPC.DirectionTo(Main.MouseWorld).X) * Math.Sign(projectile.velocity.X), pushedNPC.DirectionTo(Main.MouseWorld).Y) * projectile.velocity.Length() * pushedNPC.knockBackResist;
                            }
                        }
                    }
                }
                #endregion
            }

			dustScaleTimer++;

			projectile.rotation = projectile.velocity.ToRotation();
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            //spriteBatch.Draw(ModContent.GetTexture("TerrariaFortress/Projectiles/HitboxTest"), new Vector2(airblastFunctionHitbox.X, airblastFunctionHitbox.Y) - Main.screenPosition, airblastFunctionHitbox, Color.White, 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, 0f);
            return false;
        }
    }
}