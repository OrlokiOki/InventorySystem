public class Effect
{
    public EffectType Type      { get; set; }
    public TargetType Target    { get; set; }
    public int        Magnitude { get; set; }
    public float      Duration  { get; set; }

    public Effect(EffectType type, TargetType target, int magnitude, float duration = 0f)
    {
        Type      = type;
        Target    = target;
        Magnitude = magnitude;
        Duration  = duration;
    }

    public override string ToString()
    {
        if (Duration > 0)
            return Type + " (" + Target + ") | +" + Magnitude + " for " + Duration + "s";
        return Type + " (" + Target + ") | " + Magnitude;
    }
}
