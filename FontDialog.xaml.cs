using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace NotepadWPF
{
    public partial class FontDialog : Window
    {
        public FontDialog()
        {
            InitializeComponent();

            // Set the window background color
            SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);

            // Set the correct font properties in the sample text
            string fontName = Settings.Default.MyFontFamily;
            SampleLabel.FontFamily = new System.Windows.Media.FontFamily(fontName);
            SampleLabel.FontSize = Settings.Default.MyFontSize;
            if (Settings.Default.MyFontBold)
                SampleLabel.FontWeight = FontWeights.Bold;
            else
                SampleLabel.FontWeight = FontWeights.Normal;
            if (Settings.Default.MyFontItalic)
                SampleLabel.FontStyle = FontStyles.Italic;
            else
                SampleLabel.FontStyle = FontStyles.Normal;

            // Populate Font Style list box
            List<string> fontStyle = new List<string>
            {
                "Normal", "Italic", "Bold", "Bold Italic"
            };
            FontStyleList.DataContext = fontStyle;

            // Populate Font Size list box
            List<double> fontSize = new List<double>
            {
                8,9,10,11,12,14,16,18,20,22,24,26,28,36,48,72
            };
            FontSizeList.DataContext = fontSize;

            // Select correct item in Font list box
            for (int x = 0; x <= Fonts.SystemFontFamilies.Count; x++)
                if (FontList.Items[x].ToString() == SampleLabel.FontFamily.ToString())
                {
                    FontList.SelectedItem = FontList.Items[x];
                    FontList.ScrollIntoView(FontList.Items[x]);
                    break;
                }

            // Select correct item in Font style list box
            if (SampleLabel.FontStyle == FontStyles.Normal)
                FontStyleList.SelectedIndex = 0;
            if (SampleLabel.FontStyle == FontStyles.Italic)
                FontStyleList.SelectedIndex = 1;
            if (SampleLabel.FontWeight == FontWeights.Bold)
                FontStyleList.SelectedIndex = 2;
            if (SampleLabel.FontWeight == FontWeights.Bold && SampleLabel.FontStyle == FontStyles.Italic)
                FontStyleList.SelectedIndex = 3;

            // Select correct item in Size list box
            for (int x = 0; x < fontSize.Count; x++)
                if (fontSize[x].ToString() == SampleLabel.FontSize.ToString())
                {
                    FontSizeList.SelectedIndex = x;
                    FontSizeList.ScrollIntoView(fontSize[x]);
                    break;
                }

            // Give the focus to the Font list box
            FontList.Focus();
        }

        private void FontList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string fontName = FontList.SelectedItem.ToString();
            SampleLabel.FontFamily = new FontFamily(fontName);
        }

        private void FontStyleList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (FontStyleList.SelectedItem.ToString())
            {
                case "Normal":
                    SampleLabel.FontStyle = FontStyles.Normal;
                    SampleLabel.FontWeight = FontWeights.Normal;
                    break;
                case "Italic":
                    SampleLabel.FontStyle = FontStyles.Italic;
                    SampleLabel.FontWeight = FontWeights.Normal;
                    break;
                case "Bold":
                    SampleLabel.FontStyle = FontStyles.Normal;
                    SampleLabel.FontWeight = FontWeights.Bold;
                    break;
                case "Bold Italic":
                    SampleLabel.FontStyle = FontStyles.Italic;
                    SampleLabel.FontWeight = FontWeights.Bold;
                    break;
                default:
                    SampleLabel.FontStyle = FontStyles.Normal;
                    SampleLabel.FontWeight = FontWeights.Normal;
                    break;
            }
        }

        private void FontSizeList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string size = FontSizeList.SelectedItem.ToString();
            SampleLabel.FontSize = Convert.ToDouble(size);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Save fonts to settings
            Settings.Default.MyFontFamily = SampleLabel.FontFamily.ToString();
            Settings.Default.MyFontSize = SampleLabel.FontSize;
            if (SampleLabel.FontWeight == FontWeights.Bold)
                Settings.Default.MyFontBold = true;
            else
                Settings.Default.MyFontBold = false;
            if (SampleLabel.FontStyle == FontStyles.Italic)
                Settings.Default.MyFontItalic = true;
            else
                Settings.Default.MyFontItalic = false;

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
