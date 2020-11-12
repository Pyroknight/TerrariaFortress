using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Utilities;
using Terraria.Localization;
using System.Collections.Generic;
using System;
using TerrariaFortress.Items.Weapons;
using TerrariaFortress.Items;

namespace TerrariaFortress.NPCs
{
    [AutoloadHead]
    public class MsPauling : ModNPC
    {
        int frame = 2;
        public override bool Autoload(ref string name)
        {
            return mod.Properties.Autoload;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[npc.type] = 22;
            NPCID.Sets.AttackFrameCount[npc.type] = 1;
        }

        public override void SetDefaults()
        {
            npc.townNPC = true;
            npc.friendly = true;
            npc.width = 18;
            npc.height = 52;
            npc.aiStyle = 7;
            npc.damage = 10;
            npc.defense = 15;
            npc.lifeMax = 250;  
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.knockBackResist = 0.5f;
        }

        public override bool CanTownNPCSpawn(int numTownNPCs, int money)
        {
            return false;
        }

        public override string TownNPCName()
        {
            return "Ms. Pauling";
        }
        public override string GetChat()
        {
            WeightedRandom<string> chat = new WeightedRandom<string>();

            //#region contract uncompleted
            //chat.Add("Hey, Pauling here. Could use some help with these.");
            //chat.Add("Hi, I need a task done, you up for it?");
            //chat.Add("I could use a hand, wanna have at this contract?");
            //chat.Add("Take a look at this job.");
            //chat.Add("I need a task done. Want the details?");
            //chat.Add("You wanted dialogue? I'm too busy for that, but I can give you a contract.");
            //#endregion

            //#region contract turned in
            //chat.Add("Nice work! Thanks for your help.");
            //chat.Add("Great work on this contract.");
            //chat.Add("You did it! I appreciate it.");
            //chat.Add("I owe you one for this contract.");
            //chat.Add("Knew I could count on you, solid work.");
            //chat.Add("That'll do, thank you!");
            //chat.Add("Your results prove well, thanks for the help.");
            //chat.Add("Well done, I'll stay in touch with you.");
            //chat.Add("Your contract has been fulfilled!");
            //#endregion

            //#region contract already done
            //chat.Add("Sorry, these aren't for fun.");
            //chat.Add("I'd give you more if I could, no dice.");
            //chat.Add("I'm busy, but you can come back for more soon.");
            //chat.Add("Good news, you're set for today.");
            //chat.Add("Beeeep. You have reached Ms. Pauling. She is probably killing someone.");
            //chat.Add("I've got a phone call due. Come back in some time.");
            //#endregion

            #region normal conversation
            chat.Add("I prefer doing jobs in the third dimension.");
            chat.Add("My first name? Can't remember after ending up in Terraria.");
            chat.Add("Have you seen my mo-ped?");
            chat.Add("If you see a handsome guy sporting red and a cap, don't tell him I'm here!");
            chat.Add("I'm no good at jokes, last time I tried them I contract-ed an illness.");
            #endregion

            if (!Main.dayTime)
            {
                chat.Add("It's never too late to do a contract, you know.");
                chat.Add("Monsters? Fighting? Sounds fitting for this job I got you.");
            }

            if (Main.bloodMoon)
            {
                chat.Add("Back where I'm from, we have blood money, not blood moons.");
                chat.Add("I'm already too cranky over contracts to care about some hemopossesion.");
            }

            if (Main.eclipse)
            {
                chat.Add("Maybe Scream Fortress doesn't come to an end.");
            }
            return chat;
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28");
            button2 = "Contracts";
        }

        private void SetFrame(int frame)
        {
            npc.frame.Y = frame * 80;
        }

        public override void FindFrame(int frameHeight)
        {
            npc.spriteDirection = npc.direction;

            if (npc.velocity.Y != 0)
            {
                SetFrame(1);
            }

            if (npc.ai[0] == 1 && npc.velocity.X != 0 && npc.velocity.Y == 0)
            {
                npc.frameCounter += Math.Abs(npc.velocity.X) * 2f + 1;
                if (npc.frameCounter > 5)
                {
                    npc.frameCounter = 0;
                    frame++;
                    if (frame > 15)
                    {
                        frame = 2;
                    }
                    SetFrame(frame);
                }
            }
            else
            {
                npc.frameCounter = 0;
                frame = 2;
            }

            if (npc.velocity == new Vector2(0f, 0f))
            {
                npc.frameCounter = 0;
                SetFrame(0);
            }
        }
    }

    public class TemporaryFlamethrowerDeal : GlobalNPC
    {
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (type == NPCID.Demolitionist)
            {
                if (NPC.downedBoss3)
                {
                    shop.item[nextSlot].SetDefaults(ModContent.ItemType<Flamethrower>());
                    nextSlot++;
                    shop.item[nextSlot].SetDefaults(ModContent.ItemType<Shotgun>());
                    nextSlot++;
                    shop.item[nextSlot].SetDefaults(ModContent.ItemType<FireAxe>());
                    nextSlot++;
                    shop.item[nextSlot].SetDefaults(ModContent.ItemType<AmmoBox>());
                    nextSlot++;
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