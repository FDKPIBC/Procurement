﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POEApi.Model;

namespace Procurement.ViewModel.Recipes
{
    public class SameNameRecipe : Recipe
    {
        private string name;
        private int setCount;

        public SameNameRecipe(string recipeName, int setCount)
            : base(66)
        {
            this.name = recipeName;
            this.setCount = setCount;
        }

        public override string Name
        {
            get { return name; }
        }

        public override IEnumerable<RecipeResult> Matches(IEnumerable<POEApi.Model.Item> items)
        {
            return findDuplicates(items, setCount);
        }

        private IEnumerable<RecipeResult> findDuplicates(IEnumerable<POEApi.Model.Item> items, int setCount)
        {
            // Gear and AbyssJewel both have Rarity, but do not inherit it from a common parent class, so we need to
            // handle each subclass separately.
            IEnumerable<Item> gear = items.Where(i => i.Name != string.Empty &&
                ((i is Gear && (i as Gear).Rarity != Rarity.Unique) ||
                 (i is AbyssJewel && (i as AbyssJewel).Rarity != Rarity.Unique)));

            List<RecipeResult> matches = new List<RecipeResult>();

            var itemKeys = gear.GroupBy(i => i.Name).Where(g => g.Count() > 1);
            foreach (var item in itemKeys)
            {
                var matchedItems = gear.Where(g => g.Name == item.Key)
                    .Select(g => g as Item).ToList();

                while (matchedItems.Count > 0)
                {
                    var currentSet = matchedItems.Take(setCount).ToList();
                    matchedItems.RemoveRange(0, currentSet.Count);
                    Decimal percentMatch = ((Decimal)currentSet.Count / setCount) * 100;
                    var candidateMatch = new RecipeResult()
                    {
                        Instance = this,
                        IsMatch = percentMatch > base.ReturnMatchesGreaterThan,
                        MatchedItems = currentSet,
                        Missing = new List<string>(),
                        PercentMatch = percentMatch
                    };
                    if (candidateMatch.IsMatch)
                    {
                        matches.Add(candidateMatch);
                    }
                }
            }

            return matches;
        }
    }
}
