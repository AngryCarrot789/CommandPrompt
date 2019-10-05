using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandPrompt.CommandPromptFiles
{
    public class CommandResult
    {
        public enum ResultType
        {
            String, Int,
        }

        public string Text { get; set; }

        internal void AddTextLine(string text)
        {
            Text += $"{text}{Environment.NewLine}";
        }
    }
}
