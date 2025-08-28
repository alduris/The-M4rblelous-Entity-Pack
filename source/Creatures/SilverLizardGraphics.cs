using UnityEngine;
using LizardCosmetics;

namespace LBMergedMods.Creatures;

public class SilverLizardGraphics : LizardGraphics
{
    public SilverLizardGraphics(SilverLizard ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var spriteIndex = startOfExtraSprites + extraSprites;
        spriteIndex = AddCosmetic(spriteIndex, new AxolotlGills(this, spriteIndex));
        if (Random.value < .2f)
            spriteIndex = AddCosmetic(spriteIndex, new LongHeadScales(this, spriteIndex));
        if (Random.value < .3f)
            AddCosmetic(spriteIndex, new TailGeckoScales(this, spriteIndex));
        Random.state = state;
    }
}