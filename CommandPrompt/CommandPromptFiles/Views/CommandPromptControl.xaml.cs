using CommandPromptFiles.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommandPromptFiles.CommandPrompt.Views
{
    /// <summary>
    /// Interaction logic for CommandPromptControl.xaml
    /// </summary>
    public partial class CommandPromptControl : UserControl
    {
        public Action CloseTabCallback { get; set; }
        public CommandPromptViewModel ViewModel { get; set; }
        public CommandPromptControl()
        {
            InitializeComponent();
            ViewModel = new CommandPromptViewModel(this);
            ViewModel.CloseTabCallback = closeTabCallback;
            this.DataContext = ViewModel;
            iTextBox.Focus();
        }

        private void closeTabCallback()
        {
            CloseTabCallback?.Invoke();
        }
    }
}
