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

#region Search strategy
[Flags]
public enum PieceType
{
  Normal=0, Subtractive=1, Quoted=2
}

public struct SearchPiece
{
  public SearchPiece(string text, PieceType type, SearchFlag flags)
  {
    Text  = text;
    Type  = type;
    Flags = flags;
  }

  public string Text;
  public PieceType Type;
  public SearchFlag Flags;
}

public abstract class SearchStrategy
{
  public abstract IEnumerable<uint> Search(Dictionary dictionary, string query, SearchFlag flags);
  public abstract SearchPiece[] SplitQuery(string query, SearchFlag flags);
}

#region DefaultSearchStrategy
public class DefaultSearchStrategy : SearchStrategy
{
  public override IEnumerable<uint> Search(Dictionary dictionary, string query, SearchFlag flags)
  {
    List<IEnumerable<uint>> positives = new List<IEnumerable<uint>>();
    List<IEnumerable<uint>> negatives = new List<IEnumerable<uint>>();

    foreach(SearchPiece piece in SplitQuery(query, flags))
    {
      List<IEnumerable<uint>> addTo = (piece.Type & PieceType.Subtractive) == 0 ? positives : negatives;
      addTo.Add(dictionary.Search(piece));
    }

    if(positives.Count == 0) throw new ArgumentException("No positive search items in this query.");
    IEnumerable<uint> idIterator = DictionaryUtilities.GetIntersection(positives);

    if(negatives.Count != 0)
    {
      IEnumerable<uint> negative = DictionaryUtilities.GetUnion(negatives);
      idIterator = new SubtractionIterator(idIterator, negative);
    }

    return idIterator;
  }

  public override SearchPiece[] SplitQuery(string query, SearchFlag flags)
  {
    List<SearchPiece> pieces = new List<SearchPiece>();

    Match match = SplitRegex.Match(query);
    while(match.Success)
    {
      SearchPiece piece = new SearchPiece();
      piece.Text  = match.Value;
      piece.Flags = flags;

      if(piece.Text[0] == '-')
      {
        piece.Text  = piece.Text.Substring(1);
        piece.Type |= PieceType.Subtractive;
      }

      if(piece.Text[0] == '"')
      {
        piece.Text  = piece.Text.Substring(1, piece.Text.Length-2);
        piece.Type |= PieceType.Quoted;
      }

      PreprocessSearchPiece(ref piece);
      pieces.Add(piece);

      match = match.NextMatch();
    }

    return pieces.ToArray();
  }

  public static readonly DefaultSearchStrategy Instance = new DefaultSearchStrategy();

  protected virtual Regex SplitRegex
  {
    get { return splitRE; }
  }

  protected virtual void PreprocessSearchPiece(ref SearchPiece piece)
  {
    if((piece.Type & PieceType.Quoted) == 0)
    {
      if(piece.Text.EndsWith("*"))
      {
        piece.Text = piece.Text.Substring(0, piece.Text.Length-1);
        piece.Flags &= ~SearchFlag.MatchEnd;
      }
      if(piece.Text.StartsWith("*"))
      {
        piece.Text = piece.Text.Substring(1);
        piece.Flags &= ~SearchFlag.MatchStart;
      }
    }
  }

  static readonly Regex splitRE = new Regex(@"-?(?:""[^""]+""|\*?\w+\*?)",
                                            RegexOptions.CultureInvariant | RegexOptions.Singleline);
}
#endregion
#endregion

#region Dictionary
public abstract class Dictionary
{
  public abstract string Name { get; }
  public abstract IEnumerable<uint> Search(SearchPiece piece);
}
#endregion

#region DictionaryUtilities
public static class DictionaryUtilities
{
  public static IEnumerable<uint> GetIntersection(IList<IEnumerable<uint>> enumerables)
  {
    return enumerables.Count == 0 ? EmptyIterator.Instance :
           enumerables.Count == 1 ? enumerables[0]         :
           new IntersectionIterator(enumerables);
  }

  public static IEnumerable<uint> GetUnion(IList<IEnumerable<uint>> enumerables)
  {
    return enumerables.Count == 0 ? EmptyIterator.Instance :
           enumerables.Count == 1 ? enumerables[0]         :
           new UnionIterator(enumerables);
  }

  public static void PopulateIndex(Index index, Dictionary<string,List<uint>> idMap)
  {
    index.CreateNew();

    List<uint> sortedList = new List<uint>();
    foreach(KeyValuePair<string,List<uint>> pair in idMap)
    {
      sortedList.AddRange(pair.Value);
      DictionaryUtilities.SortAndRemoveDuplicates(sortedList);
      index.Add(pair.Key, sortedList.ToArray());
      sortedList.Clear();
    }
    
    index.FinishedAdding();
  }
  
  public static void SortAndRemoveDuplicates(List<uint> ids)
  {
    ids.Sort(); // sort the array
    uint lastKey = ~ids[ids.Count-1]; // set the last key to something other than the first key
    for(int i=ids.Count-1; i>=0; i--) // and remove duplicates
    {
      uint key = ids[i];
      if(key != lastKey) lastKey = key;
      else ids.RemoveAt(i);
    }
  }
}
#endregion

} // namespace Jappy.Backend