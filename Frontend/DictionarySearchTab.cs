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
  
  static DictionarySearchTab()
  {
    darkGreenStyle = new Style();
    darkGreenStyle.ForeColor = Color.DarkGreen;
  }

  public void LoadEntryDetails(WordDictionary dictionary, uint entryID, int headwordIndex)
  {
    details.Clear();
    DocumentNode root = details.Document.Root;

    Entry entry = dictionary.GetEntryById(entryID);
    Word headword = entry.Headwords[headwordIndex];

    // add entry summary
    root.Children.Add(new TextNode("Entry Summary", UI.BoldStyle));
    root.Children.Add(new TextNode(" (from "+dictionary.Name+" dictionary)\n"));
    RenderDictionaryEntryTo(dictionary, entry, headwordIndex, details.Document, false);

    // add kanji summary
    root.Children.Add(new TextNode("\nKanji Summary\n", UI.BoldStyle));
    //root.Children.Add(new TextNode(GetInformationAboutKanji(headword.Text)+"\n"));
    
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
        root.Children.Add(new TextNode(example.Japanese+"\n", UI.JpStyle));
        root.Children.Add(new TextNode(example.English+"\n"));
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
        RenderDictionaryEntryTo(dictionary, entry, -1, resultList.Document, true);
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

  class HeadwordNode : LinkNode
  {
    public HeadwordNode(DictionarySearchTab tab, WordDictionary dictionary, uint entryID, int headword, string text)
      : base(text, UI.JpStyle)
    {
      this.tab        = tab;
      this.dictionary = dictionary;
      this.entryID    = entryID;
      this.headword   = headword;
    }

    protected internal override void OnMouseClick(MouseEventArgs e)
    {
      base.OnMouseClick(e);
      tab.LoadEntryDetails(dictionary, entryID, headword);
    }

    readonly DictionarySearchTab tab;
    readonly WordDictionary dictionary;
    readonly uint entryID;
    readonly int headword;
  }

  void RenderDictionaryEntryTo(WordDictionary dictionary, Entry entry, int headwordIndex, Document doc,
                               bool makeHeadwordsClickable)
  {
    BlockNode node = new BlockNode();
    doc.Root.Children.Add(node);

    bool sep;

    // ADD HEADWORDS
    sep = false;
    for(int i=0; i<entry.Headwords.Length; i++)
    {
      if(headwordIndex != -1 && i != headwordIndex) continue;

      if(sep) node.Children.Add(new TextNode(", "));
      else sep = true;

      string headword = entry.Headwords[i].Text;
      if(makeHeadwordsClickable)
      {
        node.Children.Add(new HeadwordNode(this, dictionary, entry.ID, i, headword));
      }
      else
      {
        node.Children.Add(new TextNode(headword, UI.JpStyle, darkGreenStyle));
      }
    }

    // ADD READINGS
    StringBuilder sb = new StringBuilder();
    sb.Append(" ");

    if(entry.Readings != null)
    {
      sb.Append('/');
      sep = false;
      foreach(Word reading in entry.Readings)
      {
        if(headwordIndex != -1 && reading.AppliesToHeadword != -1 && reading.AppliesToHeadword != headwordIndex)
        {
          continue;
        }

        if(sep) sb.Append(", ");
        else sep = true;
        sb.Append(reading.Text);
        if(reading.Flags != ResultFlag.None) sb.Append("("+reading.Flags.ToString()+")");
      }
      sb.Append("/ ");
    }

    // ADD MEANINGS
    if(entry.Meanings == null)
    {
      sb.Append("<unknown meaning>");
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
          sb.Append('(');
          sep = false;
          foreach(SenseFlag flag in meaning.Flags)
          {
            if(sep) sb.Append(';');
            else sep = true;
            sb.Append(flag.ToString());
          }
          sb.Append(") ");
        }

        if(entry.Meanings.Length != 1)
        {
          sb.Append("(" + meaningNumber++ + ") ");
        }

        foreach(Gloss gloss in meaning.Glosses)
        {
          sb.Append(gloss.Text).Append("; ");
        }
      }
    }

    node.Children.Add(new TextNode(sb.ToString()));
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
        "Mouse over blue words for kanji information. Select text and right click to perform actions.");
    }
  }
  
  static Style darkGreenStyle;
}

} //namespace Jappy