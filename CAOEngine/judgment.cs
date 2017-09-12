﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ConditionFramework
{
    /*
     *  This class encapsulates the logic for how judgements are passed for a perticular tag, such as Envy, Respect, etc...
     */
    public class Judgement
    {
        public string JudgementTag; // The name of the opinion. Envy, Lust, Jealous etc

        public JObject ActorJudgement;  // The conditions under which the Actor in an Occurence will have this Opinion applied to them.

        public JObject TargetJudgement; // The conditions under which the Target in an Occurence will have this Opinion applied to them.

        public JObject SelfJudgement;  // The conditions under which the Witness in an Occurence will have this Opinion applied to them.

        // Update the relationships of each participant and witness of an occurence
        public World JudgeOccurence(Occurrence occurence, World world)
        {
            // For each Actor, Target and witness....
            return world;
        }
        
        // Judge the occurence from the perspective of the witness actor.
        // Pull them from the world by name, add their judgement then return the world.
        public World FormJudgement(string judgeName, Occurrence occurence, World world)
        {
            Agent judge = world.Agents.Find(a => a.Name == judgeName);

            /////////////////////
            // Judge the Actor // 
            /////////////////////

            // If the current relationship doesn't exist, this occurence we begin a new one
            // It's the relationship we'll be checking for tags, not the actual subject...
            Relationship currentRelationship = judge.Relationships?[occurence.Actor];
            if (currentRelationship == null)
            {
                currentRelationship = new Relationship()
                {
                    Actor = occurence.Actor,
                    Opinions = new List<Opinion>()
                };
            }

            string judgementReason = "";
            if(Judgement.JudgementIsValid(ref judgementReason, ActorJudgement, occurence, judge))
            {
                currentRelationship.Opinions.Add(new Opinion()
                {
                    Judgement = JudgementTag,
                    Reason = judgementReason
                });
            }

            ///////////////////////
            // Judge the Target  // 
            ///////////////////////

            // If the current relationship doesn't exist, this occurence we begin a new one
            // It's the relationship we'll be checking for tags, not the actual subject...
            currentRelationship = judge.Relationships?[occurence.Target];
            if (currentRelationship == null)
            {
                currentRelationship = new Relationship()
                {
                    Actor = occurence.Actor,
                    Opinions = new List<Opinion>()
                };
            }

            judgementReason = "";
            if (Judgement.JudgementIsValid(ref judgementReason, TargetJudgement, occurence, judge))
            {
                currentRelationship.Opinions.Add(new Opinion()
                {
                    Judgement = JudgementTag,
                    Reason = judgementReason
                });
            }

            return world;
        }

        /*
         * Notice that when making a judgement, we reference the actual agent for the judge,
         * but for the subject we use the judge's relationship to the subject and not the agent it represents.
         * This is because the two can differ.  It's the relationship which matters, which is constructed by
         * all the previous occurrences  the judge and the subject have already shared.
         */
        private delegate bool JudgementDel(ref string Desc, JObject c, Occurrence o, Agent judge);

        // Is this judgement valid from the perspective of Actor a in the Occurence o?
        public static bool JudgementIsValid(ref string Desc, JObject c, Occurrence o, Agent judge)
        {
            bool result = false;
            foreach (var p in c.Properties())
            {
                if (p.Name == "Description")
                {
                    // append any additional description to the end.
                    Desc += c["Description"].Value<string>() + Environment.NewLine;
                }
                else
                {
                    result = ConditionDict[p.Name](ref Desc, c, o, judge);
                }
            }
            return result;
        }

        private static Dictionary<string, JudgementDel> ConditionDict = new Dictionary<string, JudgementDel>()
        {
            {"All", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                // All subcondtions need to be true
                JArray s = c["All"] as JArray;
                bool result = true;
                foreach(var jc in s)
                {
                    bool currResult = JudgementIsValid(ref Desc, (JObject)jc, o, judge);
                    if(!currResult)
                    {
                        result = false;
                        break;
                    }
                }
                if(result)
                {
                    Desc = "All true: " + Desc;
                }
                return result;
            }},
            {"Any", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                // Any of subcondtions need to be true
                JArray s = c["Any"] as JArray;
                bool result = false;
                foreach(var jc in s)
                {
                    bool currResult = JudgementIsValid(ref Desc, (JObject)jc, o, judge);
                    if(currResult)
                    {
                        result = true;
                        break;
                    }
                }
                if(result)
                {
                    Desc = "At least one true: " + Desc + Environment.NewLine;
                }
                return result;
            }},
            // This returns true if the Target in the occurence is upholding
            // any of the ideals specified in c.
            {"TargetUpholds", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                bool result = false;
                List<string> upheld = new List<string>();
                foreach(var upheldIdeal in o.TargetRole.Upholds)
                {
                    if((c["TargetUpholds"] as JArray).Values().Contains(upheldIdeal))
                    {
                        result = true;
                        upheld.Add(upheldIdeal);
                    }
                }
                if(result)
                {
                    Desc = Desc + o.Target + " upheld the ideal(s): " + String.Join(",", upheld);
                }
                return result;
            }},
            // This returns true if the Actor in the occurence is upholding
            // any of the ideals specified in c.
            {"ActorUpholds", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                bool result = false;
                List<string> upheld = new List<string>();
                foreach(var upheldIdeal in o.ActorRole.Upholds)
                {
                    if((c["ActorUpholds"] as JArray).Values().Contains(upheldIdeal))
                    {
                        result = true;
                        upheld.Add(upheldIdeal);
                    }
                }
                if(result)
                {
                    Desc = Desc + o.Actor + " upheld the ideal(s): " + String.Join(",", upheld);
                }
                return result;
            }},
            // This returns true if the Target in the occurence is forsaking
            // any of the ideals specified in c.
            {"TargetForsakes", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                bool result = false;
                List<string> forsaken = new List<string>();
                foreach(var forsakenIdeal in o.TargetRole.Forsakes)
                {
                    if((c["TargetForsakes"] as JArray).Values().Contains(forsakenIdeal))
                    {
                        result = true;
                        forsaken.Add(forsakenIdeal);
                    }
                }
                if(result)
                {
                    Desc = Desc + o.Target + " has forsaken the ideal(s): " + String.Join(",", forsaken);
                }
                return result;
            }},
            // This returns true if the Actor in the occurence is forsaking
            // any of the ideals specified in c.
            {"ActorForsakes", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                bool result = false;
                List<string> forsaken = new List<string>();
                foreach(var forsakenIdeal in o.ActorRole.Forsakes)
                {
                    if((c["ActorForsakes"] as JArray).Values().Contains(forsakenIdeal))
                    {
                        result = true;
                        forsaken.Add(forsakenIdeal);
                    }
                }
                if(result)
                {
                    Desc = Desc + o.Actor + " has forsaken the ideal(s): " + String.Join(",", forsaken);
                }
                return result;
            }},
            // This returns true if the Judge's relationship to the actor Actor posesses ANY of the tags specified in c
            {"ActorAny", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                // The tags we're checking
                var tags = (c["ActorAny"] as JArray).Values();

                // The relationship under examination
                var relationship = judge.Relationships?[o.Actor];

                // No relationship?  Then it can't possess any of the tags
                if(relationship == null) return false;
                
                // Are any of the opinions contained in the tags?
                bool result = false;
                List<string> actorTags = new List<string>();
                foreach(var tag in tags)
                {
                    if(relationship.Opinions.Any(opinion => {
                        return opinion.Judgement == tag.Value<string>();
                    }))
                    {
                        result = true;
                        actorTags.Add(tag.Value<string>());
                    }
                }
                if(result)
                {
                    Desc = Desc + o.Actor + " is considered by " + judge.Name + " to be: " + String.Join(",", actorTags);
                }
                return result;
            }},
            // This returns true if the Judge's relationship to the actor Actor posesses ALL of the tags specified in c
            {"ActorAll", (ref string Desc, JObject c, Occurrence o, Agent judge) => { 
                // The tags we're checking
                var tags = (c["ActorAny"] as JArray).Values();

                // The relationship under examination
                var relationship = judge.Relationships?[o.Actor];

                // No relationship?  Then it can't possess any of the tags
                if(relationship == null) return false;

                // All the tags must be present in this relationships opinions
                bool result = true;
                foreach(var tag in tags)
                {
                    if(!relationship.Opinions.Any(opinion => {
                        return opinion.Judgement == tag.Value<string>();
                    }))
                    {
                        result = false;
                        break;
                    }
                }
                if(result)
                {
                    Desc = Desc + o.Actor + " is considered by " + judge.Name + " to be: " + String.Join(",", tags);
                }
                return result;
            }},
            // This returns true if the Judge's relationship to the actor Actor posesses NONE of the tags specified in c
            {"ActorNone", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                throw new Exception("not implemeted");
            }},
            // This returns true if the Judge's relationship to the actor Actor posesses ANY of the tags specified in c
            {"TargetAny", (ref string Desc, JObject c, Occurrence o, Agent judge) => { 
                // The tags we're checking
                var tags = (c["TargetAny"] as JArray).Values();

                // The relationship under examination
                var relationship = judge.Relationships?[o.Target];

                // No relationship?  Then it can't possess any of the tags
                if(relationship == null) return false;
                
                // Are any of the opinions contained in the tags?
                bool result = false;
                List<string> targetTags = new List<string>();
                foreach(var tag in tags)
                {
                    if(relationship.Opinions.Any(opinion => {
                        return opinion.Judgement == tag.Value<string>();
                    }))
                    {
                        result = true;
                        targetTags.Add(tag.Value<string>());
                    }
                }
                if(result)
                {
                    Desc = Desc + o.Target + " is considered by " + judge.Name + " to be: " + String.Join(",", targetTags);
                }
                return result;
            }},
            // This returns true if the Judge's relationship to the actor Actor posesses ALL of the tags specified in c
            {"TargetAll", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                // The tags we're checking
                var tags = (c["TargetAll"] as JArray).Values();

                // The relationship under examination
                var relationship = judge.Relationships?[o.Target];

                // No relationship?  Then it can't possess any of the tags
                if(relationship == null) return false;

                // All the tags must be present in this relationships opinions
                bool result = true;
                foreach(var tag in tags)
                {
                    if(!relationship.Opinions.Any(opinion => {
                        return opinion.Judgement == tag.Value<string>();
                    }))
                    {
                        result = false;
                        break;
                    }
                }
                if(result)
                {
                    Desc = Desc + o.Target + " is considered by " + judge.Name + " to be: " + String.Join(",", tags);
                }
                return result;
            }},
            // This returns true if the Judge's relationship to the actor Actor posesses NONE of the tags specified in c
            {"TargetNone", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                throw new Exception("not implemeted");
            }},
            // This returns if the judge posesesses ANY of the tags specified in c
            {"SelfAny", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                throw new Exception("not implemeted");
            }},
            // This returns if the judge posesesses ALL of the tags specified in c
            {"SelfAll", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                throw new Exception("not implemeted");
            }},
            // This returns true if the Actor in the occurence is performing any of the actions specified in c
            {"ActorActionsAny", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                bool result = false;
                List<string> actorActions = new List<string>();
                foreach(var action in o.ActorRole.Actions)
                {
                    if((c["ActionActionsAny"] as JArray).Values().Contains(action))
                    {
                        result = true;
                        actorActions.Add(action);
                    }
                }
                if(result)
                {
                    Desc = Desc + o.Actor + " performed the action(s): " + String.Join(",", actorActions);
                }
                return result;
            }},
            // This returns true if the Actor in the occurence is performing any of the actions specified in c
            {"TargetActionsAny", (ref string Desc, JObject c, Occurrence o, Agent judge) => {
                bool result = false;
                List<string> targetActions = new List<string>();
                foreach(var action in o.TargetRole.Actions)
                {
                    if((c["TargetActionsAny"] as JArray).Values().Contains(action))
                    {
                        result = true;
                        targetActions.Add(action);
                    }
                }
                if(result)
                {
                    Desc = Desc + o.Target + " performed the action(s): " + String.Join(",", targetActions);
                }
                return result;
            }},
        };
    }


}