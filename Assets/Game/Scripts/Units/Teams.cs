namespace Bootleg.Units
{
    public static class Teams
    {
        /// <summary>
        /// Player number for units no player controls, such as the ball. No player is registered with it,
        /// so the turn resolver never gives it a turn.
        /// </summary>
        public const int Neutral = -1;
    }
}
