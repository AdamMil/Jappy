using System;
using System.Windows.Forms;

namespace Jappy
{

static class Utilities
{
  public static void Dispose<T>(ref T disposable) where T : class, IDisposable
  {
    if(disposable != null)
    {
      disposable.Dispose();
      disposable = null;
    }
  }
}

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
  
  public static JapaneseDictionary NameDict
  {
    get
    {
      if(nameDict == null)
      {
        nameDict = new JapaneseDictionary();
        nameDict.Load("Names", "e:/names.index", "e:/names.dict");
      }
      halfMinutesIdle = 0;
      return nameDict;
    }
  }

  public static JapaneseDictionary WordDict
  {
    get
    {
      if(wordDict == null)
      {
        wordDict = new JapaneseDictionary();
        wordDict.Load("Word", "e:/words.index", "e:/words.dict");
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

//JapaneseDictionary nameDict = new JapaneseDictionary();
//nameDict.ImportJMDict(System.IO.File.OpenRead(@"e:/JMnedict.xml"));
//nameDict.Save("e:/names.index", "e:/names.dict");

    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.ThreadException += Application_ThreadException;

    idleTimer = new Timer();
    idleTimer.Interval = 30000;
    idleTimer.Tick += idleTimer_Tick;
    idleTimer.Start();

    Application.Run(new MainForm());
  }

  static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
  {
    MessageBox.Show("Unhandled exception:\n"+e.Exception.ToString(), "Unhandled exception");
  }

  static void idleTimer_Tick(object sender, System.EventArgs e)
  {
    halfMinutesIdle++;
    if(halfMinutesIdle == 2) GC.Collect(); // after 1 minute idle, perform a garbage collection

    if(halfMinutesIdle == 10) // after 5 minutes idle, unload the dictionaries.
    {
      Utilities.Dispose(ref charDict);
      Utilities.Dispose(ref examples);
      Utilities.Dispose(ref wordDict);
      GC.Collect();
    }
  }

  static Timer idleTimer;
  static int halfMinutesIdle;
  static CharacterDictionary charDict;
  static ExampleSentences examples;
  static JapaneseDictionary wordDict, nameDict;
}

} // namespace Jappy