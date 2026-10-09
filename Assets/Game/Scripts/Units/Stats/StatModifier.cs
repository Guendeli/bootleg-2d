namespace Bootleg.Units.Stats
{
    public enum ModifierKind
    {
        /// <summary>Added to the base value.</summary>
        Flat,
        /// <summary>Summed with other percent modifiers, then applied once: 0.1 = +10%.</summary>
        Percent,
    }

    /// <summary>
    /// A change to one stat, e.g. from equipment or a status effect.
    /// <see cref="Source"/> identifies the owner so all of its modifiers can be removed together.
    /// </summary>
    public readonly struct StatModifier
    {
        public readonly StatType Stat;
        public readonly ModifierKind Kind;
        public readonly float Value;
        public readonly object Source;

        public StatModifier(StatType stat, ModifierKind kind, float value, object source)
        {
            Stat = stat;
            Kind = kind;
            Value = value;
            Source = source;
        }
    }
}
