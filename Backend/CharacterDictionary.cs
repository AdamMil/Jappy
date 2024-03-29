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
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using BinaryReader = AdamMil.IO.BinaryReader;
using BinaryWriter = AdamMil.IO.BinaryWriter;

namespace Jappy.Backend
{

#region Kanji data structure
public enum LookupType : byte
{
  SpahnHadamitzky,
}

public enum Level : byte
{
  Unknown=0, First=1, Second, Third, Fourth, Fifth, Sixth, HighSchool=8, Names=9
}

public enum ReadingType : byte
{
  Unknown, PinYin, On, Kun, Nanori, English
}

public struct Reading
{
  public Reading(ReadingType type, string text)
  {
    Text = text;
    Type = type;
  }

  internal Reading(BinaryReader reader, StringCompressor compressor)
  {
    Debug.Assert(sizeof(ReadingType) == 1);
    Text = compressor.ReadString(reader);
    Type = (ReadingType)reader.ReadByte();
  }

  public string Text;
  public ReadingType Type;
  
  internal void Write(BinaryWriter writer, StringCompressor compressor)
  {
    compressor.WriteString(writer, Text);
    writer.Write((byte)Type);
  }
}

public struct Kanji
{
  internal Kanji(BinaryReader reader, StringCompressor compressor)
  {
    Debug.Assert(sizeof(Level) == 1);
    Readings = new Reading[reader.ReadByte()];
    for(int i=0; i<Readings.Length; i++) Readings[i] = new Reading(reader, compressor);

    SpahnLookup   = compressor.ReadString(reader);
    Character     = reader.ReadChar();
    Frequency     = reader.ReadUInt16();
    Level         = (Level)reader.ReadByte();
    Radical       = reader.ReadByte();
    NelsonRadical = reader.ReadByte();
    Strokes       = reader.ReadByte();
  }

  public Reading[] Readings;
  public string SpahnLookup;
  public char Character;
  public ushort Frequency;
  public Level Level;
  public byte Radical, NelsonRadical, Strokes;

  internal void Write(BinaryWriter writer, StringCompressor compressor)
  {
    writer.Write((byte)(Readings == null ? 0 : Readings.Length));
    if(Readings != null)
    {
      foreach(Reading reading in Readings) reading.Write(writer, compressor);
    }

    compressor.WriteString(writer, SpahnLookup);
    writer.Write(Character);
    writer.Write(Frequency);
    writer.Write((byte)Level);
    writer.Write(Radical);
    writer.Write(NelsonRadical);
    writer.Write(Strokes);
  }

  internal static char ReadCharacterAndSkip(BinaryReader reader, StringCompressor compressor)
  {
    Kanji k = new Kanji(reader, compressor);
    return k.Character;
  }
}
#endregion

#region CharacterDictionary
public class CharacterDictionary : IDisposable
{
  public ICollection<char> RetrieveAll()
  {
    AssertLoaded();
    return index.Keys;
  }

  public bool TryGetKanjiData(char kanji, out Kanji data)
  {
    if(kanjiReader != null)
    {
      uint position;
      if(index.TryGetValue(kanji, out position))
      {
        kanjiReader.Position = index[kanji];
        data = new Kanji(kanjiReader, compressor);
        return true;
      }
      else
      {
        data = new Kanji();
        return false;
      }
    }
    else if(importedData != null)
    {
      return importedData.TryGetValue(kanji, out data);
    }
    else throw new InvalidOperationException();
  }

  public void Dispose()
  {
    Unload();
  }

  public IEnumerable<uint> SearchByLookup(LookupType type, string code)
  {
    throw new NotImplementedException();
  }

  public IEnumerable<uint> SearchByRadical(byte radical)
  {
    throw new NotImplementedException();
  }

  public IEnumerable<uint> SearchByStrokes(byte strokes)
  {
    throw new NotImplementedException();
  }

  public void Load(string dictFile)
  {
    Load(new FileStream(dictFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1, FileOptions.RandomAccess));
  }

  public void Load(Stream dictStream)
  {
    Unload();

    byte[] magic = new byte[4];
    if(dictStream.Read(magic, 0, 4) < 4 || Encoding.ASCII.GetString(magic) != "DCCD")
    {
      throw new ArgumentException("Invalid dictionary file.");
    }
    if(dictStream.ReadByte() != 1)
    {
      throw new ArgumentException("This dictionary file was created with a newer version of the "+
                                  "dictionary and cannot be opened.");
    }

    using(BinaryReader reader = new BinaryReader(dictStream))
    {
      compressor.Load(reader);

      int count = reader.ReadInt32();
      index = new Dictionary<char,uint>(count);

      while(count-- != 0) // create an index mapping characters to their entry's position within the file
      {
        uint position = (uint)reader.Position;
        index.Add(Kanji.ReadCharacterAndSkip(reader, compressor), position);
      }
    }

    kanjiDataStream = dictStream;
    kanjiReader     = new BinaryReader(dictStream, true, 4096, true);
  }

  public void Save(string dictFile)
  {
    Save(new FileStream(dictFile, FileMode.Create, FileAccess.Write, FileShare.None, 1, FileOptions.SequentialScan));
  }

  public void Save(Stream dataStream)
  {
    if(importedData == null) throw new InvalidOperationException("Data has not been imported.");

    using(dataStream)
    {
      // add all strings to a string compressor
      compressor.CreateNew();
      foreach(Kanji kanji in importedData.Values)
      {
        compressor.AddString(kanji.SpahnLookup);

        if(kanji.Readings != null)
        {
          foreach(Reading reading in kanji.Readings)
          {
            compressor.AddString(reading.Text);
          }
        }
      }
      compressor.FinishedAdding();

      dataStream.Write(Encoding.ASCII.GetBytes("DCCD"), 0, 4); // write the magic token (DiCtionary Character Data)
      dataStream.WriteByte(1);                                 // version number

      using(BinaryWriter writer = new BinaryWriter(dataStream))
      {
        compressor.Save(writer);
        writer.Write(importedData.Count);
        foreach(Kanji kanji in importedData.Values)
        {
          kanji.Write(writer, compressor);
        }
      }
    }
  }

  #region KanjiDic XML importing
  public void ImportKanjiDicXml(string xmlFile)
  {
    ImportKanjiDicXml(File.Open(xmlFile, FileMode.Open, FileAccess.Read, FileShare.Read));
  }
  
  public void ImportKanjiDicXml(Stream xmlFile)
  {
    XmlReaderSettings settings = new XmlReaderSettings();
    settings.IgnoreComments   = true;
    settings.IgnoreWhitespace = true;
    settings.ProhibitDtd      = false;

    XmlReader reader = XmlReader.Create(xmlFile, settings);
    reader.ReadToDescendant("character");
    
    importedData = new Dictionary<char,Kanji>();
    while(reader.NodeType == XmlNodeType.Element)
    {
      Kanji kanji = new Kanji();
      foreach(XmlReader child in EnumerateChildNodes(reader))
      {
        SetKanjiDicProperties(ref kanji, child);
      }
      if(kanji.Character != 0)
      {
        importedData.Add(kanji.Character, kanji);
      }
    }
  }

  static void SetKanjiDicProperties(ref Kanji kanji, XmlReader reader)
  {
    switch(reader.LocalName)
    {
      case "codepoint":
        foreach(XmlReader child in EnumerateChildNodes(reader))
        {
          string type = child.GetAttribute("cp_type");
          if(type == "ucs")
          {
            string point = child.ReadElementContentAsString();
            if(point.Length <= 4) // skip kanji that can't be represented in UCS2
            {
              int num = 0;
              for(int i=0; i<point.Length; i++)
              {
                char c = char.ToLowerInvariant(point[i]);
                num = (num<<4) + (c>='a' ? c-'a'+10 : c-'0');
              }
              kanji.Character = (char)num;
            }
          }
          else
          {
            child.Skip();
          }
        }
        break;
      
      case "radical":
        foreach(XmlReader child in EnumerateChildNodes(reader))
        {
          string type = child.GetAttribute("rad_type");
          byte value = byte.Parse(child.ReadElementContentAsString());
          if(type == "classical") kanji.Radical = value;
          else if(type == "nelson_c") kanji.NelsonRadical = value;
        }
        break;
      
      case "misc":
        foreach(XmlReader child in EnumerateChildNodes(reader))
        {
          bool gotStroke = false;
          switch(child.LocalName)
          {
            case "grade":
              kanji.Level = (Level)byte.Parse(reader.ReadElementContentAsString());
              break;
            
            case "stroke_count":
              if(!gotStroke)
              {
                kanji.Strokes = byte.Parse(reader.ReadElementContentAsString());
                gotStroke = true;
              }
              else
              {
                child.Skip();
              }
              break;

            case "freq":
              kanji.Frequency = ushort.Parse(reader.ReadElementContentAsString());
              break;

            default:
              child.Skip();
              break;
          }
        }
        break;

      case "query_code":
        foreach(XmlReader code in EnumerateChildNodes(reader))
        {
          string type = code.GetAttribute("qc_type");
          if(type == "sh_desc")
          {
            kanji.SpahnLookup = reader.ReadElementContentAsString();
          }
          else
          {
            code.Skip();
          }
        }
        break;

      case "reading_meaning":
      {
        List<Reading> readings = new List<Reading>();
        foreach(XmlReader rm in EnumerateChildNodes(reader))
        {
          if(rm.LocalName == "nanori")
          {
            readings.Add(new Reading(ReadingType.Nanori, reader.ReadElementContentAsString()));
          }
          else if(rm.LocalName == "rmgroup")
          {
            foreach(XmlReader reading in EnumerateChildNodes(rm))
            {
              if(reading.LocalName == "reading")
              {
                string type = reading.GetAttribute("r_type");
                string text = reading.ReadElementContentAsString();
                ReadingType rtype;

                if(type == "pinyin") rtype = ReadingType.PinYin;
                else if(type == "ja_on") rtype = ReadingType.On;
                else if(type == "ja_kun") rtype = ReadingType.Kun;
                else rtype = ReadingType.Unknown;

                if(rtype != ReadingType.Unknown)
                {
                  readings.Add(new Reading(rtype, text));
                }
              }
              else if(reading.LocalName == "meaning")
              {
                string language = reading.GetAttribute("m_lang");
                if(string.IsNullOrEmpty(language) || language == "en")
                {
                  readings.Add(new Reading(ReadingType.English, reading.ReadElementContentAsString()));
                }
                else
                {
                  reading.Skip();
                }
              }
              else throw new NotImplementedException();
            }
          }
          else throw new NotImplementedException();
        }

        kanji.Readings = readings.ToArray();
        break;
      }

      default:
        reader.Skip();
        break;
    }
  }

  static IEnumerable<XmlReader> EnumerateChildNodes(XmlReader reader)
  {
    reader.ReadStartElement();
    while(reader.NodeType == XmlNodeType.Element)
    {
      yield return reader;
    }
    reader.ReadEndElement();
  }
  #endregion

  void AssertLoaded()
  {
    if(index == null)
    {
      throw new InvalidOperationException("The dictionary has not been loaded.");
    }
    Debug.Assert(kanjiDataStream != null && kanjiReader != null);
  }

  void Unload()
  {
    Utilities.Dispose(ref kanjiReader);
    Utilities.Dispose(ref kanjiDataStream);
    index = null;
  }

  Dictionary<char,Kanji> importedData;
  Dictionary<char,uint> index;
  StringCompressor compressor = new StringCompressor();
  Stream kanjiDataStream;
  BinaryReader kanjiReader;
}
#endregion

} // namespace Jappy.Backend