using spinner;

namespace __Tests__;

public partial class TestInputs
{
    private static IParser Parser = new CLIArgParser();
    public static IEnumerable<object[]> CLIIntputs =
    [
        // Simple command
        new object[]
        {
            Parser,
            "run   ",
            new CommandToken() { Name = new(0, 3) },
        },
        // Simple command with 1 option alias
        new object[]
        {
            Parser,
            "run -f ./testFile.xml",
            new CommandToken()
            {
                Name = new(0, 3),
                Options = [new CLIOptionToken() { Value = new(7, 14), Key = new(4, 2) }],
            },
        },
        new object[]
        {
            Parser,
            "run -f ./testFile.xml --debug",
            new CommandToken()
            {
                Name = new(0, 3),
                Options =
                [
                    new CLIOptionToken() { Value = new(7, 14), Key = new(4, 2) },
                    new CLIOptionToken() { Key = new(22, 7) },
                ],
            },
        },
        // Simple command with 1 option
        new object[]
        {
            Parser,
            "run --file ./testFile.xml",
            new CommandToken()
            {
                Name = new(0, 3),
                Options = [new CLIOptionToken() { Key = new(4, 6), Value = new(11, 14) }],
            },
        },
        // Command with option and arg
        new object[]
        {
            Parser,
            "run --output-file ./logfile.txt ./testFile.xml",
            new CommandToken()
            {
                Name = new(0, 3),
                Arg = new(32, 14),
                Options = [new CLIOptionToken() { Key = new(4, 13), Value = new(18, 13) }],
            },
        },
        new object[]
        {
            Parser,
            "run ./testFile.xml -o ./logfile.txt",
            new CommandToken()
            {
                Name = new(0, 3),
                Arg = new(4, 14),
                Options = [new CLIOptionToken() { Key = new(19, 2), Value = new(22, 13) }],
            },
        },
        new object[]
        {
            Parser,
            "run ./testFile.xml -h",
            new CommandToken()
            {
                Name = new(0, 3),
                Arg = new(4, 14),
                Options = [new CLIOptionToken() { Key = new(19, 2) }],
            },
        },
        new object[]
        {
            Parser,
            "run -f ./testFile.xml -o ./logfile.txt",
            new CommandToken()
            {
                Name = new(0, 3),
                Options =
                [
                    new CLIOptionToken() { Key = new(4, 2), Value = new(7, 14) },
                    new CLIOptionToken() { Key = new(22, 2), Value = new(25, 13) },
                ],
            },
        },
        // TODO Add subcommand test/implementation when needed
    ];

    public static IEnumerable<object[]> AppInitShouldThrow =
    [
        new object[] { "run -x", new UnknownOptionExeption("x") },
        new object[] { "runx", new UnknownCommandExeption("runx") },
        new object[] { "run", new MissingCommandArgument("input file") },
        new object[] { "run -f ", new MissingOptionArgument("f") },
        new object[] { "run -f", new MissingOptionArgument("f") },
        new object[] { "run -o ./logfile.txt", new MissingCommandArgument("input file") },
        new object[]
        {
            "run -f ./testFile.xml -o ./logfile.txt -x",
            new UnknownOptionExeption("x"),
        },
        new object[] { "run ./testFile.xml -o ./logfile.txt -x", new UnknownOptionExeption("x") },
    ];
}
