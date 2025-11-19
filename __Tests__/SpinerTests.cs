namespace __Tests__;

public partial class TestFiles
{
    public Dictionary<string, string> Files = [];

    public TestFiles()
    {
        foreach (var x in Directory.EnumerateFiles("./Files"))
        {
            string fileName = x.Split("/").Last();
            Files[fileName] = File.ReadAllText(x);
        }
    }
}

[CollectionDefinition("test-files")]
public class TestFilesEnv : ICollectionFixture<TestFiles> { }

[Collection("test-files")]
public class TestParser
{
    TestFiles Files;

    public TestParser(TestFiles files)
    {
        Files = files;
    }

    [Fact]
    public void Test1()
    {
        string fileName = "ServiceKey.xml";
        Files.Files.TryGetValue(fileName, out var content);
        Assert.SkipWhen(content is null, $"Test file not found Files/{fileName}");
        Assert.True(true);
    }
}
