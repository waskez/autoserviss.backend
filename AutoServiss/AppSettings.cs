namespace AutoServiss
{
    public class AppSettings
    {
        public bool FirstRun { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int Expiration { get; set; }
        public string ConnectionString { get; set; }
        public string EncryptionKey { get; set; }
        public string SecretKey { get; set; }
        public string FromEmail { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string BackupFolderName { get; set; }
    }
}