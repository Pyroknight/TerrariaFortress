using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaFortress
{
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

        public virtual bool ItemCritCheck(Player player, TFWeapon tfWeapon)
        {
            if (tfWeapon.canCrit)
            {
                if (randomCritCheck <= storedRandomCritChance / 100f)
                {
                    return true;
                }

                if (critBoosted)
                {
                    return true;
                }
            }

            return false;
        }

        public override void PreUpdate()
        {
            #region Random Crit Calculation
            randomCritCheck = Main.rand.NextFloat();
            if (recentDamage > 0f)
            {
                recentDamage -= 0.5f;
            }
            storedRandomCritChance = MathHelper.Clamp((recentDamage > 0 ? recentDamage / 82.5f / 2f : 0) + 2, 2f, 12f);
            randomCrit = randomCritCheck <= (storedRandomCritChance / 100f);
            #endregion

            #region Rocket Jumping
            if (player.velocity.Y == 0f)
            {
                if (isRocketJumping)
                {
                    player.velocity.X *= 0.8f;
                    isRocketJumping = false;
                }
            }
            #endregion
        }

        #region Random Crit Check

        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (target.CanBeChasedBy())
            {
                if (item.modItem is TFWeapon)
                {
                    crit = false;

                    if (recentDamage + damage < randomCritStorageCap)
                    {
                        recentDamage += damage;
                    }
                    else
                    {
                        recentDamage += randomCritStorageCap - recentDamage;
                    }
                }
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (target.CanBeChasedBy())
            {
                if (proj.modProjectile is TerrariaFortress.TFProjectile)
                {
                    crit = false;

                    if (recentDamage + damage < randomCritStorageCap)
                    {
                        recentDamage += damage;
                    }
                    else
                    {
                        recentDamage += randomCritStorageCap - recentDamage;
                    }
                }
            }
        }

        public override void ModifyHitPvp(Item item, Player target, ref int damage, ref bool crit)
        {
            if (item.modItem is TFWeapon)
            {
                crit = false;

                if (recentDamage + damage < randomCritStorageCap)
                {
                    recentDamage += damage;
                }
                else
                {
                    recentDamage += randomCritStorageCap - recentDamage;
                }
            }
        }

        public override void ModifyHitPvpWithProj(Projectile proj, Player target, ref int damage, ref bool crit)
        {
            if (proj.modProjectile is TerrariaFortress.TFProjectile)
            {
                crit = false;

                if (recentDamage + damage < randomCritStorageCap)
                {
                    recentDamage += damage;
                }
                else
                {
                    recentDamage += randomCritStorageCap - recentDamage;
                }
            }
        }
        #endregion
    }
}