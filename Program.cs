using Spectre.Console;

// Constants
const char epsilon = 'ε';
const char empty = '∅';

//// List of all states
//List<string> states = new List<string> { "q0", "q1", "q2", "q3" };
//// List of the alphabet
//List<char> alphabet = new List<char> { '0', '1' };
//// The start state
//string startState = "q0";
//// List of accept states
//List<string> acceptStates = new List<string> { "q3" };
//// Transitions
//Dictionary<(string, char), List<string>> transitions = new Dictionary<(string, char), List<string>>
//{
//    { ("q0", '0'), new List<string> { "q0", "q1" } },
//    { ("q0", '1'), new List<string> { "q0" } },
//    { ("q1", '0'), new List<string> { "q2" } },
//    { ("q1", '1'), new List<string> { "q2" } },
//    { ("q2", '0'), new List<string> { "q3" } },
//    { ("q2", '1'), new List<string> { "q3" } },
//};

// List of all states
List<string> states = new List<string>();
// List of the alphabet
List<char> alphabet = new List<char>();
// The start state
string startState = "";
// List of accept states
List<string> acceptStates = new List<string>();
// Transitions
Dictionary<(string, char), List<string>> transitions = new Dictionary<(string, char), List<string>>();
//Dictionary of regular expressions for each state
Dictionary<string, string> stateRegex = new Dictionary<string, string>();
Dictionary<string, bool> stateChanged = new Dictionary<string, bool>();

// Print the header
PrintHeader();

// Get all the required information
GetStates();
GetAlphabet();
GetStartState();
GetAcceptStates();
GetTransitions();
PrintDefinition();

// Start to do the conversion
ConvertNFAtoRegex();

// To pause the program
Console.ReadLine();

void ConvertNFAtoRegex()
{
    Dictionary<(string, string), string> intermediateRegex = new Dictionary<(string, string), string>();

    // Step 1: Initialization
    foreach (var state1 in states)
    {
        foreach (var state2 in states)
        {
            List<string> transitionRegexes = new List<string>();

            foreach (var symbol in alphabet)
            {
                var key = (state1, symbol);
                if (transitions.ContainsKey(key) && transitions[key].Contains(state2))
                {
                    transitionRegexes.Add(symbol.ToString());
                }
            }

            if (transitionRegexes.Count > 0)
            {
                intermediateRegex[(state1, state2)] = string.Join(" + ", transitionRegexes);
            }
            else
            {
                intermediateRegex[(state1, state2)] = empty.ToString();
            }
        }
    }


    // Step 2: State Elimination
    // Step 2: State Elimination
    foreach (var r in states)
    {
        if (r == startState || acceptStates.Contains(r)) continue;

        foreach (var p in states)
        {
            foreach (var q in states)
            {
                if (p == r || q == r) continue;

                if (intermediateRegex[(p, r)] != empty.ToString() && intermediateRegex[(r, q)] != empty.ToString())
                {
                    string part1 = intermediateRegex[(p, r)];
                    string loopR = intermediateRegex[(r, r)];
                    string part2 = intermediateRegex[(r, q)];
                    string newRegex = $"({part1})({loopR})*({part2})";

                    if (intermediateRegex[(p, q)] != empty.ToString())
                    {
                        intermediateRegex[(p, q)] += " + " + newRegex;
                    }
                    else
                    {
                        intermediateRegex[(p, q)] = newRegex;
                    }
                }
            }
        }

        // Set all transitions involving r to empty.
        foreach (var s in states)
        {
            intermediateRegex[(r, s)] = empty.ToString();
            intermediateRegex[(s, r)] = empty.ToString();
        }
    }

    // Step 3: Final Expression
    string regex = intermediateRegex[(startState, acceptStates[0])];

    // Cleanup the final regular expression
    regex = regex.Replace(epsilon.ToString(), "");
    regex = regex.Replace(empty.ToString(), "");
    regex = regex.Replace("()", "");
    regex = regex.Replace(" *", "*");
    regex = regex.Replace(" + ", "+");

    Console.WriteLine("Regular Expression: " + regex);
}

void GetStates()
{
    // Get the set of states
    string statesInput = AnsiConsole.Ask<string>("Enter the set of states in format 's0,s1,s2': ");
    states = statesInput.Split(',').ToList();

    // Initialize all states with an empty string in the stateRegex dictionary
    foreach (var state in states)
    {
        stateRegex[state] = empty.ToString();
    }

    // Clear the console
    ClearConsole();
}

void GetAlphabet()
{
    String alphabetInput = AnsiConsole.Ask<string>("Enter the alphabet in format 'a,b,c,d,e,f': ");

    // Split the input string by commas and add the characters to a List<char>
    alphabet = alphabetInput.Split(',')
                           .Select(s => s.Trim()[0])
                           .ToList();

    // Clear the console
    ClearConsole();
}

void GetStartState()
{
    // Create selection prompt for start state
    startState = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Select the start state")
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more states)[/]")
            .AddChoices(states));

    // Add the start state to the stateRegex dictionary
    stateRegex[startState] = epsilon.ToString();

    // Clear the console
    ClearConsole();
}

void GetAcceptStates()
{
    // Create selection prompt for accept states
    acceptStates = AnsiConsole.Prompt(
        new MultiSelectionPrompt<string>()
            .Title("Select the accept states")
            .Required()
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more states)[/]")
            .InstructionsText(
                "[grey](Press [blue]<space>[/] to select a state, " +
                "[green]<enter>[/] to accept)[/]")
            .AddChoices(states));

    // Clear the console
    ClearConsole();
}

void GetTransitions()
{
    // Add Epsilon to the alphabet
    alphabet.Add(epsilon);

    // While loop to get transitions
    while (true)
    {
        // Table of current transitions
        var table = new Table()
            .Title("Current Transitions")
            .BorderColor(Color.Red)
            .Border(TableBorder.Rounded)
            .AddColumn("Current State")
            .AddColumn("Input Character")
            .AddColumn("Next State")
            .Centered();

        // Add all transitions to the table
        foreach (var transition in transitions)
        {
            string currState = transition.Key.Item1.ToString();
            string inpStr = transition.Key.Item2.ToString();
            string nxtState = string.Join(",", transition.Value); // Join the list of next states into a single string

            table.AddRow(currState, inpStr, nxtState);
        }

        // Print the table
        AnsiConsole.Write(table);

        // Break
        Console.WriteLine();

        // Create a list of states to choose from
        var stateChoices = new List<string>(states);

        // Add the "Done" option to the list
        stateChoices.Add("Done");

        // Create selection prompt for start state
        string currentState = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select the current state")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more states)[/]")
                .AddChoices(stateChoices));

        // Check if the user is done
        if (currentState == "Done")
        {
            // Clear the console
            ClearConsole();

            // Break
            break;
        }

        char inputCharacter = AnsiConsole.Prompt(
            new SelectionPrompt<char>()
                .Title("Select the input character")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more characters)[/]")
                .AddChoices(alphabet));

        // Create selection prompt for start state
        List<string> nextStates = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select the next state(s)")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more states)[/]")
                .AddChoices(stateChoices));

        // Check if the user is done
        if (nextStates.Exists(s => s.Equals("Done")))
        {
            // Clear the console
            ClearConsole();

            // Break
            break;
        }

        // Check if the transition already exists
        if (transitions.ContainsKey((currentState, inputCharacter)))
        {
            var existingNextStates = transitions[(currentState, inputCharacter)];

            // Check if the new nextStates are different from the existing ones
            if (!existingNextStates.SequenceEqual(nextStates))
            {
                // Merge the new nextStates with the existing ones
                var mergedNextStates = existingNextStates.Union(nextStates).ToList();

                // Update the transition with the merged nextStates
                transitions[(currentState, inputCharacter)] = mergedNextStates;
            }
            // Otherwise, if the new nextStates are the same as the existing ones, do nothing.
        }
        else
        {
            // Create a new list with the nextStates and add it to the dictionary
            transitions[(currentState, inputCharacter)] = nextStates;
        }

        // Clear the console
        ClearConsole();
    }
}

void PrintDefinition()
{
    // Print the header
    PrintHeader();

    // Table of the definition
    var definitionTable = new Table()
        .Title("Definition")
        .BorderColor(Color.Red)
        .Border(TableBorder.Rounded)
        .AddColumn("States")
        .AddColumn("Alphabet")
        .AddColumn("Start State")
        .AddColumn("Accept States")
        .Centered();

    // Add the states to the table
    definitionTable.AddRow(string.Join(",", states), string.Join(",", alphabet), startState, string.Join(",", acceptStates));

    // Print the definition table
    AnsiConsole.Write(definitionTable);

    // Table of the transitions
    var transitionsTable = new Table()
        .Title("Transitions")
        .BorderColor(Color.Red)
        .Border(TableBorder.Rounded)
        .AddColumn("Current State")
        .AddColumn("Input Character")
        .AddColumn("Next State")
        .Centered();

    // Add all transitions to the table
    foreach (var transition in transitions)
    {
        var nextStateStr = string.Join(",", transition.Value);
        transitionsTable.AddRow(transition.Key.Item1.ToString(), transition.Key.Item2.ToString(), nextStateStr);
    }

    // Print the transition table
    AnsiConsole.Write(transitionsTable);
}

void ClearConsole()
{
    // Clear the console
    Console.Clear();

    // Print the header
    PrintHeader();
}

void PrintHeader()
{
    // Print the header
    AnsiConsole.Write(
    new FigletText("NFA to RegEx")
        .LeftJustified()
        .Color(Color.Blue));

    // By line
    AnsiConsole.Write(
    new FigletText("by Justin Kruskie")
        .RightJustified()
        .Color(Color.Red));

    // Padding
    Console.WriteLine();
    Console.WriteLine();
}
