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
using BinaryReader = AdamMil.IO.BinaryReader;
using BinaryWriter = AdamMil.IO.BinaryWriter;

namespace Jappy.Backend
{

public sealed class StringCompressor
{
  public void CreateNew()
  {
    inMemoryMap = new Dictionary<char,uint>();
    rootNode    = null;
  }

  public void AddString(string str)
  {
    if(str == null) str = string.Empty;

    for(int i=0; i<str.Length; i++)
    {
      Debug.Assert(str[i] != 0);   // we use '0' to indicate an unused tree node, so it cannot occur in input.
      Debug.Assert(str[i] != EOD); // we use EOD to indicate the end of a string, so it cannot occur in input either.
      AddChar(str[i]);
    }
    
    AddChar(EOD); // each string, when written, will end with an EOD character, so track its frequency as well.
  }

  public void FinishedAdding()
  {
    // create a node for each character, sorted by the frequency in ascending order
    List<Node> nodes = new List<Node>();
    foreach(KeyValuePair<char,uint> pair in inMemoryMap)
    {
      nodes.Add(new Node(pair.Key, pair.Value));
    }
    nodes.Sort();
    
    // now build the huffman tree -- the following algorithm with 2 queues does it in linear time
    Queue<Node> leaves = new Queue<Node>(nodes), internals = new Queue<Node>();
    while(leaves.Count + internals.Count > 1)
    {
      // pop the two least-weighted nodes from the queues and use those to create a new internal node.
      // if there's a tie between the leaf and internal queues, use the leaf queue to minimize codeword variance.
      Node left, right;

      if(leaves.Count == 0) left = internals.Dequeue();
      else if(internals.Count == 0) left = leaves.Dequeue();
      else left = leaves.Peek().CompareTo(internals.Peek()) <= 0 ? leaves.Dequeue() : internals.Dequeue();

      if(leaves.Count == 0) right = internals.Dequeue();
      else if(internals.Count == 0) right = leaves.Dequeue();
      else right = leaves.Peek().CompareTo(internals.Peek()) <= 0 ? leaves.Dequeue() : internals.Dequeue();
 
      internals.Enqueue(new Node(left, right));
    }

    rootNode = internals.Count != 0 ? internals.Dequeue() : // the remaining node, if any, is the root.
               leaves.Count    != 0 ? leaves.Dequeue()    : null;
    BuildCodes();
  }

  public void WriteString(BinaryWriter writer, string str)
  {
    if(rootNode == null) throw new InvalidOperationException("No strings have been added to the compressor.");
    if(str == null) str = string.Empty; // treat null strings as empty strings when writing

    uint bitsAdded=0;
    byte buffer = 0;
    for(int i=0; i<str.Length; i++)
    {
      WriteCharacter(writer, ref bitsAdded, ref buffer, str[i]);
    }
    WriteCharacter(writer, ref bitsAdded, ref buffer, EOD);

    if(bitsAdded != 0) // if there's any data remaining in b, write it
    {
      buffer >>= 8-(int)bitsAdded; // shift zeros into the high bits to fill out the byte
      writer.Write(buffer);
    }
  }

  public unsafe string ReadString(BinaryReader reader)
  {
    if(loadedTree == null) throw new InvalidOperationException("The compressor has not been loaded.");

    stringBuilder.Length = 0;

    fixed(ushort* data=loadedTree)
    {
      const int nodeSize = 2; // nodes are 2 words in length
      uint bitsRemaining = 0;
      int charValue;
      byte buffer = 0;

      do
      {
        int treePosition = 0;
        while((charValue=data[treePosition]) == 0)
        {
          if(bitsRemaining == 0)
          {
            buffer = reader.ReadByte();
            bitsRemaining = 8;
          }

          if((buffer&1) == 0) // going left, we simply move to the next node. nodes are 2 words in length
          {
            treePosition += nodeSize;
          }
          else // otherwise, we advance by the offset stored in the second word of the node
          {
            treePosition += data[treePosition+1]*nodeSize; // the offset is stored in nodes
          }

          buffer >>= 1;
          bitsRemaining--;
        }

        if(charValue != EOD) stringBuilder.Append((char)charValue);
      } while(charValue != EOD);
    }

    // return zero-length strings as null strings when reading
    return stringBuilder.Length == 0 ? null : stringBuilder.ToString();
  }

  public void Load(BinaryReader reader)
  {
    if(reader == null) throw new ArgumentNullException();
    Unload();
    loadedTree = reader.ReadUInt16(reader.ReadInt32());
  }

  public void Save(BinaryWriter writer)
  {
    if(writer == null) throw new ArgumentNullException();
    if(rootNode == null) throw new InvalidOperationException("No strings have been added to the compressor.");

    // pack the binary tree into an array. the tree is stored with the following format:
    // each node is four bytes (2 words). the first word stores the character associated with the node position. if
    // zero, no character is associated, and the node is an internal node. the second word stores the offset, in nodes,
    // from the current node to the right child node. the left child node will immediately follow its parent node.
    MemoryStream packedTree = new MemoryStream();
    using(BinaryWriter treeWriter = new BinaryWriter(packedTree))
    {
      CopyTreeToArray(treeWriter);
    }

    writer.Write((int)packedTree.Length / 2); // the length is in words, so we divide by 2
    writer.Write(packedTree.ToArray());
  }

  const char EOD = (char)3; // a character used for end-of-data (ASCII 3 == ETX ["End Of Text"])

  #region Node
  sealed class Node : IComparable<Node>
  {
    public Node(char c, uint frequency)
    {
      this.Char = c;
      this.Weight = frequency;
    }
    
    public Node(Node left, Node right)
    {
      // we want internal nodes with weight X to compare >= to leaf nodes with weight X in order to minimize variance
      // of codeword length, so we set the characters of internal nodes to 0xFFFF
      this.Char   = (char)0xffff;
      this.Weight = left.Weight + right.Weight;
      this.Left   = left;
      this.Right  = right;
    }

    public bool IsLeaf
    {
      get { return Left == null && Right == null; }
    }

    public int CompareTo(Node other)
    {
      int diff = (int)this.Weight - (int)other.Weight;
      return diff == 0 ? this.Char - other.Char : diff;
    }

    public Node Left, Right;
    public uint Weight;
    public char Char;
  }
  #endregion

  void AddChar(char c)
  {
    uint count;
    inMemoryMap.TryGetValue(c, out count);
    inMemoryMap[c] = count+1;
  }

  void BuildCodes()
  {
    // walk the tree and reuse the char map to hold the bit sequence for each character. the bit sequence is encoded
    // in a uint with the top 5 bits holding the sequence length and the bottom 27 bits holding the sequence itself,
    // in reverse order (for easy writing)
    inMemoryMap.Clear();
    maxNodeDepth = 0;
    if(rootNode != null) BuildCodes(rootNode, 0, 0);
  }

  void BuildCodes(Node node, uint length, uint code)
  {
    maxNodeDepth = Math.Max(maxNodeDepth, (int)length+1);

    if(node.IsLeaf) // if this is a leaf node, add the code to the map
    {
      code = PackCode(length == 0 ? 1 : length, code); // handle the special case where the root is a leaf node,
      inMemoryMap[node.Char] = code;                   // causing length to be zero
    }
    else
    {
      length++;
      code <<= 1;
      if(node.Left  != null) BuildCodes(node.Left,  length, code);
      if(node.Right != null) BuildCodes(node.Right, length, code|1);
    }
  }

  void CopyTreeToArray(BinaryWriter writer)
  {
    CopyTreeToArray(writer, rootNode);
  }
  
  void Unload()
  {
    inMemoryMap = null;
    rootNode    = null;
    loadedTree  = null;
  }

  void WriteCharacter(BinaryWriter writer, ref uint bitsAdded, ref byte buffer, char c)
  {
    uint code;
    if(!inMemoryMap.TryGetValue(c, out code))
    {
      throw new ArgumentException("The input contains a character that was not present in the strings "+
                                  "used to build the compressor.");
    }

    uint codeLength = GetLength(code);
    do
    {
      buffer = (byte)((buffer>>1) | (byte)((code&1) << 7)); // make room in the topmost bit add stick the next code bit
      code >>= 1;
      if(++bitsAdded == 8)
      {
        writer.Write(buffer);
        bitsAdded = 0;
      }
    } while(--codeLength != 0);
  }

  Dictionary<char,uint> inMemoryMap;
  Node rootNode;
  int maxNodeDepth;
  ushort[] loadedTree;
  StringBuilder stringBuilder = new StringBuilder();

  static void CopyTreeToArray(BinaryWriter writer, Node node)
  {
    ushort offset = 0;
    int nodePosition = (int)writer.Position;

    writer.Write(node.IsLeaf ? node.Char : (char)0);
    writer.Write(offset); // add a dummy offset which we'll replace later

    if(node.Left != null)
    {
      CopyTreeToArray(writer, node.Left);
    }
    if(node.Right != null)
    {
      int offsetInNodes = ((int)writer.Position - nodePosition) / 4; // each node is four bytes
      Debug.Assert(offsetInNodes <= 0xFFFF);
      offset = (ushort)offsetInNodes;
      CopyTreeToArray(writer, node.Right);
    }

    // update the dummy offset with the real offset
    long currentPosition = writer.Position;
    writer.Position = nodePosition + 2; // the offset is after the character, which is 2 bytes
    writer.Write(offset);
    writer.Position = currentPosition;
  }

  static uint GetLength(uint code)
  {
    return code >> 27;
  }

  static uint PackCode(uint length, uint code)
  {
    uint reversedCode = 0; // we store the code in a reversed form so we can easily write it out
    for(uint i=0; i<length; i++)
    {
      reversedCode = (reversedCode << 1) | (code&1);
      code >>= 1;
    }

    return reversedCode | (length << 27);
  }
}

} // namespace Jappy.Backend
