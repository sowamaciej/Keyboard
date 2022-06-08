using IWshRuntimeLibrary;
using System;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Keyboard
{
    public partial class Form1 : Form
    {

        private readonly String BACKSPACE_MARKER = "{BACKSPACE}";
        private readonly String ENTER_MARKER = "{ENTER}";
        private readonly WshShell shell;
        readonly SwipeType.SwipeType swipeType;
        readonly SwipeType.SwipeType distanceSwipeType;

        String acc;
        String lastLetter;
        String word;

        bool inputInProgress;

        public Form1()
        {
            shell = new WshShell();
            acc = "";
            swipeType = new SwipeType.MatchSwipeType(System.IO.File.ReadAllLines("EnglishDictionary.txt"));
            distanceSwipeType = new SwipeType.DistanceSwipeType(System.IO.File.ReadAllLines("EnglishDictionary.txt"));
            InitializeComponent();
        }

        //focus settings
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x08000000;
                return cp;
            }
        }

        private void LetterClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (inputInProgress)
            {
                inputInProgress = false;

                System.Collections.Generic.IEnumerable<string> suggestions = getSuggestions(swipeType, 10);
                int length = suggestions.Count();
                string firstSuggestion = length > 0 ? suggestions.ElementAt(0) : null;
                word = firstSuggestion;

                if (firstSuggestion != null)
                {
                    shell.SendKeys(firstSuggestion);
                    textBox1.Text = firstSuggestion;
                }

                //print all suggestions
                for (int i = 0; i < length; ++i)
                {
                    Debug.WriteLine($"M match {i + 1}: {suggestions.ElementAt(i)}");
                }

            }
            else
            {
                inputInProgress = true;
                acc = acc + btn.Text;
                lastLetter = btn.Text;
                textBox1.Text = "";
                word = null;
            }
        }

        private System.Collections.Generic.IEnumerable<string> getSuggestions(SwipeType.SwipeType swipeType, int suggestionsNum)
        {
          return  swipeType.GetSuggestion(acc, suggestionsNum);
        }

        private void LetterMouseEnter(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (inputInProgress && !lastLetter.Equals(btn.Text, StringComparison.OrdinalIgnoreCase))
            {
                acc = acc + btn.Text;
                lastLetter = btn.Text;
            }
        }

        private void buttonSendClick(object sender, EventArgs e)
        {
            SendString();
        }

        private void SendString()
        {
            shell.SendKeys(acc);
            System.Collections.Generic.IEnumerable<string> result = swipeType.GetSuggestion(acc, 10);

            int length = result.Count();
            for (int i = 0; i < length; ++i)
            {
                Debug.WriteLine($"M match {i + 1}: {result.ElementAt(i)}");
            }

            System.Collections.Generic.IEnumerable<string> result2 = distanceSwipeType.GetSuggestion(acc, 5);

            int length2 = result2.Count();
            for (int i = 0; i < length2; ++i)
            {
                Debug.WriteLine($"D match {i + 1}: {result2.ElementAt(i)}");
            }

            acc = "";
            lastLetter = null;
        }

        private void backspaceClick(object sender, EventArgs e)
        {
            SendKeys.Send(BACKSPACE_MARKER);
        }

        private void enterClick(object sender, EventArgs e)
        {
            SendKeys.Send(ENTER_MARKER);
        }

    }
}
