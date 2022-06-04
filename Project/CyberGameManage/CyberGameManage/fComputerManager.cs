using CyberGameManage.DAO;
using CyberGameManage.DTO;
using CyberGameManage.Payment;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CyberGameManage
{
    public partial class fComputerManager : Form
    {

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // width of ellipse
            int nHeightEllipse // height of ellipse
        );
        private Account loginAccount;

        public Account LoginAccount
        {
            get { return loginAccount; }
            set { loginAccount = value; ChangeAccount(loginAccount.Type); } // tính đóng gói OOP 
        }
        public fComputerManager(Account acc)
        {
            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

            this.LoginAccount = acc;

            LoadComputer();
            LoadCategory();
            LoadComboboxComputer(cbSwitchComputer);
        }

        #region Method

        void ChangeAccount(int type)
        {
            adminToolStripMenuItem.Enabled = type == 1;
            accountInformationToolStripMenuItem.Text += " (" + LoginAccount.DisplayName + ")";
        }

        void LoadCategory()
        {
            List<Category> listCategory = CategoryDAO.Instance.GetListCategory();
            cbCategory.DataSource = listCategory;
            cbCategory.DisplayMember = "Name";
        }
        void LoadOrderListByCategoryID(int id)
        {
            List<Orderr> listOrderr = OrderDAO.Instance.GetOrderrByCategoryID(id);
            cbOrderr.DataSource = listOrderr;
            cbOrderr.DisplayMember = "Name";
        }
        
        void LoadComputer()
        {
            flpTable.Controls.Clear();
            List<Computer> computerList = ComputerDAO.Instance.LoadComputerList();
            foreach (Computer item in computerList)
            {
                Button btn = new Button() { Width = ComputerDAO.ComputerWidth, Height = ComputerDAO.ComputerHeight };
                btn.Image = Image.FromFile("Resources\\PCC123.png");
                //btn.BackgroundImageLayout = ImageLayout.Stretch;

                btn.Text = item.Name + Environment.NewLine + item.Status;
                btn.Font = new Font("Palatino Linotype", 9, FontStyle.Bold);
                btn.Click += btn_Click;
                btn.Tag = item;
                switch (item.Status)
                {
                    case "Trống":
                        btn.BackColor = Color.SkyBlue;
                        break;
                    default:
                        btn.BackColor = Color.OrangeRed;
                        break;
                }
                flpTable.Controls.Add(btn); 
            }

        }
        void ShowBill(int id)
        {
            lsvBill.Items.Clear();
            List<DTO.Menu> listBillInfo = DAO.MenuDAO.Instance.GetListMenuByComputer(id);
            float totalPrice = 0;
            foreach (DTO.Menu item in listBillInfo)
            {
                ListViewItem lsvItem = new ListViewItem(item.OrderName.ToString());
                lsvItem.SubItems.Add(item.Count.ToString());
                lsvItem.SubItems.Add(item.Price.ToString());
                lsvItem.SubItems.Add(item.TotalPrice.ToString());
                totalPrice += item.TotalPrice;
                lsvBill.Items.Add(lsvItem);
            }
            CultureInfo culture = new CultureInfo("vi-VN");
            txbTotalPrice.Text = totalPrice.ToString("c", culture);
        }
        void LoadComboboxComputer(ComboBox cb)
        {
            cb.DataSource = ComputerDAO.Instance.LoadComputerList();
            cb.DisplayMember = "Name";
        }
       
        #region Events
        void btn_Click(object sender, EventArgs e)
        {
            int computerID = ((sender as Button).Tag as Computer).ID;
            lsvBill.Tag = (sender as Button).Tag;
            ShowBill(computerID);
        }
        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void userInformationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            fAccountProfile f = new fAccountProfile(LoginAccount);
            f.UpdateAccount += f_UpdateAccount;
            f.ShowDialog();
        }

        void f_UpdateAccount(object sender, AccountEvent e)
        {
            accountInformationToolStripMenuItem.Text = "Account (" + e.Acc.DisplayName + ")";
        }

        private void adminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fAdmin f = new fAdmin();
            f.loginAccount = LoginAccount;
            f.InsertOrder += f_InsertOrder;
            f.DeleteOrder += f_DeleteOrder;
            f.UpdateOrder += f_UpdateOrder;
            f.ShowDialog();
        }

        void f_UpdateOrder(object sender, EventArgs e)
        {
            LoadOrderListByCategoryID((cbCategory.SelectedItem as Category).ID);
            if (lsvBill.Tag != null)
                ShowBill((lsvBill.Tag as Computer).ID);
        }

        void f_DeleteOrder(object sender, EventArgs e)
        {
            LoadOrderListByCategoryID((cbCategory.SelectedItem as Category).ID);
            if (lsvBill.Tag != null)
                ShowBill((lsvBill.Tag as Computer).ID);
            LoadComputer();
        }

        void f_InsertOrder(object sender, EventArgs e)
        {
            LoadOrderListByCategoryID((cbCategory.SelectedItem as Category).ID);
            if (lsvBill.Tag != null)
                ShowBill((lsvBill.Tag as Computer).ID);
        }
        #endregion

        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            int id = 0;
            ComboBox cb = sender as ComboBox;

            if (cb.SelectedItem == null)
                return;

            Category selected = cb.SelectedItem as Category;
            id = selected.ID;
            LoadOrderListByCategoryID(id);
        }

        private void btnAddOrder_Click(object sender, EventArgs e)
        {
            Computer computer = lsvBill.Tag as Computer;

            if (computer == null)
            {
                MessageBox.Show("You have not selected the Computer!", "Thông báo");
                return;
            }


            int idBill = BillDAO.Instance.GetUncheckBillIDByComputerID(computer.ID);
            int orderID = (cbOrderr.SelectedItem as Orderr).ID;
            int count = (int)nmOrderCount.Value;

            if (idBill == -1)
            {
                BillDAO.Instance.InsertBill(computer.ID);
                BillInfoDAO.Instance.InsertBillInfo(BillDAO.Instance.GetMaxIDBill(), computer.ID, count);
            }
            else
            {
                BillInfoDAO.Instance.InsertBillInfo(idBill, orderID, count);
            }

            ShowBill(computer.ID);
            LoadComputer();
        }

        private void btnCheckout_Click(object sender, EventArgs e)
        {
            Computer computer = lsvBill.Tag as Computer;
            if (computer == null)
            {
                MessageBox.Show("You have not selected the Computer!", "Thông báo");
                return;
            }
            int idBill = BillDAO.Instance.GetUncheckBillIDByComputerID(computer.ID);
            int discount = (int)nmDiscount.Value;
            double totalPrice = Convert.ToDouble(txbTotalPrice.Text.Split(',')[0].Replace(".", ""));
            double finalTotalPrice = totalPrice - (totalPrice / 100) * discount;
            if (idBill != -1)
            {
                if (MyMessageBox.ShowMessage(string.Format("Your bill for the {0}:\n\nFinal Price = Total Price - (Total Price / 100) x Discount \n\n\t= {1} - ({1} / 100) x {2} \n\n\t= {3} VND \n\n\t Cashier: Nguyen Thanh Thien \t\t| Mobile: 0977214077", computer.Name, totalPrice, discount, finalTotalPrice ), "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.None) == System.Windows.Forms.DialogResult.OK)
                {
                    BillDAO.Instance.CheckOut(idBill, discount, (float)finalTotalPrice);
                    ShowBill(computer.ID);
                    //MyMessageBox.ShowMessage("Payment by Cash", "Thank you", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //Facade.getInstance().buyProductByCashWithFreeShipping("19110150@student.hcmute.edu.vn");
                    //Facade.getInstance().buyProductByPaypalWithStandardShipping("19110150@student.hcmute.edu.vn", "0977214077");
                    LoadComputer();
                }
            }
        }
        private void btn_cash_Click(object sender, EventArgs e)
        {
            Computer computer = lsvBill.Tag as Computer;
            if (computer == null)
            {
                MessageBox.Show("You have not selected the Computer!", "Thông báo");
                return;
            }
            int idBill = BillDAO.Instance.GetUncheckBillIDByComputerID(computer.ID);
            int discount = (int)nmDiscount.Value;
            double totalPrice = Convert.ToDouble(txbTotalPrice.Text.Split(',')[0].Replace(".", ""));
            double finalTotalPrice = totalPrice - (totalPrice / 100) * discount;
            if (idBill != -1)
            {
                if (MessageBox.Show(string.Format("Are you sure to pay the bill for the {0}:\n\nFinal Price = Total Price - (Total Price / 100) x Discount \n\n\t= {1} - ({1} / 100) x {2} \n\n\t= {3} VND", computer.Name, totalPrice, discount, finalTotalPrice), "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.None) == System.Windows.Forms.DialogResult.OK)
                {
                    BillDAO.Instance.CheckOut(idBill, discount, (float)finalTotalPrice);
                    ShowBill(computer.ID);
                    Facade.getInstance().buyProductByCash("Nguyen Thanh Thien");
                    //Facade.getInstance().buyProductByPaypalWithStandardShipping("19110150@student.hcmute.edu.vn", "0977214077");
                    LoadComputer();
                }
            }
        }
        private void btn_Momo_Click(object sender, EventArgs e)
        {
            Computer computer = lsvBill.Tag as Computer;
            if (computer == null)
            {
                MessageBox.Show("You have not selected the Computer!", "Thông báo");
                return;
            }
            int idBill = BillDAO.Instance.GetUncheckBillIDByComputerID(computer.ID);
            int discount = (int)nmDiscount.Value;
            double totalPrice = Convert.ToDouble(txbTotalPrice.Text.Split(',')[0].Replace(".", ""));
            double finalTotalPrice = totalPrice - (totalPrice / 100) * discount;
            if (idBill != -1)
            {
                if (MessageBox.Show(string.Format("Are you sure to pay the bill for the {0}:\n\nFinal Price = Total Price - (Total Price / 100) x Discount \n\n\t= {1} - ({1} / 100) x {2} \n\n\t= {3} VND", computer.Name, totalPrice, discount, finalTotalPrice), "Thông báo", MessageBoxButtons.OKCancel, MessageBoxIcon.None) == System.Windows.Forms.DialogResult.OK)
                {
                    BillDAO.Instance.CheckOut(idBill, discount, (float)finalTotalPrice);
                    ShowBill(computer.ID);
                    Facade.getInstance().buyProductByMomo("19110150@student.hcmute.edu.vn", "0977214077");
                    LoadComputer();
                }
            }

        }

        private void btnSwitchComputer_Click(object sender, EventArgs e)
        {
            int id1 = (lsvBill.Tag as Computer).ID;

            int id2 = (cbSwitchComputer.SelectedItem as Computer).ID;
            if (MessageBox.Show(string.Format("Do you want to switch from machine {0} to machine {1}", (lsvBill.Tag as Computer).Name, (cbSwitchComputer.SelectedItem as Computer).Name), "Thông báo", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                ComputerDAO.Instance.SwitchComputer(id1, id2);

                LoadComputer();
            }
        }

        private void paymentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnCheckout_Click(this, new EventArgs());
        }

        private void orderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnAddOrder_Click(this, new EventArgs());
        }

        private void cbOrderr_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void fComputerManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Do you want to exit App?", "Notification!", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
                e.Cancel = true;
        }

        private void ptb_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();   
        }
        #endregion
    }
}
