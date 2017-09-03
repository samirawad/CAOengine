using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConditionFramework
{
    /*
     *   Actors participate in occurences in the following ways:
     *   1. As the Actor, the one doing the action.
     *   2. As the Target, the one recieving the action.
     *   3. As a Witness.
     *   
     *   Actors apply opionions to all the participants in an occurnece, and hold a reference to the occurence
     */
    public class Actor
    {
        public string Name;
        public List<Relationship> Relationships;
        public List<string> Ideals;
        public List<string> Tags;
    }

}
