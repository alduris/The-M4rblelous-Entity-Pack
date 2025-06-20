global using static LBMergedMods.Hooks.TentaclePlantHooks;
using RWCustom;
using UnityEngine;
using Watcher;

namespace LBMergedMods.Hooks;

public static class TentaclePlantHooks
{
    internal static void On_TentaclePlant_Collide(On.TentaclePlant.orig_Collide orig, TentaclePlant self, PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        if (self.abstractCreature.RottenMode() && otherObject is DaddyLongLegs || (ModManager.Watcher && ((otherObject is Loach l && l.Rotted) || otherObject is Rattler)))
            return;
        orig(self, otherObject, myChunk, otherChunk);
    }

    internal static void On_TentaclePlant_Update(On.TentaclePlant.orig_Update orig, TentaclePlant self, bool eu)
    {
        if (self.abstractCreature.RottenMode())
            self.abstractCreature.tentacleImmune = true;
        orig(self, eu);
    }

    internal static CreatureTemplate.Relationship On_TentaclePlantAI_UpdateDynamicRelationship(On.TentaclePlantAI.orig_UpdateDynamicRelationship orig, TentaclePlantAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        if (self.creature.RottenMode() && dRelation.trackerRep?.representedCreature?.creatureTemplate is CreatureTemplate tp)
        {
            if (tp.TopAncestor().type == CreatureTemplate.Type.DaddyLongLegs || (ModManager.Watcher && (tp.type == WatcherEnums.CreatureTemplateType.Rattler)))
                return new(CreatureTemplate.Relationship.Type.Ignores, 0f);
            if (ModManager.Watcher && (tp.type == WatcherEnums.CreatureTemplateType.RotLoach))
                return new(CreatureTemplate.Relationship.Type.Afraid, 1f);
        }
        return orig(self, dRelation);
    }

    internal static void On_TentaclePlantGraphics_InitiateSprites(On.TentaclePlantGraphics.orig_InitiateSprites orig, TentaclePlantGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.plant?.abstractCreature is AbstractCreature cr && cr.RottenMode())
        {
            var sprites = sLeaser.sprites;
            var sh1 = Custom.rainWorld.Shaders["TentaclePlant"];
            var sh2 = Custom.rainWorld.Shaders["LBRottenTentaclePlant"];
            for (var i = 0; i < sprites.Length; i++)
            {
                var spr = sprites[i];
                if (spr.shader == sh1)
                {
                    spr.alpha = 1f;
                    spr.shader = sh2;
                }
            }
        }
    }

    internal static void On_TentaclePlantGraphics_ApplyPalette(On.TentaclePlantGraphics.orig_ApplyPalette orig, TentaclePlantGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.plant is TentaclePlant tl && tl.abstractCreature is AbstractCreature cr && cr.RottenMode())
        {
            var danglers = self.danglers;
            Color color;
            if (rCam.room is Room rm)
            {
                if (DaddyCorruption.SentientRotMode(rm))
                    color = RainWorld.RippleColor;
                else if (rm.world.region?.regionParams.corruptionEffectColor is Color clr)
                    color = clr;
                else
                    color = Color.blue;
            }
            else
                color = Color.blue;
            var l = danglers.Length;
            for (var i = 0; i < l; i++)
                sLeaser.sprites[i + 1].color = color;
            Shader.SetGlobalColor(_LBCustom_RottenTentacleBlack, palette.blackColor);
        }
    }
}
