using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Jappy.Backend;

namespace Jappy
{

#region DictionaryDropDown
class DictionaryDropdown : ComboBox
{
  public DictionaryDropdown()
  {
    this.DropDownStyle = ComboBoxStyle.DropDownList;
    this.Items.Add("edict");
    this.Items.Add("Names");
    this.SelectedIndex = 0;
  }
  
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public WordDictionary SelectedDictionary
  {
    get { return this.SelectedIndex == 0 ? App.WordDict : App.NameDict; }
    set { this.SelectedItem = value.Name; }
  }
  
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public new ObjectCollection Items
  {
    get { return base.Items; }
  }
}
#endregion

#region LinkLabel
class LinkLabel : Label
{
  public LinkLabel()
  {
    Cursor    = Cursors.Hand;
    ForeColor = Color.Blue;
    Font      = new Font("Verdana", 9, FontStyle.Underline);
  }
}
#endregion

#region TabBase
class TabBase : UserControl
{
  protected MainForm Form
  {
    get { return (MainForm)FindForm(); }
  }
  
  protected TabPage TabPage
  {
    get { return (TabPage)Parent; }
  }

  protected void SwitchToTab()
  {
    Form.SwitchToTab(TabPage);
  }

  protected void textBox_CommonKeyDown(object sender, KeyEventArgs e)
  {
    TextBoxBase box = (TextBoxBase)sender;

    if(e.KeyCode == Keys.A && e.Modifiers == Keys.Control) // ctrl-A means "Select All"
    {
      box.SelectAll();
      e.Handled = true;
    }
  }
  
  protected void control_RestoreStatusText(object sender, EventArgs e)
  {
    Form.RestoreStatusText((Control)sender);
  }
}
#endregion

static class UI
{
  static UI()
  {
    JpStyle = new Style();
    JpStyle.FontName = "MS PGothic";

    UnderlinedStyle = new Style();
    UnderlinedStyle.FontStyle = FontStyle.Underline;

    BoldStyle = new Style();
    BoldStyle.FontStyle = FontStyle.Bold;
    
    headwordStyle = new Style();
    headwordStyle.ForeColor = Color.Green;
  }

  public static string GetKanjiSummary(string text)
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

  public static void RenderDictionaryEntry(WordDictionary dictionary, Entry entry, int headwordIndex,
                                           DictionarySearchTab tab, DocumentNode parent, bool makeHeadwordsClickable)
  {
    bool sep;

    // add headword(s)
    sep = false;
    for(int i=0; i<entry.Headwords.Length; i++)
    {
      if(headwordIndex != -1 && i != headwordIndex) continue;

      if(sep) parent.Children.Add(new TextNode(", "));
      else sep = true;

      string headword = entry.Headwords[i].Text;
      if(makeHeadwordsClickable)
      {
        parent.Children.Add(new HeadwordNode(tab, dictionary, entry.ID, i, headword));
      }
      else
      {
        parent.Children.Add(new TextNode(headword, UI.JpStyle, headwordStyle));
      }
    }

    // add readings
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

      parent.Children.Add(new TextNode(sb.ToString(), UI.JpStyle));
      sb.Length = 0;
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

    sb.Append('\n');
    parent.Children.Add(new TextNode(sb.ToString()));
  }

  public static void RenderExampleSentence(ExampleSentence example, DocumentRenderer output)
  {
    DocumentNode root = output.Document.Root;
    root.Children.Add(new TextNode(example.Japanese, UI.JpStyle));
    root.Children.Add(new TextNode(" ["));
    root.Children.Add(new TranslateTextNode(example.Japanese));
    root.Children.Add(new TextNode("]\n"+example.English+"\n"));
  }

  public static readonly Style JpStyle, UnderlinedStyle, BoldStyle;

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

    protected internal override void OnMouseClick(object sender, MouseEventArgs e)
    {
      base.OnMouseClick(sender, e);
      if(e.Button == MouseButtons.Left) tab.LoadEntryDetails(dictionary, entryID, headword);
    }

    protected internal override void OnMouseEnter(object sender, MouseEventArgs e)
    {
      base.OnMouseEnter(sender, e);
      App.MainForm.SetStatusText((Control)sender, UI.GetKanjiSummary(Text));
    }

    protected internal override void OnMouseLeave(object sender, EventArgs e)
    {
      base.OnMouseLeave(sender, e);
      App.MainForm.RestoreStatusText((Control)sender);
    }

    readonly DictionarySearchTab tab;
    readonly WordDictionary dictionary;
    readonly uint entryID;
    readonly int headword;
  }
  
  static readonly Style headwordStyle;
}

} // namespace Jappy