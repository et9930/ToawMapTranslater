using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BaiduTranslate
{
    public enum ErrorCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 52000,
        /// <summary>
        /// 请求超时, 重试
        /// </summary>
        TimeOut = 52001,
        /// <summary>
        /// 系统错误, 重试
        /// </summary>
        SystemError = 52002,
        /// <summary>
        /// 未授权用户, 检查您的 appid 是否正确, 或者服务是否开通
        /// </summary>
        Unauthorized = 52003,
        /// <summary>
        /// 必填参数为空, 检查是否少传参数
        /// </summary>
        RequiredParameterNull = 54000,
        /// <summary>
        /// 签名错误, 请检查您的签名生成方法
        /// </summary>
        SignError = 54001,
        /// <summary>
        /// 访问频率受限, 请降低您的调用频率
        /// </summary>
        AccessFrequencyConstraint = 54003,
        /// <summary>
        /// 账户余额不足, 请前往管理控制台为账户充值
        /// </summary>
        InsufficientAccountBalance = 54004,
        /// <summary>
        /// 长query请求频繁, 请降低长query的发送频率, 3s后再试
        /// </summary>
        LongQueryRequestsFrequent = 54005,
        /// <summary>
        /// 客户端IP非法, 检查个人资料里填写的IP地址是否正确, 可前往管理控制平台修改IP限制，IP可留空
        /// </summary>
        ClientIpIllegal = 58000,
        /// <summary>
        /// 译文语言方向不支持, 检查译文语言是否在语言列表里
        /// </summary>
        LanguageUnsupported = 58001,
        /// <summary>
        /// 服务当前已关闭, 请前往管理控制台开启服务
        /// </summary>
        ServiceClosed = 58002,
        /// <summary>
        /// 认证未通过或未生效, 请前往我的认证查看认证进度
        /// </summary>
        IdentificationNotPassed = 90107,
    }

    public class BaiduTranslate
    {
        [DataContract]
        private class TranslateResult
        {
            [DataMember(Name = "from")] 
            public string From;
            [DataMember(Name = "to")] 
            public string To;
            [DataMember(Name = "trans_result")] 
            public List<TransResult> TransResultList;
            [DataMember(IsRequired = false, Name = "error_code", EmitDefaultValue = true)] 
            public ErrorCode ErrorCode = ErrorCode.Success;
        }

        [DataContract]
        private class TransResult
        {
            [DataMember(Name = "src")] public string Src;
            [DataMember(Name = "dst")] public string Dst;
        }

        /// <summary>
        /// 百度翻译API调用
        /// </summary>
        /// <param name="q">原文</param>
        /// <param name="r">译文</param>
        /// <param name="from">源语言, 默认为en</param>
        /// <param name="to">目标语言, 默认为zh</param>
        /// <returns>错误码</returns>
        public ErrorCode Translate(string q, out string r, string from = "en", string to = "zh")
        {
            // 改成您的APP ID
            const string appId = "20191002000338739";
            var rd = new Random();
            var salt = rd.Next(100000).ToString();
            // 改成您的密钥
            const string secretKey = "vxHv034MW14hSdwbeyl8";
            var sign = EncryptString(appId + q + salt + secretKey);
            var url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
            url += "q=" + HttpUtility.UrlEncode(q);
            url += "&from=" + from;
            url += "&to=" + to;
            url += "&appid=" + appId;
            url += "&salt=" + salt;
            url += "&sign=" + sign;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 6000;
            var response = (HttpWebResponse)request.GetResponse();
            var myResponseStream = response.GetResponseStream();
            if (myResponseStream != null)
            {
                var myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                var retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                ErrorCode errorCode;
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(retString)))
                {
                    var serializer1 = new DataContractJsonSerializer(typeof(TranslateResult));
                    var translateResult = (TranslateResult)serializer1.ReadObject(ms);
                    r = translateResult.ErrorCode == ErrorCode.Success ? translateResult.TransResultList[0].Dst : "";
                    errorCode = translateResult.ErrorCode;
                }

                return errorCode;
            }

            r = "";
            return ErrorCode.TimeOut;
        }

        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="str">要计算的字符串</param>
        /// <returns>MD5值</returns>
        private static string EncryptString(string str)
        {
            var md5 = MD5.Create();
            // 将字符串转换成字节数组
            var byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            var byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            var sb = new StringBuilder();
            foreach (var b in byteNew)
            {
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            }
            // 返回加密的字符串
            return sb.ToString();
        }
    }
}
