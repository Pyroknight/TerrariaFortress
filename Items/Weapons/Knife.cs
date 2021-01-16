//using Terraria;
//using static TerrariaFortress.TerrariaFortress;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Microsoft.Xna.Framework;
//using System.Collections.Generic;

//namespace TerrariaFortress.Items.Weapons
//{
//    // HUGE NOTE: THIS IS UNFINISHED, I PLAN ON FINISHING THIS LATER THIS WEEK AS THIS ITEM IS BURNING ME OUT. PLEASE DO NOT TOUCH OR ANYTHING. Comitting for now :)
//    public class Knife : ModItem
//    {
//        public const SoundType cust = SoundType.Custom;
//        public const string meleeSwing = "Sounds/Custom/MeleeSwing"; // I do this because I am smarter than you
//        public override void SetStaticDefaults()
//        {
//            DisplayName.SetDefault("Knife");
//        }
//        public override void SetDefaults()
//        {
//            item.damage = 65;
//            item.knockBack = 10;
//            item.width = 52;
//            item.height = 46;
//            item.value = 200000;
//            item.UseSound = mod.GetLegacySoundSlot(cust, meleeSwing);
//            item.useAnimation = 30;
//            item.useTime = 30;
//            item.useStyle = 6;
//            item.holdStyle = 1;
//        }
//        public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat)
//        {
//            if (player.GetModPlayer<TFModPlayer>().hasBackstabTarget)
//            {
//                mult = 100f;
//            }
//            else
//            {
//                mult = 1f;
//            }
//        }
//        public override bool UseItem(Player player)
//        {
//            if (player.itemAnimation == 1) // end of animation
//            {
//            }
//            return base.UseItem(player);
//        }
//        public override void HoldStyle(Player player)
//        {
//            if (player.direction == -1)
//            {
//                Vector2 origin = new Vector2(0, 0);
//                player.itemLocation.Y = player.Center.Y - 24 - origin.RotatedBy(player.itemRotation).Y * player.direction;
//                player.itemLocation.X = player.Center.X + (39 + origin.RotatedBy(player.itemRotation).X) * player.direction;
//            }
//            else if (player.direction == 1)
//            {
//                Vector2 origin = new Vector2(0, 0);
//                player.itemLocation.Y = player.Center.Y - 24 - origin.RotatedBy(player.itemRotation).Y * player.direction;
//                player.itemLocation.X = player.Center.X - (43 + origin.RotatedBy(player.itemRotation).X) * player.direction;
//            }
//        }
//        public override Vector2? HoldoutOffset()
//        {
//            return new Vector2(0, 0);
//        }
//        public override bool HoldItemFrame(Player player)
//        {
//            for (int j = 0; j < Main.maxNPCs; j++)
//            {
//                NPC target = Main.npc[j];
//                bool facingNPCBack = player.direction == target.direction;
//                if (!player.GetModPlayer<TFModPlayer>().hasBackstabTarget)
//                {
//                    player.bodyFrame.Y = player.bodyFrame.Height * 4;
//                }
//                else if (player.GetModPlayer<TFModPlayer>().hasBackstabTarget)
//                {
//                    player.bodyFrame.Y = player.bodyFrame.Height * 2;
//                }
//                if (target.active && target.CanBeChasedBy() && !target.friendly)
//                {
//                    float distToNPC = player.Distance(target.Center);
//                    if (distToNPC < 50f && facingNPCBack)
//                    {
//                        player.GetModPlayer<TFModPlayer>().hasBackstabTarget = true;
//                        player.bodyFrame.Y = player.bodyFrame.Height * 2;
//                    }
//                    else
//                    {
//                        player.GetModPlayer<TFModPlayer>().hasBackstabTarget = false;
//                    }
//                }
//            }
//            return true;
//        }
//        public override void UseStyle(Player player)
//        {
//            Vector2 origin = new Vector2(0, 0);
//            if (player.GetModPlayer<TFModPlayer>().hasBackstabTarget)
//            {
//                if (player.itemAnimation < player.itemAnimationMax)
//                {
//                    player.itemLocation.X = player.Center.X;
//                    player.itemLocation.Y = player.Center.Y;
//                }
//                if (player.itemAnimation < player.itemAnimationMax - 10)
//                {
//                    player.itemLocation.X = player.Center.X + 25;
//                    player.itemLocation.Y = player.Center.Y + 25;
//                }
//                if (player.itemAnimation < player.itemAnimationMax - 20)
//                {
//                    player.itemLocation.X = player.Center.X + 50;
//                    player.itemLocation.Y = player.Center.Y + 50;
//                }
//            }
//            if (!player.GetModPlayer<TFModPlayer>().hasBackstabTarget)
//            {
//                item.useStyle = ItemUseStyleID.SwingThrow;
//            }
//        }
//        public override void HoldItem(Player player)
//        {
//            Main.slimeRain = false;
//        }
//        public override bool UseItemFrame(Player player)
//        {
//            if (player.GetModPlayer<TFModPlayer>().hasBackstabTarget)
//            {
//                if (player.itemAnimation < player.itemAnimationMax - 10)
//                {
//                    Main.NewText(40);
//                    player.bodyFrame.Y = player.bodyFrame.Height * 3;
//                }
//                if (player.itemAnimation < player.itemAnimationMax - 20)
//                {
//                    Main.NewText(20);
//                    player.bodyFrame.Y = player.bodyFrame.Height * 4;
//                }
//            }
//            return true;
//        }
//        /*public override void SetConstantDefaults()
//        {
//            basicUseSound = "MeleeSwing";
//            meleeEntityCollisionSound = "AxeFleshHit" + Main.rand.Next(1, 4);
//            meleeWorldCollisionSound = "WorldMetalHit" + Main.rand.Next(1, 3);
//        }*/
//    }
//}