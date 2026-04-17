using NoxusBoss.Content.NPCs.Bosses.Draedon;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace NoxusBoss.Content.Items.Debugging;

public class SummonMarsDebug : DebugItem
{
    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 36;
        Item.useAnimation = 40;
        Item.useTime = 40;
        Item.autoReuse = false;
        Item.noMelee = true;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = null;
        Item.rare = ItemRarityID.Expert;
        Item.value = 0;
        Item.consumable = false;
    }

    public override bool CanUseItem(Player player) =>
        !NPC.AnyNPCs(ModContent.NPCType<MarsBody>());

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
            return true;

        int npcType = ModContent.NPCType<MarsBody>();
        IEntitySource src = player.GetSource_ItemUse(Item);
        int x = (int)player.Center.X - 400;
        int y = (int)player.Center.Y;

        if (Main.netMode != NetmodeID.MultiplayerClient)
            NPC.NewNPC(src, x, y, npcType, 1);
        else
            NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: npcType);

        return true;
    }
}
