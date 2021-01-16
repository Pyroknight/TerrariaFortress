using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaFortress.Dusts;

namespace TerrariaFortress.Buffs
{
    public class Afterburn : ModBuff
    {
        int burnTimer = 31;
		public override void SetDefaults()
		{
			DisplayName.SetDefault("Afterburn");
			Description.SetDefault("You appear to have burst into flames.");
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

        public override void Update(Player player, ref int buffIndex)
        {
			if (Collision.WetCollision(player.position, player.width, player.height) && !player.lavaWet)
            {
				player.ClearBuff(ModContent.BuffType<Afterburn>());
				for (int i = 0; i < 10; i++)
				{
					Dust dust1 = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Smoke, 0f, -3f, 200, new Color(255, 255, 255), 4f);
					dust1.noGravity = true;
					dust1.noLight = true;
				}
			}

			// This is old code for my attempt to remove the constant red "1s" from DoT debuffs, and rather add white "4s" to pop up over their head every so often.
			// Very bad.

			//double HurtWithoutCombatText(PlayerDeathReason damageSource, int Damage, int hitDirection, bool pvp = false, bool quiet = false, bool Crit = false, int cooldownCounter = -1)
			//{
			//	bool flag = !player.immune;
			//	bool flag2 = false;
			//	int hitContext = cooldownCounter;
			//	if (cooldownCounter == 0)
			//	{
			//		flag = player.hurtCooldowns[cooldownCounter] <= 0;
			//	}
			//	if (cooldownCounter == 1)
			//	{
			//		flag = player.hurtCooldowns[cooldownCounter] <= 0;
			//	}
			//	if (cooldownCounter == 2)
			//	{
			//		flag2 = true;
			//		cooldownCounter = -1;
			//	}
			//	if (flag)
			//	{
			//		bool customDamage = false;
			//		bool playSound = true;
			//		bool genGore = true;
			//		if (!PlayerHooks.PreHurt(player, pvp, quiet, ref Damage, ref hitDirection, ref Crit, ref customDamage, ref playSound, ref genGore, ref damageSource))
			//		{
			//			return 0.0;
			//		}
			//		if (player.whoAmI == Main.myPlayer && player.panic)
			//		{
			//			player.AddBuff(63, 300);
			//		}
			//		if (player.whoAmI == Main.myPlayer && player.setSquireT2)
			//		{
			//			player.AddBuff(205, 300);
			//		}
			//		player.stealth = 1f;
			//		if (Main.netMode == 1)
			//		{
			//			NetMessage.SendData(84, -1, -1, null, player.whoAmI);
			//		}
			//		int num = 0;
			//		double num2 = (customDamage ? ((double)num) : Main.CalculatePlayerDamage(num, player.statDefense));
			//		if (Crit)
			//		{
			//			num *= 2;
			//		}
			//		if (num2 >= 1.0)
			//		{
			//			if (player.invis)
			//			{
			//				for (int i = 0; i < Main.maxBuffTypes; i++)
			//				{
			//					if (player.buffType[i] == 10)
			//					{
			//						player.DelBuff(i);
			//					}
			//				}
			//			}
			//			num2 = (int)((double)(1f - player.endurance) * num2);
			//			if (num2 < 1.0)
			//			{
			//				num2 = 1.0;
			//			}
			//			if (player.ConsumeSolarFlare())
			//			{
			//				float num3 = 0.3f;
			//				num2 = (int)((double)(1f - num3) * num2);
			//				if (num2 < 1.0)
			//				{
			//					num2 = 1.0;
			//				}
			//				if (player.whoAmI == Main.myPlayer)
			//				{
			//					int num4 = Projectile.NewProjectile(player.Center.X, player.Center.Y, 0f, 0f, 608, 150, 15f, Main.myPlayer);
			//					Main.projectile[num4].Kill();
			//				}
			//			}
			//			if (player.beetleDefense && player.beetleOrbs > 0)
			//			{
			//				float num5 = 0.15f * (float)player.beetleOrbs;
			//				num2 = (int)((double)(1f - num5) * num2);
			//				player.beetleOrbs--;
			//				for (int j = 0; j < Main.maxBuffTypes; j++)
			//				{
			//					if (player.buffType[j] >= 95 && player.buffType[j] <= 97)
			//					{
			//						player.DelBuff(j);
			//					}
			//				}
			//				if (player.beetleOrbs > 0)
			//				{
			//					player.AddBuff(95 + player.beetleOrbs - 1, 5, quiet: false);
			//				}
			//				player.beetleCounter = 0f;
			//				if (num2 < 1.0)
			//				{
			//					num2 = 1.0;
			//				}
			//			}
			//			if (player.magicCuffs)
			//			{
			//				int num6 = num;
			//				player.statMana += num6;
			//				if (player.statMana > player.statManaMax2)
			//				{
			//					player.statMana = player.statManaMax2;
			//				}
			//				player.ManaEffect(num6);
			//			}
			//			if (player.defendedByPaladin)
			//			{
			//				if (player.whoAmI != Main.myPlayer)
			//				{
			//					if (Main.player[Main.myPlayer].hasPaladinShield)
			//					{
			//						if (player.team == player.team && player.team != 0)
			//						{
			//							float num7 = player.Distance(player.Center);
			//							bool flag3 = num7 < 800f;
			//							if (flag3)
			//							{
			//								for (int k = 0; k < 255; k++)
			//								{
			//									if (k != Main.myPlayer && Main.player[k].active && !Main.player[k].dead && !Main.player[k].immune && Main.player[k].hasPaladinShield && Main.player[k].team == player.team && (float)Main.player[k].statLife > (float)Main.player[k].statLifeMax2 * 0.25f)
			//									{
			//										float num8 = Main.player[k].Distance(player.Center);
			//										if (num7 > num8 || (num7 == num8 && k < Main.myPlayer))
			//										{
			//											flag3 = false;
			//											break;
			//										}
			//									}
			//								}
			//							}
			//							if (flag3)
			//							{
			//								int damage = (int)(num2 * 0.25);
			//								num2 = (int)(num2 * 0.75);
			//								player.Hurt(PlayerDeathReason.LegacyEmpty(), damage, 0);
			//							}
			//						}
			//					}
			//				}
			//				else
			//				{
			//					bool flag4 = false;
			//					for (int l = 0; l < 255; l++)
			//					{
			//						if (l != Main.myPlayer && Main.player[l].active && !Main.player[l].dead && !Main.player[l].immune && Main.player[l].hasPaladinShield && Main.player[l].team == player.team && (float)Main.player[l].statLife > (float)Main.player[l].statLifeMax2 * 0.25f)
			//						{
			//							flag4 = true;
			//							break;
			//						}
			//					}
			//					if (flag4)
			//					{
			//						num2 = (int)(num2 * 0.75);
			//					}
			//				}
			//			}
			//			if (player.brainOfConfusion && Main.myPlayer == player.whoAmI)
			//			{
			//				for (int m = 0; m < 200; m++)
			//				{
			//					if (!Main.npc[m].active || Main.npc[m].friendly)
			//					{
			//						continue;
			//					}
			//					int num9 = 300;
			//					num9 += (int)num2 * 2;
			//					if (Main.rand.Next(500) < num9)
			//					{
			//						float num10 = (Main.npc[m].Center - player.Center).Length();
			//						float num11 = Main.rand.Next(200 + (int)num2 / 2, 301 + (int)num2 * 2);
			//						if (num11 > 500f)
			//						{
			//							num11 = 500f + (num11 - 500f) * 0.75f;
			//						}
			//						if (num11 > 700f)
			//						{
			//							num11 = 700f + (num11 - 700f) * 0.5f;
			//						}
			//						if (num11 > 900f)
			//						{
			//							num11 = 900f + (num11 - 900f) * 0.25f;
			//						}
			//						if (num10 < num11)
			//						{
			//							float num12 = Main.rand.Next(90 + (int)num2 / 3, 300 + (int)num2 / 2);
			//							Main.npc[m].AddBuff(31, (int)num12);
			//						}
			//					}
			//				}
			//				Projectile.NewProjectile(player.Center.X + (float)Main.rand.Next(-40, 40), player.Center.Y - (float)Main.rand.Next(20, 60), player.velocity.X * 0.3f, player.velocity.Y * 0.3f, 565, 0, 0f, player.whoAmI);
			//			}
			//			PlayerHooks.Hurt(player, pvp, quiet, num2, hitDirection, Crit);
			//			if (Main.netMode == 1 && player.whoAmI == Main.myPlayer && !quiet)
			//			{
			//				NetMessage.SendData(13, -1, -1, null, player.whoAmI);
			//				NetMessage.SendData(16, -1, -1, null, player.whoAmI);
			//				NetMessage.SendPlayerHurt(player.whoAmI, damageSource, Damage, hitDirection, Crit, pvp, hitContext);
			//			}
			//			Color color = (Crit ? CombatText.DamagedFriendlyCrit : CombatText.DamagedFriendly);
			//			player.statLife -= (int)num2;
			//			switch (cooldownCounter)
			//			{
			//				case -1:
			//					player.immune = true;
			//					if (num2 == 1.0)
			//					{
			//						player.immuneTime = 20;
			//						if (player.longInvince)
			//						{
			//							player.immuneTime += 20;
			//						}
			//					}
			//					else
			//					{
			//						player.immuneTime = 40;
			//						if (player.longInvince)
			//						{
			//							player.immuneTime += 40;
			//						}
			//					}
			//					if (pvp)
			//					{
			//						player.immuneTime = 8;
			//					}
			//					break;
			//				case 0:
			//					if (num2 == 1.0)
			//					{
			//						player.hurtCooldowns[cooldownCounter] = (player.longInvince ? 40 : 20);
			//					}
			//					else
			//					{
			//						player.hurtCooldowns[cooldownCounter] = (player.longInvince ? 80 : 40);
			//					}
			//					break;
			//				case 1:
			//					if (num2 == 1.0)
			//					{
			//						player.hurtCooldowns[cooldownCounter] = (player.longInvince ? 40 : 20);
			//					}
			//					else
			//					{
			//						player.hurtCooldowns[cooldownCounter] = (player.longInvince ? 80 : 40);
			//					}
			//					break;
			//			}
			//			player.lifeRegenTime = 0;
			//			if (player.whoAmI == Main.myPlayer)
			//			{
			//				if (player.starCloak)
			//				{
			//					for (int n = 0; n < 3; n++)
			//					{
			//						float x = player.position.X + (float)Main.rand.Next(-400, 400);
			//						float y = player.position.Y - (float)Main.rand.Next(500, 800);
			//						Vector2 vector = new Vector2(x, y);
			//						float num13 = player.position.X + (float)(player.width / 2) - vector.X;
			//						float num14 = player.position.Y + (float)(player.height / 2) - vector.Y;
			//						num13 += (float)Main.rand.Next(-100, 101);
			//						float num15 = (float)Math.Sqrt(num13 * num13 + num14 * num14);
			//						num15 = 23f / num15;
			//						num13 *= num15;
			//						num14 *= num15;
			//						int num16 = Projectile.NewProjectile(x, y, num13, num14, 92, 30, 5f, player.whoAmI);
			//						Main.projectile[num16].ai[1] = player.position.Y;
			//					}
			//				}
			//				if (player.bee)
			//				{
			//					int num17 = 1;
			//					if (Main.rand.Next(3) == 0)
			//					{
			//						num17++;
			//					}
			//					if (Main.rand.Next(3) == 0)
			//					{
			//						num17++;
			//					}
			//					if (player.strongBees && Main.rand.Next(3) == 0)
			//					{
			//						num17++;
			//					}
			//					for (int num18 = 0; num18 < num17; num18++)
			//					{
			//						float speedX = (float)Main.rand.Next(-35, 36) * 0.02f;
			//						float speedY = (float)Main.rand.Next(-35, 36) * 0.02f;
			//						Projectile.NewProjectile(player.position.X, player.position.Y, speedX, speedY, player.beeType(), player.beeDamage(7), player.beeKB(0f), Main.myPlayer);
			//					}
			//				}
			//			}
			//			if (flag2 && hitDirection != 0)
			//			{
			//				if (!player.mount.Active || !player.mount.Cart)
			//				{
			//					float num19 = 10.5f;
			//					float y2 = -7.5f;
			//					if (player.noKnockback)
			//					{
			//						num19 = 2.5f;
			//						y2 = -1.5f;
			//					}
			//					player.velocity.X = num19 * (float)hitDirection;
			//					player.velocity.Y = y2;
			//				}
			//			}
			//			else if (!player.noKnockback && hitDirection != 0 && (!player.mount.Active || !player.mount.Cart))
			//			{
			//				player.velocity.X = 4.5f * (float)hitDirection;
			//				player.velocity.Y = -3.5f;
			//			}
			//			if (playSound)
			//			{
			//				if (player.stoned)
			//				{
			//					Main.PlaySound(0, (int)player.position.X, (int)player.position.Y);
			//				}
			//				else if (player.frostArmor)
			//				{
			//					Main.PlaySound(SoundID.Item27, player.position);
			//				}
			//				else if ((player.wereWolf || player.forceWerewolf) && !player.hideWolf)
			//				{
			//					Main.PlaySound(3, (int)player.position.X, (int)player.position.Y, 6);
			//				}
			//				else if (player.boneArmor)
			//				{
			//					Main.PlaySound(3, (int)player.position.X, (int)player.position.Y, 2);
			//				}
			//				else if (!player.Male)
			//				{
			//					Main.PlaySound(20, (int)player.position.X, (int)player.position.Y);
			//				}
			//				else
			//				{
			//					Main.PlaySound(1, (int)player.position.X, (int)player.position.Y);
			//				}
			//			}
			//			if (player.statLife > 0)
			//			{
			//				if (genGore)
			//				{
			//					double num20 = num2 / (double)player.statLifeMax2 * 100.0;
			//					float num21 = 2 * hitDirection;
			//					float num22 = 0f;
			//					if (flag2)
			//					{
			//						num20 *= 12.0;
			//						num22 = 6f;
			//					}
			//					for (int num23 = 0; (double)num23 < num20; num23++)
			//					{
			//						if (player.stoned)
			//						{
			//							Dust.NewDust(player.position, player.width, player.height, 1, num21 + (float)hitDirection * num22 * Main.rand.NextFloat(), -2f);
			//						}
			//						else if (player.frostArmor)
			//						{
			//							int num24 = Dust.NewDust(player.position, player.width, player.height, 135, num21 + (float)hitDirection * num22 * Main.rand.NextFloat(), -2f);
			//							Main.dust[num24].shader = GameShaders.Armor.GetSecondaryShader(player.ArmorSetDye(), player);
			//						}
			//						else if (player.boneArmor)
			//						{
			//							int num25 = Dust.NewDust(player.position, player.width, player.height, 26, num21 + (float)hitDirection * num22 * Main.rand.NextFloat(), -2f);
			//							Main.dust[num25].shader = GameShaders.Armor.GetSecondaryShader(player.ArmorSetDye(), player);
			//						}
			//						else
			//						{
			//							Dust.NewDust(player.position, player.width, player.height, 5, num21 + (float)hitDirection * num22 * Main.rand.NextFloat(), -2f);
			//						}
			//					}
			//				}
			//				PlayerHooks.PostHurt(player, pvp, quiet, num2, hitDirection, Crit);
			//			}
			//			else
			//			{
			//				player.statLife = 0;
			//				if (player.whoAmI == Main.myPlayer)
			//				{
			//					player.KillMe(damageSource, num2, hitDirection, pvp);
			//				}
			//			}
			//		}
			//		if (pvp)
			//		{
			//			num2 = Main.CalculateDamage(num, player.statDefense);
			//		}
			//		return num2;
			//	}
			//	return 0.0;
			//}

			if (player.lifeRegen > 0)
            {
                player.lifeRegen = 0;
            }
            player.lifeRegenTime = 0;
			player.lifeRegen -= 16;

            if (++burnTimer > 30)
            {
                //HurtWithoutCombatText(PlayerDeathReason.ByCustomReason(player.name + " was melted by afterburn."), 4, 0, false, true, false, -1);
                burnTimer = 0;
            }

            for (int i = 0; i < 3; i++)
			{
				Dust dust1 = Dust.NewDustDirect(player.position - new Vector2(2f, 2f), player.width + 4, player.height + 4, ModContent.DustType<AfterburnFlames>(), player.velocity.X * -0.4f, player.velocity.Y + Main.rand.Next(-10, -1) * 0.1f, 100, default, (Main.rand.Next(15) + 1) * 0.1f);
				dust1.velocity *= 1.8f;
				dust1.velocity.Y -= 0.5f;
			}
			Dust dust2 = Dust.NewDustDirect(player.position - new Vector2(2f, 2f), player.width + 4, player.height + 4, DustID.Fire, player.velocity.X * 0.4f, player.velocity.Y + Main.rand.Next(-10, -1) * 0.1f, 100, default, 1f);
			dust2.velocity *= 1.8f;
			dust2.velocity.Y -= 0.5f;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
			if (Collision.WetCollision(npc.position, npc.width, npc.height) && !npc.lavaWet)
			{
				if (npc.HasBuff(ModContent.BuffType<Afterburn>()))
				{
					npc.DelBuff(npc.FindBuffIndex(ModContent.BuffType<Afterburn>()));
					for (int i = 0; i < 10; i++)
					{
						Dust dust1 = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Smoke, 0f, -3f, 200, new Color(255, 255, 255), 4f);
						dust1.noGravity = true;
						dust1.noLight = true;
					}
				}
			}

			//double StrikeNPCWithoutCombatText(int Damage, float knockBack, int hitDirection, bool crit = false, bool noEffect = false, bool fromNet = false)
			//{
			//	int ignorePlayerInteractions = 0;
			//	bool flag = Main.netMode == 0;
			//	if (flag)
			//	{
			//		ignorePlayerInteractions++;
			//	}
			//	if (flag && ignorePlayerInteractions > 0)
			//	{
			//		ignorePlayerInteractions--;
			//		flag = false;
			//	}
			//	if (!npc.active || npc.life <= 0)
			//	{
			//		return 0.0;
			//	}
			//	double num = Damage;
			//	int num2 = npc.defense;
			//	if (npc.ichor)
			//	{
			//		num2 -= 20;
			//	}
			//	if (npc.betsysCurse)
			//	{
			//		num2 -= 40;
			//	}
			//	if (num2 < 0)
			//	{
			//		num2 = 0;
			//	}
			//	if (NPCLoader.StrikeNPC(npc, ref num, num2, ref knockBack, hitDirection, ref crit))
			//	{
			//		num = 0;
			//		if (crit)
			//		{
			//			num *= 2.0;
			//		}
			//		if (npc.takenDamageMultiplier > 1f)
			//		{
			//			num *= (double)npc.takenDamageMultiplier;
			//		}
			//	}
			//	if (num >= 1.0)
			//	{
			//		if (flag)
			//		{
			//			npc.PlayerInteraction(Main.myPlayer);
			//		}
			//		npc.justHit = true;
			//		if (npc.townNPC)
			//		{
			//			if (npc.aiStyle == 7 && (npc.ai[0] == 3f || npc.ai[0] == 4f || npc.ai[0] == 16f || npc.ai[0] == 17f))
			//			{
			//				NPC nPC = Main.npc[(int)npc.ai[2]];
			//				if (nPC.active)
			//				{
			//					nPC.ai[0] = 1f;
			//					nPC.ai[1] = 300 + Main.rand.Next(300);
			//					nPC.ai[2] = 0f;
			//					nPC.localAI[3] = 0f;
			//					nPC.direction = hitDirection;
			//					nPC.netUpdate = true;
			//				}
			//			}
			//			npc.ai[0] = 1f;
			//			npc.ai[1] = 300 + Main.rand.Next(300);
			//			npc.ai[2] = 0f;
			//			npc.localAI[3] = 0f;
			//			npc.direction = hitDirection;
			//			npc.netUpdate = true;
			//		}
			//		if (npc.aiStyle == 8 && Main.netMode != 1)
			//		{
			//			if (npc.type == 172)
			//			{
			//				npc.ai[0] = 450f;
			//			}
			//			else if (npc.type == 283 || npc.type == 284)
			//			{
			//				if (Main.rand.Next(2) == 0)
			//				{
			//					npc.ai[0] = 390f;
			//					npc.netUpdate = true;
			//				}
			//			}
			//			else if (npc.type == 533)
			//			{
			//				if (Main.rand.Next(3) != 0)
			//				{
			//					npc.ai[0] = 181f;
			//					npc.netUpdate = true;
			//				}
			//			}
			//			else
			//			{
			//				npc.ai[0] = 400f;
			//			}
			//			npc.TargetClosest();
			//		}
			//		if (npc.aiStyle == 97 && Main.netMode != 1)
			//		{
			//			npc.localAI[1] = 1f;
			//			npc.TargetClosest();
			//		}
			//		if (npc.type == 371)
			//		{
			//			num = 0.0;
			//			npc.ai[0] = 1f;
			//			npc.ai[1] = 4f;
			//			npc.dontTakeDamage = true;
			//		}
			//		if (npc.type == 346 && (double)npc.life >= (double)npc.lifeMax * 0.5 && (double)npc.life - num < (double)npc.lifeMax * 0.5)
			//		{
			//			Gore.NewGore(npc.position, npc.velocity, 517);
			//		}
			//		if (npc.type == 184)
			//		{
			//			npc.localAI[0] = 60f;
			//		}
			//		if (npc.type == 535)
			//		{
			//			npc.localAI[0] = 60f;
			//		}
			//		if (npc.type == 185)
			//		{
			//			npc.localAI[0] = 1f;
			//		}
			//		if (!npc.immortal)
			//		{
			//			if (npc.realLife >= 0)
			//			{
			//				Main.npc[npc.realLife].life -= (int)num;
			//				npc.life = Main.npc[npc.realLife].life;
			//				npc.lifeMax = Main.npc[npc.realLife].lifeMax;
			//			}
			//			else
			//			{
			//				npc.life -= (int)num;
			//			}
			//		}
			//		if (knockBack > 0f && npc.knockBackResist > 0f)
			//		{
			//			float num3 = knockBack * npc.knockBackResist;
			//			if (num3 > 8f)
			//			{
			//				float num4 = num3 - 8f;
			//				num4 *= 0.9f;
			//				num3 = 8f + num4;
			//			}
			//			if (num3 > 10f)
			//			{
			//				float num5 = num3 - 10f;
			//				num5 *= 0.8f;
			//				num3 = 10f + num5;
			//			}
			//			if (num3 > 12f)
			//			{
			//				float num6 = num3 - 12f;
			//				num6 *= 0.7f;
			//				num3 = 12f + num6;
			//			}
			//			if (num3 > 14f)
			//			{
			//				float num7 = num3 - 14f;
			//				num7 *= 0.6f;
			//				num3 = 14f + num7;
			//			}
			//			if (num3 > 16f)
			//			{
			//				num3 = 16f;
			//			}
			//			if (crit)
			//			{
			//				num3 *= 1.4f;
			//			}
			//			int num8 = (int)num * 10;
			//			if (Main.expertMode)
			//			{
			//				num8 = (int)num * 15;
			//			}
			//			if (num8 > npc.lifeMax)
			//			{
			//				if (hitDirection < 0 && npc.velocity.X > 0f - num3)
			//				{
			//					if (npc.velocity.X > 0f)
			//					{
			//						npc.velocity.X -= num3;
			//					}
			//					npc.velocity.X -= num3;
			//					if (npc.velocity.X < 0f - num3)
			//					{
			//						npc.velocity.X = 0f - num3;
			//					}
			//				}
			//				else if (hitDirection > 0 && npc.velocity.X < num3)
			//				{
			//					if (npc.velocity.X < 0f)
			//					{
			//						npc.velocity.X += num3;
			//					}
			//					npc.velocity.X += num3;
			//					if (npc.velocity.X > num3)
			//					{
			//						npc.velocity.X = num3;
			//					}
			//				}
			//				if (npc.type == 185)
			//				{
			//					num3 *= 1.5f;
			//				}
			//				num3 = (npc.noGravity ? (num3 * -0.5f) : (num3 * -0.75f));
			//				if (npc.velocity.Y > num3)
			//				{
			//					npc.velocity.Y += num3;
			//					if (npc.velocity.Y < num3)
			//					{
			//						npc.velocity.Y = num3;
			//					}
			//				}
			//			}
			//			else
			//			{
			//				if (!npc.noGravity)
			//				{
			//					npc.velocity.Y = (0f - num3) * 0.75f * npc.knockBackResist;
			//				}
			//				else
			//				{
			//					npc.velocity.Y = (0f - num3) * 0.5f * npc.knockBackResist;
			//				}
			//				npc.velocity.X = num3 * (float)hitDirection * npc.knockBackResist;
			//			}
			//		}
			//		if ((npc.type == 113 || npc.type == 114) && npc.life <= 0)
			//		{
			//			for (int i = 0; i < 200; i++)
			//			{
			//				if (Main.npc[i].active && (Main.npc[i].type == 113 || Main.npc[i].type == 114))
			//				{
			//					Main.npc[i].HitEffect(hitDirection, num);
			//				}
			//			}
			//		}
			//		else
			//		{
			//			npc.HitEffect(hitDirection, num);
			//		}
			//		if (npc.HitSound != null)
			//		{
			//			Main.PlaySound(npc.HitSound, npc.position);
			//		}
			//		if (npc.realLife >= 0)
			//		{
			//			Main.npc[npc.realLife].checkDead();
			//		}
			//		else
			//		{
			//			npc.checkDead();
			//		}
			//		return num;
			//	}
			//	return 0.0;
			//}

			if (!npc.onFire)
			{
				if (npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 16;
			}

            if (++burnTimer > 30)
            {
                burnTimer = 0;
            }

			for (int i = 0; i < 3; i++)
			{
				Dust dust1 = Dust.NewDustDirect(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, ModContent.DustType<AfterburnFlames>(), npc.velocity.X * -0.4f, npc.velocity.Y + Main.rand.Next(-10, -1) * 0.1f, 100, default, (Main.rand.Next(15) + 1) * 0.1f);
				dust1.velocity *= 1.8f;
				dust1.velocity.Y -= 0.5f;
			}
			Dust dust2 = Dust.NewDustDirect(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, DustID.Fire, npc.velocity.X * 0.4f, npc.velocity.Y + Main.rand.Next(-10, -1) * 0.1f, 100, default, 1f);
			dust2.velocity *= 1.8f;
			dust2.velocity.Y -= 0.5f;
		}
    }
}