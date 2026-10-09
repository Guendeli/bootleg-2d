using System;
using System.Collections.Generic;

namespace Bootleg.Units.Stats
{
    /// <summary>
    /// Runtime stat block: base values plus modifiers.
    /// Final value = (base + sum of flat) * (1 + sum of percent).
    /// </summary>
    public class UnitStats
    {
        private readonly Dictionary<StatType, float> _baseValues = new Dictionary<StatType, float>();
        private readonly List<StatModifier> _modifiers = new List<StatModifier>();

        /// <summary>Raised whenever a base value or modifier changes.</summary>
        public event Action<UnitStats> Changed;

        public UnitStats(IReadOnlyDictionary<StatType, float> baseValues)
        {
            foreach (var pair in baseValues)
            {
                _baseValues[pair.Key] = pair.Value;
            }
        }

        public IReadOnlyList<StatModifier> Modifiers => _modifiers;

        public float GetBase(StatType stat)
        {
            return _baseValues.TryGetValue(stat, out var value) ? value : 0f;
        }

        public void SetBase(StatType stat, float value)
        {
            _baseValues[stat] = value;
            Changed?.Invoke(this);
        }

        public float Get(StatType stat)
        {
            var flat = 0f;
            var percent = 0f;
            foreach (var modifier in _modifiers)
            {
                if (modifier.Stat != stat)
                {
                    continue;
                }

                if (modifier.Kind == ModifierKind.Flat)
                {
                    flat += modifier.Value;
                }
                else
                {
                    percent += modifier.Value;
                }
            }

            return (GetBase(stat) + flat) * (1f + percent);
        }

        public int GetInt(StatType stat)
        {
            return (int)MathF.Round(Get(stat), MidpointRounding.AwayFromZero);
        }

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            Changed?.Invoke(this);
        }

        /// <returns>The number of modifiers removed.</returns>
        public int RemoveModifiersFromSource(object source)
        {
            var removed = _modifiers.RemoveAll(m => Equals(m.Source, source));
            if (removed > 0)
            {
                Changed?.Invoke(this);
            }
            return removed;
        }
    }
}
