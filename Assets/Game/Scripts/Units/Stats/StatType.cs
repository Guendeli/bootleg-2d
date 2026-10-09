namespace Bootleg.Units.Stats
{
    /// <summary>
    /// Stats a unit can have. Each maps onto a TBSF <c>Unit</c> property in <see cref="TacticsUnit"/>.
    /// </summary>
    public enum StatType
    {
        MaxHealth,
        MaxActionPoints,
        MaxMovementPoints,
        AttackRange,
        Attack,
        Defence,
        /// <summary>How many cells a push sends the ball.</summary>
        KickPower,
        /// <summary>Chance (0..1) to intercept a pass, kick or dribble passing this unit.</summary>
        Interception,
    }
}
