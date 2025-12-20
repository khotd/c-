using Xunit;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CollectionsPerformanceLab.Tests;

public class CollectionsTests
{
    [Fact]
    public void List_Add_Works()
    {
        var list = new List<int> { 1, 2, 3 };
        Assert.Equal(3, list.Count);
    }
    
    [Fact]
    public void List_Contains_Works()
    {
        var list = new List<int> { 1, 2, 3 };
        Assert.Contains(2, list);
    }
    
    [Fact]
    public void List_Remove_Works()
    {
        var list = new List<int> { 1, 2, 3 };
        list.RemoveAt(0);
        Assert.Equal(2, list.Count);
    }
    
    [Fact]
    public void LinkedList_Add_Works()
    {
        var list = new LinkedList<int>();
        list.AddLast(1);
        list.AddFirst(0);
        Assert.Equal(2, list.Count);
        Assert.Equal(0, list.First!.Value);
    }
    
    [Fact]
    public void Queue_EnqueueDequeue_Works()
    {
        var queue = new Queue<int>();
        queue.Enqueue(1);
        queue.Enqueue(2);
        Assert.Equal(1, queue.Dequeue());
        Assert.Single(queue);
    }
    
    [Fact]
    public void Stack_PushPop_Works()
    {
        var stack = new Stack<int>();
        stack.Push(1);
        stack.Push(2);
        Assert.Equal(2, stack.Pop());
        Assert.Single(stack);
    }
    
    [Fact]
    public void ImmutableList_Add_Works()
    {
        var list = ImmutableList.Create<int>(1, 2, 3);
        var newList = list.Add(4);
        Assert.Equal(3, list.Count);
        Assert.Equal(4, newList.Count);
    }
}