using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;
//CHK
public class SurfaceSwimmer : EggBug
{
    public static Color BugCol = Color.Lerp(Color.cyan, Color.blue, .5f);

    public SurfaceSwimmer(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        bounce = .25f;
        waterFriction = .9f;
        buoyancy = 1.5f;
        Random.State state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        hue = Mathf.Lerp(.25f, .05f, Custom.ClampedRandomVariation(.5f, .5f, 2f));
        Random.state = state;
        eggsLeft = 0;
    }

    public override void Die()
    {
        dropEggs = false;
        base.Die();
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new SurfaceSwimmerGraphics(this);

    public override Color ShortCutColor() => BugCol;

    public override void Update(bool eu)
    {
        base.Update(eu);
        lungs = 1f;
        if (Submersion > 0f)
        {
            var chs = bodyChunks;
            for (var i = 0; i < chs.Length; i++)
                chs[i].vel.x *= 1.02f;
        }
    }

    public override void LoseAllGrasps() { }
}