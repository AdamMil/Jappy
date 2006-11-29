using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Jappy
{

#region Dictionary entry data structure
#region Result flags
[Flags]
public enum ResultFlag : byte
{
  /// <summary>No result flags.</summary>
  None=0,
  /// <summary>Word containing irregular kanji usage</summary>
  IrregularKanji=0x1,
  /// <summary>Word containing irregular kana usage</summary>
  IrregularKana=0x2,
  /// <summary>Irregular okurigana usage</summary>
  IrregularOkurigana=0x4,
  /// <summary>Word containing out-dated kanji</summary>
  OldKanji=0x8,
  /// <summary>Out-dated or obsolete kana usage</summary>
  OldKana=0x10,
  /// <summary>Ateji (phonetic) reading</summary>
  Ateji=0x20,
  /// <summary>Gikun (meaning) reading</summary>
  Gikun=0x40,
}

public enum SenseFlag : byte
{
  Unknown,
  /// <summary>Adjective (keiyoushi)</summary>
  Adj,
  /// <summary>Adjectival nouns or quasi-adjectives (keiyodoshi)</summary>
  AdjNa,
  /// <summary>Nouns which may take the genitive case particle 'no'</summary>
  AdjNo,
  /// <summary>Pre-noun adjectival (rentaishi)</summary>
  AdjPn,
  /// <summary>'Taru' adjective</summary>
  AdjTaru,
  /// <summary>Adverb (fukushi)</summary>
  Adv,
  /// <summary>Adverbial noun</summary>
  AdvN,
  /// <summary>Adverb taking the 'to' particle</summary>
  AdvTo,
  /// <summary>Auxiliary</summary>
  Auxiliary,
  /// <summary>Auxiliary verb</summary>
  AuxiliaryVerb,
  /// <summary>Auxiliary adjective</summary>
  AuxiliaryAdj,
  /// <summary>Noun (common) (futsuumeishi)</summary>
  Noun,
  /// <summary>"Adverbial noun (fukushitekimeishi)"</summary>
  NounAdv,
  /// <summary>"Noun, used as a suffix"</summary>
  NounSuffix,
  /// <summary>"Noun, used as a prefix"</summary>
  NounPrefix,
  /// <summary>"Noun (temporal) (jisoumeishi)"</summary>
  NounTemporal,
  /// <summary>"Ichidan verb"</summary>
  Verb1,
  /// <summary>Godan verb (not completely classified)</summary>
  Verb5,
  /// <summary>Godan verb - -aru special class</summary>
  Verb5Aru,
  /// <summary>Godan verb with 'bu' ending</summary>
  Verb5bu,
  /// <summary>Godan verb with 'gu' ending</summary>
  Verb5gu,
  /// <summary>Godan verb with 'ku' ending</summary>
  Verb5ku,
  /// <summary>Godan verb - Iku/Yuku special class</summary>
  Verb5kuSpecial,
  /// <summary>Godan verb with 'mu' ending</summary>
  Verb5mu,
  /// <summary>Godan verb with 'nu' ending</summary>
  Verb5nu,
  /// <summary>Godan verb with 'ru' ending</summary>
  Verb5ru,
  /// <summary>Godan verb with 'ru' ending (irregular verb)</summary>
  Verb5ruIrregular,
  /// <summary>Godan verb with 'su' ending</summary>
  Verb5su,
  /// <summary>Godan verb with 'tsu' ending</summary>
  Verb5tu,
  /// <summary>Godan verb with 'u' ending</summary>
  Verb5u,
  /// <summary>Godan verb with 'u' ending (special class)</summary>
  Verb5uSpecial,
  /// <summary>Godan verb - Uru old class verb (old form of Eru)</summary>
  Verb5uru,
  /// <summary>Intransitive verb</summary>
  Intransitive,
  /// <summary>Kuru verb - special class</summary>
  VerbKuru,
  /// <summary>noun or participle which takes the aux. verb suru</summary>
  VerbSuru,
  /// <summary>suru verb - special class</summary>
  VerbSuruSpecial,
  /// <summary>suru verb - irregular</summary>
  VerbSuruIrregular,
  /// <summary>zuru verb - (alternative form of -jiru verbs)</summary>
  VerbZuru,
  /// <summary>Transitive verb</summary>
  Transitive,
  /// <summary>Conjunction</summary>
  Conjunction,
  /// <summary>Expressions (phrases, clauses, etc.)</summary>
  Expression,
  /// <summary>Interjection (kandoushi)</summary>
  Interjection,
  /// <summary>Particle</summary>
  Particle,

  /// <summary>Martial arts term</summary>
  MartialArts,
  /// <summary>Rude or X-rated term (not displayed in educational software)</summary>
  XRated,
  /// <summary>Abbreviation</summary>
  Abbreviation,
  /// <summary>Exclusively kanji</summary>
  AlwaysKanji,
  /// <summary>Archaism</summary>
  Archaic,
  /// <summary>Buddhist term</summary>
  Buddhist,
  /// <summary>Children's language</summary>
  ChildSpeak,
  /// <summary>Colloquialism</summary>
  Colloquialism,
  /// <summary>Computer terminology</summary>
  Computer,
  /// <summary>Derogatory</summary>
  Derogatory,
  /// <summary>Familiar language </summary>
  Familiar,
  /// <summary>Female term or language</summary>
  Female,
  /// <summary>Food term</summary>
  Food,
  /// <summary>Geometry term</summary>
  Geometry,
  /// <summary>Grammatical term</summary>
  Grammatical,
  /// <summary>Honorific or respectful (sonkeigo) language </summary>
  Honorific,
  /// <summary>Humble (kenjougo) language </summary>
  Humble,
  /// <summary>Idiomatic expression </summary>
  Idiom,
  /// <summary>Irregular verb</summary>
  IrregularVerb,
  /// <summary>Linguistics terminology</summary>
  Linguistics,
  /// <summary>Manga slang</summary>
  MangaSlang,
  /// <summary>Male term or language</summary>
  Male,
  /// <summary>Male slang</summary>
  MaleSlang,
  /// <summary>Mathematics</summary>
  Math,
  /// <summary>Military</summary>
  Military,
  /// <summary>Negative (in a negative sentence, or with negative verb)</summary>
  Negative,
  /// <summary>Negative verb (when used with)</summary>
  NegativeVerb,
  /// <summary>Numeric</summary>
  Numeric,
  /// <summary>Obsolete term</summary>
  Obsolete,
  /// <summary>Obscure term</summary>
  Obscure,
  /// <summary>Polite (teineigo) language </summary>
  Polite,
  /// <summary>Prefix</summary>
  Prefix,
  /// <summary>Physics terminology</summary>
  Physics,
  /// <summary>Quod vide (see another entry)</summary>
  QuodVide,
  /// <summary>Rare</summary>
  Rare,
  /// <summary>Sensitive</summary>
  Sensitive,
  /// <summary>Slang</summary>
  Slang,
  /// <summary>Suffix </summary>
  Suffix,
  /// <summary>Word usually written using kanji alone </summary>
  UsuallyKanji,
  /// <summary>Word usually written using kana alone </summary>
  UsuallyKana,
  /// <summary>Vulgar expression or word </summary>
  Vulgar,
  /// <summary>Masculine gender</summary>
  MasculineGender,
  /// <summary>Feminine gender</summary>
  FeminineGender,
  /// <summary>Neuter gender</summary>
  NeutralGender,
}
#endregion

/// <summary>A structure representing a headword or a reading of a headword.</summary>
public struct Word
{
  internal Word(IOReader reader, StringCompressor compressor)
  {
    Debug.Assert(sizeof(ResultFlag) == 1);
    Text  = compressor.ReadString(reader);
    Flags = (ResultFlag)reader.ReadByte();
    AppliesToHeadword = reader.ReadSByte();
    Frequency = reader.ReadByte();
  }

  public bool Has(ResultFlag flag)
  {
    return (Flags & flag) != 0;
  }

  /// <summary>The text of the headword or reading.</summary>
  public string Text;
  /// <summary>Flags that give information about this particular word.</summary>
  public ResultFlag Flags;
  /// <summary>For readings only. The index of the headword to which this reading applies. If -1, the reading applies
  /// to all headwords.
  /// </summary>
  public sbyte AppliesToHeadword;
  /// <summary>If nonzero, A marker of how common a word is, from 1 (very common) to 99 (very uncommon).</summary>
  public byte Frequency;
  
  internal void Write(IOWriter writer, StringCompressor compressor)
  {
    Debug.Assert(sizeof(ResultFlag) == 1);
    compressor.WriteString(writer, Text);
    writer.Add((byte)Flags);
    writer.Add(AppliesToHeadword);
    writer.Add(Frequency);
  }
}

/// <summary>A structure representing a word or phrase that conveys a given meaning.</summary>
public struct Gloss
{
  internal Gloss(IOReader reader, StringCompressor meaningCompressor)
  {
    Text = meaningCompressor.ReadString(reader);
    GoodMatch = reader.ReadBool();
  }

  /// <summary>The text of the gloss.</summary>
  public string Text;
  /// <summary>If true, this gloss is a particularly good match for the source word.</summary>
  public bool GoodMatch;

  internal void Write(IOWriter writer, StringCompressor meaningCompressor)
  {
    meaningCompressor.WriteString(writer, Text);
    writer.Add(GoodMatch);
  }
}

public enum Relation : byte
{
  Synonym, Antonym
}

public struct RelatedWord
{
  public RelatedWord(string word, Relation relation)
  {
    Word     = word;
    Relation = relation;
  }
  
  internal RelatedWord(IOReader reader, StringCompressor headwordCompressor)
  {
    Debug.Assert(sizeof(Relation) == 1);
    Word = headwordCompressor.ReadString(reader);
    Relation = (Relation)reader.ReadByte();
  }

  public string Word;
  public Relation Relation;
  
  internal void Write(IOWriter writer, StringCompressor headwordCompressor)
  {
    Debug.Assert(sizeof(Relation) == 1);
    headwordCompressor.WriteString(writer, Word);
    writer.Add((byte)Relation);
  }
}

/// <summary>A structure representing a single destination meaning of a source word or phrase.</summary>
public struct Meaning
{
  internal Meaning(IOReader reader, StringCompressor headwordCompressor, StringCompressor meaningCompressor)
  {
    Glosses = new Gloss[reader.ReadByte()];
    for(int i=0; i<Glosses.Length; i++) Glosses[i] = new Gloss(reader, meaningCompressor);
    
    int length = reader.ReadByte();
    if(length == 0)
    {
      Flags = null;
    }
    else
    {
      Debug.Assert(sizeof(SenseFlag) == 1);
      Flags = new SenseFlag[length];
      for(int i=0; i<Flags.Length; i++) Flags[i] = (SenseFlag)reader.ReadByte();
    }
    
    length = reader.ReadByte();
    if(length == 0)
    {
      Related = null;
    }
    else
    {
      Related = new RelatedWord[length];
      for(int i=0; i<Related.Length; i++) Related[i] = new RelatedWord(reader, headwordCompressor);
    }
    
    AppliesToHeadword = reader.ReadSByte();
    AppliesToReading  = reader.ReadSByte();
  }
  
  /// <summary>An array of words or phrases that convey the meaning.</summary>
  public Gloss[] Glosses;
  /// <summary>A series of sense flags providing grammatical and usage information.</summary>
  public SenseFlag[] Flags;
  /// <summary>Source words that are related to this meaning.</summary>
  public RelatedWord[] Related;
  /// <summary>If not equal no -1, indicates that this meaning only applies to the indicated headword or reading.</summary>
  public sbyte AppliesToHeadword, AppliesToReading;
  
  internal void Write(IOWriter writer, StringCompressor headwordCompressor, StringCompressor meaningCompressor)
  {
    writer.Add((byte)Glosses.Length);
    foreach(Gloss gloss in Glosses) gloss.Write(writer, meaningCompressor);
    
    writer.Add((byte)(Flags == null ? 0 : Flags.Length));
    if(Flags != null)
    {
      Debug.Assert(sizeof(SenseFlag) == 1);
      for(int i=0; i<Flags.Length; i++) writer.Add((byte)Flags[i]);
    }

    writer.Add((byte)(Related == null ? 0 : Related.Length));
    if(Related != null)
    {
      foreach(RelatedWord word in Related) word.Write(writer, headwordCompressor);
    }
    
    writer.Add(AppliesToHeadword);
    writer.Add(AppliesToReading);
  }
}

/// <summary>A structure representing a dictionary entry.</summary>
public struct Entry
{
  internal Entry(IOReader reader, StringCompressor headwordCompressor,
                 StringCompressor readingCompressor, StringCompressor meaningCompressor)
  {
    ID = reader.ReadUint();

    Headwords = new Word[reader.ReadByte()];
    for(int i=0; i<Headwords.Length; i++) Headwords[i] = new Word(reader, headwordCompressor);
    
    int length = reader.ReadByte();
    if(length == 0)
    {
      Readings = null;
    }
    else
    {
      Readings = new Word[length];
      for(int i=0; i<Readings.Length; i++) Readings[i] = new Word(reader, readingCompressor);
    }
    
    Meanings = new Meaning[reader.ReadByte()];
    for(int i=0; i<Meanings.Length; i++) Meanings[i] = new Meaning(reader, headwordCompressor, meaningCompressor);
  }

  /// <summary>Returns the minimum frequency of the headwords for this entry.</summary>
  public byte Frequency
  {
    get
    {
      int lowest = int.MaxValue;

      foreach(Word word in Headwords)
      {
        if(word.Frequency != 0 && word.Frequency < lowest)
        {
          lowest = word.Frequency;
        }
      }

      return (byte)(lowest == int.MaxValue ? 0 : lowest);
    }
  }

  /// <summary>The headwords in the source language.</summary>
  public Word[] Headwords;
  /// <summary>The readings for the headwords.</summary>
  public Word[] Readings;
  /// <summary>The meanings in the destination language.</summary>
  public Meaning[] Meanings;
  /// <summary>The unique identifier of this dictionary entry.</summary>
  public uint ID;

  internal void Write(IOWriter writer, StringCompressor headwordCompressor,
                      StringCompressor readingCompressor, StringCompressor meaningCompressor)
  {
    writer.Add(ID);

    writer.Add((byte)Headwords.Length);
    foreach(Word word in Headwords) word.Write(writer, headwordCompressor);

    writer.Add((byte)(Readings == null ? 0 : Readings.Length));
    if(Readings != null)
    {
      foreach(Word word in Readings) word.Write(writer, readingCompressor);
    }

    writer.Add((byte)Meanings.Length);
    foreach(Meaning meaning in Meanings) meaning.Write(writer, headwordCompressor, meaningCompressor);
  }
}
#endregion

[Flags]
public enum SearchFlag
{
  SearchHeadwords=1, SearchReadings=2, SearchMeanings=4, SearchAll = SearchHeadwords|SearchReadings|SearchMeanings,
  MatchStart=8, MatchEnd=16, ExactMatch=MatchStart|MatchEnd, MatchMask=ExactMatch
}

#region Dictionary
public class WordDictionary : IDisposable
{
  public void Dispose()
  {
    Unload();
  }

  public Entry GetEntryById(uint id)
  {
    if(dataStream != null)
    {
      reader.Position = id; // with the native data stream, entry IDs are their position within the file
      return new Entry(reader, headwordCompressor, readingCompressor, meaningCompressor);
    }
    else if(importedData != null)
    {
      return importedData[id];
    }
    else throw new InvalidOperationException("The dictionary has not been loaded.");
  }

  #region Loading and saving
  public void Load(string indexFile, string dataFile)
  {
    if(indexFile == null || dataFile == null) throw new ArgumentNullException();
    FileStream indexStream = new FileStream(indexFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1,
                                            FileOptions.RandomAccess);
    FileStream dataStream  = new FileStream(dataFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1,
                                            FileOptions.RandomAccess);
    Load(indexStream, dataStream);
  }

  public void Load(Stream indexStream, Stream dataStream)
  {
    if(indexStream == null || dataStream == null) throw new ArgumentNullException();
    Unload();

    byte[] magic = new byte[4];
    if(dataStream.Read(magic, 0, 4) < 4 || Encoding.ASCII.GetString(magic) != "DCWD")
    {
      throw new ArgumentException("Invalid dictionary file.");
    }
    if(dataStream.ReadByte() != 1)
    {
      throw new ArgumentException("This dictionary file was created with a newer version of the "+
                                  "dictionary and cannot be opened.");
    }

    if(indexStream.Read(magic, 0, 4) < 4 || Encoding.ASCII.GetString(magic) != "DCWI")
    {
      throw new ArgumentException("Invalid dictionary file.");
    }
    if(indexStream.ReadByte() != 1)
    {
      throw new ArgumentException("This dictionary file was created with a newer version of the "+
                                  "dictionary and cannot be opened.");
    }

    headwordIndex = CreateHeadwordIndex();
    readingIndex  = CreateReadingIndex();
    meaningIndex  = CreateMeaningIndex();
    using(IOReader reader = new IOReader(indexStream))
    {
      headwordIndex.Load(reader);
      readingIndex.Load(reader);
      meaningIndex.Load(reader);
    }
    
    using(IOReader reader = new IOReader(dataStream))
    {
      headwordCompressor.Load(reader);
      readingCompressor.Load(reader);
      meaningCompressor.Load(reader);
    }

    this.indexStream = indexStream;
    this.dataStream  = dataStream;
    this.reader      = new IOReader(dataStream, 256, false);
  }

  public void Save(string indexFile, string dataFile)
  {
    if(indexFile == null || dataFile == null) throw new ArgumentNullException();
    if(importedData == null) throw new InvalidOperationException("Data is not loaded in memory.");

    FileStream indexStream = new FileStream(indexFile, FileMode.Create, FileAccess.Write, FileShare.None, 1,
                                            FileOptions.SequentialScan);
    FileStream dataStream  = new FileStream(dataFile, FileMode.Create, FileAccess.Write, FileShare.None, 1,
                                            FileOptions.SequentialScan);

    Save(indexStream, dataStream);
  }

  public void Save(Stream indexStream, Stream dataStream)
  {
    if(indexStream == null || dataStream == null) throw new ArgumentNullException();
    if(importedData == null) throw new InvalidOperationException("Data is not loaded in memory.");

    Dictionary<uint,uint> idMap = new Dictionary<uint,uint>(); // map of in-memory data IDs to new data IDs

    List<Entry> entries = new List<Entry>(importedData.Values);
    SortEntriesByFrequency(entries);

    // add all strings to the compressors
    headwordCompressor.CreateNew();
    readingCompressor.CreateNew();
    meaningCompressor.CreateNew();
    foreach(Entry entry in entries)
    {
      foreach(Word headword in entry.Headwords) headwordCompressor.AddString(headword.Text);

      if(entry.Readings != null)
      {
        foreach(Word reading in entry.Readings) readingCompressor.AddString(reading.Text);
      }

      foreach(Meaning meaning in entry.Meanings)
      {
        foreach(Gloss gloss in meaning.Glosses) meaningCompressor.AddString(gloss.Text);

        if(meaning.Related != null)
        {
          foreach(RelatedWord word in meaning.Related) headwordCompressor.AddString(word.Word);
        }
      }
    }
    headwordCompressor.FinishedAdding();
    readingCompressor.FinishedAdding();
    meaningCompressor.FinishedAdding();

    using(dataStream)
    {
      dataStream.Write(Encoding.ASCII.GetBytes("DCWD"), 0, 4); // write the magic token (DiCtionary Word Data)
      dataStream.WriteByte(1);                                 // version number

      using(IOWriter writer = new IOWriter(dataStream))
      {
        headwordCompressor.Save(writer);
        readingCompressor.Save(writer);
        meaningCompressor.Save(writer);

        writer.Add(importedData.Count);

        foreach(Entry entry in entries)
        {
          Entry clone = entry;
          clone.ID = (uint)writer.Position;
          clone.Write(writer, headwordCompressor, readingCompressor, meaningCompressor);

          idMap.Add(entry.ID, clone.ID);
        }
      }
    }

    using(indexStream)
    {
      indexStream.Write(Encoding.ASCII.GetBytes("DCWI"), 0, 4); // write the magic token (DiCtionary Word Index)
      indexStream.WriteByte(1);                                 // version number
      using(IOWriter writer = new IOWriter(indexStream))
      {
        headwordIndex.Save(writer, idMap);
        readingIndex.Save(writer, idMap);
        meaningIndex.Save(writer, idMap);
      }
    }
  }
  #endregion

  public IEnumerable<uint> Search(string query, SearchFlag flags)
  {
    AssertLoaded();

    List<IEnumerable<uint>> positives = new List<IEnumerable<uint>>();
    List<IEnumerable<uint>> negatives = new List<IEnumerable<uint>>();

    Match match = queryRE.Match(query);
    while(match.Success)
    {
      List<IEnumerable<uint>> addTo;

      string value = match.Value;

      if(value[0] == '-')
      {
        value = value.Substring(1);
        addTo = negatives;
      }
      else
      {
        addTo = positives;
      }
      
      if(value[0] == '"')
      {
        value = value.Substring(1, value.Length-2);
        throw new NotImplementedException();
      }
      else
      {
        List<IEnumerable<uint>> types = new List<IEnumerable<uint>>(3);
        if((flags & SearchFlag.SearchHeadwords) != 0)
        {
          types.Add(headwordIndex.Search(query, flags));
        }
        if((flags & SearchFlag.SearchReadings) != 0)
        {
          types.Add(readingIndex.Search(query, flags));
        }
        if((flags & SearchFlag.SearchMeanings) != 0)
        {
          types.Add(meaningIndex.Search(query, flags));
        }
        if(types.Count != 0)
        {
          addTo.Add(types.Count == 1 ? types[0] : new UnionIterator(types));
        }
      }
      
      match = match.NextMatch();
    }
    
    if(positives.Count == 0) throw new ArgumentException("No positive search items in this query.");
    IEnumerable<uint> idIterator = positives.Count == 1 ? positives[0] : new IntersectionIterator(positives);

    if(negatives.Count != 0)
    {
      IEnumerable<uint> negative = negatives.Count == 1 ? negatives[0] : new UnionIterator(negatives);
      idIterator = new SubtractionIterator(idIterator, negative);
    }
    
    return idIterator;
  }

  #region Index manipulation
  /// <summary>Returns a regular expression match that will walk through all the indexable tokens in a string of
  /// destination language text.
  /// </summary>
  /// <remarks>The default implementation returns strings of word characters, matched by the regular expression
  /// <c>\w+</c>.
  /// </remarks>
  protected virtual Match MatchMeaningWords(string glossText)
  {
    return wordRE.Match(glossText);
  }

  protected virtual Index CreateHeadwordIndex()
  {
    return new MemoryHashIndex();
  }

  protected virtual Index CreateReadingIndex()
  {
    return new MemoryHashIndex();
  }

  protected virtual Index CreateMeaningIndex()
  {
    return new MemoryHashIndex();
  }

  void PopulateHeadwordIndex(List<Entry> entries)
  {
    Dictionary<string, List<uint>> headwords = new Dictionary<string, List<uint>>();
    foreach(Entry entry in entries)
    {
      foreach(Word headword in entry.Headwords)
      {
        AddHashIndexEntry(headwords, headword.Text, entry.ID);
      }
    }

    PopulateIndex(headwordIndex, headwords);
  }

  void PopulateReadingIndex(List<Entry> entries)
  {
    Dictionary<string, List<uint>> readings = new Dictionary<string, List<uint>>();
    foreach(Entry entry in entries)
    {
      Word[] words = entry.Readings == null ? entry.Headwords : entry.Readings;
      foreach(Word word in words)
      {
        AddHashIndexEntry(readings, word.Text, entry.ID);
      }
    }

    PopulateIndex(readingIndex, readings);
  }

  void PopulateMeaningIndex(List<Entry> entries)
  {
    Dictionary<string, List<uint>> meanings = new Dictionary<string, List<uint>>();
    foreach(Entry entry in entries)
    {
      foreach(Meaning meaning in entry.Meanings)
      {
        foreach(Gloss gloss in meaning.Glosses)
        {
          Match match = MatchMeaningWords(gloss.Text);
          while(match.Success)
          {
            AddHashIndexEntry(meanings, match.Value, entry.ID);
            match = match.NextMatch();
          }
        }
      }
    }

    PopulateIndex(meaningIndex, meanings);
  }

  static void AddHashIndexEntry(Dictionary<string,List<uint>> index, string key, uint entryID)
  {
    List<uint> list;
    if(!index.TryGetValue(key, out list)) index[key] = list = new List<uint>();
    list.Add(entryID); // the list must be sorted and duplicates removed later
  }
  
  static void PopulateIndex(Index index, Dictionary<string,List<uint>> idMap)
  {
    index.CreateNew();

    List<uint> sortedList = new List<uint>();
    foreach(KeyValuePair<string,List<uint>> pair in idMap)
    {
      sortedList.AddRange(pair.Value);
      sortedList.Sort(); // sort the array
      uint lastKey = ~sortedList[sortedList.Count-1]; // set the last key to something other than the first key
      for(int i=sortedList.Count-1; i>=0; i--) // and remove duplicates
      {
        uint key = sortedList[i];
        if(key != lastKey) lastKey = key;
        else sortedList.RemoveAt(i);
      }

      index.Add(pair.Key, sortedList.ToArray());
      sortedList.Clear();
    }
    
    index.FinishedAdding();
  }
  #endregion

  protected void AssertLoaded()
  {
    if(dataStream == null)
    {
      throw new InvalidOperationException("The dictionary has not been loaded.");
    }
  }

  protected void ImportEntries(List<Entry> entries)
  {
    Unload();

    entries = new List<Entry>(entries); // clone the list because we're going to modify it
    for(int i=0; i<entries.Count; i++)  // renumber the entries sequentially
    {
      Entry entry = entries[i];
      entry.ID = (uint)i;
      entries[i] = entry;
    }

    importedData = new Dictionary<uint,Entry>(entries.Count);
    foreach(Entry entry in entries) importedData.Add(entry.ID, entry);

    headwordIndex = CreateHeadwordIndex();
    readingIndex  = CreateReadingIndex();
    meaningIndex  = CreateMeaningIndex();
    
    PopulateHeadwordIndex(entries);
    PopulateReadingIndex(entries);
    PopulateMeaningIndex(entries);
  }

  protected void Unload()
  {
    Unload(ref reader);
    Unload(ref dataStream);
    Unload(ref indexStream);
    Unload(ref headwordIndex);
    Unload(ref readingIndex);
    Unload(ref meaningIndex);
    importedData = null;
  }

  protected void Unload<T>(ref T disposable) where T : class, IDisposable
  {
    if(disposable != null)
    {
      disposable.Dispose();
      disposable = null;
    }
  }

  protected static void SortEntriesByFrequency(List<Entry> entries)
  {
    entries.Sort(FrequencyComparer.Instance); // sort them by frequency
  }

  sealed class FrequencyComparer : IComparer<Entry>
  {
    FrequencyComparer() { }

    public int Compare(Entry a, Entry b)
    {
      int freqDiff = (a.Frequency == 0 ? 50 : (int)a.Frequency) - (b.Frequency == 0 ? 50 : (int)b.Frequency);
      if(freqDiff != 0) return freqDiff;
      
      return string.Compare(a.Headwords[0].Text, b.Headwords[0].Text, StringComparison.InvariantCultureIgnoreCase);
    }
    
    public readonly static FrequencyComparer Instance = new FrequencyComparer();
  }

  Dictionary<uint, Entry> importedData;
  Index headwordIndex, readingIndex, meaningIndex;

  StringCompressor headwordCompressor=new StringCompressor(), readingCompressor=new StringCompressor(),
                   meaningCompressor=new StringCompressor();
  Stream dataStream, indexStream;
  IOReader reader;

  static readonly Regex wordRE = new Regex(@"\w+", RegexOptions.CultureInvariant | RegexOptions.Singleline);
  static readonly Regex queryRE = new Regex(@"-?(?:""[^""]+""|\w+)",
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

  sealed class JpDiskHashIndex : DiskHashIndex
  {
    protected override string NormalizeKey(string key)
    {
      return JP.ConvertStringUsingCharacterMap(base.NormalizeKey(key), charNormalizationMap);
    }
  }

  sealed class JpTrieIndex : TrieIndex
  {
    protected override string NormalizeKey(string key)
    {
      return JP.ConvertStringUsingCharacterMap(base.NormalizeKey(key), charNormalizationMap);
    }
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
    };

    ResultFlag[] resultFlags = new ResultFlag[]
    {
      ResultFlag.IrregularOkurigana, ResultFlag.IrregularKanji, ResultFlag.IrregularKana,
      ResultFlag.OldKanji, ResultFlag.OldKana, ResultFlag.Ateji, ResultFlag.Gikun,
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
      SenseFlag.Verb5uru, SenseFlag.Intransitive, SenseFlag.VerbKuru, SenseFlag.VerbSuru, SenseFlag.VerbSuruSpecial,
      SenseFlag.VerbSuruIrregular,

      SenseFlag.VerbZuru, SenseFlag.Transitive, SenseFlag.Vulgar, SenseFlag.MasculineGender, SenseFlag.FeminineGender,
      SenseFlag.NeutralGender
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

} // namespace Jappy