﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConditionFramework
{
    public static class ObjectFactory
    {
        private static Random RND = new Random();

        // p is the template parameter for creating an object
        public static JToken Create(JToken template, JObject p, World w, JToken g)
        {
            var result = template.DeepClone();
            var global = g == null ? result : g;
            bool first = true;
            if (result.Type == JTokenType.String)
            {
                string strValue = result.Value<string>();

                // If the result is the name of a creation parameter, return that parameter to be embedded into the created object
                if(p?["CreationParams"]?[strValue] != null)
                {
                    return p["CreationParams"][strValue];
                }

                // If the result is a string which is the name of another template, return the object generated by that
                // template, otherwise, return the value
                return w.Templates[strValue] != null ? Create(w.Templates[strValue], p, w, global) : result;
            }
            if (result.Type == JTokenType.Array)
            {
                // If the result is an array, return the createobject result for each item in the array
                JArray newArray = new JArray();
                foreach (var currValue in result)
                {
                    newArray.Add(Create(currValue, p, w, global));
                }
                return newArray;
            }
            else
            {
                foreach (var currProperty in result.Children<JProperty>())
                {
                    if (first) //If the first property is a selection function, pass to the function which generates the appropriate jtoken
                    {
                        first = false;
                        if (SelectorFunctions.ContainsKey(currProperty.Name))
                        {
                            return Create(SelectorFunctions[currProperty.Name](w, currProperty.Value, global), p, w, global);
                        }
                    }
                    if (currProperty.Value != null)
                    {
                        currProperty.Value = Create(currProperty.Value, p, w, global);
                    }
                }
            }
            return result;
        }

        // p: The parameters
        // g: A reference to another Jtoken which can be used in decision making.  Ususally this is a reference to the
        // current JToken being created in the Create function.
        private delegate JToken selectorDel(World w, JToken p, JToken g);
        private static Dictionary<string, selectorDel> SelectorFunctions = new Dictionary<string, selectorDel>()
        {
            {"SwitchOnValues", (w,p,g) =>
            {
                JArray switchvalues = (JArray) p["switches"];
                foreach(var currswitchvalue in switchvalues)
                {
                    if(g.SelectToken(currswitchvalue["path"].Value<string>()).Equals(currswitchvalue["value"]))
                    {
                        return currswitchvalue["return"];
                    }
                }
                throw new Exception("switch values did not reslove");
            }},
            {"MarkovStr", (w,p,g)=>
            {
                // Generate a markov string
                string basedir = AppDomain.CurrentDomain.BaseDirectory + @"\data\";
                int order = p["order"].Value<int>();
                int minlength = p["minlength"].Value<int>();
                string dataFile = p["datafile"].Value<string>();
                string basePath = basedir;

                var data = File.ReadAllLines(basePath + dataFile);
                MarkovGenerator markovgen = new MarkovGenerator(RND, data, order, minlength);
                return markovgen.NextName;
            }},
            {"SelectOne", (w,p,g)=>
            {
                JArray selectFrom = (JArray) p["selection"];
                JToken result = selectFrom[RND.Next(selectFrom.Count)];
                return result;
            }},
            {"SubSelection", (w,p,g)=>
            {
                int min = p["min"].Value<int>();
                int max = p["max"].Value<int>();
                int numToTake = RND.Next(min,(max+1));
                JArray selectFrom = (JArray) p["selection"];
                JArray result = new JArray();
                for(int i=0; i<numToTake; i++)
                {
                    var selected = selectFrom[RND.Next(selectFrom.Count)];
                    while(result.Any(j => j.Value<string>() == selected.Value<string>())) // in case of collision
                    {
                        selected = selectFrom[RND.Next(selectFrom.Count)];
                    }
                    result.Add(selected);
                }
                return result;
            }},
            {"ChancePerItem", (w,p,g)=>
            {
                int chance = p["chance"].Value<int>();
                int outof = p["outof"].Value<int>();
                JArray result = new JArray();
                JArray selectFrom = (JArray) p["selection"];
                foreach(var currItem in selectFrom)
                {
                    if(RND.Next(outof) < chance)
                    {
                        result.Add(currItem);
                    }
                }
                return result;
            }}
        };
    }
}
