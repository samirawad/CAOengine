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
        public string Name; // The name of the opinion. Envy, Lust, Jealous etc

        public JObject Actor;  // The conditions under which the Actor in an Occurence will have this Opinion applied to them.

        public JObject Target; // The conditions under which the Target in an Occurence will have this Opinion applied to them.

        public JObject Self;  // The conditions under which the Witness in an Occurence will have this Opinion applied to them.

        // Judge the occurence from the perspective of the witness actor
        public void FormJudgement(Actor witness, Occurrence occurence)
        {
            // The 
        }
    }


}
