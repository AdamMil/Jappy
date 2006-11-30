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

  protected override void OnActivated(EventArgs e)
  {
    base.OnActivated(e);
    isActive = true;
  }

  protected override void OnDeactivate(EventArgs e)
  {
    base.OnDeactivate(e);
    isActive = false;
  }

  protected override void OnLoad(EventArgs e)
  {
    base.OnLoad(e);
    AddGlobalHotkey();
  }

  protected override void OnClosed(EventArgs e)
  {
    base.OnClosed(e);
    RemoveGlobalHotkey();
  }

  protected override void OnKeyDown(KeyEventArgs e)
  {
    base.OnKeyDown(e);
    if(e.Modifiers != Keys.None) return;

    if(e.KeyCode == Keys.F2 || e.KeyCode == Keys.F3 || e.KeyCode == Keys.F4 || e.KeyCode == Keys.F5)
    {
      TabPage page;
      if(e.KeyCode == Keys.F2) page = dictionaryPage;
      else if(e.KeyCode == Keys.F3) page = translatePage;
      else if(e.KeyCode == Keys.F4) page = examplePage;
      else page = studyPage;
      tabControl.SelectedTab = page;
      e.SuppressKeyPress = true;
    }
  }

  protected override void OnSizeChanged(EventArgs e)
  {
    base.OnSizeChanged(e);
    
    if(WindowState == FormWindowState.Minimized)
    {
      this.Hide();
      this.notifyIcon.Visible = true;
    }
    else
    {
      this.Show();
      this.notifyIcon.Visible = false;
    }
  }

  protected override void WndProc(ref Message m)
  {
    base.WndProc(ref m);
    
    const int WM_HOTKEY = 0x312;
    if(m.Msg == WM_HOTKEY) OnGlobalHotkeyPressed();
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
       (Control.ModifierKeys == Keys.None || Control.ModifierKeys == Keys.Control))
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

  void transInput_KeyPress(object sender, KeyPressEventArgs e)
  {
    if((e.KeyChar == '\n' || e.KeyChar == '\r') && Control.ModifierKeys == Keys.Control)
    {
      string text = transInput.Text.Trim();
      if(text != "")
      {
        PerformTranslation(text);
      }
    }
  }

  void transInput_MouseEnter(object sender, EventArgs e)
  {
    PushStatusText("Enter Japanese text into this box and press Ctrl-Enter to lookup words in the text.");
  }

  void exampleInput_KeyPress(object sender, KeyPressEventArgs e)
  {
    if((e.KeyChar == '\n' || e.KeyChar == '\r') &&
       (Control.ModifierKeys == Keys.None || Control.ModifierKeys == Keys.Control))
    {
      string query = exampleInput.Text.Trim();
      if(query != "")
      {
        PerformExampleSearch(query);
      }
    }
  }

  void exampleInput_MouseEnter(object sender, EventArgs e)
  {
    PushStatusText("Type here and press Enter to search. Prefix romaji with @. Examples: her, 彼女, @kanojo");
  }

  void control_PopStatusText(object sender, EventArgs e)
  {
    PopStatusText();
  }

  private void exitMenuItem_Click(object sender, EventArgs e)
  {
    Close();
  }

  void notifyIcon_Click(object sender, EventArgs e)
  {
    OnGlobalHotkeyPressed();
  }
  #endregion

  #region Global hot keys
  void AddGlobalHotkey()
  {
    if(globalKeyAtom != 0) RemoveGlobalHotkey();

    string atomName = "Jappy"+System.Diagnostics.Process.GetCurrentProcess().Id;
    globalKeyAtom = GlobalAddAtom(atomName);

    if(globalKeyAtom != 0) // if we could create the atom, register the key
    {
      if(RegisterHotKey(Handle, globalKeyAtom, MOD_WIN, (int)Keys.G) == 0)
      {
        GlobalDeleteAtom(globalKeyAtom); // if we couldn't register the key, delete the atom
        globalKeyAtom = 0;
      }
    }
  }

  void RemoveGlobalHotkey()
  {
    if(globalKeyAtom != 0)
    {
      UnregisterHotKey(Handle, globalKeyAtom);
      GlobalDeleteAtom(globalKeyAtom);
    }
  }
  
  void OnGlobalHotkeyPressed()
  {
    if(!isActive || !Visible || WindowState == FormWindowState.Minimized)
    {
      Show();
      if(WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
      Activate();
      dictInput.Focus();
      dictInput.SelectAll();
    }
    else
    {
      WindowState = FormWindowState.Minimized;
    }
  }

  const int MOD_ALT = 1, MOD_CONTROL = 2, MOD_SHIFT = 4, MOD_WIN = 8;
  [System.Runtime.InteropServices.DllImport("user32")]
  static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);
  [System.Runtime.InteropServices.DllImport("user32")]
  static extern int UnregisterHotKey(IntPtr hwnd, int id);
  [System.Runtime.InteropServices.DllImport("kernel32")]
  static extern short GlobalAddAtom(string lpString);
  [System.Runtime.InteropServices.DllImport("kernel32")]
  static extern short GlobalDeleteAtom(short nAtom);
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
    public HeadwordTextRegion(MainForm form, WordDictionary dictionary, uint id, int headwordIndex) : base(form, id)
    {
      this.dictionary    = dictionary;
      this.headwordIndex = headwordIndex;
    }

    public override Color Color
    {
      get { return Color.Blue; }
    }

    protected internal override void OnMouseClick(MouseEventArgs e)
    {
      base.OnMouseClick(e);
      Form.LoadDictionaryEntryDetails(dictionary, ID, headwordIndex);
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
    
    WordDictionary dictionary;
    int headwordIndex;
  }
  
  class TranslateExampleSentenceRegion : FormTextRegion
  {
    public TranslateExampleSentenceRegion(MainForm form, string sentenceText) : base(form)
    {
      this.sentenceText = sentenceText;
    }

    public override Color Color
    {
      get { return Color.Blue; }
    }

    protected internal override void OnMouseClick(MouseEventArgs e)
    {
      base.OnMouseClick(e);
      Form.PerformTranslation(sentenceText);
    }

    protected internal override void OnMouseEnter(MouseEventArgs e)
    {
      base.OnMouseEnter(e);
      AddStyle(FontStyle.Underline);
      Form.PushStatusText("Click to translate words in this example sentence.");
      TextBox.Cursor = Cursors.Hand;
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
      base.OnMouseLeave(e);
      RemoveStyle(FontStyle.Underline);
      Form.PopStatusText();
      TextBox.Cursor = Cursors.IBeam;
    }
    
    readonly string sentenceText;
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

  void LoadDictionaryEntryDetails(WordDictionary dictionary, uint id, int headwordIndex)
  {
    dictDetails.Clear();

    Entry entry = dictionary.GetEntryById(id);
    Word headword = entry.Headwords[headwordIndex];

    // add entry summary
    dictDetails.AppendText("Entry Summary", FontStyle.Bold);
    dictDetails.AppendText(" (from "+dictionary.Name+" dictionary)\n");
    RenderDictionaryEntryTo(dictionary, entry, headwordIndex, dictDetails, false);

    // add kanji summary
    dictDetails.AppendText("\n");
    dictDetails.AppendText("Kanji Summary\n", FontStyle.Bold);
    dictDetails.AppendText(GetInformationAboutKanji(headword.Text) + "\n");

    // add example sentences
    const int exampleLimit = 50;
    List<uint> exampleIds = new List<uint>(
      JapaneseSearchStrategy.Instance.Search(App.Examples, entry.Headwords[headwordIndex].Text,
                                          SearchFlag.ExactMatch|SearchFlag.SearchHeadwords|SearchFlag.SearchReadings));
    if(exampleIds.Count != 0)
    {
      dictDetails.AppendText("\n");
      dictDetails.AppendText("Example Sentences\n", FontStyle.Bold);
      if(exampleIds.Count <= exampleLimit)
      {
        dictDetails.AppendText(exampleIds.Count+" example sentence(s)\n\n");
      }
      else
      {
        dictDetails.AppendText(exampleLimit+" example sentences randomly chosen from "+exampleIds.Count+"\n\n");
        
        // shuffle the sentence IDs
        for(int i=0; i<exampleIds.Count-1; i++)
        {
          int other = rand.Next(i, exampleIds.Count);
          uint temp = exampleIds[i];
          exampleIds[i] = exampleIds[other];
          exampleIds[other] = temp;
        }
        // then remove all after the example limit
        exampleIds.RemoveRange(exampleLimit, exampleIds.Count-exampleLimit);
      }

      foreach(ExampleSentence example in new ExampleIterator(App.Examples, exampleIds))
      {
        RenderExampleSentenceTo(dictDetails, example);
      }
    }

    dictDetails.DeselectAll();
    tabControl.SelectedTab = dictionaryPage;
  }

  void PerformDictionarySearch(string query)
  {
    if(string.IsNullOrEmpty(query)) throw new ArgumentException();

    dictInput.Text = query;
    dictResults.Clear();
    tabControl.SelectedTab = dictionaryPage;

    try
    {
      const int threshold = 100;
      int numberOfResults = 0;
      IEnumerable<uint> entryIds =
        JapaneseSearchStrategy.Instance.Search(App.WordDict, query, SearchFlag.MatchStart | SearchFlag.SearchAll);
      foreach(Entry entry in new EntryIterator(App.WordDict, entryIds))
      {
        if(++numberOfResults > threshold)
        {
          statusText.Text = "Search results truncated at "+threshold+" results.";
          break;
        }
        RenderDictionaryEntryTo(App.WordDict, entry, -1, dictResults, true);
      }
      
      if(numberOfResults <= threshold)
      {
        statusText.Text = "The search returned "+numberOfResults+" result(s).";
      }
    }
    catch(ArgumentException e)
    {
      ShowToolTip(dictInput, "Error", e.Message);
    }
  }

  void PerformExampleSearch(string query)
  {
    if(string.IsNullOrEmpty(query)) throw new ArgumentException();

    exampleInput.Text = query;
    exampleResults.Clear();
    tabControl.SelectedTab = examplePage;

    try
    {
      const int threshold = 500;
      int numberOfResults = 0;
      
      IEnumerable<uint> entryIds =
        JapaneseSearchStrategy.Instance.Search(App.Examples, query, SearchFlag.MatchStart | SearchFlag.SearchAll);
      foreach(ExampleSentence example in new ExampleIterator(App.Examples, entryIds))
      {
        if(++numberOfResults > threshold)
        {
          statusText.Text = "Search results truncated at "+threshold+" results.";
          break;
        }
        
        RenderExampleSentenceTo(exampleResults, example);
      }

      if(numberOfResults <= threshold)
      {
        statusText.Text = "The search returned "+numberOfResults+" result(s).";
      }
    }
    catch(ArgumentException e)
    {
      ShowToolTip(dictInput, "Error", e.Message);
    }
  }
  
  void PerformTranslation(string text)
  {
    if(string.IsNullOrEmpty(text)) throw new ArgumentException();

    transInput.Text = text;
    transOutput.Clear();
    tabControl.SelectedTab = translatePage;
    
    TranslatedWord[] words = WordTranslator.TranslateWordsInJapaneseText(text);

    int lastEnd = 0; // the end of the previous word
    bool oddWord = false;
    foreach(TranslatedWord word in words)
    {
      if(lastEnd < word.Position) transOutput.AppendText(text.Substring(lastEnd, word.Position-lastEnd));
      transOutput.AppendText(text.Substring(word.Position, word.Length),
                             oddWord ? Color.Blue : Color.DarkGreen,
                             word.Inflected ? FontStyle.Italic : FontStyle.Regular);
      lastEnd = word.Position + word.Length;
      oddWord = !oddWord;
    }
    if(lastEnd < text.Length) transOutput.AppendText(text.Substring(lastEnd, text.Length-lastEnd));
    transOutput.AppendText("\n\n");
    
    foreach(TranslatedWord word in words)
    {
      List<Entry> entries = new List<Entry>(new EntryIterator(word.Dictionary, word.EntryIds));
      WordDictionary.SortEntriesByFrequency(entries);
      // TODO: instead of rendering each entry on its own line, merge multiple entries into one. eg:
      // 私 /わたし, わたくし/ (Noun;AdjNo) I; myself; private affairs; 
      // 私 /あたし, あたい/ (Noun;AdjNo;Female) I; 
      // 私 /あっし, わっち, わっし/ (Noun;AdjNo) I (mainly used by working men); myself; 
      // 私, 儂 /わし/ (Noun;AdjNo) I; me (used by elderly); 
      // becomes
      // 私 /あたい, あたし, わし, わたくし, わたし/ (あたい) (adj-no,n,fem) I; (わし) (adj-no,n) I; me (used by elderly); (わたし; わたくし) (adj-no,n) I; myself; private affairs; SP
      foreach(Entry entry in entries)
      {
        RenderDictionaryEntryTo(word.Dictionary, entry, -1, transOutput, true);
      }
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
      if(makeHeadwordsClickable)
      {
        textBox.AddRegion(headword, new HeadwordTextRegion(this, dictionary, entry.ID, i));
      }
      else
      {
        textBox.AppendText(headword, Color.DarkGreen);
      }
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

    textBox.AddRegion(resultStart, textBox.TextLength-resultStart, new ResultTextRegion(this, entry.ID));
    textBox.AppendText("\n");
  }

  void RenderExampleSentenceTo(RicherTextBox textBox, ExampleSentence example)
  {
    textBox.AppendText(example.Japanese+"[");
    textBox.AddRegion(" T ", new TranslateExampleSentenceRegion(this, example.Japanese));
    textBox.AppendText("]\n");
    textBox.AppendText(example.English+"\n");
  }

  void ShowToolTip(Control control, string title, string text)
  {
    toolTip.ToolTipTitle = title;
    toolTip.Show(text, control, 50, -50, 3000);
  }

  readonly List<string> statusTexts = new List<string>();
  readonly Random rand = new Random();
  short globalKeyAtom;
  bool isActive;

  static readonly Font jpFont = new Font("Verdana", 10f, FontStyle.Regular);
  static readonly Font enFont = new Font("Verdana", 9f, FontStyle.Regular);
}


} // namespace Jappy