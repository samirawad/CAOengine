using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ConditionFramework
{
    public class JudgementCondition
    {
        private delegate bool JudgementDel(ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge);

        public string Description;

        // Is this judgement valid from the perspective of Actor a in the Occurence o?
        public bool JudgementIsValid(JObject c, Occurrence o, Actor subject, Actor judge)
        {
            bool result = false;
            foreach(var p in c.Properties())
            {
                if(p.Name != "Description") result = ConditionDict[p.Name](ref Description, c, o, subject, judge);
            }
            return result;
        }

        private static Dictionary<string, JudgementDel> ConditionDict = new Dictionary<string, JudgementDel>()
        {
            {"All", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                // All subcondtions need to be true
                JArray s = c["All"] as JArray;
                bool result = s.All(jc =>
                {
                    return false;//JudgementCondition.JudgementIsValid((JObject)jc, o, subject, judge);
                });
                return result;
            }},
            {"Any", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                // Any of subcondtions need to be true
                JArray s = c["Any"] as JArray;
                bool result = s.Any(jc =>
                {
                    return false; //JudgementCondition.JudgementIsValid((JObject)jc, o, subject, judge);
                });
                return result;
            }},
            {"Upholds", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                bool result = (c["Upholds"] as JArray).Values().Contains("blah");
                return false;
            }},
            {"Forsakes", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return false;
            }},
            {"ActorAny", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return false;
            }},
            {"TargetAny", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return false;
            }},
            {"SelfAny", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return false;
            }},
            {"ActionAny", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return false;
            }},
        };
    }
}
