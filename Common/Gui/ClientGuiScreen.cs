using Microsoft.VisualBasic;

namespace Voxel.Common.Gui; 

public abstract class ClientGuiScreen {
    
    private Dictionary<string, ServerInteraction> serverInteractions;

    // vix just because it's one line now doesn't mean it will be later
    // please dont make this a => function /srs -emily
    public void RegisterServerInteraction(ServerInteraction interaction) {
        serverInteractions.Add(interaction.Name, interaction);
    }

    public void Interact(string interactionName)
        => serverInteractions[interactionName].Interact();

    public abstract void RegisterServerInteractions();
    
    /// <summary>
    /// Pushes the client's GUI tree onto the GuiCanvas GUI layer stack
    /// </summary>
    public abstract void BuildClientGui();
    
    /// <summary>
    /// Wrapper for things that network with the server
    /// </summary>
    public struct ServerInteraction {

        public readonly string Name;
        private Behavior behavior;
        
        public void Interact()
            => behavior();

        public ServerInteraction(string name, Behavior behavior) {
            Name = name;
            this.behavior = behavior;
        }
        
        public delegate void Behavior();
    }
}
