global using static LBMergedMods.Hooks.DeerHooks;

namespace LBMergedMods.Hooks;

public static class DeerHooks
{
    internal static void On_DeerAI_NewRoom(On.DeerAI.orig_NewRoom orig, DeerAI self, Room newRoom)
    {
        orig(self, newRoom);
        var ents = newRoom.abstractRoom.entities;
        for (var j = 0; j < ents.Count; j++)
        {
            if (ents[j] is AbstractPhysicalObject obj && obj.realizedObject is SmallPuffBall)
                self.itemTracker.SeeItem(obj);
        }
    }

    internal static bool On_DeerAI_TrackItem(On.DeerAI.orig_TrackItem orig, DeerAI self, AbstractPhysicalObject obj) => orig(self, obj) || obj.type == AbstractObjectType.SporeProjectile;
}