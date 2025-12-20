using Xunit;
using System;
using System.Collections.Generic;
using Lab3.Collections;
public class SimpleListTests
{
    [Fact]
    public void Add_IncreasesCount()
    {
        var list = new SimpleList<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void Contains_ReturnsTrueForExistingItem()
    {
        var list = new SimpleList<string> { "aaa", "b", "cc" };
        Assert.True(list.Contains("b"));
        Assert.False(list.Contains("g"));
    }

    [Fact]
    public void IndexOf_ReturnsCorrectIndex()
    {
        var list = new SimpleList<int> { 10, 20, 30, 40 };
        Assert.Equal(2, list.IndexOf(30));
        Assert.Equal(-1, list.IndexOf(50));
    }

    [Fact]
    public void Remove_DecreasesCount()
    {
        var list = new SimpleList<int> { 1, 2, 3, 4, 5 };
        bool removed = list.Remove(3);
        Assert.True(removed);
        Assert.Equal(4, list.Count);
        Assert.False(list.Contains(3));
    }

    [Fact]
    public void Insert_AddsItemAtCorrectPosition()
    {
        var list = new SimpleList<string> { "a", "c", "d" };
        list.Insert(1, "b");
        Assert.Equal(4, list.Count);
        Assert.Equal("b", list[1]);
        Assert.Equal("c", list[2]);
    }

    [Fact]
    public void Indexer_GetAndSetWork()
    {
        var list = new SimpleList<int> { 100, 200, 300 };
        Assert.Equal(200, list[1]);
        list[1] = 250;
        Assert.Equal(250, list[1]);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var list = new SimpleList<int> { 1, 2, 3, 4, 5 };
        list.Clear();
        Assert.Equal(0, list.Count);
    }

    [Fact]
    public void CopyTo_CopiesToArray()
    {
        var list = new SimpleList<int> { 1, 2, 3 };
        var array = new int[5];
        list.CopyTo(array, 1);
        Assert.Equal(0, array[0]);
        Assert.Equal(1, array[1]);
        Assert.Equal(2, array[2]);
        Assert.Equal(3, array[3]);
        Assert.Equal(0, array[4]);
    }

    [Fact]
    public void Foreach_WorksWithList()
    {
        var list = new SimpleList<int> { 10, 20, 30 };
        int sum = 0;
        foreach (var item in list) sum += item;
        Assert.Equal(60, sum);
    }
}