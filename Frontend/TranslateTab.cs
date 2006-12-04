using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Jappy.Backend;

namespace Jappy
{

partial class TranslateTab : TabBase
{
  public TranslateTab()
  {
    InitializeComponent();
  }

  public void PerformTranslation(string text)
  {
    if(text == null) throw new ArgumentNullException();

    input.Text = text;
    output.Clear();

    WordTranslator translator = new WordTranslator();
    translator.WordDictionaries.Add(App.WordDict);
    translator.NameDictionaries.Add(App.NameDict);

    TranslatedWord[] words = translator.TranslateWordsInJapaneseText(text);

    int lastEnd = 0; // the end of the previous word
    bool oddWord = false;
    foreach(TranslatedWord word in words)
    {
      if(lastEnd < word.Position) output.AppendText(text.Substring(lastEnd, word.Position-lastEnd));
      output.AppendText(text.Substring(word.Position, word.Length),
                             oddWord ? Color.Blue : Color.DarkGreen);
      lastEnd = word.Position + word.Length;
      oddWord = !oddWord;
    }
    if(lastEnd < text.Length) output.AppendText(text.Substring(lastEnd, text.Length-lastEnd));
    output.AppendText("\n\n");

    foreach(TranslatedWord word in words)
    {
      // TODO: instead of rendering each entry on its own line, merge multiple entries into one.
      foreach(TranslatedWordEntry entry in word.Entries)
      {
        if(entry.Inflection != InflectionType.None)
        {
          output.AppendText("Possible inflected word");
          if(entry.Inflection != InflectionType.Unknown)
          {
            List<string> modifiers = new List<string>(4);
            if((entry.Inflection & InflectionType.TypeMask) != 0)
            {
              modifiers.Add((entry.Inflection & InflectionType.TypeMask).ToString());
            }
            if((entry.Inflection & InflectionType.Negative) != 0) modifiers.Add("negative");
            if((entry.Inflection & InflectionType.Past) != 0) modifiers.Add("past");
            if((entry.Inflection & InflectionType.Polite) != 0) modifiers.Add("polite");
            else if((entry.Inflection & InflectionType.Plain) != 0) modifiers.Add("plain");

            output.AppendText(" ("+string.Join(", ", modifiers.ToArray())+")");
          }
          output.AppendText(":\n");
        }

        //RenderDictionaryEntryTo(word.Dictionary, word.Dictionary.GetEntryById(entry.EntryId), -1, output, true);
      }
    }
  }

  void transInput_KeyPress(object sender, KeyPressEventArgs e)
  {
    if((e.KeyChar == '\n' || e.KeyChar == '\r') && Control.ModifierKeys == Keys.Control)
    {
      string text = input.Text.Trim();
      if(text != "")
      {
        PerformTranslation(text);
      }
      e.Handled = true;
    }
  }

  void input_MouseEnter(object sender, EventArgs e)
  {
    Form.SetStatusText(input, "Enter Japanese text into this box and press Ctrl-Enter to lookup words in the text.");
  }

  void common_MouseLeave(object sender, EventArgs e)
  {
    base.control_RestoreStatusText(sender, e);
  }
}

} // namespace Jappy