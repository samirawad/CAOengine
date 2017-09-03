using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ConditionFramework
{
    // This is meant to record details about a participant in an occurance
    // In this occurence, what role is the participant playing?  What tags describe the action?
    // What ideals are being upheld?  What ideals are being forsaken?
    public class Role
    {
        public string[] Actions;
        public string[] Upholds;
        public string[] Forsakes;
    }
}
