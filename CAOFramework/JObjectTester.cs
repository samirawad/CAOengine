using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace ConditionFramework
{
    public class JObjectTester
    {
        string datapath = @"C:\Users\sawad\Documents\visual studio 2015\Projects\ConditionFramework\src\ConditionFramework\data";
        public JObjectTester()
        {
            /*
             * Probably refactor this, we need access to the terms, conditions, actions and outcomes all in the same class.
             * Not the document though, because we'll be creating different libraries which all access the same document
             */
            Console.WriteLine("Testing...");
            JObject myJson = JObject.Parse(File.ReadAllText(Path.Combine(new string[] {datapath, "myjson.json" })));
            Console.WriteLine("JSON loaded");

            // Object Templates
            JObject templates = JObject.Parse(File.ReadAllText(Path.Combine(new string[] { datapath, "templates.json" })));

            WorldLibrary lib = JsonConvert.DeserializeObject<WorldLibrary>(File.ReadAllText(Path.Combine(new string[] { datapath, "gamelib.json" })));
            Console.WriteLine("Library loaded");

            World gw = new World();
            //bool checkName = lib.Terms["NameIsPlayer"].IsTrue(gw);
            //bool checkStr = lib.Terms["TooWeak"].IsTrue(gw);
            //bool checkInt = lib.Terms["TooDumb"].IsTrue(gw);
            //bool checkCond2 = lib.Conditions["SmartAndStrong"].IsTrue(gw);
            //bool checkAction = lib.Actions["GoAdventure"].IsValid(gw);

            Console.WriteLine("Checking done");
        }

        //public void testSaveLib()
        //{
        //    // Test saving
        //    WorldLibrary toSave = new WorldLibrary()
        //    {
        //        Terms = new Dictionary<string, WorldState>()
        //        {
        //            {"HasPropertyA", new WorldState()
        //            {
        //                TermFunction = "HasProperty",
        //                Parameters = new Dictionary<string, string>()
        //                {
        //                    {"propertyToCheck", "PropertyName" }
        //                }
        //            }}
        //        },
        //        Conditions = new Dictionary<string, WorldStateCondition>()
        //        {
        //            {"condition1", new WorldStateCondition()
        //            {
        //                ValidationMethod = "MustBeTrue",
        //                Terms = new List<string>()
        //                {
        //                    "term1"
        //                },
        //            }}
        //        },
        //        Actions = new Dictionary<string, Action>()
        //        {
        //            {"Action1", new Action()
        //            {
        //                ValidityCondition = "condition1",
        //                ParameterSelector = "selects for the parameter objects"
        //            }}
        //        },
        //        Outcomes = new Dictionary<string, Outcome>()
        //        {
        //            {"Outcome1", new Outcome()
        //            {

        //            }}
        //        }
        //    };

        //    File.WriteAllText(@"C:\jsondata\library.json", JsonConvert.SerializeObject(toSave));
        //    Console.WriteLine("Json written");
        //    Console.ReadKey();

        //}
    }
}
