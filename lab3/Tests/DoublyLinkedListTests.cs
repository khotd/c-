using Xunit;
using System;
using System.Collections.Generic;
using Lab3.Collections;

public class DoublyLinkedListTests
{
    [Fact]
    public void Add_IncreasesCount()
    {
        var list = new DoublyLinkedList<int>();
        list.Add(10);
        list.Add(20);
        list.Add(30);
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void AddFirst_AddsToBeginning()
    {
        var list = new DoublyLinkedList<int> { 2, 3, 4 };
        list.AddFirst(1);
        Assert.Equal(4, list.Count);
        Assert.Equal(1, list[0]);
        Assert.Equal(2, list[1]);
    }

    [Fact]
    public void Contains_ReturnsTrueForExistingItem()
    {
        var list = new DoublyLinkedList<string> { "cat", "dog", "bird" };
        Assert.True(list.Contains("dog"));
        Assert.False(list.Contains("fish"));
    }

    [Fact]
    public void Remove_DeletesItem()
    {
        var list = new DoublyLinkedList<string> { "red", "green", "blue", "yellow" };
        bool removed = list.Remove("green");
        Assert.True(removed);
        Assert.Equal(3, list.Count);
        Assert.False(list.Contains("green"));
    }

    [Fact]
    public void Insert_AddsAtPosition()
    {
        var list = new DoublyLinkedList<string> { "start", "end" };
        list.Insert(1, "middle");
        Assert.Equal(3, list.Count);
        Assert.Equal("start", list[0]);
        Assert.Equal("middle", list[1]);
        Assert.Equal("end", list[2]);
    }

    [Fact]
    public void Indexer_GetAndSetWork()
    {
        var list = new DoublyLinkedList<int> { 5, 10, 15 };
        Assert.Equal(10, list[1]);
        list[1] = 12;
        Assert.Equal(12, list[1]);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var list = new DoublyLinkedList<string> { "a", "b", "c", "d", "e" };
        list.Clear();
        Assert.Equal(0, list.Count);
    }

    [Fact]
    public void IndexOf_ReturnsCorrectIndex()
    {
        var list = new DoublyLinkedList<string> { "first", "second", "third" };
        Assert.Equal(1, list.IndexOf("second"));
        Assert.Equal(-1, list.IndexOf("fourth"));
    }

    [Fact]
    public void RemoveFirst_RemovesFirstElement()
    {
        var list = new DoublyLinkedList<int> { 1, 2, 3 };
        list.RemoveFirst();
        Assert.Equal(2, list.Count);
        Assert.Equal(2, list[0]);
    }

    [Fact]
    public void RemoveLast_RemovesLastElement()
    {
        var list = new DoublyLinkedList<int> { 1, 2, 3 };
        list.RemoveLast();
        Assert.Equal(2, list.Count);
        Assert.Equal(2, list[1]);
    }

    [Fact]
    public void Enumerates_Correctly()
    {
        var list = new DoublyLinkedList<int> { 1, 2, 3, 4, 5 };
        int sum = 0;
        foreach (var item in list) sum += item;
        Assert.Equal(15, sum);
    }
}