using CyberGameManage.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberGameManage.DAO
{
    public class OrderDAO
    {
        private static OrderDAO instance;

        public static OrderDAO Instance
        {
            get { if (instance == null) instance = new OrderDAO(); return OrderDAO.instance; }
            private set { OrderDAO.instance = value; }
        }

        private OrderDAO() { }

        public List<Orderr> GetOrderrByCategoryID(int id)
        {
            List<Orderr> list = new List<Orderr>();

            string query = "select * from Orderr where idCategory = " + id;

            DataTable data = DataProvider.Instance.ExecuteQuery(query);

            foreach (DataRow item in data.Rows)
            {
                Orderr order = new Orderr(item);
                list.Add(order);
            }

            return list;
        }
        public List<Orderr> GetListOrder()
        {
            List<Orderr> list = new List<Orderr>();

            string query = "select * from Orderr";

            DataTable data = DataProvider.Instance.ExecuteQuery(query);

            foreach (DataRow item in data.Rows)
            {
                Orderr orderr = new Orderr(item);
                list.Add(orderr);
            }

            return list;
        }


        public bool InsertOrder(string name, int id, float price)
        {
            string query = string.Format("INSERT dbo.Orderr (name, idCategory, price) VALUES  ( N'{0}', {1}, {2})", name, id, price);
            int result = DataProvider.Instance.ExecuteNonQuery(query);

            return result > 0;
        }
        public bool UpdateOrder(int idOrder, string name, int id, float price)
        {
            string query = string.Format("UPDATE dbo.Orderr SET name = N'{0}', idCategory = {1}, price = {2} WHERE id = {3}", name, id, price, idOrder);
            int result = DataProvider.Instance.ExecuteNonQuery(query);

            return result > 0;
        }

        public bool DeleteOrder(int idOrder)
        {
            BillInfoDAO.Instance.DeleteBillInfoByOrderID(idOrder);

            string query = string.Format("Delete Orderr where id = {0}", idOrder);
            int result = DataProvider.Instance.ExecuteNonQuery(query);

            return result > 0;
        }
        public List<Orderr> SearchOrderByName(string name)
        {
            List<Orderr> list = new List<Orderr>();

            string query = string.Format("SELECT * FROM dbo.Orderr WHERE dbo.fuConvertToUnsign1(name) LIKE N'%' + dbo.fuConvertToUnsign1(N'{0}') + '%'", name);

            DataTable data = DataProvider.Instance.ExecuteQuery(query);

            foreach (DataRow item in data.Rows)
            {
                Orderr orderr = new Orderr(item);
                list.Add(orderr);
            }

            return list;
        }
    }
}
