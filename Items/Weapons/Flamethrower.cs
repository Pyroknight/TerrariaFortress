using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaFortress.Dusts;
using TerrariaFortress.Projectiles;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items.Weapons
{
    public class Flamethrower : TFWeapon
    {
        SoundEffectInstance flamethrowerStartSound;
        SoundEffectInstance flamethrowerLoopSound;

        int flamethrowerTimer = 0;
        int ammoConsumptionTimer = 0;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flame Thrower");
        }

        public override void SafeSetDefaults()
        {
            TFItemType = "Flame Thrower";
            isRanged = true;
            TFItemQuality = (int)TFColorID.Normal;
            TFItemLevel = 1;
            item.damage = 20;
            item.width = 46;
            item.height = 50;
            item.useTime = 0;
            item.useAnimation = 0;
            item.shootSpeed = 12f;
            item.shoot = ModContent.ProjectileType<FlamethrowerFlame>();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributePositive], "Extinguishing teammates restores 20 health");
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeNeutral], "Afterburn reduces Medi Gun healing and resist shield effects.");
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeNeutral], "Alt-Fire: Release a blast of air that pushes enemies and");
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeNeutral], "projectiles and extinguish teammates that are on fire.");
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {

            if (Collision.CanHitLine(player.MountedCenter, 0, 0, ShootSpawnPos(player, 60f, 0f), 0, 0))
            {
                if (player.altFunctionUse == 2 && player.CountItem(item.useAmmo, 20) >= 20)
                {   
                    Dust.NewDustPerfect(ShootSpawnPos(player, 60f, 0f) + new Vector2(-15, -15), ModContent.DustType<AirblastFlash>(), new Vector2(0f, 0f), 0, default, 1f);
                    Projectile.NewProjectile(ShootSpawnPos(player, 60f, 0f), new Vector2(speedX, speedY), type, 0, 0, player.whoAmI);
                }
                else
                {
                    Projectile.NewProjectile(ShootSpawnPos(player, 60f, 0f), new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);
                }
            }

            return false;
        }

        public override void UpdateInventory(Player player)
        {
            //if (player.HeldItem.type == ModContent.ItemType<Flamethrower>())
            //{   
            //    if (flamethrowerTimer > 0)
            //    {
            //        flamethrowerTimer--;
            //    }

            //    else if (flamethrowerTimer < 0)
            //    {
            //        flamethrowerTimer = 0;
            //    }

            //    if (!player.mouseInterface)
            //    {

            //        if (flamethrowerTimer == 0)
            //        {
            //            if (Main.mouseLeft)
            //            {
            //                if (Main.mouseLeftRelease)
            //                {
            //                    //flamethrowerStartSound = Main.PlaySound(SoundLoader.customSoundType, -1, -1, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerStart"));
            //                    //flamethrowerStartSound.Play();
            //                }

            //                //if (flamethrowerStartSound.State == SoundState.Stopped)
            //                //{
            //                //    //flamethrowerLoopSound.IsLooped = true;
            //                //    //flamethrowerLoopSound = Main.PlaySound(SoundLoader.customSoundType, -1, -1, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerLoop"));
            //                //    //flamethrowerLoopSound.Play();
            //                //}
            //            }

            //            if (Main.mouseRight && !Main.mouseLeft)
            //            {
            //                flamethrowerTimer = 45;
            //            }

            //            if (!Main.mouseLeftRelease)
            //            {
            //                if (!Main.mouseLeft)
            //                {
            //                    Main.PlaySound(SoundLoader.customSoundType, -1, -1, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/FlamethrowerEnd"));
            //                }
            //            }
            //        }
            //    }
            //}

            //if (player.HeldItem.type != ModContent.ItemType<Flamethrower>())
            //{
            //}
        }

        public override bool AltFunctionUse(Player player)
        {
            return !player.wet && !player.lavaWet && player.CountItem(item.useAmmo, 20) >= 20;
        }

        public override bool CanUseItem(Player player)
        {
            if (Collision.CanHitLine(player.MountedCenter, 0, 0, ShootSpawnPos(player, 60f, 0f), 0, 0))
            {
                if (player.altFunctionUse == 2)
                {
                    if (player.CountItem(item.useAmmo, 20) >= 20)
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            player.ConsumeItem(item.useAmmo);
                        }
                        item.damage = 0;
                        item.useTime = 45;
                        item.useAnimation = 45;
                        item.shoot = ModContent.ProjectileType<FlamethrowerAirblast>();
                    }
                }
                else if (player.CountItem(item.useAmmo, 1) >= 1)
                {
                    if (++ammoConsumptionTimer > 4 && Collision.CanHitLine(player.MountedCenter, 0, 0, ShootSpawnPos(player, 60f, 0f), 0, 0))
                    {
                        ammoConsumptionTimer = 0;
                        player.ConsumeItem(item.useAmmo);
                    }
                    item.damage = 20;
                    item.useTime = 0;
                    item.useAnimation = 0;
                    item.shoot = ModContent.ProjectileType<FlamethrowerFlame>();
                }
            }

            return base.CanUseItem(player);
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.GetTexture("TerrariaFortress/Items/Weapons/Flamethrower");
            Texture2D texture2 = ModContent.GetTexture("TerrariaFortress/Items/Weapons/FlamethrowerGlowmask");

            spriteBatch.Draw(texture, item.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), item.GetAlpha(lightColor), rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture2, item.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), Color.White, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
          
            return false;
        }
    }

    //public class DynamicSound : ModSound
    //{
    //    public override SoundEffectInstance PlaySound(ref SoundEffectInstance soundInstance, float volume, float pan, SoundType type)
    //    {
    //        float volumeFactor = 1f;

    //        if (soundInstance is null)
    //        {
    //            //This is a new sound instance

    //            soundInstance = sound.CreateInstance();
    //            soundInstance.Volume = volume * volumeFactor;
    //            soundInstance.Pan = pan;
    //            Main.PlaySoundInstance(soundInstance);
    //            return soundInstance;
    //        }
    //        else if (soundInstance.State == SoundState.Stopped)
    //        {
    //            //This is an existing sound instance that just stopped (OPTIONAL: use this if you want a looping sound effect!)

    //            soundInstance.Volume = volume * volumeFactor;
    //            soundInstance.Pan = pan;
    //            Main.PlaySoundInstance(soundInstance);
    //            return soundInstance;
    //        }

    //        //This is an existing sound instance that's still playing

    //        soundInstance.Volume = volume * volumeFactor;
    //        soundInstance.Pan = pan;
    //        return soundInstance;
    //    }
    //}
}