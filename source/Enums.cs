global using LBMergedMods.Enums;
using MoreSlugcats;
using System.Collections.Generic;

namespace LBMergedMods.Enums;

public static class RoomEffectType
{
    public static RoomSettings.RoomEffect.Type SeedBats = new(nameof(SeedBats), true),
        Bigrubs = new(nameof(Bigrubs), true);

    public static void UnregisterValues()
    {
        if (SeedBats is not null)
        {
            SeedBats.Unregister();
            SeedBats = null!;
        }
        if (Bigrubs is not null)
        {
            Bigrubs.Unregister();
            Bigrubs = null!;
        }
    }
}

public static class CreatureTemplateType
{
    internal static HashSet<CreatureTemplate.Type> s_M4RCreatureList;
    public static CreatureTemplate.Type SilverLizard = new(nameof(SilverLizard), true),
        ThornBug = new(nameof(ThornBug), true),
        NoodleEater = new(nameof(NoodleEater), true),
        SurfaceSwimmer = new(nameof(SurfaceSwimmer), true),
        Scutigera = new(nameof(Scutigera), true),
        RedHorrorCenti = new(nameof(RedHorrorCenti), true),
        Polliwog = new(nameof(Polliwog), true),
        MiniLeviathan = new(nameof(MiniLeviathan), true),
        Hoverfly = new(nameof(Hoverfly), true),
        FatFireFly = new(nameof(FatFireFly), true),
        BouncingBall = new(nameof(BouncingBall), true),
        Sporantula = new(nameof(Sporantula), true),
        WaterSpitter = new(nameof(WaterSpitter), true),
        WaterBlob = new(nameof(WaterBlob), true),
        HunterSeeker = new(nameof(HunterSeeker), true),
        FlyingBigEel = new(nameof(FlyingBigEel), true),
        MiniFlyingBigEel = new(nameof(MiniFlyingBigEel), true),
        HazerMom = new(nameof(HazerMom), true),
        TintedBeetle = new(nameof(TintedBeetle), true),
        Blizzor = new(nameof(Blizzor), true),
        MoleSalamander = new(nameof(MoleSalamander), true),
        MiniBlackLeech = new(nameof(MiniBlackLeech), true),
        Denture = new(nameof(Denture), true);

    static CreatureTemplateType()
    {
        s_M4RCreatureList = [SilverLizard,
            ThornBug,
            NoodleEater,
            SurfaceSwimmer,
            Scutigera,
            RedHorrorCenti,
            Polliwog,
            MiniLeviathan,
            Hoverfly,
            FatFireFly,
            BouncingBall,
            Sporantula,
            WaterSpitter,
            WaterBlob,
            HunterSeeker,
            FlyingBigEel,
            MiniFlyingBigEel,
            HazerMom,
            TintedBeetle,
            MiniBlackLeech,
            Denture,
            Blizzor,
            MoleSalamander];
    }

    public static void UnregisterValues()
    {
        if (HazerMom is not null)
        {
            HazerMom.Unregister();
            HazerMom = null!;
        }
        if (SilverLizard is not null)
        {
            SilverLizard.Unregister();
            SilverLizard = null!;
        }
        if (ThornBug is not null)
        {
            ThornBug.Unregister();
            ThornBug = null!;
        }
        if (NoodleEater is not null)
        {
            NoodleEater.Unregister();
            NoodleEater = null!;
        }
        if (SurfaceSwimmer is not null)
        {
            SurfaceSwimmer.Unregister();
            SurfaceSwimmer = null!;
        }
        if (Scutigera is not null)
        {
            Scutigera.Unregister();
            Scutigera = null!;
        }
        if (RedHorrorCenti is not null)
        {
            RedHorrorCenti.Unregister();
            RedHorrorCenti = null!;
        }
        if (Polliwog is not null)
        {
            Polliwog.Unregister();
            Polliwog = null!;
        }
        if (MiniLeviathan is not null)
        {
            MiniLeviathan.Unregister();
            MiniLeviathan = null!;
        }
        if (Hoverfly is not null)
        {
            Hoverfly.Unregister();
            Hoverfly = null!;
        }
        if (FatFireFly is not null)
        {
            FatFireFly.Unregister();
            FatFireFly = null!;
        }
        if (BouncingBall is not null)
        {
            BouncingBall.Unregister();
            BouncingBall = null!;
        }
        if (Sporantula is not null)
        {
            Sporantula.Unregister();
            Sporantula = null!;
        }
        if (WaterSpitter is not null)
        {
            WaterSpitter.Unregister();
            WaterSpitter = null!;
        }
        if (WaterBlob is not null)
        {
            WaterBlob.Unregister();
            WaterBlob = null!;
        }
        if (HunterSeeker is not null)
        {
            HunterSeeker.Unregister();
            HunterSeeker = null!;
        }
        if (FlyingBigEel is not null)
        {
            FlyingBigEel.Unregister();
            FlyingBigEel = null!;
        }
        if (MiniFlyingBigEel is not null)
        {
            MiniFlyingBigEel.Unregister();
            MiniFlyingBigEel = null!;
        }
        if (TintedBeetle is not null)
        {
            TintedBeetle.Unregister();
            TintedBeetle = null!;
        }
        if (Blizzor is not null)
        {
            Blizzor.Unregister();
            Blizzor = null!;
        }
        if (MoleSalamander is not null)
        {
            MoleSalamander.Unregister();
            MoleSalamander = null!;
        }
        if (MiniBlackLeech is not null)
        {
            MiniBlackLeech.Unregister();
            MiniBlackLeech = null!;
        }
        if (Denture is not null)
        {
            Denture.Unregister();
            Denture = null!;
        }
    }
}

public static class SandboxUnlockID
{
    public static MultiplayerUnlocks.SandboxUnlockID ThornyStrawberry = new(nameof(ThornyStrawberry), true),
        SilverLizard = new(nameof(SilverLizard), true),
        ThornBug = new(nameof(ThornBug), true),
        NoodleEater = new(nameof(NoodleEater), true),
        SurfaceSwimmer = new(nameof(SurfaceSwimmer), true),
        SeedBat = new(nameof(SeedBat), true),
        Scutigera = new(nameof(Scutigera), true),
        RedHorrorCenti = new(nameof(RedHorrorCenti), true),
        Bigrub = new(nameof(Bigrub), true),
        Polliwog = new(nameof(Polliwog), true),
        MiniLeviathan = new(nameof(MiniLeviathan), true),
        Hoverfly = new(nameof(Hoverfly), true),
        FatFireFly = new(nameof(FatFireFly), true),
        BouncingBall = new(nameof(BouncingBall), true),
        Sporantula = new(nameof(Sporantula), true),
        WaterSpitter = new(nameof(WaterSpitter), true),
        WaterBlob = new(nameof(WaterBlob), true),
        HunterSeeker = new(nameof(HunterSeeker), true),
        FlyingBigEel = new(nameof(FlyingBigEel), true),
        MiniFlyingBigEel = new(nameof(MiniFlyingBigEel), true),
        LittleBalloon = new(nameof(LittleBalloon), true),
        BouncingMelon = new(nameof(BouncingMelon), true),
        HazerMom = new(nameof(HazerMom), true),
        TintedBeetle = new(nameof(TintedBeetle), true),
        Physalis = new(nameof(Physalis), true),
        LimeMushroom = new(nameof(LimeMushroom), true),
        Blizzor = new(nameof(Blizzor), true),
        MoleSalamander = new(nameof(MoleSalamander), true),
        MarineEye = new(nameof(MarineEye), true),
        MiniBlackLeech = new(nameof(MiniBlackLeech), true),
        StarLemon = new(nameof(StarLemon), true),
        Denture = new(nameof(Denture), true);

    public static void UnregisterValues()
    {
        if (HazerMom is not null)
        {
            HazerMom.Unregister();
            HazerMom = null!;
        }
        if (ThornyStrawberry is not null)
        {
            ThornyStrawberry.Unregister();
            ThornyStrawberry = null!;
        }
        if (SilverLizard is not null)
        {
            SilverLizard.Unregister();
            SilverLizard = null!;
        }
        if (ThornBug is not null)
        {
            ThornBug.Unregister();
            ThornBug = null!;
        }
        if (NoodleEater is not null)
        {
            NoodleEater.Unregister();
            NoodleEater = null!;
        }
        if (SurfaceSwimmer is not null)
        {
            SurfaceSwimmer.Unregister();
            SurfaceSwimmer = null!;
        }
        if (SeedBat is not null)
        {
            SeedBat.Unregister();
            SeedBat = null!;
        }
        if (Scutigera is not null)
        {
            Scutigera.Unregister();
            Scutigera = null!;
        }
        if (RedHorrorCenti is not null)
        {
            RedHorrorCenti.Unregister();
            RedHorrorCenti = null!;
        }
        if (Bigrub is not null)
        {
            Bigrub.Unregister();
            Bigrub = null!;
        }
        if (Polliwog is not null)
        {
            Polliwog.Unregister();
            Polliwog = null!;
        }
        if (MiniLeviathan is not null)
        {
            MiniLeviathan.Unregister();
            MiniLeviathan = null!;
        }
        if (Hoverfly is not null)
        {
            Hoverfly.Unregister();
            Hoverfly = null!;
        }
        if (FatFireFly is not null)
        {
            FatFireFly.Unregister();
            FatFireFly = null!;
        }
        if (BouncingBall is not null)
        {
            BouncingBall.Unregister();
            BouncingBall = null!;
        }
        if (Sporantula is not null)
        {
            Sporantula.Unregister();
            Sporantula = null!;
        }
        if (WaterSpitter is not null)
        {
            WaterSpitter.Unregister();
            WaterSpitter = null!;
        }
        if (WaterBlob is not null)
        {
            WaterBlob.Unregister();
            WaterBlob = null!;
        }
        if (HunterSeeker is not null)
        {
            HunterSeeker.Unregister();
            HunterSeeker = null!;
        }
        if (FlyingBigEel is not null)
        {
            FlyingBigEel.Unregister();
            FlyingBigEel = null!;
        }
        if (MiniFlyingBigEel is not null)
        {
            MiniFlyingBigEel.Unregister();
            MiniFlyingBigEel = null!;
        }
        if (LittleBalloon is not null)
        {
            LittleBalloon.Unregister();
            LittleBalloon = null!;
        }
        if (BouncingMelon is not null)
        {
            BouncingMelon.Unregister();
            BouncingMelon = null!;
        }
        if (TintedBeetle is not null)
        {
            TintedBeetle.Unregister();
            TintedBeetle = null!;
        }
        if (Physalis is not null)
        {
            Physalis.Unregister();
            Physalis = null!;
        }
        if (LimeMushroom is not null)
        {
            LimeMushroom.Unregister();
            LimeMushroom = null!;
        }
        if (Blizzor is not null)
        {
            Blizzor.Unregister();
            Blizzor = null!;
        }
        if (MoleSalamander is not null)
        {
            MoleSalamander.Unregister();
            MoleSalamander = null!;
        }
        if (MarineEye is not null)
        {
            MarineEye.Unregister();
            MarineEye = null!;
        }
        if (MiniBlackLeech is not null)
        {
            MiniBlackLeech.Unregister();
            MiniBlackLeech = null!;
        }
        if (StarLemon is not null)
        {
            StarLemon.Unregister();
            StarLemon = null!;
        }
        if (Denture is not null)
        {
            Denture.Unregister();
            Denture = null!;
        }
    }
}

public static class PlacedObjectType
{
    public static PlacedObject.Type ThornyStrawberry = new(nameof(ThornyStrawberry), true),
        LittleBalloon = new(nameof(LittleBalloon), true),
        BouncingMelon = new(nameof(BouncingMelon), true),
        Physalis = new(nameof(Physalis), true),
        HazerMom = new(nameof(HazerMom), true),
        DeadHazerMom = new(nameof(DeadHazerMom), true),
        AlbinoHazerMom = new(nameof(AlbinoHazerMom), true),
        DeadAlbinoHazerMom = new(nameof(DeadAlbinoHazerMom), true),
        AlbinoFormHazer = new(nameof(AlbinoFormHazer), true),
        DeadAlbinoFormHazer = new(nameof(DeadAlbinoFormHazer), true),
        LimeMushroom = new(nameof(LimeMushroom), true),
        RubberBlossom = new(nameof(RubberBlossom), true),
        MarineEye = new(nameof(MarineEye), true),
        StarLemon = new(nameof(StarLemon), true);

    public static void UnregisterValues()
    {
        if (ThornyStrawberry is not null)
        {
            ThornyStrawberry.Unregister();
            ThornyStrawberry = null!;
        }
        if (LittleBalloon is not null)
        {
            LittleBalloon.Unregister();
            LittleBalloon = null!;
        }
        if (BouncingMelon is not null)
        {
            BouncingMelon.Unregister();
            BouncingMelon = null!;
        }
        if (HazerMom is not null)
        {
            HazerMom.Unregister();
            HazerMom = null!;
        }
        if (DeadHazerMom is not null)
        {
            DeadHazerMom.Unregister();
            DeadHazerMom = null!;
        }
        if (AlbinoHazerMom is not null)
        {
            AlbinoHazerMom.Unregister();
            AlbinoHazerMom = null!;
        }
        if (DeadAlbinoHazerMom is not null)
        {
            DeadAlbinoHazerMom.Unregister();
            DeadAlbinoHazerMom = null!;
        }
        if (AlbinoFormHazer is not null)
        {
            AlbinoFormHazer.Unregister();
            AlbinoFormHazer = null!;
        }
        if (DeadAlbinoFormHazer is not null)
        {
            DeadAlbinoFormHazer.Unregister();
            DeadAlbinoFormHazer = null!;
        }
        if (Physalis is not null)
        {
            Physalis.Unregister();
            Physalis = null!;
        }
        if (LimeMushroom is not null)
        {
            LimeMushroom.Unregister();
            LimeMushroom = null!;
        }
        if (RubberBlossom is not null)
        {
            RubberBlossom.Unregister();
            RubberBlossom = null!;
        }
        if (MarineEye is not null)
        {
            MarineEye.Unregister();
            MarineEye = null!;
        }
        if (StarLemon is not null)
        {
            StarLemon.Unregister();
            StarLemon = null!;
        }
    }
}

public static class AbstractObjectType
{
    public static AbstractPhysicalObject.AbstractObjectType ThornyStrawberry = new(nameof(ThornyStrawberry), true),
        SporeProjectile = new(nameof(SporeProjectile), true),
        BlobPiece = new(nameof(BlobPiece), true),
        LittleBalloon = new(nameof(LittleBalloon), true),
        BouncingMelon = new(nameof(BouncingMelon), true),
        Physalis = new(nameof(Physalis), true),
        LimeMushroom = new(nameof(LimeMushroom), true),
        RubberBlossom = new(nameof(RubberBlossom), true),
        GummyAnther = new(nameof(GummyAnther), true),
        MarineEye = new(nameof(MarineEye), true),
        StarLemon = new(nameof(StarLemon), true);

    public static void UnregisterValues()
    {
        if (ThornyStrawberry is not null)
        {
            ThornyStrawberry.Unregister();
            ThornyStrawberry = null!;
        }
        if (SporeProjectile is not null)
        {
            SporeProjectile.Unregister();
            SporeProjectile = null!;
        }
        if (BlobPiece is not null)
        {
            BlobPiece.Unregister();
            BlobPiece = null!;
        }
        if (LittleBalloon is not null)
        {
            LittleBalloon.Unregister();
            LittleBalloon = null!;
        }
        if (BouncingMelon is not null)
        {
            BouncingMelon.Unregister();
            BouncingMelon = null!;
        }
        if (Physalis is not null)
        {
            Physalis.Unregister();
            Physalis = null!;
        }
        if (LimeMushroom is not null)
        {
            LimeMushroom.Unregister();
            LimeMushroom = null!;
        }
        if (RubberBlossom is not null)
        {
            RubberBlossom.Unregister();
            RubberBlossom = null!;
        }
        if (GummyAnther is not null)
        {
            GummyAnther.Unregister();
            GummyAnther = null!;
        }
        if (MarineEye is not null)
        {
            MarineEye.Unregister();
            MarineEye = null!;
        }
        if (StarLemon is not null)
        {
            StarLemon.Unregister();
            StarLemon = null!;
        }
    }
}

public static class MultiplayerItemType
{
    public static PlacedObject.MultiplayerItemData.Type ThornyStrawberry = new(nameof(ThornyStrawberry), true),
        LittleBalloon = new(nameof(LittleBalloon), true),
        BouncingMelon = new(nameof(BouncingMelon), true),
        Physalis = new(nameof(Physalis), true),
        LimeMushroom = new(nameof(LimeMushroom), true),
        MarineEye = new(nameof(MarineEye), true),
        StarLemon = new(nameof(StarLemon), true);

    public static void UnregisterValues()
    {
        if (ThornyStrawberry is not null)
        {
            ThornyStrawberry.Unregister();
            ThornyStrawberry = null!;
        }
        if (LittleBalloon is not null)
        {
            LittleBalloon.Unregister();
            LittleBalloon = null!;
        }
        if (BouncingMelon is not null)
        {
            BouncingMelon.Unregister();
            BouncingMelon = null!;
        }
        if (Physalis is not null)
        {
            Physalis.Unregister();
            Physalis = null!;
        }
        if (LimeMushroom is not null)
        {
            LimeMushroom.Unregister();
            LimeMushroom = null!;
        }
        if (MarineEye is not null)
        {
            MarineEye.Unregister();
            MarineEye = null!;
        }
        if (StarLemon is not null)
        {
            StarLemon.Unregister();
            StarLemon = null!;
        }
    }
}

public static class MiscItemType
{
    public static SLOracleBehaviorHasMark.MiscItemType ThornyStrawberry = new(nameof(ThornyStrawberry), true),
        BlobPiece = new(nameof(BlobPiece), true),
        LittleBalloon = new(nameof(LittleBalloon), true),
        BouncingMelon = new(nameof(BouncingMelon), true),
        Physalis = new(nameof(Physalis), true),
        LimeMushroom = new(nameof(LimeMushroom), true),
        GummyAnther = new(nameof(GummyAnther), true),
        MarineEye = new(nameof(MarineEye), true),
        StarLemon = new(nameof(StarLemon), true),
        SporeProjectile = new(nameof(SporeProjectile), true);

    public static void UnregisterValues()
    {
        if (ThornyStrawberry is not null)
        {
            ThornyStrawberry.Unregister();
            ThornyStrawberry = null!;
        }
        if (BlobPiece is not null)
        {
            BlobPiece.Unregister();
            BlobPiece = null!;
        }
        if (LittleBalloon is not null)
        {
            LittleBalloon.Unregister();
            LittleBalloon = null!;
        }
        if (BouncingMelon is not null)
        {
            BouncingMelon.Unregister();
            BouncingMelon = null!;
        }
        if (Physalis is not null)
        {
            Physalis.Unregister();
            Physalis = null!;
        }
        if (LimeMushroom is not null)
        {
            LimeMushroom.Unregister();
            LimeMushroom = null!;
        }
        if (GummyAnther is not null)
        {
            GummyAnther.Unregister();
            GummyAnther = null!;
        }
        if (MarineEye is not null)
        {
            MarineEye.Unregister();
            MarineEye = null!;
        }
        if (StarLemon is not null)
        {
            StarLemon.Unregister();
            StarLemon = null!;
        }
        if (SporeProjectile is not null)
        {
            SporeProjectile.Unregister();
            SporeProjectile = null!;
        }
    }
}

public static class SlugFood
{
    public static SlugNPCAI.Food? ThornyStrawberry, BlobPiece, LittleBalloon, Physalis, GummyAnther, MarineEye, StarLemon;

    public static void UnregisterValues()
    {
        if (ThornyStrawberry is not null)
        {
            ThornyStrawberry.Unregister();
            ThornyStrawberry = null;
        }
        if (BlobPiece is not null)
        {
            BlobPiece.Unregister();
            BlobPiece = null;
        }
        if (LittleBalloon is not null)
        {
            LittleBalloon.Unregister();
            LittleBalloon = null;
        }
        if (Physalis is not null)
        {
            Physalis.Unregister();
            Physalis = null;
        }
        if (GummyAnther is not null)
        {
            GummyAnther.Unregister();
            GummyAnther = null;
        }
        if (MarineEye is not null)
        {
            MarineEye.Unregister();
            MarineEye = null;
        }
        if (StarLemon is not null)
        {
            StarLemon.Unregister();
            StarLemon = null;
        }
    }
}

public static class NewSoundID
{
    public static SoundID Hoverfly_Startle = new(nameof(Hoverfly_Startle), true),
        Hoverfly_Idle = new(nameof(Hoverfly_Idle), true),
        Hoverfly_Fly_LOOP = new(nameof(Hoverfly_Fly_LOOP), true),
        Flying_Leviathan_Bite = new(nameof(Flying_Leviathan_Bite), true);

    public static void UnregisterValues()
    {
        if (Hoverfly_Startle is not null)
        {
            Hoverfly_Startle.Unregister();
            Hoverfly_Startle = null!;
        }
        if (Hoverfly_Idle is not null)
        {
            Hoverfly_Idle.Unregister();
            Hoverfly_Idle = null!;
        }
        if (Hoverfly_Fly_LOOP is not null)
        {
            Hoverfly_Fly_LOOP.Unregister();
            Hoverfly_Fly_LOOP = null!;
        }
        if (Flying_Leviathan_Bite is not null)
        {
            Flying_Leviathan_Bite.Unregister();
            Flying_Leviathan_Bite = null!;
        }
    }
}