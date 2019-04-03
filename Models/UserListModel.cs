using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NewsApplication.Library.Database;
using NewsApplication.Models;
using System.Data;

namespace NewsApplication.Models
{
    public class UserListModel
    {
        public IDatabaseUtility connection;
        public List<User> list;
        public UserListModel()
        {
            this.list = new List<User>();
        }
        public UserListModel(IDatabaseUtility connection)
            : this()
        {
            this.connection = connection;
        }
        public List<User> GetAll()
        {
            this.list.Clear();
            List<int> ids = new List<int>();

            using (IDataReader result = this.connection.select("*").from("user").Execute())
            {
                while (result.Read())
                {
                    ids.Add((int)result["id"]);
                }
            }

            foreach (int id in ids)
            {
                User user = new User(connection);
                user.id = id;
                user.Load();
                this.list.Add(user);
            }

            return list;
        }
        public List<User> GetAll(int s, int t)
        {
            this.list.Clear();
            List<int> ids = new List<int>();

            using (IDataReader result = this.connection.select("*").from("user").limit(s, t).Execute())
            {
                while (result.Read())
                {
                    ids.Add((int)result["id"]);
                }
            }

            foreach (int id in ids)
            {
                User user = new User(connection); 
                user.id = id;
                user.Load();
                this.list.Add(user);
            }

            return list;
        }
        public int GetTotal()
        {
            using (IDataReader result = this.connection.select("COUNT(*)").from("user").Execute())
            {
                result.Read();
                return result.GetInt32(0);
            }
        }
        public List<User> GetWhere(string w)
        {
            this.list.Clear();
            List<int> ids = new List<int>();

            using (IDataReader result = this.connection.select("*").from("user").where(w).Execute())
            {
                while (result.Read())
                {
                    ids.Add((int)result["id"]);
                }
            }

            foreach (int id in ids)
            {
                User user = new User(connection);
                user.id = id;
                user.Load();
                this.list.Add(user);
            }

            return list;
        }
        public List<User> GetWhere(string w, int s, int t)
        {
            this.list.Clear();
            List<int> ids = new List<int>();

            using (IDataReader result = this.connection.select("*").from("user").where(w).limit(s, t).Execute())
            {
                while (result.Read())
                {
                    ids.Add((int)result["id"]);
                }
            }

            foreach (int id in ids)
            {
                User user = new User(connection);
                user.id = id;
                user.Load();
                this.list.Add(user);
            }

            return list;
        }
        public int GetWhereTotal(string w)
        {
            using (IDataReader result = this.connection.select("COUNT(*)").from("user").where(w).Execute())
            {
                result.Read();
                return (int)result[0];
            }
        }
    }
}