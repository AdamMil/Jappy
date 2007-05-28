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
abstract class TabBase : UserControl
{
  public abstract DocumentRenderer OutputArea
  {
    get;
  }

  protected MainForm Form
  {
    get { return (MainForm)FindForm(); }
  }
  
  protected TabPage TabPage
  {
    get { return (TabPage)Parent; }
  }

  protected internal virtual void OnActivate() { }
  protected internal virtual void OnDeactivate() { }

  protected void SwitchToTab()
  {
    Form.SwitchToTab(TabPage);
  }

  protected ContextMenuStrip CreateSelectedTextMenu(DocumentRenderer control)
  {
    ContextMenuStrip menu = new ContextMenuStrip();
    menu.Items.Add("&Copy", null, delegate(object s, EventArgs a) { control.Copy(); });
    menu.Items.Add("&Lookup (exact)", null, delegate(object s, EventArgs a) { SearchDictionary(control, false); });
    menu.Items.Add("Lookup (starts &with)", null, delegate(object s, EventArgs a) { SearchDictionary(control, true); });
    return menu;
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

  protected void doc_MouseClick(object sender, MouseEventArgs e)
  {
    DocumentRenderer doc = (DocumentRenderer)sender;
    if(e.Button == MouseButtons.Right && doc.SelectionLength != 0) // if the user right-clicks on selected text
    {
      CreateSelectedTextMenu(doc).Show(doc, e.Location);
    }
  }

  protected void control_RestoreStatusText(object sender, EventArgs e)
  {
    Form.RestoreStatusText((Control)sender);
  }
  
  void SearchDictionary(DocumentRenderer control, bool startsWith)
  {
    string text = control.SelectedText.Trim();
    if(startsWith) text += "*";
    Form.GetDictionarySearchTab().PerformDictionarySearch(text);
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

  public static string GetMeaningText(Meaning[] meanings, bool addFlagText)
  {
    if(meanings == null)
    {
      return "<unknown meaning>";
    }
    else
    {
      StringBuilder sb = new StringBuilder();
      int meaningNumber = 1;
      foreach(Meaning meaning in meanings)
      {
        if(addFlagText && meaning.Flags != null)
        {
          sb.Append('(');
          bool sep = false;
          foreach(SenseFlag flag in meaning.Flags)
          {
            if(sep) sb.Append(';');
            else sep = true;
            sb.Append(flag.ToString());
          }
          sb.Append(") ");
        }

        if(meanings.Length != 1)
        {
          if(meaningNumber != 1) sb.Append("; ");
          sb.Append("(" + meaningNumber++ + ") ");
        }

        for(int glossIndex=0; glossIndex<meaning.Glosses.Length; glossIndex++)
        {
          sb.Append(meaning.Glosses[glossIndex].Text);
          if(glossIndex != meaning.Glosses.Length-1) sb.Append("; ");
        }
      }
      return sb.ToString();
    }
  }

  public static void RenderDictionaryEntry(WordDictionary dictionary, Entry entry, int headwordIndex,
                                           DictionarySearchTab tab, DocumentNode parent)
  {
    bool sep;

    // add headword(s)
    sep = false;
    for(int i=0; i<entry.Headwords.Length; i++)
    {
      if(headwordIndex != -1 && i != headwordIndex) continue;

      if(sep) parent.Children.Add(new TextNode(", "));
      else sep = true;

      parent.Children.Add(new HeadwordNode(tab, dictionary, entry.ID, i, entry.Headwords[i].Text));
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
    sb.Append(GetMeaningText(entry.Meanings, true));

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
      this.tab           = tab;
      this.dictionary    = dictionary;
      this.entryID       = entryID;
      this.headwordIndex = headword;
    }

    protected internal override void OnMouseClick(object sender, MouseEventArgs e)
    {
      base.OnMouseClick(sender, e);
      if(e.Button == MouseButtons.Left)
      {
        LoadDetails();
      }
      else if(e.Button == MouseButtons.Right)
      {
        ContextMenuStrip menu = new ContextMenuStrip();
        menu.Items.Add("Add to &study list", null, delegate(object o, EventArgs a) { AddToStudyList(); });
        menu.Items.Add("&Copy", null, delegate(object o, EventArgs a) { Clipboard.SetText(Text); });
        menu.Items.Add("&Load details", null, delegate(object o, EventArgs a) { LoadDetails(); });
        menu.Items.Add("Lookup (starts &with)", null, delegate(object o, EventArgs a) { SearchStartsWith(); });
        menu.Items.Add("Search &examples", null, delegate(object o, EventArgs a) { SearchExamples(); });
        menu.Show((Control)sender, e.Location);
      }
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

    void AddToStudyList()
    {
      StudyTab tab = App.MainForm.GetStudyTab();

      StudyList.Item item = new StudyList.Item();

      Entry entry = dictionary.GetEntryById(entryID);
      
      foreach(Word headword in entry.Headwords)
      {
        if(item.Phrase != null) item.Phrase += ", ";
        item.Phrase += headword.Text;
      }
      
      if(entry.Readings != null)
      {
        foreach(Word reading in entry.Readings)
        {
          if(item.Readings != null) item.Readings += ", ";
          item.Readings += reading.Text;
        }
      }
      
      item.Meanings = UI.GetMeaningText(entry.Meanings, false);
      
      StudyListEntryDialog dialog = new StudyListEntryDialog();
      dialog.LoadItem(item);
      
      if(dialog.ShowDialog() == DialogResult.OK)
      {
        if(!tab.IsListLoaded)
        {
          DialogResult result = MessageBox.Show("No study list is loaded. Do you want to load an existing one?",
                                              "Load a list?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
          
          if(result == DialogResult.Yes && !tab.LoadList() || result == DialogResult.No && !tab.CreateNewList() ||
             result == DialogResult.Cancel)
          {
            return;
          }
        }

        dialog.SaveItem(item);
        tab.AddEntry(item);
      }
    }

    void LoadDetails()
    {
      tab.LoadEntryDetails(dictionary, entryID, headwordIndex);
    }
    
    void SearchExamples()
    {
      App.MainForm.GetExampleSearchTab().PerformExampleSearch(Text);
    }
    
    void SearchStartsWith()
    {
      App.MainForm.GetDictionarySearchTab().PerformDictionarySearch(Text+"*");
    }

    readonly DictionarySearchTab tab;
    readonly WordDictionary dictionary;
    readonly uint entryID;
    readonly int headwordIndex;
  }
}

} // namespace Jappy