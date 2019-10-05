using CommandPrompt.CommandPromptFiles;
using CommandPrompt.Utilities;
using CommandPrompt.ViewModels;
using System;
using System.Windows.Input;

namespace CommandPromptFiles.ViewModels
{
    public class CommandPromptViewModel : BaseViewModel
    {
        private string outputBufferText;
        private string inputText;

        /// <summary>
        /// The main output of the command promp view (aka the big textbox)
        /// </summary>
        public string OutputBufferText { get { return outputBufferText; } set { outputBufferText = value; RaisePropertyChanged(); } }
        /// <summary>
        /// The main input of the command prompt view (aka the textbox)
        /// </summary>
        public string InputText { get { return inputText; } set { inputText = value; RaisePropertyChanged(); } }

        /// <summary>
        /// The command triggered by a button or something to "process" a command
        /// </summary>
        public ICommand SendCommandCommand { get; set; }

        public CommandPromptViewModel()
        {
            SendCommandCommand = new Command(SendCommand);
        }

        /// <summary>
        /// Uses the input text to send a command to the "command processing bits and bobs" (aka, you dont need to give it params)
        /// </summary>
        public void SendCommand()
        {
            string text = InputText;
            var callback = CommandLine.MatchCommand(text);
            WriteOutputLine(callback.ToString());
        }

        public void WriteOutput(string text)     { OutputBufferText += text; }
        public void WriteOutputLine(string text) { OutputBufferText += $"{text}{Environment.NewLine}"; }

        public void ClearOutput() => OutputBufferText = "";
        public void ClearInput()  => InputText = "";
    }
}
