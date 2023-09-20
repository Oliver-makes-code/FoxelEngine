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
        testSuites.Add(new ChunkBlockPosSuite());
    }
    
    public static void RunTests() {
        RegisterTests();

        bool hasFailed = false;

        var logger = LogUtil.PlatformLogger;
        foreach(var s in testSuites) {
            s.Run();
            
            if (s.Failed) {
                logger.Error(s.FormattedAssertions());
                hasFailed = true;
            } else {
                logger.Info (s.FormattedAssertions());
            }
        }

        if (hasFailed) {
            Environment.Exit(-1);
        }
    }
}
