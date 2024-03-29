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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Jappy.Backend
{

#region JapaneseSearchStrategy
public class JapaneseSearchStrategy : DefaultSearchStrategy
{
  public static new readonly JapaneseSearchStrategy Instance = new JapaneseSearchStrategy();

  protected override Regex SplitRegex
  {
    get { return splitRE; }
  }

  protected override void PreprocessSearchPiece(ref SearchPiece piece)
  {
    if(piece.Text.Length != 0 && piece.Text[0] == '@') // queries beginning with '@' are assumed to be roumaji.
    {                                                  // convert them to kana.
      piece.Text = piece.Text.Substring(1);
      base.PreprocessSearchPiece(ref piece);
      piece.Flags &= ~SearchFlag.SearchMeanings; // kana shouldn't be found in the meanings, so don't search there
      piece.Text = JP.ConvertRomajiToKana(piece.Text);
    }
    else
    {
      base.PreprocessSearchPiece(ref piece);
    }
  }

  static readonly Regex splitRE = new Regex(@"-?(?:""[^""]+""|@?\*?\S+\*?)",
                                            RegexOptions.CultureInvariant | RegexOptions.Singleline);
}
#endregion

#region JapaneseDictionary
public sealed class JapaneseDictionary : WordDictionary
{
  static JapaneseDictionary()
  {
    InitializeCharacterNormalizationMap();
    InitializeJMDictMaps();
  }
  
  public void ImportJMDict(Stream stream)
  {
    ImportJMDict(new StreamReader(stream, Encoding.UTF8));
  }

  public void ImportJMDict(TextReader xml)
  {
    Unload();
    ImportJMDict(new XmlTextReader(xml));
  }

  public sealed class JpDiskHashIndex : DiskHashIndex
  {
    protected override string NormalizeKey(string key)
    {
      return NormalizeString(key);
    }
  }

  public sealed class JpTrieIndex : TrieIndex
  {
    protected override string NormalizeKey(string key)
    {
      return NormalizeString(key);
    }
  }

  public static string NormalizeString(string japaneseText)
  {
    if(japaneseText == null) return null;
    return JP.ConvertStringUsingCharacterMap(japaneseText.ToLowerInvariant(), charNormalizationMap);
  }

  public static int NormalizedComparison(string a, string b)
  {
    return string.CompareOrdinal(NormalizeString(a), NormalizeString(b));
  }

  protected override Index CreateHeadwordIndex()
  {
    return new JpDiskHashIndex();
  }

  protected override Index CreateReadingIndex()
  {
    return new JpTrieIndex();
  }

  protected override Index CreateMeaningIndex()
  {
    return new TrieIndex();
  }

  #region JMDict XML importing
  #region EntryBuilder
  sealed class EntryBuilder
  {
    public uint ID
    {
      get { return entry.ID; }
      set { entry.ID = value; }
    }

    public void AddMeaning(Meaning meaning, string headwordRestriction, string readingRestriction)
    {
      meaning.AppliesToHeadword = meaning.AppliesToReading = -1;
      AddArrayElement(ref entry.Meanings, meaning);

      if(headwordRestriction != null)
      {
        senseHeadwordRestrictions.Add(new Restriction(headwordRestriction, entry.Meanings.Length-1));
      }

      if(readingRestriction != null)
      {
        senseReadingRestrictions.Add(new Restriction(readingRestriction, entry.Meanings.Length-1));
      }
    }

    public void AddReading(Word reading, string restriction)
    {
      if(restriction != null)
      {
        readingRestrictions.Add(new Restriction(restriction, readings.Count));
      }

      reading.AppliesToHeadword = -1;
      readings.Add(reading);
    }

    public void AddHeadword(Word headword)
    {
      headword.AppliesToHeadword = -1;
      headwords.Add(headword);
    }

    public void AddResultFlag(ResultFlag flag)
    {
      extraFlags |= flag;
    }

    public Entry CreateEntryAndReset()
    {
      if(headwords.Count != 0)
      {
        entry.Headwords = headwords.ToArray();
        headwords.Clear();
      }

      if(readings.Count != 0)
      {
        entry.Readings = readings.ToArray();
        readings.Clear();
      }

      if(entry.Headwords == null)
      {
        entry.Headwords = entry.Readings;
        entry.Readings  = null;
      }
      else if(readingRestrictions.Count != 0)
      {
        foreach(Restriction res in readingRestrictions)
        {
          entry.Readings[res.Index].AppliesToHeadword = FindHeadword(res.Word);
        }
        readingRestrictions.Clear();
      }

      foreach(Restriction res in senseHeadwordRestrictions)
      {
        entry.Meanings[res.Index].AppliesToHeadword = FindHeadword(res.Word);
      }
      senseHeadwordRestrictions.Clear();

      foreach(Restriction res in senseReadingRestrictions)
      {
        entry.Meanings[res.Index].AppliesToReading = FindReading(res.Word);
      }
      senseReadingRestrictions.Clear();

      entry.Headwords[0].Flags |= extraFlags;
      extraFlags = 0;

      Entry result = this.entry;
      this.entry = new Entry();
      return result;
    }

    sbyte FindHeadword(string word)
    {
      return FindWord(word, entry.Headwords);
    }

    sbyte FindReading(string word)
    {
      return FindWord(word, entry.Readings);
    }

    static sbyte FindWord(string word, Word[] words)
    {
      if(words != null)
      {
        for(int i=0; i<words.Length; i++)
        {
          if(string.Equals(words[i].Text, word, StringComparison.InvariantCultureIgnoreCase))
          {
            return (sbyte)i;
          }
        }
      }
      return -1;
    }

    struct Restriction
    {
      public Restriction(string word, int index)
      {
        this.Word  = word;
        this.Index = index;
      }

      public string Word;
      public int Index;
    }

    Entry entry;

    List<Word> headwords = new List<Word>(), readings = new List<Word>();
    List<Restriction> readingRestrictions = new List<Restriction>(),
                      senseHeadwordRestrictions = new List<Restriction>(),
                      senseReadingRestrictions = new List<Restriction>();
    ResultFlag extraFlags;
  }
  #endregion

  void ImportJMDict(XmlTextReader reader)
  {
    reader.WhitespaceHandling = WhitespaceHandling.Significant;
    if(!reader.ReadToDescendant("entry")) return;

    List<Entry> entries = new List<Entry>();
    EntryBuilder builder = new EntryBuilder();
    do
    {
      foreach(XmlTextReader child in EnumerateChildNodes(reader))
      {
        ImportJMDictEntryData(child, builder);
      }

      Entry entry = builder.CreateEntryAndReset();
      entries.Add(entry);
    } while(reader.NodeType == XmlNodeType.Element && reader.LocalName == "entry");

    SortEntriesByFrequency(entries);
    ImportEntries(entries);
  }

  static void ImportJMDictEntryData(XmlTextReader reader, EntryBuilder entry)
  {
    switch(reader.LocalName)
    {
      case "ent_seq":
        entry.ID = Convert.ToUInt32(reader.ReadElementContentAsLong());
        break;

      case "r_ele": case "k_ele":
      {
        Word word = new Word();
        char type = reader.LocalName[0];
        string restriction = null;
        int frequency = 0;
        byte news=0, ichi=0, spec=0, gai=0;

        foreach(XmlTextReader child in EnumerateChildNodes(reader))
        {
          switch(child.LocalName.Substring(1))
          {
            case "eb":
              word.Text = child.ReadElementContentAsString();
              break;

            case "e_restr":
              restriction = child.ReadElementContentAsString();
              break;

            case "e_inf": case "e_pri":
              child.ReadStartElement();
              if(child.NodeType == XmlNodeType.EntityReference)
              {
                word.Flags |= resultFlagMap[child.LocalName];
                child.Skip();
              }
              else if(child.NodeType == XmlNodeType.Text)
              {
                string content = child.ReadContentAsString();
                if(content.StartsWith("nf"))
                {
                  frequency = byte.Parse(content.Substring(2)); // frequency bracket of 500 words per bracket
                  frequency = (frequency-1) * 500 + 250; // place it in the center of the bracket
                }
                else
                {
                  switch(content)
                  {
                    case "news1": news = 1; break; // word is in the top 12,000 most common in Alexandre Girardi's wordfreq file
                    case "news2": news = 2; break; // word is in the second 12,000 most common in Alexandre Girardi's wordfreq file
                    case "ichi1": ichi = 1; break; // word is in the 10,000 most common words in the "Ichimango goi bunruishuu"
                    case "ichi2": ichi = 2; break; // word was in the "Ichimango goi bunruishuu", but was demoted
                    case "spec1": spec = 1; break; // word is not in official lists, but is detected as being common
                    case "spec2": spec = 2; break; // word is not in official lists, but is detected as being moderately common
                    case "gai1":  gai  = 1; break; // word is a common loanword
                    case "gai2":  gai  = 2; break; // word is a moderately common loanword
                    default: throw new NotImplementedException("Frequency: "+content);
                  }
                }
              }
              else
              {
                throw new NotImplementedException();
              }
              reader.ReadEndElement();
              break;

            default:
              child.Skip();
              break;
          }
        }

        // now estimate the word's frequency if the frequency marker (nf??) wasn't given
        if(frequency == 0)
        {
          // first get the initial estimate
          if(ichi == 1) { frequency = 5000; ichi = 0; }
          else if(ichi == 2) { frequency = 15000; ichi = 0; }
          else if(news == 1) { frequency = 6000; news = 0; } // place it in the center of these files
          else if(news == 2) { frequency = 18000; news = 0; }
          else if(gai  == 2) { frequency = 8000; gai = 0; }
          else if(gai  == 1) { frequency = 24000; gai = 0; }
          else if(spec == 1) { frequency = 7000; spec = 0; }
          else if(spec == 2) { frequency = 21000; spec = 0; }

          // then modify it based on the other flags that may be present
          if(news == 1) frequency -= 1000;
          else if(news == 2) frequency -= 250;

          if(gai == 1) frequency -= 750;
          else if(gai == 2) frequency -= 200;
          
          if(spec == 1) frequency -= 500;
          else if(spec == 2) frequency -= 125;
        }
        else // otherwise, if the frequency marker was given, reduce it if any of the ichi/news/gai/spec flags are set
        {
          int max = int.MaxValue;
          if(ichi == 1) max = 20;
          else if(news == 1) max = 22;
          else if(gai  == 1) max = 26;
          else if(spec == 1) max = 24;
          else if(ichi == 2) max = 40;
          else if(news == 2) max = 44;
          else if(gai  == 2) max = 52;
          else if(spec == 2) max = 48;
          
          if(frequency > max) frequency = (byte)max;
        }

        word.Frequency = (byte)(frequency == 0 ? 0 : frequency/500+1); // figure out which 500-word block it's in

        if(type == 'r') entry.AddReading(word, restriction);
        else entry.AddHeadword(word);
        break;
      }

      case "info":
        reader.Skip();
        break;

      case "sense":
      {
        string senseHeadword=null, senseReading=null;
        Meaning meaning = new Meaning();
        List<Gloss> glosses = new List<Gloss>();

        foreach(XmlTextReader child in EnumerateChildNodes(reader))
        {
          switch(child.LocalName)
          {
            case "stagk":
              senseHeadword = child.ReadElementContentAsString();
              break;
            case "stagr":
              senseReading = child.ReadElementContentAsString();
              break;

            case "ant": case "xref":
              AddArrayElement(ref meaning.Related,
                new RelatedWord(child.ReadElementContentAsString(),
                                child.LocalName == "ant" ? Relation.Antonym : Relation.Synonym));
              break;

            case "pos": case "field": case "misc":
              child.ReadStartElement();
              while(child.NodeType == XmlNodeType.EntityReference)
              {
                AddArrayElement(ref meaning.Flags, senseFlagMap[child.LocalName]);
                child.Skip();
              }
              child.ReadEndElement();
              break;

            case "gloss":
            {
              Gloss gloss = new Gloss();
              child.ReadStartElement();
              if(child.NodeType == XmlNodeType.Text)
              {
                gloss.Text = child.ReadContentAsString();
              }
              else if(child.NodeType == XmlNodeType.Element && child.LocalName == "pri")
              {
                gloss.Text      = child.ReadElementContentAsString();
                gloss.GoodMatch = true;
              }
              else throw new NotImplementedException();
              child.ReadEndElement();

              glosses.Add(gloss);
              break;
            }

            default:
              child.Skip();
              break;
          }
        }

        meaning.Glosses = glosses.ToArray();
        entry.AddMeaning(meaning, senseHeadword, senseReading);
        break;
      }

      case "trans": // this only occurs in the JM name dictionary
        foreach(XmlTextReader child in EnumerateChildNodes(reader))
        {
          if(child.LocalName == "name_type")
          {
            child.ReadStartElement();
            if(reader.NodeType == XmlNodeType.EntityReference)
            {
              entry.AddResultFlag(resultFlagMap[reader.LocalName]);
              reader.Skip();
            }
            else throw new NotImplementedException();
            child.ReadEndElement();
          }
          else if(child.LocalName == "trans_det")
          {
            string meaningText = child.ReadElementContentAsString();
            if(!string.IsNullOrEmpty(meaningText))
            {
              Meaning meaning = new Meaning();
              meaning.Glosses = new Gloss[1];
              meaning.Glosses[0].Text = meaningText;
              entry.AddMeaning(meaning, null, null);
            }
          }
          else
          {
            child.Skip();
          }
        }
        break;
      
      default:
        throw new NotImplementedException("Unhandled node type: "+reader.LocalName);
    }
  }

  static void ParseJMDictWordFlag(XmlTextReader reader, ref Word word, ref int frequencyEstimate)
  {
    reader.ReadStartElement();
    if(reader.NodeType == XmlNodeType.EntityReference)
    {
      word.Flags |= resultFlagMap[reader.LocalName];
      reader.Skip();
    }
    else if(reader.NodeType == XmlNodeType.Text)
    {
      string content = reader.ReadContentAsString();
      if(content.StartsWith("nf"))
      {
        word.Frequency = byte.Parse(content.Substring(2));
      }
      else
      {
        word.Flags |= resultFlagMap[content];
      }
    }
    else throw new NotImplementedException();
    reader.ReadEndElement();
  }

  static int AddArrayElement<T>(ref T[] array)
  {
    if(array == null)
    {
      array = new T[1];
      return 0;
    }
    else
    {
      T[] newArray = new T[array.Length+1];
      Array.Copy(array, newArray, array.Length);
      array = newArray;
      return array.Length-1;
    }
  }

  static void AddArrayElement<T>(ref T[] array, T element)
  {
    int index = AddArrayElement(ref array);
    array[index] = element;
  }

  static IEnumerable<XmlReader> EnumerateChildNodes(XmlTextReader reader)
  {
    reader.ReadStartElement();
    while(reader.NodeType == XmlNodeType.Element || reader.NodeType == XmlNodeType.Comment)
    {
      if(reader.NodeType == XmlNodeType.Comment)
      {
        reader.Skip();
        continue;
      }

      yield return reader;
    }
    reader.ReadEndElement();
  }

  static void InitializeJMDictMaps()
  {
    string[] resultStrings = new string[]
    {
      "io", "iK", "ik", "oK", "ok", "ateji", "gikun",
      "surname", "place", "unclass", "company", "product", "person", "given", "fem", "masc",
    };

    ResultFlag[] resultFlags = new ResultFlag[]
    {
      ResultFlag.IrregularOkurigana, ResultFlag.IrregularKanji, ResultFlag.IrregularKana,
      ResultFlag.OldKanji, ResultFlag.OldKana, ResultFlag.Ateji, ResultFlag.Gikun,

      ResultFlag.Surname, ResultFlag.PlaceName, ResultFlag.UnclassifiedName, ResultFlag.CompanyName,
      ResultFlag.ProductName, ResultFlag.FullName, ResultFlag.GivenName, ResultFlag.FeminineName,
      ResultFlag.MasculineName,
    };

    Debug.Assert(resultStrings.Length == resultFlags.Length);
    for(int i=0; i<resultStrings.Length; i++)
    {
      resultFlagMap.Add(resultStrings[i], resultFlags[i]);
    }

    string[] senseEntities = new string[]
    {
      "MA",    "X",     "abbr",  "adj",      "adj-na",  "adj-no", "adj-pn",  "adj-t", "adv",  "adv-n", "adv-to",
      "arch",  "aux",   "aux-v", "aux-adj",  "Buddh",   "chn",    "col",     "comp",  "conj", "derog",
      "ek",    "exp",   "fam",   "fem",      "food",    "geom",   "gram",    "hon",   "hum",   
      "id",    "int",   "iv",    "ling",     "m-sl",    "male",   "male-sl", "math",  "mil",
      "n",     "n-adv", "n-suf", "n-pref",   "n-t",     "neg",    "neg-v",   "num",   "obs",  "obsc", 
      "pol",   "pref",  "prt",    "physics", "qv",      "rare",   "sens",    "sl",    "suf",  "uK",
      "uk",    "v1",    "v5",    "v5aru",    "v5b",     "v5g",    "v5k",     "v5k-s", "v5m",  "v5n",   "v5r",
      "v5r-i", "v5s",   "v5t",   "v5u",      "v5u-s",   "v5uru",  "vi",      "vk",    "vs",   "vs-s",  "vs-i",
      "vz",    "vt",    "vulg",  "mg",       "fg",      "ng",
    };

    SenseFlag[] senseFlags = new SenseFlag[]
    {
      SenseFlag.MartialArts, SenseFlag.XRated, SenseFlag.Abbreviation, SenseFlag.Adj, SenseFlag.AdjNa, SenseFlag.AdjNo,
      SenseFlag.AdjPn, SenseFlag.AdjTaru, SenseFlag.Adv, SenseFlag.AdvN, SenseFlag.AdvTo,

      SenseFlag.Archaic, SenseFlag.Auxiliary, SenseFlag.AuxiliaryVerb, SenseFlag.AuxiliaryAdj, SenseFlag.Buddhist,
      SenseFlag.ChildSpeak, SenseFlag.Colloquialism, SenseFlag.Computer, SenseFlag.Conjunction, SenseFlag.Derogatory,
      
      SenseFlag.AlwaysKanji, SenseFlag.Expression, SenseFlag.Familiar, SenseFlag.Female, SenseFlag.Food,
      SenseFlag.Geometry, SenseFlag.Grammatical, SenseFlag.Honorific, SenseFlag.Humble,
      
      SenseFlag.Idiom, SenseFlag.Interjection, SenseFlag.IrregularVerb, SenseFlag.Linguistics, SenseFlag.MangaSlang,
      SenseFlag.Male, SenseFlag.MaleSlang, SenseFlag.Math, SenseFlag.Military,
      
      SenseFlag.Noun, SenseFlag.NounAdv, SenseFlag.NounSuffix, SenseFlag.NounPrefix, SenseFlag.NounTemporal,
      SenseFlag.Negative, SenseFlag.NegativeVerb, SenseFlag.Numeric, SenseFlag.Obsolete, SenseFlag.Obscure,
      
      SenseFlag.Polite, SenseFlag.Prefix, SenseFlag.Particle, SenseFlag.Physics, SenseFlag.QuodVide,
      SenseFlag.Rare, SenseFlag.Sensitive, SenseFlag.Slang, SenseFlag.Suffix, SenseFlag.UsuallyKanji,
      
      SenseFlag.UsuallyKana, SenseFlag.Verb1, SenseFlag.Verb5, SenseFlag.Verb5Aru, SenseFlag.Verb5bu,
      SenseFlag.Verb5gu, SenseFlag.Verb5ku, SenseFlag.Verb5kuSpecial, SenseFlag.Verb5mu, SenseFlag.Verb5nu,
      SenseFlag.Verb5ru,
      
      SenseFlag.Verb5ruIrregular, SenseFlag.Verb5su, SenseFlag.Verb5tu, SenseFlag.Verb5u, SenseFlag.Verb5uSpecial,
      SenseFlag.Verb5uru, SenseFlag.Intransitive, SenseFlag.VerbKuruSpecial, SenseFlag.VerbSuru, SenseFlag.VerbSuruSpecial,
      SenseFlag.VerbSuruIrregular,

      SenseFlag.VerbZuru, SenseFlag.Transitive, SenseFlag.Vulgar, SenseFlag.MasculineGender, SenseFlag.FeminineGender,
      SenseFlag.NeutralGender,
    };

    Debug.Assert(senseEntities.Length == senseFlags.Length);
    for(int i=0; i<senseEntities.Length; i++)
    {
      senseFlagMap.Add(senseEntities[i], senseFlags[i]);
    }
  }

  static Dictionary<string, SenseFlag> senseFlagMap = new Dictionary<string, SenseFlag>();
  static Dictionary<string, ResultFlag> resultFlagMap = new Dictionary<string, ResultFlag>();
  #endregion

  static void InitializeCharacterNormalizationMap()
  {
    // builds a map to convert half-width katakana to full-width katakana
    const string wrongChars = "｡｢｣､･ｦｧｨｩｪｫｬｭｮｯｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝﾞﾟ ‘’・”、。「」ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔ？！～＠＃＄％＾＆＊（）１２３４５６７８９０－＝＿＋｛｝＜＞；：／ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ";
    const string rightChars = ".\"\", ヲァィゥェォャュョッーアイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワン゛゜　'' \",.\"\"ァアィイゥウェエォオカガキギクグケゲコゴサザシジスズセゼソゾタダチヂッツヅテデトドナニヌネノハバパヒビピフブプヘベペホボポマミムメモャヤュユョヨラリルレロヮワヰヱヲンヴ?!~@#$%^&*()1234567890-=_+{}<>;:/abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    Debug.Assert(wrongChars.Length == rightChars.Length);
    for(int i=0; i<wrongChars.Length; i++)
    {
      charNormalizationMap.Add(wrongChars[i], rightChars[i]);
    }
  }

  static readonly Dictionary<char,char> charNormalizationMap = new Dictionary<char,char>();
}
#endregion

} // namespace Jappy.Backend
