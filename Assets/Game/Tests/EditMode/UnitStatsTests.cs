using System.Collections.Generic;
using Bootleg.Units;
using Bootleg.Units.Stats;
using NUnit.Framework;
using UnityEngine;

namespace Bootleg.Tests
{
    public class UnitStatsTests
    {
        private static UnitStats CreateStats(float attack = 10)
        {
            return new UnitStats(new Dictionary<StatType, float> { { StatType.Attack, attack } });
        }

        [Test]
        public void Get_WithoutModifiers_ReturnsBase()
        {
            Assert.AreEqual(10f, CreateStats().Get(StatType.Attack));
        }

        [Test]
        public void Get_MissingStat_ReturnsZero()
        {
            Assert.AreEqual(0f, CreateStats().Get(StatType.Defence));
        }

        [Test]
        public void Get_AppliesFlatBeforeSummedPercent()
        {
            var stats = CreateStats();
            stats.AddModifier(new StatModifier(StatType.Attack, ModifierKind.Flat, 2, "sword"));
            stats.AddModifier(new StatModifier(StatType.Attack, ModifierKind.Percent, 0.25f, "buff"));
            stats.AddModifier(new StatModifier(StatType.Attack, ModifierKind.Percent, 0.25f, "ring"));

            // (10 + 2) * (1 + 0.5)
            Assert.AreEqual(18f, stats.Get(StatType.Attack), 0.0001f);
        }

        [Test]
        public void Get_IgnoresModifiersForOtherStats()
        {
            var stats = CreateStats();
            stats.AddModifier(new StatModifier(StatType.Defence, ModifierKind.Flat, 5, "shield"));

            Assert.AreEqual(10f, stats.Get(StatType.Attack));
        }

        [Test]
        public void RemoveModifiersFromSource_RemovesOnlyThatSource()
        {
            var stats = CreateStats();
            var sword = new object();
            stats.AddModifier(new StatModifier(StatType.Attack, ModifierKind.Flat, 2, sword));
            stats.AddModifier(new StatModifier(StatType.Attack, ModifierKind.Percent, 0.1f, sword));
            stats.AddModifier(new StatModifier(StatType.Attack, ModifierKind.Flat, 3, "buff"));

            Assert.AreEqual(2, stats.RemoveModifiersFromSource(sword));
            Assert.AreEqual(13f, stats.Get(StatType.Attack));
        }

        [Test]
        public void Changed_RaisedOnModifierAndBaseChanges_NotOnNoOpRemove()
        {
            var stats = CreateStats();
            var raised = 0;
            stats.Changed += _ => raised++;

            stats.AddModifier(new StatModifier(StatType.Attack, ModifierKind.Flat, 1, "a"));
            stats.SetBase(StatType.Attack, 12);
            stats.RemoveModifiersFromSource("missing");

            Assert.AreEqual(2, raised);
        }

        [Test]
        public void GetInt_RoundsHalfAwayFromZero()
        {
            Assert.AreEqual(3, CreateStats(2.5f).GetInt(StatType.Attack));
            Assert.AreEqual(2, CreateStats(2.4f).GetInt(StatType.Attack));
        }

        [Test]
        public void UnitDefinition_CreateStats_ReturnsIndependentInstances()
        {
            var definition = ScriptableObject.CreateInstance<UnitDefinition>();
            try
            {
                var a = definition.CreateStats();
                var b = definition.CreateStats();
                a.AddModifier(new StatModifier(StatType.MaxHealth, ModifierKind.Flat, 5, "a"));

                Assert.AreEqual(b.GetBase(StatType.MaxHealth) + 5, a.Get(StatType.MaxHealth));
                Assert.AreEqual(b.GetBase(StatType.MaxHealth), b.Get(StatType.MaxHealth));
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }
    }
}
