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

} // namespace Jappy