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
using System.Drawing;
using System.Windows.Forms;
using Jappy.Backend;

namespace Jappy
{

partial class ExampleSearchTab : TabBase
{
  public ExampleSearchTab()
  {
    InitializeComponent();
  }

  public override DocumentRenderer OutputArea
  {
    get { return output; }
  }

  public void PerformExampleSearch(string query)
  {
    if(string.IsNullOrEmpty(query)) throw new ArgumentException();

    input.Text = query;
    output.Clear();
    SwitchToTab();

    try
    {
      const int threshold = 500;
      int numberOfResults = 0;

      IEnumerable<uint> entryIds =
        JapaneseSearchStrategy.Instance.Search(App.Examples, query, SearchFlag.ExactMatch | SearchFlag.SearchAll);
      foreach(ExampleSentence example in new ExampleIterator(App.Examples, entryIds))
      {
        if(++numberOfResults > threshold)
        {
          Form.SetStatusText(output, "Search results truncated at "+threshold+" results.");
          break;
        }

        UI.RenderExampleSentence(example, output);
      }

      if(numberOfResults <= threshold)
      {
        Form.SetStatusText(output, "The search returned "+numberOfResults+" result(s).");
      }
    }
    catch(ArgumentException e)
    {
      Form.ShowToolTip(input, "Error", e.Message);
    }
  }

  void input_KeyPress(object sender, KeyPressEventArgs e)
  {
    if((e.KeyChar == '\n' || e.KeyChar == '\r') &&
       (Control.ModifierKeys == Keys.None || Control.ModifierKeys == Keys.Control))
    {
      string query = input.Text.Trim();
      if(query != "")
      {
        PerformExampleSearch(query);
      }
    }
  }

  void input_MouseEnter(object sender, EventArgs e)
  {
    Form.SetStatusText(input, "Type English or Japanese words here and press Enter to search examples. "+
                              "Prefix romaji with @. Examples: her, 彼女, @kanojo");
  }

  void common_MouseLeave(object sender, EventArgs e)
  {
    base.control_RestoreStatusText(sender, e);
  }

  void output_MouseClick(object sender, MouseEventArgs e)
  {
    doc_MouseClick(sender, e);
  }

  void output_MouseEnter(object sender, EventArgs e)
  {
    if(output.Document.Root.Children.Count == 0) // if there are no results
    {
      Form.SetStatusText(output,
                         "Type english or Japanese words into the box above and press Enter to search examples.");
    }
    else
    {
      Form.SetStatusText(output,
        "Click the 'T' link to translate sentences. Select text and right click to perform actions.");
    }
  }
}

class TranslateTextNode : LinkNode
{
  public TranslateTextNode(string textToTranslate) : base(" T ")
  {
    this.textToTranslate = textToTranslate;
  }

  protected internal override void OnMouseClick(object sender, MouseEventArgs e)
  {
    base.OnMouseClick(sender, e);
    App.MainForm.GetTranslationTab().PerformTranslation(textToTranslate);
  }

  protected internal override void OnMouseEnter(object sender, MouseEventArgs e)
  {
    base.OnMouseEnter(sender, e);
    App.MainForm.SetStatusText((Control)sender, "Click to translate this text.");
  }

  protected internal override void OnMouseLeave(object sender, EventArgs e)
  {
    base.OnMouseLeave(sender, e);
    App.MainForm.RestoreStatusText((Control)sender);
  }

  string textToTranslate;
}

} // namespace Jappy