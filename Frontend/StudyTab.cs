using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using Jappy.Backend;

namespace Jappy
{

#region StudyTab
partial class StudyTab : TabBase
{
  public StudyTab()
  {
    InitializeComponent();
    EnableStudyMenu(false);
  }
  
  static StudyTab()
  {
    phraseStyle = new Style(UI.JpStyle);
    phraseStyle.ForeColor = Color.Green;
    phraseStyle.FontSize  = 10;
  }

  public bool IsListLoaded
  {
    get { return list != null; }
  }

  public StudyList List
  {
    get { return list; }
  }

  public override DocumentRenderer OutputArea
  {
    get { return output; }
  }

  public bool AddEntry()
  {
    AssertListLoaded();

    StudyListEntryDialog dialog = new StudyListEntryDialog();
    if(dialog.ShowDialog() == DialogResult.OK)
    {
      StudyList.Item item = new StudyList.Item();
      dialog.SaveItem(item);
      AddEntry(item);
      flashCardsMenuItem.Enabled = true;
      return true;
    }
    else
    {
      return false;
    }
  }

  public void AddEntry(StudyList.Item item)
  {
    list.Items.Add(item);
    UpdateStatusText();
  }

  public bool CreateNewList()
  {
    if(TryCloseList())
    {
      StudyListDialog dialog = new StudyListDialog();
      if(dialog.ShowDialog() == DialogResult.OK)
      {
        list = new StudyList();
        dialog.SaveList(list);
        OnListLoaded();
        return true;
      }
    }
    
    return false;
  }

  public bool LoadList()
  {
    if(!TryCloseList()) return false;
    
    OpenFileDialog fd = new OpenFileDialog();
    fd.DefaultExt       = "study";
    fd.Filter           = "Study lists (*.study)|*.study|All files (*.*)|*.*";
    fd.InitialDirectory = StudyListPath;
    fd.RestoreDirectory = true;
    fd.Title            = "Load study list from...";
    
    if(fd.ShowDialog() != DialogResult.OK) return false;
    
    try
    {
      using(Stream stream = File.OpenRead(fd.FileName))
      {
        list = new StudyList();
        list.Load(stream);
        listFile = fd.FileName;
        OnListLoaded();
        return true;
      }
    }
    catch(Exception e)
    {
      MessageBox.Show("An error occured while loading the study list.\n"+e.Message, "Error occurred",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
      list = null;
      return false;
    }
  }

  public void ResetAccuracy()
  {
    AssertListLoaded();
    foreach(StudyList.Item item in list.Items) item.ShownCount = item.CorrectCount = 0;
  }

  public bool SaveList()
  {
    return SaveList(false);
  }

  public bool SaveList(bool newFile)
  {
    AssertListLoaded();

    if(newFile || listFile == null)
    {
      SaveFileDialog fd = new SaveFileDialog();
      fd.DefaultExt       = "study";
      fd.Filter           = "Study lists (*.study)|*.study|All files (*.*)|*.*";
      fd.RestoreDirectory = true;
      fd.Title            = "Save study list as...";

      if(listFile == null)
      {
        fd.FileName = 
          new System.Text.RegularExpressions.Regex(@"[^\w ]", System.Text.RegularExpressions.RegexOptions.Singleline)
            .Replace(list.Name, "") + ".study";
        fd.InitialDirectory = StudyListPath;
      }
      else
      {
        fd.FileName = listFile;
        fd.InitialDirectory = Path.GetDirectoryName(listFile);
      }

      if(fd.ShowDialog() != DialogResult.OK) return false;
      
      listFile = fd.FileName;
    }
    
    try
    {
      list.Save(File.Open(listFile, FileMode.Create, FileAccess.Write));
      return true;
    }
    catch(Exception e)
    {
      MessageBox.Show("An error occured while saving the study list.\n"+e.Message, "Error occurred",
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
      return false;
    }
  }

  public void StartStudying()
  {
    AssertListLoaded();
    if(List.Items.Count == 0) throw new InvalidOperationException("The study list is empty.");

    CurrentState = State.Studying;
    output.Clear();
    output.Document.Root.Children.Add(new TextNode("Currently studying..."));

    new StudyDialog(List).ShowDialog();
    CurrentState = State.ShowingItems;
  }

  public bool TryCloseList()
  {
    DialogResult result = !IsListLoaded || !list.IsModified ? DialogResult.No :
                            MessageBox.Show("Save changes to the current study list?", "Save changes?",
                                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                            MessageBoxDefaultButton.Button1);
    if(result == DialogResult.Yes)
    {
      return SaveList();
    }
    else if(result == DialogResult.No)
    {
      list        = null;
      listFile    = null;
      EnableStudyMenu(false);
      CurrentState = State.Unloaded;
      return true;
    }
    else
    {
      return false;
    }
  }

  protected internal override void OnActivate()
  {
    ToolStripManager.Merge(menuStrip, Form.MainMenuStrip);

    ToolStripMenuItem fileMenu = (ToolStripMenuItem)Form.MainMenuStrip.Items[0];
    fileMenu.DropDownOpening += fileMenu_DropDownOpening;
  }

  protected internal override void OnDeactivate()
  {
    ToolStripMenuItem fileMenu = (ToolStripMenuItem)Form.MainMenuStrip.Items[0];
    fileMenu.DropDownOpening -= fileMenu_DropDownOpening;

    ToolStripManager.RevertMerge(Form.MainMenuStrip, menuStrip);
  }

  enum State
  {
    Unloaded, ShowingItems, Studying
  }

  sealed class EditNode : LinkNode
  {
    public EditNode(StudyList.Item item) : base("edit")
    {
      this.item = item;
    }

    protected internal override void OnMouseClick(object sender, MouseEventArgs e)
    {
      base.OnMouseClick(sender, e);

      if(e.Button == MouseButtons.Left)
      {
        StudyListEntryDialog dialog = new StudyListEntryDialog();
        dialog.LoadItem(item);
        if(dialog.ShowDialog() == DialogResult.OK)
        {
          dialog.SaveItem(item);
        }
      }
    }

    readonly StudyList.Item item;
  }

  sealed class DeleteNode : LinkNode
  {
    public DeleteNode(StudyList.Item item) : base("delete")
    {
      this.item = item;
    }

    protected internal override void OnMouseClick(object sender, MouseEventArgs e)
    {
      base.OnMouseClick(sender, e);

      if(e.Button == MouseButtons.Left &&
         MessageBox.Show("Are you sure you want to delete this item?", "Delete item?", MessageBoxButtons.YesNo,
                         MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
      {
        item.List.Items.Remove(item);
      }
    }

    readonly StudyList.Item item;
  }

  State CurrentState
  {
    get { return state; }
    set
    {
      if(state != value)
      {
        state = value;
        UpdateDocument();
      }
    }
  }

  string StudyListPath
  {
    get
    {
      string sep  = Path.DirectorySeparatorChar.ToString();
      string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                 "AdamMil"+sep+"Jappy"+sep+"StudyLists");
      if(!Directory.Exists(path)) Directory.CreateDirectory(path);
      return path;
    }
  }

  void AssertListLoaded()
  {
    if(!IsListLoaded) throw new InvalidOperationException("A list is not loaded.");
  }

  void EnableStudyMenu(bool enable)
  {
    RecursivelyEnable(studyMenu, enable);
  }

  void OnListLoaded()
  {
    CurrentState = State.ShowingItems;
    EnableStudyMenu(true);
    list.Modified += delegate { OnListModified(); };
    flashCardsMenuItem.Enabled = list.Items.Count != 0;
  }

  void OnListModified()
  {
    UpdateDocument();
    flashCardsMenuItem.Enabled = list.Items.Count != 0;
  }

  void OutputListEntries()
  {
    AssertListLoaded();

    output.Clear();
    DocumentNode root = output.Document.Root;
    foreach(StudyList.Item item in list.Items)
    {
      root.Children.Add(new TextNode(item.Phrase, phraseStyle));
      root.Children.Add(new TextNode(" ["));
      root.Children.Add(new EditNode(item));
      root.Children.Add(new TextNode("] ["));
      root.Children.Add(new DeleteNode(item));
      root.Children.Add(new TextNode("]\n"+item.Meanings+"\n\n"));
    }
  }

  void UpdateDocument()
  {
    switch(CurrentState)
    {
      case State.Unloaded:
        output.Clear();
        break;

      case State.ShowingItems:
        OutputListEntries();
        break;

      case State.Studying: // don't update the document while studying
        break;

      default: throw new NotImplementedException();
    }
  }

  void UpdateStatusText()
  {
    string statusText = null;
    
    if(!IsListLoaded)
    {
      statusText = "Load a study list by choosing Open from the File menu.";
    }
    else if(CurrentState == State.ShowingItems)
    {
      if(list.Items.Count == 0)
      {
        statusText = "Add items to the study list by pressing Ctrl-N or right-clicking on a headword.";
      }
      else
      {
        statusText = "Edit list items by clicking the links provided, or use the Study menu to study.";
      }
    }
    
    if(statusText != null) Form.SetStatusText(output, statusText);
  }

  void fileMenu_DropDownOpening(object sender, EventArgs e)
  {
    newEntryMenuItem.Enabled = saveListMenuItem.Enabled = saveListAsMenuItem.Enabled = IsListLoaded;
  }

  void newEmptyListMenuItem_Click(object sender, EventArgs e)
  {
    CreateNewList();
  }

  void loadStudylistMenuItem_Click(object sender, EventArgs e)
  {
    LoadList();
  }

  void saveListMenuItem_Click(object sender, EventArgs e)
  {
    SaveList();
  }

  void saveListAsMenuItem_Click(object sender, EventArgs e)
  {
    SaveList(true);
  }

  void newEntryMenuItem_Click(object sender, EventArgs e)
  {
    AddEntry();
  }

  void resetAccuracyMenuItem_Click(object sender, EventArgs e)
  {
    ResetAccuracy();
  }

  void flashCardsMenuItem_Click(object sender, EventArgs e)
  {
    StartStudying();
  }

  void output_MouseEnter(object sender, EventArgs e)
  {
    UpdateStatusText();
  }

  void output_MouseLeave(object sender, System.EventArgs e)
  {
    control_RestoreStatusText(sender, e);
  }

  void output_MouseClick(object sender, MouseEventArgs e)
  {
    doc_MouseClick(sender, e);
  }

  void newListFromKanji_Click(object sender, EventArgs e)
  {
    if(!TryCloseList()) return;

    Level level = (Level)((ToolStripMenuItem)sender).Tag;

    list = new StudyList();
    list.Name = level.ToString() + " level kanji";

    foreach(char c in App.CharDict.RetrieveAll())
    {
      Kanji kanji;
      App.CharDict.TryGetKanjiData(c, out kanji);

      if(kanji.Level == level && kanji.Readings != null)
      {
        StudyList.Item item = new StudyList.Item();
        item.Phrase = c.ToString();

        StringBuilder sb = new StringBuilder();
        foreach(Backend.Reading reading in kanji.Readings)
        {
          if(reading.Type == Jappy.Backend.ReadingType.Kun || reading.Type == Jappy.Backend.ReadingType.On)
          {
            if(sb.Length != 0) sb.Append(", ");
            sb.Append(reading.Text);
          }
        }
        item.Readings = sb.Length == 0 ? "<unknown reading>" : sb.ToString();

        sb.Length = 0;
        foreach(Backend.Reading reading in kanji.Readings)
        {
          if(reading.Type == Jappy.Backend.ReadingType.English)
          {
            if(sb.Length != 0) sb.Append(", ");
            sb.Append(reading.Text);
          }
        }
        item.Meanings = sb.Length == 0 ? "<unknown meaning>" : sb.ToString();
        
        list.Items.Add(item);
      }
    }
    
    OnListLoaded();
  }

  void settingsMenuItem_Click(object sender, EventArgs e)
  {
    StudyListDialog dialog = new StudyListDialog();
    dialog.LoadList(list);

    if(dialog.ShowDialog() == DialogResult.OK)
    {
      dialog.SaveList(list);
    }
  }

  StudyList list;
  string listFile;
  State state;

  static void RecursivelyEnable(ToolStripMenuItem item, bool enable)
  {
    item.Enabled = enable;
    foreach(ToolStripMenuItem child in item.DropDownItems)
    {
      RecursivelyEnable(child, enable);
    }
  }
  
  static readonly Style phraseStyle;
}
#endregion

#region StudyList
class StudyList
{
  public StudyList()
  {
    items = new ItemCollection(this);
  }

  #region Item
  public class Item
  {
    public Item() { }

    internal Item(XmlElement item)
    {
      XmlAttribute attr = item.Attributes["shown"];
      ShownCount = attr == null ? 0 : int.Parse(attr.Value);

      attr = item.Attributes["correct"];
      CorrectCount = attr == null ? 0 : int.Parse(attr.Value);

      Phrase = item.SelectSingleNode("phrase").InnerText;
      Meanings = item.SelectSingleNode("meanings").InnerText;
      
      XmlElement child = (XmlElement)item.SelectSingleNode("readings");
      if(child != null) Readings = child.InnerText;

      child = (XmlElement)item.SelectSingleNode("example/source");
      if(child != null) ExampleSource = child.InnerText;

      child = (XmlElement)item.SelectSingleNode("example/destination");
      if(child != null) ExampleDest = child.InnerText;
    }

    public StudyList List
    {
      get { return owningList; }
    }

    public double CorrectRate
    {
      get { return ShownCount == 0 ? 0 : CorrectCount / (double)ShownCount; }
    }

    public string Phrase
    {
      get { return phrase; }
      set
      {
        if(value != phrase)
        {
          phrase = value;
          SetModified();
        }
      }
    }

    public string Readings
    {
      get { return readings; }
      set
      {
        if(value != readings)
        {
          readings = value;
          SetModified();
        }
      }
    }

    public string Meanings
    {
      get { return meanings; }
      set
      {
        if(value != meanings)
        {
          meanings = value;
          SetModified();
        }
      }
    }

    public string ExampleSource
    {
      get { return exampleSource; }
      set
      {
        if(value != exampleSource)
        {
          exampleSource = value;
          SetModified();
        }
      }
    }

    public string ExampleDest
    {
      get { return exampleDest; }
      set
      {
        if(value != exampleDest)
        {
          exampleDest = value;
          SetModified();
        }
      }
    }

    public int ShownCount
    {
      get { return shownCount; }
      set
      {
        if(value != shownCount)
        {
          shownCount = value;
          SetModified();
        }
      }
    }

    public int CorrectCount
    {
      get { return correctCount; }
      set
      {
        if(value != correctCount)
        {
          correctCount = value;
          SetModified();
        }
      }
    }

    internal void Write(XmlWriter writer)
    {
      writer.WriteStartElement("item");
      writer.WriteAttributeString("shown", ShownCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
      writer.WriteAttributeString("correct", CorrectCount.ToString(System.Globalization.CultureInfo.InvariantCulture));

      writer.WriteStartElement("phrase");
      writer.WriteString(Phrase);
      writer.WriteEndElement();
      
      if(Readings != null)
      {
        writer.WriteStartElement("readings");
        writer.WriteString(Readings);
        writer.WriteEndElement();
      }
      
      writer.WriteStartElement("meanings");
      writer.WriteString(Meanings);
      writer.WriteEndElement();
      
      if(ExampleSource != null || ExampleDest != null)
      {
        writer.WriteStartElement("example");
        if(ExampleSource != null)
        {
          writer.WriteStartElement("source");
          writer.WriteString(ExampleSource);
          writer.WriteEndElement();
        }
        if(ExampleDest != null)
        {
          writer.WriteStartElement("destination");
          writer.WriteString(ExampleDest);
          writer.WriteEndElement();
        }
        writer.WriteEndElement();
      }
      
      writer.WriteEndElement();
    }

    void SetModified()
    {
      if(owningList != null)
      {
        owningList.SetModified();
      }
    }

    internal StudyList owningList;
    string phrase, readings, meanings, exampleSource, exampleDest;
    int shownCount, correctCount;
  }
  #endregion

  #region ItemCollection
  public sealed class ItemCollection : Collection<Item>
  {
    public ItemCollection(StudyList owner)
    {
      this.owner = owner;
    }

    protected override void ClearItems()
    {
      base.ClearItems();
      foreach(Item item in this) item.owningList = null;
      owner.SetModified();
    }

    protected override void InsertItem(int index, Item item)
    {
      AssertValidItem(item);
      base.InsertItem(index, item);
      item.owningList = owner;
      owner.SetModified();
    }

    protected override void RemoveItem(int index)
    {
      this[index].owningList = null;
      base.RemoveItem(index);
      owner.SetModified();
    }

    protected override void SetItem(int index, Item item)
    {
      if(item == this[index]) return;

      AssertValidItem(item);
      this[index].owningList = null;
      base.SetItem(index, item);
      item.owningList = owner;
      owner.SetModified();
    }

    readonly StudyList owner;

    static void AssertValidItem(Item item)
    {
      if(item == null) throw new ArgumentNullException();
      if(item.List != null) throw new ArgumentException("This item already belongs to a study list.");
    }
  }
  #endregion

  public event EventHandler Modified;

  public string Name
  {
    get { return name; }
    set
    {
      if(value == null) throw new ArgumentNullException();
      if(value != name)
      {
        name = value;
        SetModified();
      }
    }
  }

  public bool HintExample
  {
    get { return hintExamples; }
    set
    {
      if(value != hintExamples)
      {
        hintExamples = value;
        SetModified();
      }
    }
  }

  public bool HintReadings
  {
    get { return hintReadings; }
    set
    {
      if(value != hintReadings)
      {
        hintReadings = value;
        SetModified();
      }
    }
  }

  public bool ShowReversedCards
  {
    get { return showReversed; }
    set
    {
      if(value != showReversed)
      {
        showReversed = value;
        SetModified();
      }
    }
  }

  public ItemCollection Items
  {
    get { return items; }
  }
  
  public bool IsModified
  {
    get { return isModified; }
  }
  
  public void Load(Stream stream)
  {
    ValidationEventHandler validate = delegate(object sender, ValidationEventArgs e) { throw e.Exception; };
    
    XmlSchema schema = XmlSchema.Read(new StringReader(Properties.Resources.StudyListSchema), validate);

    XmlDocument doc = new XmlDocument();
    doc.Schemas.Add(schema);
    doc.Load(stream);
    doc.Validate(validate);

    XmlElement el = doc.DocumentElement;

    if(int.Parse(el.Attributes["version"].Value) != 1)
    {
      throw new ArgumentException("This study list was created by a different version of the dictionary.");
    }

    Name = el.Attributes["name"].Value;

    XmlAttribute attr = el.Attributes["hintReadings"];
    hintReadings = attr != null && XmlConvert.ToBoolean(attr.Value);

    attr = el.Attributes["hintExamples"];
    hintExamples = attr != null && XmlConvert.ToBoolean(attr.Value);

    attr = el.Attributes["showReversed"];
    showReversed = attr != null && XmlConvert.ToBoolean(attr.Value);

    el = (XmlElement)el.SelectSingleNode("items");

    items.Clear();
    foreach(XmlElement item in el.ChildNodes)
    {
      items.Add(new Item(item));
    }

    isModified = false;
  }

  public void Save(Stream stream)
  {
    XmlWriterSettings settings = new XmlWriterSettings();
    settings.Indent = true;

    XmlWriter writer = XmlWriter.Create(stream, settings);
    writer.WriteStartElement("studyList");
    writer.WriteAttributeString("version", "1");
    writer.WriteAttributeString("name", Name == null ? "" : Name);
    writer.WriteAttributeString("hintReadings", hintReadings ? "true" : "false");
    writer.WriteAttributeString("hintExamples", hintExamples ? "true" : "false");
    writer.WriteAttributeString("showReversed", hintExamples ? "true" : "false");
    writer.WriteStartElement("items");
    foreach(Item item in items)
    {
      item.Write(writer);
    }
    writer.WriteEndElement();
    writer.WriteEndElement();
    writer.Flush();

    isModified = false;
  }
  
  void SetModified()
  {
    isModified = true;
    if(Modified != null) Modified(this, EventArgs.Empty);
  }

  readonly ItemCollection items;
  string name = string.Empty;
  bool isModified, hintReadings, hintExamples, showReversed;
}
#endregion

} // namespace Jappy