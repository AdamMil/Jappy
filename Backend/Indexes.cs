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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BinaryReader = AdamMil.IO.BinaryReader;
using BinaryWriter = AdamMil.IO.BinaryWriter;

namespace Jappy.Backend
{

#region Index
public abstract class Index : IDisposable
{
  public virtual void Dispose()
  {
    Unload();
  }

  public virtual void CreateNew()
  {
    Unload();
    inMemoryData = new Dictionary<string, uint[]>();
  }

  public virtual void Add(string key, uint[] sortedIds)
  {
    key = NormalizeKey(key);
    if(string.IsNullOrEmpty(key)) throw new ArgumentException("Normalized key must not be empty.");

    uint[] ids;
    if(inMemoryData.TryGetValue(key, out ids)) // if the data already exists, combine the id arrays
    {
      if(ids.Length == 1 && sortedIds.Length == 1 && ids[0] == sortedIds[0]) // handle the special case of identical
      {                                                                      // one-length arrays, which seems common
        return;
      }

      ids = new List<uint>(new UnionIterator(ids, sortedIds)).ToArray();
    }
    else
    {
      ids = sortedIds;
    }

    inMemoryData[key] = ids;
  }

  public virtual void FinishedAdding() { }

  public abstract void Load(BinaryReader reader);
  public abstract void Save(BinaryWriter writer, Dictionary<uint,uint> idMap);

  public abstract IEnumerable<uint> Search(string query, SearchFlag flags);

  protected virtual string NormalizeKey(string key)
  {
    if(string.IsNullOrEmpty(key)) return null;
    return key.ToLowerInvariant();
  }
  
  protected virtual void Unload()
  {
    inMemoryData = null;
  }
  
  protected Dictionary<string,uint[]> inMemoryData;
}
#endregion

#region DiskHashIndex
public class DiskHashIndex : Index
{
  public override void Load(BinaryReader reader)
  {
    Unload();

    // read the table size and key data length
    this.tableSize = reader.ReadUInt32();
    int dataLength = reader.ReadInt32();
    this.tableStart = (uint)reader.Position;

    reader.Skip(dataLength); // now skip over the data so the next object can load properly
    this.smallReader = new BinaryReader(reader.BaseStream, true, 4096, true);
    this.bigReader   = new BinaryReader(reader.BaseStream, true, 32768, true);
  }

  public override void Save(BinaryWriter writer, Dictionary<uint,uint> idMap)
  {
    if(inMemoryData == null) throw new InvalidOperationException("Data has not been added to the hash.");

    const double targetFullness = 0.72;
    uint tableSize = 0;
    for(int i=0,threshold=(int)Math.Round(inMemoryData.Count/targetFullness); i<primes.Length; i++)
    {
      if(primes[i] > threshold)
      {
        tableSize = primes[i];
        break;
      }
    }
    if(tableSize == 0) throw new InvalidOperationException("Too many keys.");

    /* The hash table will be stored on disk using open hashing with each bucket having the given layout:
       FIELD        TYPE      DESCRIPTION
       keyLength    ushort    The length of the headword.
       key0         char      The first character of the headword.
       keyIndex     uint      The index of the headword within the file, stored in little-endian UCS2 unicode.
                              For keys of length one, this field is invalid. The key is stored within the 'key0' field.
       dataIndex    uint      If the high bit is set, the lower 31 bits indicate an index into the data file where an
                              array of entry IDs will be stored. Otherwise, the lower 31 bits are the entry ID itself.
    */

    // first we'll figure out where the buckets will go. this information will be used to optimize the layout of the
    // string and array tables
    string[] buckets = new string[tableSize];
    uint keyDataLength = 0, arrayDataLength = 0;
    foreach(KeyValuePair<string,uint[]> pair in inMemoryData)
    {
      uint index = (uint)pair.Key.GetHashCode() % tableSize; // this is the ideal place for the bucket
      while(buckets[(int)index] != null) // but because we're using open hashing, we'll need to do a linear search
      {                                  // until we find an unoccupied bucket. there's guaranteed to be one.
        if(++index == tableSize) index = 0;
      }
      buckets[index] = pair.Key;

      if(pair.Key.Length > 1) // one-length keys are not stored in the key data area
      {
        if(pair.Key.Length > ushort.MaxValue)
        {
          throw new ArgumentException("Normalized keys must be <= 65535 characters long.");
        }
        keyDataLength += (uint)pair.Key.Length*sizeof(char); // while we're at it, keep track of the key length in bytes
      }
      
      if(pair.Value.Length > 1) // one-length arrays are not stored in the array data area
      {
        arrayDataLength += (uint)pair.Value.Length*sizeof(uint) + sizeof(int);
      }
    }

    writer.Write(tableSize); // write the table size, in buckets
    writer.Write(tableSize*BucketSize + keyDataLength + arrayDataLength); // write the total length of data for easy skipping

    uint keyDataStart   = (uint)writer.Position + tableSize*BucketSize; // the key data starts after the table
    uint arrayDataStart = keyDataStart + keyDataLength; // the array data starts after the key data

    // first write the bucket data
    byte[] emptyBucket = new byte[BucketSize]; // all-zero bytes for an empty bucket
    for(int i=0; i<buckets.Length; i++)
    {
      string key = buckets[i];
      if(key == null)
      {
        writer.Write(emptyBucket);
      }
      else
      {
        uint[] array = inMemoryData[key];

        writer.Write((ushort)key.Length);
        writer.Write(key[0]);

        if(key.Length == 1) // add the index of the key data. one-length keys are not stored separately
        {
          writer.Write((uint)0);
        }
        else
        {
          writer.Write(keyDataStart);
          keyDataStart += (uint)key.Length*sizeof(char);
        }

        if(array.Length == 1) // add the index of the array data. one-length arrays are stored inline in this slot
        {
          uint id = array[0];
          writer.Write(idMap == null ? id : idMap[id]);
        }
        else
        {
          writer.Write(arrayDataStart | 0x80000000); // the high bit set indicates that it points to external array data
          arrayDataStart += (uint)(array.Length*sizeof(uint) + sizeof(uint));
        }
      }
    }

    // now write the key string data
    for(int i=0; i<buckets.Length; i++)
    {
      string key = buckets[i];
      if(key != null && key.Length > 1) writer.Write(key); // one-length keys are not stored separately
    }

    // now write the array data
    for(int i=0; i<buckets.Length; i++)
    {
      string key = buckets[i];
      if(key == null) continue;

      uint[] data = inMemoryData[key];

      if(idMap != null)
      {
        data = (uint[])data.Clone();
        for(int j=0; j<data.Length; j++)
        {
          data[j] = idMap[data[j]];
        }
      }

      if(data.Length != 1) // one-length arrays are not stored separately
      {
        writer.Write(data.Length);
        writer.Write(data);
      }
    }
  }

  public override IEnumerable<uint> Search(string query, SearchFlag flags)
  {
    if(smallReader == null) throw new InvalidOperationException("The index has not been loaded.");

    query = NormalizeKey(query);
    if(query == null) return EmptyIterator.Instance;

    flags &= SearchFlag.MatchMask;
    return flags == SearchFlag.ExactMatch ?
      new ExactMatchIterator(query, this) : (IEnumerable<uint>)new SearchIterator(query, this, flags);
  }

  protected override void Unload()
  {
    Utilities.Dispose(ref smallReader);
    Utilities.Dispose(ref bigReader);

    base.Unload();
  }

  const uint BucketSize = 12;

  #region ExactMatchIterator
  sealed class ExactMatchIterator : IEnumerable<uint>
  {
    public ExactMatchIterator(string key, DiskHashIndex index)
    {
      this.key   = key;
      this.index = index;
    }

    public unsafe IEnumerator<uint> GetEnumerator()
    {
      char* bucketKey = stackalloc char[key.Length];
      uint bucketIndex = (uint)key.GetHashCode() % index.tableSize;

      index.smallReader.Position = index.tableStart + bucketIndex*BucketSize;
      while(true)
      {
        ushort keyLength = index.smallReader.ReadUInt16();
        if(keyLength == 0) // if the key was not found in the hash table, return an empty enumerator
        {
          return EmptyIterator.Instance.GetEnumerator();
        }

        if(keyLength != key.Length) // if the key lengths don't match, advance to the next bucket
        {
          index.smallReader.Skip((int)BucketSize - sizeof(ushort));
          goto nextBucket;
        }

        if(key[0] != index.smallReader.ReadChar()) // if the first letters of the keys don't match, advance
        {
          index.smallReader.Skip((int)BucketSize - (sizeof(ushort)+sizeof(char)));
          goto nextBucket;
        }

        // the length and first characters match. see if the rest of the key matches
        if(keyLength == 1) // if the key length is one, the key already matches
        {
          index.smallReader.Skip(sizeof(uint)); // skip the "key index", which contains nothing useful for one-length keys
        }
        else // otherwise, read the key index and do a character-by-character comparison
        {
          uint dataIndex = index.smallReader.ReadUInt32();
          uint currentPosition = (uint)index.smallReader.Position;

          index.smallReader.Position = dataIndex;
          index.smallReader.ReadChar(bucketKey, keyLength);
          index.smallReader.Position = currentPosition;

          for(int i=1; i<keyLength; i++)
          {
            if(bucketKey[i] != key[i])
            {
              index.smallReader.Skip((int)BucketSize - (sizeof(ushort)+sizeof(char)+sizeof(uint)));
              goto nextBucket;
            }
          }
        }
        
        // at this point, the keys match. return the data.
        {
          uint dataIndex = index.smallReader.ReadUInt32();
          if((dataIndex & 0x80000000) == 0) // if the high bit is clear, the data is a single uint stored inline
          {
            return new SingleItemIterator(dataIndex).GetEnumerator();
          }
          else
          {
            uint currentPosition  = (uint)index.smallReader.Position;
            index.smallReader.Position = dataIndex&0x7FFFFFFF;
            uint[] data = index.smallReader.ReadUInt32(index.smallReader.ReadInt32());
            return ((IEnumerable<uint>)data).GetEnumerator();
          }
        }

        nextBucket:
        if(++bucketIndex == index.tableSize) // if we've reached the end of the table, jump back to the start
        {
          index.smallReader.Position = index.tableStart;
        }
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    readonly string key;
    readonly DiskHashIndex index;
  }
  #endregion

  #region SearchIterator
  sealed class SearchIterator : IEnumerable<uint>
  {
    public SearchIterator(string key, DiskHashIndex index, SearchFlag searchType)
    {
      this.key        = key;
      this.index      = index;
      this.searchType = searchType & SearchFlag.MatchMask;
    }

    public unsafe IEnumerator<uint> GetEnumerator()
    {
      List<IEnumerable<uint>> arrays = null;
      List<uint> singles = null;

      char[] bucketKey = new char[16];

      index.bigReader.Position = index.tableStart;
      for(int bucketIndex=0; bucketIndex<index.tableSize; bucketIndex++) // scan through the entire table
      {
        Debug.Assert(index.bigReader.Position == index.tableStart+bucketIndex*BucketSize);
        ushort keyLength = index.bigReader.ReadUInt16();
        if(keyLength < key.Length) // skip empty buckets and buckets with keys that are too short to possibly match
        {
          index.bigReader.Skip((int)BucketSize - sizeof(ushort));
          continue;
        }

        // if it's a "Starts With" match and the first characters don't match, move to the next bucket
        if(searchType == SearchFlag.MatchStart)
        {
          if(key[0] != index.bigReader.ReadChar())
          {
            index.bigReader.Skip((int)BucketSize - (sizeof(ushort)+sizeof(char)));
            continue;
          }
          else if(key.Length == 1) // if the key is of length one (a common case when searching for a starting kanji),
          {                        // and the match is "starts with", then we know we've got a match.
            index.bigReader.Skip(sizeof(uint)); // so we can skip the string index
            goto foundMatch; // and go right to the match-finding code
          }
        }
        else // we have to make sure to advance the reader in any case
        {
          index.bigReader.Skip(sizeof(char));
        }

        // it may match, but we need to read the key index and do a character-by-character comparison
        if(keyLength > bucketKey.Length)
        {
          int newSize = bucketKey.Length;
          do newSize *= 2; while(newSize < keyLength);
          bucketKey = new char[newSize];
        }

        index.smallReader.Position = index.bigReader.ReadUInt32();
        index.smallReader.ReadChar(bucketKey, 0, keyLength);

        bool found = true;
        if(searchType == SearchFlag.MatchStart)
        {
          for(int i=1; i<key.Length; i++)
          {
            if(bucketKey[i] != key[i])
            {
              found = false;
              break;
            }
          }
        }
        else if(searchType == SearchFlag.MatchEnd)
        {
          for(int i=key.Length-1,j=keyLength-1; i>=0; j--,i--)
          {
            if(bucketKey[j] != key[i])
            {
              found = false;
              break;
            }
          }
        }
        else // do a "contains" search
        {
          char first = key[0];
          for(int start=0,end=keyLength-key.Length; start<end; start++)
          {
            if(bucketKey[start] == first) // if we find what might be the start of a match...
            {
              for(int i=1; i<key.Length; i++) // check to see if the rest of the key matches
              {
                if(bucketKey[start+i] != key[i]) goto continueOuter; // if not, continue searching in this key
              }
              goto doesContainIt;
            }
            continueOuter:;
          }
          found = false;
          doesContainIt:;
        }
        
        if(!found)
        {
          index.bigReader.Skip((int)BucketSize - (sizeof(ushort)+sizeof(char)+sizeof(uint)));
          continue; // next bucket
        }

        foundMatch:
        // at this point, the keys match. we want to return the data.
        uint dataIndex = index.bigReader.ReadUInt32();
        if((dataIndex & 0x80000000) == 0) // if the high bit is clear, the data is a single uint stored inline
        {
          if(singles == null) singles = new List<uint>();
          int newPos = singles.BinarySearch(dataIndex);
          if(newPos < 0) singles.Insert(~newPos, dataIndex);
        }
        else
        {
          index.smallReader.Position = dataIndex & 0x7FFFFFFF;
          if(arrays == null) arrays = new List<IEnumerable<uint>>();
          arrays.Add(index.smallReader.ReadUInt32(index.smallReader.ReadInt32()));
        }
      }
      
      if(arrays == null)
      {
        if(singles == null) return EmptyIterator.Instance.GetEnumerator();
        else return singles.GetEnumerator();
      }
      else
      {
        if(singles != null) arrays.Add(singles);
        return (arrays.Count == 1 ? arrays[0] : new UnionIterator(arrays)).GetEnumerator();
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    readonly string key;
    readonly DiskHashIndex index;
    readonly SearchFlag searchType;
  }
  #endregion

  BinaryReader smallReader, bigReader;
  uint tableStart, tableSize;

  static readonly uint[] primes = new uint[]
  {
    11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
    1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
    17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363,
    156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
    968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 
    4999559, 5999471, 7199369
  };
}
#endregion

#region MemoryHashIndex
public class MemoryHashIndex : Index
{
  public override void Load(BinaryReader reader)
  {
    Unload();

    int count = reader.ReadInt32();
    inMemoryData = new Dictionary<string, uint[]>(count);

    while(count-- != 0)
    {
      inMemoryData.Add(reader.ReadStringWithLength(), reader.ReadUInt32(reader.ReadInt32()));
    }
  }

  public override void Save(BinaryWriter writer, Dictionary<uint,uint> idMap)
  {
    if(inMemoryData == null) throw new InvalidOperationException();

    int largestArraySize = 0;
    foreach(uint[] array in inMemoryData.Values)
    {
      if(array.Length > largestArraySize) largestArraySize = array.Length;
    }
    uint[] buffer = new uint[largestArraySize];

    writer.Write(inMemoryData.Count);
    foreach(KeyValuePair<string,uint[]> pair in inMemoryData)
    {
      int length = pair.Value.Length;
      writer.WriteStringWithLength(pair.Key);
      writer.Write(length);

      Array.Copy(pair.Value, buffer, length);
      if(idMap != null)
      {
        for(int i=0; i<length; i++)
        {
          buffer[i] = idMap[buffer[i]];
        }
      }
      writer.Write(buffer, 0, length);
    }
  }

  public override IEnumerable<uint> Search(string query, SearchFlag flags)
  {
    query = NormalizeKey(query);
    if(query == null) return EmptyIterator.Instance;
    return new SearchIterator(inMemoryData, query, flags);
  }

  #region SearchIterator
  sealed class SearchIterator : IEnumerable<uint>
  {
    public SearchIterator(Dictionary<string,uint[]> index, string key, SearchFlag flags)
    {
      this.index = index;
      this.key   = key;
      this.flags = flags & SearchFlag.MatchMask;
    }

    public IEnumerator<uint> GetEnumerator()
    {
      if(enumerable == null)
      {
        List<IEnumerable<uint>> enumerables = new List<IEnumerable<uint>>();
        if(flags == SearchFlag.ExactMatch)
        {
          uint[] array;
          if(index.TryGetValue(key, out array)) enumerables.Add(array);
        }
        else
        {
          foreach(KeyValuePair<string,uint[]> pair in index)
          {
            if(flags == SearchFlag.MatchStart && pair.Key.StartsWith(key, StringComparison.Ordinal) ||
               flags == SearchFlag.MatchEnd   && pair.Key.EndsWith(key, StringComparison.Ordinal)   ||
               flags == SearchFlag.None       && pair.Key.IndexOf(key, StringComparison.Ordinal) != -1)
            {
              enumerables.Add(pair.Value);
            }
          }
        }

        enumerable = DictionaryUtilities.GetUnion(enumerables);
      }

      return enumerable.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    readonly Dictionary<string,uint[]> index;
    readonly string key;
    readonly SearchFlag flags;
    IEnumerable<uint> enumerable;
  }
  #endregion
}
#endregion

#region TrieIndex
public class TrieIndex : Index
{
  public override void FinishedAdding()
  {
    base.FinishedAdding();

    // calculate the character map that we'll use to reduce keys to their minimum size
    charMap = new Dictionary<char,byte>();
    foreach(string key in inMemoryData.Keys)
    {
      foreach(char c in key)
      {
        if(!charMap.ContainsKey(c))
        {
          if(charMap.Count == 255) throw new InvalidOperationException("The alphabet size must be <= 255.");
          charMap.Add(c, (byte)charMap.Count);
        }
      }
    }

    // build the trie
    rootNode = new Node();
    foreach(KeyValuePair<string,uint[]> pair in inMemoryData)
    {
      InsertKey(ConvertToTrieAlphabet(pair.Key), pair.Value);
    }
  }

  public override void Load(BinaryReader reader)
  {
    // read the character map
    int charMapSize = reader.ReadByte();
    this.charMap = new Dictionary<char,byte>(charMapSize);
    for(int i=0; i<charMapSize; i++)
    {
      this.charMap.Add(reader.ReadChar(), reader.ReadByte());
    }

    this.trieData = reader.ReadByte(reader.ReadInt32());     // now read the trie
    this.reader   = new BinaryReader(reader.BaseStream, true, 512, true); // create our reader
    reader.Skip(reader.ReadInt32()); // and skip the array data
  }

  public override void Save(BinaryWriter writer, Dictionary<uint,uint> idMap)
  {
    // write the character map
    writer.Write((byte)charMap.Count);
    foreach(KeyValuePair<char,byte> pair in charMap)
    {
      writer.Write(pair.Key);
      writer.Write(pair.Value);
    }

    // then write the trie and the array data
    uint startPosition = (uint)writer.Position;

    uint trieLength = CalculateTrieLength(rootNode); // the length of the trie, not counting array data
    uint dataIndex = trieLength + startPosition + sizeof(uint)*2; // the index where we can start writing array data

    writer.Write(trieLength);
    WriteNode(rootNode, writer, idMap, (uint)writer.Position, ref dataIndex);
    Debug.Assert(writer.Position == startPosition + trieLength + sizeof(uint));
    writer.Write(dataIndex - (uint)writer.Position - sizeof(uint)); // add the amount of array data to skip
    writer.Position = dataIndex; // move the writer to the end so the next object will save properly
  }

  public override IEnumerable<uint> Search(string query, SearchFlag flags)
  {
    byte[] key = ConvertToTrieAlphabet(NormalizeKey(query));
    // if the key cannot be converted to our alphabet, it can't match any keys, so just return an empty iterator
    if(key == null) return EmptyIterator.Instance;

    flags &= SearchFlag.MatchMask;
    IEnumerable<uint> iterator =
      flags == SearchFlag.ExactMatch ? new ExactMatchIterator(key, this) :
      flags == SearchFlag.MatchStart ? new StartsWithIterator(key, this) :
      (IEnumerable<uint>)new EndsWithOrContainsIterator(key, this, flags);

    return iterator;
  }

  protected override void Unload()
  {
    base.Unload();
    trieData = null;
    charMap  = null;
    rootNode = null;
  }

  #region TrieIteratorBase
  abstract unsafe class TrieIteratorBase : IEnumerable<uint>
  {
    protected TrieIteratorBase(byte[] key, TrieIndex index)
    {
      this.key   = key;
      this.index = index;
    }

    public abstract IEnumerator<uint> GetEnumerator();

    protected IEnumerable<uint> GetEnumerableFromOffset(uint offset)
    {
      if((offset & 0x40000000) != 0) // if the ID is stored inline use it
      {
        return new SingleItemIterator(offset&0x3FFFFFFF);
      }
      else // an array of IDs are stored in a separate data block. read it.
      {
        index.reader.Position = offset & 0x3FFFFFFF;
        return (IEnumerable<uint>)index.reader.ReadUInt32(index.reader.ReadInt32());
      }
    }

    protected bool Match(ref byte* startingNodePtr, out uint matchingOffset)
    {
      byte* nodePtr = startingNodePtr;
      uint offset;

      for(int i=0; i<key.Length; i++) // loop through the key to find 
      {
        int nchildren = GetNumberOfChildren(nodePtr);
        int childIndex = FindChild(nodePtr, nchildren, key[i]);

        if(childIndex == -1)
        {
          goto notFound;
        }
        else if(IsChildOffsetImplicit(nodePtr)) // if its index is implicit...
        {
          Debug.Assert(childIndex == 0);
          nodePtr += HasValue(nodePtr) ? 7 : 3; // it's at a fixed offset from the current node
        }
        else
        {
          offset = ((uint*)(nodePtr + 2 + nchildren))[childIndex]; // index into the offset table

          if(IsNodeOffset(offset)) // if the child is not a leaf node, 'offset' is the offset of the child node
          {
            nodePtr += offset;
          }
          else if(i != key.Length-1) // the child is a leaf but we're not on the last character, so we can't match
          {
            goto notFound;
          }
          else // otherwise this is the final character and it terminated in a leaf, so we've got only one match
          {
            startingNodePtr = null;
            matchingOffset  = offset;
            return true;
          }
        }
      }

      // we're pointing at the node matching this key
      startingNodePtr = nodePtr;
      matchingOffset  = 0;
      return true;

      notFound:
      startingNodePtr = null;
      matchingOffset  = 0;
      return false;
    }

    protected void RecursivelyAddChildren(byte* node, List<IEnumerable<uint>> enumerables)
    {
      if(IsChildOffsetImplicit(node))
      {
        RecursivelyAddChildren(NextNode(node), enumerables);
      }
      else
      {
        int nchildren = GetNumberOfChildren(node);
        uint* children = (uint*)(node + 2 + nchildren);
        for(int i=0; i<nchildren; i++) // for each child index pointer...
        {
          uint offset = children[i];
          if(IsNodeOffset(offset))
          {
            RecursivelyAddChildren(node+offset, enumerables);
          }
          else
          {
            enumerables.Add(GetEnumerableFromOffset(offset));
          }
        }
      }

      if(HasValue(node))
      {
        uint offset;
        TryGetNodeValue(node, out offset);
        enumerables.Add(GetEnumerableFromOffset(offset));
      }
    }

    protected readonly byte[] key;
    protected readonly TrieIndex index;

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    protected static int FindChild(byte* nodePtr, int nchildren, byte b)
    {
      nodePtr += 2; // skip the 'cLength' and 'flags' fields

      int left=0, right = nchildren-1; // do a binary search to find the index of the child node with the given value
      while(left <= right)
      {
        int middle = (left+right)/2;
        if(b > nodePtr[middle]) left = middle+1;
        else if(b < nodePtr[middle]) right = middle-1;
        else return middle;
      }

      return -1; // if no child nodes have it, return -1
    }

    protected static int GetNumberOfChildren(byte* nodePtr)
    {
      return nodePtr[0];
    }
    
    protected static bool HasValue(byte* nodePtr)
    {
      return (nodePtr[1] & 1) != 0;
    }

    protected static bool IsChildOffsetImplicit(byte* nodePtr)
    {
      return (nodePtr[1] & 2) != 0;
    }

    protected static bool IsNodeOffset(uint offset)
    {
      return (offset & 0x80000000) == 0;
    }

    protected static bool TryGetNodeValue(byte* nodePtr, out uint value)
    {
      if(HasValue(nodePtr))
      {
        nodePtr = NextNode(nodePtr) - sizeof(uint);
        value = *(uint*)nodePtr;
        return true;
      }
      else
      {
        value = 0;
        return false;
      }
    }

    protected static byte* NextNode(byte* nodePtr)
    {
      int valueSize = HasValue(nodePtr) ? sizeof(uint) : 0;

      if(IsChildOffsetImplicit(nodePtr)) // if the next index is implicit...
      {
        nodePtr += 3+valueSize; // it's at a fixed offset from the current node
      }
      else
      {
        int nchildren = GetNumberOfChildren(nodePtr);
        nodePtr += 2 + nchildren + nchildren*sizeof(uint) + valueSize;
      }
      
      return nodePtr;
    }
  }
  #endregion

  #region ExactMatchIterator
  sealed unsafe class ExactMatchIterator : TrieIteratorBase
  {
    public ExactMatchIterator(byte[] key, TrieIndex index) : base(key, index) { }

    public override IEnumerator<uint> GetEnumerator()
    {
      fixed(byte* data = index.trieData)
      {
        byte* node = data;
        uint  offset;
        if(Match(ref node, out offset) && (node == null || TryGetNodeValue(node, out offset)))
        {
          return GetEnumerableFromOffset(offset).GetEnumerator();
        }
        else
        {
          return EmptyIterator.Instance.GetEnumerator();
        }
      }
    }
  }
  #endregion

  #region StartsWithIterator
  sealed unsafe class StartsWithIterator : TrieIteratorBase
  {
    public StartsWithIterator(byte[] key, TrieIndex index) : base(key, index) { }

    public override IEnumerator<uint> GetEnumerator()
    {
      fixed(byte* data = index.trieData)
      {
        byte* nodePtr = data;
        uint  offset;

        if(!Match(ref nodePtr, out offset))
        {
          return EmptyIterator.Instance.GetEnumerator();
        }

        if(nodePtr == null) // if the node has no children, just return its enumerator
        {
          return GetEnumerableFromOffset(offset).GetEnumerator();
        }
        else // otherwise, we need to get the union of it and all its descendants
        {
          List<IEnumerable<uint>> enumerables = new List<IEnumerable<uint>>();
          RecursivelyAddChildren(nodePtr, enumerables);
          return DictionaryUtilities.GetUnion(enumerables).GetEnumerator();
        }
      }
    }
  }
  #endregion
  
  #region EndsWithOrContainsIterator
  sealed unsafe class EndsWithOrContainsIterator : TrieIteratorBase
  {
    public EndsWithOrContainsIterator(byte[] key, TrieIndex index, SearchFlag searchType) : base(key, index)
    {
      matchLeavesOnly = (searchType & SearchFlag.MatchEnd) != 0;
    }

    public override IEnumerator<uint> GetEnumerator()
    {
      List<IEnumerable<uint>> enumerables = new List<IEnumerable<uint>>();
      fixed(byte* data = index.trieData)
      {
        RecursivelySearch(data, 0, enumerables);
      }
      return DictionaryUtilities.GetUnion(enumerables).GetEnumerator();
    }
    
    void RecursivelySearch(byte* nodePtr, int keyIndex, List<IEnumerable<uint>> enumerables)
    {
      int nchildren  = GetNumberOfChildren(nodePtr);
      int childIndex = FindChild(nodePtr, nchildren, key[keyIndex]);
      bool found = false;

      if(childIndex == -1) // if a character of the key wasn't found...
      {
        if(keyIndex != 0) // reset the search, if we're not already at the beginning
        {
          keyIndex = 0;
          childIndex = FindChild(nodePtr, nchildren, key[0]); // and retry
        }
      }
      else if(keyIndex == key.Length-1) // otherwise, if it was the last character that was found, we have a match
      {
        found = true;
      }

      if(childIndex != -1) keyIndex++; // if one of our children matched the next character, increment to the next char

      if(IsChildOffsetImplicit(nodePtr))
      {
        nodePtr = NextNode(nodePtr);
        if(found) // if the next node (which is not a leaf) was the match, add it and its children, if we're allowed
        {
          if(!matchLeavesOnly) RecursivelyAddChildren(nodePtr, enumerables);
        }
        else // otherwise, continue the search
        {
          RecursivelySearch(nodePtr, keyIndex, enumerables);
        }
      }
      else
      {
        uint* children = (uint*)(nodePtr + 2 + nchildren);
        for(int i=0; i<nchildren; i++) // for each child index pointer...
        {
          uint offset = children[i];
          if(i == childIndex) // if we found the character we were looking for and this was the child that had it...
          {
            if(found) // if this child was the match for the key, add it and its children
            {
              if(!IsNodeOffset(offset)) enumerables.Add(GetEnumerableFromOffset(offset));
              else if(!matchLeavesOnly) RecursivelyAddChildren(nodePtr+offset, enumerables);
            }
            else if(IsNodeOffset(offset)) // otherwise, it wasn't the match, but a descendant may be. continue
            {                             // the search
              RecursivelySearch(nodePtr+offset, keyIndex, enumerables);
            }
          }
          else if(IsNodeOffset(offset)) // this child did not match the character, restart the search with this child
          {
            RecursivelySearch(nodePtr+offset, 0, enumerables);
          }
        }
      }
    }
    
    readonly bool matchLeavesOnly;
  }
  #endregion

  #region Node
  sealed class Node
  {
    public Node GetChild(byte c)
    {
      if(Children == null) Children = new ArrayList();

      int index = Children.BinarySearch(c, NodeComparer.Instance);
      if(index < 0)
      {
        Node newChild = new Node();
        newChild.Char = c;
        Children.Insert(~index, newChild);
        return newChild;
      }
      else
      {
        return (Node)Children[index];
      }
    }

    public ArrayList Children;
    public uint[] Entries;
    public byte Char;
  }
  #endregion

  #region NodeComparer
  sealed class NodeComparer : IComparer
  {
    NodeComparer() { }

    public int Compare(object a, object b)
    {
      byte ca = a is byte ? (byte)a : ((Node)a).Char, cb = b is byte ? (byte)b : ((Node)b).Char;
      return ca - cb;
    }

    public readonly static NodeComparer Instance = new NodeComparer();
  }
  #endregion

  byte[] ConvertToTrieAlphabet(string str)
  {
    if(str == null) return null;

    byte[] bytes = new byte[str.Length];
    for(int i=0; i<str.Length; i++)
    {
      if(!charMap.TryGetValue(str[i], out bytes[i])) return null;
    }
    return bytes;
  }

  void InsertKey(byte[] key, uint[] ids)
  {
    Node node = rootNode;
    foreach(byte b in key) node = node.GetChild(b);
    Debug.Assert(node.Entries == null);
    node.Entries = ids;
  }

  byte[] trieData;
  BinaryReader reader;
  Dictionary<char,byte> charMap;
  Node rootNode;

  static uint CalculateNodeLength(Node node)
  {
    uint childCount = node.Children == null ? 0 : (uint)node.Children.Count;
    uint length = 2 + childCount;

    // if the index of the next child is not implicit (or there are multiple children) add space for the indexes
    if(childCount != 1 || ((Node)node.Children[0]).Children == null) length += childCount*sizeof(uint);

    if(node.Entries != null) length += sizeof(uint); // add space for the current node's value, if there is one
    
    return length;
  }

  static uint CalculateTrieLength(Node node)
  {
    uint length = CalculateNodeLength(node); // get the length of the node

    int childCount = node.Children == null ? 0 : node.Children.Count;
    for(int i=0; i<childCount; i++) // plus the length of the child nodes
    {
      Node child = (Node)node.Children[i];
      if(child.Children != null) length += CalculateTrieLength(child);
    }

    return length;
  }

  static uint[] Remap(uint[] ids, Dictionary<uint,uint> idMap)
  {
    if(idMap != null)
    {
      ids = (uint[])ids.Clone();
      for(int i=0; i<ids.Length; i++)
      {
        ids[i] = idMap[ids[i]];
      }
    }
    return ids;
  }

  static void WriteNode(Node node, BinaryWriter writer, Dictionary<uint,uint> idMap, uint trieStart, ref uint dataIndex)
  {
    /* There will be a data type called index, which is an index into either the trie or the data on disk, or an entry
     * ID. The high bit of this index indicates whether the index references entry ID data or trie node index data. If
     * set, it references entry ID data. If clear, it's an index into the trie. The second highest bit is only valid
     * for references to entry ID data. If set, the index is a pointer to an area within the file containing a
     * length-prefixed array of entry IDs. If clear, the lower 30 bits are the entry ID.
     * 
     * The trie will have the following node layout:
     * 
     * FIELD        TYPE      DESCRIPTION
     * cCount       byte      A count of the number of child nodes in this node.
     * flags        byte      The first (lowest) bit indicates whether this node contains a value.
     *                        The second bit, if set, indicates that the node contains only a single child
     *                        which begins immediately after the current node, and childIndices will be empty. This is
     *                        to handle the strings of non-shared characters, where we want to minimize overhead.
     * childChars   byte[]    A number of bytes equal to 'cCount'. These are the characters for the
     *                        child nodes, sorted by ordinal.
     * childIndices index[]   A number of index pointers to the child nodes of this node equal to 'cCount'.
     *                        If the second bit of 'flags' is set, this field will be empty.
     * myIndex?     index     The value for this node. An index into the data (with the high bit set to 1).
     *                        This value will only exist if the low bit of 'flags' is set.
     */

    byte cLength = (byte)(node.Children == null ? 0 : node.Children.Count);
    bool indexIsImplicit = false;

    // create the 'flags' field
    byte flags = 0;
    if(node.Entries != null) flags |= 1; // mark whether we have a value
    if(cLength == 1 && ((Node)node.Children[0]).Children != null) // if there's only one child and it's not a leaf...
    {
      flags |= 2; // mark that there is only one child and its position is implicit
      indexIsImplicit = true;
    }

    byte[] childChars = null;
    uint[] childIndices = null;

    if(cLength != 0)
    {
      uint savedPosition = 0;
      uint childIndex = CalculateNodeLength(node);

      childChars = new byte[cLength];
      childIndices = new uint[cLength];

      for(int i=0; i<node.Children.Count; i++)
      {
        Node child = (Node)node.Children[i];
        childChars[i] = child.Char;

        if(child.Children == null) // leaf children with only one ID are stored inline or in a separate data section
        {
          uint[] data = Remap(child.Entries, idMap);
          if(data.Length == 1) // one-length arrays are stored inline
          {
            childIndices[i] = 0xC0000000 | data[0]; // the upper two bits set indicate an inline entry ID
          }
          else
          {
            childIndices[i] = 0x80000000 | dataIndex; // the upper two bits as 10 indicate a pointer to an array

            if(savedPosition == 0)
            {
              savedPosition = (uint)writer.Position;
              writer.Position = dataIndex;
            }
            writer.Write(data.Length);
            writer.Write(data);
            dataIndex = (uint)writer.Position; // move 'dataIndex' to the position where the next array will be written
          }
        }
        else // otherwise, the child index is stored as an offset from the current node
        {
          childIndices[i] = childIndex;
          childIndex += CalculateTrieLength(child);
        }
      }

      if(savedPosition != 0) writer.Position = savedPosition;
    }

    writer.Write(cLength); // write the cLength field
    writer.Write(flags);   // and the 'flags' field

    if(node.Children != null) // now write the childChars and childIndices fields if we have children
    {
      writer.Write(childChars);
      if(!indexIsImplicit) writer.Write(childIndices);
    }

    if(node.Entries != null) // now write our own index value, if we have it.
    {
      uint[] data = Remap(node.Entries, idMap);
      if(data.Length == 1)
      {
        writer.Write(0xC0000000 | data[0]);
      }
      else
      {
        writer.Write(0x80000000 | dataIndex);
        uint savedPosition = (uint)writer.Position;
        writer.Position = dataIndex;
        writer.Write(data.Length);
        writer.Write(data);
        dataIndex = (uint)writer.Position;
        writer.Position = savedPosition;
      }
    }

    if(node.Children != null) // now write the actual child nodes, recursively
    {
      for(int i=0; i<node.Children.Count; i++)
      {
        if((childIndices[i] & 0x80000000) == 0) // if we haven't stored this child inline, write it now
        {
          WriteNode((Node)node.Children[i], writer, idMap, trieStart, ref dataIndex);
        }
      }
    }
  }
}
#endregion

} // namespace Jappy.Backend
