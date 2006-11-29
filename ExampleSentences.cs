using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Jappy
{

[Flags]
public enum ExampleFlag : byte
{
  Masculine=1, Feminine=2
}

public struct ExampleSentence
{
  public string Japanese;
  public string English;
  public ExampleFlag Flags;
}

[Flags]
public enum ExampleSearch
{
  Japanese=1, English=2
}

public class ExampleSentences
{
  public IEnumerable<uint> Search(string query)
  {
    throw new NotImplementedException();
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

    importedData = new List<ExampleSentence>();
    Dictionary<string,List<uint>> wordIndex = new Dictionary<string,List<uint>>();

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
            AddWord(wordIndex, word, exampleID);
          }

          // the japanese words are pre-split, but they have modifiers appended. for now we'll just strip off these
          // modifiers
          char[] modifierChars = new char[] { '(', '[', '{' };
          foreach(string encodedWord in bLine.Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
          {
            Match m = jpWordsRE.Match(encodedWord);
            if(!m.Success) continue; //goto badData;
            AddWord(wordIndex, m.Groups[1].Value, exampleID); // add the primary word
            if(m.Groups[2].Success)
            {
              AddWord(wordIndex, m.Groups[2].Value, exampleID); // and the reading, if any
            }
          }

          aLine = bLine = null; // clear the lines so we'll read in two fresh ones
        }
      }
    }

    
    return;
    
    badData:
    throw new ArgumentException("The stream does not contain valid Tanaka Corpus data.");
  }
  #endregion
  
  public void Load(string exampleFile)
  {
    Load(new FileStream(exampleFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1, FileOptions.RandomAccess));
  }

  public void Load(Stream exampleStream)
  {
    throw new NotImplementedException();
  }
  
  public void Save(string exampleFile)
  {
    Save(new FileStream(exampleFile, FileMode.Create, FileAccess.Write, FileShare.None, 1,
                        FileOptions.SequentialScan));
  }

  public void Save(Stream exampleStream)
  {
    throw new NotImplementedException();
  }

  List<ExampleSentence> importedData;
  StringCompressor jpCompressor = new StringCompressor(), enCompressor = new StringCompressor();
  
  static void AddWord(Dictionary<string,List<uint>> index, string word, uint id)
  {
    List<uint> ids;
    if(!index.TryGetValue(word, out ids))
    {
      index[word] = ids = new List<uint>();
    }

    ids.Add(id); // this does not check for duplicates. they'll have to be removed later.
  }

  static readonly Regex jpWordsRE =
    new Regex(@"^\w+(?:\(([\w\p{Pd}～・]+)\)|\[(\d+)\]|\{([^}]+)\})*$",
              RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
}

} // namespace Jappy