using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Speech.Recognition;

namespace test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Point Mousepoint;
        bool MouseLeft = false;
        public int Imageindex = 1;    //1为眨眼，2为无语
        int zhen = imagegif1.GetFrameCount(frameDimension);   //帧数控制
        public List<string> Gamepath = new List<string>();   //游戏路径
        public List<string> Musicpath = new List<string>();  //音乐路径
        public Dictionary<string, string> RmCommond = new Dictionary<string, string>();   //口令
        public SpeechRecognitionEngine Sre = new SpeechRecognitionEngine();
        public GrammarBuilder Gb = new GrammarBuilder();
        public bool SreCom = false;

        /*绘制动画*/
        #region
        Bitmap imagegif = imagegif1;
        static Bitmap imagegif1 = new Bitmap("Image/灵梦眨眼.gif");
        Bitmap imagegif2 = new Bitmap("Image/灵梦无语.gif");
        Bitmap imagegif3 = new Bitmap("Image/灵梦睡觉1.gif");
        Bitmap imagegif4 = new Bitmap("Image/灵梦睡觉2.gif");
        Bitmap imagegif5 = new Bitmap("Image/灵梦音乐.gif");
        static FrameDimension frameDimension = new FrameDimension(imagegif1.FrameDimensionsList[0]);

        public void PlayAnimate()
        {
            if (ImageAnimator.CanAnimate(imagegif))
                ImageAnimator.Animate(imagegif, new EventHandler(this.OnFrameChanged));
        }

        public void OnFrameChanged(object o, EventArgs e)
        {
            if (Imageindex == 1)
            {
                if (zhen <= 0)
                    ImageAnimator.StopAnimate(imagegif, new EventHandler(this.OnFrameChanged));
                else
                    zhen--;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ImageAnimator.UpdateFrames();

            Invoke(new Action(() =>
            {
                e.Graphics.DrawImage(imagegif, new Point(0, 0));
            }));
        }

        #endregion

        /*鼠标拖动窗体*/
        #region
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Mousepoint = new Point(-e.X, -e.Y);
                MouseLeft = true;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseLeft)
            {
                Point MouseSet = MousePosition;
                MouseSet.Offset(Mousepoint.X, Mousepoint.Y);

                if (Left <= -30 && MouseSet.X < Location.X)
                    Location = new Point(Location.X, MouseSet.Y);
                else if (Location.X + 120 >= Screen.PrimaryScreen.Bounds.Right && MouseSet.X > Location.X)
                    Location = new Point(Location.X, MouseSet.Y);
                else if (Top <= 0 && MouseSet.Y < Location.Y)
                    Location = new Point(MouseSet.X, Location.Y);
                else if (Location.Y + 175 >= Screen.PrimaryScreen.Bounds.Height && MouseSet.Y > Location.Y)
                    Location = new Point(MouseSet.X, Location.Y);
                else
                    Location = MouseSet;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (MouseLeft)
                MouseLeft = false;
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            string s;

            /*将GamesPath文件中所有启动路径信息读出到泛型中*/
            FileStream fs = new FileStream("Data/GamesPath", FileMode.OpenOrCreate);
            StreamReader sr = new StreamReader(fs);
            while ((s = sr.ReadLine()) != null)
                Gamepath.Add(s);
            sr.Close();
            fs.Close();

            /*将MusicPath文件中的音乐路径信息读出到泛型中*/
            FileStream MusicPath = new FileStream("Data/MusicPath", FileMode.OpenOrCreate);
            StreamReader MusicSR = new StreamReader(MusicPath);
            while ((s = MusicSR.ReadLine()) != null)
                Musicpath.Add(s);
            MusicSR.Close();
            MusicPath.Close();

            /*将口令.txt文件中的口令信息读出到泛型中*/
            try
            {
                FileStream RmCommondPath = new FileStream("Data/口令.txt", FileMode.OpenOrCreate);
                StreamReader RmCommondSR = new StreamReader(RmCommondPath,System.Text.Encoding.Default);
                while ((s = RmCommondSR.ReadLine()) != null)
                {
                    string[] Rms = s.Split('：');
                    RmCommond.Add(Rms[0],Rms[1]);
                }
                RmCommondSR.Close();
                RmCommondPath.Close();
            }
            catch
            {
                MessageBox.Show("口令.txt文件内容出错");
            }

            /*将GamesPath文件中的同人游戏的文件名读出并显示到界面上*/
            foreach (string str in Gamepath)
            {
                string[] st = str.Split('|');
                if (st[0] == "同人游戏")
                {
                    ToolStripMenuItem thother = new ToolStripMenuItem();
                    ToolStripMenuItem other = new ToolStripMenuItem();
                    thother.Name = st[1];
                    thother.Text = st[1];
                    other.Name = st[1];
                    other.Text = st[1];
                    启动游戏.DropDownItems.Add(thother);
                    删除同人游戏.DropDownItems.Add(other);
                }
                if (st[0] == "常用软件")
                {
                    ToolStripMenuItem thother = new ToolStripMenuItem();
                    ToolStripMenuItem other = new ToolStripMenuItem();
                    thother.Name = st[1];
                    thother.Text = st[1];
                    other.Name = st[1];
                    other.Text = st[1];
                    启动软件.DropDownItems.Add(thother);
                    删除常用软件.DropDownItems.Add(other);
                }
            }

            /*将WebCollection文件中的网站链接信息读出并显示到界面上*/
            FileStream Webfs = new FileStream("Data/WebCollection", FileMode.OpenOrCreate);
            StreamReader Websr = new StreamReader(Webfs);
            while((s = Websr.ReadLine())!=null)
            {
                string[] str = s.Split('|');
                ToolStripMenuItem Webts = new ToolStripMenuItem();
                Webts.Text = str[0];
                Webts.Tag = str[1];
                网站链接ToolStripMenuItem.DropDownItems.Add(Webts);
            }
            Websr.Close();
            Webfs.Close();

            /*语音识别准备*/
            Sre.SetInputToDefaultAudioDevice();
            Gb.Append(new Choices(new string[] {
                RmCommond["唤醒"],
                RmCommond["退出"],
                RmCommond["打开贴吧"],
                RmCommond["播放音乐"],
                RmCommond["停止播放"] }));
            Grammar G = new Grammar(Gb);
            Sre.SpeechRecognized += Sre_SpeechRecognized;
            Sre.LoadGrammar(G);
        }

        private void 最小化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        /*网站链接*/
        #region

        private void 单击此处输入名称_Click(object sender, EventArgs e)
        {
            单击此处输入名称.Text = null;
        }

        private void 单击此处输入网址_Click(object sender, EventArgs e)
        {
            单击此处输入网址.Text = null;
        }

        private void 确定ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (单击此处输入名称.Text == "（单击此处输入名称）" || 单击此处输入网址.Text == "（单击此处输入网址）")
                MessageBox.Show("请输入名称与网址");
            else
            {
                FileStream webfs = new FileStream("Data/WebCollection", FileMode.Append);
                StreamWriter websw = new StreamWriter(webfs);
                websw.WriteLine(单击此处输入名称.Text + "|" + 单击此处输入网址.Text);
                websw.Close();
                webfs.Close();
                ToolStripMenuItem WebC = new ToolStripMenuItem();
                WebC.Tag = 单击此处输入网址.Text;
                WebC.Text = 单击此处输入名称.Text;
                网站链接ToolStripMenuItem.DropDownItems.Add(WebC);
                单击此处输入名称.Text = "（单击此处输入名称）";
                单击此处输入网址.Text = "（单击此处输入网址）";
            }
        }

        private void 取消ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            单击此处输入名称.Text = "（单击此处输入名称）";
            单击此处输入网址.Text = "（单击此处输入网址）";
        }

        private void 网站链接ToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                if (e.ClickedItem.Tag != null)
                    Process.Start(e.ClickedItem.Tag.ToString());
            }
            catch
            {
                MessageBox.Show("网址设置有误！");
            }
        }
        #endregion

        /*由双击图标呼出变为单击*/
        private void Test_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Visible = true;
                WindowState = FormWindowState.Normal;
            }
            Focus();
        }

        /*点击灵梦播放眨眼的动画*/
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            ZhaYan();
        }
        public void ZhaYan()
        {
            timer1.Stop();
            timer1.Start();
            if (imagegif == imagegif5)      //灵梦专注唱歌的时候不会理你
                return;
            if (Imageindex == 2)
            {
                ImageAnimator.StopAnimate(imagegif, new EventHandler(this.OnFrameChanged));
                imagegif = imagegif1;
                Imageindex = 1;
            }
            imagegif.SelectActiveFrame(frameDimension, 0);
            zhen = imagegif.GetFrameCount(frameDimension);
            PlayAnimate();
        }

        /*放置play的计时器*/
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (SreCom)
                SreCom = false;
            if (imagegif == imagegif5)
                return;
            if (imagegif == imagegif1)
            {
                ImageAnimator.StopAnimate(imagegif, new EventHandler(this.OnFrameChanged));
                imagegif = imagegif2;
                Imageindex = 2;
                PlayAnimate();
            }
            if (imagegif == imagegif1 || imagegif == imagegif2)
            {
                if (DateTime.Now.Hour >= 22)
                {
                    ImageAnimator.StopAnimate(imagegif, new EventHandler(this.OnFrameChanged));
                    imagegif = imagegif3;       //10点过后为困
                    Imageindex = 2;
                    PlayAnimate();
                }
            }
            if (DateTime.Now.Hour <= 6 && imagegif != imagegif4)
            {
                ImageAnimator.StopAnimate(imagegif, new EventHandler(this.OnFrameChanged));
                imagegif = imagegif4;          //0点过后为蹲着睡
                Imageindex = 2;
                PlayAnimate();
            }
        }

        /*开始游戏*/
        void GameStart(string gameclass, string gamename)
        {
            foreach (string s in Gamepath)
            {
                string[] gp = s.Split('|');
                if (gp[1] == gamename)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = gp[2];
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.WorkingDirectory = Path.GetDirectoryName(gp[2]);
                    p.Start();
                    return;
                }
            }
            MessageBox.Show("请先设置游戏路径");
            GameStartPath(gameclass, gamename);
        }

        /*设置路径*/
        void GameStartPath(string Gclass, string Gname)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream("Data/GamesPath", FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                if (Gname == null)
                    Gname = openFileDialog1.SafeFileName;
                string st = null;
                foreach (string s in Gamepath)
                {
                    string[] str = s.Split('|');
                    if (str[0] == Gclass && str[1] == Gname)
                    {
                        st = s;
                        sw.WriteLine(Gclass + "|" + Gname + "|" + openFileDialog1.FileName);
                    }
                    else
                        sw.WriteLine(s);
                }
                if (st != null)
                {
                    Gamepath.Remove(st);
                    Gamepath.Add(Gclass + "|" + Gname + "|" + openFileDialog1.FileName);
                }
                else
                {
                    Gamepath.Add(Gclass + "|" + Gname + "|" + openFileDialog1.FileName);
                    sw.WriteLine(Gclass + "|" + Gname + "|" + openFileDialog1.FileName);
                    ToolStripMenuItem thother = new ToolStripMenuItem();
                    ToolStripMenuItem other = new ToolStripMenuItem();
                    thother.Name = Gname;
                    thother.Text = Gname;
                    other.Name = Gname;
                    other.Text = Gname;
                    if (Gclass == "同人游戏")
                    {
                        启动游戏.DropDownItems.Add(thother);
                        删除同人游戏.DropDownItems.Add(other);
                    }
                    else if (Gclass == "常用软件")
                    {
                        启动软件.DropDownItems.Add(thother);
                        删除常用软件.DropDownItems.Add(other);
                    }
                }
                sw.Close();
                fs.Close();
            }
        }

        /*快捷方式*/
        #region
        /*同人游戏*/
        private void 启动游戏_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            GameStart("同人游戏", e.ClickedItem.Name);
        }

        private void 添加_Click(object sender, EventArgs e)
        {
            GameStartPath("同人游戏", null);
        }

        private void 删除同人游戏_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            FileStream fs = new FileStream("Data/GamesPath", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            string s = null;
            foreach (string str in Gamepath)
            {
                string[] st = str.Split('|');
                if (st[0] == "同人游戏" && st[1] == e.ClickedItem.Text)
                    s = str;
                else
                    sw.WriteLine(str);
            }
            Gamepath.Remove(s);
            string[] t = s.Split('|');
            for (int i = 0; i < 启动游戏.DropDownItems.Count; i++)
            {
                if (启动游戏.DropDownItems[i].Text == t[1])
                    启动游戏.DropDownItems.RemoveAt(i);
                if (删除同人游戏.DropDownItems[i].Text == t[1])
                    删除同人游戏.DropDownItems.RemoveAt(i);
            }
            sw.Close();
            fs.Close();
        }

        /*常用软件*/
        private void 启动软件ToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            GameStart("常用软件", e.ClickedItem.Name);
        }

        private void 添加ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GameStartPath("常用软件", null);
        }

        private void 删除常用软件_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            FileStream fs = new FileStream("Data/GamesPath", FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            string s = null;
            foreach (string str in Gamepath)
            {
                string[] st = str.Split('|');
                if (st[0] == "常用软件" && st[1] == e.ClickedItem.Text)
                    s = str;
                else
                    sw.WriteLine(str);
            }
            Gamepath.Remove(s);
            string[] t = s.Split('|');
            for (int i = 0; i < 启动软件.DropDownItems.Count; i++)
            {
                if (启动软件.DropDownItems[i].Text == t[1])
                    启动软件.DropDownItems.RemoveAt(i);
                if (删除常用软件.DropDownItems[i].Text == t[1])
                    删除常用软件.DropDownItems.RemoveAt(i);
            }
            sw.Close();
            fs.Close();
        }

        /*官方游戏*/
        private void STG_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (启动游戏模式.Checked)
                GameStart("官方游戏", e.ClickedItem.Name);
            else
                GameStartPath("官方游戏", e.ClickedItem.Name);
        }

        private void FTG_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (启动游戏模式.Checked)
                GameStart("官方游戏", e.ClickedItem.Name);
            else
                GameStartPath("官方游戏", e.ClickedItem.Name);
        }

        private void 启动游戏模式_Click(object sender, EventArgs e)
        {
            启动游戏模式.Enabled = false;
            启动游戏模式.Checked = true;
            设置路径模式.Enabled = true;
            设置路径模式.Checked = false;
        }

        private void 设置路径模式_Click(object sender, EventArgs e)
        {
            设置路径模式.Enabled = false;
            设置路径模式.Checked = true;
            启动游戏模式.Enabled = true;
            启动游戏模式.Checked = false;
        }
        #endregion

        /*音乐播放器*/
        #region
        private void 添加歌曲_Click(object sender, EventArgs e)
        {
            OpenFileDialog Music = new OpenFileDialog();
            Music.Filter = @"音乐文件|*.mp3";
            Music.Multiselect = true;
            Music.Title = "请选择音乐文件";
            if (Music.ShowDialog() == DialogResult.OK)
            {
                FileStream MusicPath = new FileStream("Data/MusicPath", FileMode.Append);
                StreamWriter MusicSW = new StreamWriter(MusicPath);
                foreach (string music in Music.FileNames)
                {
                    bool IsChongfu = false;
                    foreach (string oldmusic in Musicpath)
                        if (oldmusic == music)
                        {
                            IsChongfu = true;
                            break;
                        }
                    if (IsChongfu)
                        continue;
                    MusicSW.WriteLine(music);
                    Musicpath.Add(music);
                }
                MusicSW.Close();
                MusicPath.Close();
            }
        }

        /*播放歌曲函数*/
        public void MusicPlay()
        {
            if (Musicpath.Count == 0)
            {
                MessageBox.Show("音乐播放列表为空，请先添加音乐！");
                return;
            }
            Random r = new Random();
            MusicPlayer.URL = Musicpath[r.Next(Musicpath.Count)];
            ImageAnimator.StopAnimate(imagegif, new EventHandler(this.OnFrameChanged));
            imagegif = imagegif5;
            Imageindex = 2;
            PlayAnimate();
        }
        private void 播放歌曲_Click(object sender, EventArgs e)
        {
            MusicPlay();
        }

        /*停止播放函数*/
        public void MusicPlayStop()
        {
            MusicPlayer.URL = null;
            ImageAnimator.StopAnimate(imagegif, new EventHandler(this.OnFrameChanged));
            imagegif = imagegif2;
            Imageindex = 2;
            PlayAnimate();
        }
        private void 停止播放ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MusicPlayStop();
        }

        private void MusicPlayer_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            if (MusicPlayer.playState == WMPLib.WMPPlayState.wmppsMediaEnded)
                timer2.Start();
        }

        //因无法在播放状态改变事件响应函数中直接改变URL播放歌曲故延时100ms在事件响应函数外执行
        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();
            Random r = new Random();
            MusicPlayer.URL = Musicpath[r.Next(Musicpath.Count)];
        }
        #endregion

        /*语音识别*/
        #region
        private void 语音识别开启ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            语音识别关闭ToolStripMenuItem.Checked = false;
            语音识别开启ToolStripMenuItem.Checked = true;
            语音识别关闭ToolStripMenuItem.Enabled = true;
            语音识别开启ToolStripMenuItem.Enabled = false;
            Sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void 语音识别关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            语音识别关闭ToolStripMenuItem.Checked = true;
            语音识别开启ToolStripMenuItem.Checked = false;
            语音识别关闭ToolStripMenuItem.Enabled = false;
            语音识别开启ToolStripMenuItem.Enabled = true;
            Sre.RecognizeAsyncStop();
        }

        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Text == RmCommond["唤醒"])
            {
                ZhaYan();
                SreCom = true;
            }
            else if (SreCom)
            {
                if (e.Result.Text == RmCommond["退出"])
                    Environment.Exit(0);
                else if (e.Result.Text == RmCommond["打开贴吧"])
                {
                    Process.Start("https://tieba.baidu.com/f?kw=%E4%B8%9C%E6%96%B9&fr=index&fp=0&ie=utf-8");
                    SreCom = false;
                }
                else if (e.Result.Text == RmCommond["播放音乐"])
                {
                    MusicPlay();
                    SreCom = false;
                }
                else if (e.Result.Text == RmCommond["停止播放"])
                {
                    MusicPlayStop();
                    SreCom = false;
                }
            }
        }
        #endregion
    }
}
