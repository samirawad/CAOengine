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
        string Basedir = AppDomain.CurrentDomain.BaseDirectory + @"\data\";

        [TestMethod]
        public void TestFormJudgement()
        {
            World world = new World()
            {
                Actors = new List<Actor>()
                {
                    new Actor(),
                    new Actor()
                }
            };

            // Loads the judgement data for envy
            Judgement judgement = JsonConvert.DeserializeObject<Judgement>(File.ReadAllText(Basedir + "opinions.json"));

            Actor judge = new Actor()
            {

            };

            Occurrence occurence = new Occurrence()
            {
                Description = "some occurence",
                Actors = new List<Actor>()
                {

                },
                Targets = new List<Actor>()
                {

                },
                Witnesses = new List<Actor>()
                {

                },
                ActorRole = new Role()
                {
                    Actions = new string[] { },
                    Upholds = new string[] { },
                    Forsakes = new string[] { },
                },
                TargetRole = new Role()
                {
                    Actions = new string[] { },
                    Upholds = new string[] { },
                    Forsakes = new string[] { },
                }
            };

            judge = judgement.FormJudgement(judge, occurence, world);

            // The judge should now have two relationships, one the actor and the target
            Assert.IsTrue(judge.Relationships.Count == 2);

            // The judge's relationships should include one which has the envy tag
            Assert.IsTrue(
                judge.Relationships.Exists(r =>
                {
                    return r.Opinions.Exists(o => { return o.Judgement == "Envy"; });
                })
            );

            // Judge the entire occurence
            world = judgement.JudgeOccurence(occurence, world);

        }
    }
}
