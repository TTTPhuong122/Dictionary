using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace Demo
{
    public partial class Team16_Dictionary : Form
    {
        private IKeyboardMouseEvents m_Events;
        public static string source_lang = "";
        public static string target_lang = "";
        private int boderRadius = 20;
        private int borderSize = 2;
        private Color borderColor = Color.FromArgb(3, 32, 145);
        public Team16_Dictionary()
        {

            InitializeComponent();
            textBoxLog.Visible = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(borderSize);
            this.BackColor = borderColor;
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }
        //Drag Form
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void panelTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://dictionarypbl.blogspot.com/2022/12/dictionary-translate-simply.html");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SubscribeGlobal();
            this.Hide();
        }

        private void Team16_Dictionary_Closing(object sender, EventArgs e)
        {
            Unsubscribe();
        }
        private void SubscribeGlobal()
        {
            Unsubscribe();
            Subscribe(Hook.GlobalEvents());
        }

        private void Subscribe(IKeyboardMouseEvents events)
        {
            m_Events = events;

            m_Events.MouseDoubleClick += OnMouseDoubleClick;

            m_Events.MouseMove += HookManager_MouseMove;

            m_Events.MouseDragFinished += OnMouseDragFinished;

        }

        private void Unsubscribe()
        {
            if (m_Events == null) return;
            m_Events.MouseDoubleClick -= OnMouseDoubleClick;

            m_Events.MouseMove -= HookManager_MouseMove;
            m_Events.MouseDragFinished -= OnMouseDragFinished;


            m_Events.Dispose();
            m_Events = null;
        }

        public async void Translate(int x, int y)
        {
            int sl = comboBox1.SelectedIndex;
            int tl = comboBox2.SelectedIndex;
            switch(sl){
                case 1:
                    source_lang = "en";
                    break;
                case 3:
                    source_lang = "vi";
                    break;
                case 2:
                    source_lang = "ja";
                    break ;
                case 0:
                    source_lang = "zh-TW";
                    break;
                default:
                    source_lang="";
                    break;
            }
            switch (tl)
            {
                case 1:
                    target_lang = "en";
                    break;
                case 3:
                    target_lang = "vi";
                    break;
                case 2:
                    target_lang = "ja";
                    break;
                case 0:
                    target_lang = "zh-TW";
                    break;
                default:
                    target_lang = "";
                    break;
            }
            string rs = null;
            //textBoxLog.Clear();
            string input = "";
            try
            {
                SendKeys.SendWait("^(c)");
                async Task Puttaskdelay()
                {
                    await Task.Delay(50);
                }

                await Puttaskdelay();
                input = Clipboard.GetText();
                string url = String.Format("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}", source_lang, target_lang, Uri.EscapeUriString(input));
                HttpClient httpClient = new HttpClient();
                string result = httpClient.GetStringAsync(url).Result;
                var jsonData = new JavaScriptSerializer().Deserialize<List<dynamic>>(result);
                var translationItems = jsonData[0];
                string translation = "";
                if (translationItems != null)
                {
                    foreach (object item in translationItems)
                    {
                        IEnumerable translationLineObject = item as IEnumerable;
                        IEnumerator translationLineString = translationLineObject.GetEnumerator();
                        translationLineString.MoveNext();
                        translation += string.Format(" {0}", Convert.ToString(translationLineString.Current));
                    }
                    if (translation.Length > 1) { translation = translation.Substring(1); };
                    Log(input);
                    cm_trans.Items.Clear();
                    cm_trans.Items.Add(translation);
                    cm_trans.Show(this, x, y);
                }
            }
            catch (Exception ex)
            {
                Log(input);
                cm_trans.Items.Clear();
                cm_trans.Items.Add(rs);
                cm_trans.Show(this, x, y);
            }
        }

        private void HookManager_Supress(object sender, MouseEventExtArgs e)
        {
            if (e.Button != MouseButtons.Right)
            {
                Log(string.Format("MouseDown \t\t {0}\n", e.Button));
                return;
            }

            Log(string.Format("MouseDown \t\t {0} Suppressed\n", e.Button));
            e.Handled = true;
        }


        private void HookManager_MouseMove(object sender, MouseEventArgs e)
        {
            //labelMousePosition.Text = string.Format("x={0:0000}; y={1:0000}", e.X, e.Y);
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            //Copy();
            int x_position = e.X - 30 - this.Location.X;
            int y_position = e.Y - 80 - this.Location.Y;
            Translate(x_position, y_position);
            /*Log(string.Format("MouseDoubleClick \t\t {0}\n", e.Button));*/
        }


        private void OnMouseDragFinished(object sender, MouseEventArgs e)
        {
            //Copy();
            int x_position = e.X - 30 - this.Location.X;
            int y_position = e.Y - 80 - this.Location.Y;
            Translate(x_position, y_position);
            /*Log("MouseDragFinished\n");*/
        }


        private void Log(string text)
        {
            if (IsDisposed) return;
            textBoxLog.AppendText(text);
            textBoxLog.ScrollToCaret();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //textBoxLog.Text = comboBox1.SelectedIndex;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void Dictionary_Click(object sender, EventArgs e)
        {

        }
    }
}
