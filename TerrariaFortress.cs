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

        public class TFModPlayer : ModPlayer
        {
            public float recentDamage = 0;
            public float storedRandomCritChance = 0f;
            /// <summary>
            /// The max amount of damage stored for determining random crit chance.
            /// </summary>
            public float randomCritStorageCap = 2060f;
            /// <summary>
            /// If the player has crits active.
            /// </summary>
            public bool critBoosted = false;
            /// <summary>
            /// If the player has mini-crits active.
            /// </summary>
            public bool miniCritBoosted = false;
            /// <summary>
            /// If the player is rocket jumping.
            /// </summary>
            public bool isRocketJumping = false;
            public float randomCritCheck = Main.rand.NextFloat();
            public bool randomCrit = false;

            public virtual bool ItemCritCheck(Player player, TFWeapon TFWeapon)
            {
                if (TFWeapon.canCrit)
                {
                    if (player.GetModPlayer<TFModPlayer>().randomCritCheck <= player.GetModPlayer<TFModPlayer>().storedRandomCritChance / 100f)
                    {
                        return true;
                    }   

                    if (player.GetModPlayer<TFModPlayer>().critBoosted)
                    {
                        return true;
                    }
                }
                
                return false;
            }

            public override void PreUpdate()
            {
                #region Random Crit Calculation
                player.GetModPlayer<TFModPlayer>().randomCritCheck = Main.rand.NextFloat();
                if (player.GetModPlayer<TFModPlayer>().recentDamage > 0f)
                {
                    player.GetModPlayer<TFModPlayer>().recentDamage -= 0.5f;
                }
                player.GetModPlayer<TFModPlayer>().storedRandomCritChance = MathHelper.Clamp((player.GetModPlayer<TFModPlayer>().recentDamage > 0 ? player.GetModPlayer<TFModPlayer>().recentDamage / 82.5f / 2f : 0) + 2, 2f, 12f);
                player.GetModPlayer<TFModPlayer>().randomCrit = player.GetModPlayer<TFModPlayer>().randomCritCheck <= (storedRandomCritChance / 100f);
                #endregion

                #region Rocket Jumping
                if (player.velocity.Y == 0f)
                {
                    if (player.GetModPlayer<TFModPlayer>().isRocketJumping)
                    {
                        player.velocity.X *= 0.8f;
                        player.GetModPlayer<TFModPlayer>().isRocketJumping = false;
                    }
                }
                #endregion
            }

            #region Random Crit Check

            public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
            {
                if (target.CanBeChasedBy())
                {
                    if (item.modItem is TFWeapon TFWeapon)
                    {
                        crit = false;

                        if (player.GetModPlayer<TFModPlayer>().recentDamage + damage < player.GetModPlayer<TFModPlayer>().randomCritStorageCap)
                        {
                            player.GetModPlayer<TFModPlayer>().recentDamage += damage;
                        }
                        else
                        {
                            player.GetModPlayer<TFModPlayer>().recentDamage += player.GetModPlayer<TFModPlayer>().randomCritStorageCap - player.GetModPlayer<TFModPlayer>().recentDamage;
                        }
                    }
                }
            }

            public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
            {
                if (target.CanBeChasedBy())
                {
                    if (proj.modProjectile is TFProjectile TFProjectile)
                    {
                        crit = false;

                        if (player.GetModPlayer<TFModPlayer>().recentDamage + damage < player.GetModPlayer<TFModPlayer>().randomCritStorageCap)
                        {
                            player.GetModPlayer<TFModPlayer>().recentDamage += damage;
                        }
                        else
                        {
                            player.GetModPlayer<TFModPlayer>().recentDamage += player.GetModPlayer<TFModPlayer>().randomCritStorageCap - player.GetModPlayer<TFModPlayer>().recentDamage;
                        }
                    }
                }
            }

            public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
            {
                if (item.modItem is TFWeapon TFWeapon)
                {
                    crit = false;

                    if (player.GetModPlayer<TFModPlayer>().recentDamage + damage < player.GetModPlayer<TFModPlayer>().randomCritStorageCap)
                    {
                        player.GetModPlayer<TFModPlayer>().recentDamage += damage;
                    }
                    else
                    {
                        player.GetModPlayer<TFModPlayer>().recentDamage += player.GetModPlayer<TFModPlayer>().randomCritStorageCap - player.GetModPlayer<TFModPlayer>().recentDamage;
                    }
                }
            }

            public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
            {
                if (proj.modProjectile is TFProjectile TFProjectile)
                {
                    crit = false;

                    if (player.GetModPlayer<TFModPlayer>().recentDamage + damage < player.GetModPlayer<TFModPlayer>().randomCritStorageCap)
                    {
                        player.GetModPlayer<TFModPlayer>().recentDamage += damage;
                    }
                    else
                    {
                        player.GetModPlayer<TFModPlayer>().recentDamage += player.GetModPlayer<TFModPlayer>().randomCritStorageCap - player.GetModPlayer<TFModPlayer>().recentDamage;
                    }
                }
            }
            #endregion
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

        public abstract class TFWeapon : ModItem
        {
            public string basicUseSound = "";

            public string meleeEntityCollisionSound = "";
            public bool meleeEntityCollided = false;
            public bool meleeEntityCollisionSoundCheck = false;

            public string meleeWorldCollisionSound = "";
            public bool meleeWorldCollided = false;
            public bool meleeWorldCollisionSoundCheck = false;

            public string holsterSound = "";
            public bool holsterSoundCheck = false;

            public string deploySound = "";
            public bool deploySoundCheck = false;


            /// <summary>
            /// Item type, used in the tooltip.
            /// </summary>
            public string TFItemType = "";
            /// <summary>
            /// If this item is a swing melee. Sets damage for melee, used for most melees.
            /// </summary>
            public bool isSwingMelee = default;
            /// <summary>
            /// If this item is ranged. Sets damage to ranged, used for most primaries and secondaries.
            /// </summary>
            public bool isRanged = default;
            /// <summary>
            /// If this item is a SF spell. Sets damage to magic, among other things.
            /// </summary>
            public bool isScreamFortressSpell = default;
            /// <summary>
            /// If this item is strange. Counts kills.
            /// </summary>
            public bool isStrange = default;
            /// <summary>
            /// If this item has a killstreaker active.
            /// </summary>
            public bool killstreaksActive = default;

            /// <summary>
            /// If this item is from a contract or not. Used for determining sell prices.
            /// </summary>
            public bool contractItem = default;
            /// <summary>
            /// Item quality for name color. See <see cref="TFColorID"/> enum.
            /// </summary>
            public int TFItemQuality = 0;
            /// <summary>
            /// Item level for tooltip.
            /// </summary>
            public int TFItemLevel;

            /// <summary>
            /// Type of strange rank, for specific items. See <see cref="StrangeRankType"/> enum.
            /// </summary>
            public int strangeRankType = (int)StrangeRankType.Generic;
            /// <summary>
            /// What each strange point is referred to as.
            /// </summary>
            public string strangeStat = "Kills";
            /// <summary>
            /// Type of strange rank as a string, shown in the tooltip.
            /// </summary>
            public string strangeRank = "Strange";
            /// <summary>
            /// Amount of strange points stored.
            /// </summary>
            public int strangePoints = 0;

            /// <summary>
            /// How long, in ticks, it takes for the item to deploy.
            /// </summary>
            public int deployTimeMax = 0;
            /// <summary>
            /// The item's remaining deploy time, in ticks.
            /// </summary>
            public int deployTime = 0;

            float tooltipHeight;
            string longestTooltipLine = "";

            /// Enable random crits.
            public bool canCrit = true;


            public Vector2 ShootSpawnPos(Player player, float horizontalOffset, float verticalOffset)
            {
                return player.MountedCenter + (new Vector2(player.direction * horizontalOffset, verticalOffset)).RotatedBy(player.itemRotation + player.fullRotation);
            }

            /// <summary>
            /// Generates a hitbox for TF melee weapons.
            /// </summary>
            /// <param name="player">Player parameter.</param>
            /// <param name="horizontalOffset">The hitbox's horizontal offset.</param>
            /// <param name="verticalOffset">The hitbox's vertical offset.</param>
            /// <param name="width">The hitbox's width.</param>
            /// <param name="height">The hitbox's height.</param>
            /// <returns></returns>
            public virtual Rectangle TFMeleeHitbox(Player player, float horizontalOffset, float verticalOffset, int width, int height)
            {
                Vector2 targetDirection = new Vector2(0f, 0f);
                if (player == Main.player[Main.myPlayer])
                {
                    targetDirection = Vector2.Normalize(Main.MouseWorld - player.MountedCenter);
                }
                Rectangle hitbox = new Rectangle((int)(player.MountedCenter.X + (player.direction == -1 ? -width - horizontalOffset : horizontalOffset)), (int)(player.MountedCenter.Y + verticalOffset * player.gravDir + (player.gravDir == -1 ? (-height) : 0f)), width, height);
                if (player == Main.player[Main.myPlayer])
                {
                    Vector2 position = player.MountedCenter + new Vector2(width * 0.5f - horizontalOffset, 0f).RotatedBy(targetDirection.ToRotation()) - hitbox.Size() * 0.5f;
                    hitbox.X = (int)(position.X);
                    hitbox.Y = (int)(position.Y);
                }
                return hitbox;
            }

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

            public virtual bool CanShootWithOffset(Player player, float horizontalOffset, float verticalOffset)
            {
                Vector2 spawningPosition = (player.MountedCenter + new Vector2(player.direction * horizontalOffset, verticalOffset).RotatedBy(player.itemRotation + player.fullRotation));

                if (Collision.CanHitLine(player.MountedCenter, 0, 0, spawningPosition, 0, 0))
                {
                    return true;
                }

                return false;
            }

            /// <summary>
            /// System for TF tooltips.
            /// </summary>
            /// <param name="tooltips">Tooltips parameter.</param>
            /// <param name="attributeColor">The tooltip's color.</param>
            /// <param name="attributeText">The tooltip text.</param>
            public virtual void AddTFAttribute(List<TooltipLine> tooltips, Color attributeColor, string attributeText)
            {
                int index = tooltips.FindIndex(tooltip => tooltip.Name == "ItemName");

                if (index >= 0 && attributeText != default)
                {
                    TooltipLine attribute = new TooltipLine(mod, "ItemName", attributeText);
                    attribute.overrideColor = attributeColor;
                    tooltips.Add(attribute);
                }
            }

            public virtual void StrangeSystem()
            {
                switch (strangeRankType)
                {
                    default:
                    case (int)StrangeRankType.Generic:
                        if (strangePoints < 10)
                        {
                            strangeRank = "Strange";
                        }
                        else if (strangePoints >= 10 && strangePoints < 25)
                        {
                            strangeRank = "Unremarkable";
                        }
                        else if (strangePoints >= 25 && strangePoints < 45)
                        {
                            strangeRank = "Scarcely Lethal";
                        }
                        else if (strangePoints >= 45 && strangePoints < 70)
                        {
                            strangeRank = "Mildly Menacing";
                        }
                        else if (strangePoints >= 70 && strangePoints < 100)
                        {
                            strangeRank = "Somewhat Threatening";
                        }
                        else if (strangePoints >= 100 && strangePoints < 135)
                        {
                            strangeRank = "Uncharitable";
                        }
                        else if (strangePoints >= 135 && strangePoints < 175)
                        {
                            strangeRank = "Notably Dangerous";
                        }
                        else if (strangePoints >= 175 && strangePoints < 225)
                        {
                            strangeRank = "Sufficiently Lethal";
                        }
                        else if (strangePoints >= 225 && strangePoints < 275)
                        {
                            strangeRank = "Truly Feared";
                        }
                        else if (strangePoints >= 275 && strangePoints < 350)
                        {
                            strangeRank = "Spectacularly Lethal";
                        }
                        else if (strangePoints >= 350 && strangePoints < 500)
                        {
                            strangeRank = "Gore-Spattered";
                        }
                        else if (strangePoints >= 500 && strangePoints < 750)
                        {
                            strangeRank = "Wicked Nasty";
                        }
                        else if (strangePoints >= 750 && strangePoints < 999)
                        {
                            strangeRank = "Positively Inhumane";
                        }
                        else if (strangePoints >= 999 && strangePoints < 1000)
                        {
                            strangeRank = "Totally Ordinary";
                        }
                        else if (strangePoints >= 1000 && strangePoints < 1500)
                        {
                            strangeRank = "Face-Melting";
                        }
                        else if (strangePoints >= 1500 && strangePoints < 2500)
                        {
                            strangeRank = "Rage-Inducing";
                        }
                        else if (strangePoints >= 2500 && strangePoints < 5000)
                        {
                            strangeRank = "Server-Clearing";
                        }
                        else if (strangePoints >= 5000 && strangePoints < 7500)
                        {
                            strangeRank = "Epic";
                        }
                        else if (strangePoints >= 7500 && strangePoints < 7616)
                        {
                            strangeRank = "Legendary";
                        }
                        else if (strangePoints >= 7616 && strangePoints < 8500)
                        {
                            strangeRank = "Australian";
                        }
                        else if (strangePoints >= 8500)
                        {
                            strangeRank = "Hale's Own";
                        }
                        break;
                    case (int)StrangeRankType.HolidayPunch:
                        if (strangePoints < 10)
                        {
                            strangeRank = "Strange";
                        }
                        else if (strangePoints >= 10 && strangePoints < 25)
                        {
                            strangeRank = "Unremarkable";
                        }
                        else if (strangePoints >= 25 && strangePoints < 45)
                        {
                            strangeRank = "Almost Amusing";
                        }
                        else if (strangePoints >= 45 && strangePoints < 70)
                        {
                            strangeRank = "Mildly Mirthful";
                        }
                        else if (strangePoints >= 70 && strangePoints < 100)
                        {
                            strangeRank = "Somewhat Droll";
                        }
                        else if (strangePoints >= 100 && strangePoints < 135)
                        {
                            strangeRank = "Thigh-Slapping";
                        }
                        else if (strangePoints >= 135 && strangePoints < 175)
                        {
                            strangeRank = "Notably Cheery";
                        }
                        else if (strangePoints >= 175 && strangePoints < 225)
                        {
                            strangeRank = "Sufficiently Wry";
                        }
                        else if (strangePoints >= 225 && strangePoints < 275)
                        {
                            strangeRank = "Truly Feared";
                        }
                        else if (strangePoints >= 275 && strangePoints < 350)
                        {
                            strangeRank = "Spectacularly Jocular";
                        }
                        else if (strangePoints >= 350 && strangePoints < 500)
                        {
                            strangeRank = "Riotous";
                        }
                        else if (strangePoints >= 500 && strangePoints < 749)
                        {
                            strangeRank = "Wicked Funny";
                        }
                        else if (strangePoints >= 749 && strangePoints < 750)
                        {
                            strangeRank = "Totally Unamusing";
                        }
                        else if (strangePoints >= 750 && strangePoints < 1000)
                        {
                            strangeRank = "Positively Persiflagious";
                        }
                        else if (strangePoints >= 1000 && strangePoints < 1500)
                        {
                            strangeRank = "Frown-Annihilating";
                        }
                        else if (strangePoints >= 1500 && strangePoints < 2500)
                        {
                            strangeRank = "Grin-Inducing";
                        }
                        else if (strangePoints >= 2500 && strangePoints < 5000)
                        {
                            strangeRank = "Server-Clearing";
                        }
                        else if (strangePoints >= 5000 && strangePoints < 7500)
                        {
                            strangeRank = "Epic";
                        }
                        else if (strangePoints >= 7500 && strangePoints < 7923)
                        {
                            strangeRank = "Legendary";
                        }
                        else if (strangePoints >= 7923 && strangePoints < 8500)
                        {
                            strangeRank = "Australian";
                        }
                        else if (strangePoints >= 8500)
                        {
                            strangeRank = "Mann Co. Select";
                        }
                        break;
                    case (int)StrangeRankType.InvisWatch:
                        if (strangePoints < 200)
                        {
                            strangeRank = "Strange";
                        }
                        else if (strangePoints >= 200 && strangePoints < 500)
                        {
                            strangeRank = "Unremarkable";
                        }
                        else if (strangePoints >= 500 && strangePoints < 900)
                        {
                            strangeRank = "Scarcely Shocking";
                        }
                        else if (strangePoints >= 900 && strangePoints < 1337)
                        {
                            strangeRank = "Mildly Magnetizing";
                        }
                        else if (strangePoints >= 1337 && strangePoints < 2000)
                        {
                            strangeRank = "Somewhat Inducting";
                        }
                        else if (strangePoints >= 2000 && strangePoints < 2700)
                        {
                            strangeRank = "Unfortunate";
                        }
                        else if (strangePoints >= 2700 && strangePoints < 3500)
                        {
                            strangeRank = "Notably Deleterious";
                        }
                        else if (strangePoints >= 3500 && strangePoints < 4500)
                        {
                            strangeRank = "Sufficiently Ruinous";
                        }
                        else if (strangePoints >= 4500 && strangePoints < 5500)
                        {
                            strangeRank = "Truly Conducting";
                        }
                        else if (strangePoints >= 5500 && strangePoints < 7000)
                        {
                            strangeRank = "Spectacularly Psuedoful";
                        }
                        else if (strangePoints >= 7000 && strangePoints < 9000)
                        {
                            strangeRank = "Ion-Spattered";
                        }
                        else if (strangePoints >= 9000 && strangePoints < 12000)
                        {
                            strangeRank = "Wickedly Dynamizing";
                        }
                        else if (strangePoints >= 12000 && strangePoints < 16000)
                        {
                            strangeRank = "Positively Plasmatic";
                        }
                        else if (strangePoints >= 16000 && strangePoints < 21337)
                        {
                            strangeRank = "Totally Ordinary";
                        }
                        else if (strangePoints >= 21337 && strangePoints < 35000)
                        {
                            strangeRank = "Circuit-Melting";
                        }
                        else if (strangePoints >= 35000 && strangePoints < 58007)
                        {
                            strangeRank = "Nullity-Inducing";
                        }
                        else if (strangePoints >= 58007 && strangePoints < 90000)
                        {
                            strangeRank = "Server-Clearing";
                        }
                        else if (strangePoints >= 90000 && strangePoints < 120000)
                        {
                            strangeRank = "Epic";
                        }
                        else if (strangePoints >= 120000 && strangePoints < 140000)
                        {
                            strangeRank = "Legendary";
                        }
                        else if (strangePoints >= 140000 && strangePoints < 160000)
                        {
                            strangeRank = "Australian";
                        }
                        else if (strangePoints >= 160000)
                        {
                            strangeRank = "Mann Co. Select";
                        }
                        break;
                    case (int)StrangeRankType.Mantreads:
                        break;
                    case (int)StrangeRankType.Sapper:
                        break;
                    case (int)StrangeRankType.SpiritOfGiving:
                        break;
                    case (int)StrangeRankType.Cosmetic:
                        break;
                    case (int)StrangeRankType.DuckJournal:
                        break;
                    case (int)StrangeRankType.SoulGargoyle:
                        break;
                }
            }

            /// <summary>
            /// For setting constantly changing fields, i.e. variation for weapon sounds.
            /// </summary>
            public virtual void SetConstantDefaults()
            {

            }

            /// <summary>
            /// SetDefaults(), for TF items.
            /// </summary>
            public virtual void TFDefaults()
            {

            }

            public sealed override void SetDefaults()
            {
                SetConstantDefaults();
                TFDefaults();
                if (isSwingMelee)
                {
                    item.useStyle = ItemUseStyleID.SwingThrow;
                    item.melee = true;
                    item.useTurn = true;
                    item.holdStyle = ItemHoldStyleID.HoldingOut;
                    item.value = 200000;

                }
                if (isRanged)
                {
                    item.useStyle = ItemUseStyleID.HoldingOut;
                    item.useAmmo = ModContent.ItemType<AmmoBox>();
                    item.ranged = true;
                    item.noMelee = true;
                    item.value = 300000;
                    item.shoot = 10;
                }
                item.magic = isScreamFortressSpell;
                item.noUseGraphic = true;
                item.autoReuse = true;
                TFDefaults();
            }

            /// <summary>
            /// Allow things to happen while in the inventory.
            /// </summary>
            /// <param name="player">Player parameter.</param>
            public virtual void TFUpdateInventory(Player player)
            {

            }

            public override void UpdateInventory(Player player)
            {
                TFUpdateInventory(player);
                SetConstantDefaults();
                StrangeSystem();
                DrawWeaponHoldout(player);
                if (player.HeldItem.modItem == this)
                {
                    if (deploySound != "" && !deploySoundCheck)
                    {
                        Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + deploySound));
                        deploySoundCheck = true;
                    }
                }
                if (player.HeldItem.modItem != this)
                {
                    if (holsterSound != "" && !holsterSoundCheck)
                    {
                        Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + holsterSound));
                        holsterSoundCheck = true;
                    }
                }
            }

            public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
            {
                crit = false;
            }

            public override void ModifyHitPvp(Player player, Player target, ref int damage, ref bool crit)
            {
                crit = false;
            }

            /// <summary>
            /// For setting TF tooltips.
            /// </summary>
            /// <param name="tooltips">Tooltips parameter.</param>
            public virtual void TFDescription(List<TooltipLine> tooltips)
            {

            }

            public override void ModifyTooltips(List<TooltipLine> tooltips)
            {
                TFDefaults();

                string[] linesToRemove =
                {
                    "CritChance",
                    "Speed",
                    "Knockback",
                };

                string[] linesToRecolor =
                {
                    "Damage",
                    "Ammo",
                    "Consumable",
                };

                foreach (TooltipLine line in tooltips)
                {
                    if (line.mod == "Terraria")
                    {
                        if (line.Name == "ItemName")
                        {
                            if (isStrange)
                            {
                                line.overrideColor = TFColor[(int)TFColorID.Strange];
                                line.text = strangeRank + " " + line.text;
                            }
                            else
                            {
                                line.overrideColor = TFColor[TFItemQuality];
                            }
                        }

                        for (int i = 0; i < linesToRecolor.Length; i++)
                        {
                            int index = tooltips.FindIndex(tooltip => tooltip.Name == linesToRecolor[i]);

                            if (index >= 0 && line.Name == linesToRecolor[i])
                            {
                                line.overrideColor = TFColor[(int)TFColorID.AttributeNeutral];
                            }
                        }
                    }
                }

                for (int i = 0; i < linesToRemove.Length; i++)
                {
                    int index = tooltips.FindIndex(tooltip => tooltip.Name == linesToRemove[i]);

                    if (index >= 0)
                    {
                        tooltips.RemoveAt(index);
                    }
                }

                int index2 = tooltips.FindIndex(tooltip => tooltip.Name == "ItemName");
                if (index2 >= 0)
                {
                    if (TFItemType != "")
                    {
                        if (isStrange)
                        {
                            TooltipLine level = new TooltipLine(mod, "ItemName", strangeRank + " " + TFItemType + " - " + strangeStat + ": " + strangePoints);
                            level.overrideColor = TFColor[(int)TFColorID.AttributeLevel];
                            tooltips.Add(level);
                        }
                        else
                        {
                            TooltipLine level = new TooltipLine(mod, "ItemName", "Level " + TFItemLevel + " " + TFItemType);
                            level.overrideColor = TFColor[(int)TFColorID.AttributeLevel];
                            tooltips.Add(level);
                        }
                    }

                }

                TFDescription(tooltips);
            }

            public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
            {
                SpriteBatch spriteBatch = Main.spriteBatch;
                DynamicSpriteFont tooltip = Main.fontMouseText;
                Vector2 drawPos = new Vector2(x, y);
                Rectangle topLeft = new Rectangle(0, 0, 16, 16);
                Rectangle topMiddle = new Rectangle(16, 0, 20, 16);
                Rectangle topRight = new Rectangle(36, 0, 16, 16);
                Rectangle middleLeft = new Rectangle(0, 16, 16, 20);
                Rectangle center = new Rectangle(16, 16, 20, 20);
                Rectangle middleRight = new Rectangle(36, 16, 16, 20);
                Rectangle bottomLeft = new Rectangle(0, 36, 16, 16);
                Rectangle bottomMiddle = new Rectangle(16, 36, 20, 16);
                Rectangle bottomRight = new Rectangle(36, 36, 16, 16);
                float horizontalPadding = 18f;
                float verticalPadding = 6f;

                string longestLine = "";
                foreach (TooltipLine tooltipLine in lines)
                {
                    if (tooltip.MeasureString(tooltipLine.text).X > tooltip.MeasureString(longestLine).X)
                    {
                        longestLine = tooltipLine.text;
                    }
                }
                Vector2 tooltipDimensions = tooltip.MeasureString(longestLine);
                longestTooltipLine = longestLine;
                {
                    foreach (TooltipLine line in lines)
                    {
                        tooltipHeight += tooltip.MeasureString(line.text).Y;
                    }
                }
                Texture2D texture = ModContent.GetTexture("TerrariaFortress/Items/TFItemTooltipUI");
                Color drawColor = Color.White;
                float drawScale = 1f;
                bool TrueForNormalDrawing_FalseForNewDrawing = true;

                switch (TrueForNormalDrawing_FalseForNewDrawing)
                {
                    case true:
                        #region Normal UI Drawing
                        spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding, -verticalPadding), topLeft, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        for (int i = 0; i < (tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) / topMiddle.Width; i++)
                        {
                            spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + topLeft.Width + i * topMiddle.Width, -verticalPadding), i > (tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) / topMiddle.Width - 1 ? new Rectangle(topMiddle.X, topMiddle.Y, (int)(tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) % topMiddle.Width, topMiddle.Height) : topMiddle, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        }
                        spriteBatch.Draw(texture, drawPos + new Vector2(tooltipDimensions.X - topRight.Width + horizontalPadding, -verticalPadding), topRight, drawColor, default, default, drawScale, SpriteEffects.None, 0f);

                        for (int i = 0; i < (tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) / center.Height; i++)
                        {
                            spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding, -verticalPadding + topLeft.Height + i * middleLeft.Height), i > (tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) / center.Height - 1 ? new Rectangle(middleLeft.X, middleLeft.Y, middleLeft.Width, (int)(tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) % middleLeft.Height) : middleLeft, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        }
                        for (int i = 0; i < (tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) / center.Width; i++)
                        {
                            for (int j = 0; j < (tooltipHeight - topMiddle.Height - bottomMiddle.Height + verticalPadding * 2) / center.Height; j++)
                            {
                                spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + middleLeft.Width + i * center.Width, -verticalPadding + topMiddle.Height + j * center.Height), new Rectangle(center.X, center.Y, i > (tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) / center.Width - 1 ? (int)(tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) % center.Width : center.Width, j > (tooltipHeight - topMiddle.Height - bottomMiddle.Height + verticalPadding * 2) / center.Height - 1 ? (int)(tooltipHeight - topMiddle.Height - bottomMiddle.Height + verticalPadding * 2) % center.Height : center.Height), drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                            }
                        }
                        for (int i = 0; i < (tooltipHeight - topRight.Height - bottomRight.Height + verticalPadding * 2) / center.Height; i++)
                        {
                            spriteBatch.Draw(texture, drawPos + new Vector2(tooltipDimensions.X - middleLeft.Width + horizontalPadding, -verticalPadding + topRight.Height + i * middleRight.Height), i > (tooltipHeight - topRight.Height - topRight.Height + verticalPadding * 2) / center.Height - 1 ? new Rectangle(middleRight.X, middleRight.Y, middleRight.Width, (int)(tooltipHeight - topRight.Height - bottomRight.Height + verticalPadding * 2) % middleRight.Height) : middleRight, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        }

                        spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding, tooltipHeight - bottomRight.Height + verticalPadding), bottomLeft, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        for (int i = 0; i < (tooltipDimensions.X - bottomLeft.Width - bottomRight.Width + horizontalPadding * 2) / bottomMiddle.Width; i++)
                        {
                            spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + bottomLeft.Width + i * bottomMiddle.Width, tooltipHeight - bottomRight.Height + verticalPadding), i > (tooltipDimensions.X - bottomLeft.Width - bottomRight.Width + horizontalPadding * 2) / bottomMiddle.Width - 1 ? new Rectangle(bottomMiddle.X, bottomMiddle.Y, (int)(tooltipDimensions.X - bottomLeft.Width - bottomRight.Width + horizontalPadding * 2) % bottomMiddle.Width, bottomMiddle.Height) : bottomMiddle, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        }
                        spriteBatch.Draw(texture, drawPos + new Vector2(tooltipDimensions.X - bottomRight.Width + horizontalPadding, tooltipHeight - bottomRight.Height + verticalPadding), bottomRight, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        #endregion
                        break;
                    case false:
                        #region Proper UI Drawing
                        spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding, -verticalPadding), topLeft, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        for (int i = 0; i < (tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) / topMiddle.Width; i++)
                        {
                            spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + topLeft.Width + i * topMiddle.Width, -verticalPadding), i > (tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) / topMiddle.Width - 1 ? new Rectangle(topMiddle.X, topMiddle.Y, (int)(tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) % topMiddle.Width, topMiddle.Height) : topMiddle, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                            if (i > (tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) / topMiddle.Width - 1)
                            {
                                spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + topLeft.Width + i * topMiddle.Width + ((int)(tooltipDimensions.X - topLeft.Width - topRight.Width + horizontalPadding * 2) % topMiddle.Width), -verticalPadding), topRight, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                            }
                        }

                        for (int i = 0; i < (tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) / center.Height; i++)
                        {
                            spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding, -verticalPadding + topLeft.Height + i * middleLeft.Height), i > (tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) / center.Height - 1 ? new Rectangle(middleLeft.X, middleLeft.Y, middleLeft.Width, (int)(tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) % middleLeft.Height) : middleLeft, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                            if (i > (tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) / center.Height - 1)
                            {
                                spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding, -verticalPadding + topLeft.Height + i * middleLeft.Height + ((int)(tooltipHeight - topLeft.Height - bottomLeft.Height + verticalPadding * 2) % middleLeft.Height)), bottomLeft, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                            }
                        }
                        for (int i = 0; i < (tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) / center.Width; i++)
                        {
                            for (int j = 0; j < (tooltipHeight - topMiddle.Height - bottomMiddle.Height + verticalPadding * 2) / center.Height; j++)
                            {
                                spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + middleLeft.Width + i * center.Width, -verticalPadding + topMiddle.Height + j * center.Height), new Rectangle(center.X, center.Y, i > (tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) / center.Width - 1 ? (int)(tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) % center.Width : center.Width, j > (tooltipHeight - topMiddle.Height - bottomMiddle.Height + verticalPadding * 2) / center.Height - 1 ? (int)(tooltipHeight - topMiddle.Height - bottomMiddle.Height + verticalPadding * 2) % center.Height : center.Height), drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                            }
                            for (int k = 0; k < (tooltipHeight - topRight.Height - bottomRight.Height + verticalPadding * 2) / center.Height; k++)
                            {
                                if (i > (tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) / center.Width - 1)
                                {
                                    spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + middleLeft.Width + i * center.Width + ((int)(tooltipDimensions.X - middleLeft.Width - middleRight.Width + horizontalPadding * 2) % center.Width), -verticalPadding + topRight.Height + k * middleRight.Height), new Rectangle(middleRight.X, middleRight.Y, middleRight.Width, k > (tooltipHeight - topRight.Height - bottomRight.Height + verticalPadding * 2) / center.Height - 1 ? (int)(tooltipHeight - topRight.Height - bottomRight.Height + verticalPadding * 2) % middleRight.Height : middleRight.Height), drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                                }
                            }
                        }

                        //for (int i = 0; i < (tooltipDimensions.X - bottomLeft.Width - bottomRight.Width + horizontalPadding * 2) / bottomMiddle.Width; i++)
                        //{
                        //    spriteBatch.Draw(texture, drawPos + new Vector2(-horizontalPadding + bottomLeft.Width + i * bottomMiddle.Width, tooltipHeight - bottomRight.Height + verticalPadding), i > (tooltipDimensions.X - bottomLeft.Width - bottomRight.Width + horizontalPadding * 2) / bottomMiddle.Width - 1 ? new Rectangle(bottomMiddle.X, bottomMiddle.Y, (int)(tooltipDimensions.X - bottomLeft.Width - bottomRight.Width + horizontalPadding * 2) % bottomMiddle.Width, bottomMiddle.Height) : bottomMiddle, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        //}
                        //spriteBatch.Draw(texture, drawPos + new Vector2(tooltipDimensions.X - bottomRight.Width + horizontalPadding, tooltipHeight - bottomRight.Height + verticalPadding), bottomRight, drawColor, default, default, drawScale, SpriteEffects.None, 0f);
                        #endregion
                        break;
                }
                return true;
            }

            public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
            {
                DynamicSpriteFont tooltip = Main.fontMouseText;

                line.X = (int)(line.X + ((tooltip.MeasureString(longestTooltipLine).X - tooltip.MeasureString(line.text).X) * 0.5f));
                return true;
            }

            public virtual bool TFCanUseItem(Player player)
            {
                return true;
            }

            public override bool CanUseItem(Player player)
            {
                if (isSwingMelee)
                {
                    player.itemAnimation = item.useAnimation;
                    meleeEntityCollided = false;
                    meleeWorldCollided = false;
                    meleeEntityCollisionSoundCheck = false;
                    meleeWorldCollisionSoundCheck = false;
                    Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + basicUseSound));
                }

                return base.CanUseItem(player) && TFCanUseItem(player);
            }

            public override bool ConsumeAmmo(Player player)
            {
                return false;
            }

            public override bool NewPreReforge()
            {
                return false;
            }

            public override bool? PrefixChance(int pre, UnifiedRandom rand)
            {
                return pre == -3 || pre == -1 ? false : true;
            }

            /// <summary>
            /// Allows you to make things happen when this item strikes a tile in the world.
            /// </summary>
            /// <param name="player">Player parameter.</param>
            public virtual void OnHitTile(Player player)
            {

            }

            public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
            {
                if (isSwingMelee)
                {
                    if (player.itemAnimation == (int)(item.useTime * 0.7f))
                    {
                        noHitbox = false;
                    }
                    else
                    {
                        noHitbox = true;
                    }

                    if (!noHitbox)
                    {
                        for (int i = 0; i < hitbox.Width;)
                        {
                            if (i + 16 < hitbox.Width)
                            {
                                i += 16;
                            }
                            else
                            {
                                i += hitbox.Width - i;
                            }
                            for (int j = 0; j < hitbox.Height;)
                            {
                                if (j + 16 < hitbox.Height)
                                {
                                    j += 16;
                                }
                                else
                                {
                                    j += hitbox.Height - j;
                                }

                                Point point = new Vector2(hitbox.X + i, hitbox.Y + j).ToTileCoordinates();
                                if (WorldGen.SolidOrSlopedTile((int)point.X, (int)point.Y))
                                {
                                    meleeWorldCollided = true;
                                }
                            }
                        }

                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            Player impactedPlayer = Main.player[i];

                            if (HitboxCheckPlayer(player, hitbox, impactedPlayer))
                            {
                                meleeEntityCollided = true;
                            }
                        }

                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC impactedNPC = Main.npc[i];
    
                            if (HitboxCheckNPC(player, hitbox, impactedNPC))
                            {
                                meleeEntityCollided = true;
                            }
                            if (hitbox.Intersects(impactedNPC.Hitbox) && impactedNPC.active)
                            {
                                meleeEntityCollided = true;
                            }
                        }
                    }

                    if (meleeEntityCollided && !meleeEntityCollisionSoundCheck)
                    {
                        meleeEntityCollisionSoundCheck = true;
                        Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + meleeEntityCollisionSound));
                    }
                    if (!meleeEntityCollisionSoundCheck && meleeWorldCollided && !meleeWorldCollisionSoundCheck)
                    {
                        meleeWorldCollisionSoundCheck = true;
                        OnHitTile(player);
                        Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/" + meleeWorldCollisionSound));
                    }
                }
                if (isRanged)
                {
                    noHitbox = true;
                }
            }

            #region Drawcode Methods
            #region Draw Fields
            public virtual Texture2D ItemTexture(Player player)
            {
                return Main.itemTexture[player.HeldItem.type];
            }

            public virtual float MeleeRotation(Player player)
            {
                return player.itemRotation * player.gravDir;
            }

            public virtual Color LightColor(Player player)
            {
                return Lighting.GetColor((int)(player.MountedCenter.X / 16), (int)(player.MountedCenter.Y / 16));
            }

            public virtual Color RangedWeaponColor(Player player)
            {
                int alphaOffset = (int)(Math.Abs(player.stealth - 1) * 255);
                return new Color(LightColor(player).R - alphaOffset, LightColor(player).G - alphaOffset, LightColor(player).B - alphaOffset, LightColor(player).A - alphaOffset);
            }

            public virtual Color GlowmaskColor(Player player)
            {
                int alphaOffset = (int)(Math.Abs(player.stealth - 1) * 255);
                return new Color(255 - alphaOffset, 255 - alphaOffset, 255 - alphaOffset, 255 - alphaOffset);
            }

            public virtual SpriteEffects FlipEffect(Player player)
            {
                return (player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None) | (player.gravDir == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None);
            }

            public virtual int MuzzleFrame(Player player, int frames = 0, float fps = 60)
            {
                return (int)MathHelper.Clamp(Math.Abs(player.itemTime - player.HeldItem.useTime) / (60 / fps) - 1, 0, frames - 1);
            }
            #endregion

            public virtual void DrawSimpleHoldout(Texture2D texture, Vector2 offset, Vector2 originOffset, float rotation, Color suppliedColor, Player player, int horizontalFrames = 1, int verticalFrames = 1, int frameX = 0, int frameY = 0, int shader = 0)
            {
                Rectangle rectangle = texture.Frame(horizontalFrames, verticalFrames, frameX, frameY);
                Vector2 drawPosition = (player.MountedCenter - Main.screenPosition + new Vector2(player.direction * offset.X, player.gfxOffY + player.gravDir * offset.Y));
                drawPosition = new Vector2((int)drawPosition.X, (int)drawPosition.Y);
                Vector2 origin = rectangle.Size() * 0.5f + new Vector2(0f - offset.X * player.direction + originOffset.X * player.direction - offset.X * player.direction + (rectangle.Width * -player.direction * 0.5f), player.gravDir * (-offset.Y + originOffset.Y));
                DrawData drawData = new DrawData(texture, drawPosition, rectangle, suppliedColor, rotation, origin, player.HeldItem.scale, FlipEffect(player), 0);
                drawData.shader = shader;
                Main.playerDrawData.Add(drawData);
            }

            public virtual void DrawSimpleMeleeHoldout(Texture2D texture, Vector2 offset, Vector2 originOffset, float rotation, Color suppliedColor, Player player, int horizontalFrames = 1, int verticalFrames = 1, int frameX = 0, int frameY = 0, int shader = 0)
            {
                Rectangle rectangle = texture.Frame(horizontalFrames, verticalFrames, frameX, frameY);
                originOffset += new Vector2(0f, rectangle.Height * 0.5f);
                offset = offset.RotatedBy(MathHelper.ToRadians(45f));
                offset = new Vector2((int)offset.X, (int)offset.Y);
                DrawSimpleHoldout(texture, offset, originOffset, rotation, suppliedColor, player, horizontalFrames, verticalFrames, frameX, frameY, shader);
            }

            public virtual void DrawWeaponHoldout(Player player)
            {

            }
            #endregion
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