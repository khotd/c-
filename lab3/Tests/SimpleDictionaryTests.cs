using Xunit;
using System;
using System.Collections.Generic;
using Lab3.Collections;

public class SimpleDictionaryTests
{
    [Fact]
    public void Add_And_Get_Work()
    {
        var dict = new SimpleDictionary<int, string>();
        dict.Add(1, "One");
        dict.Add(2, "Two");
        dict[3] = "Three";
        Assert.Equal("One", dict[1]);
        Assert.Equal("Two", dict[2]);
        Assert.Equal("Three", dict[3]);
    }

    [Fact]
    public void ContainsKey_ReturnsCorrectValue()
    {
        var dict = new SimpleDictionary<string, int>
        {
            { "aaaa", 1 },
            { "bbbb", 2 },
            { "cccc", 3 }
        };
        Assert.True(dict.ContainsKey("bbbb"));
        Assert.False(dict.ContainsKey("o"));
    }

    [Fact]
    public void TryGetValue_Works()
    {
        var dict = new SimpleDictionary<string, double>
        {
            { "pi", 3.14 },
            { "e", 2.71 }
        };
        Assert.True(dict.TryGetValue("pi", out double piValue));
        Assert.Equal(3.14, piValue);
        Assert.False(dict.TryGetValue("sqrt2", out _));
    }

    [Fact]
    public void Remove_DeletesKey()
    {
        var dict = new SimpleDictionary<int, string>
        {
            { 1, "a" },
            { 2, "b" },
            { 3, "c" },
            { 4, "d" }
        };
        bool removed = dict.Remove(2);
        Assert.True(removed);
        Assert.Equal(3, dict.Count);
        Assert.False(dict.ContainsKey(2));
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var dict = new SimpleDictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 },
            { "three", 3 }
        };
        dict.Clear();
        Assert.Equal(0, dict.Count);
        Assert.False(dict.ContainsKey("one"));
    }

    [Fact]
    public void Keys_Collection_ContainsAllKeys()
    {
        var dict = new SimpleDictionary<int, string>
        {
            { 1, "A" },
            { 2, "B" },
            { 3, "C" }
        };
        var keys = dict.Keys;
        Assert.Equal(3, keys.Count);
        Assert.Contains(1, keys);
        Assert.Contains(2, keys);
        Assert.Contains(3, keys);
    }

    [Fact]
    public void Values_Collection_ContainsAllValues()
    {
        var dict = new SimpleDictionary<string, int>
        {
            { "a", 100 },
            { "b", 200 },
            { "c", 300 }
        };
        var values = dict.Values;
        Assert.Equal(3, values.Count);
        Assert.Contains(100, values);
        Assert.Contains(200, values);
        Assert.Contains(300, values);
    }

    [Fact]
    public void Update_Value_Works()
    {
        var dict = new SimpleDictionary<string, string> { { "name", "Имя1" } };
        dict["name"] = "Имя2";
        Assert.Equal("Имя2", dict["name"]);
    }

    [Fact]
    public void AddDuplicateKey_ThrowsException()
    {
        var dict = new SimpleDictionary<int, string> { { 1, "first" } };
        Assert.Throws<ArgumentException>(() => dict.Add(1, "duplicate"));
    }
}