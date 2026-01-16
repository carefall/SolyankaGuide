namespace SolyankaGuide.Internals
{
    public class SubButton
    {

        public SubButton(string name, string header, int iconId, string imagePath, string text) {
            this.name = name;
            this.header = header;
            this.iconId = iconId;
            this.imagePath = imagePath;
            this.text = text;
        }
        public string name { get; set; }
        public string header { get; set; }
        public int iconId { get; set; }
        public string imagePath { get; set; }
        public string text { get; set; }

        public SubControl BuildSubButtonUI()
        {
            SubControl subControl = new SubControl();
            subControl.SubImage.Source = IconStorage.GetById(iconId);
            subControl.SubName.Text = name;
            subControl.relatedButton = this;
            return subControl;
        }
    }
}
