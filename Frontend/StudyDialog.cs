using System;
using System.Drawing;
using System.Windows.Forms;

namespace Jappy
{

partial class StudyDialog : Form
{
  public StudyDialog()
  {
    InitializeComponent();
  }

  public StudyDialog(StudyList list) : this()
  {
    if(list == null) throw new ArgumentNullException();
    if(list.Items.Count == 0) throw new ArgumentException("The study list is empty.");

    this.Text = "Study: "+list.Name;
    this.list = list;

    foreach(StudyList.Item item in list.Items)
    {
      totalChance += GetWeight(item);
    }

    if(indices == null) indices = new int[list.Items.Count];
    for(int i=0; i<list.Items.Count; i++) indices[i] = i;
    ShuffleCards();

    ShowNextCard();
  }

  static StudyDialog()
  {
    headwordStyle = new Style(UI.JpStyle);
    headwordStyle.FontSize = 12;

    readingStyle = headwordStyle;

    exampleStyle = new Style(UI.JpStyle);
    exampleStyle.ForeColor = Color.Green;

    meaningStyle = UI.JpStyle;
  }

  StudyList.Item Item
  {
    get { return list.Items[indices[index]]; }
  }

  void MarkCardAndProceed(bool wasCorrect)
  {
    totalChance -= GetWeight(Item);
    Item.ShownCount++;
    if(wasCorrect) Item.CorrectCount++;
    totalChance += GetWeight(Item);
    ShowNextCard();
  }

  void RenderAnswer(string meaning, string reading, string example)
  {
    area.Document.Root.Children.Add(new TextNode("\n"));
    RenderHeadword(meaning, reading);
    RenderExample(example);
  }

  void RenderCard(string phrase, string reading, string example)
  {
    area.Clear();

    RenderHeadword(phrase, reading);
    RenderExample(example);
  }

  void RenderExample(string example)
  {
    if(!string.IsNullOrEmpty(example))
    {
      area.Document.Root.Children.Add(new TextNode("Example: ", exampleStyle, UI.BoldStyle));
      area.Document.Root.Children.Add(new TextNode(example+"\n", exampleStyle));
    }
  }

  void RenderHeadword(string phrase, string reading)
  {
    if(string.IsNullOrEmpty(reading))
    {
      area.Document.Root.Children.Add(new TextNode(phrase+"\n", headwordStyle));
    }
    else
    {
      area.Document.Root.Children.Add(new TextNode(phrase, headwordStyle));
      area.Document.Root.Children.Add(new TextNode(" ("+reading+")\n", readingStyle));
    }
  }

  void ShowAnswer()
  {
    if(!answerShown)
    {
      if(reversed)
      {
        if(!list.HintExample) RenderExample(Item.ExampleDest);
        RenderAnswer(Item.Phrase, Item.Readings, Item.ExampleSource);
      }
      else
      {
        if(!list.HintExample) RenderExample(Item.ExampleSource);
        RenderAnswer(Item.Meanings, null, Item.ExampleDest);
      }
      answerShown = true;
    }
  }

  void ShowNextCard()
  {
    // first we'll make a pass straight through. then, if ShowReversedCards == true, we'll shuffle and make a pass
    // through with cards that are reversed. finally, we'll choose random cards.
    if(state == State.FirstPass && ++index >= list.Items.Count) // if we're done with the first pass
    {
      if(list.ShowReversedCards) // do a reversed pass if we're allowed
      {
        ShuffleCards();
        state = State.ReversedPass;
      }
      else // otherwise just go straight to picking random cards
      {
        state = State.Random;
      }
      index = 0;
    }
    else if(state == State.ReversedPass && ++index >= list.Items.Count)
    {
      state = State.Random;
      index = 0;
    }

    reversed = state == State.ReversedPass;

    if(state == State.Random) // if we're picking random cards...
    {
      reversed = list.ShowReversedCards && (rand.Next() & 1) == 0; // show them reversed with a 50% chance

      int previousIndex = index;
      do
      {
        double value = rand.NextDouble() * totalChance; // then, pick a random card, with more weight going to cards
        while(true)                                     // that the user has trouble with
        {
          value -= GetWeight(Item);
          if(value <= 0) break;
          if(++index == list.Items.Count) index = 0;
        }
      } while(index == previousIndex && list.Items.Count > 1); // but never display the same card as before
    }

    string phrase, reading, example;
    if(reversed)
    {
      phrase  = Item.Meanings;
      reading = null;
      example = Item.ExampleDest;
    }
    else
    {
      phrase  = Item.Phrase;
      reading = Item.Readings;
      example = Item.ExampleSource;
    }

    if(!list.HintReadings) reading = null;
    if(!list.HintExample) example = null;

    answerShown = false;
    RenderCard(phrase, reading, example);

    if(state == State.Random && ++index == list.Items.Count) index = 0;
  }

  void ShuffleCards()
  {
    for(int i=0; i<list.Items.Count; i++)
    {
      int swapIndex = rand.Next(i, list.Items.Count), temp = indices[swapIndex];
      indices[swapIndex] = indices[i];
      indices[i]         = temp;
    }
  }

  enum State { FirstPass, ReversedPass, Random }

  readonly StudyList list;
  /// <summary>The sum of the item chances (weights).</summary>
  double totalChance;
  /// <summary>An array storing randomized indices into the study list, one per item.</summary>
  int[] indices;
  /// <summary>The current index into <see cref="indices"/>.</summary>
  int index;
  /// <summary>The card traversal state.</summary>
  State state = State.FirstPass;
  /// <summary>Whether the current card is reversed.</summary>
  bool reversed;
  /// <summary>Whether the answer has been shown.</summary>
  bool answerShown;

  void btnYes_Click(object sender, EventArgs e)
  {
    MarkCardAndProceed(true);
  }

  void btnNo_Click(object sender, EventArgs e)
  {
    MarkCardAndProceed(false);
  }

  void btnShowAnswer_Click(object sender, EventArgs e)
  {
    ShowAnswer();
  }

  void btnFinish_Click(object sender, EventArgs e)
  {
    Close();
  }

  void StudyDialog_KeyUp(object sender, KeyEventArgs e)
  {
    if(!e.Alt && !e.Control)
    {
      switch(e.KeyCode)
      {
        case Keys.A: case Keys.Space: ShowAnswer(); break;
        case Keys.Y: MarkCardAndProceed(true); break;
        case Keys.N: MarkCardAndProceed(false); break;
        default: return;
      }

      e.SuppressKeyPress = true;
    }
  }

  void StudyDialog_KeyDown(object sender, KeyEventArgs e)
  {
    if(!e.Alt && !e.Control)
    {
      switch(e.KeyCode)
      {
        case Keys.A: case Keys.Space: case Keys.Y: case Keys.N:
          e.SuppressKeyPress = true;
          break;
      }
    }
  }

  static double GetWeight(StudyList.Item item)
  {
    return 1.1 - item.CorrectRate;
  }

  static readonly Style headwordStyle, readingStyle, exampleStyle, meaningStyle;
  static readonly Random rand = new Random();
}

} // namespace Jappy