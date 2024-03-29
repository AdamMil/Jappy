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

partial class DictionarySearchTab : TabBase
{
  public DictionarySearchTab()
  {
    InitializeComponent();
  }

  public override DocumentRenderer OutputArea
  {
    get { return resultList; }
  }
  
  public void LoadEntryDetails(WordDictionary dictionary, uint entryID, int headwordIndex)
  {
    details.Clear();
    SwitchToTab();

    DocumentNode root = details.Document.Root;

    Entry entry = dictionary.GetEntryById(entryID);
    Word headword = entry.Headwords[headwordIndex];

    // add entry summary
    root.Children.Add(new TextNode("Entry Summary", UI.BoldStyle));
    root.Children.Add(new TextNode(" (from "+dictionary.Name+" dictionary)\n"));
    UI.RenderDictionaryEntry(dictionary, entry, headwordIndex, this, root);

    // add kanji summary
    root.Children.Add(new TextNode("\nKanji Summary\n", UI.BoldStyle));
    root.Children.Add(new TextNode(UI.GetKanjiSummary(headword.Text)+"\n"));
    
    // add example sentences
    const int exampleLimit = 50;
    List<uint> exampleIds = new List<uint>(
      JapaneseSearchStrategy.Instance.Search(App.Examples, entry.Headwords[headwordIndex].Text,
                                          SearchFlag.ExactMatch|SearchFlag.SearchHeadwords|SearchFlag.SearchReadings));
    if(exampleIds.Count != 0)
    {
      root.Children.Add(new TextNode("\nExample Sentences\n", UI.BoldStyle));
      if(exampleIds.Count <= exampleLimit)
      {
        root.Children.Add(new TextNode(exampleIds.Count+" example sentence(s)\n\n"));
      }
      else
      {
        root.Children.Add(new TextNode(exampleLimit+" example sentences randomly chosen from "+
                                       exampleIds.Count+"\n\n"));

        // shuffle the sentence IDs
        for(int i=0; i<exampleIds.Count-1; i++)
        {
          int other = App.Random.Next(i, exampleIds.Count);
          uint temp = exampleIds[i];
          exampleIds[i] = exampleIds[other];
          exampleIds[other] = temp;
        }
        // then remove all after the example limit
        exampleIds.RemoveRange(exampleLimit, exampleIds.Count-exampleLimit);
      }

      foreach(ExampleSentence example in new ExampleIterator(App.Examples, exampleIds))
      {
        UI.RenderExampleSentence(example, details);
      }
    }
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
    SwitchToTab();

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
          Form.SetStatusText("Search results truncated at "+itemLimit+" results.");
          break;
        }

        UI.RenderDictionaryEntry(dictionary, entry, -1, this, resultList.Document.Root);
      }

      if(itemLimit <= 0 || numberOfResults <= itemLimit)
      {
        Form.SetStatusText("The search returned "+numberOfResults+" result(s).");
      }
    }
    catch(ArgumentException e)
    {
      Form.ShowToolTip(input, "Error", e.Message);
    }
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

  void input_MouseEnter(object sender, EventArgs e)
  {
    Form.SetStatusText(input,
                       "Type here and press Enter to search. Prefix romaji with @. Examples: her, 彼女, @kanojo");
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

  void input_KeyDown(object sender, KeyEventArgs e)
  {
    this.textBox_CommonKeyDown(sender, e);
  }

  void common_MouseLeave(object sender, EventArgs e)
  {
    this.control_RestoreStatusText(sender, e);
  }

  void resultList_MouseEnter(object sender, EventArgs e)
  {
    if(resultList.Document.Root.Children.Count == 0) // if there are no results
    {
      Form.SetStatusText(resultList,
                         "Type something into the search box above and press Enter to get search results.");
    }
    else
    {
      Form.SetStatusText(resultList,
        "Mouse over headwords for kanji information. Right click headwords or selected text to perform actions.");
    }
  }

  private void output_MouseClick(object sender, MouseEventArgs e)
  {
    doc_MouseClick(sender, e);
  }
}

} //namespace Jappy