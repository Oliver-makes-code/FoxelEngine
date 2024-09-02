namespace Foxel.Common.World.Content;

public record ContentStore<TValue>(ContentStage Stage) {
    
}

public enum ContentStage {
    Static,
    Dynamic
}
