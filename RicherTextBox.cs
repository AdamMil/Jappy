using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Jappy
{

#region TextRegion
public abstract class TextRegion
{
  public event MouseEventHandler MouseClick, MouseEnter, MouseHover;
  public event EventHandler MouseLeave, DoubleClick;

  public virtual Font Font
  {
    get { return TextBox.Font; }
  }

  public virtual Color Color
  {
    get { return TextBox.ForeColor; }
  }

  public virtual FontStyle Style
  {
    get { return Font.Style; }
  }

  public string Text
  {
    get { return TextBox.Text.Substring(Start, Length); }
  }

  public RicherTextBox TextBox
  {
    get { return textBox; }
  }

  public int Start
  {
    get { return start; }
  }
  
  public int Length
  {
    get { return length; }
  }
  
  protected internal virtual void OnDoubleClick(EventArgs e)
  {
    if(DoubleClick != null) DoubleClick(this, e);
  }

  protected internal virtual void OnMouseClick(MouseEventArgs e)
  {
    if(MouseClick != null) MouseClick(this, e);
  }

  protected internal virtual void OnMouseHover(MouseEventArgs e)
  {
    if(MouseHover != null) MouseHover(this, e);
  }

  protected internal virtual void OnMouseEnter(MouseEventArgs e)
  {
    if(MouseEnter != null) MouseEnter(this, e);
  }
  
  protected internal virtual void OnMouseLeave(EventArgs e)
  {
    if(MouseLeave != null) MouseLeave(this, e);
  }

  protected void SelectText()
  {
    TextBox.Select(Start, Length);
  }

  protected void AddStyle(FontStyle style)
  {
    TextBox.BeginUpdate();
    SelectText();
    TextBox.AddStyle(style);
    TextBox.EndUpdate();
  }

  protected void RemoveStyle(FontStyle style)
  {
    TextBox.BeginUpdate();
    SelectText();
    TextBox.RemoveStyle(style);
    TextBox.EndUpdate();
  }

  protected void SetBackColor(Color color)
  {
    TextBox.BeginUpdate();
    SelectText();
    TextBox.SelectionBackColor = color;
    TextBox.EndUpdate();
  }

  internal bool Contains(int charIndex)
  {
    return charIndex >= start && charIndex < start+length;
  }

  internal RicherTextBox textBox;
  internal int start, length;
}
#endregion

#region RicherTextBox
public class RicherTextBox : RichTextBox
{
  public void AddRegion(string text, TextRegion region)
  {
    if(region == null) throw new ArgumentNullException();
    if(region.textBox != null) throw new ArgumentException("This region already belongs to a text box.");
    int start, length;
    AppendText(text, out start, out length);
    AddRegion(start, length, region);
    SelectionFont  = CloneFontWithStyle(region.Font, region.Style);
    SelectionColor = region.Color;
  }

  public void AddRegion(int start, int length, TextRegion region)
  {
    if(region == null) throw new ArgumentNullException();
    if(region.textBox != null) throw new ArgumentException("This region already belongs to a text box.");
    region.start   = start;
    region.length  = length;
    region.textBox = this;
    regions.Add(region);
  }

  public new void AppendText(string text) { AppendText(text, this.Font, this.ForeColor); }
  public void AppendText(string text, Font font) { AppendText(text, font, this.ForeColor); }
  public void AppendText(string text, Color color) { AppendText(text, this.Font, color); }
  public void AppendText(string text, FontStyle style) { AppendText(text, this.ForeColor, style); }

  public void AppendText(string text, Color color, FontStyle style)
  {
    AppendText(text, CloneFontWithStyle(this.Font, style), color);
  }

  public void AppendText(string text, Font font, Color color)
  {
    int start, length;
    AppendText(text, out start, out length);
    SelectionFont  = font;
    SelectionColor = color;
  }

  public void BeginUpdate()
  {
    selStart  = SelectionStart;
    selLength = SelectionLength;
  }

  public void EndUpdate()
  {
    Select(selStart, selLength);
  }

  public void AddStyle(FontStyle style)
  {
    SetStyle(CurrentFont.Style | style);
  }

  public void RemoveStyle(FontStyle style)
  {
    SetStyle(CurrentFont.Style & ~style);
  }

  public void SetStyle(FontStyle style)
  {
    SelectionFont = CloneFontWithStyle(CurrentFont, style);
  }

  public void ToggleStyle(FontStyle style)
  {
    SetStyle(CurrentFont.Style ^ style);
  }

  public new void Clear()
  {
    base.Clear();

    foreach(TextRegion region in regions)
    {
      region.textBox = null;
    }
    regions.Clear();
    inside.Clear();
  }

  public new int GetCharIndexFromPosition(Point pt)
  {
    if(pt.X == 0) pt.X = 1;
    if(pt.Y == 0) pt.Y = 1;
    return base.GetCharIndexFromPosition(pt);
  }

  Font CurrentFont
  {
    get
    {
      Font font = SelectionFont;
      if(font == null) font = this.Font;
      return font;
    }
  }

  void AppendText(string text, out int start, out int length)
  {
    start = this.TextLength;
    base.AppendText(text);
    length = this.TextLength - start;
    this.Select(start, length);
  }

  /// <summary>Returns the shortest region that contains the given index.</summary>
  protected TextRegion GetRegionFromCharIndex(int index)
  {
    TextRegion shortest = null;
    foreach(TextRegion region in regions)
    {
      if(region.Contains(index) && (shortest == null || region.Length < shortest.Length))
      {
        shortest = region;
      }
    }
    return shortest;
  }
  
  protected TextRegion GetRegionFromPosition(Point pt)
  {
    return GetRegionFromCharIndex(GetCharIndexFromPosition(pt));
  }

  protected override void OnMouseDown(MouseEventArgs e)
  {
    base.OnMouseDown(e);
    mouseDown = e.Location;
  }

  protected override void OnMouseUp(MouseEventArgs e)
  {
    base.OnMouseUp(e);
    int xd = e.X-mouseDown.X, yd=e.Y-mouseDown.Y;

    if(xd*xd+yd*yd < 4) // if the mouse moved too far since the button was pressed, don't count it as a click
    {
      TextRegion region = GetRegionFromPosition(mouseDown);
      if(region != null)
      {
        region.OnMouseClick(e);
      }
      
      mouseDown = new Point(-1000, -1000); // we don't handle tracking of individual buttons, so make sure
                                           // additional button releases don't retrigger the event
    }
  }
  protected override void OnDoubleClick(EventArgs e)
  {
    base.OnDoubleClick(e);

    TextRegion region = GetRegionFromPosition(this.PointToClient(Cursor.Position));
    if(region != null)
    {
      region.OnDoubleClick(e);
    }
  }

  protected override void OnMouseHover(EventArgs e)
  {
    base.OnMouseHover(e);

    Point position = this.PointToClient(Cursor.Position);
    TextRegion region = GetRegionFromPosition(position);
    if(region != null)
    {
      region.OnMouseHover(new MouseEventArgs(MouseButtons.None, 0, position.X, position.Y, 0));
    }
  }

  protected override void OnMouseMove(MouseEventArgs e)
  {
    base.OnMouseMove(e);

    // only fire enter events when no button is pressed, because, for instance, when the user is doing a drag select,
    // the selection won't actually be updated until the user releases the mouse button. in that case, if we fired the
    // an event, a region might update the selection, ruining the user's attempt to drag-select
    if(e.Button != MouseButtons.None) return;

    int index = GetCharIndexFromPosition(e.Location);
    for(int i=0; i<inside.Count; i++)
    {
      if(!inside[i].Contains(index))
      {
        inside[i].OnMouseLeave(e);
        inside.RemoveAt(i--);
      }
    }

    foreach(TextRegion region in regions)
    {
      if(region.Contains(index) && !inside.Contains(region))
      {
        inside.Add(region);
        region.OnMouseEnter(e);
      }
    }
  }

  protected override void OnMouseLeave(EventArgs e)
  {
    base.OnMouseLeave(e);

    if(inside.Count != 0)
    {
      foreach(TextRegion region in inside)
      {
        region.OnMouseLeave(e);
      }
      inside.Clear();
    }
  }

  static Font CloneFontWithStyle(Font font, FontStyle style)
  {
    return style == font.Style ? font : new Font(font, style);
  }

  List<TextRegion> regions = new List<TextRegion>();
  List<TextRegion> inside = new List<TextRegion>();
  
  Point mouseDown;
  int selStart, selLength;
}
#endregion

} // namespace Jappy