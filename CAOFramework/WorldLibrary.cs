using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ConditionFramework
{
    public class WorldLibrary
    {
        public Dictionary<string, Selector> Selectors;

        public Dictionary<string, Filter> Filters;

        public Dictionary<string, FilterCondition> CompoundFilters;

        public Dictionary<string, Action> Actions;

        public Dictionary<string, Outcome> Outcomes;

        public Dictionary<string, Judgement> Judgements;
    }
}
