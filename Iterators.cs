using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Jappy
{

#region EmptyIterator
public sealed class EmptyIterator : IEnumerable<uint>
{
  EmptyIterator() { }

  sealed class EmptyEnumerator : IEnumerator<uint>
  {
    public uint Current
    {
      get { throw new InvalidOperationException(); }
    }
    
    public bool MoveNext()
    {
      return false;
    }
    
    public void Reset() { }
    
    object IEnumerator.Current
    {
      get { return Current; }
    }
    
    void IDisposable.Dispose() { }
  }
  
  public IEnumerator<uint> GetEnumerator()
  {
    return new EmptyEnumerator();
  }
  
  public static readonly EmptyIterator Instance = new EmptyIterator();
  
  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}
#endregion

#region EntryIterator
public sealed class EntryIterator : IEnumerable<Entry>
{
  public EntryIterator(WordDictionary dictionary, IEnumerable<uint> iterator)
  {
    this.dictionary = dictionary;
    this.iterator   = iterator;
  }

  sealed class EntryEnumerator : IEnumerator<Entry>
  {
    public EntryEnumerator(WordDictionary dictionary, IEnumerator<uint> iterator)
    {
      this.dictionary = dictionary;
      this.enumerator = iterator;
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
      if(!enumerator.MoveNext()) return false;
      current = dictionary.GetEntryById(enumerator.Current);
      return true;
    }

    public void Reset()
    {
      current = null;
      enumerator.Reset();
    }

    object IEnumerator.Current
    {
      get { return Current; }
    }

    void IDisposable.Dispose()
    {
      enumerator.Dispose();
    }

    readonly WordDictionary dictionary;
    readonly IEnumerator<uint> enumerator;
    Entry? current;
  }

  public IEnumerator<Entry> GetEnumerator()
  {
    return new EntryEnumerator(dictionary, iterator.GetEnumerator());
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  readonly WordDictionary dictionary;
  readonly IEnumerable<uint> iterator;
}
#endregion

#region ExampleIterator
public sealed class ExampleIterator : IEnumerable<ExampleSentence>
{
  public ExampleIterator(ExampleSentences examples, IEnumerable<uint> iterator)
  {
    this.examples = examples;
    this.iterator = iterator;
  }

  sealed class ExampleEnumerator : IEnumerator<ExampleSentence>
  {
    public ExampleEnumerator(ExampleSentences examples, IEnumerator<uint> iterator)
    {
      this.examples   = examples;
      this.enumerator = iterator;
    }

    public ExampleSentence Current
    {
      get
      {
        if(!current.HasValue) throw new InvalidOperationException();
        return current.Value;
      }
    }

    public bool MoveNext()
    {
      if(!enumerator.MoveNext()) return false;
      current = examples.GetExampleById(enumerator.Current);
      return true;
    }

    public void Reset()
    {
      current = null;
      enumerator.Reset();
    }

    object IEnumerator.Current
    {
      get { return Current; }
    }

    void IDisposable.Dispose()
    {
      enumerator.Dispose();
    }

    readonly ExampleSentences examples;
    readonly IEnumerator<uint> enumerator;
    ExampleSentence? current;
  }

  public IEnumerator<ExampleSentence> GetEnumerator()
  {
    return new ExampleEnumerator(examples, iterator.GetEnumerator());
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  readonly ExampleSentences examples;
  readonly IEnumerable<uint> iterator;
}
#endregion

#region SingleItemIterator
public sealed class SingleItemIterator : IEnumerable<uint>
{
  public SingleItemIterator(uint value)
  {
    this.value = value;
  }

  sealed class SingleItemEnumerator : IEnumerator<uint>
  {
    public SingleItemEnumerator(uint value)
    {
      this.value = value;
    }

    public uint Current
    {
      get
      {
        if(state != State.Current) throw new InvalidOperationException();
        return value;
      }
    }

    public bool MoveNext()
    {
      if(state == State.BOF)
      {
        state = State.Current;
        return true;
      }
      else if(state == State.Current)
      {
        state = State.EOF;
      }

      return false;
    }

    public void Reset()
    {
      state = State.BOF;
    }

    object IEnumerator.Current
    {
      get { return Current; }
    }

    void IDisposable.Dispose() { }

    enum State { BOF, Current, EOF }

    uint value;
    State state;
  }

  public IEnumerator<uint> GetEnumerator()
  {
    return new SingleItemEnumerator(value);
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  uint value;
}
#endregion

#region IntersectionIterator
/// <summary>An iterator that represents the intersection of several other iterators. The iterators must return uints
/// in a sorted order, and this iterator will do the same.
/// </summary>
public sealed class IntersectionIterator : IEnumerable<uint>
{
  public IntersectionIterator(IList<IEnumerable<uint>> iterators)
  {
    if(iterators == null || iterators.Count == 0) throw new ArgumentException();
    this.iterators = iterators;
  }

  sealed class IntersectionEnumerator : IEnumerator<uint>
  {
    public IntersectionEnumerator(IEnumerator<uint>[] enumerators)
    {
      this.enumerators = enumerators;
    }

    public uint Current
    {
      get
      {
        if(!current.HasValue) throw new InvalidOperationException();
        return current.Value;
      }
    }

    public bool MoveNext()
    {
      // at this point, all enumerators are BOF, or they have the same value, or one or more are done.

      for(int i=0; i<enumerators.Length; i++) // advance all enumerators to the next value (first if they're all BOF)
      {
        if(!enumerators[i].MoveNext())
        {
          current = null;
          return false;
        }
      }

      // now find the largest value of the enumerators. all the other values aren't in the intersection.
      uint desiredValue = enumerators[0].Current;
      for(int i=1; i<enumerators.Length; i++)
      {
        if(enumerators[i].Current > desiredValue) desiredValue = enumerators[i].Current;
      }

      // now advance all enumerators until one is completed, or until they all have the same value.
      for(int i=0; i<enumerators.Length; i++)
      {
        restart:
        uint value = enumerators[i].Current; // at this point, the value may be smaller, but it won't be larger.
        while(value < desiredValue) // if it's smaller, advance it until it's not smaller.
        {
          if(!enumerators[i].MoveNext())
          {
            current = null;
            return false;
          }
          value = enumerators[i].Current;
        }

        // at this point, it may be larger or equal. if it's larger, set the desired value to that value and restart
        // the process
        while(value > desiredValue)
        {
          desiredValue = value;
          i = 0;
          goto restart;
        }
      }

      // at this point, all enumerators have the same value. that value will become the current value.
      current = desiredValue;
      return true;
    }

    public void Reset()
    {
      current = null;
      for(int i=0; i<enumerators.Length; i++)
      {
        enumerators[i].Reset();
      }
    }

    object IEnumerator.Current
    {
      get { return Current; }
    }

    void IDisposable.Dispose()
    {
      foreach(IEnumerator<uint> enumerator in enumerators)
      {
        enumerator.Dispose();
      }
    }

    readonly IEnumerator<uint>[] enumerators;
    uint? current;
  }

  public IEnumerator<uint> GetEnumerator()
  {
    IEnumerator<uint>[] enumerators = new IEnumerator<uint>[iterators.Count];
    for(int i=0; i<enumerators.Length; i++)
    {
      enumerators[i] = iterators[i].GetEnumerator();
    }
    return new IntersectionEnumerator(enumerators);
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  readonly IList<IEnumerable<uint>> iterators;
}
#endregion

#region SubtractionIterator
public sealed class SubtractionIterator : IEnumerable<uint>
{
  public SubtractionIterator(IEnumerable<uint> add, IEnumerable<uint> subtract)
  {
    this.add      = add;
    this.subtract = subtract;
  }

  sealed class SubtractionEnumerator : IEnumerator<uint>
  {
    public SubtractionEnumerator(IEnumerator<uint> add, IEnumerator<uint> subtract)
    {
      this.add      = add;
      this.subtract = subtract;
    }

    public uint Current
    {
      get
      {
        if(!current.HasValue) throw new InvalidOperationException();
        return current.Value;
      }
    }

    public bool MoveNext()
    {
      if(!current.HasValue) // if this is the first time, attempt to move 'subtract' to the first value
      {
        subtractDone = subtract.MoveNext();
      }

      do
      {
        if(!add.MoveNext()) // attempt to move 'add' ahead one value
        {
          current = null;
          return false;
        }

        current = add.Current;

        while(!subtractDone && subtract.Current < current.Value) // loop until we know whether we can return or not
        {
          subtractDone = subtract.MoveNext();
        }

      } while(!subtractDone && subtract.Current == current.Value); // if it's in both enumerators, we can't return it

      // at this point, subtract is done or subtract.Current is greater than current.Value, so we can return it
      return true;
    }

    public void Reset()
    {
      current = null;
      subtract.Reset();
      subtractDone = false;
      add.Reset();
    }

    object IEnumerator.Current
    {
      get { return Current; }
    }

    void IDisposable.Dispose()
    {
      add.Dispose();
      subtract.Dispose();
    }

    readonly IEnumerator<uint> add, subtract;
    bool subtractDone;
    uint? current;
  }

  public IEnumerator<uint> GetEnumerator()
  {
    return new SubtractionEnumerator(add.GetEnumerator(), subtract.GetEnumerator());
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  readonly IEnumerable<uint> add, subtract;
}
#endregion

#region UnionIterator
/// <summary>An iterator that represents the union of several other iterators. The iterators must return uints
/// in a sorted order, and this iterator will do the same.
/// </summary>
public sealed class UnionIterator : IEnumerable<uint>
{
  public UnionIterator(params IEnumerable<uint>[] iterators) : this((IList<IEnumerable<uint>>)iterators) { }

  public UnionIterator(IList<IEnumerable<uint>> iterators)
  {
    if(iterators == null || iterators.Count == 0) throw new ArgumentException();
    this.iterators = iterators;
  }

  sealed class UnionEnumerator : IEnumerator<uint>
  {
    public UnionEnumerator(IEnumerator<uint>[] enumerators)
    {
      this.enumerators = enumerators;
      this.done        = new bool[enumerators.Length];
    }

    public uint Current
    {
      get
      {
        if(!current.HasValue) throw new InvalidOperationException();
        return current.Value;
      }
    }

    public bool MoveNext()
    {
      int first = -1;

      if(!current.HasValue) // if we're at the beginning, attempt to move all enumerators so they all have a current
      {                     // value or are marked as done
        for(int i=0; i<enumerators.Length; i++)
        {
          if(!done[i])
          {
            if(!enumerators[i].MoveNext()) done[i] = true;
            else if(first == -1) first = i;
          }
        }
      }
      else // otherwise, we can't move all enumerators because that may drop values. what we'll do instead is move all
      {    // enumerators with a value equal to the current value.
        for(int i=0; i<enumerators.Length; i++)
        {
          if(!done[i])
          {
            Debug.Assert(enumerators[i].Current >= current.Value);
            if(enumerators[i].Current == current.Value && !enumerators[i].MoveNext())
            {
              done[i] = true;
            }
            else if(first == -1)
            {
              first = i;
            }
          }
        }
      }

      if(first == -1) // if they were all done, so are we.
      {
        current = null;
        return false;
      }

      // at this point, we have an enumerator with a value. but we'll continue searching until we find the one with the
      // smallest value.
      uint value = enumerators[first].Current;
      for(int i=first+1; i<enumerators.Length; i++)
      {
        if(!done[i] && enumerators[i].Current < value)
        {
          value = enumerators[i].Current;
        }
      }

      current = value;
      return true;
    }

    public void Reset()
    {
      current = null;
      for(int i=0; i<enumerators.Length; i++)
      {
        enumerators[i].Reset();
      }
      for(int i=0; i<done.Length; i++)
      {
        done[i] = false;
      }
    }

    object IEnumerator.Current
    {
      get { return Current; }
    }

    void IDisposable.Dispose()
    {
      foreach(IEnumerator<uint> enumerator in enumerators)
      {
        enumerator.Dispose();
      }
    }

    readonly IEnumerator<uint>[] enumerators;
    readonly bool[] done;
    uint? current;
  }

  public IEnumerator<uint> GetEnumerator()
  {
    IEnumerator<uint>[] enumerators = new IEnumerator<uint>[iterators.Count];
    for(int i=0; i<enumerators.Length; i++)
    {
      enumerators[i] = iterators[i].GetEnumerator();
    }
    return new UnionEnumerator(enumerators);
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  readonly IList<IEnumerable<uint>> iterators;
}
#endregion

} // namespace Jappy