using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConditionFramework
{
    /*
     *   Agents participate in occurences in the following ways:
     *   1. As the Actor, the one performing the action.
     *   2. As the Target, the one recieving the action.
     *   3. As a Witness.
     *   
     *   Agents apply opionions to all the participants in an occurrence, and hold a reference to the occurence
     */
    public class Agent
    {
        public string Name;

        public Dictionary<string, Relationship> Relationships;
        
        // For any particular occurence, multiple judgements may be valid.
        // The ordering of ideals allows us to determine which judgment has the 
        // greatest effect on this actor.
        public List<string> Ideals;

        // Tags affect which judgements are valid in an occurence.
        // For example: An actor with the 'kind' tag will likely form the 'contempt' relationship
        // with any actor who upholds 'cruelty' in an occurence.
        public List<string> Tags;
    }

}
