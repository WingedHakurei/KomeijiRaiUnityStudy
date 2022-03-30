using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Common.ORM
{
    public class Table<T> where T : DBEntity
    {
        /// <summary>
        /// 对应的集合的引用
        /// </summary>
        private IMongoCollection<T> collection = Proxy<T>.Collection;

        /// <summary>
        /// 增加一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Add(T entity)
        {
            try
            {
                collection.InsertOne(entity);

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public bool Delete(T entity, Expression<Func<T, bool>> conditions = null)
        {
            try
            {
                string _id = string.Empty;

                if (conditions == null)
                {
                    foreach (PropertyInfo item in entity.GetType().GetProperties())
                    {
                        if (item.Name == "ID" && item.GetValue(entity) != null)
                        {
                            _id = item.GetValue(entity).ToString();

                            DeleteResult result = collection.DeleteOne(new BsonDocument("_id", BsonValue.Create(new ObjectId(_id))));

                            return result.IsAcknowledged;
                        }

                    }
                }

                DeleteResult res = collection.DeleteOne(conditions);

                return res.IsAcknowledged;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 更新一条记录
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public bool Update(T entity, Expression<Func<T, bool>> conditions = null)
        {
            try
            {
                ObjectId _id;

                var options = new ReplaceOptions() { IsUpsert = true };

                if (conditions == null)
                {
                    foreach (var item in entity.GetType().GetProperties())
                    {
                        if (item.Name == "ID" && item.GetValue(entity) != null)
                        {
                            _id = new ObjectId(item.GetValue(entity).ToString());

                            ReplaceOneResult result = collection.ReplaceOne(new BsonDocument("_id", BsonValue.Create(_id)), entity, options);

                            return result.IsAcknowledged;
                        }
                    }
                }

                ReplaceOneResult res = collection.ReplaceOne(conditions, entity, options);

                return res.IsAcknowledged;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 查找一条记录
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public List<T> Find(Expression<Func<T, bool>> conditions = null)
        {
            try
            {
                if (conditions == null)
                {
                    conditions = t => true;
                }

                return collection.Find(conditions).ToList() ?? new List<T>();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}