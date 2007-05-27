using System;
using System.IO;
using System.Windows.Forms;
using Jappy.Backend;

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
  static App()
  {
    #if DEBUG
    exeDir = "d:/adammil/code/jappy/data";
    #else
    exeDir = Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName),
                          "dictionaries");
    #endif
  }

  public static MainForm MainForm
  {
    get { return mainForm; }
  }

  public static CharacterDictionary CharDict
  {
    get
    {
      if(charDict == null)
      {
        charDict = new CharacterDictionary();
        charDict.Load(Path.Combine(exeDir, "kanji.dict"));
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
        examples.Load(Path.Combine(exeDir, "examples.dict"));
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
        nameDict.Load("Names", Path.Combine(exeDir, "names.index"), Path.Combine(exeDir, "names.dict"));
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
        wordDict.Load("edict", Path.Combine(exeDir, "words.index"), Path.Combine(exeDir, "words.dict"));
      }
      halfMinutesIdle = 0;
      return wordDict;
    }
  }

  public static readonly Random Random = new Random();

  [STAThread]
  static void Main()
  {
//ExampleSentences examples = new ExampleSentences();
//examples.ImportModifiedTanakaCorpusInUTF8(new System.IO.Compression.GZipStream(File.OpenRead("e:/examples.txt.gz"), System.IO.Compression.CompressionMode.Decompress));
//examples.Save("e:/examples.dict");

//CharacterDictionary charDict = new CharacterDictionary();
//charDict.ImportKanjiDicXml(System.IO.File.OpenRead(@"e:\kanjidic2.xml"));
//charDict.Save("e:/kanji.dict");

//JapaneseDictionary wordDict = new JapaneseDictionary();
//wordDict.ImportJMDict(new System.IO.Compression.GZipStream(System.IO.File.OpenRead(@"e:/words.xml.gz"), System.IO.Compression.CompressionMode.Decompress));
//wordDict.Save("e:/words.index", "e:/words.dict");

//JapaneseDictionary nameDict = new JapaneseDictionary();
///nameDict.ImportJMDict(new System.IO.Compression.GZipStream(System.IO.File.OpenRead(@"e:/names.xml.gz"), System.IO.Compression.CompressionMode.Decompress));
//nameDict.Save("e:/names.index", "e:/names.dict");

    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.ThreadException += Application_ThreadException;

    idleTimer = new Timer();
    idleTimer.Interval = 30000;
    idleTimer.Tick += idleTimer_Tick;
    idleTimer.Start();

    mainForm = new MainForm();
    Application.Run(mainForm);
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
      /*Utilities.Dispose(ref charDict); we can't do this as long as TextRegions are holding references to dictionaries
      Utilities.Dispose(ref examples);
      Utilities.Dispose(ref wordDict);
      GC.Collect();*/
    }
  }

  static Timer idleTimer;
  static int halfMinutesIdle;
  static CharacterDictionary charDict;
  static ExampleSentences examples;
  static JapaneseDictionary wordDict, nameDict;
  static MainForm mainForm;
  
  static readonly string exeDir;
}

} // namespace Jappy