using System;
using Common.ORM;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.DB
{
    [Serializable]
    public class UserDB : DBEntity
    {
        [BsonConstructor]
        public UserDB(string nickName, long coin)
        {
            this.NickName = nickName;

            this.Coin = coin;
        }

        /// <summary>
        /// 账号名
        /// </summary>
        /// <value></value>
        public string Account { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        /// <value></value>
        public string NickName { get; set; }

        /// <summary>
        /// 用户金币数量
        /// </summary>
        /// <value></value>
        public long Coin { get; set; }
    }
}