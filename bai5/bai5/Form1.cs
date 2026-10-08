using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace bai5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // wire DataGridView events
            dataGridView1.CellEndEdit += DataGridView1_CellEndEdit;
            dataGridView1.CellValidating += DataGridView1_CellValidating;
            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;
            dataGridView1.RowsRemoved += DataGridView1_RowsRemoved;
            dataGridView1.UserDeletedRow += DataGridView1_UserDeletedRow;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
            UpdateTotals();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void DataGridView1_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            var dgv = dataGridView1;
            if (e.RowIndex < 0) return;

            var colName = dgv.Columns[e.ColumnIndex].Name;
            if (colName == "colQty" || colName == "colWeight")
            {
                string newVal = e.FormattedValue?.ToString() ?? string.Empty;
                if (!double.TryParse(newVal, NumberStyles.Number, CultureInfo.InvariantCulture, out double val) || val <= 0)
                {
                    // Use ErrorProvider on editing control if available
                    if (dgv.EditingControl is Control ctrl)
                    {
                        errorProvider1.SetError(ctrl, "Value must be a number > 0");
                    }
                }
                else
                {
                    if (dgv.EditingControl is Control ctrl)
                    {
                        errorProvider1.SetError(ctrl, string.Empty);
                    }
                }
            }
        }

        private void DataGridView1_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            // clear any error on editing control
            if (dataGridView1.EditingControl is Control ctrl)
            {
                errorProvider1.SetError(ctrl, string.Empty);
            }

            RecalculateRow(e.RowIndex);
            UpdateTotals();
        }

        private void DataGridView1_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            // when values change (e.g., programmatically), recalc
            RecalculateRow(e.RowIndex);
            UpdateTotals();
        }

        private void DataGridView1_RowsRemoved(object? sender, DataGridViewRowsRemovedEventArgs e)
        {
            UpdateTotals();
        }

        private void DataGridView1_UserDeletedRow(object? sender, DataGridViewRowEventArgs e)
        {
            UpdateTotals();
        }

        private void RecalculateRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dataGridView1.Rows.Count) return;
            var row = dataGridView1.Rows[rowIndex];
            if (row.IsNewRow) return;

            double qty = ParseDouble(row.Cells["colQty"].Value);
            double weight = ParseDouble(row.Cells["colWeight"].Value);
            decimal unitPrice = ParseDecimal(row.Cells["colUnitPrice"].Value);

            // Amount = qty * unitPrice
            decimal amount = (decimal)qty * unitPrice;
            row.Cells["colAmount"].Value = amount.ToString("F2", CultureInfo.InvariantCulture);

            // For weight display, keep as number (optional)
            if (row.Cells["colWeight"].Value != null)
            {
                row.Cells["colWeight"].Value = weight.ToString("F2", CultureInfo.InvariantCulture);
            }
        }

        private double ParseDouble(object? o)
        {
            if (o == null) return 0;
            if (double.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) return v;
            return 0;
        }

        private decimal ParseDecimal(object? o)
        {
            if (o == null) return 0m;
            if (decimal.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v)) return v;
            return 0m;
        }

        private void UpdateTotals()
        {
            double totalQty = 0;
            double totalWeight = 0;
            decimal totalAmount = 0m;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;
                double qty = ParseDouble(row.Cells["colQty"].Value);
                double weight = ParseDouble(row.Cells["colWeight"].Value);
                decimal amount = ParseDecimal(row.Cells["colAmount"].Value);

                totalQty += qty;
                totalWeight += qty * weight; // total weight = qty * per-item weight
                totalAmount += amount;
            }

            toolTotalQty.Text = $"Total Qty: {totalQty}";
            toolTotalWeight.Text = $"Total Wt: {totalWeight:F2} kg";
            toolTotalAmount.Text = $"Total Amt: {totalAmount:F2}";
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                // add new row and begin edit on Item Name
                int idx = dataGridView1.Rows.Add();
                dataGridView1.CurrentCell = dataGridView1.Rows[idx].Cells["colItem"];
                dataGridView1.BeginEdit(true);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                if (dataGridView1.CurrentRow != null && !dataGridView1.CurrentRow.IsNewRow)
                {
                    dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                    UpdateTotals();
                }
                e.Handled = true;
            }
        }
    }
}
