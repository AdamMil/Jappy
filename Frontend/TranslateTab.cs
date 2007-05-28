/*
Jappy is a Japanese dictionary and study tool.

http://www.adammil.net/
Copyright (C) 2007 Adam Milazzo

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
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

  static TranslateTab()
  {
    matchedStyle = new Style();
    matchedStyle.ForeColor = Color.Blue;

    inflectedStyle = new Style();
    inflectedStyle.ForeColor = Color.Green;
  }

  public override DocumentRenderer OutputArea
  {
    get { return output; }
  }

  public void PerformTranslation(string text)
  {
    if(text == null) throw new ArgumentNullException();

    input.Text = text;
    output.Clear();
    SwitchToTab();

    WordTranslator translator = new WordTranslator();
    translator.WordDictionaries.Add(App.WordDict);
    translator.NameDictionaries.Add(App.NameDict);

    TranslatedWord[] words = translator.TranslateWordsInJapaneseText(text);

    DocumentNode root = output.Document.Root;
    int lastEnd = 0; // the end of the previous word
    bool oddWord = false;
    foreach(TranslatedWord word in words)
    {
      if(lastEnd < word.Position) root.Children.Add(new TextNode(text.Substring(lastEnd, word.Position-lastEnd)));
      root.Children.Add(new TextNode(text.Substring(word.Position, word.Length)+" ",
                                     word.PossiblyInflected ? inflectedStyle : matchedStyle));
      lastEnd = word.Position + word.Length;
      oddWord = !oddWord;
    }
    if(lastEnd < text.Length) root.Children.Add(new TextNode(text.Substring(lastEnd, text.Length-lastEnd)));
    root.Children.Add(new TextNode("\n\n"));

    DictionarySearchTab tab = Form.GetDictionarySearchTab();
    foreach(TranslatedWord word in words)
    {
      // TODO: instead of rendering each entry on its own line, merge multiple entries into one.
      foreach(TranslatedWordEntry entry in word.Entries)
      {
        if(entry.Inflection != InflectionType.None)
        {
          StringBuilder sb = new StringBuilder();
          sb.Append("Possible inflected word");
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

            sb.Append(" (").Append(string.Join(", ", modifiers.ToArray())).Append(')');
          }
          sb.Append(":\n- ");
          root.Children.Add(new TextNode(sb.ToString()));
        }

        UI.RenderDictionaryEntry(word.Dictionary, word.Dictionary.GetEntryById(entry.EntryId), -1, tab, root);
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

  void output_MouseClick(object sender, MouseEventArgs e)
  {
    doc_MouseClick(sender, e);
  }

  void output_MouseEnter(object sender, EventArgs e)
  {
    if(output.Document.Root.Children.Count == 0) // if there are no results
    {
      Form.SetStatusText(output,
                         "Enter Japanese text into the box above and press Ctrl-Enter to lookup words in the text.");
    }
    else
    {
      Form.SetStatusText(output,
        "Mouse over headwords for kanji information. Right click headwords or selected text to perform actions.");
    }
  }

  static readonly Style matchedStyle, inflectedStyle;
}

} // namespace Jappy