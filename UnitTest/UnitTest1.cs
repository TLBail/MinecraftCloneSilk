using NUnit.Framework;

namespace UnitTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void Test1()
    {
        Console.WriteLine("test1");
        Assert.Pass();
    }
}