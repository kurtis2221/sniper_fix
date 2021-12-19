using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

class Form1 : Form
{
    public const string TITLE = "A mesterlövész v2.33 - Elakadás javító";
    public const string DIR_SAVE = "save";

    const uint ADDR_MAP = 0x87;
    const uint ADDR_POS = 0x8B;
    const uint ADDR_NAM = 0xAC;

    const int CT_XY = 12;
    const int CT_WD = 64;
    const int CT_HT = 24;
    const int CT_MG = 8;

    const int BT_WD = 104;

    int fixid;
    const int MAX_MAPS = 2;

    string[] fix_map =
    {
        "worlds\\wiez_wn2", //Vadászat a vadászra
        "worlds\\wiez_wn3" //>LNIn3 vagy Mélyebbre
    };

    float[,] fix_pos =
    {
        {-809.5f,-69.5f,368.0f},
        {2306.5f,394.0f,-116.0f}
    };

    int offs;

    ComboBox cb_saves;
    Label lb_saves, lb_name, lb_name2;
    string[] files;

    Button bt_fix, bt_res, bt_abt;

    BinaryReader br;
    BinaryWriter bw;

    public Form1()
    {
        Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
        Text = TITLE;
        MaximizeBox = false;
        ClientSize = new Size(350, 110);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        StartPosition = FormStartPosition.CenterScreen;
        //Mentés választás
        lb_saves = new Label();
        lb_saves.Bounds = new Rectangle(CT_XY, CT_XY, CT_WD, CT_HT);
        lb_saves.Text = "Mentések";
        Controls.Add(lb_saves);
        cb_saves = new ComboBox();
        cb_saves.Bounds = new Rectangle(lb_saves.Right, lb_saves.Top, 256, CT_HT);
        cb_saves.SelectedIndexChanged += cb_saves_SelectedIndexChanged;
        cb_saves.DropDownStyle = ComboBoxStyle.DropDownList;
        Controls.Add(cb_saves);
        //Pálya neve
        lb_name = new Label();
        lb_name.Bounds = new Rectangle(CT_XY, lb_saves.Bottom + CT_MG, CT_WD, CT_HT);
        lb_name.Text = "Pálya neve:";
        Controls.Add(lb_name);
        lb_name2 = new Label();
        lb_name2.Bounds = new Rectangle(lb_name.Right, lb_name.Top, 256, CT_HT);
        Controls.Add(lb_name2);
        //Gombok
        bt_fix = new Button();
        bt_fix.Bounds = new Rectangle(CT_XY, lb_name.Bottom + CT_MG, 104, CT_HT);
        bt_fix.Text = "Javít";
        bt_fix.Click += bt_fix_Click;
        bt_fix.Enabled = false;
        Controls.Add(bt_fix);
        bt_res = new Button();
        bt_res.Bounds = new Rectangle(bt_fix.Right + CT_MG, bt_fix.Top, 104, CT_HT);
        bt_res.Text = "Helyreállít";
        bt_res.Click += bt_res_Click;
        bt_res.Enabled = false;
        Controls.Add(bt_res);
        bt_abt = new Button();
        bt_abt.Bounds = new Rectangle(bt_res.Right + CT_MG, bt_fix.Top, 104, CT_HT);
        bt_abt.Text = "Programról";
        bt_abt.Click += bt_abt_Click;
        Controls.Add(bt_abt);
        files = Directory.GetFiles(DIR_SAVE, "*.sav");
        foreach (string f in files)
            cb_saves.Items.Add(f + " - " + File.GetLastWriteTime(f).ToString());
    }

    void cb_saves_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            string map;
            int tmp;
            fixid = -1;
            br = new BinaryReader(new FileStream(files[cb_saves.SelectedIndex], FileMode.Open, FileAccess.Read), Encoding.Default);
            br.BaseStream.Position = ADDR_MAP;
            tmp = br.ReadInt32();
            offs = tmp;
            map = new string(br.ReadChars(tmp));
            for (int i = 0; i < fix_map.Length; i++)
            {
                if (string.Compare(map, fix_map[i]) == 0)
                {
                    fixid = i;
                    break;
                }
            }
            br.BaseStream.Position = ADDR_NAM + offs;
            tmp = br.ReadInt32();
            lb_name2.Text = new string(br.ReadChars(tmp));
            br.Close();
            bt_fix.Enabled = fixid != -1;
            bt_res.Enabled = File.Exists(files[cb_saves.SelectedIndex] + ".bak");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Source + " - " + ex.Message,
                TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    void bt_fix_Click(object sender, EventArgs e)
    {
        try
        {
            int tmp = cb_saves.SelectedIndex;
            File.Copy(files[tmp], files[tmp] + ".bak", true);
            bw = new BinaryWriter(new FileStream(files[tmp], FileMode.Open, FileAccess.Write));
            bw.BaseStream.Position = ADDR_POS + offs;
            for (int i = 0; i < 3; i++)
                bw.Write(fix_pos[fixid, i]);
            bw.Close();
            bt_res.Enabled = true;
            MessageBox.Show("Mentés javítva.", TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Source + " - " + ex.Message,
                TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    void bt_res_Click(object sender, EventArgs e)
    {
        try
        {
            int tmp = cb_saves.SelectedIndex;
            if (File.Exists(files[tmp]))
                File.Delete(files[tmp]);
            File.Move(files[tmp] + ".bak", files[tmp]);
            bt_res.Enabled = false;
            MessageBox.Show("Helyreállítva.", TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Source + " - " + ex.Message,
                TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    void bt_abt_Click(object sender, EventArgs e)
    {
        MessageBox.Show("A programot készítette: Kurtis (2016)\nVisual C# 2008 Express-ben íródott.",
                TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

class Program
{
    [STAThread]
    static void Main()
    {
        if (!Directory.Exists(Form1.DIR_SAVE))
        {
            MessageBox.Show("A " + Form1.DIR_SAVE + " mappa nem található! Ezt a programot a Mesterlövész mappájából indítsd!",
                Form1.TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new Form1());
    }
}