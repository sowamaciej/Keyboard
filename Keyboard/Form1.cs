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

                System.Collections.Generic.IEnumerable<string> suggestions = GetSuggestions(swipeType, 10);
                int length = suggestions.Count();
                string firstSuggestion = length > 0 ? suggestions.ElementAt(0) : null;
                word = firstSuggestion;

                if (firstSuggestion != null)
                {
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
                Button btn = new Button();
                btn.Text = suggestions.ElementAt(i);
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

        private void ButtonSendClick(object sender, EventArgs e)
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

        private void BackspaceClick(object sender, EventArgs e)
        {
            SendKeys.Send(BACKSPACE_MARKER);
        }

        private void EnterClick(object sender, EventArgs e)
        {
            RemoveSuggestionButtons();
            SendKeys.Send(ENTER_MARKER);
        }

    }
}

/* TODO
 * Nazwy własne - może przycisk, który przełącza tryby swipe/normal typing
 * Poprawić działanie sugestii dla języka polskiego
 * Wprowadzanie zduplikowanych znaków
 * Wprowadzenie pojedynczych znaków 
 * Polskie znaki 
 * Backspace dla całych wyrazów - ? 
 * znaki specjalne
 * poprawić blad kiedy klikniecie nastepuje poza klawiszami
 * Upiększyć widok klawiatury
 * */
