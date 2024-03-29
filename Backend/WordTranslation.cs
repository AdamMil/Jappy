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
using System.Text.RegularExpressions;

namespace Jappy.Backend
{

[Flags]
public enum InflectionType
{
  TypeMask=15, None=0,

  Unknown=1, Conjunctive=2, Te=3, ConditionalEba=4, ConditionalTara=5, Tari=6, Imperative=7, Volitional=8,
  Causative=9, PassiveOrPotential=10, CausativePassive=11, Adverbial=12, NegativeForm=13, SeemsLike=14,
  ImperativeOrPotential=15,

  Negative=16, Past=32, Plain=64, Polite=128,
}

public struct TranslatedWordEntry
{
  public TranslatedWordEntry(uint id)
  {
    EntryId    = id;
    Inflection = InflectionType.None;
  }
  
  public TranslatedWordEntry(uint id, InflectionType inflection)
  {
    EntryId    = id;
    Inflection = inflection;
  }

  public uint EntryId;
  public InflectionType Inflection;
}

public struct TranslatedWord
{
  public bool PossiblyInflected
  {
    get
    {
      foreach(TranslatedWordEntry entry in Entries)
      {
        if(entry.Inflection != InflectionType.None) return true;
      }
      return false;
    }
  }

  public WordDictionary Dictionary;  
  public TranslatedWordEntry[] Entries;
  public int Position, Length;
}

public class WordTranslator
{
  static WordTranslator()
  {
    // create a dictionary mapping certain kana words to their exact headwords. this is used to force acceptance for
    // these particular kana words even when the would not normally be accepted, due to it not passing the
    // SearchEntryIsOkay() check
    allowedKanaWords = new Dictionary<string,string>();
    allowedKanaWords["こと"] = "事";
    allowedKanaWords["こんにちは"] = "今日は";
    allowedKanaWords["とき"]   = "時";
    allowedKanaWords["けど"]   = "けど";
    allowedKanaWords["のに"]   = "のに";
    allowedKanaWords["なら"]   = "なら";
    allowedKanaWords["という"] = "と言う";
    allowedKanaWords["すると"] = "すると";
    allowedKanaWords["あれほど"] = "彼程";
    allowedKanaWords["すばらしい"] = "素晴らしい";
    allowedKanaWords["とりあえず"] = "取り敢えず";

    // create a list of words that we won't consider as possibly inflected.
    ignoreInflected = new List<string>();
    ignoreInflected.Add("よく"); // yoku is often used like "yoku wakaranai". we don't want よく -> 良い
    ignoreInflected.Add("いう"); // we don't want いう -> いる
    ignoreInflected.Add("なら"); // we don't want なら -> なる
    ignoreInflected.Add("なり"); // we don't want なり -> なる
    ignoreInflected.Add("しあわ"); // we don't want しあわせ -> しあう
    ignoreInflected.Add("しあわせ"); // we don't want しあわせ -> しあう
    ignoreInflected.Sort();
  }

  public int MinimumHiraganaLength
  {
    get { return minHiraganaLength; }
  }

  public int MinimumKatakanaLength
  {
    get { return minKatakanaLength; }
  }

  public readonly List<WordDictionary> WordDictionaries = new List<WordDictionary>();
  public readonly List<WordDictionary> NameDictionaries = new List<WordDictionary>();

  public TranslatedWord[] TranslateWordsInJapaneseText(string japaneseText)
  {
    List<TranslatedWord> words = new List<TranslatedWord>();

    foreach(Region region in SplitIntoRegions(japaneseText))
    {
      string text = japaneseText.Substring(region.Position, region.Length); // get the region text

      // now find words within the text. start by searching the entire region, and progressively shrink the window
      int index = 0;
      while(index < text.Length)
      {
        TranslatedWord word = new TranslatedWord();

        string block = text.Substring(index, text.Length-index);
        while(block.Length != 0 && !SearchFor(block, ref word))
        {
          block = block.Substring(0, block.Length-1);
          
          if(block.Length == 1 && JP.IsHiragana(block[0])) // if the block has been reduced to a single hiragana,
          {                                                // don't bother looking it up
            block = string.Empty;
          }
        }

        if(block.Length == 0) // if the search was unsucessful, ignore the first character and try again
        {
          index++;
        }
        else// otherwise, we have a match
        {
          word.Position = region.Position + index;
          word.Length   = block.Length;
          words.Add(word);
          index += word.Length;
        }
      }
    }
    
    return words.ToArray();
  }

  struct Region
  {
    public int Position, Length;
  }

  enum WordType
  {
    Invalid, HasKanji, MixedKana, AllHiragana, AllKatakana
  }
  
  struct InflectionRoot
  {
    public InflectionRoot(InflectionType type, SenseFlag pos, string dictionaryForm)
    {
      Inflection     = type;
      PartOfSpeech   = pos;
      DictionaryForm = dictionaryForm;
    }

    public string DictionaryForm;
    public InflectionType Inflection;
    public SenseFlag PartOfSpeech;
  }


  bool SearchEntryIsOkay(ref Entry entry, string queryWord, WordType queryType)
  {
    bool entryIsOkay = false;

    string explicitlyAllowed;
    allowedKanaWords.TryGetValue(queryWord, out explicitlyAllowed);

    foreach(Word headword in entry.Headwords)
    {
      // if this headword is explicitly allowed, use it
      if(explicitlyAllowed != null && string.Equals(headword.Text, explicitlyAllowed, StringComparison.Ordinal))
      {
        entryIsOkay = true;
        break;
      }

      WordType headwordType = ClassifyWord(headword.Text);
      if(headwordType == WordType.HasKanji)
      {
        if(entry.Meanings == null) continue;

        foreach(Meaning meaning in entry.Meanings)
        {
          if(!meaning.HasFlag(SenseFlag.UsuallyKana)) continue; // only accept words usually written in kana
          if(meaning.HasFlag(SenseFlag.Particle)) continue; // don't accept particles

          if(queryType == WordType.AllKatakana && !JP.IsKatakana(headword.Text[0]))
          {
            continue; // reject words if the query started with katakana but the headword didn't
          }

          if(meaning.AppliesToReading != -1 && // reject words that apply to a specific headword and the headword it
             (entry.Readings == null ||        // applies to does not match the query word
              JapaneseDictionary.NormalizedComparison(
                entry.Readings[meaning.AppliesToReading].Text, queryWord) != 0))
          {
            continue;
          }

          entryIsOkay = true;
          break;
        }
      }
      // otherwise if the headword is the same type of kana, only accept it if the word is long,
      // is katakana and >= 1 char, or is very popular. words with differing kana types are rejected.
      else if(headwordType == queryType &&
              (queryType == WordType.AllHiragana && queryWord.Length >= MinimumHiraganaLength ||
               queryType == WordType.AllKatakana && queryWord.Length >= MinimumKatakanaLength || 
               entry.Frequency != 0 && entry.Frequency < 10))
      {
        entryIsOkay = true;
      }

      if(entryIsOkay) break;
    }

    return entryIsOkay;
  }

  bool SearchFor(string query, ref TranslatedWord word)
  {
    query = query.Replace("・", ""); // strip ・ characters out of blocks of katakana

    WordType type = ClassifyWord(query);
    if(type == WordType.MixedKana) return false; // assume words with mixed kana types won't match

    SearchFlag flags = SearchFlag.ExactMatch | SearchFlag.SearchHeadwords;
    if(type != WordType.HasKanji) flags |= SearchFlag.SearchReadings; // if the word is all kana, also search readings

    foreach(WordDictionary dictionary in WordDictionaries)
    {
      // if it matched exactly in a word dictionary, we win.
      if(SearchFor(dictionary, query, type, flags, ref word)) return true;

      // it didn't match exactly in the word dictionary, but it might be a conjugated verb or adjective.
      if(ignoreInflected.BinarySearch(query) < 0) // don't consider words that are explicitly ignored
      {
        List<InflectionRoot> inflections = GetPossibleInflections(query);
        if(inflections != null) // if it's possibly an inflected word...
        {
          List<TranslatedWordEntry> entries = new List<TranslatedWordEntry>();
          foreach(InflectionRoot root in inflections)
          {
            SearchForInflection(dictionary, root, entries);
          }

          if(entries.Count != 0)
          {
            word.Dictionary = dictionary;
            word.Entries    = entries.ToArray();
            return true;
          }
        }
      }
    }

    // it wasn't in the word dictionaries, so try the name dictionaries
    foreach(WordDictionary dictionary in NameDictionaries)
    {
      if(SearchFor(dictionary, query, type, flags, ref word)) return true;
    }

    return false;
  }

  unsafe bool SearchFor(WordDictionary dictionary, string query, WordType type, SearchFlag flags,
                        ref TranslatedWord word)
  {
    const int maxEntries = 8;
    TranslatedWordEntry* entryList = stackalloc TranslatedWordEntry[maxEntries];
    int numEntries = 0;

    foreach(uint id in JapaneseSearchStrategy.Instance.Search(dictionary, query, flags))
    {
      bool entryIsOkay = type == WordType.HasKanji;

      if(type != WordType.HasKanji) // if the search word was all kana, we have to be careful about it...
      {
        Entry entry = dictionary.GetEntryById(id);
        entryIsOkay = SearchEntryIsOkay(ref entry, query, type);
      }

      if(entryIsOkay)
      {
        entryList[numEntries++] = new TranslatedWordEntry(id);
        if(numEntries == maxEntries) break;
      }
    }

    if(numEntries == 0)
    {
      return false;
    }
    else
    {
      word.Dictionary = dictionary;
      word.Entries    = new TranslatedWordEntry[numEntries];
      for(int i=0; i<word.Entries.Length; i++)
      {
        word.Entries[i] = entryList[i];
      }
      return true;
    }
  }

  void SearchForInflection(WordDictionary dictionary, InflectionRoot root, List<TranslatedWordEntry> entries)
  {
    WordType type = ClassifyWord(root.DictionaryForm);

    SearchFlag flags = SearchFlag.ExactMatch | SearchFlag.SearchHeadwords;
    if(type != WordType.HasKanji) flags |= SearchFlag.SearchReadings;

    foreach(uint id in JapaneseSearchStrategy.Instance.Search(dictionary, root.DictionaryForm, flags))
    {
      if(root.PartOfSpeech != SenseFlag.Unknown) // if the part of speech for the inflection is known, only accept
      {                                          // entries with headwords having that part of speech
        Entry entry = dictionary.GetEntryById(id);
        if(entry.Meanings != null)
        {
          foreach(Meaning meaning in entry.Meanings)
          {
            if(meaning.HasFlag(root.PartOfSpeech)) // if it has the right part of speech, accept the word
            {
              if(type == WordType.HasKanji || SearchEntryIsOkay(ref entry, root.DictionaryForm, type))
              {
                entries.Add(new TranslatedWordEntry(id, root.Inflection));
              }
              break;
            }
          }
        }
      }
    }
  }

  int minHiraganaLength = 4, minKatakanaLength = 1;

  static string RemoveLast(string text, int charsToChop)
  {
    return text.Substring(0, text.Length - charsToChop);
  }

  static WordType ClassifyWord(string word)
  {
    bool containsHiragana=false, containsKatakana=false;
    for(int i=0; i<word.Length; i++)
    {
      if(JP.IsHiragana(word[i]))
      {
        containsHiragana = true;
      }
      else if(JP.IsKatakana(word[i]))
      {
        containsKatakana = true;
      }
      else // it must be kanji
      {
        return WordType.HasKanji;
      }
    }
    
    if(containsHiragana)
    {
      return containsKatakana ? WordType.MixedKana : WordType.AllHiragana;
    }
    else
    {
      return containsKatakana ? WordType.AllKatakana : WordType.Invalid;
    }
  }

  static void AddInflection(ref List<InflectionRoot> inflections, SenseFlag pos, InflectionType type,
                            string dictionaryForm)
  {
    if(inflections == null) inflections = new List<InflectionRoot>();

    foreach(InflectionRoot root in inflections) // prevent duplicate or useless entries
    {
      if(root.PartOfSpeech == pos && (root.Inflection == type || type == InflectionType.Unknown) &&
         string.Equals(dictionaryForm, root.DictionaryForm, StringComparison.Ordinal))
      {
        return;
      }
    }

    inflections.Add(new InflectionRoot(type, pos, dictionaryForm));
  }

  static void AddV5Inflection(ref List<InflectionRoot> inflections, char lastChar, InflectionType type, string stem)
  {
    switch(GetVoicedRow(lastChar))
    {
      case KanaInfo.BaRow: AddInflection(ref inflections, SenseFlag.Verb5bu, type, stem+"ぶ"); break;
      case KanaInfo.GaRow: AddInflection(ref inflections, SenseFlag.Verb5gu, type, stem+"ぐ"); break;
      case KanaInfo.KaRow:
        AddInflection(ref inflections, stem[0] == '行' ? SenseFlag.Verb5kuSpecial : SenseFlag.Verb5ku, type, stem+"く");
        break;
      case KanaInfo.MaRow: AddInflection(ref inflections, SenseFlag.Verb5mu, type, stem+"む"); break;
      case KanaInfo.NaRow: AddInflection(ref inflections, SenseFlag.Verb5nu, type, stem+"ぬ"); break;
      case KanaInfo.RaRow: AddInflection(ref inflections, SenseFlag.Verb5ru, type, stem+"る"); break;
      case KanaInfo.SaRow: AddInflection(ref inflections, SenseFlag.Verb5su, type, stem+"す"); break;
      case KanaInfo.TaRow: AddInflection(ref inflections, SenseFlag.Verb5tu, type, stem+"つ"); break;
      case KanaInfo.ARow: case KanaInfo.WaRow:
        AddInflection(ref inflections, SenseFlag.Verb5u, type, stem+"う");
        break;
    }
  }

  static List<InflectionRoot> GetPossibleInflections(string word)
  {
    List<InflectionRoot> inflections = null;

    if(word.Length < 2) return inflections; // inflected words have at least 2 characters

    char lastChar = word[word.Length-1];
    if(!JP.IsHiragana(lastChar)) return inflections; // inflected words end in hiragana

    for(int i=0; i<word.Length; i++) // inflected words contain no katakana
    {
      if(JP.IsKatakana(word[i])) return inflections;
    }

    char prevChar  = word[word.Length-2];
    string allBut1 = RemoveLast(word, 1);

    #region Inflected adjectives
    // first check for inflected adjectives, since it's relatively simple
    if(EndsWithAndHasMore(word, "くありませんでした"))
    {
      AddInflection(ref inflections, SenseFlag.Adj, InflectionType.Polite|InflectionType.Negative|InflectionType.Past,
                    RemoveLast(word, 9)+"い");
    }
    else if(EndsWithAndHasMore(word, "くありません"))
    {
      AddInflection(ref inflections, SenseFlag.Adj, InflectionType.Polite|InflectionType.Negative,
                    RemoveLast(word, 6)+"い");
    }
    else
    {
      InflectionRoot root = HandleNai(word);
      if(root.Inflection != InflectionType.None && root.DictionaryForm[root.DictionaryForm.Length-1] == 'く')
      {
        AddInflection(ref inflections, SenseFlag.Adj, root.Inflection, RemoveLast(root.DictionaryForm, 1)+"い");
      }
      else if(lastChar == 'く') // -く
      {
        AddInflection(ref inflections, SenseFlag.Adj, InflectionType.Adverbial, allBut1+"い");
      }
      else if(lastChar == 'て' && prevChar == 'く') // -くて
      {
        AddInflection(ref inflections, SenseFlag.Adj, InflectionType.Te, RemoveLast(word, 2)+"い");
      }
      else if(word.EndsWith("かったら")) // -かったら
      {
        AddInflection(ref inflections, SenseFlag.Adj, InflectionType.ConditionalTara, RemoveLast(word, 4)+"い");
      }
      else if(word.EndsWith("かった")) // -かった
      {
        AddInflection(ref inflections, SenseFlag.Adj, InflectionType.Past|InflectionType.Plain,
                      RemoveLast(word, 3)+"い");
      }
      else if(word.EndsWith("ければ"))
      {
        AddInflection(ref inflections, SenseFlag.Adj, InflectionType.ConditionalEba, RemoveLast(word, 3)+"い");
      }
      else if(EndsWithAndHasMore(word, "そう"))
      {
        string stem = RemoveLast(word, 2);
        if(stem == "よさ" || stem == "良さ")
        {
          AddInflection(ref inflections, SenseFlag.Adj, InflectionType.SeemsLike, "良い");
        }
        else if(stem == "なさ" || stem == "無さ")
        {
          AddInflection(ref inflections, SenseFlag.Adj, InflectionType.SeemsLike, "無い");
        }
        else
        {
          AddInflection(ref inflections, SenseFlag.Adj, InflectionType.SeemsLike, stem+"い");
        }
      }
    }
    #endregion

    #region Polite verbs (-masu)
    // then check for polite verbs (-masu)
    {
      InflectionType type = InflectionType.Polite;
      int masuLength = 0;

      if(EndsWithAndHasMore(word, "ませんでした"))
      {
        masuLength = 6;
        type |= InflectionType.Negative|InflectionType.Past;
      }
      else if(EndsWithAndHasMore(word, "ません"))
      {
        masuLength = 3;
        type |= InflectionType.Negative;
      }
      else if(EndsWithAndHasMore(word, "ました"))
      {
        masuLength = 3;
        type |= InflectionType.Past;
      }
      else if(EndsWithAndHasMore(word, "ましょう"))
      {
        masuLength = 4;
        type |= InflectionType.Volitional;
      }
      else if(EndsWithAndHasMore(word, "ます"))
      {
        masuLength = 2;
      }

      if(masuLength != 0)
      {
        word     = RemoveLast(word, masuLength);
        lastChar = word[word.Length-1];

        if(IsV1StemEnd(lastChar)) // polite v1 words
        {
          AddInflection(ref inflections, SenseFlag.Verb1, type, word+"る");
        }

        if(word.Length > 1)
        {
          // check for polite v5 words
          allBut1 = RemoveLast(word, 1);
          KanaInfo info = JP.GetKanaInfo(lastChar);
          if((info & KanaInfo.SoundMask) == KanaInfo.I) // hiragana い sound (ie, i, ki, si, etc)
          {
            AddV5Inflection(ref inflections, lastChar, type, allBut1);
          }
        }
        else if(word == "し" || word == "為") // polite suru
        {
          AddInflection(ref inflections, SenseFlag.VerbSuruIrregular, type, "為る");
        }
        else if(word == "来" || word == "き")
        {
          AddInflection(ref inflections, SenseFlag.VerbKuruSpecial, type, "来る");
        }
        
        return inflections;
      }
    }
    #endregion

    #region -た,-て,-たら,-たり
    // -た,-て,-たら,-たり
    if(lastChar == 'た' || lastChar == 'て' || prevChar == 'た' && (lastChar == 'ら' || lastChar == 'り'))
    {
      string pre = lastChar == 'た' || lastChar == 'て' ? allBut1 : RemoveLast(word, 2);
      InflectionType type = lastChar == 'た' ? InflectionType.Past|InflectionType.Plain :
                            lastChar == 'て' ? InflectionType.Te :
                            lastChar == 'ら' ? InflectionType.ConditionalTara :
                            InflectionType.Tari;

      if(pre.Length >= 1)
      {
        char lastPreChar = pre[pre.Length-1];
        if(pre == "し" || pre == "為") // suru... する -> した
        {
          AddInflection(ref inflections, SenseFlag.VerbSuruIrregular, type, "為る");
        }
        else if(pre == "来" || pre == "き") // kuru... 来る -> 来た
        {
          AddInflection(ref inflections, SenseFlag.VerbKuruSpecial, type, "来る");
        }
        else
        {
          if(IsV1StemEnd(lastPreChar)) // if it might be an inflected v1 verb... 食べる/見る -> 食べた/見た
          {
            AddInflection(ref inflections, SenseFlag.Verb1, type, pre+"る");
          }
          if(pre.Length >= 2)
          {
            if(lastPreChar == 'い') // inflected v5ku verb... 書く -> 書いた
            {
              AddInflection(ref inflections, SenseFlag.Verb5ku, type, RemoveLast(pre, 1)+"く");
            }
            else if(lastPreChar == 'し') // inflected v5su verb... 話す -> 話したら
            {
              AddInflection(ref inflections, SenseFlag.Verb5su, type, RemoveLast(pre, 1)+"す");
            }
            else if(lastPreChar == 'っ') // inflected v5tu, v5ru, or v5u verb, or 行く... 待つ/取る -> 待って/取った
            {
              pre = RemoveLast(pre, 1);
              if(pre == "行") // handle the special case of 行く -> 行った
              {
                AddInflection(ref inflections, SenseFlag.Verb5kuSpecial, type, "行く");
              }
              else
              {
                AddInflection(ref inflections, SenseFlag.Verb5ru, type, pre + "る");
                AddInflection(ref inflections, SenseFlag.Verb5tu, type, pre + "つ");
                AddInflection(ref inflections, SenseFlag.Verb5u, type, pre + "う");
                if(pre == "い" || pre == "ゆ") // it might be いく/ゆく (行く) -- いった/ゆった
                {
                  AddInflection(ref inflections, SenseFlag.Verb5kuSpecial, type, pre + "く");
                }
              }
            }
          }
        }
      }
      
      return inflections;
    }
    // -だ,-で,-だら,-だり
    else if(lastChar == 'だ' || lastChar == 'で' || prevChar == 'だ' && (lastChar == 'ら' || lastChar == 'り'))
    {
      string pre = lastChar == 'だ' || lastChar == 'で' ? allBut1 : RemoveLast(word, 2);
      InflectionType type = lastChar == 'だ' ? InflectionType.Past|InflectionType.Plain :
                            lastChar == 'で' ? InflectionType.Te :
                            lastChar == 'ら' ? InflectionType.ConditionalTara :
                            InflectionType.Tari;

      if(pre.Length > 1)
      {
        char lastPreChar = pre[pre.Length-1];
        if(lastPreChar == 'い') // inflected v5gu verb... 泳ぐ -> 泳いだ
        {
          AddInflection(ref inflections, SenseFlag.Verb5ku, type, RemoveLast(pre, 1)+"ぐ");
        }
        else if(lastPreChar == 'ん') // inflected v5nu, v5bu, or v5mu verb... 死ぬ/飛ぶ/読む -> 死んだ/飛んだ/読んだ
        {
          pre = RemoveLast(pre, 1);
          AddInflection(ref inflections, SenseFlag.Verb5nu, type, pre + "ぬ");
          AddInflection(ref inflections, SenseFlag.Verb5bu, type, pre + "ぶ");
          AddInflection(ref inflections, SenseFlag.Verb5mu, type, pre + "む");
        }
      }
      
      return inflections;
    }
    #endregion

    #region Causative-passive (-させられる,-させれる)
    {
      int length = 0;
      // TODO: handle suru
      if(word.Length > 3 && EndsWithAndHasMore(word, "せれ")) length = 2;
      else if(word.Length > 4 && EndsWithAndHasMore(word, "せられ")) length = 3;

      if(length != 0 && HandleCausativePassive(word, length, 'さ', InflectionType.CausativePassive, ref inflections))
      {
        return inflections;
      }
    }
    #endregion
    
    #region Causative (-させる)
    {
      int length = 0;

      // TODO: handle suru
      if(word.Length > 2 && EndsWithAndHasMore(word, "せ")) length = 1;

      if(length != 0 && HandleCausativePassive(word, length, 'さ', InflectionType.Causative, ref inflections))
      {
        return inflections;
      }
    }
    #endregion

    #region Passive/Potential (-られる)
    {
      int length = 0;

      // TODO: handle suru
      if(word.Length > 2 && EndsWithAndHasMore(word, "れ")) length = 1;

      if(length != 0 && HandleCausativePassive(word, length, 'ら', InflectionType.PassiveOrPotential, ref inflections))
      {
        return inflections;
      }
    }
    #endregion

    #region Negative (-ない)
    {
      InflectionRoot root = HandleNai(word);
      if(root.Inflection != InflectionType.None)
      {
        string stem = root.DictionaryForm;
        lastChar = stem[stem.Length-1];
        
        if(IsV1StemEnd(lastChar))
        {
          AddInflection(ref inflections, SenseFlag.Verb1, root.Inflection, stem+"る");
        }

        if(stem.Length > 1 && IsV5NegativeEnd(lastChar))
        {
          AddV5Inflection(ref inflections, lastChar, root.Inflection, RemoveLast(stem, 1));
        }
        else if(word == "し" || word == "為")
        {
          AddInflection(ref inflections, SenseFlag.VerbSuruIrregular, root.Inflection, "為る");
          return inflections;
        }
        else if(word == "こ" || word == "来")
        {
          AddInflection(ref inflections, SenseFlag.VerbKuruSpecial, root.Inflection, "来る");
          return inflections;
        }
      }
    }
    #endregion

    #region Seems like (-そう)
    if(word.Length >= 3 && word.EndsWith("そう"))
    {
      word = RemoveLast(word, 2);
      lastChar = word[word.Length-1];

      if(IsV1StemEnd(lastChar))
      {
        AddInflection(ref inflections, SenseFlag.Verb1, InflectionType.SeemsLike, word+"る");
        return inflections;
      }
      else if(IsV5ConjunctiveEnd(lastChar))
      {
        AddV5Inflection(ref inflections, lastChar, InflectionType.SeemsLike, RemoveLast(word, 1));
        return inflections;
      }
    }
    #endregion

    #region Special base forms (くる/する)
    // check for special base forms
    {
      InflectionType type = InflectionType.None;
      if(word == "すれば" || word == "為れば")
      {
        type = InflectionType.ConditionalEba;
      }
      else if(word == "しよう" || word == "為よう")
      {
        type = InflectionType.Volitional;
      }
      else if(word == "せよ" || word == "しろ" || word == "為よ" || word == "為ろ")
      {
        type = InflectionType.Imperative;
      }
      if(type != InflectionType.None)
      {
        AddInflection(ref inflections, SenseFlag.VerbSuruIrregular, type, "為る");
        return inflections;
      }

      type = InflectionType.None;
      if(word == "くれば" || word == "来れば")
      {
        type = InflectionType.ConditionalEba;
      }
      else if(word == "こよう" || word == "来よう")
      {
        type = InflectionType.Volitional;
      }
      else if(word == "こい" || word == "来い")
      {
        type = InflectionType.Imperative;
      }
      if(type != InflectionType.None)
      {
        AddInflection(ref inflections, SenseFlag.VerbKuruSpecial, type, "来る");
        return inflections;
      }
    }
    #endregion

    #region v1/v5 base forms
    // TODO: add support for -tai, etc
    // check for v1 base forms (negative, conjunctive, imperative, volitional)
    if(word.Length >= 3)
    {
      // v1 volitional and base+reba
      if(IsV1StemEnd(word[word.Length-3]) && (word.EndsWith("よう") || word.EndsWith("れば")))
      {
        // v1 volitional or base+reba
        InflectionType type = prevChar == 'よ' ? InflectionType.Volitional : InflectionType.ConditionalEba;
        AddInflection(ref inflections, SenseFlag.Verb1, type, RemoveLast(word, 2)+"る");
      }
      else if(IsV1StemEnd(prevChar) && JP.IsHiragana(lastChar)) // v1 base+almost anything
      {
        AddInflection(ref inflections, SenseFlag.Verb1, InflectionType.Unknown, allBut1+"る");
      }
    }
    else if(IsV1StemEnd(lastChar) && JP.IsHiragana(lastChar)) // v1 base, but only if it ends with an I or E hiragana
    {
      AddInflection(ref inflections, SenseFlag.Verb1, InflectionType.Unknown, word+"る");
    }

    // check for v5 base forms
    if(lastChar == 'う' && (JP.GetKanaInfo(prevChar) & KanaInfo.SoundMask) == KanaInfo.O)
    {
      // if it looks like the last part of -~ou (volitional) ending...
      AddV5Inflection(ref inflections, prevChar, InflectionType.Volitional, RemoveLast(word, 2));
    }
    else if(lastChar == 'ば' && (JP.GetKanaInfo(prevChar) & KanaInfo.SoundMask) == KanaInfo.E)
    {
      // if it looks like the last part of -~eba (conditional eba) ending...
      AddV5Inflection(ref inflections, prevChar, InflectionType.ConditionalEba, RemoveLast(word, 2));
    }
    else
    {
      KanaInfo info = JP.GetKanaInfo(lastChar), sound = info & KanaInfo.SoundMask;
      if(sound != KanaInfo.U && sound != KanaInfo.N)
      {
        InflectionType type;
        switch(sound)
        {
          case KanaInfo.A: type = InflectionType.NegativeForm; break;
          case KanaInfo.I: type = InflectionType.Conjunctive; break;
          case KanaInfo.E: type = InflectionType.ImperativeOrPotential; break;
          case KanaInfo.O: type = InflectionType.Volitional; break;
          default: type = InflectionType.Unknown; break;
        }

        // if it's not in the Wa row, or it is in the Wa row but the sound is 'A', then it's safe to call AddV5Inflection
        if((info & KanaInfo.RowMask) != KanaInfo.WaRow || sound == KanaInfo.A)
        {
          AddV5Inflection(ref inflections, lastChar, type, allBut1);
        }
      }
    }
    #endregion

    return inflections;
  }

  static bool EndsWithAndHasMore(string word, string end)
  {
    return word.Length > end.Length && word.EndsWith(end);
  }

  static KanaInfo GetVoicedRow(char c)
  {
    return JP.GetKanaInfo(c) & (KanaInfo.RowMask|KanaInfo.VoiceMask);
  }

  static bool IsV1StemEnd(char c)
  {
    if(JP.IsKanji(c)) return true;
    KanaInfo info = JP.GetKanaInfo(c), sound = info & KanaInfo.SoundMask;
    // it must end in an え or い sound and not be small or katakana
    return (sound == KanaInfo.E || sound == KanaInfo.I) && (info & (KanaInfo.Katakana|KanaInfo.Small)) == 0;
  }

  static bool IsV5ConjunctiveEnd(char c)
  {
    return "いきりしびぎみにち".IndexOf(c) != -1;
  }

  static bool IsV5NegativeEnd(char c)
  {
    return "わからさばがまなた".IndexOf(c) != -1;
  }

  static bool HandleCausativePassive(string word, int length, char prefixChar, InflectionType type,
                                     ref List<InflectionRoot> inflections)
  {
    if(length != 0)
    {
      word = RemoveLast(word, length);
      string allBut1 = RemoveLast(word, 1);
      char  lastChar = word[word.Length-1];
      char  prevChar = word[word.Length-2];

      if(lastChar == prefixChar && IsV1StemEnd(prevChar)) // v1 verbs
      {
        AddInflection(ref inflections, SenseFlag.Verb1, type, allBut1+"る");
      }
      else if(type == InflectionType.PassiveOrPotential && IsV1StemEnd(lastChar)) // v1 verb + reru (rareru shortened)
      {
        AddInflection(ref inflections, SenseFlag.Verb1, type, word+"る");
      }
      else
      {
        if(word.Length == 2 && lastChar == prefixChar && (prevChar == '来' || prevChar == 'こ'))
        {
          AddInflection(ref inflections, SenseFlag.VerbKuruSpecial, type, "来る");
        }

        if(IsV5NegativeEnd(lastChar))
        {
          AddV5Inflection(ref inflections, lastChar, type, allBut1);
        }
      }

      return true;
    }
    else
    {
      return false;
    }
  }

  static InflectionRoot HandleNai(string word)
  {
    int length = 0;
    InflectionType type = InflectionType.Negative;

    if(EndsWithAndHasMore(word, "ない"))
    {
      type |= InflectionType.Plain;
      length = 2;
    }
    else if(EndsWithAndHasMore(word, "なく"))
    {
      type |= InflectionType.Adverbial;
      length = 2;
    }
    else if(EndsWithAndHasMore(word, "なくて"))
    {
      type |= InflectionType.Te;
      length = 3;
    }
    else if(EndsWithAndHasMore(word, "なければ"))
    {
      type |= InflectionType.ConditionalEba;
      length = 4;
    }
    else if(EndsWithAndHasMore(word, "なかった"))
    {
      type |= InflectionType.Past|InflectionType.Plain;
      length = 4;
    }

    InflectionRoot root = new InflectionRoot();
    if(length != 0)
    {
      root.DictionaryForm = RemoveLast(word, length);
      root.Inflection     = type;
    }
    return root;
  }

  static Region[] SplitIntoRegions(string text)
  {
    List<Region> regions = new List<Region>();

    int index = 0;
    while(index < text.Length)
    {
      Region region = new Region();
      // first find the start of the region. skip all region separators
      while(index < text.Length)
      {
        char c = text[index];
        if(JP.IsKanji(c) || (JP.IsKana(c) && c != '・')) break;
        index++;
      }
      if(index == text.Length) break;
      region.Position = index;

      // find the end of the region. skip until we find a region separator, or a different type of character.
      while(index < text.Length)
      {
        char c = text[index];
        if(!JP.IsKanji(c) && !JP.IsKana(c) ||
        // we'll allow ・ characters only if they occur between katakana characters
           c == '・' && (!JP.IsKatakana(text[index-1]) || index >= text.Length-1 || !JP.IsKatakana(text[index+1])))
        {
          break;
        }
        index++;
      }
      region.Length = index - region.Position;
      regions.Add(region);
      index++; // the second loop stopped on a character we don't want, so we can just advance one right away.
    }
    
    return regions.ToArray();
  }
  
  static Dictionary<string,string> allowedKanaWords;
  static List<string> ignoreInflected;
}

} // namespace Jappy.Backend
