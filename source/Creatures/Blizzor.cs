namespace LBMergedMods.Creatures;

public class Blizzor : MirosBird
{
    public Blizzor(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var bs = bodyChunks;
        for (var i = 0; i < bs.Length; i++)
        {
            var chunk = bs[i];
            chunk.rad *= 1.4f;
            chunk.mass *= 1.1f;
        }
        var chunk4 = bs[4];
        chunk4.rad = 10f;
        chunk4.mass *= 1.05f;
        abstractCreature.HypothermiaImmune = true;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new BlizzorGraphics(this);

    public override void Blind(int blnd)
    {
        base.Blind(blnd);
        blind = 0;
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}