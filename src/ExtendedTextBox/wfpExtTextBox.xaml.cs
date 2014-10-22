namespace ExtendedTextBox
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for wfpExtTextBox.xaml
    /// </summary>
    public partial class wfpExtTextBox : UserControl
    {
        #region Constructors

        /// <summary>
        /// Auto generated initialiser
        /// </summary>
        public wfpExtTextBox()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Events

        public event TextChangedEventHandler TextChanged
        {
            add { theTextBox.TextChanged += value; }
            remove { theTextBox.TextChanged -= value; }
        }


        #endregion Events

        #region Properties

        /// <summary>
        /// Allows the textbox to accept the return character
        /// </summary>
        public bool AcceptsReturn
        {
            get { return theTextBox.AcceptsReturn; }
            set { theTextBox.AcceptsReturn = value; }
        }

        /// <summary>
        /// Allows the tab character to work in the textbox
        /// </summary>
        public bool AcceptsTab
        {
            get { return theTextBox.AcceptsTab; }
            set { theTextBox.AcceptsTab = value; }
        }

        public Brush BackColor
        {
            get { return theTextBox.Background; }
            set { theTextBox.Background = value; }
        }

        /// <summary>
        /// Force the case of the textbox
        /// </summary>
        public CharacterCasing CharacterCase
        {
            get { return theTextBox.CharacterCasing; }
            set { theTextBox.CharacterCasing = value; }
        }

        /// <summary>
        /// Enable / disable the textbox
        /// </summary>
        public bool Enabled
        {
            get { return theTextBox.IsEnabled; }
            set { theTextBox.IsEnabled = value; }
        }

        /// <summary>
        /// Maximum length of the text
        /// </summary>
        public int MaxLength
        {
            get { return theTextBox.MaxLength; }
            set { theTextBox.MaxLength = value; }
        }

        /// <summary>
        /// Turn the spell checking on and off
        /// </summary>
        public bool SpellCheck
        {
            get { return theTextBox.SpellCheck.IsEnabled; }
            set { theTextBox.SpellCheck.IsEnabled = value; }
        }

        /// <summary>
        /// set the contents of the textbox
        /// </summary>
        public string Text
        {
            get { return theTextBox.Text; }
            set { theTextBox.Text = value; }
        }

        /// <summary>
        /// Alignment of the text within the textbox
        /// </summary>
        public TextAlignment TextAlignment
        {
            get { return theTextBox.TextAlignment; }
            set { theTextBox.TextAlignment = value; }
        }

        /// <summary>
        /// Turn text wrapping on and off
        /// </summary>
        public TextWrapping TextWrapping
        {
            get { return theTextBox.TextWrapping; }
            set { theTextBox.TextWrapping = value; }
        }

        public ScrollBarVisibility VerticalScrollBar
        {
            get { return theTextBox.VerticalScrollBarVisibility; }
            set {theTextBox.VerticalScrollBarVisibility = value; }
        }

        #endregion Properties
    }
}