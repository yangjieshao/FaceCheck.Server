using System;
using System.Text.Json.Serialization;

namespace FaceCheck.Server.Model
{
    /// <summary>
    /// </summary>
    public class SetPicInfo
    {
        [JsonPropertyName("pic1")]
        public byte[] Pic1 { set; get; }

        [JsonPropertyName("pic2")]
        public byte[] Pic2 { set; get; }

        /// <summary>
        /// </summary>
        [JsonPropertyName("pic1_url")]
        public string Pic1Url { get; set; }

        /// <summary>
        /// <see cref="Pic1Url" />
        /// </summary>
        [JsonPropertyName("pic1Url")]
        [Obsolete("请使用Pic1Url")]
        public string Pic1Url_
        {
            set
            {
                if (string.IsNullOrEmpty(Pic1Url))
                {
                    Pic1Url = value;
                }
            }
            get
            {
                return Pic1Url;
            }
        }

        [JsonPropertyName("pic2_url")]
        public string Pic2Url { set; get; }

        /// <summary>
        /// <see cref="Pic2Url" />
        /// </summary>
        [JsonPropertyName("pic2Url")]
        [Obsolete("请使用Pic2Url")]
        public string Pic2Url_
        {
            set
            {
                if (string.IsNullOrEmpty(Pic2Url))
                {
                    Pic2Url = value;
                }
            }
            get
            {
                return Pic2Url;
            }
        }
    }

    /// <summary>
    /// 日志用
    /// </summary>
    public class SetPicInfo4Log
    {
        public bool Pic1 { set; get; } = false;

        public string Pic1Path { set; get; }

        public bool Pic2 { set; get; }

        public string Pic2Path { set; get; }
    }
}