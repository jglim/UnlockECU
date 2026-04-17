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
            string dbFile = "db.json";
            if (File.Exists(dbFile))
            {
                string definitionJson = File.ReadAllText(dbFile);
                Definitions = System.Text.Json.JsonSerializer.Deserialize<List<Definition>>(definitionJson);
            }
            else 
            {
                Definitions = new List<Definition>();
                MessageBox.Show("No definitions loaded. Check if the file 'db.json' is accessible in the same folder as VisualUnlockECU.");
            }

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
            if (dgvMain.SelectedRows.Count != 1)
                return;

            int selectedIndex = int.Parse(dgvMain.SelectedRows[0].Cells[0].Value.ToString());
            Definition definition = Definitions[selectedIndex];

            string input = txtSeedValue.Text
                .Replace(" ", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("\t", "")
                .Replace("-", "")
                .ToUpper();

            if (string.IsNullOrEmpty(input))
            {
                TryGenerateKey(Array.Empty<byte>());
                return;
            }

            byte[] seed;

            if (definition.InputType == "ASCII")
            {
                // 🔥 ASCII mode
                seed = Encoding.ASCII.GetBytes(input);
                txtSeedValue.BackColor = System.Drawing.SystemColors.Window;
            }
            else
            {
                // 🔒 HEX mode
                bool isHex = input.Length % 2 == 0 &&
                             System.Text.RegularExpressions.Regex.IsMatch(input, @"\A[0-9A-F]+\Z");

                if (!isHex)
                {
                    txtSeedValue.BackColor = System.Drawing.Color.LavenderBlush;
                    return;
                }

                seed = BitUtility.BytesFromHex(input);
                txtSeedValue.BackColor = System.Drawing.SystemColors.Window;
            }

            TryGenerateKey(seed);
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

            groupBox2.Text = $"Key Generation ({definition}) [{definition.InputType}]";

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

        private void btnPasteSeed_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                txtSeedValue.Text = Clipboard.GetText();
            }
        }

        private void btnCopyKey_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(txtKeyValue.Text);
        }
    }
}
