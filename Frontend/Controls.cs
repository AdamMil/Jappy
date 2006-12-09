using System;
using System.ComponentModel;
using System.Drawing;
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
  }

  public static readonly Style JpStyle, UnderlinedStyle, BoldStyle;
}

} // namespace Jappy