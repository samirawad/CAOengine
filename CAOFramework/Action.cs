using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ConditionFramework
{
    public delegate string EventTextDelegate(World world);

    /*
     * The idea behind a game action is that it's selectable.  It's available based on the state of the game world, and representes a decision point 
     * for the player.
     * 
     * Determining if an action is available, or an outcome is possible is broken down into two parts.
     * - A set of entities which must be selected for checking
     * - Checking the status / relationships of these entities.
     */
    public class Action
    {
        public string ID;
        public string Requires;             // If not null, this selection must return a non-empty collection for the action to be valid
        public string RequiresEmpty;        // If not null, this selection must return an empty collection for the action to be valid.
        public string ParameterSelector;
        public string Description;
        public string SetsStage;

        public bool IsValid(World g)
        {
            // A requires clause means that all the groups of a selection must be non-empty
            // A requiresempty clause means that the groups of a seletion must be empty
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
                requiresClauseMet = selection.Values.All(v => v.Length == 0) ? true : false;
            }
            return requiresClauseMet && requiresEmptyClauseMet;
        }

        public string ListOutcomes(World g)
        {
            // List the possible outcomes for this action
            // We don't know what the current stage is, so we temporarily replace it with the stage of this action
            StringBuilder sbOutput = new StringBuilder();

            string tempStage = g?.OutcomeStage;
            g.OutcomeStage = SetsStage;
            string[] validOutcomes = g.ActionLibrary.Outcomes.Where(o => o.Value.IsValid(g)).Select(s => s.Key).ToArray();
            foreach (string currOutcome in validOutcomes)
            {
                sbOutput.AppendLine(currOutcome);
            }
            g.OutcomeStage = tempStage;

            return sbOutput.ToString();
        }

        public string SelectOutcome(World g)
        {
            bool debug = false;
            /*
             *  Outcomes generally chain together by OutcomeStage, though they may be dependant upon validity conditions as well
             *  the world performed, we need to ensure that no further outcomes are valid and require performing.
             */
            Outcome[] validOutcomes = g.ActionLibrary.Outcomes.Values.Where(o => o.IsValid(g)).ToArray();
            StringBuilder result = new StringBuilder();
            if (debug)
            {
                Console.WriteLine("Valid outcomes for " + ID + ":");
                foreach (var currOutcome in validOutcomes)
                {
                    Console.WriteLine(currOutcome.ID);
                }
                Console.WriteLine("----------------------------------");
            }
            
            while (validOutcomes.Length > 0)
            {
                // When multiple outcomes are valid, one is simply selected randomly
                // In the future I was thinking about adding weights to outcomes so that
                // some are more likely than others.
                Outcome selected = validOutcomes[g.RND.Next(validOutcomes.Length)];

                // The outcome is performed.  This is what actually alters the gameworld state.
                // The selected outcome is also saved, so we can potentially refer to it.
                g.LastOutcome = selected;
                string outcome = selected.PerformOutcomes(ref g);
                Console.WriteLine(outcome);
                Console.ReadKey();
                result.AppendLine(outcome);

                // Determine any additional outcomes
                validOutcomes = g.ActionLibrary.Outcomes.Values.Where(o => o.IsValid(g)).ToArray();

                if (debug)
                {
                    Console.WriteLine("Valid outcomes for " + selected.ID + ":");
                    foreach (var currOutcome in validOutcomes)
                    {
                        Console.WriteLine(currOutcome.ID);
                    }
                    Console.WriteLine("----------------------------------");
                }
            }
            return result.ToString();
        }
    }
}
