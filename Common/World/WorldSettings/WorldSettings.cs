namespace Voxel.Common.World.WorldSettings; 

// Registry of per-world settings
// Settings are sorted into Groups
// Groups have string-identified Values with arbitrary types
public static class WorldSettings {
    // stores a dictionary of values
    public class Group {
        //private Group() {} // delete the default constructor
        // TODO: Make a constructor that registers values once values are implemented
        
        private Dictionary<string, Value> values = new();

        // TODO: make this return a nullable value?
        public Value this[string key] {
            get => values[key];
            set => values[key] = value; // TODO: It will be annoying to cast to Value when setting these; make an implicit cast for common types?
        }
    }
    // stores data of arbitrary type, and has a way of extracting that type
    // TODO: maybe a binary array (void* equivalent) and a delegate to cast it back to the correct type?
    public struct Value {
        // TODO: temporary test method, delete later
        public bool AsBool() {
            return true;
        }

        public int AsInt() {
            return 0;
        }
    }

    private static Dictionary<string, Group> groups;
    // C# doesnt allow static indexers grrr
    public static Group GetGroup(string groupName) {
        return groups[groupName];
    }
    // value path of the form "groupName:valueName"
    public static Value GetValue(string valuePath) {
        string[] names = valuePath.Split(':');
        return groups[names[0]][names[1]];
    }

    public static void AddGroup(string name, Group group)
        => groups.Add(name, group);

    public static void SanitizeGroupName(ref string name) {
        for (int i = name.Length - 1; i >= 0; i--) {
            if (name[i] == ':') name = name.Remove(i);
        }
    }
    public static void SanitizeValueName(ref string name) {
        for (int i = name.Length - 1; i >= 0; i--) {
            if (name[i] == ':') name = name.Remove(i);
        }
    }
}

internal static class TestThingyToDeleteLater {
    private static void Wawa() {
        // pretend mobGriefing and spawnCap are initialized here \/
        WorldSettings.AddGroup("Mobs", new WorldSettings.Group());
        var mobSettings = WorldSettings.GetGroup("Mobs");
        
        if (mobSettings["mobGriefing"].AsBool()) {
            // esplosion >:3
        }

        int spawnedMobs = 10_000_000; // woa,,
        if (spawnedMobs < mobSettings["spawnCap"].AsInt()) {
            // spawn mobs hehehe
        }
        
        WorldSettings.AddGroup("Environment", new WorldSettings.Group());
        if (WorldSettings.GetValue("Environment:doFireTick").AsBool()) {
            // BURNNNN >:3c
        }
    }
}
