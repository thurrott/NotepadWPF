using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace NotepadWPF
{
    public partial class AboutBox : Window
    {
        public AboutBox()
        {
            InitializeComponent();

            // Set the window background color
            SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);

            // Populate the window title and labels in the About box
            this.Title = "About " + Application.Current.MainWindow.GetType().Assembly.GetName().Name;
            AppNameLabel.Content = Application.Current.MainWindow.GetType().Assembly.GetName().Name + " (WPF version)";
            VersionLabel.Content = "Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            CopyrightLabel.Content = "Copyright © " + System.DateTime.Now.Year + " " + FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).CompanyName;
            MJFLabel.Content = "Dedicated to Mary Jo Foley, the Queen of Notepad!";
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
