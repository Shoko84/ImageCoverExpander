using System.Runtime.CompilerServices;
using ImageCoverExpander.Models;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace ImageCoverExpander
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        public bool RegenerateConfig = true;
        public Float4 ButtonColor = new Float4(0, 0, 0, 0.5f);
    }
}
