global using static LBMergedMods.Hooks.WorldHooks;

namespace LBMergedMods.Hooks;
//CHK
public static class WorldHooks
{
    internal static AbstractRoomNode On_World_GetNode(On.World.orig_GetNode orig, World self, WorldCoordinate c)
    {
        if (c.abstractNode < 0 || self.GetAbstractRoom(c.room)?.nodes is not AbstractRoomNode[] nds || c.abstractNode >= nds.Length)
            return new(new("UnregisteredNodeType"), 0, 0, false, 0, 0);
        return orig(self, c);
    }

    internal static int On_World_TotalShortCutLengthBetweenTwoConnectedRooms_AbstractRoom_AbstractRoom(On.World.orig_TotalShortCutLengthBetweenTwoConnectedRooms_AbstractRoom_AbstractRoom orig, World self, AbstractRoom room1, AbstractRoom room2)
    {
        if (room1?.nodes is AbstractRoomNode[] nds1 && room2?.nodes is AbstractRoomNode[] nds2 && (room1.ExitIndex(room2.index) >= nds1.Length || room2.ExitIndex(room1.index) >= nds2.Length))
            return -1;
        return orig(self, room1, room2);
    }
}