using System.Windows;

namespace NotepadWPF
{
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();

            // Set the window background color
            SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);

            AnswerTextBox.Focus();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
