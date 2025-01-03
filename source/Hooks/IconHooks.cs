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
        var tp = iconData.critType;
        var dt = iconData.intData;
        if (dt == M4R_DATA_NUMBER)
        {
            if (tp == CreatureTemplate.Type.TubeWorm)
                return Color.green;
            if (tp == CreatureTemplate.Type.Hazer || tp == CreatureTemplateType.Denture || tp == CreatureTemplateType.Glowpillar)
                return Color.white;
            if (tp == CreatureTemplateType.ThornBug)
                return new(0f, 133f / 255f, 250f / 255f);
            if (tp == CreatureTemplateType.FatFireFly)
                return new(33f / 255f, 155f / 255f, 217f / 255f);
            if (tp == CreatureTemplateType.NoodleEater)
                return new(138f / 255f, 245f / 255f, 0f);
            if (tp == CreatureTemplateType.HazerMom)
                return new(18f / 85f, .7921569f, 33f / 85f);
            if (tp == CreatureTemplateType.TintedBeetle)
                return new(80f / 255f, 167f / 255f, 233f / 255f);
            if (tp == CreatureTemplateType.CommonEel)
                return new(0f, 72f / 255f, 1f);
        }
        else if (dt == M4R_DATA_NUMBER2 && tp == CreatureTemplate.Type.TubeWorm)
            return new(1f, .65f, .05f);
        else if (dt == M4R_DATA_NUMBER3 && tp == CreatureTemplate.Type.TubeWorm)
            return new(.05f, .3f, .7f);
        return orig(iconData);
    }

    internal static string On_CreatureSymbol_SpriteNameOfCreature(On.CreatureSymbol.orig_SpriteNameOfCreature orig, IconSymbol.IconSymbolData iconData)
    {
        var tp = iconData.critType;
        var dt = iconData.intData;
        if (dt == M4R_DATA_NUMBER && tp == CreatureTemplate.Type.Fly)
            return "Kill_SeedBat";
        else if (dt is M4R_DATA_NUMBER or M4R_DATA_NUMBER2 or M4R_DATA_NUMBER3 && tp == CreatureTemplate.Type.TubeWorm)
            return "Kill_Bigrub";
        return orig(iconData);
    }

    internal static IconSymbol.IconSymbolData On_CreatureSymbol_SymbolDataFromCreature(On.CreatureSymbol.orig_SymbolDataFromCreature orig, AbstractCreature creature)
    {
        var res = orig(creature);
        var tp = creature.creatureTemplate.type;
        if ((tp == CreatureTemplate.Type.Fly && creature.IsSeed()) ||
            ((tp == CreatureTemplate.Type.Hazer || tp == CreatureTemplateType.Denture || tp == CreatureTemplateType.Glowpillar) && Albino.TryGetValue(creature, out var props) && props.Value) ||
            ((tp == CreatureTemplateType.ThornBug || tp == CreatureTemplateType.FatFireFly || tp == CreatureTemplateType.NoodleEater || tp == CreatureTemplateType.CommonEel || tp == CreatureTemplateType.HazerMom || tp == CreatureTemplateType.TintedBeetle) && creature.superSizeMe))
            res.intData = M4R_DATA_NUMBER;
        else if (tp == CreatureTemplate.Type.TubeWorm && creature.IsBig(out var prop))
            res.intData = prop.NormalLook ? M4R_DATA_NUMBER3 : (creature.superSizeMe ? M4R_DATA_NUMBER2 : M4R_DATA_NUMBER);
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
        if (itemType == AbstractObjectType.MiniBlueFruit)
            return Color.blue;
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
        if (itemType == AbstractObjectType.MiniBlueFruit)
            return "Symbol_DangleFruit"; //tochange
        return orig(itemType, intData);
    }

    internal static void IL_Map_Draw(ILContext il)
    {
        var vars = il.Body.Variables;
        var c = new ILCursor(il);
        var label = il.DefineLabel();
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_WhiteLizard,
            s_MatchCall_Any,
            s_MatchBrfalse_Any))
        {
            label.Target = c.Next;
            c.Index -= 5;
            var l = vars[s_loc1];
            c.EmitDelegate((AbstractCreature crit) => crit.creatureTemplate.type == CreatureTemplateType.HunterSeeker);
            c.Emit(OpCodes.Brtrue, label)
             .Emit(OpCodes.Ldloc, l);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook HUD.Map.Draw! (part 1)");
        if (c.TryGotoNext(
            s_MatchLdloc_OutLoc1,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Overseer))
        {
            ++c.Index;
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractCreature crit, Map self) =>
             {
                 if (Jelly.TryGetValue(crit, out var jelly) && jelly.IsJelly)
                 {
                     var symbol = self.creatureSymbols[self.creatureSymbols.Count - 1];
                     symbol.myColor = jelly.Color;
                     symbol.spriteName = "Kill_JellyLL";
                     symbol.shadowSprite2.element = symbol.shadowSprite1.element = symbol.symbolSprite.element = Futile.atlasManager.GetElementWithName("Kill_JellyLL");
                     symbol.graphWidth = symbol.symbolSprite.element.sourcePixelSize.x;
                     symbol.symbolSprite.scale = symbol.shadowSprite2.scale = symbol.shadowSprite1.scale = 1f + jelly.IconRadBonus;
                 }
             });
            c.Emit(OpCodes.Ldloc, vars[s_loc1]);
            if (c.TryGotoNext(MoveType.After,
                s_MatchCallOrCallvirt_IconSymbol_Draw))
            {
                c.Emit(OpCodes.Ldarg_0)
                 .Emit(OpCodes.Ldloc, vars[s_loc1])
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