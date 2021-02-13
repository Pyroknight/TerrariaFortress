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
    public abstract class TFBuilding : TFProjectile
    {
        /// <summary>
        /// The building's current health.
        /// </summary>
        public int life = 0;
        /// <summary>
        /// The building's max health.
        /// </summary>
        public int lifeMax = TFUtils.BuildingStats.maxHealth[TFUtils.BuildingStats.Levels.Three];
        /// <summary>
        /// The building's initial cost to build.
        /// </summary>
        public int buildCost = TFUtils.BuildingStats.buildCost[TFUtils.BuildingStats.Types.Sentry];
        /// <summary>
        /// How much metal each building upgrade takes.
        /// </summary>
        public int upgradeCost = 200;
        /// <summary>
        /// If the building has already completed initial construction.
        /// </summary>
        public bool fullyConstructed = false;
        public bool upgrading = false;
        /// <summary>
        /// The building's current level for statistics. Specifically, this is set to 0 for mini-sentries.
        /// </summary>
        public int statLevel = 1;
        /// <summary>
        /// The building's current level. This is set to 1 for sentries and mini-sentries.
        /// </summary>
        public int level = 1;

        /// <summary>
        /// Called upon initial construction.
        /// </summary>
        public virtual void OnConstruct()
        {

        }

        /// <summary>
        /// Called upon the first tick of hauling this building.
        /// </summary>
        public virtual void OnHaul()
        {

        }

        /// <summary>
        /// Called while this building is being hauled.
        /// </summary>
        public virtual void Hauling()
        {

        }

        /// <summary>
        /// Called upon the first tick of rebuilding this building.
        /// </summary>
        public virtual void Rebuild()
        {

        }

        /// <summary>
        /// Called when this building is destroyed.
        /// </summary>
        /// <param name="manual">If the engineer destroyed it themself via destruction P.D.A.</param>
        public void Destroy(bool manual)
        {
            projectile.Kill();
            OnDestroy(manual);
        }

        /// <summary>
        /// Called when this building is destroyed.
        /// </summary>
        /// <param name="manual">If the engineer destroyed it themself via destruction P.D.A.</param>
        public virtual void OnDestroy(bool manual)
        {
            ReleaseScrap();
        }

        /// <summary>
        /// Releases scrap metal, called in OnDestroy().
        /// </summary>
        public virtual void ReleaseScrap()
        {

        }
    }
}