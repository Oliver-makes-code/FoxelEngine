using Voxel.Test;
using Voxel.Test.Tests;
using Voxel.Common.World;
using Voxel.Common;
using System.Collections.Generic;
using System;

namespace Voxel.Test;

public static class TestRegistry {
    private static List<TestSuite> testSuites = new();
    public static void RegisterTests() {
        testSuites.Add(new BlockViewSuite());
        testSuites.Add(new AABBSuite());
    }

    public static void Main() {
        RegisterTests();

        bool hasFailed = false;
        
        foreach(var s in testSuites) {
            s.Run();

            if (s.Failed) {
                Console.Error.WriteLine(s.FormattedAssertions());
                hasFailed = true;
            } else {
                Console.WriteLine(s.FormattedAssertions());
            }
        }

        if (hasFailed) {
            Environment.Exit(-1);
        }
    }
}
