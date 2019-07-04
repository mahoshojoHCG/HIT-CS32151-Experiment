namespace VideoManager
{
    public class Config
    {
        /// <summary>
        ///     连接数据库的字符串
        /// </summary>
        public string SqlConnectionString { get; set; }

        /// <summary>
        ///     保存的用户名
        /// </summary>
        public string SavedUsername { get; set; }

        /// <summary>
        ///     保存的密码
        /// </summary>
        public string SavedPassword { get; set; }


        /// <summary>
        ///     是否使用外部程序播放
        /// </summary>
        public bool ExternalPlay { get; set; }
    }
}