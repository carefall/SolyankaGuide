namespace SolyankaGuide.Internals
{
    internal class Element
    {

        public string? Name { get; set; }
        public string? Header { get; set; }
        public string[]? Text { get; set; }
        public string? ImagePath { get; set; }
        public bool Centered { get; set; }
        public Description[]? Descriptions { get; set; }

    }
}
