namespace LBMergedMods.Creatures;

public class PolliwogCommunication(ArtificialIntelligence AI) : YellowAI(AI)
{
    public float LastFlicker, CurrentFlicker;
    public bool Increase, PackLeader;
}