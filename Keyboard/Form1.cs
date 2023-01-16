using IWshRuntimeLibrary;
using System;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;

namespace Keyboard
{
    public partial class Form1 : Form
    {

        private readonly String BACKSPACE_MARKER = "{BACKSPACE}";
        private readonly String ENTER_MARKER = "{ENTER}";
        private readonly String SPACE_MARKER = " ";

        private readonly String ENGLISH_DICT = "EnglishDictionary.txt";
        private readonly String POLISH_DICT = "PolishDictionary2.txt";

        private readonly WshShell shell;
        readonly SwipeType.SwipeType swipeType;
        readonly SwipeType.SwipeType distanceSwipeType;
        readonly List<string> phrases = new List<string>();

        String acc;
        String lastLetter;
        String word;

        bool inputInProgress;

        bool swypeMode;

        public Form1()
        {
            shell = new WshShell();
            acc = "";
            swypeMode = true;
            swipeType = new SwipeType.MatchSwipeType(System.IO.File.ReadAllLines(ENGLISH_DICT));
            distanceSwipeType = new SwipeType.DistanceSwipeType(System.IO.File.ReadAllLines(ENGLISH_DICT));
            addPhrases();
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

            if (swypeMode == false)
            {
                if (checkBox1.Checked == true || checkBox2.Checked == true)
                {
                    SendKeys.Send(btn.Text.ToUpper());
                    checkBox1.Checked = false;
                } else
                {
                    SendKeys.Send(btn.Text.ToLower());
                }
                return;
            }



            if (inputInProgress)
            {
                inputInProgress = false;

                System.Collections.Generic.IEnumerable<string> suggestions = GetSuggestions(swipeType, 8);
                int length = suggestions.Count();
                string firstSuggestion = length > 0 ? suggestions.ElementAt(0) : null;
                word = firstSuggestion;

                if (firstSuggestion != null)
                {

                    if (checkBox2.Checked == true)
                    {
                        word = word.ToUpper();
                    } else if (checkBox1.Checked == true)
                    {
                        word = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word.ToLower());
                        checkBox1.Checked = false;
                    }

                    shell.SendKeys(word + SPACE_MARKER);
                    textBox1.Text = word;
                }

                AddSuggestionsButtons(suggestions);

                //print all suggestions to debug console
                /*
                for (int i = 0; i < length; ++i)
                {
                    Debug.WriteLine($"M match {i + 1}: {suggestions.ElementAt(i)}");
                }
                */

                acc = "";
                lastLetter = null;
            }
            else
            {
                RemoveSuggestionButtons();
                inputInProgress = true;
                acc = acc + btn.Text;
                lastLetter = btn.Text;
                textBox1.Text = "";
                word = null;
            }
        }

        private void AddSuggestionsButtons(IEnumerable<string> suggestions)
        {
            int length = suggestions.Count();

            for (int i = 1; i < length; ++i)
            {

                string text = suggestions.ElementAt(i);

                if (checkBox2.Checked == true)
                {
                    text = text.ToUpper();
                } else if (checkBox1.Checked == true)
                {
                    text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
                    checkBox1.Checked = false;
                }

                Button btn = new Button();
                btn.Text = text;
                btn.Name = "suggestion";
                btn.Size = new Size(50, 50);
                btn.Location = new Point(50*i, 50);
                btn.MouseClick += SuggestionClick;
                Controls.Add(btn);
            }
        }

        private void SuggestionClick(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            RemoveSuggestionButtons();
            shell.SendKeys(String.Concat(Enumerable.Repeat(BACKSPACE_MARKER, word.Length + 1)));

            word = btn.Text;
            textBox1.Text = "";
            shell.SendKeys(word + SPACE_MARKER);
        }

        private void RemoveSuggestionButtons()
        {
            Control[] controls = Controls.Find("suggestion", true);
            
            foreach (Control control in controls)
            {
                Controls.Remove(control);
            }
        }

        private System.Collections.Generic.IEnumerable<string> GetSuggestions(SwipeType.SwipeType swipeType, int suggestionsNum)
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

        private void SpaceClick(object sender, EventArgs e)
        {
            if (inputInProgress)
            {
                inputInProgress = false;
                String singleLetter = acc.Substring(0, 1);
                shell.SendKeys(singleLetter.ToLower() + SPACE_MARKER);
                acc = "";
                word = null;
                lastLetter = null;
            }
            else
            {
                SendKeys.Send(SPACE_MARKER);
            }
        }

        private void BackspaceClick(object sender, EventArgs e)
        {
            SendKeys.Send(BACKSPACE_MARKER);
        }

        private void EnterClick(object sender, EventArgs e)
        {
            RemoveSuggestionButtons();
            SendKeys.Send(ENTER_MARKER);
        }

        private void ChangeMode(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (swypeMode)
            {
                swypeMode = false;
                btn.Text = "swype";
            }
            else
            {
                swypeMode = true;
                btn.Text = "normal";
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            SendKeys.Send("?");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SendKeys.Send(".");
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Label label = (Label)sender;
            label.Text = getRandomPhrase();
        }

        private string getRandomPhrase()
        {
            Random r = new Random();

            int index = r.Next(0, phrases.Count);

            return phrases.ElementAt(index);
        }


        private void addPhrases()
        {
            phrases.Add("I like it.");
            phrases.Add("Did that happen?");
            phrases.Add("See you soon!");
            phrases.Add("Can you handle?");
            phrases.Add("Are you sure?");
            phrases.Add("What a pain.");
            phrases.Add("Good for you.");
            phrases.Add("What is this?");
            phrases.Add("Are you there?");
            phrases.Add("What a mess.");
            phrases.Add("Is that OK?");
            phrases.Add("Thanks good job.");
            phrases.Add("This looks fine.");
            phrases.Add("I can return earlier.");
            phrases.Add("Do we have anyone in Portland?");
            phrases.Add("Both of us are still here.");
            phrases.Add("You are the greatest.");
            phrases.Add("Can you help me here?");
            phrases.Add("Have a great trip.");
            phrases.Add("They have capacity now.");
            phrases.Add("If so what was it?");
            phrases.Add("Wednesday is definitely a hot chocolate day.");
            phrases.Add("I agree since I am at the bank right now.");
            phrases.Add("I hope he is having a fantastic time.");
            phrases.Add("Hopefully this can wait until Monday.");
            phrases.Add("Please call tomorrow if possible.");
            phrases.Add("I am waiting until she comes home.");
            phrases.Add("Thanks for the quick turnaround.");
            phrases.Add("Do you still need me to sign something?");
            phrases.Add("I think those are the right dates.");
            phrases.Add("I would be glad to participate.");
        }

      
    }
}
