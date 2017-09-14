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
            /*
             *  We'll need a new outcome which generates new actors into the world
             */
            World world = new World()
            {
                Agents = new List<Agent>()
                {
                    new Agent()
                    {
                        Name = "Usurper",
                        Tags = new List<string>() { "Ambitious" }
                    },
                    new Agent()
                    {
                        Name = "King",
                        Tags = new List<string>() { "Unjust", "Rich" }
                    },
                    new Agent()
                    {
                        Name = "Witness",
                        Tags = new List<string>() { "Ambitious", "Greedy" }
                    }
                }
            };
            // Loads the judgement data for envy
            Judgement judgement = JsonConvert.DeserializeObject<Judgement>(File.ReadAllText(Basedir + "envy.json"));

            /*
             *  Its imporatant to note than when a judgement is being calculated, it's from the perspective
             *  of the witness.  Regardless of if the Tyrant actually posesses the 'unjust' trait, the witness
             *  is drawing from their current relationship to make the judgement.  If the witness doesn't actually
             *  have a relationship to the  
             */
            Occurrence occurence = new Occurrence()
            {
                Description = "The ambitious Usurper siezes the throne from the unjust King!",
                Actor = "Usurper",
                Target = "King",
                Witness = "Witness",
                ActorRole = new Role()
                {
                    Actions = new List<string>() { "Dethrone" },
                    Upholds = new List<string>() { "Power" },
                    Forsakes = new List<string>() { },
                },
                TargetRole = new Role()
                {
                    Actions = new List<string>() { },
                    Upholds = new List<string>() { },
                    Forsakes = new List<string>() { },
                }
            };

            // Judge the occurence from the point of view of the witness
            world = judgement.FormJudgement("Witness", occurence, world);
            Agent judge = world.Agents.Find(p => p.Name == "Witness");

            // The judge should now have two relationships, one the actor and the target
            Assert.IsTrue(judge.Relationships.Count == 2);

            // The judge's relationships should include one which has the envy tag
            Assert.IsTrue(
                judge.Relationships["Usurper"].Opinions.Exists(opinion => {
                    return opinion.Judgement == "Envy";
                })
            );

            // Judge the entire occurence
            world = judgement.JudgeOccurence(occurence, world);
            
            

        }
    }
}
