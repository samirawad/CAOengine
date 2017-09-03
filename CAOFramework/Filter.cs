using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ConditionFramework
{
    public class Filter
    {
        public string Function;
        public JToken Params;

        public bool IsTrue(World w, JToken s)
        {
            // A filter is applied to a single JToken to deteremine if it is true
            return FilterFunctions[Function](w, Params, s);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="w">The world</param>
        /// <param name="p">parameters for the function</param>
        /// <param name="s">JArray: the elements are the selection to be acted upon</param>
        /// <returns></returns>
        private delegate bool termDel(World w, JToken p, JToken s);
        // We're gonna have to rework all of these functions to use the selection instead of the parameter.
        // Before, the parameter containted paths to find the objects we're manipulating, but now we're just going to
        // Find the objects using the selection.  Params will stil contain other info.
        private static Dictionary<string, termDel> FilterFunctions = new Dictionary<string, termDel>()
        {
            {"All", (w,p,s) =>
            {
                return true;
            }},
            {"Exists", (w,p,s) =>
            {
                int elementCount = (s as JArray).Count;
                return elementCount > 0;
            }},
            {"ArrayNotEmpty", (w,p,s) =>
            {
                string pathToCheck = p["PathToCheck"].Value<string>();
                var path = w.WorldDoc.SelectToken(pathToCheck);
                return path != null ? ((JArray) path).Count > 0 : false;
            }},
            {"ArrayContainsToken", (w,p,s) =>
            {
                string propertyToCheck = p["PropertyToCheck"].Value<string>();
                string valueToCheck = p["ValueToCheck"].Value<string>();
                return false; // not implemented.  We could use this as a general form of StringArrayContainsValue?
            }},
            {"StringArrayContainsValue", (w,p,s) =>
            {
                string propertyToCheck = p["PropertyToCheck"].Value<string>();
                string valueToCheck = p["ValueToCheck"].Value<string>();
                JArray arrayToCheck = (w.WorldDoc.SelectToken(propertyToCheck) as JArray);
                return arrayToCheck != null ? arrayToCheck.Any(v => v.Value<string>() == valueToCheck) : false;
            }},

            {"HasTag", (w,p,s) =>
            {
                // Check the selection, and make sure that there is at least one which doesent have the 
                // tag specified in the parameter
                string tagTocheck = p["Tag"].Value<string>();
                bool result = (s["Tags"] as JArray).Values().Contains(tagTocheck);
                return result;
            }},
            {"NotIn", (w,p,s) =>
            {
                // The specified item must not exist in any of the other groups under consideration
                var groupsToCheck = p["NotIn"];
                return false;
            }},
            {"HasValue", (w,p,s) =>
            {
                // The property of the selected value we wish to check
                JToken propertyToCheck = s.SelectToken("$.."+p["Property"].Value<string>());
                string valueToCheck = p["Value"].Value<string>();
                return propertyToCheck != null ? propertyToCheck.Value<string>() == valueToCheck : false;
            }},
            {"PropertyHasMinimumValue", (w,p,s) =>
            {
                string propertyToCheck = p["PropertyToCheck"].Value<string>();
                int valueToCheck = int.Parse(p["ValueToCheck"].Value<string>());
                return w.WorldDoc.SelectToken(propertyToCheck).Value<int>() >= valueToCheck;
            }},
            {"PropertyHasMaxValue", (w,p,s) =>
            {
                string propertyToCheck = p["PropertyToCheck"].Value<string>();
                int valueToCheck = int.Parse(p["ValueToCheck"].Value<string>());
                return w.WorldDoc.SelectToken(propertyToCheck).Value<int>() <= valueToCheck;
            }},
        };
    }


}
