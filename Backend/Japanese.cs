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
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Jappy.Backend
{

#region KanaInfo
[Flags]
public enum KanaInfo
{
  /// <summary>Not a valid kana</summary>
  Invalid,
  /// <summary>The A row (あいうえお) and its variants</summary>
  ARow=1,
  /// <summary>The Ka row (かきくけこ) and its variants</summary>
  KaRow=2,
  /// <summary>The Ka row (さしすせそ) and its variants</summary>
  SaRow=3,
  /// <summary>The Ka row (たちつてと) and its variants</summary>
  TaRow=4,
  /// <summary>The Ka row (なにぬねの) and its variants</summary>
  NaRow=5,
  /// <summary>The Ka row (はひふへほ) and its variants</summary>
  HaRow=6,
  /// <summary>The Ka row (まみむめも) and its variants</summary>
  MaRow=7,
  /// <summary>The Ka row (やゆよ) and its variants</summary>
  YaRow=8,
  /// <summary>The Ka row (らりるれろ) and its variants</summary>
  RaRow=9,
  /// <summary>The Ka row (わゐゑを) and its variants</summary>
  WaRow=10,
  /// <summary>The N, containing only ん</summary>
  NRow=11,
  /// <summary>A mask that can be applied to get which row the kana is in</summary>
  RowMask=0xF,

  /// <summary>The Ka row, but voiced</summary>
  GaRow=KaRow|Voiced,
  /// <summary>The Sa row, but voiced</summary>
  ZaRow=SaRow|Voiced,
  /// <summary>The Ta row, but voiced</summary>
  DaRow=TaRow|Voiced,
  /// <summary>The Ha row, but voiced</summary>
  BaRow=HaRow|Voiced,
  /// <summary>The Ha row, but semivoiced</summary>
  PaRow=HaRow|SemiVoiced,
  /// <summary>The Wa row, but voiced</summary>
  VaRow=WaRow|Voiced,

  /// <summary>Kana ending in the あ sound</summary>
  A=0x10,
  /// <summary>Kana ending in the い sound</summary>
  I=0x20,
  /// <summary>Kana ending in the う sound</summary>
  U=0x30,
  /// <summary>Kana ending in the え sound</summary>
  E=0x40,
  /// <summary>Kana ending in the お sound</summary>
  O=0x50,
  /// <summary>The ん kana and its variants</summary>
  N=0x60,
  /// <summary>A mask that can be applied to get which ending sound the kana has</summary>
  SoundMask=0x70,

  /// <summary>The kana is voiced (has a ゛ mark)</summary>
  Voiced=0x80,
  /// <summary>The kana is semi-voiced (has a ゜ mark). Note that this enumeration value includes the bit for Voiced.</summary>
  SemiVoiced=0x180,
  /// <summary>A mask that can be applied to determine the voicing of a kana</summary>
  VoiceMask=0x180,
  /// <summary>The kana is katakana. In the absense of this flag, it can be assumed to be hiragana.</summary>
  Katakana=0x200,
  /// <summary>The kana is small, as in っゃゅょ, etc</summary>
  Small=0x400,
  /// <summary>The kana is halfwidth</summary>
  HalfWidth=0x800,
  /// <summary>The kana is circled</summary>
  Circled=0x1000,
  /// <summary>The kana is a halfwidth katakana. This is simply the combination of the Katakana and Halfwidth flags.</summary>
  HalfwidthKatakana=Katakana|HalfWidth,
  /// <summary>The kana is a circled katakana. This is simply the combination of the Katakana and Circled flags.</summary>
  CircledKatakana=Katakana|Circled,
}
#endregion

public static class JP
{
  static JP()
  {
    InitializeDoubleToSingleMap();
    InitializeHiraganaToKatakanaMap();
    InitializeRomajiToKanaMap();
    InitializeKanaInfoMap();
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

  public static KanaInfo GetKanaInfo(char c)
  {
    KanaInfo info;
    return kanaInfoMap.TryGetValue(c, out info) ? info : KanaInfo.Invalid;
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

  public static bool IsNormalKatakana(char c)
  {
    return c >= 0x30a0 && c <= 0x30ff; // normal (not half-width or circled) katakana
  }

  public static bool IsKatakana(char c)
  {
    return IsNormalKatakana(c) || IsHalfWidthKatakana(c) || IsCircledKatakana(c);
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
    return c >= 0x3001 && c <= 0x303f && c != '々' /*normal punctuation, except space and repetition mark */ ||
           IsHalfWidthJapanesePunctuation(c);
  }

  public static bool IsKanji(char c)
  {
    return c >= 0x4e00 && c <= 0x9faf /*normal kanji*/ || c >= 0x3400 && c <= 0x4dbf /*rare kanji extension*/ ||
           c == '々' /*kanji repetition mark*/;
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
    doubleToSingleMap = new Dictionary<char,char>();
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
    hiraToKataMap = new Dictionary<char,char>(hiragana.Length);
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
      "n", "ン", "nn", "ン",
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

    romajiKanaMap = new Dictionary<string,string>(romajiKanaTable.Length/2);
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
  
  static void InitializeKanaInfoMap()
  {
    #region Kana info map
    char[] kana = new char[]
    {
      'あ', 'い', 'う', 'え', 'お',
      'ぁ', 'ぃ', 'ぅ', 'ぇ', 'ぉ',
      'か', 'き', 'く', 'け', 'こ',
      'が', 'ぎ', 'ぐ', 'げ', 'ご',
      'さ', 'し', 'す', 'せ', 'そ',
      'ざ', 'じ', 'ず', 'ぜ', 'ぞ',
      'た', 'ち', 'つ', 'て', 'と', 'っ',
      'だ', 'ぢ', 'づ', 'で', 'ど',
      'な', 'に', 'ぬ', 'ね', 'の',
      'は', 'ひ', 'ふ', 'へ', 'ほ',
      'ば', 'び', 'ぶ', 'べ', 'ぼ',
      'ぱ', 'ぴ', 'ぷ', 'ぺ', 'ぽ',
      'ま', 'み', 'む', 'め', 'も',
      'や', 'ゆ', 'よ', 'ゃ', 'ゅ', 'ょ',
      'ら', 'り', 'る', 'れ', 'ろ',
      'わ', 'ゐ', 'ゑ', 'を', 'ゎ',
      'ん',

      'ア', 'イ', 'ウ', 'エ', 'オ',
      'ァ', 'ィ', 'ゥ', 'ェ', 'ォ',
      'カ', 'キ', 'ク', 'ケ', 'コ',
      'ガ', 'ギ', 'グ', 'ゲ', 'ゴ',
      'サ', 'シ', 'ス', 'セ', 'ソ',
      'ザ', 'ジ', 'ズ', 'ゼ', 'ゾ',
      'タ', 'チ', 'ツ', 'テ', 'ト', 'ッ',
      'ダ', 'ヂ', 'ヅ', 'デ', 'ド',
      'ナ', 'ニ', 'ヌ', 'ネ', 'ノ',
      'ハ', 'ヒ', 'フ', 'ヘ', 'ホ',
      'バ', 'ビ', 'ブ', 'ベ', 'ボ',
      'パ', 'ピ', 'プ', 'ペ', 'ポ',
      'マ', 'ミ', 'ム', 'メ', 'モ',
      'ヤ', 'ユ', 'ヨ', 'ャ', 'ュ', 'ョ',
      'ラ', 'リ', 'ル', 'レ', 'ロ',
      'ワ', 'ヰ', 'ヱ', 'ヲ', 'ヮ',
      'ン', 

      'ヷ', 'ヸ', 'ヹ', 'ヺ', 'ヵ', 'ヶ',

      'ｱ', 'ｲ', 'ｳ', 'ｴ', 'ｵ',
      'ｶ', 'ｷ', 'ｸ', 'ｹ', 'ｺ',
      'ｻ', 'ｼ', 'ｽ', 'ｾ', 'ｿ',
      'ﾀ', 'ﾁ', 'ﾂ', 'ﾃ', 'ﾄ', 'ｯ',
      'ﾅ', 'ﾆ', 'ﾇ', 'ﾈ', 'ﾉ',
      'ﾊ', 'ﾋ', 'ﾌ', 'ﾍ', 'ﾎ',
      'ﾏ', 'ﾐ', 'ﾑ', 'ﾒ', 'ﾓ',
      'ﾔ', 'ﾕ', 'ﾖ', 'ｬ', 'ｭ', 'ｮ',
      'ﾗ', 'ﾘ', 'ﾙ', 'ﾚ', 'ﾛ',
      'ｦ', 'ﾝ',
      
      '㋐', '㋑', '㋒', '㋓', '㋔',
      '㋕', '㋖', '㋗', '㋘', '㋙',
      '㋚', '㋛', '㋜', '㋝', '㋞',
      '㋟', '㋠', '㋡', '㋢', '㋣',
      '㋤', '㋥', '㋦', '㋧', '㋨',
      '㋩', '㋪', '㋫', '㋬', '㋭',
      '㋮', '㋯', '㋰', '㋱', '㋲',
      '㋳', '㋴', '㋵',
      '㋶', '㋷', '㋸', '㋹', '㋺',
      '㋻', '㋼', '㋽', '㋾',
    };

    KanaInfo[] info = new KanaInfo[]
    {
      // hiragana
      KanaInfo.ARow|KanaInfo.A, KanaInfo.ARow|KanaInfo.I, KanaInfo.ARow|KanaInfo.U, KanaInfo.ARow|KanaInfo.E,
      KanaInfo.ARow|KanaInfo.O,

      KanaInfo.ARow|KanaInfo.A|KanaInfo.Small, KanaInfo.ARow|KanaInfo.I|KanaInfo.Small,
      KanaInfo.ARow|KanaInfo.U|KanaInfo.Small, KanaInfo.ARow|KanaInfo.E|KanaInfo.Small,
      KanaInfo.ARow|KanaInfo.O|KanaInfo.Small,
      
      KanaInfo.KaRow|KanaInfo.A, KanaInfo.KaRow|KanaInfo.I, KanaInfo.KaRow|KanaInfo.U,
      KanaInfo.KaRow|KanaInfo.E, KanaInfo.KaRow|KanaInfo.O,

      KanaInfo.GaRow|KanaInfo.A, KanaInfo.GaRow|KanaInfo.I, KanaInfo.GaRow|KanaInfo.U,
      KanaInfo.GaRow|KanaInfo.E, KanaInfo.GaRow|KanaInfo.O,

      KanaInfo.SaRow|KanaInfo.A, KanaInfo.SaRow|KanaInfo.I, KanaInfo.SaRow|KanaInfo.U,
      KanaInfo.SaRow|KanaInfo.E, KanaInfo.SaRow|KanaInfo.O,

      KanaInfo.ZaRow|KanaInfo.A, KanaInfo.ZaRow|KanaInfo.I, KanaInfo.ZaRow|KanaInfo.U,
      KanaInfo.ZaRow|KanaInfo.E, KanaInfo.ZaRow|KanaInfo.O,

      KanaInfo.TaRow|KanaInfo.A, KanaInfo.TaRow|KanaInfo.I, KanaInfo.TaRow|KanaInfo.U,
      KanaInfo.TaRow|KanaInfo.E, KanaInfo.TaRow|KanaInfo.O, KanaInfo.TaRow|KanaInfo.U|KanaInfo.Small,

      KanaInfo.DaRow|KanaInfo.A, KanaInfo.DaRow|KanaInfo.I, KanaInfo.DaRow|KanaInfo.U,
      KanaInfo.DaRow|KanaInfo.E, KanaInfo.DaRow|KanaInfo.O,

      KanaInfo.NaRow|KanaInfo.A, KanaInfo.NaRow|KanaInfo.I, KanaInfo.NaRow|KanaInfo.U,
      KanaInfo.NaRow|KanaInfo.E, KanaInfo.NaRow|KanaInfo.O,
 
      KanaInfo.HaRow|KanaInfo.A, KanaInfo.HaRow|KanaInfo.I, KanaInfo.HaRow|KanaInfo.U,
      KanaInfo.HaRow|KanaInfo.E, KanaInfo.HaRow|KanaInfo.O,

      KanaInfo.BaRow|KanaInfo.A, KanaInfo.BaRow|KanaInfo.I, KanaInfo.BaRow|KanaInfo.U,
      KanaInfo.BaRow|KanaInfo.E, KanaInfo.BaRow|KanaInfo.O,

      KanaInfo.PaRow|KanaInfo.A, KanaInfo.PaRow|KanaInfo.I, KanaInfo.PaRow|KanaInfo.U,
      KanaInfo.PaRow|KanaInfo.E, KanaInfo.PaRow|KanaInfo.O,

      KanaInfo.MaRow|KanaInfo.A, KanaInfo.MaRow|KanaInfo.I, KanaInfo.MaRow|KanaInfo.U,
      KanaInfo.MaRow|KanaInfo.E, KanaInfo.MaRow|KanaInfo.O,

      KanaInfo.YaRow|KanaInfo.A, KanaInfo.YaRow|KanaInfo.U, KanaInfo.YaRow|KanaInfo.O,
      KanaInfo.YaRow|KanaInfo.A|KanaInfo.Small, KanaInfo.YaRow|KanaInfo.U|KanaInfo.Small,
      KanaInfo.YaRow|KanaInfo.O|KanaInfo.Small,

      KanaInfo.RaRow|KanaInfo.A, KanaInfo.RaRow|KanaInfo.I, KanaInfo.RaRow|KanaInfo.U,
      KanaInfo.RaRow|KanaInfo.E, KanaInfo.RaRow|KanaInfo.O,

      KanaInfo.WaRow|KanaInfo.A, KanaInfo.WaRow|KanaInfo.I,
      KanaInfo.WaRow|KanaInfo.E, KanaInfo.WaRow|KanaInfo.O, KanaInfo.WaRow|KanaInfo.A|KanaInfo.Small,

      KanaInfo.NRow|KanaInfo.N,

      // katakana characters that match hiragana characters
      KanaInfo.ARow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.ARow|KanaInfo.I|KanaInfo.Katakana,
      KanaInfo.ARow|KanaInfo.U|KanaInfo.Katakana, KanaInfo.ARow|KanaInfo.E|KanaInfo.Katakana,
      KanaInfo.ARow|KanaInfo.O|KanaInfo.Katakana,

      KanaInfo.ARow|KanaInfo.A|KanaInfo.Small|KanaInfo.Katakana, KanaInfo.ARow|KanaInfo.I|KanaInfo.Small|KanaInfo.Katakana,
      KanaInfo.ARow|KanaInfo.U|KanaInfo.Small|KanaInfo.Katakana, KanaInfo.ARow|KanaInfo.E|KanaInfo.Small|KanaInfo.Katakana,
      KanaInfo.ARow|KanaInfo.O|KanaInfo.Small|KanaInfo.Katakana,
      
      KanaInfo.KaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.KaRow|KanaInfo.I|KanaInfo.Katakana, KanaInfo.KaRow|KanaInfo.U|KanaInfo.Katakana,
      KanaInfo.KaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.KaRow|KanaInfo.O|KanaInfo.Katakana,

      KanaInfo.GaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.GaRow|KanaInfo.I|KanaInfo.Katakana, KanaInfo.GaRow|KanaInfo.U|KanaInfo.Katakana,
      KanaInfo.GaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.GaRow|KanaInfo.O|KanaInfo.Katakana,

      KanaInfo.SaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.SaRow|KanaInfo.I|KanaInfo.Katakana, KanaInfo.SaRow|KanaInfo.U|KanaInfo.Katakana,
      KanaInfo.SaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.SaRow|KanaInfo.O|KanaInfo.Katakana,

      KanaInfo.ZaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.ZaRow|KanaInfo.I|KanaInfo.Katakana, KanaInfo.ZaRow|KanaInfo.U|KanaInfo.Katakana,
      KanaInfo.ZaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.ZaRow|KanaInfo.O|KanaInfo.Katakana,

      KanaInfo.TaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.TaRow|KanaInfo.I|KanaInfo.Katakana, KanaInfo.TaRow|KanaInfo.U|KanaInfo.Katakana,
      KanaInfo.TaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.TaRow|KanaInfo.O|KanaInfo.Katakana,
      KanaInfo.TaRow|KanaInfo.U|KanaInfo.Small|KanaInfo.Katakana,

      KanaInfo.DaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.DaRow|KanaInfo.I|KanaInfo.Katakana, KanaInfo.DaRow|KanaInfo.U|KanaInfo.Katakana,
      KanaInfo.DaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.DaRow|KanaInfo.O|KanaInfo.Katakana,

      KanaInfo.NaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.NaRow|KanaInfo.I|KanaInfo.Katakana, KanaInfo.NaRow|KanaInfo.U|KanaInfo.Katakana,
      KanaInfo.NaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.NaRow|KanaInfo.O|KanaInfo.Katakana,
 
      KanaInfo.HaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.HaRow|KanaInfo.I|KanaInfo.Katakana, KanaInfo.HaRow|KanaInfo.U|KanaInfo.Katakana,
      KanaInfo.HaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.HaRow|KanaInfo.O|KanaInfo.Katakana,

      KanaInfo.BaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.BaRow|KanaInfo.I|KanaInfo.Katakana, KanaInfo.BaRow|KanaInfo.U|KanaInfo.Katakana,
      KanaInfo.BaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.BaRow|KanaInfo.O|KanaInfo.Katakana,

      KanaInfo.PaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.PaRow|KanaInfo.I|KanaInfo.Katakana, KanaInfo.PaRow|KanaInfo.U|KanaInfo.Katakana,
      KanaInfo.PaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.PaRow|KanaInfo.O|KanaInfo.Katakana,

      KanaInfo.MaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.MaRow|KanaInfo.I|KanaInfo.Katakana, KanaInfo.MaRow|KanaInfo.U|KanaInfo.Katakana,
      KanaInfo.MaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.MaRow|KanaInfo.O|KanaInfo.Katakana,

      KanaInfo.YaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.YaRow|KanaInfo.U|KanaInfo.Katakana, KanaInfo.YaRow|KanaInfo.O|KanaInfo.Katakana,
      KanaInfo.YaRow|KanaInfo.A|KanaInfo.Small|KanaInfo.Katakana, KanaInfo.YaRow|KanaInfo.U|KanaInfo.Small|KanaInfo.Katakana,
      KanaInfo.YaRow|KanaInfo.O|KanaInfo.Small|KanaInfo.Katakana,

      KanaInfo.RaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.RaRow|KanaInfo.I|KanaInfo.Katakana, KanaInfo.RaRow|KanaInfo.U|KanaInfo.Katakana,
      KanaInfo.RaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.RaRow|KanaInfo.O|KanaInfo.Katakana,

      KanaInfo.WaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.WaRow|KanaInfo.I|KanaInfo.Katakana,
      KanaInfo.WaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.WaRow|KanaInfo.O|KanaInfo.Katakana,
      KanaInfo.WaRow|KanaInfo.A|KanaInfo.Small|KanaInfo.Katakana,

      KanaInfo.NRow|KanaInfo.N|KanaInfo.Katakana,
      
      // katakana characters that don't exist in hiragana
      KanaInfo.VaRow|KanaInfo.A|KanaInfo.Katakana, KanaInfo.VaRow|KanaInfo.I|KanaInfo.Katakana, // va vi
      KanaInfo.VaRow|KanaInfo.E|KanaInfo.Katakana, KanaInfo.VaRow|KanaInfo.O|KanaInfo.Katakana, // ve vo
      KanaInfo.KaRow|KanaInfo.A|KanaInfo.Katakana|KanaInfo.Small, KanaInfo.KaRow|KanaInfo.E|KanaInfo.Katakana|KanaInfo.Small, // small ka and ke
      
      // halfwidth katakana
      KanaInfo.ARow|KanaInfo.A|KanaInfo.HalfwidthKatakana, KanaInfo.ARow|KanaInfo.I|KanaInfo.HalfwidthKatakana,
      KanaInfo.ARow|KanaInfo.U|KanaInfo.HalfwidthKatakana, KanaInfo.ARow|KanaInfo.E|KanaInfo.HalfwidthKatakana,
      KanaInfo.ARow|KanaInfo.O|KanaInfo.HalfwidthKatakana,

      KanaInfo.KaRow|KanaInfo.A|KanaInfo.HalfwidthKatakana, KanaInfo.KaRow|KanaInfo.I|KanaInfo.HalfwidthKatakana,
      KanaInfo.KaRow|KanaInfo.U|KanaInfo.HalfwidthKatakana, KanaInfo.KaRow|KanaInfo.E|KanaInfo.HalfwidthKatakana,
      KanaInfo.KaRow|KanaInfo.O|KanaInfo.HalfwidthKatakana,

      KanaInfo.SaRow|KanaInfo.A|KanaInfo.HalfwidthKatakana, KanaInfo.SaRow|KanaInfo.I|KanaInfo.HalfwidthKatakana,
      KanaInfo.SaRow|KanaInfo.U|KanaInfo.HalfwidthKatakana, KanaInfo.SaRow|KanaInfo.E|KanaInfo.HalfwidthKatakana,
      KanaInfo.SaRow|KanaInfo.O|KanaInfo.HalfwidthKatakana,

      KanaInfo.TaRow|KanaInfo.A|KanaInfo.HalfwidthKatakana, KanaInfo.TaRow|KanaInfo.I|KanaInfo.HalfwidthKatakana,
      KanaInfo.TaRow|KanaInfo.U|KanaInfo.HalfwidthKatakana, KanaInfo.TaRow|KanaInfo.E|KanaInfo.HalfwidthKatakana,
      KanaInfo.TaRow|KanaInfo.O|KanaInfo.HalfwidthKatakana, KanaInfo.TaRow|KanaInfo.U|KanaInfo.Small|KanaInfo.HalfwidthKatakana,

      KanaInfo.NaRow|KanaInfo.A|KanaInfo.HalfwidthKatakana, KanaInfo.NaRow|KanaInfo.I|KanaInfo.HalfwidthKatakana,
      KanaInfo.NaRow|KanaInfo.U|KanaInfo.HalfwidthKatakana, KanaInfo.NaRow|KanaInfo.E|KanaInfo.HalfwidthKatakana,
      KanaInfo.NaRow|KanaInfo.O|KanaInfo.HalfwidthKatakana,

      KanaInfo.HaRow|KanaInfo.A|KanaInfo.HalfwidthKatakana, KanaInfo.HaRow|KanaInfo.I|KanaInfo.HalfwidthKatakana,
      KanaInfo.HaRow|KanaInfo.U|KanaInfo.HalfwidthKatakana, KanaInfo.HaRow|KanaInfo.E|KanaInfo.HalfwidthKatakana,
      KanaInfo.HaRow|KanaInfo.O|KanaInfo.HalfwidthKatakana,

      KanaInfo.MaRow|KanaInfo.A|KanaInfo.HalfwidthKatakana, KanaInfo.MaRow|KanaInfo.I|KanaInfo.HalfwidthKatakana,
      KanaInfo.MaRow|KanaInfo.U|KanaInfo.HalfwidthKatakana, KanaInfo.MaRow|KanaInfo.E|KanaInfo.HalfwidthKatakana,
      KanaInfo.MaRow|KanaInfo.O|KanaInfo.HalfwidthKatakana,

      KanaInfo.YaRow|KanaInfo.A|KanaInfo.HalfwidthKatakana, KanaInfo.YaRow|KanaInfo.U|KanaInfo.HalfwidthKatakana,
      KanaInfo.YaRow|KanaInfo.O|KanaInfo.HalfwidthKatakana, KanaInfo.YaRow|KanaInfo.A|KanaInfo.Small|KanaInfo.HalfwidthKatakana,
      KanaInfo.YaRow|KanaInfo.U|KanaInfo.Small|KanaInfo.HalfwidthKatakana, KanaInfo.YaRow|KanaInfo.O|KanaInfo.Small|KanaInfo.HalfwidthKatakana,

      KanaInfo.RaRow|KanaInfo.A|KanaInfo.HalfwidthKatakana, KanaInfo.RaRow|KanaInfo.I|KanaInfo.HalfwidthKatakana,
      KanaInfo.RaRow|KanaInfo.U|KanaInfo.HalfwidthKatakana, KanaInfo.RaRow|KanaInfo.E|KanaInfo.HalfwidthKatakana,
      KanaInfo.RaRow|KanaInfo.O|KanaInfo.HalfwidthKatakana,
      
      KanaInfo.WaRow|KanaInfo.O|KanaInfo.HalfwidthKatakana, KanaInfo.NRow|KanaInfo.N|KanaInfo.HalfwidthKatakana,

      // circled katakana
      KanaInfo.ARow|KanaInfo.A|KanaInfo.CircledKatakana, KanaInfo.ARow|KanaInfo.I|KanaInfo.CircledKatakana,
      KanaInfo.ARow|KanaInfo.U|KanaInfo.CircledKatakana, KanaInfo.ARow|KanaInfo.E|KanaInfo.CircledKatakana,
      KanaInfo.ARow|KanaInfo.O|KanaInfo.CircledKatakana,

      KanaInfo.KaRow|KanaInfo.A|KanaInfo.CircledKatakana, KanaInfo.KaRow|KanaInfo.I|KanaInfo.CircledKatakana,
      KanaInfo.KaRow|KanaInfo.U|KanaInfo.CircledKatakana, KanaInfo.KaRow|KanaInfo.E|KanaInfo.CircledKatakana,
      KanaInfo.KaRow|KanaInfo.O|KanaInfo.CircledKatakana,

      KanaInfo.SaRow|KanaInfo.A|KanaInfo.CircledKatakana, KanaInfo.SaRow|KanaInfo.I|KanaInfo.CircledKatakana,
      KanaInfo.SaRow|KanaInfo.U|KanaInfo.CircledKatakana, KanaInfo.SaRow|KanaInfo.E|KanaInfo.CircledKatakana,
      KanaInfo.SaRow|KanaInfo.O|KanaInfo.CircledKatakana,

      KanaInfo.TaRow|KanaInfo.A|KanaInfo.CircledKatakana, KanaInfo.TaRow|KanaInfo.I|KanaInfo.CircledKatakana,
      KanaInfo.TaRow|KanaInfo.U|KanaInfo.CircledKatakana, KanaInfo.TaRow|KanaInfo.E|KanaInfo.CircledKatakana,
      KanaInfo.TaRow|KanaInfo.O|KanaInfo.CircledKatakana,

      KanaInfo.NaRow|KanaInfo.A|KanaInfo.CircledKatakana, KanaInfo.NaRow|KanaInfo.I|KanaInfo.CircledKatakana,
      KanaInfo.NaRow|KanaInfo.U|KanaInfo.CircledKatakana, KanaInfo.NaRow|KanaInfo.E|KanaInfo.CircledKatakana,
      KanaInfo.NaRow|KanaInfo.O|KanaInfo.CircledKatakana,

      KanaInfo.HaRow|KanaInfo.A|KanaInfo.CircledKatakana, KanaInfo.HaRow|KanaInfo.I|KanaInfo.CircledKatakana,
      KanaInfo.HaRow|KanaInfo.U|KanaInfo.CircledKatakana, KanaInfo.HaRow|KanaInfo.E|KanaInfo.CircledKatakana,
      KanaInfo.HaRow|KanaInfo.O|KanaInfo.CircledKatakana,

      KanaInfo.MaRow|KanaInfo.A|KanaInfo.CircledKatakana, KanaInfo.MaRow|KanaInfo.I|KanaInfo.CircledKatakana,
      KanaInfo.MaRow|KanaInfo.U|KanaInfo.CircledKatakana, KanaInfo.MaRow|KanaInfo.E|KanaInfo.CircledKatakana,
      KanaInfo.MaRow|KanaInfo.O|KanaInfo.CircledKatakana,

      KanaInfo.YaRow|KanaInfo.A|KanaInfo.CircledKatakana, KanaInfo.YaRow|KanaInfo.U|KanaInfo.CircledKatakana,
      KanaInfo.YaRow|KanaInfo.O|KanaInfo.CircledKatakana,

      KanaInfo.RaRow|KanaInfo.A|KanaInfo.CircledKatakana, KanaInfo.RaRow|KanaInfo.I|KanaInfo.CircledKatakana,
      KanaInfo.RaRow|KanaInfo.U|KanaInfo.CircledKatakana, KanaInfo.RaRow|KanaInfo.E|KanaInfo.CircledKatakana,
      KanaInfo.RaRow|KanaInfo.O|KanaInfo.CircledKatakana,
      
      KanaInfo.WaRow|KanaInfo.A|KanaInfo.CircledKatakana, KanaInfo.WaRow|KanaInfo.I|KanaInfo.CircledKatakana,
      KanaInfo.WaRow|KanaInfo.E|KanaInfo.CircledKatakana, KanaInfo.WaRow|KanaInfo.O|KanaInfo.CircledKatakana,
    };
    #endregion
    
    Debug.Assert(kana.Length == info.Length);
    
    kanaInfoMap = new Dictionary<char,KanaInfo>(kana.Length);
    for(int i=0; i<kana.Length; i++)
    {
      kanaInfoMap.Add(kana[i], info[i]);
    }
  }

  static Dictionary<char,char> hiraToKataMap;
  static Dictionary<char,char> doubleToSingleMap;
  static Dictionary<char,KanaInfo> kanaInfoMap;
  static Dictionary<string,string> romajiKanaMap;

  static readonly Regex romajiRE = new Regex(
    @"(([bp]{1,2}|[hjmnrvz])y? | cc?[hy]? | dd?[hwy]? | ([gq]{1,2}|f)[wy]? | ss?[hwy]? | tt?[hswy]? | wh? | kk?y? | y)?[aeiou] |
      nn? | [lx](y?[aeiou]|k[ae]|tu|wa) | k?kwa | xn | ~ | -",
    RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace |
    RegexOptions.Singleline | RegexOptions.ExplicitCapture);

  static readonly Regex whitespaceRE = new Regex(@"\s+", RegexOptions.CultureInvariant|RegexOptions.Singleline);
}

} // namespace Jappy.Backend
