using CommandPrompt;
using CommandPrompt.CommandPromptFiles;
using CommandPrompt.Utilities;
using CommandPrompt.ViewModels;
using CommandPromptFiles.CommandPrompt.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CommandPromptFiles.ViewModels
{
    public class CommandPromptViewModel : BaseViewModel
    {
        public bool UseConsoleAPI { get; set; }
        public Action CloseTabCallback { get; set; }
        private CommandPromptControl Tab;
        private CommandLine CMD = new CommandLine();
        private string outputBufferText;
        private string inputText;
        private double outputFontSize;
        private double inputFontSize;
        private Brush backgroundColour;
        private Brush textColour;
        //Serial stuff
        private SerialPort sPort;
        bool AllowSerialReading;
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

        public double OutputFontSize { get { return outputFontSize; } set { outputFontSize = value; RaisePropertyChanged(); } }
        public double InputFontSize { get { return inputFontSize; } set { inputFontSize = value; RaisePropertyChanged(); } }

        public Brush BackgroundColour { get => backgroundColour; set { backgroundColour = value; RaisePropertyChanged(); } }
        public Brush TextColour { get => textColour; set { textColour = value; RaisePropertyChanged(); } }

        public CommandPromptViewModel(CommandPromptControl view)
        {
            Tab = view;
            SendCommandCommand = new Command(SendCommand);
            UseConsoleAPI = false;
            OutputFontSize = 12;
            InputFontSize = 16;
            BackgroundColour = new SolidColorBrush(Colors.Transparent);
            TextColour = new SolidColorBrush(Colors.White);

            WriteOutputLine("Command Prompt (with tabs)");
        }

        /// <summary>
        /// Uses the input text to send a command to the "command processing bits and bobs" (aka, you dont need to give it params)
        /// </summary>
        public void SendCommand()
        {
            if (string.IsNullOrEmpty(InputText))
            {
                WriteOutputLine("> (Input Empty)");
                return;
            }

            string preparedCommand = InputText.ToLower();
            WriteOutputLine($"> {preparedCommand}");
            ClearInput();
            try
            {
                if (!UseConsoleAPI)
                {
                    switch (preparedCommand)
                    {
                        //Helper Commands
                        case "help":
                            WriteOutputLine("-------------- Helper Commands ---------------");
                            WriteOutputLine("Help      - Displays every command");
                            WriteOutputLine("Clear     - Clears the output");
                            WriteOutputLine("Cls       - Clears the output");
                            WriteOutputLine("--------------- Real Commands ----------------");
                            WriteOutputLine("Info      - Displays info about this computer");
                            WriteOutputLine("----------------- UI Commands ----------------");
                            WriteOutputLine("Background(Colour) - Sets the background colour");
                            WriteOutputLine("TextColour(Colour) - Sets the text colour");
                            WriteOutputLine("-------------- Serial Commands ---------------");
                            WriteOutputLine("Listports          - Lists all COM ports on this computer");
                            WriteOutputLine("Start(port, baud)  - Opens a serial 'link' thingy");
                            WriteOutputLine("Closeport          - Closes the current open serial link thingy");
                            WriteOutputLine("Write(text)        - Writes text to the current open serial port");
                            WriteOutputLine("read(yes or no)    - Sets whether any text received gets written to the output or not.");
                            WriteOutputLine("-------------- Program Commands --------------");
                            WriteOutputLine("Consoleapi - Changes the input 'redirection' from internal to Console API (aka use cmd not custom)");
                            WriteOutputLine("Close      - Closes this tab");
                            WriteOutputLine("Exit       - Closes the entire program");
                            break;
                        case "clear":
                        case "cls":
                            ClearOutput();
                            break;
                        //Real Commands
                        case "info":
                            foreach (string infoln in CMD.SysInfo())
                                WriteOutputLine(infoln);
                            break;
                        //UI Commands
                        case var f when StringExtension.Before(preparedCommand, "(").Trim() == "background":
                            System.Drawing.Color culurBcg = System.Drawing.Color.FromName(StringExtension.Between(preparedCommand, "(", ")"));
                            BackgroundColour = new SolidColorBrush(Color.FromRgb(culurBcg.R, culurBcg.G, culurBcg.B));
                            break;
                        case var fg when StringExtension.Before(preparedCommand, "(").Trim() == "textcolour":
                            System.Drawing.Color culurTxt = System.Drawing.Color.FromName(StringExtension.Between(preparedCommand, "(", ")"));
                            TextColour = new SolidColorBrush(Color.FromRgb(culurTxt.R, culurTxt.G, culurTxt.B));
                            break;
                        //Serial Commands
                        case "listports":
                            foreach (string port in SerialPort.GetPortNames())
                                WriteOutputLine(port);
                            break;

                        case var strt when preparedCommand.Before("(").Trim() == "start":
                            string portName = StringExtension.Between(preparedCommand, "(", ",");
                            int baudRate = Convert.ToInt32(StringExtension.Between(preparedCommand, ",", ")"));
                            try
                            {
                                sPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
                                sPort.Open();
                                sPort.DataReceived += SPort_DataReceived;
                                WriteOutputLine($"Opened SerialPort on {portName}, Baud {baudRate}");
                            }
                            catch { WriteOutputLine($"Failed to open serial port on {portName}, Baud {baudRate}"); }
                            break;
                        case "closeport":
                            try
                            { sPort.Close(); WriteOutputLine("Closed current serial port"); }
                            catch (Exception exc) { WriteOutputLine($"Couldn't close serial port. Error: {exc.Message}"); }
                            break;
                        case var tbr when preparedCommand.Before("(").Trim() == "write":
                            string textToWrite = StringExtension.Between(preparedCommand, "(", ")");
                            try
                            { sPort.WriteLine(textToWrite); WriteOutputLine($"TX> {textToWrite}"); }
                            catch (Exception rip) { WriteOutputLine($"Failed to write to port. Error: {rip.Message}"); }
                            break;
                        case var yetoread when preparedCommand.Before("(").Trim() == "read":
                            switch (StringExtension.Between(preparedCommand, "(", ")").ToLower())
                            {
                                case "yes":
                                    AllowSerialReading = true;
                                    break;
                                case "true":
                                    AllowSerialReading = true;
                                    break;

                                case "no":
                                    AllowSerialReading = false;
                                    break;
                                case "false":
                                    AllowSerialReading = false;
                                    break;
                            }
                            break;
                        //Program Commands
                        case "consoleapi":
                            string msg = "";
                            if (UseConsoleAPI)
                            {
                                msg = "Console API Enabled. Disabling it.";
                                UseConsoleAPI = false;
                            }
                            else
                            {
                                msg = "Console API Disabled. Enabling it.";
                                UseConsoleAPI = true;
                            }
                            WriteOutputLine(msg);
                            break;
                        case "close":
                            CloseTabCallback?.Invoke();
                            break;
                        case "exit":
                            CMD.Exit();
                            break;
                        default:
                            WriteOutputLine($"Cannot find command: '{preparedCommand}");

                            //Process cmd = new Process();
                            //cmd.StartInfo.FileName = "cmd.exe";
                            //cmd.StartInfo.RedirectStandardOutput = true;
                            //cmd.StartInfo.RedirectStandardInput = true;
                            //cmd.StartInfo.UseShellExecute = false;
                            //cmd.StartInfo.CreateNoWindow = true;
                            //cmd.Start();
                            //Task.Run(() =>
                            //{
                            //    cmd.StandardInput.WriteLine(preparedCommand);
                            //    Task.Delay(1000);
                            //    StreamReader reader = cmd.StandardOutput;
                            //    string line;
                            //    while ((line = reader.ReadLine()) != null)
                            //    {
                            //        WriteOutputLine(line);
                            //    }
                            //    cmd.Kill();
                            //});
                            break;
                    }
                }
                else
                {
                    if (preparedCommand == "consoleapi")
                    {
                        string msg = "";
                        if (UseConsoleAPI)
                        {
                            msg = "Console API Enabled. Disabling it.";
                            UseConsoleAPI = false;
                        }
                        else
                        {
                            msg = "Console API Disabled. Enabling it.";
                            UseConsoleAPI = true;
                        }
                        WriteOutputLine(msg);
                        return;
                    }

                    //Process cmd = new Process();
                    //cmd.StartInfo.FileName = "cmd.exe";
                    //cmd.StartInfo.RedirectStandardOutput = true;
                    //cmd.StartInfo.RedirectStandardInput = true;
                    //cmd.StartInfo.UseShellExecute = false;
                    //cmd.StartInfo.CreateNoWindow = true;
                    //cmd.Start();
                    //Task.Run(() =>
                    //{
                    //    cmd.StandardInput.WriteLine(preparedCommand);
                    //    Task.Delay(1000);
                    //    StreamReader reader = cmd.StandardOutput;
                    //    string line;
                    //    while ((line = reader.ReadLine()) != null)
                    //    {
                    //        WriteOutputLine(line);
                    //    }
                    //    cmd.Kill();
                    //});
                }
            }
            catch (Exception g) { WriteOutputLine($"Big boi error in actual code... whhaaat?. Err:{g.Message}, -__-"); }
        }

        private void SPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (AllowSerialReading)
            {
                try { string text = sPort.ReadLine(); WriteOutputLine($"RX> {text}"); }
                catch(Exception ex) { WriteOutputLine($"Couldn't read line from serial port. Error: {ex.Message}"); }
            }
        }

        public void WriteOutput(string text)     { App.Current.Dispatcher.InvokeAsync(() => { OutputBufferText += text; }); }
        public void WriteOutputLine(string text) { App.Current.Dispatcher.InvokeAsync(() => { OutputBufferText += $"{text}{Environment.NewLine}"; }); }

        public void ClearOutput() => OutputBufferText = "";
        public void ClearInput()  => InputText = "";
    }
}
