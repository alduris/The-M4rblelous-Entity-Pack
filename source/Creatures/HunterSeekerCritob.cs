using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using System.Collections.Generic;
using UnityEngine;
using Fisobs.Sandbox;

namespace LBMergedMods.Creatures;

sealed class HunterSeekerCritob : Critob
{
    internal HunterSeekerCritob() : base(CreatureTemplateType.HunterSeeker) 
    {
        Icon = new SimpleIcon("Kill_HunterSeeker", Color.white);
        LoadedPerformanceCost = 50f;
        SandboxPerformanceCost = new(.5f, .5f);
        RegisterUnlock(KillScore.Configurable(9), SandboxUnlockID.HunterSeeker);
    }

    public override int ExpeditionScore() => 9;

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.WhiteLizard;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.white;

    public override string DevtoolsMapName(AbstractCreature acrit) => "HunS";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Lizards,
        RoomAttractivenessPanel.Category.LikesOutside
    ];

    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null);

    public override void EstablishRelationships()
    {
        var p = new Relationships(Type);
        p.Rivals(Type, .1f);
        p.Rivals(CreatureTemplate.Type.LizardTemplate, .2f);
        p.Rivals(CreatureTemplate.Type.WhiteLizard, .5f);
    }

    public override IEnumerable<string> WorldFileAliases() => ["seeker", "hunterseeker", "hunter seeker"];

    public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => new HunterSeekerAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new HunterSeeker(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }
}