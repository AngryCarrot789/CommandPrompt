using CommandPrompt.Utilities;
using CommandPromptFiles.CommandPrompt.Views;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CommandPrompt.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainWindow Window;
        private ObservableCollection<TabItem> tabs = new ObservableCollection<TabItem>();
        private int selectedIndex;

        public ObservableCollection<TabItem> Tabs { get => tabs; set { tabs = value; RaisePropertyChanged(); } }
        public int SelectedIndex { get => selectedIndex; set { selectedIndex = value; RaisePropertyChanged(); } }

        public ICommand NewTabCommand { get; set; }

        public MainViewModel(MainWindow window)
        {
            Window = window;
            NewTabCommand = new Command(NewTab);
            NewTab();
        }

        public void NewTab()
        {
            TabItem ti = new TabItem();
            CommandPromptControl cpc = new CommandPromptControl();
            cpc.CloseTabCallback = CloseTab;
            ti.Content = cpc;
            ti.Header = $"Console {Tabs.Count}";

            Tabs.Add(ti);
        }

        public void CloseTab()
        {
            Tabs.RemoveAt(selectedIndex);
        }
    }
}
