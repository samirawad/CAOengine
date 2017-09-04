using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConditionFramework
{
    public class Program
    {
        public static void log(string l)
        {
            Console.WriteLine(Environment.NewLine + l);
            File.AppendAllText("log.txt", l);
        }

        public static void Main(string[] args)
        {
            Test();
        }

        public static void Test()
        {
            string basedir = AppDomain.CurrentDomain.BaseDirectory + @"\data\";

            Judgement judgement = JsonConvert.DeserializeObject<Judgement>(File.ReadAllText(basedir + "opinions.json"));
            log("Judgements loaded");

            string judgementDesc = "";
            bool valid = JudgementCondition.JudgementIsValid(ref judgementDesc, judgement.HowToJudgeActor, null, null, null);
            Console.ReadKey(true);
        }

        public static void Run()
        {
            bool exit = false;

            World WorldEngine = new World();
            while (!exit)
            {
                if (!exit)
                {
                    // Show the possible actions
                    log(JsonConvert.SerializeObject(WorldEngine.WorldDoc, Formatting.Indented));
                    log(JsonConvert.SerializeObject(WorldEngine.History, Formatting.Indented));
                    log(WorldEngine.LastOutcomeLog);
                    string validEvents = WorldEngine.GetValidActions();

                    // If there is only one possible valid event, it's detail and potential outcomes are displayed instead.
                    // We just need the user to press a key to continue.

                    if (WorldEngine.CurrentValidActions.Count > 1)
                    {
                        log(validEvents);
                        var currCommand = Console.ReadKey(true);
                        exit = currCommand.Key == ConsoleKey.Escape ? true : false;
                        if (exit) break;
                        var commandChar = currCommand.KeyChar;
                        if (WorldEngine.IsActionValid(commandChar))
                        {
                            log(WorldEngine.ListValidActions(commandChar));
                            log("--------------------------------------------------------------");
                            log("Press enter to execute this event, any other key to abort.");
                            currCommand = Console.ReadKey(true);
                            if (currCommand.Key == ConsoleKey.Enter)
                            {
                                Console.Clear();
                                WorldEngine.DoGameAction(commandChar);
                            }
                        }
                        else
                        {
                            log(commandChar + " is not a valid selection.");
                        }
                    }
                    // only one valid event.  Show description, detail, possible outcomes, then just do it.
                    else if (WorldEngine.CurrentValidActions.Count == 1)
                    {
                        log(WorldEngine.CurrentValidActions.First().Value.Description);
                        log(WorldEngine.ListValidActions(WorldEngine.CurrentValidActions.First().Key));
                        log("--------------------------------------------------------------");
                        log("Press any key to continue.");
                        Console.ReadKey(true);
                        WorldEngine.DoGameAction(WorldEngine.CurrentValidActions.First().Key);
                    }
                    else // There is probably something wrong.
                    {
                        log("There are no valid events!");
                        Console.ReadKey(true);
                    }

                }
            }
        }

    }
}
