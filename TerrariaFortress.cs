using Terraria.ModLoader;
using Terraria.ID;
using TerrariaFortress.Projectiles;
using Terraria;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TerrariaFortress.Items;
using Terraria.Enums;
using System.Collections.ObjectModel;
using Terraria.Utilities;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria.DataStructures;
using TerrariaFortress.Items.Weapons;
using TerrariaFortress.Dusts;
using System.Linq;
using TerrariaFortress.Buffs;
using Microsoft.Xna.Framework.Audio;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TerrariaFortress
{
    public class TerrariaFortress : Mod
    {
        [Label("Terraria Fortress Settings")]
        public class TFConfig : ModConfig
        {
            public override ConfigScope Mode => ConfigScope.ClientSide;
            [Header("Visuals")]
            [Label("Ranged Weapon Holdout Rotation")]
            [DefaultValue(true)]
            [Tooltip("If enabled, ranged weapons' holdouts will rotate\nto your cursor and change your player's direction.")]
            public bool shouldRangedHoldoutRotate;

            [Label("Melee Weapon Holdout Rotation")]
            [DefaultValue(true)]
            [Tooltip("If enabled, melee weapons' holdouts will rotate\nto your cursor and change your player's direction.")]
            public bool shouldMeleeHoldoutRotate;
        }

        public static readonly Color[] TFColor =
        {
            new Color(178, 178, 178),
            new Color(255, 215, 0),
            new Color(71, 98, 145),
            new Color(77, 116, 85),
            new Color(207, 106, 50),
            new Color(134, 80, 172),
            new Color(56, 243, 171),
            new Color(170, 0, 0),
            new Color(250, 250, 250),
            new Color(112, 176, 74),
            new Color(165, 15, 121),
            new Color(117, 107, 94),
            new Color(153, 204, 255),
            new Color(255, 64, 64),
            new Color(235, 226, 202),
            new Color(0, 160, 0),
            new Color(22, 255, 29),
            new Color(255, 255, 16),
        };

        public enum TFColorID
        {
            Normal,
            Unique,
            Vintage,
            Genuine,
            Strange,
            Unusual,
            Haunted,
            Collectors,
            Decorated,
            CommunityAndSelfMade,
            Valve,
            AttributeLevel,
            AttributePositive,
            AttributeNegative,
            AttributeNeutral,
            AttributeUseLimit,
            CombatCrit,
            CombatMiniCrit,
        }

        public enum StrangeRankType
        {
            Generic,
            HolidayPunch,
            InvisWatch,
            Mantreads,
            Sapper,
            SpiritOfGiving,
            Cosmetic,
            DuckJournal,
            SoulGargoyle
        }

        public abstract class TFProjectile : ModProjectile
        {
            public bool willCrit = false;
            public bool critBoosted = false;
            public bool miniCritBoosted = false;
            public int damageFalloffCounter = 0;
            public bool rangedDamage = false;
            public bool explosiveDamage = false;
            public bool fireDamage = false;
            public bool sentryProjectile = false;

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
                spriteBatch.Draw(texture, drawPosition + (offsetDrawPosition ? OffsetDrawPosition(texture) : new Vector2(0f, 0f)), rectangle, drawColor, projectile.rotation, rectangle.Size() * 0.5f, projectile.scale, spriteEffects, 0f);

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
                    {
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

                                    for (int j = 0; j < extinguishedDebuffs.Length; j++)
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
                                            for (int j = 0; j < extinguishedDebuffs.Length; j++)
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
                                    if (!airblastReflectBlacklist.Contains(reflectedProjectile.type))
                                    {
                                        if (Collision.CanHitLine(player.MountedCenter, 0, 0, reflectedProjectile.Center, 0, 0))
                                        {
                                            if (!projectile.wet && !WorldGen.SolidOrSlopedTile(point.X, point.Y))
                                            {
                                                reflectedDustCheck = true;
                                            }
                                            Main.PlaySound(SoundLoader.customSoundType, (int)projectile.Center.X, (int)projectile.Center.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerReflect"));

                                            reflectedProjectile.friendly = true;
                                            reflectedProjectile.owner = player.whoAmI;
                                            reflectedProjectile.velocity = reflectedProjectile.DirectionTo(Main.MouseWorld) * projectile.velocity.Length();

                                            if (reflectedProjectile.modProjectile is TFProjectile TFProjectile)
                                            {
                                                TFProjectile.miniCritBoosted = true;
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
                    player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was blown up."), (int)damage, 0, false, true, false, -1);
                }

                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Rectangle hitbox = explosion;
                    Player playerToIntersect = Main.player[i];
                    if (hitbox.Intersects(playerToIntersect.Hitbox) && playerToIntersect.whoAmI != player.whoAmI && playerToIntersect.active/* && Collision.CanHitLine(collisionCheckPosition, 0, 0, playerToIntersect.MountedCenter, 0, 0)*/)
                    {
                        void PVPExplosion()
                        {
                            playerToIntersect.jump = 1;
                            player.immuneTime = 0;
                            player.immune = false;
                            Vector2 direction = Vector2.Normalize(playerToIntersect.MountedCenter - explosionPosition);
                            if (direction.HasNaNs())
                            {
                                direction = Vector2.Normalize(playerToIntersect.MountedCenter - (explosionPosition + projectile.velocity));
                            }
                            playerToIntersect.velocity += (direction * (Math.Abs((Vector2.Distance(explosionPosition, playerToIntersect.MountedCenter) - explosionArea * 0.5f))) / (explosionArea * 0.5f)) * explosionPower * (playerToIntersect.noKnockback ? 0f : 1f);
                            playerToIntersect.GetModPlayer<TFModPlayer>().isRocketJumping = true;
                            float damage = projectile.damage * (Math.Abs((Vector2.Distance(explosionPosition, playerToIntersect.Center) - explosionArea * 0.5f))) / (explosionArea * 0.5f);
                            playerToIntersect.Hurt(PlayerDeathReason.ByCustomReason(playerToIntersect.name + " was blown up by " + player.name + "."), (int)damage, 0, false, false, false, -1);
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
                        if (!npcToIntersect.friendly)
                        {
                            if (npcToIntersect.CanBeChasedBy())
                            {
                                Vector2 direction = Vector2.Normalize(npcToIntersect.Center - explosionPosition);
                                if (direction.HasNaNs())
                                {
                                    direction = Vector2.Normalize(npcToIntersect.Center - (explosionPosition + projectile.velocity));
                                }
                                npcToIntersect.velocity += (direction * (Math.Abs((Vector2.Distance(explosionPosition, npcToIntersect.Center) - explosionArea * 0.5f))) / (explosionArea * 0.5f)) * explosionPower * npcToIntersect.knockBackResist;
                            }
                            if (npcToIntersect.CanBeChasedBy() || npcToIntersect.type == NPCID.TargetDummy)
                            {
                                float damage = projectile.damage * (Math.Abs((Vector2.Distance(explosionPosition, npcToIntersect.Center) - explosionArea * 0.5f))) / (explosionArea * 0.5f);
                                npcToIntersect.StrikeNPC((int)damage, 0, 0, false, false, false);
                                player.addDPS((int)damage);
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
                crit = false;
                TFModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
            }

            public override void ModifyHitPvp(Player target, ref int damage, ref bool crit)
            {
                crit = false;
                TFModifyHitPvp(target, ref damage, ref crit);
            }
        }

        public static readonly int[] airblastReflectBlacklist =
        {
            ModContent.ProjectileType<FlamethrowerAirblast>(),
            ModContent.ProjectileType<FlamethrowerFlame>(),
            ModContent.ProjectileType<TFBullet>(),
            ProjectileID.VilethornBase,
            ProjectileID.VilethornTip,
            ProjectileID.Bullet,
            ProjectileID.MagicMissile,
            ProjectileID.Flamelash,
            ProjectileID.NettleBurstRight,
            ProjectileID.NettleBurstLeft,
            ProjectileID.NettleBurstEnd,
            ProjectileID.PhantasmalDeathray,
            ProjectileID.LastPrism,
        };

        public class HoldoutDrawLayer : ModPlayer
        {
            public bool pilotSoundCheck = false;
            public bool meleeRotationBuildup = true;
            public float meleeRotationFactor = 0f;
            public float meleeSwingBackRotation = 180f;

            public override void PostUpdate()
            {
                if (player.HeldItem.modItem is TFWeapon TFWeapon)
                {
                    if (TFWeapon.isSwingMelee)
                    {
                        void MeleeSwingAnimation()
                        {
                            float toMouse = 0f;
                            if (player == Main.player[Main.myPlayer])
                            {
                                if (ModContent.GetInstance<TFConfig>().shouldMeleeHoldoutRotate)
                                {
                                    player.GetModPlayer<HoldoutDrawLayer>().meleeSwingBackRotation = 180f;
                                    player.ChangeDir((Main.MouseWorld.X > player.MountedCenter.X).ToDirectionInt());
                                    toMouse = ((Main.MouseWorld - player.MountedCenter).ToRotation() + MathHelper.ToRadians((player.direction == -1 ? 180f : 0f) * player.direction));
                                }
                                else
                                {
                                    player.GetModPlayer<HoldoutDrawLayer>().meleeSwingBackRotation = 135f;
                                }
                            }

                            if (player.itemAnimation == player.HeldItem.useAnimation - 1 || player.itemAnimation == 1 || player.itemAnimation == 0)
                            {
                                player.GetModPlayer<HoldoutDrawLayer>().meleeRotationBuildup = true;
                                player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor = 0f;
                                player.itemRotation = player.fullRotation + (player.direction == -1 ? MathHelper.ToRadians(90f) : MathHelper.ToRadians(270f)) + toMouse + MathHelper.ToRadians(0f) * player.direction;
                            }
                            else
                            {
                                if (player == Main.player[Main.myPlayer])
                                {
                                    if (ModContent.GetInstance<TFConfig>().shouldRangedHoldoutRotate)
                                    {
                                        player.ChangeDir((Main.MouseWorld.X > player.MountedCenter.X).ToDirectionInt());
                                    }
                                }

                                if (player.itemAnimation != player.HeldItem.useAnimation - 1)
                                {
                                    if (player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor < player.GetModPlayer<HoldoutDrawLayer>().meleeSwingBackRotation)
                                    {
                                        player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor += 5f + player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor * (100f / (player.HeldItem.useAnimation != 0f ? player.HeldItem.useAnimation : 1f) * 0.025f) * 3f;
                                    }
                                    else
                                    {
                                        player.GetModPlayer<HoldoutDrawLayer>().meleeRotationBuildup = false;
                                    }

                                    if (!player.GetModPlayer<HoldoutDrawLayer>().meleeRotationBuildup)
                                    {
                                        player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor -= (player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor - player.GetModPlayer<HoldoutDrawLayer>().meleeSwingBackRotation) * 0.2f;
                                    }
                                    player.itemRotation = (player.direction == -1 ? MathHelper.ToRadians(90f - player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor) : MathHelper.ToRadians(270f + player.GetModPlayer<HoldoutDrawLayer>().meleeRotationFactor)) + toMouse;
                                }
                            }
                        }
                        if (Main.netMode == NetmodeID.SinglePlayer)
                        {
                            if (!Main.gamePaused)
                            {
                                MeleeSwingAnimation();
                            }
                        }
                        else
                        {
                            MeleeSwingAnimation();
                        }
                    }

                    if (TFWeapon.isRanged)
                    {
                        void RangedHoldoutFrame()
                        {
                            if (player == Main.player[Main.myPlayer])
                            {
                                if (ModContent.GetInstance<TFConfig>().shouldRangedHoldoutRotate)
                                {
                                    player.ChangeDir((Main.MouseWorld.X > player.MountedCenter.X).ToDirectionInt());
                                    Vector2 toPlayer = Main.MouseWorld - player.MountedCenter;
                                    float rotation = -(float)Math.Atan2(toPlayer.X * player.direction, toPlayer.Y * player.direction) + MathHelper.ToRadians(90f) - player.fullRotation;
                                    player.itemRotation = rotation;
                                    rotation *= player.direction;
                                    player.bodyFrame.Y = player.bodyFrame.Height * 3;
                                    if (rotation < -0.75f)
                                    {
                                        player.bodyFrame.Y = player.bodyFrame.Height * 2;
                                        if (player.gravDir == -1f)
                                        {
                                            player.bodyFrame.Y = player.bodyFrame.Height * 4;
                                        }
                                    }
                                    if (rotation > 0.6f)
                                    {
                                        player.bodyFrame.Y = player.bodyFrame.Height * 4;
                                        if (player.gravDir == -1f)
                                        {
                                            player.bodyFrame.Y = player.bodyFrame.Height * 2;
                                        }
                                    }
                                }
                                else if (player.itemAnimation == 0)
                                {
                                    player.itemRotation = 0f;
                                    player.bodyFrame.Y = player.bodyFrame.Height * 3;
                                }
                            }
                        }
                        if (Main.netMode == NetmodeID.SinglePlayer)
                        {
                            if (!Main.gamePaused)
                            {
                                RangedHoldoutFrame();
                            }
                        }
                        else
                        {
                            RangedHoldoutFrame();
                        }
                    }
                }
            }

            public static readonly PlayerLayer HeldItem = new PlayerLayer("TerrariaFortress", "HeldItem", PlayerLayer.HeldItem, delegate (PlayerDrawInfo drawInfo)
            {
                Player player = drawInfo.drawPlayer;
                bool drawConditions = !player.dead && player.active && !player.invis && !player.frozen && !player.stoned;

                void DrawAllHoldouts()
                {
                    if (player.HeldItem.modItem is TFWeapon TFWeapon)
                    {
                        if (player.HeldItem.type == ModContent.ItemType<Flamethrower>())
                        {
                            //int R = 255, G = 255, B = 255;
                            //float brightness = 0.15f;
                            //Vector2 blueFlamePos = player.Center + new Vector2(player.direction * 60, player.gfxOffY + 6f).RotatedBy(player.itemRotation + player.fullRotation);

                            //Lighting.AddLight(blueFlamePos, R / 255f * brightness, G / 255f * brightness, B / 255f * brightness);
                            //Dust dust1 = Main.dust[Dust.NewDust(blueFlamePos, 2, 2, 211, 0f, -0.2f, 100, default, 0.5f)];
                            //dust1.noGravity = true;
                            //dust1.noLight = true;

                            //if (!pilotSoundCheck)
                            //{
                            //    Main.PlaySound(SoundLoader.customSoundType, -1, -1, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerPilot"));
                            //    pilotSoundCheck = true;
                            //}
                        }

                        TFWeapon.DrawWeaponHoldout(player);
                    }
                }

                if (drawConditions && !player.pulley)
                {
                    DrawAllHoldouts();
                }
                else if (player.pulley && (player.controlUseItem || player.altFunctionUse == 2))
                {
                    DrawAllHoldouts();
                }

                if (player.HeldItem.type != ModContent.ItemType<Flamethrower>())
                {
                    player.GetModPlayer<HoldoutDrawLayer>().pilotSoundCheck = false;
                }
            });
            public static readonly PlayerLayer Skin = new PlayerLayer("TerrariaFortress", "Skin", PlayerLayer.Skin, delegate (PlayerDrawInfo drawInfo)
            {
                Player player = drawInfo.drawPlayer;
                if (player.HeldItem.modItem is TFWeapon TFWeapon && TFWeapon.isSwingMelee)
                {
                    if (player.itemAnimation < player.itemAnimationMax * 0.777f)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 3;
                    }
                    else if (player.itemAnimation < player.itemAnimationMax * 0.888f)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 2;
                    }
                    else if (player.itemAnimation > player.itemAnimationMax * 0.888f)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height;
                    }

                    if (player.itemAnimation == 0)
                    {
                        player.bodyFrame.Y = player.bodyFrame.Height * 1;
                    }
                }
            });

            public override void ModifyDrawLayers(List<PlayerLayer> layers)
            {
                HeldItem.visible = true;
                Skin.visible = true;
                int index = layers.FindIndex(x => x == PlayerLayer.HeldItem);
                int index2 = layers.FindIndex(y => y == PlayerLayer.Skin);

                if (index != -1)
                {
                    layers.Insert(index, HeldItem);
                }
                if (index2 != -1)
                {
                    layers.Insert(index2, Skin);
                }
            }
        }

        public class TemporaryItemDeal : GlobalNPC
        {
            public override void SetupShop(int type, Chest shop, ref int nextSlot)
            {
                void AddPurchasable(int itemType, ref int slot)
                {
                    shop.item[slot].SetDefaults(itemType);
                    slot++;
                }

                if (type == NPCID.Demolitionist)
                {
                    if (NPC.downedBoss3)
                    {
                        int[] shopItems =
                        {
                            ModContent.ItemType<AmmoBox>(),
                            ModContent.ItemType<Flamethrower>(),
                            ModContent.ItemType<Shotgun>(),
                            ModContent.ItemType<FireAxe>(),
                            ModContent.ItemType<RocketLauncher>(),
                            ModContent.ItemType<Shovel>(),
                            ModContent.ItemType<Scattergun>(),
                            ModContent.ItemType<Pistol>(),
                            ModContent.ItemType<Bat>(),
                            ModContent.ItemType<GrenadeLauncher>(),
                            ModContent.ItemType<StickybombLauncher>(),
                            ModContent.ItemType<Bottle>(),
                        };

                        for (int i = 0; i < shopItems.Length; i++)
                        {
                            AddPurchasable(shopItems[i], ref nextSlot);
                        }
                    }
                }

                if (type == NPCID.PartyGirl)
                {
                    if (NPC.downedBoss3)
                    {
                        shop.item[nextSlot].SetDefaults(ModContent.ItemType<NoiseMakerBirthday>());
                        nextSlot++;
                    }
                }
            }
        }

        public class AIStats : GlobalItem
        {
            public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
            {
                //if (item.type == ItemID.GPS)
                //{
                //    NPC trackedNPC = Main.npc[NPC.FindFirstNPC(NPCID.Merchant)];
                //    if (trackedNPC.active)
                //    {
                //        tooltips.Add(new TooltipLine(mod, "ItemName", ("[c/FF0000:AI 0: ]" + trackedNPC.ai[0] + "[c/FF0000:,] ") + ("[c/FF0000:AI 1:] " + trackedNPC.ai[1] + "[c/FF0000:,] ") + ("[c/FF0000:AI 2:] " + trackedNPC.ai[2] + "[c/FF0000:,] ") + ("[c/FF0000:AI 3:] " + trackedNPC.ai[3])));
                //    }
                //}
            }
        }
    }
}