using System.Windows.Forms;

namespace Jappy
{

static class App
{
  public static JapaneseDictionary WordDict
  {
    get
    {
      if(wordDict == null)
      {
        wordDict = new JapaneseDictionary();
        wordDict.Load("e:/words.index", "e:/words.dict");
      }
      return wordDict;
    }
  }
  
  public static CharacterDictionary CharDict
  {
    get
    {
      if(charDict == null)
      {
        charDict = new CharacterDictionary();
        charDict.Load("e:/kanji.dict");
      }
      return charDict;
    }
  }

  static void Main()
  {
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new MainForm());
  }

  static JapaneseDictionary wordDict;
  static CharacterDictionary charDict;
  static ExampleSentences examples;
}

} // namespace Jappy