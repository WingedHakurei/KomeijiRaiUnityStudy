using System;
using Common.ORM;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.DB
{
    [Serializable]
    public class AccountDB : DBEntity
    {
        [BsonConstructor]
        public AccountDB(string account, string md5Str)
        {
            this.Account = account;

            this.MD5str = md5Str;
        }

        /// <summary>
        /// 账号名
        /// </summary>
        /// <value></value>
        public string Account { get; set; }

        /// <summary>
        /// 账号名和密码生成的MD5码
        /// </summary>
        /// <value></value>
        public string MD5str { get; set; }
    }
}