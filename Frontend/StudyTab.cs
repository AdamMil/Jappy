using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Jappy
{

partial class StudyTab : TabBase
{
  public StudyTab()
  {
    InitializeComponent();
    
    DocumentRenderer doc = new DocumentRenderer();
    doc.Dock = DockStyle.Fill;
    Controls.Add(doc);
    
    doc.Document.Root.Children.Add(new TextNode("This is a long line of text. I expect it to break somewhere, but who knows where that will happen?"));

    Style boldStyle = new Style();
    boldStyle.FontStyle = FontStyle.Bold;
    Style underline = new Style();
    underline.FontStyle = FontStyle.Underline;

    BlockNode node = new BlockNode();
    node.Children.Add(new TextNode("The "));
    node.Children.Add(new TextNode("bold", boldStyle));
    node.Children.Add(new TextNode(" knight "));
    node.Children.Add(new TextNode("underscored", underline));
    node.Children.Add(new TextNode(" the need for good algorithms!"));
    doc.Document.Root.Children.Add(node);
  }
}

} // namespace Jappy