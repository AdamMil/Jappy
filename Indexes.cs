using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Jappy
{

/* Index optimization:
 * There are three types of indices in the Japanese dictionary (headword, readings, meanings), and they all have
 * different characteristics.
 * 
 *                            Headwords     Readings      Meanings
 * Largest index array                5           28         10803
 * Average index array             1.01         1.21          9.96
 * Bytes to store size info        464k         307k          150k
 * Bytes to store strings         1281k        1150k          740k
 * Bytes to store indices          468k         373k         1498k
 * Alphabet size                   4734          102            36
 * Shared prefixes between keys    Some         Lots         Lots!
 * 
 * The headword index has a nearly 1:1 mapping between keys and entries. This leads to a large waste of space storing
 * sizes (on disk) and one-length array objects (in memory). The keys have some shared prefixes, but this is very
 * limited due to the large alphabet size and extensive use of kanji as the first character of the key.
 * 
 * The readings index has a nearly 1:1 mapping between keys and entries but has more multi-value index arrays in there.
 * There's still a lot of wasted space storing sizes (on disk) and one-length array objects (in memory). In addition,
 * a lot of space is wasted storing strings due to the fact that the keys have a lot of shared prefixes.
 * 
 * The meanings index is about 1:10 between keys and entries and has some very large index arrays. A lot of space is
 * wasted storing strings due to the large number of shared prefixes in this index.
 * 
 * Currently all of these indexes are stored in memory.
 * 
 * For the Readings and Meanings indexes, the following structure is proposed:
 * 
 * The keys will be stored in a trie kept in memory. The data will be stored in a dword-aligned section of the disk
 * file, except where the data can fit within a single uint (ie, when there's a 1:1 mapping from key to entry ID).
 * 
 * 
*/

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

  public abstract void Load(IOReader reader);
  public abstract void Save(IOWriter writer, Dictionary<uint,uint> idMap);

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
  public override void Load(IOReader reader)
  {
    Unload();

    // read the table size and key data length
    this.tableSize = reader.ReadUint();
    int dataLength = reader.ReadInt();
    this.tableStart = (uint)reader.Position;

    reader.Skip(dataLength); // now skip over the data so the next index can load properly

    this.smallReader = new IOReader(reader.BaseStream, 128, true);
    this.bigReader   = new IOReader(reader.BaseStream, 4096, true);
  }

  public override void Save(IOWriter writer, Dictionary<uint,uint> idMap)
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
      
      if(pair.Value.Length > 1)
      {
        arrayDataLength += (uint)pair.Value.Length*sizeof(uint) + sizeof(int);
      }
    }

    writer.Add(tableSize); // write the table size, in buckets
    writer.Add(tableSize*BucketSize + keyDataLength + arrayDataLength); // write the total length of data for easy skipping

    uint keyDataStart   = (uint)writer.Position + tableSize*BucketSize; // the key data starts after the table
    uint arrayDataStart = keyDataStart + keyDataLength; // the array data starts after the key data

    // first write the bucket data
    byte[] emptyBucket = new byte[BucketSize]; // all-zero bytes for an empty bucket
    for(int i=0; i<buckets.Length; i++)
    {
      string key = buckets[i];
      if(key == null)
      {
        writer.Add(emptyBucket);
      }
      else
      {
        uint[] array = inMemoryData[key];

        writer.Add((ushort)key.Length);
        writer.Add(key[0]);

        if(key.Length == 1) // add the index of the key data. one-length keys are not stored separately
        {
          writer.Add((uint)0);
        }
        else
        {
          writer.Add(keyDataStart);
          keyDataStart += (uint)key.Length*sizeof(char);
        }

        if(array.Length == 1) // add the index of the array data. one-length arrays are stored inline in this slot
        {
          uint id = array[0];
          writer.Add(idMap == null ? id : idMap[id]);
        }
        else
        {
          writer.Add(arrayDataStart | 0x80000000); // the high bit set indicates that it points to external array data
          arrayDataStart += (uint)(array.Length*sizeof(uint) + sizeof(uint));
        }
      }
    }

    // now write the key string data
    for(int i=0; i<buckets.Length; i++)
    {
      string key = buckets[i];
      if(key != null && key.Length > 1) writer.Add(key); // one-length keys are not stored separately
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
        writer.Add(data.Length);
        writer.Add(data);
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
    if(smallReader != null)
    {
      smallReader.Dispose();
      smallReader = null;
    }
    
    if(bigReader != null)
    {
      bigReader.Dispose();
      bigReader = null;
    }

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
        ushort keyLength = index.smallReader.ReadUShort();
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
          uint dataIndex = index.smallReader.ReadUint();
          uint currentPosition = (uint)index.smallReader.Position;

          index.smallReader.Position = dataIndex;
          index.smallReader.ReadCharArray(bucketKey, keyLength);
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
          uint dataIndex = index.smallReader.ReadUint();
          if((dataIndex & 0x80000000) == 0) // if the high bit is clear, the data is a single uint stored inline
          {
            return new SingleItemIterator(dataIndex).GetEnumerator();
          }
          else
          {
            uint currentPosition  = (uint)index.smallReader.Position;
            index.smallReader.Position = dataIndex&0x7FFFFFFF;
            uint[] data = index.smallReader.ReadUintArray(index.smallReader.ReadInt());
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
      this.searchType = searchType;
    }

    public unsafe IEnumerator<uint> GetEnumerator()
    {
      List<IEnumerable<uint>> arrays = null;
      List<uint> singles = null;

      char[] bucketKey = new char[16];

      index.bigReader.Position = index.tableStart;
      for(int bucketIndex=0; bucketIndex<index.tableSize; bucketIndex++) // scan through the entire table
      {
        ushort keyLength = index.bigReader.ReadUShort();
        if(keyLength < key.Length) // skip empty buckets and buckets with keys that are too short to possibly match
        {
          index.bigReader.Skip((int)BucketSize - sizeof(ushort));
          continue;
        }

        // if it's a "Starts With" match and the first characters don't match, move to the next bucket
        if(searchType == SearchFlag.MatchStart && key[0] != index.bigReader.ReadChar())
        {
          index.bigReader.Skip((int)BucketSize - (sizeof(ushort)+sizeof(char)));
          continue;
        }

        // otherwise, read the key index and do a character-by-character comparison
        {
          uint dataIndex = index.bigReader.ReadUint();
          uint currentPosition = (uint)index.bigReader.Position;
          bool found = true;

          if(keyLength > bucketKey.Length)
          {
            int newSize = bucketKey.Length;
            do newSize *= 2; while(newSize < keyLength);
            bucketKey = new char[newSize];
          }

          index.bigReader.Position = dataIndex;
          index.bigReader.ReadCharArray(bucketKey, 0, keyLength);
          index.bigReader.Position = currentPosition;

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
        }

        // at this point, the keys match. we want to return the data.
        {
          uint dataIndex = index.bigReader.ReadUint();
          if((dataIndex & 0x80000000) == 0) // if the high bit is clear, the data is a single uint stored inline
          {
            if(singles == null) singles = new List<uint>();
            int newPos = singles.BinarySearch(dataIndex);
            if(newPos < 0) singles.Insert(~newPos, dataIndex);
          }
          else
          {
            uint currentPosition  = (uint)index.bigReader.Position;
            index.bigReader.Position = dataIndex&0x7FFFFFFF;
            if(arrays == null) arrays = new List<IEnumerable<uint>>();
            arrays.Add(index.bigReader.ReadUintArray(index.bigReader.ReadInt()));
          }
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

  IOReader smallReader, bigReader;
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
  public override void Load(IOReader reader)
  {
    Unload();

    int count = reader.ReadInt();
    inMemoryData = new Dictionary<string, uint[]>(count);

    while(count-- != 0)
    {
      inMemoryData.Add(reader.ReadStringWithLength(), reader.ReadUintArray(reader.ReadInt()));
    }
  }

  public override void Save(IOWriter writer, Dictionary<uint,uint> idMap)
  {
    if(inMemoryData == null) throw new InvalidOperationException();

    int largestArraySize = 0;
    foreach(uint[] array in inMemoryData.Values)
    {
      if(array.Length > largestArraySize) largestArraySize = array.Length;
    }
    uint[] buffer = new uint[largestArraySize];

    writer.Add(inMemoryData.Count);
    foreach(KeyValuePair<string,uint[]> pair in inMemoryData)
    {
      int length = pair.Value.Length;
      writer.AddStringWithLength(pair.Key);
      writer.Add(length);

      Array.Copy(pair.Value, buffer, length);
      if(idMap != null)
      {
        for(int i=0; i<length; i++)
        {
          buffer[i] = idMap[buffer[i]];
        }
      }
      writer.Add(buffer, 0, length);
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
      this.flags = flags;
    }

    public IEnumerator<uint> GetEnumerator()
    {
      if(enumerable == null)
      {
        List<IEnumerable<uint>> enumerables = new List<IEnumerable<uint>>();
        if((flags & SearchFlag.ExactMatch) == SearchFlag.ExactMatch)
        {
          uint[] array;
          if(index.TryGetValue(key, out array)) enumerables.Add(array);
        }
        else
        {
          foreach(KeyValuePair<string,uint[]> pair in index)
          {
            if((flags & SearchFlag.MatchStart) != 0 && pair.Key.StartsWith(key, StringComparison.Ordinal) ||
               (flags & SearchFlag.MatchEnd)   != 0 && pair.Key.EndsWith(key, StringComparison.Ordinal) ||
               (flags & SearchFlag.ExactMatch) == 0 && pair.Key.IndexOf(key, StringComparison.Ordinal) != -1)
            {
              enumerables.Add(pair.Value);
            }
          }
        }

        enumerable = enumerables.Count == 0 ? EmptyIterator.Instance :
                     enumerables.Count == 1 ? enumerables[0] :
                     new UnionIterator(enumerables);
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
          if(charMap.Count == 127) throw new InvalidOperationException("The alphabet size must be <= 127.");
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

  public override void Load(IOReader reader)
  {
    // read the character map
    int charMapSize = reader.ReadByte();
    this.charMap = new Dictionary<char,byte>(charMapSize);
    for(int i=0; i<charMapSize; i++)
    {
      this.charMap.Add(reader.ReadChar(), reader.ReadByte());
    }

    this.trieData = reader.ReadByteArray(reader.ReadInt());     // now read the trie
    this.reader   = new IOReader(reader.BaseStream, 128, true); // create our reader
    reader.Skip(reader.ReadInt()); // and skip the array data
  }

  public override void Save(IOWriter writer, Dictionary<uint,uint> idMap)
  {
    // write the character map
    writer.Add((byte)charMap.Count);
    foreach(KeyValuePair<char,byte> pair in charMap)
    {
      writer.Add(pair.Key);
      writer.Add(pair.Value);
    }

    // then write the trie and the array data
    uint startPosition = (uint)writer.Position;

    uint trieLength = CalculateTrieLength(rootNode); // the length of the trie, not counting array data
    uint dataIndex = trieLength + startPosition + sizeof(uint)*2; // the index where we can start writing array data

    writer.Add(trieLength);
    WriteNode(rootNode, writer, idMap, (uint)writer.Position, ref dataIndex);
    Debug.Assert(writer.Position == startPosition + trieLength + sizeof(uint));
    writer.Add(dataIndex - (uint)writer.Position - sizeof(uint)); // add the amount of array data to skip
    writer.Position = dataIndex; // move the writer to the end so the next object will save properly
  }

  public override IEnumerable<uint> Search(string query, SearchFlag flags)
  {
    byte[] key = ConvertToTrieAlphabet(query);
    // if the key cannot be converted to our alphabet, it can't match any keys, so just return an empty iterator
    if(key == null) return EmptyIterator.Instance;
    
    flags &= SearchFlag.MatchMask;
    return new ExactMatchIterator(key, this);
  }

  protected override void Unload()
  {
    base.Unload();
    trieData = null;
    charMap  = null;
    rootNode = null;
  }

  #region ExactMatchIterator
  sealed class ExactMatchIterator : IEnumerable<uint>
  {
    public ExactMatchIterator(byte[] key, TrieIndex index)
    {
      this.key   = key;
      this.index = index;
    }

    public unsafe IEnumerator<uint> GetEnumerator()
    {
      fixed(byte* data = index.trieData)
      {
        int nodePos = 0;
        byte* nodePtr;

        for(int i=0; i<key.Length; i++) // loop through the key to find 
        {
          nodePtr = data+nodePos;
          int nchildren = GetNumberOfChildren(nodePtr);
          int childIndex = FindChild(nodePtr, nchildren, key[i]);
          if(childIndex == -1) break;
          
          if(childIndex == 0 && (nodePtr[1]&0x40) != 0) // if it's the first child and its index is implicit...
          {
            nodePos += sizeof(ushort)+sizeof(byte); // it's at a fixed offset from the current node
            continue;
          }

          uint offset = ((uint*)(nodePtr + sizeof(ushort) + nchildren))[childIndex]; // index into the offset table
          
          if((offset & 0x80000000) == 0) // if the child is not a leaf node, it's a pointer to the next node
          {
            nodePos = (int)offset;
          }
          else if(i < key.Length-1) // the child is a leaf but we're not on the last character, so we can't match
          {
            break;
          }
          else // otherwise this is the final character and it terminated in a leaf, so we've got a match
          {
            return GetEnumeratorFromOffset(offset);
          }
        }
        
        // we're pointing at the node matching this key. it's not a leaf node, but if it has a value, we have a match
        nodePtr = data+nodePos;
        if((nodePtr[1] & 0x80) != 0) // if the node has a value
        {
          int nchildren = GetNumberOfChildren(nodePtr);
          if(nchildren == 1 && (nodePtr[1]&0x40) != 0) // if there's only one child and its index is implicit...
          {
            nodePtr += sizeof(ushort)+sizeof(byte); // it's at a fixed offset from the current node
          }
          else
          {
            nodePtr += sizeof(ushort) + nchildren + nchildren*sizeof(uint);
          }
          return GetEnumeratorFromOffset(*(uint*)nodePtr);
        }
      }
      
      return EmptyIterator.Instance.GetEnumerator(); // no match
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator<uint> GetEnumeratorFromOffset(uint offset)
    {
      if((offset & 0x40000000) != 0) // if the ID is stored inline use it
      {
        return new SingleItemIterator(offset&0x3FFFFFFF).GetEnumerator();
      }
      else // an array of IDs are stored in a separate data block. read it.
      {
        index.reader.Position = offset & 0x3FFFFFFF;
        return ((IEnumerable<uint>)index.reader.ReadUintArray(index.reader.ReadInt())).GetEnumerator();
      }
    }

    readonly byte[] key;
    readonly TrieIndex index;

    static unsafe int FindChild(byte* nodePtr, int nchildren, byte b)
    {
      nodePtr += 2;

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
    
    static unsafe int GetNumberOfChildren(byte* nodePtr)
    {
      return (nodePtr[0] | (nodePtr[1]<<8)) & 0x3FFF;
    }
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

  uint CalculateTrieLength()
  {
    return CalculateTrieLength(rootNode);
  }

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
  IOReader reader;
  Dictionary<char,byte> charMap;
  Node rootNode;

  static uint CalculateTrieLength(Node node)
  {
    uint childCount = node.Children == null ? 0 : (uint)node.Children.Count;
    uint length = sizeof(ushort) + childCount;

    // if the index of the next child is not implicit (or there are multiple children) add space for the indexes
    if(childCount != 1 || ((Node)node.Children[0]).Children == null) length += childCount*sizeof(uint);

    if(node.Entries != null) length += sizeof(uint); // add space for the current node's value, if there is one

    for(int i=0; i<childCount; i++)
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

  static void WriteNode(Node node, IOWriter writer, Dictionary<uint,uint> idMap, uint trieStart, ref uint dataIndex)
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
     * cLength      ushort    A 14-bit count of the number of child nodes in this node.
     *                        The high bit indicates whether this node contains a value.
     *                        The second highest bit, if set, indicates that the node contains only a single child
     *                        which begins immediately after the current node, and childIndices will be empty. This is
     *                        to handle the strings of non-shared characters, where we want to minimize overhead.
     * childChars   byte[]    A number of bytes equal to the count from 'cCount'. These are the characters for the
     *                        child nodes, sorted by ordinal.
     * childIndices index[]   A number of index pointers to the child nodes of this node equal to the count from
     *                        'cCount'. If the second highest bit of cLength is set, this field will be empty.
     * myIndex?     index     The value for this node. An index into the data (with the high bit set to 1).
     *                        This value will only exist if the high bit of 'cCount' is set.
     */

    ushort cLength = (ushort)(node.Children == null ? 0 : node.Children.Count);
    if(cLength > 0x3FFF) throw new InvalidOperationException("Too many child nodes.");

    byte[] childChars = null;
    uint[] childIndices = null;
    bool indexIsImplicit = false;

    if(cLength != 0)
    {
      uint savedPosition = (uint)writer.Position;
      writer.Position = dataIndex;

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
            writer.Add(data.Length);
            writer.Add(data);
            dataIndex = (uint)writer.Position;
          }
        }
      }

      writer.Position = savedPosition;
    }

    if(node.Entries != null) cLength |= 0x8000;
    if(childIndices.Length == 1 && childIndices[0] == 0)
    {
      cLength |= 0x4000; // mark that there is only one child and its position is implicit
      indexIsImplicit = true;
    }

    writer.Add(cLength); // write the cLength field

    uint indicesPosition = 0;
    if(node.Children != null) // now write the childChars and childIndices fields if we have children. we may need to
    {                         // come back and update the childIndices later, so save its position.
      writer.Add(childChars);
      if(!indexIsImplicit)
      {
        indicesPosition = (uint)writer.Position;
        writer.Add(childIndices);
      }
    }

    if(node.Entries != null) // now write our own index value, if we have it.
    {
      uint[] data = Remap(node.Entries, idMap);
      if(data.Length == 1)
      {
        writer.Add(0xC0000000 | data[0]);
      }
      else
      {
        writer.Add(0x80000000 | dataIndex);
        uint savedPosition = (uint)writer.Position;
        writer.Position = dataIndex;
        writer.Add(data.Length);
        writer.Add(data);
        dataIndex = (uint)writer.Position;
        writer.Position = savedPosition;
      }
    }

    if(node.Children != null) // now write the actual child nodes, recursively
    {
      bool wroteSome = false;
      for(int i=0; i<node.Children.Count; i++)
      {
        if(childIndices[i] == 0) // if we haven't stored this child inline, write it now
        {
          childIndices[i] = (uint)writer.Position - trieStart;
          WriteNode((Node)node.Children[i], writer, idMap, trieStart, ref dataIndex);
          wroteSome = true;
        }
      }

      if(wroteSome && !indexIsImplicit) // rewrite the childIndices field if necessary
      {
        uint savedPosition = (uint)writer.Position;
        writer.Position = indicesPosition;
        writer.Add(childIndices);
        writer.Position = savedPosition;
      }
    }
  }
}
#endregion

} // namespace Jappy