using System.Windows.Controls.Primitives;

namespace SolyankaGuide.Internals
{
    internal class Category
    {
        public string? Name { get; set; }
        public string[]? ElementsPaths { get; set; }

        public ToggleButton? RelatedButton { get; set; }
    }
}
