namespace BookManager.Model
{
    public class DirectoryItem
    {
        public DirectoryItemType Type { get; set; }
        public string Name { get; set; }
        public bool EditMode { get; set; } = false;
    }
}
