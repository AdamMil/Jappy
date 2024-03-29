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
using BinaryReader = AdamMil.IO.BinaryReader;
using BinaryWriter = AdamMil.IO.BinaryWriter;

namespace Jappy.Backend
{

#region Dictionary entry data structure
#region Result flags
[Flags]
public enum ResultFlag : ushort
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

  /// <summary>A family or surname</summary>
  Surname=0x80,
  /// <summary>The name of a place</summary>
  PlaceName=0x100,
  /// <summary>The name of a company</summary>
  CompanyName=0x180,
  /// <summary>The name of a product</summary>
  ProductName=0x200,
  /// <summary>A person's full name</summary>
  FullName=0x280,
  /// <summary>A given name</summary>
  GivenName=0x300,
  /// <summary>An unclassified name</summary>
  UnclassifiedName=0x380,
  /// <summary>A mask that can be applied to the result flags to obtain the name type</summary>
  NameMask=0x380,

  /// <summary>The name is feminine</summary>
  FeminineName=0x400,
  /// <summary>The name is masculine</summary>
  MasculineName=0x800,
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
  /// <summary>Kuru verb - special class. The verb kuru itself, and verb phrases such as ～て来る</summary>
  VerbKuruSpecial,
  /// <summary>noun or participle which takes the aux. verb suru</summary>
  VerbSuru,
  /// <summary>suru verb - special class. This is used for special types of noun + suru verbs.</summary>
  VerbSuruSpecial,
  /// <summary>suru verb - irregular. This is used for the suru verb itself (and verb phrases containing the suru verb
  /// itself, like そうする).
  /// </summary>
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
  /// <summary>Word usually written using kanji alone</summary>
  UsuallyKanji,
  /// <summary>Word usually written using kana alone</summary>
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
  internal Word(BinaryReader reader, StringCompressor compressor)
  {
    Debug.Assert(sizeof(ResultFlag) == 2);
    Text  = compressor.ReadString(reader);
    Flags = (ResultFlag)reader.ReadUInt16();
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
  
  internal void Write(BinaryWriter writer, StringCompressor compressor)
  {
    Debug.Assert(sizeof(ResultFlag) == 2);
    compressor.WriteString(writer, Text);
    writer.Write((ushort)Flags);
    writer.Write(AppliesToHeadword);
    writer.Write(Frequency);
  }
}

/// <summary>A structure representing a word or phrase that conveys a given meaning.</summary>
public struct Gloss
{
  internal Gloss(BinaryReader reader, StringCompressor meaningCompressor)
  {
    Text = meaningCompressor.ReadString(reader);
    GoodMatch = reader.ReadBool();
  }

  /// <summary>The text of the gloss.</summary>
  public string Text;
  /// <summary>If true, this gloss is a particularly good match for the source word.</summary>
  public bool GoodMatch;

  internal void Write(BinaryWriter writer, StringCompressor meaningCompressor)
  {
    meaningCompressor.WriteString(writer, Text);
    writer.Write(GoodMatch);
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
  
  internal RelatedWord(BinaryReader reader, StringCompressor headwordCompressor)
  {
    Debug.Assert(sizeof(Relation) == 1);
    Word = headwordCompressor.ReadString(reader);
    Relation = (Relation)reader.ReadByte();
  }

  public string Word;
  public Relation Relation;
  
  internal void Write(BinaryWriter writer, StringCompressor headwordCompressor)
  {
    Debug.Assert(sizeof(Relation) == 1);
    headwordCompressor.WriteString(writer, Word);
    writer.Write((byte)Relation);
  }
}

/// <summary>A structure representing a single destination meaning of a source word or phrase.</summary>
public struct Meaning
{
  internal Meaning(BinaryReader reader, StringCompressor headwordCompressor, StringCompressor meaningCompressor)
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
  
  public bool HasFlag(SenseFlag flag)
  {
    return Flags != null && Array.IndexOf(Flags, flag) != -1;
  }

  /// <summary>An array of words or phrases that convey the meaning.</summary>
  public Gloss[] Glosses;
  /// <summary>A series of sense flags providing grammatical and usage information.</summary>
  public SenseFlag[] Flags;
  /// <summary>Source words that are related to this meaning.</summary>
  public RelatedWord[] Related;
  /// <summary>If not equal no -1, indicates that this meaning only applies to the indicated headword or reading.</summary>
  public sbyte AppliesToHeadword, AppliesToReading;
  
  internal void Write(BinaryWriter writer, StringCompressor headwordCompressor, StringCompressor meaningCompressor)
  {
    writer.Write((byte)Glosses.Length);
    foreach(Gloss gloss in Glosses) gloss.Write(writer, meaningCompressor);
    
    writer.Write((byte)(Flags == null ? 0 : Flags.Length));
    if(Flags != null)
    {
      Debug.Assert(sizeof(SenseFlag) == 1);
      for(int i=0; i<Flags.Length; i++) writer.Write((byte)Flags[i]);
    }

    writer.Write((byte)(Related == null ? 0 : Related.Length));
    if(Related != null)
    {
      foreach(RelatedWord word in Related) word.Write(writer, headwordCompressor);
    }
    
    writer.Write(AppliesToHeadword);
    writer.Write(AppliesToReading);
  }
}

/// <summary>A structure representing a dictionary entry.</summary>
public struct Entry
{
  internal Entry(uint id, BinaryReader reader, StringCompressor headwordCompressor,
                 StringCompressor readingCompressor, StringCompressor meaningCompressor)
  {
    ID = id;

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
    
    length = reader.ReadByte();
    if(length == 0)
    {
      Meanings = null;
    }
    else
    {
      Meanings = new Meaning[length];
      for(int i=0; i<Meanings.Length; i++) Meanings[i] = new Meaning(reader, headwordCompressor, meaningCompressor);
    }
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

  internal void Write(BinaryWriter writer, StringCompressor headwordCompressor,
                      StringCompressor readingCompressor, StringCompressor meaningCompressor)
  {
    writer.Write((byte)Headwords.Length);
    foreach(Word word in Headwords) word.Write(writer, headwordCompressor);

    writer.Write((byte)(Readings == null ? 0 : Readings.Length));
    if(Readings != null)
    {
      foreach(Word word in Readings) word.Write(writer, readingCompressor);
    }

    writer.Write((byte)(Meanings == null ? 0 : Meanings.Length));
    if(Meanings != null)
    {
      foreach(Meaning meaning in Meanings) meaning.Write(writer, headwordCompressor, meaningCompressor);
    }
  }
}
#endregion

[Flags]
public enum SearchFlag
{
  None=0,
  SearchHeadwords=1, SearchReadings=2, SearchMeanings=4, SearchAll = SearchHeadwords|SearchReadings|SearchMeanings,
  MatchStart=8, MatchEnd=16, ExactMatch=MatchStart|MatchEnd, MatchMask=ExactMatch
}

#region Dictionary
public class WordDictionary : Dictionary, IDisposable
{
  public override string Name
  {
    get { return name; }
  }

  public void Dispose()
  {
    Unload();
  }

  public Entry GetEntryById(uint id)
  {
    if(dataStream != null)
    {
      reader.Position = id; // with the native data stream, entry IDs are their position within the file
      return new Entry(id, reader, headwordCompressor, readingCompressor, meaningCompressor);
    }
    else if(importedData != null)
    {
      return importedData[id];
    }
    else throw new InvalidOperationException("The dictionary has not been loaded.");
  }

  public IEnumerable<Entry> RetrieveAll()
  {
    return new EntryIterator(this);
  }

  public override IEnumerable<uint> Search(SearchPiece piece)
  {
    AssertLoaded();

    if(piece.Text == "") // if it's an empty string, return nothing
    {
      return EmptyIterator.Instance;
    }

    if((piece.Type & PieceType.Quoted) != 0)
    {
      throw new NotImplementedException();
    }

    List<IEnumerable<uint>> types = new List<IEnumerable<uint>>(3);
    if((piece.Flags & SearchFlag.SearchHeadwords) != 0)
    {
      types.Add(headwordIndex.Search(piece.Text, piece.Flags));
    }
    if((piece.Flags & SearchFlag.SearchReadings) != 0)
    {
      types.Add(readingIndex.Search(piece.Text, piece.Flags));
    }
    if((piece.Flags & SearchFlag.SearchMeanings) != 0)
    {
      types.Add(meaningIndex.Search(piece.Text, piece.Flags));
    }
    return DictionaryUtilities.GetUnion(types);
  }

  #region Loading and saving
  public void Load(string name, string indexFile, string dataFile)
  {
    if(name == null || indexFile == null || dataFile == null) throw new ArgumentNullException();
    FileStream indexStream = new FileStream(indexFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1,
                                            FileOptions.RandomAccess);
    FileStream dataStream  = new FileStream(dataFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1,
                                            FileOptions.RandomAccess);
    Load(name, indexStream, dataStream);
  }

  public void Load(string name, Stream indexStream, Stream dataStream)
  {
    if(name == null || indexStream == null || dataStream == null) throw new ArgumentNullException();
    Unload();

    byte[] magic = new byte[4];
    if(dataStream.Read(magic, 0, 4) < 4 || Encoding.ASCII.GetString(magic) != "DCWD")
    {
      throw new ArgumentException("Invalid dictionary file.");
    }
    if(dataStream.ReadByte() != 1)
    {
      throw new ArgumentException("This dictionary file was created with a different version of the "+
                                  "dictionary and cannot be opened.");
    }

    if(indexStream.Read(magic, 0, 4) < 4 || Encoding.ASCII.GetString(magic) != "DCWI")
    {
      throw new ArgumentException("Invalid dictionary file.");
    }
    if(indexStream.ReadByte() != 1)
    {
      throw new ArgumentException("This dictionary file was created with a different version of the "+
                                  "dictionary and cannot be opened.");
    }

    headwordIndex = CreateHeadwordIndex();
    readingIndex  = CreateReadingIndex();
    meaningIndex  = CreateMeaningIndex();
    using(BinaryReader reader = new BinaryReader(indexStream))
    {
      headwordIndex.Load(reader);
      readingIndex.Load(reader);
      meaningIndex.Load(reader);
    }
    
    using(BinaryReader reader = new BinaryReader(dataStream))
    {
      headwordCompressor.Load(reader);
      readingCompressor.Load(reader);
      meaningCompressor.Load(reader);
      reader.Skip(sizeof(int)); // skip entries.Count
      this.dataStart = (uint)reader.Position;
    }

    this.indexStream = indexStream;
    this.dataStream  = dataStream;
    this.reader      = new BinaryReader(dataStream, true, 32768, false);
    this.name        = name;
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

      if(entry.Meanings != null)
      {
        foreach(Meaning meaning in entry.Meanings)
        {
          foreach(Gloss gloss in meaning.Glosses) meaningCompressor.AddString(gloss.Text);

          if(meaning.Related != null)
          {
            foreach(RelatedWord word in meaning.Related) headwordCompressor.AddString(word.Word);
          }
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

      using(BinaryWriter writer = new BinaryWriter(dataStream))
      {
        headwordCompressor.Save(writer);
        readingCompressor.Save(writer);
        meaningCompressor.Save(writer);

        writer.Write(importedData.Count);

        foreach(Entry entry in entries)
        {
          uint position = (uint)writer.Position;
          entry.Write(writer, headwordCompressor, readingCompressor, meaningCompressor);
          idMap.Add(entry.ID, position);
        }
      }
    }

    using(indexStream)
    {
      indexStream.Write(Encoding.ASCII.GetBytes("DCWI"), 0, 4); // write the magic token (DiCtionary Word Index)
      indexStream.WriteByte(1);                                 // version number
      using(BinaryWriter writer = new BinaryWriter(indexStream))
      {
        headwordIndex.Save(writer, idMap);
        readingIndex.Save(writer, idMap);
        meaningIndex.Save(writer, idMap);
      }
    }
  }
  #endregion

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

    DictionaryUtilities.PopulateIndex(headwordIndex, headwords);
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

    DictionaryUtilities.PopulateIndex(readingIndex, readings);
  }

  void PopulateMeaningIndex(List<Entry> entries)
  {
    Dictionary<string, List<uint>> meanings = new Dictionary<string, List<uint>>();
    foreach(Entry entry in entries)
    {
      if(entry.Meanings != null)
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
    }

    DictionaryUtilities.PopulateIndex(meaningIndex, meanings);
  }

  static void AddHashIndexEntry(Dictionary<string,List<uint>> index, string key, uint entryID)
  {
    List<uint> list;
    if(!index.TryGetValue(key, out list)) index[key] = list = new List<uint>();
    list.Add(entryID); // the list must be sorted and duplicates removed later
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
    Utilities.Dispose(ref headwordIndex);
    Utilities.Dispose(ref readingIndex);
    Utilities.Dispose(ref meaningIndex);
    Utilities.Dispose(ref reader);
    Utilities.Dispose(ref dataStream);
    Utilities.Dispose(ref indexStream);
    importedData = null;
    name = null;
  }

  protected internal static void SortEntriesByFrequency(List<Entry> entries)
  {
    entries.Sort(FrequencyComparer.Instance); // sort them by frequency
  }

  #region EntryIterator
  sealed class EntryIterator : IEnumerable<Entry>
  {
    public EntryIterator(WordDictionary dictionary)
    {
      this.dictionary = dictionary;
    }
    
    sealed class EntryEnumerator : IEnumerator<Entry>
    {
      public EntryEnumerator(WordDictionary dictionary)
      {
        this.dictionary = dictionary;
        this.currentPos = dictionary.dataStart;
      }
      
      public Entry Current
      {
        get
        {
          if(!current.HasValue) throw new InvalidOperationException();
          return current.Value;
        }
      }
      
      public bool MoveNext()
      {
        dictionary.reader.Position = currentPos;
        if(dictionary.reader.Position == dictionary.reader.BaseStream.Length)
        {
          current = null;
          return false;
        }

        current = new Entry(currentPos, dictionary.reader, dictionary.headwordCompressor,
                            dictionary.readingCompressor, dictionary.meaningCompressor);
        currentPos = (uint)dictionary.reader.Position;
        return true;
      }
      
      public void Reset()
      {
        currentPos = dictionary.dataStart;
        current    = null;
      }

      object System.Collections.IEnumerator.Current
      {
        get { return Current; }
      }

      void IDisposable.Dispose() { }

      readonly WordDictionary dictionary;
      uint currentPos;
      Entry? current;
    }

    public IEnumerator<Entry> GetEnumerator()
    {
      return new EntryEnumerator(dictionary);
    }
    
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    readonly WordDictionary dictionary;
  }
  #endregion

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
  BinaryReader reader;
  string name;
  uint dataStart;

  static readonly Regex wordRE = new Regex(@"\w+", RegexOptions.CultureInvariant | RegexOptions.Singleline);
}
#endregion

} // namespace Jappy.Backend
