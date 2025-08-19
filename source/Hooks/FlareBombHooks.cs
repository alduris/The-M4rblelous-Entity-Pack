global using static LBMergedMods.Hooks.FlareBombHooks;
using Random = UnityEngine.Random;
using RWCustom;

namespace LBMergedMods.Hooks;

public static class FlareBombHooks
{
    internal static void On_FlareBomb_Update(On.FlareBomb.orig_Update orig, FlareBomb self, bool eu)
    {
        orig(self, eu);
        if (self.burning > 0f)
        {
            var crits = self.room.abstractRoom.creatures;
            for (var i = 0; i < crits.Count; i++)
            {
                if (crits[i] is AbstractCreature acr && acr.realizedCreature is MiniLeech l && acr.SameRippleLayer(self.abstractPhysicalObject) && !l.dead && Custom.DistLess(self.firstChunk.pos, l.firstChunk.pos, self.LightIntensity * 600f) && self.room.VisualContact(self.firstChunk.pos, l.firstChunk.pos))
                {
                    l.airDrown = 1f;
                    l.Die();
                    l.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 7f;
                }
            }
        }
    }
}