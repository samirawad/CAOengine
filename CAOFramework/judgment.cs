using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ConditionFramework
{
    public class Judgement
    {
        public string JudgementTag; // The name of the opinion. Envy, Lust, Jealous etc

        public JObject ActorJudgement;  // The conditions under which the Actor in an Occurence will have this Opinion applied to them.

        public JObject TargetJudgement; // The conditions under which the Target in an Occurence will have this Opinion applied to them.

        public JObject SelfJudgement;  // The conditions under which the Witness in an Occurence will have this Opinion applied to them.

        // Judge the occurence from the perspective of the witness actor
        public Actor FormJudgement(Actor Judge, Actor Subject, Occurrence occurence)
        {
            Relationship currentRelationship = Judge.Relationships.FirstOrDefault(r => { return r.Actor == Subject.Name; });
            // If the current relationship doesn't exist, this occurence we begin a new one
            if(currentRelationship == null)
            {
                currentRelationship = new Relationship()
                {
                    Actor = Subject.Name,
                    Opinions = new List<Opinion>()
                };
            }

            return Judge;
        }

        private delegate bool JudgementDel(ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge);

        // Is this judgement valid from the perspective of Actor a in the Occurence o?
        public static bool JudgementIsValid(ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge)
        {
            bool result = false;
            foreach (var p in c.Properties())
            {
                if (p.Name == "Description")
                {
                    Desc += c["Description"].Value<string>() + Environment.NewLine;
                }
                else
                {
                    result = ConditionDict[p.Name](ref Desc, c, o, subject, judge);
                }
            }
            return result;
        }

        private static Dictionary<string, JudgementDel> ConditionDict = new Dictionary<string, JudgementDel>()
        {
            {"All", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                // All subcondtions need to be true
                JArray s = c["All"] as JArray;
                bool result = true;
                foreach(var jc in s)
                {
                    bool currResult = JudgementIsValid(ref Desc, (JObject)jc, o, subject, judge);
                    if(!currResult)
                    {
                        result = false;
                        break;
                    }
                }
                return result;
            }},
            {"Any", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                // Any of subcondtions need to be true
                JArray s = c["Any"] as JArray;
                bool result = false;
                foreach(var jc in s)
                {
                    bool currResult = JudgementIsValid(ref Desc, (JObject)jc, o, subject, judge);
                    if(currResult)
                    {
                        result = true;
                        break;
                    }
                }
                return result;
            }},
            // This returns true if the Target in the occurence is upholding
            // any of the ideals specified in c.
            {"TargetUpholds", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return o.Target.Upholds.Any(u =>
                {
                   return (c["Upholds"] as JArray).Values().Contains(u);
                });
            }},
            // This returns true if the Actor in the occurence is upholding
            // any of the ideals specified in c.
            {"ActorUpholds", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return o.Actor.Upholds.Any(u =>
                {
                   return (c["Upholds"] as JArray).Values().Contains(u);
                });
            }},
            // This returns true if the Target in the occurence is forsaking
            // any of the ideals specified in c.
            {"TargetForsakes", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return o.Target.Forsakes.Any(u =>
                {
                   return (c["Forsakes"] as JArray).Values().Contains(u);
                });
            }},
            // This returns true if the Actor in the occurence is forsaking
            // any of the ideals specified in c.
            {"ActorForsakes", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return o.Actor.Forsakes.Any(u =>
                {
                   return (c["Forsakes"] as JArray).Values().Contains(u);
                });
            }},
            // This returns true if the Actor posesses ANY of the tags specified in c
            {"ActorAny", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return false;
            }},
            // This returns true if the Actor posesses ALL of the tags specified in c
            {"ActorAll", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return false;
            }},
            // This returns true if the Actor posesses NONE of the tags specified in c
            {"ActorNone", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return false;
            }},
            // This returns true if the Target posesses ANY of the tags specified in c
            {"TargetAny", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return false;
            }},
            // This returns true if the Target posesses ALL of the tags specified in c
            {"TargetAll", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
                return false;
            }},
            // This returns true if the Target posesses NONE of the tags specified in c
            {"TargetNone", (ref string Desc, JObject c, Occurrence o, Actor subject, Actor judge) => {
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
