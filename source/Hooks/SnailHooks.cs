global using static LBMergedMods.Hooks.SnailHooks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Hooks;

public static class SnailHooks
{
    internal static void IL_Snail_Click(ILContext il)
    {
        var loc = 0;
        var c = new ILCursor(il);
        ILLabel? beq = null;
        if (c.TryGotoNext(
            x => x.MatchLdloc(out loc),
            x => x.MatchLdarg(0),
            x => x.MatchBeq(out beq))
        && beq is not null)
        {
            c.Emit(OpCodes.Ldloc, il.Body.Variables[loc])
             .EmitDelegate((PhysicalObject self) => self is Lizard l && (l.IsPolliwog() || l.IsWaterSpitter()));
            c.Emit(OpCodes.Brtrue, beq);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Snail.Click!");
    }

    internal static void On_Snail_Click(On.Snail.orig_Click orig, Snail self)
    {
        if (self is BouncingBall b && (self.dead || b.NarrowSpace()))
            return;
        orig(self);
        if (self is BouncingBall && self.room is Room rm)
        {
            self.stun -= self.safariControlled ? 35 : 50;
            var vector = self.mainBodyChunk.pos;
            var color = self.shellColor[1];
            rm.AddObject(new Explosion.ExplosionLight(vector, 160f, 1f, 3, color));
            rm.AddObject(new ExplosionSpikes(rm, vector, 9, 4f, 5f, 5f, 90f, color));
            for (var j = 0; j < 20; j++)
            {
                var vector2 = Custom.RNV();
                rm.AddObject(new Spark(vector + vector2 * Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, Random.value), color, null, 4, 18));
            }
            var ar = self.bodyChunks;
            for (var i = 0; i < ar.Length; i++)
                ar[i].vel *= 1.25f;
            var chunk1 = ar[1];
            var num = 60f * self.size;
            var phO = rm.physicalObjects;
            for (var m = 0; m < phO.Length; m++)
            {
                var phOm = phO[m];
                for (var m2 = 0; m2 < phOm.Count; m2++)
                {
                    if (phOm[m2] is not Creature c || c is Snail or Overseer)
                        continue;
                    var flag = false;
                    var array = c.bodyChunks;
                    for (var i = 0; i < array.Length; i++)
                    {
                        var bodyChunk = array[i];
                        if (!Custom.DistLess(bodyChunk.pos, chunk1.pos, num * (1f + bodyChunk.submersion * chunk1.submersion * 4.5f) + bodyChunk.rad + chunk1.rad) || !rm.VisualContact(bodyChunk.pos, chunk1.pos))
                            continue;
                        flag = true;
                    }
                    if (flag)
                    {
                        var mc = c.mainBodyChunk;
                        c.Violence(chunk1, Custom.DirVec(chunk1.pos, mc.pos) * 4f, mc, null, Creature.DamageType.Explosion, .08f, 0f);
                    }
                }
            }
        }
    }

    internal static void On_Snail_VibrateLeeches(On.Snail.orig_VibrateLeeches orig, Snail self, float rad)
    {
        orig(self, rad);
        if (self.bodyChunks[1].submersion <= .5f)
            return;
        var crits = self.room.abstractRoom.creatures;
        for (var i = 0; i < crits.Count; i++)
        {
            if (crits[i].realizedCreature is MiniLeech l && l.room == self.room && Custom.DistLess(self.mainBodyChunk.pos, l.mainBodyChunk.pos, rad) && (Custom.DistLess(self.mainBodyChunk.pos, l.mainBodyChunk.pos, rad / 4f) || self.room.VisualContact(self.mainBodyChunk.pos, l.mainBodyChunk.pos)))
                l.HeardSnailClick(self.mainBodyChunk.pos);
        }
    }

    internal static void IL_SnailAI_CreatureUnease(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<CreatureTemplate.Type>("Leech"),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((bool flag, AbstractCreature crit) => flag || crit.creatureTemplate.type == CreatureTemplateType.MiniBlackLeech);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Leech.Swim! (part 2)");
    }

    internal static void IL_SnailAI_TileIdleScore(ILContext il)
    {
        var c = new ILCursor(il);
        MethodReference? ref1 = null;
        int loc = 0, loc2 = 0;
        if (c.TryGotoNext(
            x => x.MatchLdloc(out loc),
            x => x.MatchLdcR4(1000f),
            x => x.MatchSub(),
            x => x.MatchStloc(loc))
         && c.TryGotoNext(
            x => x.MatchLdloc(out loc2),
            x => x.MatchLdfld<Tracker.CreatureRepresentation>("representedCreature"),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("Snail"),
            x => x.MatchCall(out ref1),
            x => x.MatchBrfalse(out _)))
        {
            ++c.Index;
            var local = il.Body.Variables[loc];
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_1)
             .Emit(OpCodes.Ldloc, local)
             .EmitDelegate((Tracker.CreatureRepresentation rep, SnailAI self, WorldCoordinate pos, float num) =>
             {
                 if (rep.representedCreature?.creatureTemplate.type == CreatureTemplateType.BouncingBall && rep.representedCreature != self.creature && Custom.ManhattanDistance(pos, rep.BestGuessForPosition()) < 1)
                     num -= 20f;
                 return num;
             });
            c.Emit(OpCodes.Stloc, local)
             .Emit(OpCodes.Ldloc, il.Body.Variables[loc2]);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook SnailAI.TileIdleScore! (part 1)");
        c.Index = 0;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(loc2),
            x => x.MatchLdfld<Tracker.CreatureRepresentation>("representedCreature"),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("Leech"),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldloc, il.Body.Variables[loc2])
             .EmitDelegate((bool flag, Tracker.CreatureRepresentation rep) => flag || rep.representedCreature.creatureTemplate.type == CreatureTemplateType.MiniBlackLeech);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook SnailAI.TileIdleScore! (part 2)");
    }

    internal static float On_SnailAI_TileIdleScore(On.SnailAI.orig_TileIdleScore orig, SnailAI self, WorldCoordinate pos) => self.snail is BouncingBall s && s.NarrowSpace() ? float.MinValue : orig(self, pos);
}