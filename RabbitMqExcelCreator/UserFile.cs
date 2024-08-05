namespace RabbitMqExcelCreator
{
    public enum FileStatus : byte
    {
        InProgress,
        Completed
    }
    public class UserFile
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string? FilePath { get; set; }
        public DateTime? CreatedAt { get; set; }
        public FileStatus FileStatus { get; set; }
    }
}
