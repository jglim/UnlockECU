using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnlockECU;

namespace VisualUnlockECU
{
    public partial class MainForm : Form
    {
        List<Definition> Definitions;
        public MainForm()
        {
            string definitionJson = File.ReadAllText("db.json");
            Definitions = System.Text.Json.JsonSerializer.Deserialize<List<Definition>>(definitionJson);

            InitializeComponent();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            EnableDoubleBuffer(dgvMain, true);
            UpdateGrid();
        }
        public static void EnableDoubleBuffer(DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
        }

        private void UpdateGrid(string filter = "")
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("Index");
            dt.Columns.Add("Name");
            dt.Columns.Add("Origin");
            dt.Columns.Add("Level");
            dt.Columns.Add("Seed Size");
            dt.Columns.Add("Key Size");
            dt.Columns.Add("Security Provider");

            List<SecurityProvider> providers = SecurityProvider.GetSecurityProviders();
            for (int i = 0; i < Definitions.Count; i++) 
            {
                Definition definition = Definitions[i];

                if (providers.Find(x => x.GetProviderName() == definition.Provider) is null)
                {
                    continue;
                }
                if (!definition.Origin.ToLower().Contains(filter.ToLower()))
                {
                    continue;
                }

                dt.Rows.Add(new string[] {
                    i.ToString(),
                    definition.EcuName,
                    definition.Origin,
                    definition.AccessLevel.ToString(),
                    definition.SeedLength.ToString(),
                    definition.KeyLength.ToString(),
                    definition.Provider
                });
            }

            dgvMain.DataSource = dt;
            dgvMain.Columns[0].Visible = false;
            dgvMain.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvMain.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvMain.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dgvMain.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dgvMain.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dgvMain.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            UpdateGrid(txtFilter.Text);
        }

        private void txtSeedValue_TextChanged(object sender, EventArgs e)
        {
            TryRefreshKey();
        }
        private void dgvMain_SelectionChanged(object sender, EventArgs e)
        {
            TryRefreshKey();
        }

        public void TryRefreshKey()
        {
            bool validHex = true;
            string cleanedText = txtSeedValue.Text.Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("-", "").ToUpper();
            if (cleanedText.Length % 2 != 0)
            {
                validHex = false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(cleanedText, @"\A\b[0-9a-fA-F]+\b\Z"))
            {
                validHex = false;
            }

            if (validHex)
            {
                byte[] seed = BitUtility.BytesFromHex(cleanedText);
                txtSeedValue.BackColor = System.Drawing.SystemColors.Window;
                TryGenerateKey(seed);
            }
            else
            {
                if (cleanedText.Length == 0)
                {
                    TryGenerateKey(new byte[] { });
                }
                txtSeedValue.BackColor = System.Drawing.Color.LavenderBlush;
            }
        }

        public void TryGenerateKey(byte[] inByte) 
        {
            if (dgvMain.SelectedRows.Count != 1) 
            {
                txtKeyValue.Text = "Please select a definition first";
                return;
            }
            int selectedIndex = int.Parse(dgvMain.SelectedRows[0].Cells[0].Value.ToString());
            Definition definition = Definitions[selectedIndex];

            groupBox2.Text = $"Key Generation ({definition})";

            if (definition.SeedLength != inByte.Length)
            {
                txtKeyValue.Text = $"Expecting a {definition.SeedLength}-byte seed. Current length is {inByte.Length}";
                return;
            }

            SecurityProvider provider = SecurityProvider.GetSecurityProviders().Find(x => x.GetProviderName() == definition.Provider);

            if (provider is null)
            {
                txtKeyValue.Text = $"Could not load security provider for {definition.Provider}";
                return;
            }
            byte[] outKey = new byte[definition.KeyLength];

            if (provider.GenerateKey(inByte, outKey, definition.AccessLevel, definition.Parameters))
            {
                txtKeyValue.Text = BitUtility.BytesToHex(outKey, true);
            }
            else
            {
                txtKeyValue.Text = $"Key generation was unsuccessful ({definition.Provider})";
                return;
            }
        }

    }
}
