using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ConditionFramework
{
    /*
     * The result of a selection is a nested structure of named subsets
     */
    public class Group
    {
        public string[] From;
        public string[] NotFrom;
        public string Filter;
        public string How;
        public JObject Params;
    }
    public class Selector
    {
        public string How;                              // The manner of selection (random, single, etc)
        public string[] From;                           // Either another selector, or a direct JSONpath
        public Dictionary<string, Group> Groups;        //  
        public int Minimum;                             // The minimum number which must be returned.  If the minimum cannot be returned, nothing is.
        public JObject Params;                          // Filter parameters

        public Dictionary<string, string[]> Select(ref World w)
        {
            Dictionary<string, string[]> SourceDict = new Dictionary<string, string[]>();
            Dictionary<string, string[]> ResultDict = new Dictionary<string, string[]>();
            /*
             * The source dictionary uses the group names as keys, and the paths to all the selected items as values
             */
            foreach (string currFrom in From)
            {
                // The from clause specifies a list of Selectors.  These selectors are either defined in the source json, or 
                // are direct JSONpath statements
                if (w.ActionLibrary.Selectors.ContainsKey(currFrom))
                {
                    Dictionary<string, string[]> Subselect = w.ActionLibrary.Selectors[currFrom].Select(ref w);
                    foreach (string currKey in Subselect.Keys)
                    {
                        SourceDict.Add(currKey, Subselect[currKey]);
                    }
                }
                else
                {
                    // The current From clause isn't a subselection, it must be a JsonPath expression.
                    // We resolve it to the individual paths and save it.  
                    JToken source = w.WorldDoc.SelectToken(currFrom);
                    string[] sourcePaths = source != null ?  
                        source.Type == JTokenType.Array ?                   // does it resolve to a json array?
                        source.Children().Select(c => c.Path).ToArray() :   // Then we save the paths of the individual items.
                        new string[] { source.Path } :                      // Otherwise, we save the path of the returned object.
                        new string[] { };                                   // An empty array is returned if the from clause doesn't resolve
                    SourceDict.Add(currFrom, sourcePaths);
                }
            }

            /*
             *  Resolve the output dictionary.  The keys are the names of the subgroups of the selection, and the values are the path.
             *  Each group clause has a name and a filter that is applied to the results of the 'From' collection.  If the group clause is empty, the result is
             *  a dictionary with one key, using the name
             *  
             *  Absence of a Where clause implies 'All'
             *  
             *  When resolving the groups, some filters may refer to other groups, thus we pass in new groups as they are generated into
             *  the SourceDict.  This also means that we cannot refer to a group until it is created, so the order in which they are listed matters
             */

            // for each group, apply the filter to the source to get the collection of paths to save under the key
            foreach (string currGroupKey in Groups.Keys)
            {
                Group currGroup = Groups[currGroupKey];
                // keysToSearch refers to the Selections in the From clause, AND the currently generated groups from which we wish to populate this group.
                // A null 'From' clause in the group means we use all of the selections and groups
                // If the 'From' clause isn't null, then we restrict our search to the specified Selections.
                var keysToSearch = currGroup.From != null ? (SourceDict.Keys.Where(k => currGroup.From.Contains(k))) : SourceDict.Keys;
                
                List<string> filtered = new List<string>();
                string[] selected = new string[] { };
                foreach (string currKey in keysToSearch)
                {
                    string[] toFilter = SourceDict[currKey];
                    foreach (string currPath in toFilter)
                    {
                        // If it passes the filter, place it in the dictionary
                        bool tokenIsValid = false;
                        JToken currToken = w.WorldDoc.SelectToken(currPath);

                        // Find the current token and if it isn't null, try and pass it through the filters
                        if (currToken != null)
                        {
                            // If the token returned is a collection, we add the paths separately
                            if (currToken.Type == JTokenType.Array)
                            {
                                foreach (var currSubToken in currToken.Children())
                                {
                                    // Apply filter clause
                                    if (currGroup.Filter != null)
                                    {
                                        if (w.ActionLibrary.CompoundFilters.ContainsKey(currGroup.Filter))
                                        {
                                            tokenIsValid = w.ActionLibrary.CompoundFilters[currGroup.Filter].IsTrue(w, currSubToken);
                                        }
                                        else if (w.ActionLibrary.Filters.ContainsKey(currGroup.Filter))
                                        {
                                            tokenIsValid = w.ActionLibrary.Filters[currGroup.Filter].IsTrue(w, currSubToken);
                                        }
                                    }
                                    else
                                    {
                                        tokenIsValid = true;
                                    }

                                    // Check to make sure the token isn't contained in any of the groups specified
                                    // in the NotFrom clause
                                    if (currGroup.NotFrom != null)
                                    {
                                        foreach (string notgroup in currGroup.NotFrom)
                                        {
                                            if (SourceDict[notgroup].Contains(currSubToken.Path))
                                            {
                                                tokenIsValid = false;
                                                break;
                                            };
                                        }
                                    }

                                    if (tokenIsValid)
                                    {
                                        filtered.Add(currSubToken.Path);
                                        tokenIsValid = false;
                                    }
                                }
                            }
                            else // not an array
                            {
                                // Apply filter clause
                                if (currGroup.Filter != null)
                                {
                                    if (w.ActionLibrary.CompoundFilters.ContainsKey(currGroup.Filter))
                                    {
                                        tokenIsValid = w.ActionLibrary.CompoundFilters[currGroup.Filter].IsTrue(w, currToken);
                                    }
                                    else if (w.ActionLibrary.Filters.ContainsKey(currGroup.Filter))
                                    {
                                        tokenIsValid = w.ActionLibrary.Filters[currGroup.Filter].IsTrue(w, currToken);
                                    }
                                }
                                else
                                {
                                    tokenIsValid = true;
                                }

                                // Check to make sure the token isn't contained in any of the groups specified
                                // in the NotFrom clause
                                if(currGroup.NotFrom != null)
                                {
                                    foreach (string notgroup in currGroup.NotFrom)
                                    {
                                        if (SourceDict[notgroup].Contains(currPath))
                                        {
                                            tokenIsValid = false;
                                            break;
                                        };
                                    }
                                }

                                if (tokenIsValid)
                                {
                                    filtered.Add(currPath);
                                }
                            }
                        }
                    }
                    // If there is a HOW clause, select the items from the filtered collection using the appropriate function.
                    // Otherwise, return all the filtered items.
                    selected = currGroup.How != null ? SelectorLibrary.Selectors[currGroup.How](filtered.ToArray(), currGroup.Params) : filtered.ToArray();
                }
                ResultDict.Add(currGroupKey, selected);
                // Newly generated groups are added to the selection of groups we can choose to draw from
                // When other groups are generated
                SourceDict.Add(currGroupKey, selected);
            }
            return ResultDict;
        }
    }

    public delegate string[] selectorDel(string[] s, JObject p);

    public static class SelectorLibrary
    {
        private static Random RND = new Random();

        // Each of these functions returns an array of paths to the relevant items in the WorldDoc.
        // These paths act as direct references so we can manipulate the elements they point to.
        public static Dictionary<string, selectorDel> Selectors = new Dictionary<string, selectorDel>()
        {
            { "RndOne", (string[] s, JObject p) =>{
                List<string> resultList = new List<string>();
                if(s.Length > 0)
                {
                    int selectionCount = s.Count();
                    resultList.Add(s.ElementAt(RND.Next(selectionCount)));
                }
                return resultList.ToArray();
            } },
            { "RndMulti", (string[] s, JObject p) =>{
                List<string> resultList = new List<string>();
                if(s.Length > 0)
                {
                    // Takes between min and max values
                    int min = p["Min"].Value<int>();
                    int max = p["Max"].Value<int>();
                    // Max cannot be larger than the selection length
                    max = max > s.Length ? s.Length : max;
                    int numToTake = RND.Next(min,(max+1));
                    List<int> indiciesToTake = new List<int>();
                    int selectionCount = s.Count();
                    int toTake = RND.Next(RND.Next(selectionCount));
                    while(indiciesToTake.Count < numToTake)
                    {
                        if(!indiciesToTake.Contains(toTake))
                        {
                            indiciesToTake.Add(toTake);
                        }
                        toTake = RND.Next(selectionCount);
                    }
                    List<string> result = new List<string>();
                    foreach(int i in indiciesToTake)
                    {
                        resultList.Add(s.ElementAt(i));
                    }
                }
                return resultList.ToArray();
            } }
        };
    }
}
