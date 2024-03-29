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

[Flags]
public enum ExampleFlag : byte
{
  Masculine=1, Feminine=2
}

public struct ExampleSentence
{
  public ExampleSentence(BinaryReader reader, StringCompressor jpCompressor, StringCompressor enCompressor)
  {
    Debug.Assert(sizeof(ExampleFlag) == 1);
    Japanese = jpCompressor.ReadString(reader);
    English  = enCompressor.ReadString(reader);
    Flags    = (ExampleFlag)reader.ReadByte();
  }

  public string Japanese;
  public string English;
  public ExampleFlag Flags;

  internal void Write(BinaryWriter writer, StringCompressor jpCompressor, StringCompressor enCompressor)
  {
    Debug.Assert(sizeof(ExampleFlag) == 1);
    jpCompressor.WriteString(writer, Japanese);
    enCompressor.WriteString(writer, English);
    writer.Write((byte)Flags);
  }
}

[Flags]
public enum ExampleSearch
{
  Japanese=1, English=2
}

public class ExampleSentences : Dictionary, IDisposable
{
  public ExampleSentences()
  {
    Initialize();
  }

  public override string Name
  {
    get { return "Example Sentences"; }
  }  

  public void Dispose()
  {
    Unload();
  }

  public ExampleSentence GetExampleById(uint id)
  {
    if(sentenceReader == null) throw new InvalidOperationException();
    sentenceReader.Position = id;
    return new ExampleSentence(sentenceReader, jpCompressor, enCompressor);
  }

  public void Load(string exampleFile)
  {
    Load(new FileStream(exampleFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1, FileOptions.RandomAccess));
  }

  public void Load(Stream exampleStream)
  {
    Unload();
    
    using(BinaryReader reader = new BinaryReader(exampleStream))
    {
      reader.Skip(reader.ReadInt32()); // skip over the data

      jpIndex.Load(reader);
      enIndex.Load(reader);
      
      jpCompressor.Load(reader);
      enCompressor.Load(reader);
    }
    
    sentenceReader = new BinaryReader(exampleStream, true, 32768, true);
  }
  
  public void Save(string exampleFile)
  {
    Save(new FileStream(exampleFile, FileMode.Create, FileAccess.Write, FileShare.None, 1,
                        FileOptions.SequentialScan));
  }

  public void Save(Stream exampleStream)
  {
    if(importedData == null) throw new InvalidOperationException("Examples have not been imported.");

    using(exampleStream)
    using(BinaryWriter writer = new BinaryWriter(exampleStream))
    {
      Dictionary<uint, uint> idMap = new Dictionary<uint, uint>();

      uint startPosition = (uint)writer.Position;
      writer.Write((uint)0); // add a length of data to skip to get to the indexes
      writer.Write(importedData.Count);
      for(int i=0; i<importedData.Count; i++)
      {
        idMap[(uint)i] = (uint)writer.Position; // map the sentence IDs from ordinal to stream position
        importedData[i].Write(writer, jpCompressor, enCompressor);
      }
      uint endPosition = (uint)writer.Position; // go back and update the data length in bytes
      writer.Position = startPosition;
      writer.Write(endPosition - startPosition - sizeof(uint));
      writer.Position = endPosition;

      jpIndex.Save(writer, idMap);
      enIndex.Save(writer, idMap);
      
      jpCompressor.Save(writer);
      enCompressor.Save(writer);
    }
  }

  public override IEnumerable<uint> Search(SearchPiece piece)
  {
    List<IEnumerable<uint>> types = new List<IEnumerable<uint>>(2);

    if((piece.Flags & (SearchFlag.SearchHeadwords|SearchFlag.SearchReadings)) != 0)
    {
      types.Add(jpIndex.Search(piece.Text, piece.Flags));
    }
    if((piece.Flags & SearchFlag.SearchMeanings) != 0)
    {
      types.Add(enIndex.Search(piece.Text, piece.Flags));
    }

    return DictionaryUtilities.GetUnion(types);
  }

  #region Importing the modified Tanaka corpus
  public void ImportModifiedTanakaCorpusInUTF8(string corpusFile)
  {
    ImportModifiedTanakaCorpusInUTF8(new FileStream(corpusFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                                                    FileOptions.SequentialScan));
  }

  public void ImportModifiedTanakaCorpusInUTF8(Stream corpusStream)
  {
    ImportModifiedTanakaCorpus(new StreamReader(corpusStream, Encoding.UTF8));
  }

  public void ImportModifiedTanakaCorpus(TextReader reader)
  {
    if(reader == null) throw new ArgumentNullException();
    Unload();

    importedData = new List<ExampleSentence>();
    jpCompressor.CreateNew();
    enCompressor.CreateNew();

    Dictionary<string, List<uint>> jpWordIndex = new Dictionary<string, List<uint>>();
    Dictionary<string, List<uint>> enWordIndex = new Dictionary<string, List<uint>>();
    StringBuilder sb = new StringBuilder();

    using(reader)
    {
      string aLine=null, bLine=null;
      char[] sentenceSplitChar = new char[] { '\t' };

      while(true)
      {
        string line = reader.ReadLine();
        if(line == null) break; // break on EOF
        if(line.Length == 0 || line[0] == '#') continue; // skip blank lines and comment lines

        if(aLine == null)
        {
          if(!line.StartsWith("A: ")) goto badData;
          aLine = line.Substring(3);
        }
        else if(bLine == null)
        {
          if(!line.StartsWith("B: ")) goto badData;
          bLine = line.Substring(3);

          // create the example and add it to the imported data
          ExampleSentence example = new ExampleSentence();

          while(true) // trim modifiers off of the first line
          {
            if(aLine.EndsWith(" [M]"))
            {
              example.Flags |= ExampleFlag.Masculine;
              aLine = aLine.Substring(0, aLine.Length-4);
            }
            else if(aLine.EndsWith(" [F]"))
            {
              example.Flags |= ExampleFlag.Feminine;
              aLine = aLine.Substring(0, aLine.Length-4);
            }
            else break; // break when no modifiers are left
          }

          string[] bits = aLine.Split(sentenceSplitChar);
          if(bits.Length != 2) goto badData;

          example.Japanese = bits[0];
          example.English  = bits[1];
          importedData.Add(example);
          jpCompressor.AddString(example.Japanese);
          enCompressor.AddString(example.English);

          uint exampleID = (uint)(importedData.Count-1);

          // then update the indices.
          // strip all non word characters from the example english and lowercase the text.
          sb.Length = 0;
          foreach(char c in example.English)
          {
            if(char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c=='\'') sb.Append(char.ToLowerInvariant(c));
          }
          // split the text on whitespace to obtain the english words
          foreach(string word in sb.ToString().Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
          {
            AddWord(enWordIndex, word, exampleID);
          }

          // the japanese words are pre-split, but they have modifiers appended. for now we'll just strip off these
          // modifiers
          char[] modifierChars = new char[] { '(', '[', '{' };
          foreach(string encodedWord in bLine.Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
          {
            Match m = jpWordsRE.Match(encodedWord);
            if(!m.Success) goto badData;
            AddWord(jpWordIndex, m.Groups[1].Value, exampleID); // add the primary word
            if(m.Groups[2].Success)
            {
              AddWord(jpWordIndex, m.Groups[2].Value, exampleID); // and the reading, if any
            }
          }

          aLine = bLine = null; // clear the lines so we'll read in two fresh ones
        }
      }
    }

    jpCompressor.FinishedAdding();
    enCompressor.FinishedAdding();
    DictionaryUtilities.PopulateIndex(jpIndex, jpWordIndex);
    DictionaryUtilities.PopulateIndex(enIndex, enWordIndex);
    return;

    badData:
    Unload();
    throw new ArgumentException("The stream does not contain valid Tanaka Corpus data.");
  }
  #endregion

  void Initialize()
  {
    if(jpIndex != null) jpIndex.Dispose();
    jpIndex = new JapaneseDictionary.JpDiskHashIndex();

    if(enIndex != null) enIndex.Dispose();
    enIndex = new TrieIndex();
    
    jpCompressor = new StringCompressor();
    enCompressor = new StringCompressor();
  }
  
  void Unload()
  {
    Utilities.Dispose(ref sentenceReader);
    Utilities.Dispose(ref exampleStream);
    importedData = null;

    Initialize();
  }

  Index jpIndex, enIndex;
  List<ExampleSentence> importedData;
  StringCompressor jpCompressor, enCompressor;

  BinaryReader sentenceReader;
  Stream exampleStream;
  
  static void AddWord(Dictionary<string,List<uint>> index, string word, uint id)
  {
    Debug.Assert(!string.IsNullOrEmpty(word));

    List<uint> ids;
    if(!index.TryGetValue(word, out ids))
    {
      index[word] = ids = new List<uint>();
    }

    ids.Add(id); // this does not sort or check for duplicates. that will need to be done later
  }

  static readonly Regex jpWordsRE =
    new Regex(@"^(\w+)(?:\(([\w\p{Pd}～・]+)\)|\[(\d+)\]|\{([^}]+)\})*$",
              RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
}

} // namespace Jappy.Backend