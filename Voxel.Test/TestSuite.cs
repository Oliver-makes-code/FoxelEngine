using NLog;
using NLog.Targets;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Voxel.Test;

public readonly struct AssertionResult {

    public AssertionResult(string parentTest, bool didSucceed, string assertionDescription) { parentTestName = parentTest; description = assertionDescription; success = didSucceed; }

    public readonly string parentTestName;
    public readonly string description;
    public readonly bool success;

    public static bool operator ==(AssertionResult lhs, AssertionResult rhs) =>
        lhs.parentTestName == rhs.parentTestName && lhs.description == rhs.description;
    public static bool operator !=(AssertionResult lhs, AssertionResult rhs) =>
        !(lhs == rhs);

    public override bool Equals(object? obj) {
        //Check for null and compare run-time types.
        if ((obj == null) || !GetType().Equals(obj.GetType())) {
            return false;
        }
        else {
            AssertionResult a = (AssertionResult)obj;
            return a == this;
        }
    }
    public override int GetHashCode() {
        return parentTestName.GetHashCode() * description.GetHashCode();
    }
}

public abstract class TestSuite {
    public static readonly Logger Log = LogManager.GetCurrentClassLogger();
       
    public bool Failed { get; private set; }
    public List<AssertionResult> Run() {
        var tests = DefineTests();
        
        foreach(var t in tests) {
            currentTest = t.Key;
            t.Value();
        }
        
        return assertions;
    }
    public string FormattedAssertions() {
       
        // Coallate all duplicate assertion results into a single object for cleaner printing later
        // Not using a direct dictionary here to preserve order of assertions
        List<AssertionResultMetadata> metadata = new();
        Dictionary<AssertionResult, int> assertionToIdx = new();
        foreach(var a in assertions) {
            
            // add this assertion to the table if it's missing
            if(!assertionToIdx.ContainsKey(a)) {
                assertionToIdx.Add(a, metadata.Count);
                metadata.Add(new(a));
            }

            var m = metadata[assertionToIdx[a]];

            if (a.success) m.successes++;
            else           m.fails++;
        }

        string output = $"{GetType().Name}:";
        string prevTest = "";
        foreach(var m in metadata) {

            // if this is the start of a new test's results
            if(prevTest != m.result.parentTestName) {
                output += $"\n{m.result.parentTestName}:";
            }
            prevTest = m.result.parentTestName;

            output += '\n';

            string status = "";
            if (m.successes + m.fails > 1) status = $"Successes: {m.successes} / Fails: {m.fails}";
            else if (m.successes == 1)     status =  "Success";
            else                           status =  "Fail";

            output += $"\t{m.result.description}: {status}";
        }

        return output;
    }

    protected delegate void Test();
    protected abstract Dictionary<string, Test> DefineTests();

    private List<AssertionResult> assertions = new();
    private string currentTest = "No current test";
    protected bool Assert(bool condition, string description) {
        assertions.Add(new(currentTest, condition, description));
        Failed |= !condition;
        return condition;
    }

    private class AssertionResultMetadata {
        public AssertionResultMetadata(AssertionResult assertionResult) {
            result = assertionResult;
        }

        public readonly AssertionResult result;
        public uint successes = 0;
        public uint fails = 0;
    }
}
