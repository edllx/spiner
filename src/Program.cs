using spinner;

string input = "jhon";

var parser = new KeyParser();

var res = parser.Parse(input);

//Console.WriteLine($"{res.ToString(input)}");

(int, int)[] dep = [
    (1,0),
    (2,0),
    (3,1),
    (3,2),
];

var order = Tools.TopoSort(dep, 4);

Console.WriteLine($"[{string.Join(",", order)}]");
