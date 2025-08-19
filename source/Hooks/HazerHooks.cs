global using static LBMergedMods.Hooks.HazerHooks;
using Random = UnityEngine.Random;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class HazerHooks
{
    internal static void On_Hazer_Collide(On.Hazer.orig_Collide orig, Hazer self, PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        if (otherObject is not HazerMom)
            orig(self, otherObject, myChunk, otherChunk);
    }

    internal static void On_Hazer_Update(On.Hazer.orig_Update orig, Hazer self, bool eu)
    {
        var nds = self.abstractPhysicalObject.Room.nodes;
        if ((!self.abstractPhysicalObject.pos.NodeDefined || self.abstractPhysicalObject.pos.abstractNode >= nds.Length) && nds.Length > 0)
            self.abstractPhysicalObject.pos.abstractNode = Random.Range(0, nds.Length);
        orig(self, eu);
    }

    internal static void On_HazerGraphics_ctor(On.HazerGraphics.orig_ctor orig, HazerGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (self.bug.abstractCreature.Albino())
        {
            self.skinColor = HSLColor.Lerp(new((Random.value < .5f ? .348f : .56f) + Mathf.Lerp(-.03f, .03f, Random.value), .6f + Random.value * .1f, .7f + Random.value * .1f), HazerMomGraphics.WhiteCol, .85f);
            self.secondColor = HSLColor.Lerp(new(self.skinColor.hue + Mathf.Lerp(-.1f, .1f, Random.value), Mathf.Lerp(self.skinColor.saturation, 1f, Random.value), self.skinColor.lightness - Random.value * .4f), HazerMomGraphics.RedCol, .9f);
            self.eyeColor = HSLColor.Lerp(new((self.skinColor.hue + self.secondColor.hue) * .5f + .5f, 1f, .4f + Random.value * .1f), HazerMomGraphics.RedCol, .9f);
        }
    }

    internal static void IL_HazerGraphics_ApplyPalette(ILContext il)
    {
        var c = new ILCursor(il);
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            if (instrs[i].MatchLdfld<RoomPalette>("blackColor"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((Color blackColor, HazerGraphics self) => self.bug is Hazer h && h.abstractCreature.Albino() ? Color.Lerp(blackColor, new(.87f, .87f, .87f), .35f) : blackColor);
            }
        }
    }

    public static bool Camouflaged(this Hazer self) => self.graphicsModule is HazerGraphics hgr && hgr.camo >= .8f;
}