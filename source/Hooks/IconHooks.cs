global using static LBMergedMods.Hooks.IconHooks;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using HUD;
using Fisobs.Core;

namespace LBMergedMods.Hooks;

public static class IconHooks
{
    public static Color BalloonColor = new(.5f, .1f, .35f),
        PhysalisColor = new(198f / 255f, 110f / 255f, 0f),
        StationPlantCol = new(181f / 255f, 110f / 255f, 220f / 255f),
        AntherCol = new(171f / 255f, 56f / 255f, 27f / 255f),
        MarineCol = new(17f / 255f, 118f / 255f, 223f / 255f);

    internal static Color On_CreatureSymbol_ColorOfCreature(On.CreatureSymbol.orig_ColorOfCreature orig, IconSymbol.IconSymbolData iconData)
    {
        var res = orig(iconData);
        if (iconData.critType == CreatureTemplate.Type.TubeWorm && iconData.intData == M4R_DATA_NUMBER)
            res = Color.green;
        else if (iconData.critType == CreatureTemplate.Type.Hazer && iconData.intData == M4R_DATA_NUMBER)
            res = Color.white;
        return res;
    }

    internal static string On_CreatureSymbol_SpriteNameOfCreature(On.CreatureSymbol.orig_SpriteNameOfCreature orig, IconSymbol.IconSymbolData iconData)
    {
        var res = orig(iconData);
        if (iconData.critType == CreatureTemplate.Type.Fly && iconData.intData == M4R_DATA_NUMBER)
            res = "Kill_SeedBat";
        else if (iconData.critType == CreatureTemplate.Type.TubeWorm && iconData.intData == M4R_DATA_NUMBER)
            res = "Kill_Bigrub";
        return res;
    }

    internal static IconSymbol.IconSymbolData On_CreatureSymbol_SymbolDataFromCreature(On.CreatureSymbol.orig_SymbolDataFromCreature orig, AbstractCreature creature)
    {
        var res = orig(creature);
        if (creature.creatureTemplate.type == CreatureTemplate.Type.Fly && creature.IsSeed())
            res.intData = M4R_DATA_NUMBER;
        else if (creature.creatureTemplate.type == CreatureTemplate.Type.TubeWorm && creature.IsBig())
            res.intData = M4R_DATA_NUMBER;
        else if (creature.creatureTemplate.type == CreatureTemplate.Type.Hazer && Albino.TryGetValue(creature, out var props) && props.Value)
            res.intData = M4R_DATA_NUMBER;
        return res;
    }

    internal static Color On_ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        if (itemType == AbstractObjectType.ThornyStrawberry)
            return new(189f / 255f, 0f, 0f);
        if (itemType == AbstractObjectType.SporeProjectile)
            return new(.9f, 1f, .8f);
        if (itemType == AbstractObjectType.LittleBalloon)
            return Color.Lerp(BalloonColor, Color.white, .2f);
        if (itemType == AbstractObjectType.BouncingMelon)
            return new(34f / 255f, 130f / 255f, 44f / 255f);
        if (itemType == AbstractObjectType.Physalis)
            return PhysalisColor;
        if (itemType == AbstractObjectType.LimeMushroom)
            return new(179f / 255f, 1f, 0f);
        if (itemType == AbstractObjectType.RubberBlossom)
            return StationPlantCol;
        if (itemType == AbstractObjectType.GummyAnther)
            return AntherCol;
        if (itemType == AbstractObjectType.MarineEye)
            return MarineCol;
        if (itemType == AbstractObjectType.StarLemon)
            return new(1f, 210f / 255f, 0f);
        if (itemType == AbstractObjectType.DendriticNeuron)
            return Ext.MenuGrey;
        return orig(itemType, intData);
    }

    internal static string On_ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
    {
        if (itemType == AbstractObjectType.ThornyStrawberry)
            return "Symbol_ThornyStrawberry";
        if (itemType == AbstractObjectType.SporeProjectile)
            return "Symbol_SporeProjectile";
        if (itemType == AbstractObjectType.BlobPiece)
            return "Kill_WaterBlob";
        if (itemType == AbstractObjectType.LittleBalloon)
            return "Symbol_LBHBulb";
        if (itemType == AbstractObjectType.BouncingMelon)
            return "Symbol_BouncingMelon";
        if (itemType == AbstractObjectType.Physalis)
            return "Symbol_Physalis";
        if (itemType == AbstractObjectType.LimeMushroom)
            return "Symbol_LimeMushroom";
        if (itemType == AbstractObjectType.RubberBlossom)
            return "Symbol_BigStationPlant";
        if (itemType == AbstractObjectType.GummyAnther)
            return "Symbol_BigStationPlantFruit";
        if (itemType == AbstractObjectType.MarineEye)
            return "Symbol_MarineEye";
        if (itemType == AbstractObjectType.StarLemon)
            return "Symbol_StarLemon";
        if (itemType == AbstractObjectType.DendriticNeuron)
            return "Symbol_DendriticNeuron";
        return orig(itemType, intData);
    }

    internal static void IL_Map_Draw(ILContext il)
    {
        var c = new ILCursor(il);
        var loc = 0;
        var label = il.DefineLabel();
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(out loc),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("WhiteLizard"),
            x => x.MatchCall(out _),
            x => x.MatchBrfalse(out _)))
        {
            label.Target = c.Next;
            c.Index -= 5;
            var l = il.Body.Variables[loc];
            c.EmitDelegate((AbstractCreature crit) => crit.creatureTemplate.type == CreatureTemplateType.HunterSeeker);
            c.Emit(OpCodes.Brtrue, label)
             .Emit(OpCodes.Ldloc, l);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook HUD.Map.Draw! (part 1)");
        loc = 0;
        if (c.TryGotoNext(
            x => x.MatchLdloc(out loc),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("Overseer")))
        {
            ++c.Index;
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractCreature crit, Map self) =>
             {
                 if (crit.creatureTemplate.type == CreatureTemplateType.ThornBug && crit.superSizeMe)
                     self.creatureSymbols[self.creatureSymbols.Count - 1].myColor = new(0f, 133f / 255f, 250f / 255f);
                 else if (crit.creatureTemplate.type == CreatureTemplateType.FatFireFly && crit.superSizeMe)
                     self.creatureSymbols[self.creatureSymbols.Count - 1].myColor = new(33f / 255f, 155f / 255f, 217f / 255f);
                 else if (crit.creatureTemplate.type == CreatureTemplateType.NoodleEater && crit.superSizeMe)
                     self.creatureSymbols[self.creatureSymbols.Count - 1].myColor = new(138f / 255f, 245f / 255f, 0f);
                 else if (crit.creatureTemplate.type == CreatureTemplateType.HazerMom && crit.superSizeMe)
                     self.creatureSymbols[self.creatureSymbols.Count - 1].myColor = new(18f / 85f, .7921569f, 33f / 85f);
                 else if (crit.creatureTemplate.type == CreatureTemplateType.TintedBeetle && crit.superSizeMe)
                     self.creatureSymbols[self.creatureSymbols.Count - 1].myColor = new(80f / 255f, 167f / 255f, 233f / 255f);
                 else if (Big.TryGetValue(crit, out var props) && props.IsBig && crit.superSizeMe)
                     self.creatureSymbols[self.creatureSymbols.Count - 1].myColor = new(1f, .65f, .05f);
                 else if (Albino.TryGetValue(crit, out var props2) && props2.Value && crit.superSizeMe)
                     self.creatureSymbols[self.creatureSymbols.Count - 1].myColor = Color.white;
                 else if (Jelly.TryGetValue(crit, out var jelly) && jelly.IsJelly)
                 {
                     var symbol = self.creatureSymbols[self.creatureSymbols.Count - 1];
                     symbol.myColor = jelly.Color;
                     symbol.spriteName = "Kill_JellyLL";
                     symbol.shadowSprite2.element = symbol.shadowSprite1.element = symbol.symbolSprite.element = Futile.atlasManager.GetElementWithName("Kill_JellyLL");
                     symbol.graphWidth = symbol.symbolSprite.element.sourcePixelSize.x;
                     symbol.symbolSprite.scale = symbol.shadowSprite2.scale = symbol.shadowSprite1.scale = 1f + jelly.IconRadBonus;
                 }
             });
            c.Emit(OpCodes.Ldloc, il.Body.Variables[loc]);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt<IconSymbol>("Draw")))
            {
                c.Emit(OpCodes.Ldarg_0)
                 .Emit(OpCodes.Ldloc, il.Body.Variables[loc])
                 .EmitDelegate((Map self, AbstractCreature crit) =>
                 {
                     CreatureSymbol symbol;
                     if (Jelly.TryGetValue(crit, out var jelly) && jelly.IsJelly)
                     {
                         symbol = self.creatureSymbols[self.creatureSymbols.Count - 1];
                         var rs = 1f + jelly.IconRadBonus;
                         symbol.symbolSprite.scale *= rs;
                         symbol.shadowSprite2.scale *= rs;
                         symbol.shadowSprite1.scale *= rs;
                     }
                     else if (crit.realizedCreature is Denture dt && (!ModManager.MSC || !self.hud.rainWorld.safariMode))
                     {
                         symbol = self.creatureSymbols[self.creatureSymbols.Count - 1];
                         symbol.shadowSprite1.alpha = symbol.shadowSprite2.alpha = symbol.symbolSprite.alpha = Mathf.Lerp(symbol.symbolSprite.alpha, 0f, dt.JawOpen);
                     }
                 });
            }
            else
                LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook HUD.Map.Draw! (part 3)");
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook HUD.Map.Draw! (part 2)");
    }
}