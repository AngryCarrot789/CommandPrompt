using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandPrompt.CommandPromptFiles
{
    public class CommandLine
    {
        public void Exit()         => Environment.Exit(0);
        public void Exit(int code) => Environment.Exit(code);

        public string[] SysInfo()
        {
            List<string> output = new List<string>();
            string[] drives = Environment.GetLogicalDrives();
            output.Add("Windows 10");
            output.Add(Environment.Is64BitOperatingSystem ? "64 Bit" : "probably 32 bit");
            output.Add($"{drives.Count()} Logical drives; they are:");
            foreach (string drive in drives)
                output.Add($"- {drive}");
            return output.ToArray();
        }

        public Action<string> ConsoleWrite { get; set; }
        public Action<string> ConsoleWriteLine { get; set; }
    }
}
