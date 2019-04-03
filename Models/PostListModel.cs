using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NewsApplication.Library.Database;
using System.Data;

namespace NewsApplication.Models
{
    public class PostListModel
    {
        public List<Post> list;
        IDatabaseUtility connection;
        public PostListModel()
        {
            this.list = new List<Post>();
        }
        public PostListModel(IDatabaseUtility connection) : this()
        {
            this.connection = connection;
        }
        public List<Post> GetAll()
        {
            list.Clear();
            List<int> ids = new List<int>();
            using (IDataReader result = this.connection.select("*").from("post").Execute())
            {
                ids.Add((int)result["id"]);
            }

            foreach (int id in ids)
            {
                Post post = new Post(this.connection);
                post.id = id;
                post.Load();
                this.list.Add(post);
            }
            return this.list;
        }

        public List<Post> GetLimit(int s, int t)
        {
            list.Clear();
            List<int> ids = new List<int>();
            using (IDataReader result = this.connection.select("*").from("post").limit(s, t).Execute())
            {
                while (result.Read())
                {
                    ids.Add((int)result["id"]);
                }
            }


            foreach (int id in ids)
            {
                Post post = new Post(this.connection);
                post.id = id;
                post.Load();
                this.list.Add(post);
            }
            return this.list;
        }
        public int GetTotal()
        {
            using (IDataReader result = this.connection.select("count(*)").from("post").Execute())
            {
                result.Read();
                return result.GetInt32(0);
            }
        }
        public int GetTotalWhere(string where)
        {
            using (IDataReader result = this.connection.select("COUNT(*)").from("post").where(where).Execute())
            {
                result.Read();
                return result.GetInt32(0);
            }
        }
        public List<Post> GetWhere(string where, int s = -1, int t = -1)
        {
            this.list.Clear();
            List<int> ids = new List<int>();
            if (s == -1)
            {
                using (IDataReader result = this.connection.select("*").from("post").where(where).Execute())
                {
                    while (result.Read())
                    {
                        ids.Add((int)result["id"]);
                    }
                }
            }
            else
            {
                using (IDataReader result = this.connection.select("*").from("post").where(where).limit(s, t).Execute())
                {
                    while (result.Read())
                    {
                        ids.Add((int)result["id"]);
                    }
                }
            }

            foreach (int id in ids)
            {
                Post post = new Post(this.connection);
                post.id = id;
                post.Load();
                this.list.Add(post);
            }
            return this.list;
        }
    }
}