using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Jappy.Backend;

namespace Jappy
{

partial class DictionarySearchTab : UserControl
{
  public DictionarySearchTab()
  {
    InitializeComponent();
  }

  public void PerformDictionarySearch(string query)
  {
    PerformDictionarySearch(query, cmbDictionary.SelectedDictionary, chkCommon.Checked ? 26 : 0, 100, null);
  }

  public void PerformDictionarySearch(string query, WordDictionary dictionary, int frequencyThreshold, int itemLimit,
                                      SenseFlag[] partsOfSpeech)
  {
    if(query == null || dictionary == null) throw new ArgumentNullException();

    input.Text = query;
    resultList.Clear();

    try
    {
      int numberOfResults = 0;
      IEnumerable<Entry> entries;

      if(string.IsNullOrEmpty(query))
      {
        entries = dictionary.RetrieveAll();
      }
      else
      {
        entries = new EntryIterator(dictionary,
          JapaneseSearchStrategy.Instance.Search(dictionary, query, SearchFlag.ExactMatch | SearchFlag.SearchAll));
      }

      foreach(Entry entry in entries)
      {
        if(frequencyThreshold > 0 && entry.Frequency > frequencyThreshold) continue;

        if(partsOfSpeech != null)
        {
          bool hasAny = false;
          if(entry.Meanings != null)
          {
            foreach(Meaning meaning in entry.Meanings)
            {
              foreach(SenseFlag flag in partsOfSpeech)
              {
                if(meaning.HasFlag(flag))
                {
                  hasAny = true;
                  goto done;
                }
              }
            }
          }
          done:
          if(!hasAny) continue;
        }
        
        if(itemLimit > 0 && ++numberOfResults > itemLimit)
        {
          //statusText.Text = "Search results truncated at "+threshold+" results.";
          break;
        }
        RenderDictionaryEntryTo(dictionary, entry, -1, resultList, true);
      }

      if(itemLimit <= 0 || numberOfResults <= itemLimit)
      {
        //statusText.Text = "The search returned "+numberOfResults+" result(s).";
      }
    }
    catch(ArgumentException e)
    {
      //ShowToolTip(dictInput, "Error", e.Message);
    }
  }

  void RenderDictionaryEntryTo(WordDictionary dictionary, Entry entry, int headwordIndex, RicherTextBox textBox,
                               bool makeHeadwordsClickable)
  {
    int resultStart = textBox.TextLength;
    bool sep;

    sep = false;
    for(int i=0; i<entry.Headwords.Length; i++)
    {
      if(headwordIndex != -1 && i != headwordIndex) continue;

      if(sep) textBox.AppendText(", ");
      else sep = true;
      string headword = entry.Headwords[i].Text;
      textBox.AppendText(headword, Color.DarkGreen);
    }
    textBox.AppendText(" ");

    if(entry.Readings != null)
    {
      textBox.AppendText("/");
      sep = false;
      foreach(Word reading in entry.Readings)
      {
        if(headwordIndex != -1 && reading.AppliesToHeadword != -1 && reading.AppliesToHeadword != headwordIndex)
        {
          continue;
        }

        if(sep) textBox.AppendText(", ");
        else sep = true;
        textBox.AppendText(reading.Text);
        if(reading.Flags != ResultFlag.None) textBox.AppendText("("+reading.Flags.ToString()+")");
      }
      textBox.AppendText("/ ");
    }

    if(entry.Meanings == null)
    {
      textBox.AppendText("<unknown meaning>");
    }
    else
    {
      int meaningNumber = 1;
      foreach(Meaning meaning in entry.Meanings)
      {
        if(headwordIndex != -1 && meaning.AppliesToHeadword != -1 && meaning.AppliesToHeadword != headwordIndex)
        {
          continue;
        }

        if(meaning.Flags != null)
        {
          textBox.AppendText("(");
          sep = false;
          foreach(SenseFlag flag in meaning.Flags)
          {
            if(sep) textBox.AppendText(";");
            else sep = true;
            textBox.AppendText(flag.ToString());
          }
          textBox.AppendText(") ");
        }

        if(entry.Meanings.Length != 1)
        {
          textBox.AppendText("(" + meaningNumber++ + ") ");
        }

        foreach(Gloss gloss in meaning.Glosses)
        {
          FontStyle style = textBox.Font.Style;
          if(gloss.GoodMatch) style |= FontStyle.Underline;
          textBox.AppendText(gloss.Text, style);
          textBox.AppendText("; ");
        }
      }
    }

    textBox.AppendText("\n");
  }

  void input_KeyPress(object sender, KeyPressEventArgs e)
  {
    if((e.KeyChar == '\n' || e.KeyChar == '\r') &&
       (Control.ModifierKeys == Keys.None || Control.ModifierKeys == Keys.Control))
    {
      string query = input.Text.Trim();
      if(query != "")
      {
        PerformDictionarySearch(query);
      }
    }
  }

  void lblAdvanced_Click(object sender, EventArgs e)
  {
    AdvancedDictionarySearchDialog dialog = new AdvancedDictionarySearchDialog();
    dialog.QueryText  = input.Text;
    dialog.Dictionary = cmbDictionary.SelectedDictionary;
    dialog.Frequency  = chkCommon.Checked ? 26 : 0;

    if(dialog.ShowDialog() == DialogResult.OK)
    {
      PerformDictionarySearch(dialog.QueryText, dialog.Dictionary, dialog.Frequency, dialog.ItemLimit,
                              dialog.GetPartsOfSpeech());
    }
  }
}

} //namespace Jappy