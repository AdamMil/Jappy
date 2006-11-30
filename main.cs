using System;
using System.Windows.Forms;

namespace Jappy
{

static class App
{
  public static CharacterDictionary CharDict
  {
    get
    {
      if(charDict == null)
      {
        charDict = new CharacterDictionary();
        charDict.Load("e:/kanji.dict");
      }
      halfMinutesIdle = 0;
      return charDict;
    }
  }

  public static ExampleSentences Examples
  {
    get
    {
      if(examples == null)
      {
        examples = new ExampleSentences();
        examples.Load("e:/examples.dict");
      }
      halfMinutesIdle = 0;
      return examples;
    }
  }

  public static JapaneseDictionary WordDict
  {
    get
    {
      if(wordDict == null)
      {
        wordDict = new JapaneseDictionary();
        wordDict.Load("e:/words.index", "e:/words.dict");
      }
      halfMinutesIdle = 0;
      return wordDict;
    }
  }

  static void Main()
  {
//ExampleSentences examples = new ExampleSentences();
//examples.ImportModifiedTanakaCorpusInUTF8("e:/examples.txt");
//examples.Save("e:/examples.dict");

//CharacterDictionary charDict = new CharacterDictionary();
//charDict.ImportKanjiDicXml(System.IO.File.OpenRead(@"e:\kanjidic2.xml"));
//charDict.Save("e:/kanji.dict");

//JapaneseDictionary wordDict = new JapaneseDictionary();
//wordDict.ImportJMDict(System.IO.File.OpenRead(@"e:/jmdict_e.xml"));
//wordDict.Save("e:/words.index", "e:/words.dict");

    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);

    idleTimer = new Timer();
    idleTimer.Interval = 30000;
    idleTimer.Tick += idleTimer_Tick;
    idleTimer.Start();

    Application.Run(new MainForm());
  }

  static void idleTimer_Tick(object sender, System.EventArgs e)
  {
    halfMinutesIdle++;
    if(halfMinutesIdle == 2) GC.Collect(); // after 1 minute idle, perform a garbage collection

    if(halfMinutesIdle == 10) // after 5 minutes idle, unload the dictionaries.
    {
      charDict.Dispose();
      examples.Dispose();
      wordDict.Dispose();
      charDict = null;
      examples = null;
      wordDict = null;
      GC.Collect();
    }
  }

  static Timer idleTimer;
  static int halfMinutesIdle;
  static CharacterDictionary charDict;
  static ExampleSentences examples;
  static JapaneseDictionary wordDict;
}

} // namespace Jappy