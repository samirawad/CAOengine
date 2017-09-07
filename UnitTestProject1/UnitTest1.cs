using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConditionFramework;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestJudgement()
        {
            var World = new World()
            {
                Actors = new List<Actor>()
                {
                    new Actor(),
                    new Actor()
                }
            };

            // Loads the judgement data for envy
            string basedir = AppDomain.CurrentDomain.BaseDirectory + @"\data\";
            Judgement judgement = JsonConvert.DeserializeObject<Judgement>(File.ReadAllText(basedir + "opinions.json"));

            Actor judge = new Actor()
            {

            };

            Occurrence occurence = new Occurrence()
            {
                Description = "some occurence",
                Actor = new Role()
                {
                    Actions = new string[] { },
                    Upholds = new string[] { },
                    Forsakes = new string[] { },
                },
                Target = new Role()
                {
                    Actions = new string[] { },
                    Upholds = new string[] { },
                    Forsakes = new string[] { },
                }
            };

            judge = judgement.FormJudgement(judge, occurence, World);

            // The judge should now have two relationships, one the actor and the target
            Assert.IsTrue(judge.Relationships.Count == 2);

            // The judge's relationships should include one which has the envy tag
            Assert.IsTrue(
                judge.Relationships.Exists(r =>
                {
                    return r.Opinions.Exists(o => { return o.Judgement == "Envy"; });
                })    
            );
            
        }
    }
}
