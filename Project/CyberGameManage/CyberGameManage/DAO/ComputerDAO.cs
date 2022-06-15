using CyberGameManage.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyberGameManage.DAO
{
    public class ComputerDAO
    {
        private static ComputerDAO instance;
        public static ComputerDAO Instance 
        { 
            get { if (instance == null) instance = new ComputerDAO();  return ComputerDAO.instance; }
            private set { ComputerDAO.instance = value; }
        }
        public static int ComputerWidth = 132;
        public static int ComputerHeight = 125;
        private ComputerDAO() { }

        public void SwitchComputer(int id1, int id2)
        {
            DataProvider.Instance.ExecuteQuery("USP_SwitchComputer @idComputer1 , @idComputer2", new object[] { id1, id2 });
        }
        public List<Computer> LoadComputerList()
        {
            List<Computer> computerList = new List<Computer>();
            DataTable data = DataProvider.Instance.ExecuteQuery("USP_GetComputerList");
            foreach (DataRow item in data.Rows)
            {
                Computer computer = new Computer(item);
                computerList.Add(computer);
            }
            return computerList;
        }
        public bool InsertComputer(string name, string status)
        {
            string query = string.Format("INSERT dbo.Computer (name, status)VALUES  ( N'{0}', N'{1}')", name, status);
            int result = DataProvider.Instance.ExecuteNonQuery(query);

            return result > 0;
        }
        public bool UpdateComputer(string name, string status)
        {
            string query = string.Format("UPDATE dbo.Computer SET status = N'{1}' WHERE name = N'{0}'", name, status);
            int result = DataProvider.Instance.ExecuteNonQuery(query);

            return result > 0;
        }

        public bool DeleteComputer(string name)
        {
            string query = string.Format("Delete Account where name = N'{0}'", name);
            int result = DataProvider.Instance.ExecuteNonQuery(query);

            return result > 0;
        }
    }
}
