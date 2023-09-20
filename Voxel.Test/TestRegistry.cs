﻿using Voxel.Test;
using Voxel.Test.Tests;
using Voxel.Common.World;
using Voxel.Common;

namespace Voxel.Test;

public static class TestRegistry {
    private static List<TestSuite> testSuites = new();
    public static void RegisterTests() {
        testSuites.Add(new ChunkPosSuite());
    }
    
    public static void RunTests() {
        RegisterTests();

        var logger = LogUtil.PlatformLogger;
        foreach(var s in testSuites) {
            s.Run();
            
            if (s.Failed) logger.Error(s.FormattedAssertions());
            else          logger.Info (s.FormattedAssertions());
        }
    }
}