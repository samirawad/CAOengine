using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConditionFramework
{
    public struct Task
    {
        public string Function;
        public JObject Parameters;
    }
    public class Outcome
    {
        public string ID;
        public string Requires;
        public string RequiresEmpty;
        public string[] ParentStages;
        public string SetsStage;
        public string Selection;  // These are named sets of objects which the tasks of the outcome operate on
        public List<Task> Steps;

        private delegate string outcomeDel(ref World w, Dictionary<string, string[]> t, JObject p);

        public bool IsValid(World g)
        {
            bool result = false;

            // The current stage can be set by any outcome.
            // An outcome can be deemed valid either by having a specified parent stage OR by having a valid ValidityCondition
            bool hasParentParameter = ParentStages != null;
            bool hasRequirement = Requires != null;
            bool hasParentParameterAndRequirement = hasRequirement && hasParentParameter;

            bool hasParent = hasParentParameter ? (ParentStages.Contains(g.OutcomeStage)) : false;
            bool requirementsMet = false;
            if (hasRequirement)
            {
                bool requiresClauseMet = true;
                bool requiresEmptyClauseMet = true;
                if (Requires != null)
                {
                    var selection = g.ActionLibrary.Selectors[Requires].Select(ref g);
                    requiresClauseMet = selection.Values.All(v => v.Length > 0) ? true : false;
                }
                if (RequiresEmpty != null)
                {
                    var selection = g.ActionLibrary.Selectors[RequiresEmpty].Select(ref g);
                    requiresEmptyClauseMet = selection.Values.All(v => v.Length == 0) ? true : false;
                }
                requirementsMet = requiresClauseMet && requiresEmptyClauseMet;
            }
            // If an outcome has both a parent and a requirement, they must BOTH be true to be valid.
            // If only one parameter exists, either having a parent or having a validity function, then either resolving to true
            // will cause the Outcome to be valid.
            result = hasParentParameterAndRequirement ? (hasParent && requirementsMet) : (hasParent || requirementsMet);
            return result;
        }

        //private static void UpdateRelations(JObject target, List<OccurrenceRole> participants, string memoryID)
        //{
        //    // A relation consists of path to another entity, under which is a collection of memory ids is kept.
        //    // It is meant to represent a collection of occurences that this entity (target) shares with all
        //    // participants in the occurence
        //    if (target?["Relations"] == null)
        //    {
        //        target.Add("Relations", new JArray());
        //    }

        //    // Each participant needs to update the relations of all the participants in the memory
        //    // A relation is a collection of memories under an entity
        //    foreach (OccurrenceRole currentRole in participants)
        //    {
        //        bool relationupdated = false;
        //        // Find the relation under the relations collection of this entity.
        //        foreach (JObject relation in target["Relations"].Children())
        //        {
        //            // We've found the relation, update it with the new memory by adding the id.
        //            //if (((string)relation["relationPath"]) == currentRole.RolePath)
        //            //{
        //            //    ((JArray)relation["Memories"]).Add(memoryID);
        //            //    relationupdated = true;
        //            //}
        //        }
        //        if (!relationupdated)
        //        {
        //            // This is a new relation, add it to the relations collection of this participant
        //            JObject newRelation = new JObject();
        //            //newRelation.Add("relationPath", currentRole.RolePath);
        //            newRelation.Add("Memories", new JArray());
        //            ((JArray)newRelation["Memories"]).Add(memoryID);
        //            ((JArray)target["Relations"]).Add(newRelation);
        //        }
        //    }
        //}


        public string PerformOutcomes(ref World world)
        {
            // Collect the selections for the outcome, and pass them to the functions
            string result = "";

            Dictionary<string, string[]> selectionPaths = world.ActionLibrary.Selectors[Selection].Select(ref world);

            foreach (var currentTask in Steps)
            {
                result += PerformOutcome(ref world, selectionPaths, currentTask.Function, currentTask.Parameters);
            };
            world.LastOutcome = this;
            world.OutcomeStage = SetsStage;
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="world">direct reference to the world document so it can be manipulated</param>
        /// <param name="selected">the collection of objects directly targeted by the outcome</param>
        /// <returns></returns>
        private string PerformOutcome(ref World world, Dictionary<string, string[]> selectedPaths, string function, JObject parameters)
        {
            // Each target has a selection which returns one or more Json objects.
            // For each selection, we can perform a sequence of actions to all the objects returned by the selection

            string result = OutcomeFunctions[function](ref world, selectedPaths, parameters);
            return result;
        }

        // Find and replace any values in the description string from the parameters or game objects
        public static string formatDescription(string toFormat, JObject p)
        {
            string result = toFormat;
            var tokensToReplace = Regex.Matches(toFormat, @"<([^<>]+)>");
            foreach (Match currMatch in tokensToReplace)
            {
                // replace any references to the parameter with the value of the parameter
                var replaceValue = p.SelectToken(currMatch.Groups[1].Value);
                if (replaceValue != null)
                {
                    result = result.Replace(currMatch.Groups[0].Value, (string)replaceValue);
                }
            }
            return result;
        }

        /*
         * If a 'ROLES' collection is found in the parameters, we know to update memories and relations for the entities of that collection.
         * Also, we can perform outcome functions on the items in the ROLES collection. 
         */

        // IF it resolves to a jtoken that means a single value, array or compound object.  
        private Dictionary<string, outcomeDel> OutcomeFunctions = new Dictionary<string, outcomeDel>()
        {
        { "Description", (ref World w, Dictionary<string, string[]> t, JObject p) =>{
            string Description = (string) p["Description"];
            Description = formatDescription(Description, p);
            return Description;
            } },
        { "IncrementProperty", (ref World w, Dictionary<string, string[]> t, JObject p) =>{
            // -- INCOMPLETE --
            string Description = (string) p["Description"];
            Description = formatDescription(Description, p);
            return Description;
         } },
        { "SetProperty", (ref World w, Dictionary<string, string[]> t, JObject p) =>{
            // -- INCOMPLETE --
            string Description = (string) p["Description"];
            Description = formatDescription(Description, p);
            return Description;
         } },
        { "RemoveProperty", (ref World w, Dictionary<string, string[]> t, JObject p) =>{
            // -- INCOMPLETE --
            string Description = (string) p["Description"];
            Description = formatDescription(Description, p);
            return Description;
         } },
         { "UpdateObjectToProperty", (ref World w, Dictionary<string, string[]> t, JObject p) =>{
            // TODO: the path variable is obsolete.  we pass the references in directly through t
            string template = (string) p["Template"];
            string path = (string) p["Path"];
            string property = (string) p["Property"];
            string Description = (string) p["Description"];
            Description = formatDescription(Description, p);
            var newObj = ObjectFactory.Create(w.Templates[template], p, w, null);
             if(w.WorldDoc.SelectToken(path)[property] == null)
             {
                 (w.WorldDoc.SelectToken(path) as JObject).Add(property, newObj);
             }
             else
             {
                 w.WorldDoc.ReplacePath<JToken>(path+"."+property, newObj);
             }
            return Description;
         } },
         { "MoveObjectToProperty", (ref World w, Dictionary<string, string[]> t, JObject p) =>{
            // TODO: the path variable is obsolete.  we pass the references in directly through t
            string source = (string) p["Source"];
            string destination = (string) p["Destination"];
            string property = (string) p["Property"];
            string Description = (string) p["Description"];
            Description = formatDescription(Description, p);
            var sourceObj = w.WorldDoc.SelectToken(source+"."+property);
            if(w.WorldDoc.SelectToken(destination)[property] == null)
             {
                 (w.WorldDoc.SelectToken(destination) as JObject).Add(property, sourceObj);
             }
             else
             {
                 w.WorldDoc.ReplacePath<JToken>(destination+"."+property, sourceObj);
             }
            w.WorldDoc.SelectToken(source).RemoveFields(new string[] { property });
            return Description;
         } },
        { "CreateObjectToCollection", (ref World w, Dictionary<string, string[]> t, JObject p) =>{
            // Create a number of objects with the specified template to the specified containter (bucket)
            string template = (string) p["Template"];
            string collection = (string) p["Collection"];
            string Description = (string) p["Description"];
            int Number = p?["Number"] == null ? 1 : (int) p["Number"];
            Description = formatDescription(Description, p);
            if(w.WorldDoc[collection] == null)
            {
                w.WorldDoc.Add(collection, new JArray());
            }
            for(int i=0;i<Number;i++)
            {
                var newObj= ObjectFactory.Create(w.Templates[template], p, w, null);
                (w.WorldDoc[collection] as JArray).Add(newObj);
            }
            return Description;
         } },
        { "AddTag", (ref World w, Dictionary<string, string[]> t, JObject p) =>{
            // Find the Tags collection of the object, and if the tag doesn't already exist, add it.
            string target = p["Target"].Value<string>();
            string[] paths = t[target];
            foreach(string currentPath in paths)
            {
                string collectionToAddTagTo = p["Property"].Value<string>();
                JArray currObj = (JArray) w.WorldDoc.SelectToken(currentPath+"."+collectionToAddTagTo);
                JToken ItemToAdd =  p["Value"];
            
                // Make sure that the value doesn't already exist.  That way we can make the JArray behave like a set.
                if(!currObj.Any(i => i.Equals(ItemToAdd)))
                {
                    currObj.Add(ItemToAdd);
                }
            }
            string Description = (string) p["Description"];
            return formatDescription(Description, p);
         } },
        { "RemoveTag", (ref World w, Dictionary<string, string[]> t, JObject p) =>{
            string target = p["Target"].Value<string>();
            string[] paths = t[target];
            foreach(string currentPath in paths)
            {
                string collectionToAddTagTo = p["Property"].Value<string>();
                JArray currObj = (JArray) w.WorldDoc.SelectToken(currentPath+"."+collectionToAddTagTo);
                JToken ItemToRemove =  p["Value"];
                // Remove the item
                currObj.Remove(ItemToRemove);
            }
            string Description = (string) p["Description"];
            return formatDescription(Description, p);
         } },
        { "ReplaceTag", (ref World w, Dictionary<string, string[]> t, JObject p) =>{
            // Find the Tags collection of the object, and if the tag doesn't already exist, add it.
            string target = p["Target"].Value<string>();
            string[] paths = t[target];
            foreach(string currentPath in paths)
            {
                string collectionToAddTagTo = p["Property"].Value<string>();
                JArray currObj = (JArray) w.WorldDoc.SelectToken(currentPath+"."+collectionToAddTagTo);
                JToken oldItem =  currObj.Where(o => o.Value<string>() == p["Value"].Value<string>()).First();
                JToken newItem =  p["NewValue"];
                currObj.Remove(oldItem);
                if(!currObj.Any(i => i.Equals(newItem)))
                {
                    currObj.Add(newItem);
                }
                else
                {
                    throw new Exception("Item already exists");
                }
            }
            string Description = (string) p["Description"];
            return formatDescription(Description, p);
         } },
        { "AddOccurence", (ref World w, Dictionary<string, string[]> t, JObject p) =>{
            /*
             *  TODO: The Occurence needs to be in a new format.  The type of occurence, so it can be judged,
             *  also the occurence upholds or forsakes ideals
             */
            
            
            // In this case, t represents the particpants in an occurence.

            // The occurence requires a unique id which can be referenced in the memory of participating entities
            string memoryID = System.Guid.NewGuid().ToString();
            string description = (string) p["Description"];

            // An occurence has a description and a set of roles.
            // Each role in an occurence is a path to a JObject in the world document,
            // with the details of that role as a JObject
            Occurrence newOccurence = new Occurrence()
            {
                Description = description,
                //Roles = new List<OccurrenceRole>()
            };

            // The dictionary t contains the names of the partcipating groups as the key, and the paths of all the participants as the value
            // We can use the key to obtain the details from the parameter object p.
            // We can then create the occurenceroles in the occurence
            foreach(string groupKey in t.Keys)
            {
                string[] groupMembers = t[groupKey];
                foreach(string memberPath in groupMembers)
                {

                }
            }

            w.History.Add(memoryID, newOccurence);
            return formatDescription(description, p);
        }}
        };
    }
}

