using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NotepadWPF
{
    public partial class MainWindow : Window
    {
        // global variables
        bool TextHasChanged = false;
        string DocumentName = "";
        int ZoomValue = 100;
        double MasterFontSize;
        public double Scale;

        // global variables for Find and Replace
        string FindTextString = "";
        int FindLastIndexFound = 0;

        // Need for auto save
        readonly DispatcherTimer Timer1 = new DispatcherTimer();

        // For app scaling
        // from inchoatethoughts.com/scaling-your-user-interface-in-a-wpf-application
        #region ScaleValue Dependency Property
        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(MainWindow), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        private static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            MainWindow mainWindow = o as MainWindow;
            if (mainWindow != null)
                return mainWindow.OnCoerceScaleValue((double)value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MainWindow mainWindow = o as MainWindow;
            if (mainWindow != null)
                mainWindow.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual double OnCoerceScaleValue(double value)
        {
            if (double.IsNaN(value))
                return 1.0d;

            value = Math.Max(0.1, value);
            return value;
        }

        protected virtual void OnScaleValueChanged(double oldValue, double newValue)
        {

        }

        public double ScaleValue
        {
            get
            {
                return (double)GetValue(ScaleValueProperty);
            }
            set
            {
                SetValue(ScaleValueProperty, value);
            }
        }
        #endregion

        public void CalculateAppScale(double scale)
        {
            double value = Math.Min(scale, scale);
            ScaleValue = (double)OnCoerceScaleValue(MainGrid, value);
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AppWindow_Initialized(object sender, System.EventArgs e)
        {
            // Get application name from assembly
            AppWindow.Title = "Untitled - " + Application.Current.MainWindow.GetType().Assembly.GetName().Name;

            // Get window location from settings
            Application.Current.MainWindow.Left = Settings.Default.MyLocationX;
            Application.Current.MainWindow.Top = Settings.Default.MyLocationY;

            if (Settings.Default.MyMaximized == true)
                AppWindow.WindowState = WindowState.Maximized;
            else
                AppWindow.WindowState = WindowState.Normal;

            // Get window size from settings
            Application.Current.MainWindow.Width = Settings.Default.MyWidth;
            Application.Current.MainWindow.Height = Settings.Default.MyHeight;

            // Get text box fonts from settings
            string fontName = Settings.Default.MyFontFamily;
            TextBox1.FontFamily = new System.Windows.Media.FontFamily(fontName);
            TextBox1.FontSize = Settings.Default.MyFontSize;
            MasterFontSize = Settings.Default.MyFontSize;
            if (Settings.Default.MyFontBold)
                TextBox1.FontWeight = FontWeights.Bold;
            else
                TextBox1.FontWeight = FontWeights.Normal;
            if (Settings.Default.MyFontItalic)
                TextBox1.FontStyle = FontStyles.Italic;
            else
                TextBox1.FontStyle = FontStyles.Normal;

            // Get textbox foreground and background colors from settings
            TextBox1.Foreground = new BrushConverter().ConvertFromString(Settings.Default.MyForegroundColor) as SolidColorBrush;
            TextBox1.Background = new BrushConverter().ConvertFromString(Settings.Default.MyBackgroundColor) as SolidColorBrush;

            // Get Word Wrap from settings
            if (Settings.Default.MyWordWrap == true)
            {
                WordWrapMenu.IsChecked = true;
                TextBox1.TextWrapping = TextWrapping.Wrap;
            }
            else
            {
                WordWrapMenu.IsChecked = false;
                TextBox1.TextWrapping = TextWrapping.NoWrap;
            }

            // Get Status bar from settings
            if (Settings.Default.MyStatusBar == true)
            {
                StatusBar1.Visibility = Visibility.Visible;
                StatusBarMenu.IsChecked = true;
            }
            else
            {
                StatusBar1.Visibility = Visibility.Collapsed;
                StatusBarMenu.IsChecked = false;
            }

            // Get Auto Save from settings, configure timer
            Timer1.Interval = System.TimeSpan.FromSeconds(30);
            Timer1.Tick += new EventHandler(Timer1_Tick);
            if (Settings.Default.MyAutoSave)
            {
                AutoSaveText.Text = "Auto Save: On";
                AutoSaveMenu.IsChecked = true;
                Timer1.Start();
            }
            else
            {
                AutoSaveText.Text = "Auto Save: Off";
                AutoSaveMenu.IsChecked = false;
                Timer1.Stop();
            }

            // Get Scaling from settings
            Scale = Settings.Default.MyAppScale;
            CalculateAppScale(Scale);
            Scale100Menu.IsChecked = false;
            Scale110Menu.IsChecked = false;
            Scale125Menu.IsChecked = false;
            Scale150Menu.IsChecked = false;
            Scale175Menu.IsChecked = false;
            switch (Scale)
            {
                case 1:
                    Scale100Menu.IsChecked = true;
                    break;
                case 1.1:
                    Scale110Menu.IsChecked = true;
                    break;
                case 1.25:
                    Scale125Menu.IsChecked = true;
                    break;
                case 1.5:
                    Scale150Menu.IsChecked = true;
                    break;
                case 1.75:
                    Scale175Menu.IsChecked = true;
                    break;
            }

            // Make sure text box is selected so user can just start typing
            TextBox1.Focus();
        }

        private void AppWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            bool result = DisplaySavePrompt();
            if (result == false)
                e.Cancel = true;
        }

        private void SaveSettings()
        {
            // Save window location to settings
            if (AppWindow.WindowState == WindowState.Maximized)
                Settings.Default.MyMaximized = true;
            else
                Settings.Default.MyMaximized = false;

            Settings.Default.MyLocationX = Application.Current.MainWindow.Left;
            Settings.Default.MyLocationY = Application.Current.MainWindow.Top;

            // Save window size from settings
            Settings.Default.MyWidth = Application.Current.MainWindow.Width;
            Settings.Default.MyHeight = Application.Current.MainWindow.Height;

            // Save text box fonts to settings
            Settings.Default.MyFontFamily = TextBox1.FontFamily.ToString();
            Settings.Default.MyFontSize = MasterFontSize;
            if (TextBox1.FontWeight == FontWeights.Bold)
                Settings.Default.MyFontBold = true;
            else
                Settings.Default.MyFontBold = false;
            if (TextBox1.FontStyle == FontStyles.Italic)
                Settings.Default.MyFontItalic = true;
            else
                Settings.Default.MyFontItalic = false;

            // Save text box foreground and background colors
            Settings.Default.MyForegroundColor = TextBox1.Foreground.ToString();
            Settings.Default.MyBackgroundColor = TextBox1.Background.ToString();

            // Save text box Word Wrap configuration to settings
            Settings.Default.MyWordWrap = WordWrapMenu.IsChecked;

            // Save Status bar configuration to settings
            Settings.Default.MyStatusBar = StatusBarMenu.IsChecked;

            // Save Auto Save to settings, kill timer
            Timer1.Stop();
            Settings.Default.MyAutoSave = AutoSaveMenu.IsChecked;

            // Save Scaling
            Settings.Default.MyAppScale = Scale;

            // Save settings
            Settings.Default.Save();
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void PrintCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog
            {
                PageRangeSelection = PageRangeSelection.AllPages,
                UserPageRangeEnabled = true
            };

            bool? print = printDialog.ShowDialog();
            if (print == true)
            {
                FlowDocument flowDocument = new FlowDocument(new Paragraph(new Run(TextBox1.Text)))
                {
                    ColumnWidth = printDialog.PrintableAreaWidth
                };
                IDocumentPaginatorSource iDocumentPaginatorSource = flowDocument;
                printDialog.PrintDocument(iDocumentPaginatorSource.DocumentPaginator, DocumentName);
            }
        }

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextHasChanged == false)
            {
                AppWindow.Title = "*" + AppWindow.Title;
                TextHasChanged = true;
            }
            int Count = System.Text.RegularExpressions.Regex.Matches(TextBox1.Text, @"[\S]+").Count;
            WordCountText.Text = Count.ToString() + " word";
            if (Count != 1)
                WordCountText.Text += "s";

            ChangePositionText();
        }

        private void ChangePositionText()
        {
            int Line = TextBox1.GetLineIndexFromCharacterIndex(TextBox1.CaretIndex);
            int Column = TextBox1.CaretIndex - TextBox1.GetCharacterIndexFromLineIndex(Line);
            PositionText.Text = "Ln " + (Line + 1).ToString() + ", Col " + (Column + 1).ToString();
        }

        private void WordWrapMenu_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox1.TextWrapping == TextWrapping.NoWrap)
            {
                TextBox1.TextWrapping = TextWrapping.Wrap;
                WordWrapMenu.IsChecked = true;
            }
            else
            {
                TextBox1.TextWrapping = TextWrapping.NoWrap;
                WordWrapMenu.IsChecked = false;
            }
        }

        private void StatusBarMenu_Click(object sender, RoutedEventArgs e)
        {
            if (StatusBar1.Visibility == Visibility.Collapsed)
            {
                StatusBar1.Visibility = Visibility.Visible;
                StatusBarMenu.IsChecked = true;
            }
            else
            {
                StatusBar1.Visibility = Visibility.Collapsed;
                StatusBarMenu.IsChecked = false;
            }
        }

        private void ZoomCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            switch (e.Parameter)
            {
                case "In":
                    if (ZoomValue <= 500)
                    {
                        ZoomValue += 10;
                        TextBox1.FontSize = (MasterFontSize * ZoomValue) / 100;
                        ZoomText.Text = ZoomValue.ToString() + "%";
                    }
                    break;
                case "Out":
                    if (ZoomValue > 10)
                    {
                        ZoomValue -= 10;
                        TextBox1.FontSize = (MasterFontSize * ZoomValue) / 100;
                        ZoomText.Text = ZoomValue.ToString() + "%";
                    }
                    break;
                case "Restore":
                    TextBox1.FontSize = MasterFontSize;
                    ZoomText.Text = "100%";
                    ZoomValue = 100;
                    break;
            }
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool result = DisplaySavePrompt();

            if (result == true)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        TextBox1.Text = File.ReadAllText(openFileDialog.FileName);
                        AppWindow.Title = openFileDialog.SafeFileName + " - " + Application.Current.MainWindow.GetType().Assembly.GetName().Name;
                        TextHasChanged = false;
                        DocumentName = openFileDialog.FileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void TextBox1_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ChangePositionText();
        }

        private void Save()
        {
            if (DocumentName.Length > 0)
            {
                try
                {
                    File.WriteAllText(DocumentName, TextBox1.Text);
                    TextHasChanged = false;
                    AppWindow.Title = AppWindow.Title.Replace("*", "");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                SaveAs();
            }
        }

        private void SaveAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    DocumentName = saveFileDialog.FileName;
                    AppWindow.Title = Path.GetFileNameWithoutExtension(DocumentName) + "- " + Application.Current.MainWindow.GetType().Assembly.GetName().Name;
                    TextHasChanged = false;
                    File.WriteAllText(saveFileDialog.FileName, TextBox1.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Save();
        }

        private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAs();
        }

        private void BlackOnWhiteMenu_Click(object sender, RoutedEventArgs e)
        {
            TextBox1.Foreground = Brushes.Black;
            TextBox1.Background = Brushes.White;
        }

        private void BlackOnLightGrayMenu_Click(object sender, RoutedEventArgs e)
        {
            TextBox1.Foreground = Brushes.Black;
            TextBox1.Background = Brushes.LightGray;
        }

        private void AmberOnBlackMenu_Click(object sender, RoutedEventArgs e)
        {
            TextBox1.Foreground = Brushes.Orange;
            TextBox1.Background = Brushes.Black;
        }

        private void GreenOnBlackMenu_Click(object sender, RoutedEventArgs e)
        {
            TextBox1.Foreground = Brushes.LightGreen;
            TextBox1.Background = Brushes.Black;
        }

        private void TextColorMenu_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBox1.Foreground = new SolidColorBrush(Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B));
            }
            cd.Dispose();
        }

        private void BackgroundColorMenu_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextBox1.Background = new SolidColorBrush(Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B));
            }
            cd.Dispose();
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool result = false;
            if (TextHasChanged)
            {
                result = DisplaySavePrompt();
            }

            if (result == false)
                return;

            TextBox1.Text = "";
            TextHasChanged = false;
            DocumentName = "";
            Application.Current.MainWindow.Title = "Untitled - " + Application.Current.MainWindow.GetType().Assembly.GetName().Name;
        }

        private void NewWindowMenu_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));

            //ProcessStartInfo processStartInfo = new ProcessStartInfo
            //{
            //    FileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName),
            //    UseShellExecute = true
            //};
            //Process.Start(processStartInfo);
        }

        private bool DisplaySavePrompt()
        {
            if (TextHasChanged == true)
            {
                MessageBoxResult SavePrompt = MessageBox.Show("Do you want to save changes before closing?", Application.Current.MainWindow.GetType().Assembly.GetName().Name, MessageBoxButton.YesNoCancel);
                switch (SavePrompt)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        return true;
                    case MessageBoxResult.No:
                        return true;
                    case MessageBoxResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        private void FontMenu_Click(object sender, RoutedEventArgs e)
        {
            // Save settings so the Font dialog has the correct font info
            SaveSettings();

            // With settings saved, open the Font dialog
            FontDialog fontDialog = new FontDialog
            {
                Owner = this
            };
            fontDialog.ShowDialog();

            // When the dialog closes, change the font settings if necessary
            if (fontDialog.DialogResult == true)
            {
                // FontFamily
                string fontName = Settings.Default.MyFontFamily;
                TextBox1.FontFamily = new System.Windows.Media.FontFamily(fontName);
                // FontSize
                TextBox1.FontSize = Settings.Default.MyFontSize;
                MasterFontSize = Settings.Default.MyFontSize;
                // Bold (FontWeight)
                if (Settings.Default.MyFontBold)
                    TextBox1.FontWeight = FontWeights.Bold;
                else
                    TextBox1.FontWeight = FontWeights.Normal;
                // Italic (FontStyle)
                if (Settings.Default.MyFontItalic)
                    TextBox1.FontStyle = FontStyles.Italic;
                else
                    TextBox1.FontStyle = FontStyles.Normal;
            }
        }

        private void AboutMenu_Click(object sender, RoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox
            {
                Owner = this
            };
            aboutBox.ShowDialog();
        }

        private void SearchWithBingCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = true
            };

            if (TextBox1.SelectedText.Length >= 0)
                processStartInfo.FileName = "https://www.bing.com/search?q=" + TextBox1.SelectedText;
            else
                processStartInfo.FileName = "https://www.bing.com";

            Process.Start(processStartInfo);
        }

        private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TextBox1.SelectAll();
        }

        private void TimeDateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Obtain the time/date, insert it, move the selection point to the end
            System.DateTime now = System.DateTime.Now;
            TextBox1.SelectedText = now.ToShortTimeString() + " " + now.ToShortDateString();
            TextBox1.SelectionStart += (now.ToShortTimeString() + " " + now.ToShortDateString()).Length;
            TextBox1.SelectionLength = 0;
        }

        private void FindTheText()
        {
            if (FindLastIndexFound > -1)
                TextBox1.Select(FindLastIndexFound, FindTextString.Length);
            else
                MessageBox.Show(this, "Cannot find " + (char)34 + FindTextString + (char)34, Application.Current.MainWindow.GetType().Assembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void FindTextIndex(int FindFromIndex, bool FindPreviousIndex)
        {
            string text = TextBox1.Text;

            if (FindPreviousIndex == false)
            {
                FindLastIndexFound = text.IndexOf(FindTextString, FindFromIndex);
                if (FindLastIndexFound == -1)
                {
                    // If text is not found, try searching from the beginning
                    FindLastIndexFound = text.IndexOf(FindTextString, 0);
                }
            }
            else
            {
                FindLastIndexFound = text.LastIndexOf(FindTextString, FindFromIndex);
                if (FindLastIndexFound == -1)
                {
                    //  If text is not found, try searching from the end
                    FindLastIndexFound = text.LastIndexOf(FindTextString, text.Length - 1);
                }
            }
        }

        private void FindCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InputBox inputBox = new InputBox
            {
                Title = "Find"
            };
            inputBox.QuestionLabel.Content = "Find what:";

            bool? input = inputBox.ShowDialog();
            if (input == true)
            {

                FindTextString = inputBox.AnswerTextBox.Text;
                FindTextIndex(TextBox1.SelectionStart, false);
                FindTheText();
            }
        }

        private void FindNextCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (FindTextString.Length > 0)
            {
                FindTextIndex(FindLastIndexFound + FindTextString.Length, false);
                FindTheText();
            }
        }

        private void FindPreviousCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (FindTextString.Length > 0)
            {
                FindTextIndex(FindLastIndexFound, true);
                FindTheText();
            }
        }

        private void ReplaceCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InputBox inputBox = new InputBox
            {
                Title = "Replace"
            };
            inputBox.QuestionLabel.Content = "Find what:";

            inputBox.ShowDialog();
            FindTextString = inputBox.AnswerTextBox.Text;

            InputBox inputBox2 = new InputBox
            {
                Title = "Replace All"
            };
            inputBox2.QuestionLabel.Content = "Replace With:";

            inputBox2.ShowDialog();
            string ReplaceWith = inputBox2.AnswerTextBox.Text;

            // Find text from current cursor position
            FindTextIndex(TextBox1.SelectionStart, false);

            if (FindLastIndexFound > -1)
                TextBox1.Text = TextBox1.Text.Substring(0, FindLastIndexFound) + ReplaceWith + TextBox1.Text.Substring(FindLastIndexFound + FindTextString.Length);
            else
                MessageBox.Show(this, "Cannot find " + (char)34 + FindTextString + (char)34, Application.Current.MainWindow.GetType().Assembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReplaceAllCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InputBox inputBox = new InputBox
            {
                Title = "Replace All"
            };
            inputBox.QuestionLabel.Content = "Find what:";

            bool? input = inputBox.ShowDialog();
            if (input == true)
            {
                FindTextString = inputBox.AnswerTextBox.Text;

                InputBox inputBox2 = new InputBox
                {
                    Title = "Replace All"
                };
                inputBox2.QuestionLabel.Content = "Replace With:";

                bool? input2 = inputBox2.ShowDialog();
                if (input2 == true)
                {
                    string ReplaceWith = inputBox2.AnswerTextBox.Text;

                    FindTextIndex(0, false);

                    if (FindLastIndexFound > -1)
                    {
                        string NewText = Microsoft.VisualBasic.Strings.Replace(TextBox1.Text, FindTextString, ReplaceWith, 1);
                        TextBox1.Text = NewText;
                    }
                    else
                        MessageBox.Show(this, "Cannot find " + (char)34 + FindTextString + (char)34, Application.Current.MainWindow.GetType().Assembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void GoToCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InputBox inputBox = new InputBox
            {
                Title = "Go to line"
            };
            inputBox.QuestionLabel.Content = "Line number:";

            bool? input = inputBox.ShowDialog();
            if (input == true)
            {
                try
                {
                    int LineNum = System.Convert.ToInt32(inputBox.AnswerTextBox.Text);
                    if (LineNum <= TextBox1.LineCount)
                    {
                        TextBox1.SelectionStart = TextBox1.GetCharacterIndexFromLineIndex(LineNum - 1);
                        TextBox1.SelectionLength = 0;
                        TextBox1.ScrollToLine(LineNum - 1);
                    }
                    else
                    {
                        MessageBox.Show("The line number is beyond the total number of lines", "Go to line");
                        GoToCommand_Executed(this, e);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    GoToCommand_Executed(this, e);
                }
            }
        }

        private void AutoSaveMenu_Click(object sender, RoutedEventArgs e)
        {
            if (AutoSaveMenu.IsChecked == true)
            {
                if (MessageBox.Show(this, "Click OK to disable Auto Save.", "Disable Auto Save", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    AutoSaveText.Text = "Auto Save: Off";
                    AutoSaveMenu.IsChecked = false;
                    Timer1.Stop();
                }
            }
            else
            {
                if (MessageBox.Show(this, "Click OK to automatically save your document every 30 seconds.", "Enable Auto Save", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    AutoSaveText.Text = "Auto Save: On";
                    AutoSaveMenu.IsChecked = true;
                    Timer1.Start();
                }
            }
        }

        void Timer1_Tick(object sender, EventArgs e)
        {
            if (TextHasChanged)
            {
                ApplicationCommands.Save.Execute("Save", this);
            }
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Keyboard.PrimaryDevice != null)
            {
                if (Keyboard.PrimaryDevice.ActiveSource != null)
                {
                    KeyEventArgs keyEventArgs = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Delete)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    };
                    InputManager.Current.ProcessInput(keyEventArgs);
                }
            }
        }

        // Derived from this Stack Overflow post:
        // stackoverflow.com/questions/52621314/wpf-drag-and-drop-text-file-into-application
        private void TextBox1_Drop(object sender, DragEventArgs e)
        {
            if (TextHasChanged)
            {
                bool result = DisplaySavePrompt();
                if (result == false)
                    return;
            }

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                string extension = Path.GetExtension(files[0]);
                if (extension == ".txt" || extension == ".text")
                {
                    if (files != null && files.Length > 0)
                    {
                        TextBox1.Text = File.ReadAllText(files[0]);
                        string filename = Path.GetFileNameWithoutExtension(files[0]);

                        AppWindow.Title = filename + " - " + Application.Current.MainWindow.GetType().Assembly.GetName().Name;
                        TextHasChanged = false;
                        DocumentName = filename;
                    }
                }
                else
                    MessageBox.Show("Drag and drop supports plain text (*.txt, *.text) files only.", Application.Current.MainWindow.GetType().Assembly.GetName().Name, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void TextBox1_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void Scale100Menu_Click(object sender, RoutedEventArgs e)
        {
            Scale = 1;
            CalculateAppScale(Scale);
            Scale100Menu.IsChecked = true;
            Scale110Menu.IsChecked = false;
            Scale125Menu.IsChecked = false;
            Scale150Menu.IsChecked = false;
            Scale175Menu.IsChecked = false;
        }

        private void Scale110Menu_Click(object sender, RoutedEventArgs e)
        {
            Scale = 1.1;
            CalculateAppScale(Scale);
            Scale100Menu.IsChecked = false;
            Scale110Menu.IsChecked = true;
            Scale125Menu.IsChecked = false;
            Scale150Menu.IsChecked = false;
            Scale175Menu.IsChecked = false;
        }

        private void Scale125Menu_Click(object sender, RoutedEventArgs e)
        {
            Scale = 1.25;
            CalculateAppScale(Scale);
            Scale100Menu.IsChecked = false;
            Scale110Menu.IsChecked = false;
            Scale125Menu.IsChecked = true;
            Scale150Menu.IsChecked = false;
            Scale175Menu.IsChecked = false;
        }

        private void Scale150Menu_Click(object sender, RoutedEventArgs e)
        {
            Scale = 1.5;
            CalculateAppScale(Scale);
            Scale100Menu.IsChecked = false;
            Scale110Menu.IsChecked = false;
            Scale125Menu.IsChecked = false;
            Scale150Menu.IsChecked = true;
            Scale175Menu.IsChecked = false;
        }

        private void Scale175Menu_Click(object sender, RoutedEventArgs e)
        {
            Scale = 1.75;
            CalculateAppScale(Scale);
            Scale100Menu.IsChecked = false;
            Scale110Menu.IsChecked = false;
            Scale125Menu.IsChecked = false;
            Scale150Menu.IsChecked = false;
            Scale175Menu.IsChecked = true;
        }
    }
}
