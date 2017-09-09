using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ConditionFramework
{
    public class Occurrence
    {
        public string Description;
        public List<string> Actors;
        public List<string> Targets;
        public List<string> Witnesses;
        public Role ActorRole;
        public Role TargetRole;
    }
    
}
