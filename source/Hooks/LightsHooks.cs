global using static LBMergedMods.Hooks.LightsHooks;
using MonoMod.Cil;
using RWCustom;

namespace LBMergedMods.Hooks;
//CHK
public static class LightsHooks
{
    internal static void On_LightSource_InitiateSprites(On.LightSource.orig_InitiateSprites orig, LightSource self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.tiedToObject is StarLemon && self.flat)
            sLeaser.sprites[0].shader = Custom.rainWorld.Shaders["FlatLightBehindTerrain"];
    }

    internal static void IL_MeltLights_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchIsinst_Fly))
            c.EmitDelegate((Fly fly) => (fly?.room?.world?.name == "NP" && fly.IsSeed()) ? null : fly);
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook MeltLights.Update!");
    }
}