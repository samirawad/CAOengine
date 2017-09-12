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
        public string Actor;
        public string Target;
        public string Witness;
        public Role ActorRole;
        public Role TargetRole;
    }
    
}
