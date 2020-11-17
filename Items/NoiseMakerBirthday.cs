using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaFortress.Dusts;
using static TerrariaFortress.TerrariaFortress;

namespace TerrariaFortress.Items
{
    public class NoiseMakerBirthday : TFWeapon
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Noise Maker - TF Birthday");
        }

        public override void SafeSetDefaults()
        {
            TFItemType = "Party Favor";
            TFItemQuality = (int)TFColorID.Unique;
            TFItemLevel = 5;
            item.width = 44;
            item.height = 40;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useTime = 70;
            item.useAnimation = 70;
            item.value = Item.buyPrice(0);
            item.autoReuse = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributePositive], "Unlimited use");
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeNeutral], "Thank you for 2000 downloads!");
            AddTFAttribute(tooltips, TFColor[(int)TFColorID.AttributeNeutral], "Release barrages of joy and festivity on use");
        }

        public override bool CanUseItem(Player player)
        {
            Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/NoiseMaker/happy_birthday_tf_" + Main.rand.Next(30)), 0.4f);
            Main.PlaySound(SoundLoader.customSoundType, (int)player.MountedCenter.X, (int)player.MountedCenter.Y, mod.GetSoundSlot(SoundType.Custom, "Sounds/Custom/NoiseMaker/PyroThanksFor2k" + Main.rand.Next(6)));
            CombatText combatText = Main.combatText[CombatText.NewText(new Rectangle((int)player.MountedCenter.X, (int)player.MountedCenter.Y, 0, 0), new Color(Main.DiscoR, Main.DiscoB, Main.DiscoG), "Thank you for 2000+ downloads!", true, true)];
            combatText.rotation = 0f;
            combatText.velocity.Y *= 2f;
            combatText.lifeTime = 100;
            CombatText combatText2 = Main.combatText[CombatText.NewText(new Rectangle((int)player.MountedCenter.X, (int)player.MountedCenter.Y, 0, 0), new Color(255, 127, 0), "-Terraria Fortress DevTeam", true, true)];
            combatText2.rotation = 0f;
            combatText2.velocity.Y *= 2f;
            combatText2.position.Y += 16f;
            combatText2.lifeTime = 100;

            #region Vanilla Firework
            Vector2 vector17 = (vector17 = ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2());
            float num674 = Main.rand.Next(5, 9);
            float num675 = (float)Main.rand.Next(10, 15) * 0.66f;
            float num676 = (float)Main.rand.Next(4, 7) / 2f;
            int num677 = 30;
            for (int num678 = 0; (float)num678 < (float)num677 * num674; num678++)
            {
                if (num678 % num677 == 0)
                {
                    vector17 = vector17.RotatedBy((float)Math.PI * 2f / num674);
                }
                float scaleFactor3 = MathHelper.Lerp(num676, num675, (float)(num678 % num677) / (float)num677);
                int num679 = 130;
                int num680 = Dust.NewDust(new Vector2(player.MountedCenter.X, player.MountedCenter.Y - 160f), 6, 6, num679, 0f, 0f, 100);
                Dust dust197 = Main.dust[num680];
                Dust dust2 = dust197;
                dust2.velocity *= 0.1f;
                dust197 = Main.dust[num680];
                dust2 = dust197;
                dust2.velocity += vector17 * scaleFactor3;
                Main.dust[num680].scale = 1.3f;
                Main.dust[num680].noGravity = true;
            }
            for (int num681 = 0; num681 < 100; num681++)
            {
                float num682 = num675;
                if (num681 < 30)
                {
                    num682 = (num676 + num675) / 2f;
                }
                int num683 = 130;
                int num684 = Dust.NewDust(new Vector2(player.MountedCenter.X, player.MountedCenter.Y - 160f), 6, 6, num683, 0f, 0f, 100);
                float num685 = Main.dust[num684].velocity.X;
                float y6 = Main.dust[num684].velocity.Y;
                if (num685 == 0f && y6 == 0f)
                {
                    num685 = 1f;
                }
                float num686 = (float)Math.Sqrt(num685 * num685 + y6 * y6);
                num686 = num682 / num686;
                num685 *= num686;
                y6 *= num686;
                Dust dust198 = Main.dust[num684];
                Dust dust2 = dust198;
                dust2.velocity *= 0.5f;
                Main.dust[num684].velocity.X += num685;
                Main.dust[num684].velocity.Y += y6;
                Main.dust[num684].scale = 1.3f;
                Main.dust[num684].noGravity = true;
            }
            #endregion
            int dustLength = 72;
            for (int i = 0; i < dustLength; i++)
            {
                Vector2 vel = new Vector2(0f, -2f).RotatedBy(MathHelper.ToRadians(i * (360 / dustLength)));
                Dust dust1 = Main.dust[Dust.NewDust(player.position, player.width, player.height, Main.rand.Next(139, 143), vel.X, vel.Y - 1f, 0, default, 1f)];
            }

            for (int i = 0; i < Main.rand.Next(5, 26); i++)
            {
                Dust.NewDustPerfect(player.Center + new Vector2(0f, Main.rand.Next(-8, 9)), ModContent.DustType<BirthdayBalloon>(), new Vector2(i % 2 == 0 ? Main.rand.NextFloat(1f, 7f) : Main.rand.NextFloat(-7f, 1f), 0f), 0, default, 1f);
            }
            return base.CanUseItem(player);
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6f, -6f);
        }

        public override void UpdateInventory(Player player)
        {
            if (player.HeldItem.type == item.type)
            {
                if (player.itemAnimation != item.useAnimation)
                {
                    float brightness = MathHelper.Clamp((player.itemAnimation) * 0.03f, 0f, 2f);
                    Lighting.AddLight(player.MountedCenter, Main.DiscoR / 255f * brightness, Main.DiscoG / 255f * brightness, Main.DiscoB / 255f * brightness);
                }
            }
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.GetTexture("TerrariaFortress/Items/NoiseMakerBirthday");
            Texture2D texture2 = ModContent.GetTexture("TerrariaFortress/Items/NoiseMakerBirthdayGlowmask");

            spriteBatch.Draw(texture, item.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), item.GetAlpha(lightColor), rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture2, item.Center - Main.screenPosition, new Rectangle(0, 0, texture.Width, texture.Height), Color.White, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);

            return false;
        }
    }
}