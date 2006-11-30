using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Jappy
{

public struct TranslatedWord
{
  public WordDictionary Dictionary;  
  public uint[] EntryIds;
  public int Position, Length;
  public bool Inflected;
}

public static class WordTranslator
{
  static WordTranslator()
  {
    // create a dictionary mapping certain kana words to their kanji headwords. this is used to force acceptance for
    // these particular kana words even when the would not normally be accepted, due to the kanji headword lacking the
    // UsuallyKana flag.
    allowedKanaWords = new Dictionary<string,string>();
    allowedKanaWords["こと"] = "事";
    allowedKanaWords["こんにちは"] = "今日は";
    allowedKanaWords["とき"] = "時";
  }

  public static TranslatedWord[] TranslateWordsInJapaneseText(string japaneseText)
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

  static bool SearchFor(string query, ref TranslatedWord word)
  {
    query = query.Replace("・", ""); // strip ・ characters out of blocks of katakana

    WordType type = ClassifyWord(query);
    if(type == WordType.MixedKana) return false; // assume words with mixed kana types won't match

throw new NotImplementedException("conjugated verbs");

    SearchFlag flags = SearchFlag.ExactMatch | SearchFlag.SearchHeadwords;
    if(type != WordType.HasKanji) flags |= SearchFlag.SearchReadings; // if the word is all kana, also search readings

    return SearchFor(App.WordDict, query, type, flags, ref word) ||
           SearchFor(App.NameDict, query, type, flags, ref word);
  }
  
  static unsafe bool SearchFor(WordDictionary dictionary, string query, WordType type, SearchFlag flags,
                               ref TranslatedWord word)
  {
    const int maxIds = 4;
    uint* idList = stackalloc uint[maxIds];
    int numIds = 0;

    IEnumerable<uint> ids = JapaneseSearchStrategy.Instance.Search(dictionary, query, flags);
    foreach(uint id in ids)
    {
      if(type != WordType.HasKanji) // if the search word was all kana, we have to be careful about it...
      {
        Entry entry = dictionary.GetEntryById(id);
        bool entryIsOkay = false;

        string explicitlyAllowed;
        allowedKanaWords.TryGetValue(query, out explicitlyAllowed);

        foreach(Word headword in entry.Headwords)
        {
          WordType headwordType = ClassifyWord(headword.Text);
          if(headwordType == WordType.HasKanji) // if the headword is kanji, only accept ones usually written in kana
          {
            // if this headword is explicitly allowed, use it
            if(explicitlyAllowed != null && string.Equals(headword.Text, explicitlyAllowed, StringComparison.Ordinal))
            {
              entryIsOkay = true;
              break;
            }

            if(entry.Meanings == null) continue;
            foreach(Meaning meaning in entry.Meanings)
            {
              if(meaning.Flags == null || Array.IndexOf(meaning.Flags, SenseFlag.UsuallyKana) == -1) continue;
              if(meaning.AppliesToReading != -1 &&
                 (entry.Readings == null ||
                  JapaneseDictionary.NormalizedComparison(
                    entry.Readings[meaning.AppliesToReading].Text, query) != 0)) continue;
              entryIsOkay = true;
              break;
            }
          }
          // otherwise if the headword is the same type of kana, only accept it if the word is katakana, long, or
          // very popular. words with differing kana types are rejected.
          else if(headwordType == type &&
                  (type == WordType.AllKatakana || query.Length >= 4 || entry.Frequency != 0 && entry.Frequency < 10))
          {
            entryIsOkay = true;
          }

          if(entryIsOkay) break;
        }

        if(!entryIsOkay) goto nextEntry;
      }

      idList[numIds++] = id;
      if(numIds == maxIds) break;
      nextEntry:;
    }

    if(numIds == 0)
    {
      return false;
    }
    else
    {
      word.Dictionary = dictionary;
      word.EntryIds   = new uint[numIds];
      for(int i=0; i<word.EntryIds.Length; i++)
      {
        word.EntryIds[i] = idList[i];
      }
      return true;
    }
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
        if(JP.IsKanji(c) || JP.IsKana(c)) break;
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
    }
    
    return regions.ToArray();
  }
  
  static Dictionary<string,string> allowedKanaWords;
}

} // namespace Jappy