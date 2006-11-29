using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Jappy
{

partial class MainForm : Form
{
  public MainForm()
  {
    InitializeComponent();
  }

  static MainForm()
  {
//examples.ImportModifiedTanakaCorpusInUTF8("e:/examples.txt");
//charDict.ImportKanjiDicXml(System.IO.File.OpenRead(@"e:\kanjidic2.xml"));
//charDict.Save("e:/kanji.dict");

//JapaneseDictionary wordDict = new JapaneseDictionary();
//wordDict.ImportJMDict(System.IO.File.OpenRead(@"e:/jmdict_e.xml"));
//wordDict.Save("e:/words.index", "e:/words.dict");
}

  #region Event handlers
  void textBox_CommonKeyDown(object sender, KeyEventArgs e)
  {
    TextBoxBase box = (TextBoxBase)sender;

    if(e.KeyCode == Keys.A && e.Modifiers == Keys.Control) // ctrl-A means "Select All"
    {
      box.SelectAll();
      e.Handled = true;
    }
  }

  void dictInput_MouseEnter(object sender, EventArgs e)
  {
    PushStatusText("Type here and press Enter to search. Prefix romaji with @. Examples: her, 彼女, @kanojo");
  }

  void dictInput_KeyPress(object sender, KeyPressEventArgs e)
  {
    if((e.KeyChar == '\n' || e.KeyChar == '\r') &&
       (Control.ModifierKeys == Keys.None || Control.ModifierKeys == Keys.ControlKey))
    {
      string query = dictInput.Text.Trim();
      if(query != "")
      {
        PerformDictionarySearch(query);
      }
    }
  }

  void dictResults_MouseEnter(object sender, EventArgs e)
  {
    if(dictResults.TextLength == 0) // if there are no results
    {
      PushStatusText("Type something into the search box above and press Enter to get search results.");
    }
    else
    {
      PushStatusText("Mouse over blue words for kanji information. Select text and right click to perform actions.");
    }
  }

  void transInput_MouseEnter(object sender, EventArgs e)
  {
    PushStatusText("Enter Japanese text into this box and press Ctrl-Enter to lookup words in the text.");
  }

  void control_PopStatusText(object sender, EventArgs e)
  {
    PopStatusText();
  }

  private void exitMenuItem_Click(object sender, EventArgs e)
  {
    Close();
  }
  #endregion

  #region Status text handling
  void PushStatusText(string text)
  {
    if(statusText.Text != text)
    {
      statusTexts.Add(statusText.Text);
      statusText.Text = text;
    }
    
    Debug.Assert(statusTexts.Count <= 4);

    if(statusTexts.Count > 4) // make sure the list doesn't get too big (although it shouldn't)
    {
      statusTexts.RemoveRange(0, statusTexts.Count - 4);
    }
  }
  
  void PopStatusText()
  {
    Debug.Assert(statusTexts.Count != 0);

    if(statusTexts.Count == 0)
    {
      statusText.Text = "Welcome to Jappy!";
    }
    else
    {
      statusText.Text = statusTexts[statusTexts.Count-1];
      statusTexts.RemoveAt(statusTexts.Count-1);
    }
  }
  #endregion

  #region Text regions
  abstract class FormTextRegion : TextRegion
  {
    protected FormTextRegion(MainForm form)
    {
      this.Form = form;
    }

    protected readonly MainForm Form;
  }

  class ResultTextRegion : FormTextRegion
  {
    public ResultTextRegion(MainForm form, uint id) : base(form)
    {
      this.ID = id;
    }

    protected uint ID;
  }

  class HeadwordTextRegion : ResultTextRegion
  {
    public HeadwordTextRegion(MainForm form, uint id) : base(form, id) { }

    public override Color Color
    {
      get { return Color.Blue; }
    }

    protected internal override void OnMouseClick(MouseEventArgs e)
    {
      base.OnMouseClick(e);
      Form.LoadDictionaryEntryDetails(ID);
    }

    protected internal override void OnMouseEnter(MouseEventArgs e)
    {
      base.OnMouseEnter(e);
      AddStyle(FontStyle.Underline);
      Form.PushStatusText(GetInformationAboutKanji(Text));
      TextBox.Cursor = Cursors.Hand;
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
      base.OnMouseLeave(e);
      RemoveStyle(FontStyle.Underline);
      Form.PopStatusText();
      TextBox.Cursor = Cursors.IBeam;
    }
  }
  #endregion

  static string GetInformationAboutKanji(string text)
  {
    List<char> chars = new List<char>(text.Length);

    for(int i=0; i<text.Length; i++)
    {
      char c = text[i];
      if(JP.IsKanji(c) && !chars.Contains(c))
      {
        chars.Add(c);
      }
    }
    
    if(chars.Count == 0)
    {
      return "There are no kanji in this headword.";
    }
    else
    {
      StringBuilder sb = new StringBuilder();
      foreach(char c in chars)
      {
        if(sb.Length != 0) sb.Append("; ");
        sb.Append(c).Append(' ');

        Kanji kanji;
        int numEnglishReadings = 0;
        if(App.CharDict.TryGetKanjiData(c, out kanji) && kanji.Readings != null)
        {
          bool readingSep = false;
          foreach(Reading reading in kanji.Readings)
          {
            if(reading.Type == ReadingType.English)
            {
              if(readingSep) sb.Append(", ");
              else readingSep = true;
              sb.Append(reading.Text);
              numEnglishReadings++;
            }
          }
        }

        if(numEnglishReadings == 0)
        {
          sb.Append("<unknown>");
        }
      }

      return sb.ToString();
    }
  }

  void LoadDictionaryEntryDetails(uint id)
  {
    dictDetails.Clear();

    Entry entry = App.WordDict.GetEntryById(id);

    dictDetails.AppendText("Entry Summary\n", FontStyle.Bold);
    RenderDictionaryEntryTo(entry, dictDetails, false);

    string headwords = null;
    foreach(Word headword in entry.Headwords) headwords += headword.Text;

    dictDetails.AppendText("\n");
    dictDetails.AppendText("Kanji Summary\n", FontStyle.Bold);
    dictDetails.AppendText(GetInformationAboutKanji(headwords) + "\n");
    
    dictDetails.DeselectAll();
  }

  void PerformDictionarySearch(string query)
  {
    if(string.IsNullOrEmpty(query)) throw new ArgumentException();

    dictInput.Text = query;
    dictResults.Clear();
    tabControl.SelectedTab = dictionaryPage;

    SearchFlag flags = SearchFlag.MatchStart;
    if(query[0] == '@') // queries beginning with '@' are assumed to be roumaji. convert them to kana.
    {
      string error;
      query = JP.ConvertRomajiToKana(query.Substring(1), out error);

      if(error != null)
      {
        ShowToolTip(dictInput, "Input error", "Invalid romaji: "+error);
        return;
      }
      
      flags |= SearchFlag.SearchHeadwords | SearchFlag.SearchReadings;
    }
    else
    {
      flags |= SearchFlag.SearchAll;
    }

    int numberOfResults = 0;
    foreach(Entry entry in new EntryIterator(App.WordDict, App.WordDict.Search(query, flags)))
    {
      if(++numberOfResults > 100)
      {
        statusText.Text = "Search results truncated at 100 results.";
        break;
      }
      RenderDictionaryEntryTo(entry, dictResults, true);
    }
  }

  void RenderDictionaryEntryTo(Entry entry, RicherTextBox textBox, bool makeHeadwordsClickable)
  {
    int resultStart = textBox.TextLength;
    bool sep;

    sep = false;
    foreach(Word headword in entry.Headwords)
    {
      if(sep) textBox.AppendText(", ");
      else sep = true;
      if(makeHeadwordsClickable)
      {
        textBox.AddRegion(headword.Text, new HeadwordTextRegion(this, entry.ID));
      }
      else
      {
        textBox.AppendText(headword.Text, Color.DarkGreen);
      }
    }
    textBox.AppendText(" ");

    if(entry.Readings != null)
    {
      textBox.AppendText("/");
      sep = false;
      foreach(Word reading in entry.Readings)
      {
        if(sep) textBox.AppendText(", ");
        else sep = true;
        textBox.AppendText(reading.Text);
      }
      textBox.AppendText("/ ");
    }

    int meaningNumber = 1;
    foreach(Meaning meaning in entry.Meanings)
    {
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

    textBox.AddRegion(resultStart, textBox.TextLength-resultStart, new ResultTextRegion(this, entry.ID));
    textBox.AppendText("\n");
  }

  void ShowToolTip(Control control, string title, string text)
  {
    toolTip.ToolTipTitle = title;
    toolTip.Show(text, control, 50, -50, 3000);
  }

  readonly List<string> statusTexts = new List<string>();

  static readonly Font jpFont = new Font("Verdana", 10f, FontStyle.Regular);
  static readonly Font enFont = new Font("Verdana", 9f, FontStyle.Regular);
}


} // namespace Jappy