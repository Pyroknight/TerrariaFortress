using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaFortress.Buffs;
using TerrariaFortress.Dusts;
using TerrariaFortress.Items;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Projectiles
{
    public abstract class TFProjectile : ModProjectile
    {
        public bool rangedDamage = false;
        public bool explosiveDamage = false;
        public bool fireDamage = false;
        public bool sentryProjectile = false;
        public bool critting = false;
        public bool miniCritting = false;
        public float critDamageMultiplier = 3f;
        public bool randomCritting = false;
        public bool heavyProjectile = false;

        /// <summary>
        /// Casts basic light.
        /// </summary>
        /// <param name="lightColor">RGB color value.</param>
        /// <param name="strength">Radius multiplier.</param>
        /// <param name="delegateLighting">Set to true for PlotTileLine lighting.</param>
        public virtual void CastLight(Color lightColor, float strength = 40f, bool delegateLighting = false)
        {
            if (delegateLighting)
            {
                DelegateMethods.v3_1 = lightColor.ToVector3() / 255;
                Utils.PlotTileLine(projectile.Center, projectile.Center + projectile.velocity * 4f, strength, DelegateMethods.CastLightOpen);
            }
            else
            {
                Lighting.AddLight(projectile.Center, (int)lightColor.R / 255f * strength, (int)lightColor.G / 255f * strength, (int)lightColor.B / 255f * strength);
            }
        }

        /// <summary>
        /// Enum to switch draw mode types for projectile spritebatches.
        /// </summary>
        public enum DrawMode
        {
            /// <summary>
            /// Only run trail drawcode
            /// </summary>
            TrailOnly,
            /// <summary>
            /// Only run base projectile drawcode
            /// </summary>
            ProjectileOnly,
            /// <summary>
            /// Run both base projectile drawcode as well as trail drawcode
            /// </summary>
            ProjectileTrail
        }

        /// <summary>
        /// Draws the projectile with math for hitbox offset orientation if necessary, and with trails if TrailCacheLength is set in SetStaticDefaults.
        /// </summary>
        public virtual void DrawProjectileTemplate(SpriteBatch spriteBatch, Color color, int horizontalFrames = 1, int verticalFrames = 1, int frameX = 0, int frameY = 0, DrawMode drawMode = DrawMode.ProjectileTrail, bool offsetDrawPosition = true)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            Rectangle rectangle = texture.Frame(horizontalFrames, verticalFrames, frameX, frameY);
            Vector2 drawPosition = projectile.Center - Main.screenPosition + new Vector2(0f, projectile.gfxOffY);
            SpriteEffects spriteEffects = projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Color drawColor;
            if (ProjectileID.Sets.TrailCacheLength[projectile.type] > 0 && drawMode == DrawMode.TrailOnly || drawMode == DrawMode.ProjectileTrail)
            {
                for (int i = 0; i < projectile.oldPos.Length; i++)
                {
                    Vector2 trailPosition = projectile.oldPos[i] - Main.screenPosition + projectile.Size * 0.5f + new Vector2(0f, projectile.gfxOffY);
                    drawColor = color * ((float)(projectile.oldPos.Length - i) / (float)projectile.oldPos.Length);
                    spriteBatch.Draw(texture, trailPosition + (offsetDrawPosition ? OffsetDrawPosition(texture) : new Vector2(0f, 0f)), rectangle, drawColor, ProjectileID.Sets.TrailingMode[projectile.type] == 2 ? projectile.oldRot[i] : projectile.rotation, rectangle.Size() * 0.5f, projectile.scale, spriteEffects, 0f);
                }
            }
            drawColor = color;
            if (drawMode == DrawMode.ProjectileOnly || drawMode == DrawMode.ProjectileTrail)
            {
                spriteBatch.Draw(texture, drawPosition + (offsetDrawPosition ? OffsetDrawPosition(texture) : new Vector2(0f, 0f)), rectangle, drawColor, projectile.rotation, rectangle.Size() * 0.5f, projectile.scale, spriteEffects, 0f);
            }
        }

        /// <summary>
        /// The position used for drawing with hitbox offset orientation.
        /// </summary>
        /// <param name="texture">Texture to determine the hitbox's parameter.</param>
        /// <returns></returns>
        public virtual Vector2 OffsetDrawPosition(Texture2D texture)
        {
            Vector2 hitboxOffset = new Vector2((texture.Width - projectile.width) * -0.5f, 0f).RotatedBy(projectile.rotation);
            return hitboxOffset;
        }

        /// <summary>
        /// Hitbox checking for players, accounting for teams and PVP.
        /// </summary>
        /// <param name="player">Player parameter.</param>
        /// <param name="hitbox">The hitbox to check intersection for.</param>
        /// <param name="playerToIntersect">The player whose hitbox will be checked for intersection.</param>
        /// <returns></returns>
        public virtual bool HitboxCheckPlayer(Player player, Rectangle hitbox, Player playerToIntersect)
        {
            if (hitbox.Intersects(playerToIntersect.Hitbox) && playerToIntersect.whoAmI != player.whoAmI && playerToIntersect.active)
            {
                if (player.team != (int)Team.None && player.team != playerToIntersect.team && player.hostile == playerToIntersect.hostile && player.hostile)
                {
                    return true;
                }
                else if (player.team == (int)Team.None && player.hostile == playerToIntersect.hostile && player.hostile)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Hitbox checking for NPCs, accounting for exclusion of critters and Town NPCs, and the like.
        /// </summary>
        /// <param name="player">Player parameter.</param>
        /// <param name="hitbox">The hitbox to check intersection for.</param>
        /// <param name="npcToIntersect">The NPC whose hitbox will be checked for intersection.</param>
        /// <returns></returns>
        public virtual bool HitboxCheckNPC(Player player, Rectangle hitbox, NPC npcToIntersect)
        {
            if (hitbox.Intersects(npcToIntersect.Hitbox) && npcToIntersect.active)
            {
                if (!npcToIntersect.friendly)
                {
                    if (npcToIntersect.CanBeChasedBy())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Runs the functions of a critical hit.
        /// </summary>
        /// <param name="damage">The damage to multiply by 3.</param>
        /// <param name="player">The player who deals the critical hit.</param>
        /// <param name="target">The target who receives the critical hit.</param>
        public void CriticalHit(ref int damage, Player player, Entity target)
        {
            damage = (int)(damage * critDamageMultiplier);
            Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/CriticalHitLanded" + Main.rand.Next(1, 6)));
            Rectangle position = new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height);
            Vector2 CombatTextDimensions(int combatTextIndex, string text)
            {
                CombatText combatText = Main.combatText[combatTextIndex];
                return Main.fontCombatText[combatText.crit ? 1 : 0].MeasureString(text);
            }
            int index = CombatText.NewText(position, TFColor[(int)TFColorID.CombatCrit], "CRITICAL");
            if (index >= 0 && index < Main.combatText.Length)
            {
                CombatText combatText = Main.combatText[index];
                combatText.position = target.Top + new Vector2(CombatTextDimensions(index, combatText.text).X * -0.5f, 0f); ;
                combatText.lifeTime = 60;
                combatText.rotation = 0f;
                combatText.velocity = new Vector2(0f, -4f);
            }
            index = CombatText.NewText(position, TFColor[(int)TFColorID.CombatCrit], "HIT!!!");
            if (index >= 0 && index < Main.combatText.Length)
            {
                CombatText combatText = Main.combatText[index];
                combatText.position = target.Top + new Vector2(CombatTextDimensions(index, "HIT").X * -0.5f, 0f);
                combatText.lifeTime = 60;
                combatText.rotation = 0f;
                combatText.velocity = new Vector2(0f, -4f);
                combatText.position.Y += 16f;
            }
        }

        /// <summary>
        /// Runs the functions of a mini-crit hit.
        /// </summary>
        /// <param name="damage">The damage to multiply by 1.35.</param>
        /// <param name="player">The player who deals the mini-crit hit.</param>
        /// <param name="target">The target who receives the mini-crit hit.</param>
        public void MiniCritHit(ref int damage, Player player, Entity target)
        {
            damage = (int)(damage * 1.35f);
            Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/MiniCritHitLanded" + Main.rand.Next(1, 6)));
            Rectangle position = new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height);
            Vector2 CombatTextDimensions(int combatTextIndex, string text)
            {
                CombatText combatText = Main.combatText[combatTextIndex];
                return Main.fontCombatText[combatText.crit ? 1 : 0].MeasureString(text);
            }
            int index = CombatText.NewText(position, TFColor[(int)TFColorID.CombatMiniCrit], "MINI");
            if (index >= 0 && index < Main.combatText.Length)
            {
                CombatText combatText = Main.combatText[index];
                combatText.position = target.Top + new Vector2(CombatTextDimensions(index, combatText.text).X * -0.5f, 0f); ;
                combatText.lifeTime = 60;
                combatText.rotation = 0f;
                combatText.velocity = new Vector2(0f, -4f);
            }
            index = CombatText.NewText(position, TFColor[(int)TFColorID.CombatMiniCrit], "CRIT!");
            if (index >= 0 && index < Main.combatText.Length)
            {
                CombatText combatText = Main.combatText[index];
                combatText.position = target.Top + new Vector2(CombatTextDimensions(index, "CRIT").X * -0.5f, 0f);
                combatText.lifeTime = 60;
                combatText.rotation = 0f;
                combatText.velocity = new Vector2(0f, -4f);
                combatText.position.Y += 16f;
            }
        }

        public virtual void CritBoost()
        {
            critting = true;
        }

        public virtual void MiniCritBoost()
        {
            miniCritting = true;
        }

        /// <summary>
        /// Run the mechanics of a compression blast (mini-crit them, change their velocity, etc.)
        /// </summary>
        /// <param name="player">Player parameter.</param>
        public virtual void Airblast(Player player)
        {
            if (player == Main.player[Main.myPlayer])
            {
                int[] extinguishedDebuffs =
                {
                    BuffID.OnFire,
                    ModContent.BuffType<Afterburn>()
                };

                Point point = projectile.Center.ToTileCoordinates();
                Rectangle airblastFunctionHitbox;
                int widthBase = 20, heightBase = 10;
                int width = widthBase * 16, height = heightBase * 16;
                Vector2 center = new Vector2(0f, 0f);
                if (player == Main.player[Main.myPlayer])
                {
                    center = player.MountedCenter + player.DirectionTo(Main.MouseWorld) * width * 0.4f;
                }
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
                            bool flameCheck = false;

                            for (int j = 0; j < extinguishedDebuffs.Length; j++)
                            {
                                flameCheck = true;
                            }

                            if (flameCheck)
                            {
                                if (Collision.CanHitLine(player.MountedCenter, 0, 0, extinguishedPlayer.MountedCenter, 0, 0))
                                {
                                    Main.PlaySound(SoundLoader.customSoundType, (int)extinguishedPlayer.MountedCenter.X, (int)extinguishedPlayer.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerExtinguish"));
                                    for (int j = 0; j < extinguishedDebuffs.Length; j++)
                                    {
                                        extinguishedPlayer.ClearBuff(j);
                                    }

                                    if (!player.moonLeech)
                                    {
                                        player.statLife += 20;
                                        player.HealEffect(20, true);
                                    }
                                }
                            }
                        }
                        else if (player.team == (int)Team.None && player.hostile == extinguishedPlayer.hostile && !player.hostile)
                        {
                            bool flameCheck = false;

                            for (int j = 0; j < extinguishedDebuffs.Length; j++)
                            {
                                flameCheck = true;
                            }

                            if (flameCheck)
                            {
                                if (Collision.CanHitLine(player.MountedCenter, 0, 0, extinguishedPlayer.MountedCenter, 0, 0))
                                {
                                    Main.PlaySound(SoundLoader.customSoundType, (int)extinguishedPlayer.MountedCenter.X, (int)extinguishedPlayer.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerExtinguish"));
                                    for (int j = 0; j < extinguishedDebuffs.Length; j++)
                                    {
                                        extinguishedPlayer.ClearBuff(j);
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
                }

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC extinguishedNPC = Main.npc[i];

                    if (airblastFunctionHitbox.Intersects(extinguishedNPC.Hitbox))
                    {
                        if (extinguishedNPC.friendly && extinguishedNPC.active)
                        {
                            bool flameCheck = false;

                            foreach (int j in extinguishedDebuffs)
                            {
                                if (extinguishedNPC.HasBuff(j))
                                {
                                    flameCheck = true;
                                }
                            }

                            if (flameCheck)
                            {
                                if (Collision.CanHitLine(player.MountedCenter, 0, 0, extinguishedNPC.Center, 0, 0))
                                {
                                    Main.PlaySound(SoundLoader.customSoundType, (int)extinguishedNPC.Center.X, (int)extinguishedNPC.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerExtinguish"));
                                    foreach (int j in extinguishedDebuffs)
                                    {
                                        if (extinguishedNPC.HasBuff(j))
                                        {
                                            extinguishedNPC.DelBuff(extinguishedNPC.FindBuffIndex(j));
                                        }
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
                }
                #endregion

                #region Reflect
                bool reflectedDustCheck = false;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile reflectedProjectile = Main.projectile[i];

                    if (airblastFunctionHitbox.Intersects(reflectedProjectile.Hitbox))
                    {
                        if (reflectedProjectile.hostile && reflectedProjectile.active)
                        {
                            if (!(reflectedProjectile.modProjectile is TFBuilding) && !airblastReflectBlacklist.Contains(reflectedProjectile.type))
                            {
                                if (Collision.CanHitLine(player.MountedCenter, 0, 0, reflectedProjectile.Center, 0, 0))
                                {
                                    if (!projectile.wet && !WorldGen.SolidOrSlopedTile(point.X, point.Y))
                                    {
                                        reflectedDustCheck = true;
                                    }
                                    Main.PlaySound(SoundLoader.customSoundType, (int)projectile.Center.X, (int)projectile.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerReflect"));

                                    reflectedProjectile.friendly = true;
                                    Vector2 velocity = Vector2.Normalize(Main.MouseWorld - reflectedProjectile.Center);
                                    if (Vector2.Distance(Main.MouseWorld, player.MountedCenter) <= Vector2.Distance(reflectedProjectile.Center, player.MountedCenter))
                                    {
                                        velocity = Vector2.Normalize(reflectedProjectile.Center - player.MountedCenter);
                                    }
                                    float minSpeed = projectile.velocity.Length();
                                    reflectedProjectile.velocity = velocity * (reflectedProjectile.velocity.Length() < minSpeed ? minSpeed : reflectedProjectile.velocity.Length());

                                    if (reflectedProjectile.modProjectile is TFProjectile TFProjectile)
                                    {
                                        if (TFProjectile.SwitchOwnerOnAirblast())
                                        {
                                            reflectedProjectile.owner = player.whoAmI;
                                        }
                                    }
                                    else
                                    {
                                        reflectedProjectile.owner = player.whoAmI;
                                    }
                                }
                            }
                        }
                    }
                }

                if (reflectedDustCheck)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        Dust dust1 = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, ModContent.DustType<VelocityLines>(), projectile.velocity.X, projectile.velocity.Y, 0, default, 1.5f);
                    }
                }
                #endregion

                #region Push
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player pushedPlayer = Main.player[i];

                    if (airblastFunctionHitbox.Intersects(pushedPlayer.Hitbox) && pushedPlayer.whoAmI != player.whoAmI && pushedPlayer.active)
                    {
                        if (Collision.CanHitLine(player.MountedCenter, 0, 0, pushedPlayer.MountedCenter, 0, 0))
                        {
                            if (player.team != (int)Team.None && player.team != pushedPlayer.team && player.hostile == pushedPlayer.hostile && player.hostile)
                            {
                                Main.PlaySound(SoundLoader.customSoundType, (int)pushedPlayer.MountedCenter.X, (int)pushedPlayer.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerImpact" + Main.rand.Next(1, 5)));
                                pushedPlayer.velocity += new Vector2(Math.Abs(pushedPlayer.DirectionTo(Main.MouseWorld).X) * Math.Sign(projectile.velocity.X), pushedPlayer.DirectionTo(Main.MouseWorld).Y) * projectile.velocity.Length() * (pushedPlayer.noKnockback ? 0f : 1f);
                            }
                            else if (player.team == (int)Team.None && player.hostile == pushedPlayer.hostile && player.hostile)
                            {
                                Main.PlaySound(SoundLoader.customSoundType, (int)pushedPlayer.MountedCenter.X, (int)pushedPlayer.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerImpact" + Main.rand.Next(1, 5)));
                                pushedPlayer.velocity += new Vector2(Math.Abs(pushedPlayer.DirectionTo(Main.MouseWorld).X) * Math.Sign(projectile.velocity.X), pushedPlayer.DirectionTo(Main.MouseWorld).Y) * projectile.velocity.Length() * (pushedPlayer.noKnockback ? 0f : 1f);
                            }
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
                                if (Collision.CanHitLine(player.MountedCenter, 0, 0, pushedNPC.Center, 0, 0))
                                {
                                    Main.PlaySound(SoundLoader.customSoundType, (int)pushedNPC.Center.X, (int)pushedNPC.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerImpact" + Main.rand.Next(1, 5)));
                                    pushedNPC.velocity += new Vector2(Math.Abs(pushedNPC.DirectionTo(Main.MouseWorld).X) * Math.Sign(projectile.velocity.X), pushedNPC.DirectionTo(Main.MouseWorld).Y) * projectile.velocity.Length() * pushedNPC.knockBackResist * 2;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Returns whether or not projectiles will switch their owner (i.e. stickybombs.) upon being reflected.
        /// </summary>
        /// <returns></returns>
        public virtual bool SwitchOwnerOnAirblast()
        {
            return true;
        }

        public virtual void TFAI()
        {

        }

        /// <summary>
        /// Allows you to change things while this projectile is critting.
        /// </summary>
        public virtual void CritEffects()
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust1 = Dust.NewDustPerfect(projectile.Center, DustID.RainbowMk2, projectile.velocity * 0.25f, 0, new Color(255, 0, 0), 1f);
                dust1.noGravity = true;
            }
        }

        public override void AI()
        {
            if (critting)
            {
                CritEffects();
            }

            TFAI();
        }

        /// <summary>
        /// Basic explosion.
        /// </summary>
        /// <param name="radius">How large the rectangle of the explosion is.</param>
        /// <param name="strength">A multiplier for the explosion's velocity.</param>
        public virtual void Explode(int radius = 150, float strength = 20f)
        {
            Player player = Main.player[projectile.owner];

            Main.PlaySound(SoundLoader.customSoundType, (int)projectile.Center.X, (int)projectile.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/RocketLauncherExplode" + Main.rand.Next(1, 4)));
            CastLight(new Color(255, 64, 0), 1.4f);

            Dust dust1 = Dust.NewDustPerfect(projectile.Center + new Vector2(-45f, -45f), ModContent.DustType<LargeFlash>(), new Vector2(0f, 0f), 0, new Color(255, 240, 0, 255), 1f);

            int rings = Main.rand.Next(30, 61);
            for (int k = 0; k < rings; k++)
            {
                if (Main.rand.Next(2) == 0)
                {
                    for (int i = 0; i < (int)(radius / 150f) + 5; i++)
                    {
                        if (Main.rand.Next(3) == 0)
                        {
                            Dust dust2 = Dust.NewDustPerfect(projectile.Center + new Vector2(0f, -1f * (radius / 150f)) + new Vector2(0f, -i).RotatedBy(MathHelper.ToRadians(k * (360f / rings))), DustID.Smoke, new Vector2(0f, -i * (radius / 150f) * 1.2f).RotatedBy(MathHelper.ToRadians(k * (360f / rings))), 50, new Color(200, 200, 200), 3f);
                            dust2.noGravity = true;
                            dust2.noLight = true;
                        }

                        for (int j = 0; j < 2; j++)
                        {
                            Dust dust3 = Dust.NewDustPerfect(projectile.Center + new Vector2(0f, -1f + (radius / 150f) * 1.5f).RotatedBy(MathHelper.ToRadians(k * (360f / rings))), DustID.Fire, new Vector2(0f, -5f * (radius / 150f) * 1.25f * j).RotatedBy(MathHelper.ToRadians(k * (360f / rings))) + new Vector2(0f, -1f), 100, default, 3f);
                            dust3.noGravity = true;
                            dust3.noLight = true;
                        }
                    }

                    if (Main.rand.Next(2) == 0)
                    {
                        Dust dust4 = Dust.NewDustPerfect(projectile.Center + new Vector2(0f, -4f + (radius / 150f) * 0.1f).RotatedBy(MathHelper.ToRadians(k * (360f / rings))), ModContent.DustType<VelocityLines>(), new Vector2(0f, -8f * (radius / 150f) * Main.rand.NextFloat(0.5f, 1.5f)).RotatedBy(MathHelper.ToRadians(k * (360f / rings))) + new Vector2(0f, -1f), 0, default, 1f);
                        dust4.noGravity = true;
                        dust4.noLight = true;
                        dust4.scale *= 1.3f;
                    }
                }
            }
            int explosionArea = radius;
            Rectangle explosion = new Rectangle((int)projectile.Center.X - (explosionArea / 2), (int)projectile.Center.Y - (explosionArea / 2), explosionArea, explosionArea);
            Vector2 explosionPosition = new Vector2(explosion.Center.X, explosion.Center.Y);
            Vector2 explosionPower = new Vector2(1.5f, 1.5f) * strength;
            Vector2 collisionCheckPosition = explosionPosition + Vector2.Normalize(-projectile.velocity) * 8f;

            if (explosion.Intersects(player.Hitbox) && player.active)
            {
                player.jump = 1;
                player.immuneTime = 0;
                player.immune = false;
                Vector2 direction = Vector2.Normalize(player.MountedCenter - explosionPosition);
                if (direction.HasNaNs())
                {
                    if (player == Main.player[Main.myPlayer])
                    {
                        direction = Vector2.Normalize(player.MountedCenter - Main.MouseWorld);
                    }
                }
                player.velocity += (direction * (Math.Abs((Vector2.Distance(explosionPosition, player.MountedCenter) - explosionArea * 0.5f))) / (explosionArea * 0.5f)) * explosionPower;
                player.GetModPlayer<TFModPlayer>().isRocketJumping = true;
                float damage = projectile.damage * (Math.Abs((Vector2.Distance(explosionPosition, player.Center) - explosionArea * 0.5f))) / (explosionArea * 0.5f);
                int dealtDamage = (int)damage;
                if (critting)
                {
                    CriticalHit(ref dealtDamage, player, player);
                }
                player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was blown up."), dealtDamage, 0, false, true, false, -1);
            }

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Rectangle hitbox = explosion;
                Player playerToIntersect = Main.player[i];
                if (hitbox.Intersects(playerToIntersect.Hitbox) && playerToIntersect.whoAmI != player.whoAmI && playerToIntersect.active/* && Collision.CanHitLine(collisionCheckPosition, 0, 0, playerToIntersect.MountedCenter, 0, 0)*/)
                {
                    float damage = projectile.damage * (Math.Abs((Vector2.Distance(explosionPosition, playerToIntersect.Center) - explosionArea * 0.5f))) / (explosionArea * 0.5f);
                    int dealtDamage = (int)damage;
                    if (critting)
                    {
                        CriticalHit(ref dealtDamage, player, playerToIntersect);
                    }
                    void PVPExplosion()
                    {
                        playerToIntersect.jump = 1;
                        playerToIntersect.immuneTime = 0;
                        playerToIntersect.immune = false;
                        Vector2 direction = Vector2.Normalize(playerToIntersect.MountedCenter - explosionPosition);
                        if (direction.HasNaNs())
                        {
                            direction = Vector2.Normalize(playerToIntersect.MountedCenter - (explosionPosition + projectile.velocity));
                        }
                        playerToIntersect.velocity += (direction * (Math.Abs((Vector2.Distance(explosionPosition, playerToIntersect.MountedCenter) - explosionArea * 0.5f))) / (explosionArea * 0.5f)) * explosionPower * (playerToIntersect.noKnockback ? 0f : 1f);
                        playerToIntersect.GetModPlayer<TFModPlayer>().isRocketJumping = true;
                        playerToIntersect.Hurt(PlayerDeathReason.ByCustomReason(playerToIntersect.name + " was blown up by " + player.name + "."), dealtDamage, 0, false, false, false, -1);
                    }

                    if (player.team != (int)Team.None && player.team != playerToIntersect.team && player.hostile == playerToIntersect.hostile && player.hostile)
                    {
                        PVPExplosion();
                    }
                    else if (player.team == (int)Team.None && player.hostile == playerToIntersect.hostile && player.hostile)
                    {
                        PVPExplosion();
                    }
                }
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                Rectangle hitbox = explosion;
                NPC npcToIntersect = Main.npc[i];
                if (hitbox.Intersects(npcToIntersect.Hitbox) && npcToIntersect.active /*&& Collision.CanHitLine(collisionCheckPosition, 0, 0, npcToIntersect.Center, 0, 0)*/)
                {
                    float damage = projectile.damage * (Math.Abs((Vector2.Distance(explosionPosition, npcToIntersect.Center) - explosionArea * 0.5f))) / (explosionArea * 0.5f);
                    int dealtDamage = (int)damage;
                    {
                        bool hitCheck = npcToIntersect.CanBeChasedBy(projectile);
                        if (hitCheck || npcToIntersect.type == NPCID.TargetDummy)
                        {
                            if (critting)
                            {
                                CriticalHit(ref dealtDamage, player, npcToIntersect);
                            }
                            npcToIntersect.StrikeNPC(dealtDamage, 0, 0, false, false, false);
                            player.addDPS((int)damage);
                            if (hitCheck)
                            {
                                Vector2 direction = Vector2.Normalize(npcToIntersect.Center - explosionPosition);
                                if (direction.HasNaNs())
                                {
                                    direction = Vector2.Normalize(npcToIntersect.Center - (explosionPosition + projectile.velocity));
                                }
                                npcToIntersect.velocity += (direction * (Math.Abs((Vector2.Distance(explosionPosition, npcToIntersect.Center) - explosionArea * 0.5f))) / (explosionArea * 0.5f)) * explosionPower * npcToIntersect.knockBackResist;
                                player.GetModPlayer<TFModPlayer>().AddRecentDamage(dealtDamage);
                            }
                        }
                    }
                }
            }

            projectile.Kill();
        }

        /// <summary>
        /// (For NPCs) Used for things like dealing grenades dealing no damage after striking surfaces. Lazy alternative to running the base method of ModifyHitNPC.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="damage">The modifiable damage.</param>
        /// <param name="knockback">The modifiable knockback.</param>
        /// <param name="crit">The modifiable crit.</param>
        /// <param name="hitDirection">The modifiable hit direction.</param>
        public virtual void TFModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {

        }

        /// <summary>
        /// (For players) Used for things like dealing grenades dealing no damage after striking surfaces. Lazy alternative to running the base method of ModifyHitPvp.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="damage">The modifiable damage.</param>
        /// <param name="crit">The modifiable crit.</param>
        public virtual void TFModifyHitPvp(Player target, ref int damage, ref bool crit)
        {

        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[projectile.owner];
            crit = false;
            if (critting)
            {
                CriticalHit(ref damage, player, target);
                knockback *= critDamageMultiplier;
            }
            TFModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
        {
            Player player = Main.player[projectile.owner];
            crit = false;
            if (critting || (randomCritting && player.GetModPlayer<TFModPlayer>().canBeRandomCritted))
            {
                CriticalHit(ref damage, player, target);
            }
            TFModifyHitPvp(target, ref damage, ref crit);
        }
    }
}