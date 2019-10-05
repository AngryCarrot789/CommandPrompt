using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandPrompt.CommandPromptFiles
{
    public class CommandLine
    {
        public enum CommandsList
        {
            Help,
        }

        public static CommandResult MatchCommand(string command)
        {
            string inCommand = command.ToLower();
            CommandResult cr = new CommandResult();
            switch (inCommand)
            {
                case "help":
                {
                    //foreach(string val in Enum.GetNames(typeof(CommandsList)))
                    //cr.AddTextLine($"{val} - ")

                    cr.AddTextLine("Help - Lists all Commands");
                    cr.AddTextLine("Move(loc1)#(loc2) - Moves a file from loc1 to loc2");

                } break;

                case var e when inCommand.Substring(0, 4) == "move":
                {

                } break;

                case "close": break;
            }
            return cr;
        }
    }
}
