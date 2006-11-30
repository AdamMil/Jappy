using System.Collections.Generic;

namespace Jappy
{

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
}

} // namespace Jappy