using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Jappy
{

#region TextRegion
abstract class TextRegion
{
  public Font Font
  {
    get { return font; }
    set { font = value; }
  }

  public Color Color
  {
    get { return Color; }
    set { color = value; }
  }

  Font font;
  Color color;
}
#endregion

#region Block
class Block : TextRegion
{
  public void AppendSpan(Span span)
  {
    if(firstSpan == null)
    {
      firstSpan = span;
    }
    else
    {
      Span previous = firstSpan;
      while(previous.nextSpan != null) previous = previous.nextSpan;
      previous.nextSpan = span;
    }
  }

  public void AppendText(string text, Font font, Color color)
  {
    throw new NotImplementedException();
  }

  Span firstSpan;
}
#endregion

#region Span
class Span : TextRegion
{
  public void AppendText(string text)
  {
    throw new NotImplementedException();
  }

  internal Span nextSpan;
}
#endregion

#region InfoRenderer
class InfoRenderer : Control
{
  public int SelectionStart
  {
    get { return selStart; }
    set { Select(value, SelectionLength); }
  }

  public int SelectionLength
  {
    get { return selLength; }
    set { Select(SelectionStart, value); }
  }

  public override string Text
  {
    get
    {
      if(text == null)
      {
        throw new NotImplementedException();
      }
      return text;
    }
    set
    {
      throw new NotImplementedException();
    }
  }

  public int TextLength
  {
    get
    {
      return Text.Length;
    }
  }

  public void AppendBlock(Block block)
  {
    if(block == null) throw new ArgumentNullException();
    blocks.Add(block);
  }

  public void AppendSpan(Span span)
  {
    if(span == null) throw new ArgumentNullException();
    if(blocks.Count == 0) blocks.Add(new Block());
    blocks[blocks.Count-1].AppendSpan(span);
  }

  public void AppendText(string text) { AppendText(text, this.Font, this.ForeColor); }
  public void AppendText(string text, Font font) { AppendText(text, font, this.ForeColor); }
  public void AppendText(string text, Color color) { AppendText(text, this.Font, color); }
  public void AppendText(string text, FontStyle style) { AppendText(text, this.ForeColor, style); }

  public void AppendText(string text, Color color, FontStyle style)
  {
    AppendText(text, CloneFontWithStyle(this.Font, style), color);
  }

  public void AppendText(string text, Font font, Color color)
  {
    if(text == null || font == null) throw new ArgumentNullException();
    if(blocks.Count == 0) blocks.Add(new Block());
    blocks[blocks.Count-1].AppendText(text, font, color);
  }

  public void Clear()
  {
    blocks.Clear();
    text = null;
    selStart = selLength = 0;
  }

  public void Select(int start, int length)
  {
    if(start < 0 || length < 0 || start + length > TextLength) throw new ArgumentOutOfRangeException();
    throw new NotImplementedException();
  }

  static Font CloneFontWithStyle(Font font, FontStyle style)
  {
    return style == font.Style ? font : new Font(font, style);
  }
  
  List<Block> blocks = new List<Block>();
  string text;
  int selStart, selLength;
}
#endregion

} // namespace Jappy