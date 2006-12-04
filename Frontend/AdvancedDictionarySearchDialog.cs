using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Jappy.Backend;

namespace Jappy
{

partial class AdvancedDictionarySearchDialog : Form
{
  public AdvancedDictionarySearchDialog()
  {
    InitializeComponent();
    
    pos.Items.Add(new PosItem("Adjectives", SenseFlag.Adj, SenseFlag.AdjNa, SenseFlag.AdjNo, SenseFlag.AdjPn,
                                            SenseFlag.AdjTaru, SenseFlag.AuxiliaryAdj));
    pos.Items.Add(new PosItem("Adverbs", SenseFlag.Adv, SenseFlag.AdvN, SenseFlag.AdvTo, SenseFlag.NounAdv));
    pos.Items.Add(new PosItem("Nouns", SenseFlag.Noun, SenseFlag.NounAdv, SenseFlag.NounPrefix, SenseFlag.NounSuffix,
                                       SenseFlag.NounTemporal));
    pos.Items.Add(new PosItem("Particles", SenseFlag.Particle));
    pos.Items.Add(new PosItem("Verbs", SenseFlag.Verb1, SenseFlag.Verb5, SenseFlag.Verb5Aru, SenseFlag.Verb5bu,
                              SenseFlag.Verb5gu, SenseFlag.Verb5ku, SenseFlag.Verb5kuSpecial, SenseFlag.Verb5mu,
                              SenseFlag.Verb5nu, SenseFlag.Verb5ru, SenseFlag.Verb5ruIrregular, SenseFlag.Verb5su,
                              SenseFlag.Verb5tu, SenseFlag.Verb5u, SenseFlag.Verb5uru, SenseFlag.Verb5uSpecial,
                              SenseFlag.VerbKuruSpecial, SenseFlag.VerbSuru, SenseFlag.VerbSuruIrregular,
                              SenseFlag.VerbSuruSpecial, SenseFlag.VerbZuru, SenseFlag.AuxiliaryVerb,
                              SenseFlag.IrregularVerb, SenseFlag.NegativeVerb));
    pos.Items.Add(new PosItem("Others", SenseFlag.Abbreviation, SenseFlag.Auxiliary, SenseFlag.Conjunction,
                                        SenseFlag.Expression, SenseFlag.Idiom, SenseFlag.Interjection,
                                        SenseFlag.QuodVide, SenseFlag.Prefix, SenseFlag.Suffix));

    for(int i=0; i<pos.Items.Count; i++) pos.SetItemChecked(i, true);
  }
  
  public string QueryText
  {
    get { return txtQuery.Text.Trim(); }
    set { txtQuery.Text = value; }
  }
  
  public WordDictionary Dictionary
  {
    get { return cmbDictionary.SelectedDictionary; }
    set { cmbDictionary.SelectedDictionary = value; }
  }

  public int Frequency
  {
    get
    {
      int value;
      int.TryParse(txtFrequency.Text, out value);
      return value;
    }
    set
    {
      txtFrequency.Text = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
  }
  
  public int ItemLimit
  {
    get
    {
      int value;
      int.TryParse(txtLimit.Text, out value);
      return value;
    }
  }

  public SenseFlag[] GetPartsOfSpeech()
  {
    if(pos.CheckedItems.Count == pos.Items.Count) return null;

    List<SenseFlag> flags = new List<SenseFlag>();
    foreach(PosItem item in pos.CheckedItems)
    {
      flags.AddRange(item.Flags);
    }
    return flags.ToArray();
  }

  protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
  {
    base.OnClosing(e);
    
    if(searchClicked && string.IsNullOrEmpty(QueryText) &&
       MessageBox.Show("This query will return all items in the dictionary. Continue?", "Return all items?",
                       MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
         == DialogResult.No)
    {
      e.Cancel = true;
      searchClicked = false;
    }
  }

  sealed class PosItem
  {
    public PosItem(string text, params SenseFlag[] flags)
    {
      this.text  = text;
      this.Flags = flags;
    }

    public override string ToString()
    {
      return text;
    }
    
    public readonly SenseFlag[] Flags;

    readonly string text;
  }

  void btnSearch_Click(object sender, EventArgs e)
  {
    searchClicked = true;
  }
  
  bool searchClicked;
}

} // namespace Jappy