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

  public void PerformExampleSearch(string query)
  {
    if(string.IsNullOrEmpty(query)) throw new ArgumentException();

    input.Text = query;
    output.Clear();

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
          //statusText.Text = "Search results truncated at "+threshold+" results.";
          break;
        }

        output.AppendText(example.Japanese+"[ T ]\n");
        output.AppendText(example.English+"\n");
      }

      if(numberOfResults <= threshold)
      {
        //statusText.Text = "The search returned "+numberOfResults+" result(s).";
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
    Form.SetStatusText(input,
      "Type here and press Enter to search. Prefix romaji with @. Examples: her, 彼女, @kanojo");
  }

  void common_MouseLeave(object sender, EventArgs e)
  {
    base.control_RestoreStatusText(sender, e);
  }
}

} // namespace Jappy