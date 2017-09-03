using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ConditionFramework
{

    public class FilterCondition
    {
        private delegate bool FilterCondtionDel(List<string> terms, World g, JToken s);

        public string Condition;

        public List<string> Filters;

        public bool IsTrue(World g, JToken s)
        {
            return FilterCondtionPredicates[Condition](Filters, g, s);
        }

        private static bool CheckCondtion(string m, World g, JToken s)
        {
            bool result = false;
            if (g.ActionLibrary.Filters.ContainsKey(m))
            {
                result = g.ActionLibrary.Filters[m].IsTrue(g, s);
            }
            return result;
        }

        private static Dictionary<string, FilterCondtionDel> FilterCondtionPredicates = new Dictionary<string, FilterCondtionDel>()
        {
            {"AllMustBeTrue", (t,g,s) =>
            {
                // All items must be true
                bool result =
                t.All(currentTerm => {
                    return CheckCondtion(currentTerm, g, s);
                });
                return result;
            }},
            {"NoneMustBeTrue", (t,g,s) =>
            {
                // None of the items can be true
                bool result =
                t.Any(currentTerm =>
                {
                    return CheckCondtion(currentTerm, g, s);
                }) ? false : true;
                return result;
            }},
            {"AnyMustBeTrue", (t,g,s) =>
            {
                // None of the items can be true
                bool result =
                t.Any(currentTerm =>
                {
                    return CheckCondtion(currentTerm, g, s);
                }) ? true : false;
                return result;
            }},
        };
    }
}
