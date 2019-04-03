using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NewsApplication.Models;
using NewsApplication.Library.Database;
using System.Data;

namespace NewsApplication.Models
{
	public class PostImage
	{
        public const string POSTER_IMAGE_DIR = "/upload/posters/";
        public int id { get; set; }
        public int post_id { get; set; }
        public string path { get; set; }
        public DateTime created { get; set; }
        public IDatabaseUtility connection;
        public PostImage()
        {

        }
        public PostImage(IDatabaseUtility connection) : this()
        {
            this.connection = connection;
        }

        public bool Load()
        {
            using (IDataReader result = this.connection.select("*").from("postimage").where("id=" + this.id).Execute())
            {
                if (!result.Read())
                {
                    return false;
                }
                this.post_id = (int)result["post_id"];
                this.path = (string)result["path"];
                this.created = (DateTime)result["created"];
                return true;
            }
        }

        public bool Add()
        {
            this.connection.Insert("postimage", new SortedList<string, IDBDataType>()
            {
                {"post_id", new DBNumber(this.post_id)},
                {"path", new DBString(this.path)}
            });
            return true;
        }
        public bool Update(PostImage newdata)
        {
            this.connection.Update("postimage", new SortedList<string, IDBDataType>()
            {
                {"path", new DBString(newdata.path)},
            }, "id= " + this.id);
            this.path = path;
            return true;
        }
        public void SetConnection(IDatabaseUtility connection)
        {
            this.connection = connection;
        }
	}
}