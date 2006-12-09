using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Jappy
{

#region TextSpan
struct TextSpan
{
  public TextSpan(int start, int length)
  {
    Start  = start;
    Length = length;
  }

  public int Start, Length;
}
#endregion

#region DocumentNode
class DocumentNode
{
  internal DocumentNode(bool blockNode)
  {
    this.nodes = new NodeCollection(this);
    this.index = -1;
    this.isBlockNode = blockNode;
  }

  public bool IsBlockNode
  {
    get { return isBlockNode; }
  }

  #region Nodes
  #region NodeCollection
  public sealed class NodeCollection : System.Collections.ObjectModel.Collection<DocumentNode>
  {
    internal NodeCollection(DocumentNode owner)
    {
      this.owner = owner;
    }
    
    protected override void ClearItems()
    {
      foreach(DocumentNode node in this)
      {
        node.OnNodeRemoved(owner);
      }
      base.ClearItems();
      owner.NodeChanged();
    }

    protected override void InsertItem(int index, DocumentNode item)
    {
      ValidateNewItem(item);

      for(int i=index; i<Count; i++)
      {
        this[i].index++;
      }

      base.InsertItem(index, item);
      item.OnNodeAdded(owner, index);
      owner.NodeChanged();
    }

    protected override void RemoveItem(int index)
    {
      for(int i=index+1; i<Count; i++)
      {
        this[i].index--;
      }
      this[index].OnNodeRemoved(owner);
      base.RemoveItem(index);
      owner.NodeChanged();
    }

    protected override void SetItem(int index, DocumentNode item)
    {
      this[index].OnNodeRemoved(owner);
      ValidateNewItem(item);
      base.SetItem(index, item);
      item.OnNodeAdded(owner, index);
      owner.NodeChanged();
    }

    void ValidateNewItem(DocumentNode node)
    {
      if(node == null) throw new ArgumentNullException();
      if(node.parentNode != null) throw new ArgumentException("This node already has a parent.");

      if(node.document != null && node.document != owner.document)
      {
        throw new ArgumentException("This node belongs to another document.");
      }
    }

    DocumentNode owner;
  }
  #endregion

  public NodeCollection Children
  {
    get { return nodes; }
  }

  public DocumentNode FirstChild
  {
    get { return nodes.Count == 0 ? null : nodes[0]; }
  }
  
  public DocumentNode LastChild
  {
    get { return nodes.Count == 0 ? null : nodes[nodes.Count-1]; }
  }
  
  public DocumentNode PreviousSibling
  {
    get { return parentNode == null || index == 0 ? null : parentNode.nodes[index-1]; }
  }

  public DocumentNode NextSibling
  {
    get { return parentNode == null || index == parentNode.nodes.Count-1 ? null : parentNode.nodes[index+1]; }
  }
  
  public DocumentNode Parent
  {
    get { return parentNode; }
  }
  
  public int Index
  {
    get
    {
      if(parentNode == null) throw new InvalidOperationException("This node does not have a parent.");
      return index;
    }
  }

  public Document Document
  {
    get { return document; }
  }

  public IEnumerable<DocumentNode> EnumerateDescendants()
  {
    return EnumerateDescendants(false);
  }

  public IEnumerable<DocumentNode> EnumerateDescendants(bool includeThis)
  {
    if(includeThis) yield return this;
    foreach(DocumentNode child in Children)
    {
      foreach(DocumentNode desc in child.EnumerateDescendants(true))
      {
        yield return desc;
      }
    }
  }

  protected virtual void OnNodeAdded(DocumentNode parent, int index)
  {
    this.parentNode = parent;
    this.index      = index;
    RecursivelySetDocument(parentNode.document);
  }

  protected virtual void OnNodeRemoved(DocumentNode parent)
  {
    this.parentNode = null;
    this.index      = -1;
    RecursivelySetDocument(null);
  }

  void RecursivelySetDocument(Document document)
  {
    this.document = document;
    foreach(DocumentNode child in nodes)
    {
      child.RecursivelySetDocument(document);
    }
  }

  DocumentNode parentNode;
  NodeCollection nodes;
  Document document;
  int index;
  #endregion

  #region Styles
  public void AddStyle(Style style)
  {
    if(FindStyle(style) == -1)
    {
      if(styles == null) styles = new List<Style>(2);
      styles.Add(style);

      if(IsFontRelated(style)) OnFontChanged();
      NeedRepaint();
    }
  }

  public void RemoveStyle(Style style)
  {
    int index = FindStyle(style);
    if(index != -1)
    {
      if(IsFontRelated(styles[index])) OnFontChanged();
      styles.RemoveAt(index);
      NeedRepaint();
    }
  }

  public void ReplaceStyle(Style style, Style newStyle)
  {
    if(newStyle == null) throw new ArgumentNullException();
    if(style == newStyle) return;

    int index = FindStyle(style);
    if(index == -1)
    {
      AddStyle(newStyle);
    }
    else
    {
      if(IsFontRelated(newStyle) || IsFontRelated(styles[index])) OnFontChanged();
      styles[index] = newStyle;
      NeedRepaint();
    }
  }

  public void SetStyles(params Style[] styles)
  {
    if(this.styles == null)
    {
      if(styles.Length == 0) return;
      this.styles = new List<Style>();
    }
    else
    {
      this.styles.Clear();
    }
    this.styles.AddRange(styles);
    NeedRepaint();
  }

  int FindStyle(Style style)
  {
    if(style == null) throw new ArgumentNullException();
    return styles == null ? -1 : styles.IndexOf(style);
  }

  List<Style> styles;
  #endregion

  #region Effective style values
  public Cursor GetEffectiveCursor()
  {
    DocumentNode node = this;
    while(node != null)
    {
      if(node.styles != null)
      {
        for(int i=node.styles.Count-1; i>=0; i--)
        {
          if(node.styles[i].Cursor != null) return node.styles[i].Cursor;
        }
      }
      node = node.parentNode;
    }

    if(document != null && document.DefaultStyle.Cursor != null)
    {
      return document.DefaultStyle.Cursor;
    }

    return Style.Default.Cursor;
  }
  
  public Font GetEffectiveFont()
  {
    if(cachedFont == null)
    {
      string fontName = null;
      float? fontSize = null;
      FontStyle? fontStyle = null;

      DocumentNode node = this;
      while(node != null && (fontName == null || !fontSize.HasValue || !fontStyle.HasValue))
      {
        if(node.styles != null)
        {
          for(int i=node.styles.Count-1; i>=0; i--)
          {
            GetFontStyle(node.styles[i], ref fontName, ref fontSize, ref fontStyle);
          }
        }
        node = node.parentNode;
      }

      if(document != null && (fontName == null || !fontSize.HasValue || !fontStyle.HasValue))
      {
        GetFontStyle(document.DefaultStyle, ref fontName, ref fontSize, ref fontStyle);
      }

      GetFontStyle(Style.Default, ref fontName, ref fontSize, ref fontStyle);

      cachedFont = new Font(fontName, fontSize.Value, fontStyle.Value);
    }
    
    return cachedFont;
  }

  public Color GetEffectiveForeColor()
  {
    DocumentNode node = this;
    while(node != null)
    {
      if(node.styles != null)
      {
        for(int i=node.styles.Count-1; i>=0; i--)
        {
          if(!node.styles[i].ForeColor.IsEmpty) return node.styles[i].ForeColor;
        }
      }
      node = node.parentNode;
    }

    if(document != null && !document.DefaultStyle.ForeColor.IsEmpty)
    {
      return document.DefaultStyle.ForeColor;
    }

    return Style.Default.ForeColor;
  }

  public Color GetEffectiveBackColor()
  {
    DocumentNode node = this;
    while(node != null)
    {
      if(node.styles != null)
      {
        for(int i=node.styles.Count-1; i>=0; i--)
        {
          if(!node.styles[i].BackColor.IsEmpty) return node.styles[i].BackColor;
        }
      }
      node = node.parentNode;
    }

    if(document != null && !document.DefaultStyle.BackColor.IsEmpty)
    {
      return document.DefaultStyle.BackColor;
    }

    return Style.Default.BackColor;
  }

  public FourSide GetEffectivePadding()
  {
    FourSide padding = new FourSide();
    if(isBlockNode && styles != null)
    {
      foreach(Style style in styles)
      {
        if(style.Padding.HasValue) padding = style.Padding.Value;
      }
    }
    return padding;
  }

  static void GetFontStyle(Style style, ref string name, ref float? size, ref FontStyle? fontStyle)
  {
    if(name == null) name = style.FontName;
    if(!size.HasValue) size = style.FontSize;
    if(!fontStyle.HasValue) fontStyle = style.FontStyle;
  }
  #endregion

  #region User input events
  protected internal virtual void OnDoubleClick(EventArgs e)
  {
    if(parentNode != null) parentNode.OnDoubleClick(e);
  }

  protected internal virtual void OnMouseClick(MouseEventArgs e)
  {
    if(parentNode != null) parentNode.OnMouseClick(e);
  }

  protected internal virtual void OnMouseHover(MouseEventArgs e)
  {
    if(parentNode != null) parentNode.OnMouseHover(e);
  }

  protected internal virtual void OnMouseEnter(MouseEventArgs e) { }
  protected internal virtual void OnMouseLeave(EventArgs e) { }
  #endregion

  protected void NeedRepaint()
  {
    if(document != null)
    {
      document.OnNeedRepaint(this);
    }
  }

  protected void NodeChanged()
  {
    if(document != null)
    {
      document.OnNodeChanged(this);
    }

    RecursivelyClearCachedData();
  }
  
  internal void InitializeDocument(Document document)
  {
    this.document = document;
  }

  void OnFontChanged()
  {
    RecursivelyClearCachedData();
  }

  void RecursivelyClearCachedData()
  {
    cachedFont = null;
    foreach(DocumentNode child in Children)
    {
      child.RecursivelyClearCachedData();
    }
  }

  Font cachedFont;
  bool isBlockNode;
  
  static bool IsFontRelated(Style style)
  {
    return style.FontName != null || style.FontSize.HasValue || style.FontStyle.HasValue;
  }
}
#endregion

#region BlockNode
class BlockNode :  DocumentNode
{
  public BlockNode() : base(true) { }
}
#endregion

#region RootNode
class RootNode : BlockNode
{
  internal RootNode(Document document)
  {
    if(document == null) throw new ArgumentNullException();
    InitializeDocument(document);
  }
}
#endregion

#region InlineNode
class InlineNode : DocumentNode
{
  public InlineNode() : base(false) { }
}
#endregion

#region TextNode
class TextNode : InlineNode
{
  public TextNode() : this(string.Empty) { }
  public TextNode(string text)
  {
    this.text = text;
  }
  public TextNode(string text, params Style[] styles)
  {
    this.text = text;
    SetStyles(styles);
  }

  public string Text
  {
    get { return text; }
    set
    {
      if(value == null) value = string.Empty;
      if(value != text)
      {
        if(Document != null) Document.InvalidateText();
        value = text;
        NodeChanged();
      }
    }
  }

  public TextSpan TextSpan
  {
    get
    {
      if(Document == null) throw new InvalidOperationException("This node is not part of a document.");
      Document.EnsureText();
      return span;
    }
  }

  protected override void OnNodeAdded(DocumentNode parent, int index)
  {
    base.OnNodeAdded(parent, index);
    if(parent.Document != null) parent.Document.InvalidateText();
  }

  protected override void OnNodeRemoved(DocumentNode parent)
  {
    base.OnNodeRemoved(parent);
    if(parent.Document != null) parent.Document.InvalidateText();
  }

  string text;
  internal TextSpan span;
}
#endregion

#region LinkNode
class LinkNode : TextNode
{
  public LinkNode(string text) : this(text, null) { }

  public LinkNode(string text, params Style[] styles) : base(text)
  {
    AddStyle(linkStyle);
    if(styles != null)
    {
      foreach(Style style in styles) AddStyle(style);
    }
  }

  static LinkNode()
  {
    linkStyle = new Style();
    linkStyle.ForeColor = Color.Blue;
    
    hoverStyle = new Style();
    hoverStyle.Cursor    = Cursors.Hand;
    hoverStyle.FontStyle = FontStyle.Underline;
  }

  protected internal override void OnMouseEnter(MouseEventArgs e)
  {
    AddStyle(hoverStyle);
  }

  protected internal override void OnMouseLeave(EventArgs e)
  {
    RemoveStyle(hoverStyle);
  }

  static Style linkStyle, hoverStyle;
}
#endregion

delegate void NodeEventHandler(Document document, DocumentNode node);

#region Document
class Document
{
  public Document()
  {
    root = new RootNode(this);
  }

  public event NodeEventHandler NeedRepaint, NodeChanged;

  public Style DefaultStyle
  {
    get { return style; }
    set
    {
      if(value == null) throw new ArgumentNullException();
      style = value;
    }
  }

  public DocumentNode Root
  {
    get { return root; }
  }

  public string Text
  {
    get
    {
      EnsureText();
      return documentText;
    }
  }

  public void Clear()
  {
    root.Children.Clear();
  }

  protected internal virtual void OnNodeChanged(DocumentNode node)
  {
    if(NodeChanged != null) NodeChanged(this, node);
  }

  protected internal virtual void OnNeedRepaint(DocumentNode node)
  {
    if(NeedRepaint != null) NeedRepaint(this, node);
  }

  internal void EnsureText()
  {
    if(documentText == null)
    {
      StringBuilder sb = new StringBuilder();
      foreach(DocumentNode node in root.EnumerateDescendants())
      {
        TextNode textNode = node as TextNode;
        if(textNode != null)
        {
          textNode.span = new TextSpan(sb.Length, textNode.Text.Length);
          sb.Append(textNode.Text);
        }
      }
      documentText = sb.ToString();
    }
  }

  internal void InvalidateText()
  {
    documentText = null;
  }
  
  readonly DocumentNode root;
  string documentText;
  Style style = Style.Default;
}
#endregion

#region FourSide
struct FourSide
{
  public int TotalHorizontal
  {
    get { return Left + Right; }
  }
  
  public int TotalVertical
  {
    get { return Top + Bottom; }
  }
  
  public int Left, Top, Right, Bottom;

  public void SetAll(int value)
  {
    Top = Left = Right = Bottom = value;
  }

  public void SetHorizontal(int value)
  {
    Left = Right = value;
  }
  
  public void SetVertical(int value)
  {
    Top = Bottom = value;
  }
}
#endregion

#region Style
class Style
{
  public Style() { }

  static Style()
  {
    Default = new Style();
    Default.Cursor    = Cursors.Default;
    Default.BackColor = SystemColors.Window;
    Default.ForeColor = SystemColors.WindowText;
    Default.FontName  = "Verdana";
    Default.FontSize  = 9;
    Default.FontStyle = System.Drawing.FontStyle.Regular;
  }

  public Cursor Cursor
  {
    get { return cursor; }
    set { cursor = value; }
  }

  public Color ForeColor
  {
    get { return foreColor; }
    set { foreColor = value; }
  }

  public Color BackColor
  {
    get { return backColor; }
    set { backColor = value; }
  }

  public string FontName
  {
    get { return fontName; }
    set { fontName = value; }
  }

  public float? FontSize
  {
    get { return fontSize; }
    set
    {
      if(value <= 0) throw new ArgumentOutOfRangeException();
      fontSize = value;
    }
  }

  public FontStyle? FontStyle
  {
    get { return fontStyle; }
    set { fontStyle = value; }
  }

  public FourSide? Padding
  {
    get { return padding; }
    set
    {
      if(value.HasValue &&
         (value.Value.Left < 0 || value.Value.Top < 0 || value.Value.Right < 0 || value.Value.Bottom < 0))
      {
        throw new ArgumentOutOfRangeException();
      }
      padding = value;
    }
  }

  public readonly static Style Default;

  Cursor cursor;
  string fontName;
  float? fontSize;
  FontStyle? fontStyle;
  Color foreColor, backColor;
  FourSide? padding;
}
#endregion

#region DocumentRenderer
class DocumentRenderer : Control
{
  public DocumentRenderer()
  {
    SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque | ControlStyles.ResizeRedraw |
             ControlStyles.Selectable | ControlStyles.StandardClick | ControlStyles.UserPaint |
             ControlStyles.OptimizedDoubleBuffer, true);

    SetStyle(ControlStyles.ContainerControl | ControlStyles.FixedHeight | ControlStyles.FixedWidth |
             ControlStyles.StandardDoubleClick | ControlStyles.SupportsTransparentBackColor, false);

    document.NeedRepaint += delegate(Document doc, DocumentNode node) { Invalidate(node); };
    document.NodeChanged += delegate(Document doc, DocumentNode node) { InvalidateLayout(); };
  }

  public BorderStyle BorderStyle
  {
    get { return borderStyle; }
    set
    {
      if(value != borderStyle)
      {
        borderStyle = value;
        InvalidateLayout();
      }
    }
  }

  public Document Document
  {
    get { return document; }
  }

  public void Clear()
  {
    Document.Clear();
    if(scrollBar != null) scrollBar.Value = 0;
  }

  public DocumentNode GetNodeFromPosition(Point pt)
  {
    Layout();

    Rectangle bounds = scrollBar == null ? rootBlock.Bounds : rootBlock.GetAdjustedBounds(scrollBar.Value);
    pt.Offset(BorderWidth, BorderWidth);

    Span span;
    if(bounds.Contains(pt))
    {
      span = GetNodeFromPosition(pt, rootBlock) as Span;
    }
    else
    {
      span = null;
    }

    return span == null ? null : span.Node;
  }

  public new void Layout()
  {
    if(rootBlock == null)
    {
      using(Graphics g = Graphics.FromHwnd(Handle))
      {
        EnsureLayout(g);
      }
    }
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);

    EnsureLayout(e.Graphics);

    using(Brush bgBrush = new SolidBrush(document.DefaultStyle.BackColor))
    {
      e.Graphics.FillRectangle(bgBrush, e.ClipRectangle);
    }

    Rectangle renderRect = RenderRect, clipRect = Rectangle.Intersect(e.ClipRectangle, renderRect);
    rootBlock.Render(e.Graphics, ref renderRect, ref clipRect, scrollBar == null ? 0 : scrollBar.Value);

    if(borderStyle != BorderStyle.None)
    {
      ControlPaint.DrawBorder3D(e.Graphics, this.ClientRectangle,
                                borderStyle == BorderStyle.Fixed3D ? Border3DStyle.Sunken : Border3DStyle.Flat);
    }
  }

  protected override void OnSizeChanged(EventArgs e)
  {
    base.OnSizeChanged(e);
    InvalidateLayout();
  }

  #region Layout
  #region Layout classes
  abstract class TextRegion
  {
    public Point Position
    {
      get { return Bounds.Location; }
      set { Bounds.Location = value; }
    }

    public Size Size
    {
      get { return Bounds.Size; }
      set { Bounds.Size = value; }
    }
    
    public int Left
    {
      get { return Bounds.X; }
      set { Bounds.X = value; }
    }

    public int Top
    {
      get { return Bounds.Y; }
      set { Bounds.Y = value; }
    }

    public int Width
    {
      get { return Bounds.Width; }
      set { Bounds.Width = value; }
    }
    
    public int Height
    {
      get { return Bounds.Height; }
      set { Bounds.Height = value; }
    }

    public int TextStart
    {
      get { return TextSpan.Start; }
      set { TextSpan.Start = value; }
    }
    
    public int TextEnd
    {
      get { return TextSpan.Start + TextSpan.Length; }
    }

    public int TextLength
    {
      get { return TextSpan.Length; }
      set { TextSpan.Length = value; }
    }

    public Rectangle GetAdjustedBounds(int scrollOffset)
    {
      Rectangle rect = Bounds;
      rect.Y -= scrollOffset;
      return rect;
    }

    public Rectangle Bounds;
    public TextSpan TextSpan;
  }
  
  abstract class BlockBase : TextRegion
  {
    public abstract Rectangle GetNodeBounds(DocumentNode node);
    public abstract void Render(Graphics gdi, ref Rectangle area, ref Rectangle clip, int scrollOffset);
  }

  sealed class Block : BlockBase
  {
    public override Rectangle GetNodeBounds(DocumentNode node)
    {
      foreach(BlockBase block in Blocks)
      {
        Rectangle rect = block.GetNodeBounds(node);
        if(rect.Height != 0) // if the height is zero, the block doesn't contain this node
        {
          rect.Offset(Left, Top);
          return rect;
        }
      }
      return new Rectangle();
    }

    public override void Render(Graphics gdi, ref Rectangle area, ref Rectangle clip, int scrollOffset)
    {
      foreach(BlockBase block in Blocks)
      {
        Rectangle childArea = new Rectangle(area.Left+block.Left, area.Top+block.Top-scrollOffset,
                                            block.Width, block.Height);
        if(childArea.IntersectsWith(clip))
        {
          block.Render(gdi, ref childArea, ref clip, 0);
        }
      }
    }

    public BlockBase[] Blocks;
  }

  sealed class LineBlock : BlockBase
  {
    public LineBlock(Line[] lines) { Lines = lines; }

    public override Rectangle GetNodeBounds(DocumentNode node)
    {
      Rectangle rect = new Rectangle();
      foreach(Line line in Lines)
      {
        Rectangle rectInLine = line.GetNodeBounds(node);
        if(rectInLine.Height != 0) // if the height is zero, the block doesn't contain this node
        {
          rect = rect.Height == 0 ? rectInLine : Rectangle.Union(rect, rectInLine);
        }
      }
      if(rect.Height != 0) rect.Offset(Left, Top);
      return rect;
    }

    public override void Render(Graphics gdi, ref Rectangle area, ref Rectangle clip, int scrollOffset)
    {
      foreach(Line line in Lines)
      {
        Rectangle childArea = new Rectangle(area.Left+line.Left, area.Top+line.Top-scrollOffset,
                                            line.Width, line.Height);
        if(childArea.IntersectsWith(clip))
        {
          line.Render(gdi, ref childArea);
        }
      }
    }

    public Line[] Lines;
  }
  
  sealed class Line : TextRegion
  {
    public Line(Span[] spans) { Spans = spans; }

    public Rectangle GetNodeBounds(DocumentNode node)
    {
      Rectangle rect = new Rectangle();
      foreach(Span span in Spans)
      {
        if(span.Node == node)
        {
          rect = rect.Height == 0 ? span.Bounds : Rectangle.Union(rect, span.Bounds);
        }
      }
      if(rect.Height != 0) rect.Offset(Left, Top);
      return rect;
    }

    public void Render(Graphics gdi, ref Rectangle area)
    {
      foreach(Span span in Spans)
      {
        span.Render(gdi, new Point(area.Left+span.Left, area.Top+span.Top));
      }
    }

    public Span[] Spans;
  }
  
  sealed class Span : TextRegion
  {
    public Span(DocumentNode node) { Node = node; }

    public void Render(Graphics gdi, Point point)
    {
      TextRenderer.DrawText(gdi, Node.Document.Text.Substring(TextStart, TextLength), Node.GetEffectiveFont(),
                            new Rectangle(point, Size), Node.GetEffectiveForeColor(), Node.GetEffectiveBackColor(),
                            TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
    }

    public DocumentNode Node;
  }
  #endregion

  void EnsureLayout(Graphics gdi)
  {
    if(rootBlock == null)
    {
      Rectangle renderRect = RenderRect;
      rootBlock = CreateBlock(gdi, document.Root.Children, renderRect.Width);

      // if we need to show or hide the scrollbar, do so and re-layout
      if(rootBlock.Bounds.Bottom < renderRect.Height && scrollBar != null)
      {
        Controls.Remove(scrollBar);
        scrollBar = null;
        rootBlock = CreateBlock(gdi, document.Root.Children, RenderRect.Width);
      }
      else if(rootBlock.Bounds.Bottom > renderRect.Height && scrollBar == null)
      {
        scrollBar = new VScrollBar();
        scrollBar.Cursor  = Cursors.Default;
        scrollBar.ValueChanged += scrollBar_ValueChanged;
        Controls.Add(scrollBar);
        rootBlock = CreateBlock(gdi, document.Root.Children, RenderRect.Width);
      }

      if(scrollBar != null) // if the scrollbar should be displayed, update its range and page size
      {
        scrollBar.Top    = BorderWidth;
        scrollBar.Left   = Width - BorderWidth - scrollBar.Width;
        scrollBar.Height = Height - BorderWidth*2;

        scrollBar.Maximum     = rootBlock.Bounds.Bottom;
        scrollBar.SmallChange = (int)Math.Ceiling(document.Root.GetEffectiveFont().GetHeight(gdi));
        scrollBar.LargeChange = renderRect.Height - scrollBar.SmallChange;
      }
    }
  }

  static BlockBase CreateBlock(Graphics gdi, IList<DocumentNode> blockNodes, int availableWidth)
  {
    BlockBase newBlock;

    bool allInline = true;
    foreach(DocumentNode child in blockNodes)
    {
      if(child.IsBlockNode)
      {
        allInline = false;
        break;
      }
    }
    
    if(allInline)
    {
      newBlock = LayoutLines(gdi, blockNodes, availableWidth);
    }
    else
    {
      Block blockBlock = new Block();
      List<BlockBase> childBlocks = new List<BlockBase>(blockNodes.Count);

      List<DocumentNode> anonymous = null;
      foreach(DocumentNode child in blockNodes)
      {
        if(child.IsBlockNode)
        {
          if(anonymous != null && anonymous.Count != 0)
          {
            childBlocks.Add(LayoutLines(gdi, anonymous, availableWidth));
            anonymous.Clear();
          }

          FourSide padding = child.GetEffectivePadding();
          BlockBase block  = CreateBlock(gdi, child.Children, availableWidth - padding.TotalHorizontal);
          block.Position   = Point.Add(block.Position, new Size(padding.Left, padding.Top));
          block.Height    += padding.Bottom;
          childBlocks.Add(block);
        }
        else
        {
          if(anonymous == null) anonymous = new List<DocumentNode>();
          anonymous.Add(child);
        }
      }

      if(anonymous != null && anonymous.Count != 0)
      {
        childBlocks.Add(LayoutLines(gdi, anonymous, availableWidth));
      }

      if(childBlocks.Count == 1)
      {
        newBlock = childBlocks[0];
      }
      else
      {
        blockBlock.Blocks = childBlocks.ToArray();
        foreach(BlockBase child in blockBlock.Blocks)
        {
          int effectiveHeight = child.Top + child.Height;
          child.Position = new Point(child.Left, child.Top + blockBlock.Height);
          
          int childRight = child.Bounds.Right;
          if(childRight > blockBlock.Bounds.Right)
          {
            blockBlock.Width = childRight - blockBlock.Left;
          }
          blockBlock.Height += effectiveHeight;
        }
        newBlock = blockBlock;
      }
    }

    return newBlock;
  }

  // This function splits the text from a list of inline nodes into lines and spans.
  // It outputs a new line when:
  // 1. A newline sequence is encountered
  // 2. A line becomes too long and needs to be wrapped
  // 3. All nodes have been processed and the current line contains spans
  // It outputs a new span when:
  // 1. A new line is being output, and the current span contains text or the line has no spans
  // 2. The node changes
  // 3. All nodes have been processed and the current span contains text
  static LineBlock LayoutLines(Graphics gdi, IList<DocumentNode> inlineNodes, int availableWidth)
  {
    const TextFormatFlags measureFlags =
      TextFormatFlags.NoPrefix | TextFormatFlags.NoClipping | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
    
    List<Line> lineObjs = new List<Line>(inlineNodes.Count);
    List<Span> spans = new List<Span>();
    int currentWidth = 0;

    Span span = null;
    foreach(DocumentNode node in inlineNodes)
    {
      TextNode textNode = node as TextNode;
      if(textNode == null) continue;

      Font font = textNode.GetEffectiveFont();
      span = new Span(node);
      int textIndex = textNode.TextSpan.Start; // get the index from where this node's text starts
      span.TextStart = textIndex; // and use it to create the current span's initial TextSpan

      foreach(Match lineMatch in lineRE.Matches(textNode.Text)) // for each blob of text in this node
      {
        foreach(Match match in wordRE.Matches(lineMatch.Groups[1].Value)) // for each word in the blob of text
        {
          string word = match.Value; // get the word (plus whitespace)

          if(spans.Count == 0 && span.TextLength == 0) // if the current line is empty
          {
            string trimmed = word.TrimStart(); // trim whitespace from the start of the word
            span.TextStart += word.Length-trimmed.Length; // offset the span start by the number of characters trimmed
            word = trimmed;
          }

          int spaceLeft = availableWidth-currentWidth; // calculate the remaining horizontal space
          // and measure to see whether this text fits within that space
          Size size = TextRenderer.MeasureText(gdi, word, font, new Size(spaceLeft, int.MaxValue), measureFlags);

          // if the text did not fit, and the current line is not empty, we'll output the current line.
          if(size.Width > spaceLeft && (spans.Count != 0 || span.TextLength != 0))
          {
            if(span.TextLength != 0) // if the current span contains text, output that first
            {
              spans.Add(span);
              int newStart = span.TextEnd;
              span = new Span(node);
              span.TextStart = newStart;
            }

            lineObjs.Add(new Line(spans.ToArray()));
            spans.Clear();
            currentWidth = 0;

            // since the word is now on its own line, trim it. if that changed the length, re-measure it
            string trimmed = word.TrimStart();
            if(trimmed.Length != word.Length)
            {
              size = TextRenderer.MeasureText(gdi, word, font, new Size(availableWidth, int.MaxValue), measureFlags);
              span.TextStart += word.Length - trimmed.Length;
            }
            word = trimmed;
          }

          currentWidth += size.Width; // increase the current line width by the width of the span
          span.Size = new Size(span.Width+size.Width, Math.Max(span.Height, size.Height));
          span.TextLength += match.Length; // increase the span length by the length of the word
        }

        textIndex += lineMatch.Length; // increase the text index to point past this line

        if(lineMatch.Groups["eol"].Length != 0) // if the line had a line terminator, force the line to end
        {
          if(span.TextLength != 0) // if the current span contains text, output it
          {
            spans.Add(span);
            span = new Span(node);
          }

          Line line = new Line(spans.ToArray());
          if(line.Spans.Length == 0) // if the line is empty, we'll still need to give it a height
          {
            line.Size = new Size(0, TextRenderer.MeasureText(gdi, " ", font).Height);
          }
          lineObjs.Add(line);
          spans.Clear();
          currentWidth = 0;

          span.TextStart = textIndex; // the current span (which is empty) will start past this line
        }
      }

      // now we're done with the current node.
      if(span.TextLength != 0) // if the current span contains text, output it
      {
        spans.Add(span);
      }
    }

    if(spans.Count != 0) // if there are any spans, add the final line
    {
      lineObjs.Add(new Line(spans.ToArray()));
    }

    LineBlock block = new LineBlock(lineObjs.ToArray());
    if(lineObjs.Count != 0)
    {
      block.TextStart = lineObjs[0].TextSpan.Start;
      foreach(Line line in lineObjs)
      {
        if(line.Spans.Length != 0) // if the line has no spans, it will have been given a height above
        {
          line.TextStart = line.Spans[0].TextSpan.Start;

          foreach(Span s in line.Spans) // go through and measure the line and position spans horizontally
          {
            s.Position       = new Point(line.Width, 0);
            line.Height      = Math.Max(line.Height, s.Height);
            line.Width      += s.Width;
            line.TextLength += s.TextSpan.Length;
          }
          
          foreach(Span s in line.Spans) // go back through and position spans vertically (bottom-align them)
          {
            s.Top += line.Height-s.Height;
          }
        }

        line.Position    = new Point(0, block.Height);
        block.Width      = Math.Max(line.Width, block.Width);
        block.Height    += line.Height;
        block.TextLength = line.TextEnd - block.TextStart;
      }
    }
    return block;
  }

  void InvalidateLayout()
  {
    rootBlock = null;
    Invalidate();
  }
  #endregion

  #region Mouse handling
  protected override void OnMouseDown(MouseEventArgs e)
  {
    base.OnMouseDown(e);
    Focus();
    mouseDown = e.Location;
  }

  protected override void OnMouseUp(MouseEventArgs e)
  {
    base.OnMouseUp(e);

    int xd = e.X-mouseDown.X, yd=e.Y-mouseDown.Y;
    if(xd*xd+yd*yd < 4) // if the mouse moved too far since the button was pressed, don't count it as a click
    {
      DocumentNode node = GetNodeFromPosition(mouseDown);
      if(node != null) node.OnMouseClick(e);
      // we don't handle tracking of individual buttons, so make sure additional button releases don't retrigger it
      mouseDown = new Point(-1000, -1000); 
    }
  }
  protected override void OnDoubleClick(EventArgs e)
  {
    base.OnDoubleClick(e);

    DocumentNode node = GetNodeFromPosition(this.PointToClient(Cursor.Position));
    if(node != null) node.OnDoubleClick(e);
  }

  protected override void OnMouseHover(EventArgs e)
  {
    base.OnMouseHover(e);

    Point position = this.PointToClient(Cursor.Position);
    DocumentNode node = GetNodeFromPosition(position);
    if(node != null)
    {
      node.OnMouseHover(new MouseEventArgs(MouseButtons.None, 0, position.X, position.Y, 0));
    }
  }

  protected override void OnMouseMove(MouseEventArgs e)
  {
    base.OnMouseMove(e);

    // only fire enter events when no button is pressed, because, for instance, when the user is doing a drag select,
    // the selection won't actually be updated until the user releases the mouse button. in that case, if we fired the
    // an event, a region might update the selection, ruining the user's attempt to drag-select
    if(e.Button != MouseButtons.None) return;

    DocumentNode node = GetNodeFromPosition(e.Location);

    if(inside == node) return; // if the mouse is over the same node as before, then nothing needs to be done

    List<DocumentNode> before = GetAncestorList(inside), after = GetAncestorList(node);
    if(inside == null) // if it wasn't over a node before but now is, call Enter from the root down to it
    {
      for(int i=after.Count-1; i>=0; i--) after[i].OnMouseEnter(e);
    }
    else if(node == null) // otherwise, if it was over before but now isn't, call Leave from the root down to it
    {
      for(int i=0; i<before.Count; i++) before[i].OnMouseLeave(e);
    }
    else // otherwise, it was over a node both times
    {
      after.Reverse(); // reverse the arrays for easy usage
      before.Reverse();

      // find the index where the node values diverge
      int diffIndex = 0;
      for(int len=Math.Min(after.Count, before.Count); diffIndex<len; diffIndex++)
      {
        if(after[diffIndex] != before[diffIndex]) break;
      }
      
      // now call Leave and Enter on the nodes that need it
      for(int i=before.Count-1; i>=diffIndex; i--) before[i].OnMouseLeave(e);
      for(int i=diffIndex; i<after.Count; i++) after[i].OnMouseEnter(e);
    }
    
    inside = node;
    Cursor = node == null ? Cursors.Default : node.GetEffectiveCursor();
  }

  protected override void OnMouseLeave(EventArgs e)
  {
    base.OnMouseLeave(e);

    while(inside != null)
    {
      inside.OnMouseLeave(e);
      inside = inside.Parent;
    }
  }

  protected override void OnMouseWheel(MouseEventArgs e)
  {
    base.OnMouseWheel(e);
    if(scrollBar != null)
    {
      int newValue = scrollBar.Value + -e.Delta*3*scrollBar.SmallChange/120; // scroll ~3 lines per click
      if(newValue >= scrollBar.Minimum && newValue <= scrollBar.Maximum-scrollBar.LargeChange+1)
      {
        scrollBar.Value = newValue;
      }
    }
  }
  #endregion

  int BorderWidth
  {
    get { return borderStyle == BorderStyle.Fixed3D ? 2 : borderStyle == BorderStyle.FixedSingle ? 1 : 0; }
  }

  Rectangle RenderRect
  {
    get
    {
      Rectangle rect = this.ClientRectangle;
      int borderWidth = BorderWidth + 1; // add one extra pixel so the text doesn't but up against the border
      rect.Inflate(-borderWidth, -borderWidth);
      if(scrollBar != null) rect.Width -= scrollBar.Width; // account for the scroll bar
      return rect;
    }
  }

  TextRegion GetNodeFromPosition(Point pt, TextRegion parent)
  {
    TextRegion[] children;
    if(parent is Block)
    {
      children = ((Block)parent).Blocks;
    }
    else if(parent is LineBlock)
    {
      children = ((LineBlock)parent).Lines;
    }
    else if(parent is Line)
    {
      children = ((Line)parent).Spans;
    }
    else return parent; // it's a span

    int borderWidth = BorderWidth;
    foreach(TextRegion child in children)
    {
      Rectangle bounds = scrollBar == null ? child.Bounds : child.GetAdjustedBounds(scrollBar.Value);
      if(bounds.Contains(pt))
      {
        pt = new Point(pt.X-child.Left, pt.Y-child.Top);
        TextRegion lowest = GetNodeFromPosition(pt, child);
        if(lowest != null) return lowest;
        break;
      }
    }

    return parent;
  }

  void Invalidate(DocumentNode node)
  {
    Rectangle rect = rootBlock.GetNodeBounds(node);
    rect.X += BorderWidth;
    rect.Y += BorderWidth - (scrollBar == null ? 0 : scrollBar.Value);
    rect.Inflate(1, 1); // inflate slightly to handle rendered text getting larger due to underline, bold, etc
    if(rect.IntersectsWith(ClientRectangle)) Invalidate(rect);
  }

  void scrollBar_ValueChanged(object sender, EventArgs e)
  {
    Invalidate();
  }

  BorderStyle borderStyle = BorderStyle.Fixed3D;
  VScrollBar scrollBar = new VScrollBar();
  readonly Document document = new Document();
  BlockBase rootBlock;
  DocumentNode inside;
  Point mouseDown;

  static List<DocumentNode> GetAncestorList(DocumentNode node)
  {
    List<DocumentNode> list = null;
    if(node != null)
    {
      list = new List<DocumentNode>();
      do
      {
        list.Add(node);
        node = node.Parent;
      } while(node != null);
    }
    return list;
  }

  static readonly Regex wordRE = new Regex(@"\s*\S+\s*", RegexOptions.Singleline);
  static readonly Regex lineRE = new Regex(@"(?<eol>\r\n?|\n)|([^\r\n]+)(?<eol>\r\n?|\n)?",
                                           RegexOptions.Singleline | RegexOptions.Compiled);
}
#endregion

} // namespace Jappy