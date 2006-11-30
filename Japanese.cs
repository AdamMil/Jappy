using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Jappy
{

public static class JP
{
  static JP()
  {
    InitializeDoubleToSingleMap();
    InitializeHiraganaToKatakanaMap();
    InitializeRomajiToKanaMap();
  }

  public static string ConvertRomajiToKana(string romaji)
  {
    if(romaji == null) throw new ArgumentNullException();
    romaji = whitespaceRE.Replace(romaji, "").ToLowerInvariant();
    if(romaji == "") return romaji;

    romaji = DoubleWidthToSingleWidth(romaji);
    Match m = romajiRE.Match(romaji);
    int matchEnd = 0;

    StringBuilder kana = new StringBuilder();
    while(m.Success)
    {
      string value = m.Value;
      matchEnd = m.Index + m.Length;

      // handle doubled consonants
      if(value.Length > 2 && value[0] == value[1])
      {
        kana.Append('ッ');
        value = value.Substring(1);
      }

      kana.Append(romajiKanaMap[value]);

      m = m.NextMatch();
      if(m.Success && m.Index != matchEnd) // if the match wasn't immediately following the last one, it's a failure
      {
        while(m.Success) m = m.NextMatch();
      }
    }

    if(matchEnd < romaji.Length)
    {
      int length = romaji.Length-matchEnd > 3 ? 3 : romaji.Length-matchEnd;
      throw new ArgumentException("'"+romaji+"' contains invalid romaji beginning with "+
                                  romaji.Substring(matchEnd, length));
    }
    else
    {
      return kana.ToString();
    }
  }

  public static string ConvertHiraganaToKatakana(string input)
  {
    return ConvertStringUsingCharacterMap(input, hiraToKataMap);
  }

  public static string DoubleWidthToSingleWidth(string input)
  {
    return ConvertStringUsingCharacterMap(input, doubleToSingleMap);
  }

  public static bool IsFullWidthRoman(char c)
  {
    return c == 0x3000 /*space*/ || c >= 0xff01 && c <= 0xff5e /*other roman chars*/;
  }

  public static bool IsFullWidthRomanLetter(char c)
  {
    return c >= 0xff41 && c <= 0xff5a || c >= 0xff21 && c <= 0xff3a;
  }
  
  public static bool IsFullWidthRomanDigit(char c)
  {
    return c >= 0xff10 && c <= 0xff19;
  }

  public static bool IsFullWidthRomanLetterOrDigit(char c)
  {
    return IsFullWidthRomanLetter(c) || IsFullWidthRomanDigit(c);
  }

  public static bool IsFullWidthRomanPunctuation(char c)
  {
    return c >= 0xff01 && c <= 0xff0f || c >= 0xff1a && c <= 0xff20 || c >= 0xff3b && c <= 0xff40 ||
           c >= 0xff5b && c <= 0xff5e;
  }

  public static bool IsCircledKatakana(char c)
  {
    return c >= 0x32d0 && c <= 0x32fe;
  }

  public static bool IsHalfWidthKatakana(char c)
  {
    return c >= 0xff66 && c <= 0xff9f;
  }
  
  public static bool IsHalfWidthJapanesePunctuation(char c)
  {
    return c >= 0xff61 && c <= 0xff65;
  }

  public static bool IsHalfWidthJapanese(char c)
  {
    return IsHalfWidthKatakana(c) || IsHalfWidthJapanesePunctuation(c);
  }

  public static bool IsHiragana(char c)
  {
    return c >= 0x3040 && c <= 0x309f;
  }

  public static bool IsKatakana(char c)
  {
    return c >= 0x30a0 && c <= 0x30ff || IsHalfWidthKatakana(c) || IsCircledKatakana(c);
  }

  public static bool IsKana(char c)
  {
    return IsHiragana(c) || IsKatakana(c);
  }
  
  public static bool IsJapanese(char c)
  {
    return IsKanji(c) || IsKana(c) || IsJapanesePunctuation(c);
  }

  public static bool IsJapanesePunctuation(char c)
  {
    return c >= 0x3001 && c <= 0x303f /*normal punctuation, except space*/ || IsHalfWidthJapanesePunctuation(c);
  }

  public static bool IsKanji(char c)
  {
    return c >= 0x4e00 && c <= 0x9faf /*normal kanji*/ || c >= 0x3400 && c <= 0x4dbf /*rare kanji extension*/;
  }

  public static bool IsParticle(char c)
  {
    return c == 'を' || c == 'の' || c == 'な' || c == 'に' || c == 'と' || c == 'も' || c == 'で' || c == 'が' ||
           c == 'は' || c == 'へ' || c == 'ね' || c == 'か' || c == 'よ' || c == 'や';
  }

  internal static string ConvertStringUsingCharacterMap(string str, Dictionary<char,char> map)
  {
    if(str == null) return null;

    char[] chars = str.ToCharArray();
    for(int i=0; i<str.Length; i++)
    {
      char newChar;
      if(map.TryGetValue(str[i], out newChar))
      {
        chars[i] = newChar;
      }
    }
    return new string(chars);
  }

  static void InitializeDoubleToSingleMap()
  {
    // some of these, like 「」, aren't mapped so well. others, like ‘’”, don't really belong...
    const string doubleWidthChars = "　‘’”、。「」？ー！～＠＃＄％＾＆＊（）１２３４５６７８９０－＝＿＋｛｝＜＞；：・ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ";
    const string singleWidthChars = " ''\",.\"\"?-!~@#$%^&*()1234567890-=_+{}<>;:/abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    Debug.Assert(doubleWidthChars.Length == singleWidthChars.Length);
    for(int i=0; i<doubleWidthChars.Length; i++)
    {
      doubleToSingleMap.Add(doubleWidthChars[i], singleWidthChars[i]);
    }
  }

  static void InitializeHiraganaToKatakanaMap()
  {
    const string hiragana = "ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔ";
    const string katakana = "ァアィイゥウェエォオカガキギクグケゲコゴサザシジスズセゼソゾタダチヂッツヅテデトドナニヌネノハバパヒビピフブプヘベペホボポマミムメモャヤュユョヨラリルレロヮワヰヱヲンヴ";

    Debug.Assert(hiragana.Length == katakana.Length);
    for(int i=0; i<hiragana.Length; i++)
    {
      hiraToKataMap.Add(hiragana[i], katakana[i]);
    }
  }

  static void InitializeRomajiToKanaMap()
  {
    #region Romaji -> Kana map
    string[] romajiKanaTable =
    {
      "a", "ア", "i", "イ", "u", "ウ", "e", "エ", "o", "オ", 
      "ba", "バ", "bi", "ビ", "bu", "ブ", "be", "ベ", "bo", "ボ", 
      "bya", "ビャ", "byi", "ビィ", "byu", "ビュ", "bye", "ビェ", "byo", "ビョ", 
      "ca", "カ", "ci", "シ", "cu", "ク", "ce", "セ", "co", "コ", 
      "cha", "チャ", "chi", "チ", "chu", "チュ", "che", "チェ", "cho", "チョ", 
      "cya", "チャ", "cyi", "チィ", "cyu", "チュ", "cye", "チェ", "cyo", "チョ", 
      "da", "ダ", "di", "ヂ", "du", "ヅ", "de", "デ", "do", "ド", 
      "dha", "デャ", "dhi", "ディ", "dhu", "デュ", "dhe", "デェ", "dho", "デョ", 
      "dwa", "ドァ", "dwi", "ドィ", "dwu", "ドゥ", "dwe", "ドェ", "dwo", "ドォ", 
      "dya", "ヂャ", "dyi", "ヂィ", "dyu", "ヂュ", "dye", "ヂェ", "dyo", "ヂョ", 
      "fa", "ファ", "fi", "フィ", "fu", "フ", "fe", "フェ", "fo", "フォ", 
      "fwa", "ファ", "fwi", "フィ", "fwu", "フゥ", "fwe", "フェ", "fwo", "フォ", 
      "fya", "フャ", "fyi", "フィ", "fyu", "フュ", "fye", "フェ", "fyo", "フョ", 
      "ga", "ガ", "gi", "ギ", "gu", "グ", "ge", "ゲ", "go", "ゴ", 
      "gwa", "グァ", "gwi", "グィ", "gwu", "グゥ", "gwe", "グェ", "gwo", "グォ", 
      "gya", "ギャ", "gyi", "ギィ", "gyu", "ギュ", "gye", "ギェ", "gyo", "ギョ", 
      "ha", "ハ", "hi", "ヒ", "hu", "フ", "he", "ヘ", "ho", "ホ", 
      "hya", "ヒャ", "hyi", "ヒィ", "hyu", "ヒュ", "hye", "ヒェ", "hyo", "ヒョ", 
      "ja", "ジャ", "ji", "ジ", "ju", "ジュ", "je", "ジェ", "jo", "ジョ", 
      "jya", "ジャ", "jyi", "ジィ", "jyu", "ジュ", "jye", "ジェ", "jyo", "ジョ", 
      "ka", "カ", "ki", "キ", "ku", "ク", "ke", "ケ", "ko", "コ", 
      "kwa", "クァ", 
      "kya", "キャ", "kyi", "キィ", "kyu", "キュ", "kye", "キェ", "kyo", "キョ", 
      "la", "ァ", "li", "ィ", "lu", "ゥ", "le", "ェ", "lo", "ォ", 
      "lya", "ャ", "lyi", "ィ", "lyu", "ュ", "lye", "ェ", "lyo", "ョ", 
      "ma", "マ", "mi", "ミ", "mu", "ム", "me", "メ", "mo", "モ", 
      "mya", "ミャ", "myi", "ミィ", "myu", "ミュ", "mye", "ミェ", "myo", "ミョ", 
      "na", "ナ", "ni", "ニ", "nu", "ヌ", "ne", "ネ", "no", "ノ", 
      "nya", "ニャ", "nyi", "ニィ", "nyu", "ニュ", "nye", "ニェ", "nyo", "ニョ", 
      "n", "ン",
      "pa", "パ", "pi", "ピ", "pu", "プ", "pe", "ペ", "po", "ポ", 
      "pya", "ピャ", "pyi", "ピィ", "pyu", "ピュ", "pye", "ピェ", "pyo", "ピョ", 
      "qa", "クァ", "qi", "クィ", "qu", "ク", "qe", "クェ", "qo", "クォ", 
      "qwa", "クァ", "qwi", "クィ", "qwu", "クゥ", "qwe", "クェ", "qwo", "クォ", 
      "qya", "クャ", "qyi", "クィ", "qyu", "クュ", "qye", "クェ", "qyo", "クョ", 
      "ra", "ラ", "ri", "リ", "ru", "ル", "re", "レ", "ro", "ロ", 
      "rya", "リャ", "ryi", "リィ", "ryu", "リュ", "rye", "リェ", "ryo", "リョ", 
      "sa", "サ", "si", "シ", "su", "ス", "se", "セ", "so", "ソ", 
      "sha", "シャ", "shi", "シ", "shu", "シュ", "she", "シェ", "sho", "ショ", 
      "swa", "スァ", "swi", "スィ", "swu", "スゥ", "swe", "スェ", "swo", "スォ", 
      "sya", "シャ", "syi", "シィ", "syu", "シュ", "sye", "シェ", "syo", "ショ", 
      "ta", "タ", "ti", "チ", "tu", "ツ", "te", "テ", "to", "ト", 
      "tha", "テャ", "thi", "ティ", "thu", "テュ", "the", "テェ", "tho", "テョ", 
      "tsa", "ツァ", "tsi", "ツィ", "tsu", "ツ", "tse", "ツェ", "tso", "ツォ", 
      "twa", "トァ", "twi", "トィ", "twu", "トゥ", "twe", "トェ", "two", "トォ", 
      "tya", "チャ", "tyi", "チィ", "tyu", "チュ", "tye", "チェ", "tyo", "チョ", 
      "va", "ヴァ", "vi", "ヴィ", "vu", "ヴ", "ve", "ヴェ", "vo", "ヴォ", 
      "vya", "ヴャ", "vyi", "ヴィ", "vyu", "ヴュ", "vye", "ヴェ", "vyo", "ヴョ", 
      "wa", "ワ", "wi", "ウィ", "wu", "ウ", "we", "ウェ", "wo", "ヲ",
      "wha", "ウァ", "whi", "ウィ", "whu", "ウ", "whe", "ウェ", "who", "ウォ", 
      "xa", "ァ", "xi", "ィ", "xu", "ゥ", "xe", "ェ", "xo", "ォ", 
      "xka", "ヵ", "xke", "ヶ", "xn", "ン", "xtu", "ッ", "xwa", "ヮ", 
      "xya", "ャ", "xyi", "ィ", "xyu", "ュ", "xye", "ェ", "xyo", "ョ", 
      "ya", "ヤ", "yi", "イ", "yu", "ユ", "ye", "イェ", "yo", "ヨ", 
      "za", "ザ", "zi", "ジ", "zu", "ズ", "ze", "ゼ", "zo", "ゾ", 
      "zya", "ジャ", "zyi", "ジィ", "zyu", "ジュ", "zye", "ジェ", "zyo", "ジョ",
      "~", "ー", "-", "ー",
    };
    #endregion

    Debug.Assert(romajiKanaTable.Length % 2 == 0); // make sure there are an even number of them

    for(int i=0; i<romajiKanaTable.Length; i+=2)
    {
      #if DEBUG // make sure the romaji part contains no high characters and the kana contains no low characters.
      {         // this helps ensure that the pairs are properly matched
        string s = romajiKanaTable[i];
        for(int si=0; si<s.Length; si++)
        {
          Debug.Assert((char)s[si] < 127);
        }

        s = romajiKanaTable[i+1];
        for(int si=0; si<s.Length; si++)
        {
          Debug.Assert((char)s[si] > 127);
        }
      }
      #endif

      romajiKanaMap.Add(romajiKanaTable[i], romajiKanaTable[i+1]);
    }
  }

  static readonly Dictionary<char, char> hiraToKataMap = new Dictionary<char, char>();
  static readonly Dictionary<char, char> doubleToSingleMap = new Dictionary<char, char>();
  static readonly Dictionary<string, string> romajiKanaMap = new Dictionary<string, string>();

  static readonly Regex romajiRE = new Regex(
    @"(([bp]{1,2}|[hjmnrvz])y? | cc?[hy]? | dd?[hwy]? | ([gq]{1,2}|f)[wy]? | ss?[hwy]? | tt?[hswy]? | wh? | kk? | y)?[aeiou] |
      n | [lx](y?[aeiou]|k[ae]|tu|wa) | k?kwa | xn",
    RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace |
    RegexOptions.Singleline | RegexOptions.ExplicitCapture);

  static readonly Regex whitespaceRE = new Regex(@"\s+", RegexOptions.CultureInvariant|RegexOptions.Singleline);
}

} // namespace Jappy