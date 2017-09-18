using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace ConditionFramework
{
    // Behold, the smallest game engine evah... hmm not so small now?
    public class World
    {
        private char[] Alphabet = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

        public Random RND = new Random();

        // Every game turn, an action must be selected by the player.  We can reference it here.
        public Action CurrentAction = null;

        public Outcome LastOutcome = null;

        public string OutcomeStage;

        public string LastOutcomeLog;

        public Dictionary<char, Action> CurrentValidActions = new Dictionary<char, Action>();

        public WorldLibrary ActionLibrary;

        public JObject Templates;

        public JObject WorldDoc; // Stores all game state that isn't stored on actors

        public List<Judgement> Judgements; // Library of functions which control how judgements are made

        public Dictionary<string, Occurrence> History = new Dictionary<string, Occurrence>();

        public World()
        {
            // If we move this to AWS, we can read these files from an S3 bucket instead, or perhaps 
            // from a dynamodb?  Could be a mobile game.  I will eventually need to partner with good writers.
            string basedir = AppDomain.CurrentDomain.BaseDirectory + @"\data";
            string actionDir = basedir + @"\actions";
            string templateDir = basedir + @"\templates";
            string judgementDir = basedir + @"\judgements";

            // The game world
            JObject myJson = JObject.Parse(File.ReadAllText(Path.Combine(new string[] { basedir, "world.json" })));
            Console.WriteLine("JSON loaded");
            WorldDoc = myJson;

            // Object Templates
            if (!Directory.Exists(templateDir)) throw new Exception("No judgement subdir exists!  Please place judgements in a subdirectory called 'judgements'.");
            string[] templateFiles = Directory.GetFiles(templateDir, "*.json");
            if (templateFiles.Length == 0) throw new Exception("No templates found!");
            JObject templates = JObject.Parse(File.ReadAllText(Path.Combine(new string[] { templateDir, templateFiles[0] })));
            for (int i = 1; i < templateFiles.Length; i++)
            {
                var currTemplateFile = JObject.Parse(File.ReadAllText(Path.Combine(new string[] { templateDir, templateFiles[i] })));
                foreach (var currTemplate in currTemplateFile.Children())
                {
                    templates.Add(currTemplate);
                }
            }
            Templates = templates;

            string[] judgementFiles = Directory.GetFiles(judgementDir, "*.json");
            if (judgementFiles.Length == 0) throw new Exception("No judgements found!");
            Judgements = new List<Judgement>();
            for (int i = 1; i < judgementFiles.Length; i++)
            {
                string currJudgementFile  = Path.Combine(new string[] { actionDir, judgementFiles[i] });
                Judgement currentJudgement = JsonConvert.DeserializeObject<Judgement>(File.ReadAllText(currJudgementFile));
                Judgements.Add(currentJudgement);
            }

            // The game libraries can be split across multiple files in the action directory.
            // They are all loaded into the ActionLibrary Actions dictionary.
            string[] actionFiles = Directory.GetFiles(actionDir, "*.json");
            string currActionFile = Path.Combine(new string[] { actionDir, actionFiles[0] });
            ActionLibrary = JsonConvert.DeserializeObject<WorldLibrary>(File.ReadAllText(currActionFile));
            for (int i = 1; i < actionFiles.Length; i++)
            {
                currActionFile = Path.Combine(new string[] { actionDir, actionFiles[i] });
                WorldLibrary currLib = JsonConvert.DeserializeObject<WorldLibrary>(File.ReadAllText(currActionFile));
                foreach (var currSelector in currLib.Selectors)
                {
                    ActionLibrary.Selectors.Add(currSelector.Key, currSelector.Value);
                }
                foreach (var currFilter in currLib.Filters)
                {
                    ActionLibrary.Filters.Add(currFilter.Key, currFilter.Value);
                }
                foreach (var currFilterCondition in currLib.CompoundFilters)
                {
                    ActionLibrary.CompoundFilters.Add(currFilterCondition.Key, currFilterCondition.Value);
                }
                foreach (var currAction in currLib.Actions)
                {
                    ActionLibrary.Actions.Add(currAction.Key, currAction.Value);
                }
                foreach (var currOutcome in currLib.Outcomes)
                {
                    ActionLibrary.Outcomes.Add(currOutcome.Key, currOutcome.Value);
                }
            }

            // Populate the id fields of the actions
            foreach (string currAction in ActionLibrary.Actions.Keys)
            {
                ActionLibrary.Actions[currAction].ID = currAction;
            }

            foreach (string currOutcome in ActionLibrary.Outcomes.Keys)
            {
                ActionLibrary.Outcomes[currOutcome].ID = currOutcome;
            }

            Console.WriteLine("Libraries loaded");
        }

        // Return a string describing which events are currently valid, while populating the CurrentValidEvents dictionary.
        public string GetValidActions()
        {
            StringBuilder sbResult = new StringBuilder();
            int keyIndex = 0;
            CurrentValidActions.Clear();
            foreach (Action currAction in ActionLibrary.Actions.Values)
            {
                if (currAction.IsValid(this))
                {
                    CurrentValidActions.Add(Alphabet[keyIndex], currAction);
                    sbResult.AppendLine(Alphabet[keyIndex] + ": " + currAction.Description);
                    keyIndex++;
                }
            }
            return sbResult.ToString();
        }

        public string ListValidActions(char eventKey)
        {
            return CurrentValidActions[eventKey].ListOutcomes(this);
        }

        public bool IsActionValid(char eventKey)
        {
            bool isValid = CurrentValidActions.ContainsKey(eventKey);
            return isValid;
        }

        public string DoGameAction(char eventKey)
        {
            CurrentAction = CurrentValidActions[eventKey];
            OutcomeStage = CurrentAction.SetsStage;
            LastOutcomeLog = CurrentValidActions[eventKey].SelectOutcome(this);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return LastOutcomeLog;
        }
    }


}
