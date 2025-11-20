using spinner;

namespace __Tests__;

public partial class TestFiles
{
    public Dictionary<string, string> Files = [];

    public SpinnerTestToken ExpectedServiceKeyTest
    {
        get
        {
            var txt1 = new TextTestToken() { Body = "Define the structure of each services" };

            var txt2 = new TextTestToken()
            {
                Body =
                    "Key values are resolved when the element is instantiated wich let the posibility ",
            };

            var txt3 = new TextTestToken() { Body = "  to compose key based on other keys" };
            var txt4 = new TextTestToken() { Body = "Services Comment " };
            var txt5 = new TextTestToken() { Body = "Service Comment " };

            var exepted = new SpinnerTestToken()
            {
                Children =
                [
                    new XMLCommentTestToken() { Children = [txt1] },
                    new XMLCommentTestToken() { Children = [txt2, txt3] },
                    new ServicesTestToken()
                    {
                        Children =
                        [
                            new XMLCommentTestToken() { Children = [txt4] },
                            new ServiceTestToken()
                            {
                                Children =
                                [
                                    new XMLAttributeTestToken() { Name = "name", Value = "db" },
                                    new XMLAttributeTestToken()
                                    {
                                        Name = "image",
                                        Value = "postgres:17",
                                    },
                                    new SpinnerKeyTestToken()
                                    {
                                        KeyType = "Key",
                                        Name = "POSTGRES_USER",
                                        Value = "spiner",
                                    },
                                    new XMLCommentTestToken() { Children = [txt5] },
                                    new SpinnerKeyTestToken()
                                    {
                                        KeyType = "GeneratedKey",
                                        Name = "POSTGRES_PASSWORD",
                                        Len = "32",
                                    },
                                    new SpinnerKeyTestToken()
                                    {
                                        KeyType = "GeneratedKey",
                                        Name = "POSTGRES_DB",
                                        Len = "10",
                                    },
                                    new SpinnerKeyTestToken()
                                    {
                                        KeyType = "Key",
                                        Name = "DB_CONNECTION_STRING",
                                        Value =
                                            "Server=${CONTAINER_NAME};Port=5432;Database=${POSTGRES_DB};User ID=${POSTGRES_USER};Password=${POSTGRES_PASSWORD};",
                                    },
                                ],
                            },
                            new ServiceTestToken()
                            {
                                Children =
                                [
                                    new XMLAttributeTestToken() { Name = "name", Value = "api" },
                                    new XMLAttributeTestToken()
                                    {
                                        Name = "build",
                                        Value = "./API.Dockerfile",
                                    },
                                    new SpinnerKeyTestToken()
                                    {
                                        KeyType = "Key",
                                        Name = "DB_CONNECTION_STRING",
                                    },
                                ],
                            },
                        ],
                    },
                ],
            };

            return exepted;
        }
    }

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
    public void ServiceTest()
    {
        string fileName = "ServiceKey.xml";
        Files.Files.TryGetValue(fileName, out var content);
        Assert.SkipWhen(content is null, $"Test file not found Files/{fileName}");

        SpinnerParser p = new();
        ParseResult actual = p.Parse(content);

        var exepted = Files.ExpectedServiceKeyTest;

        Assert.True(actual.Success);
        Assert.Equal(exepted.ToString(content), actual.ToString(content));
    }

    [Fact]
    public void GenericElementTest()
    {
        string fileName = "GenericElements.xml";
        Files.Files.TryGetValue(fileName, out var content);
        Assert.SkipWhen(content is null, $"Test file not found Files/{fileName}");

        SpinnerParser p = new();
        ParseResult actual = p.Parse(content);

        var exepted = Files.ExpectedServiceKeyTest;
        Assert.True(actual.Success);
        Console.WriteLine(actual.ToString(content));

        Assert.Equal(exepted.ToString(content), actual.ToString(content));
    }
}
