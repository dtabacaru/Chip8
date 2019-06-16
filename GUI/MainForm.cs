using Chip8.Core;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI
{
    public partial class MainForm : Form
    {
        private Processor m_Chip8Emulator = null;
        private Processor m_Chip8SaveState = null;
        private bool m_Running = false;

        private Brush m_SetBrush = Brushes.Black;
        private Brush m_UnSetBrush = Brushes.White;

        private bool[,] m_Pixels = new bool[CPU_CONSTANTS.DISPLAY_WIDTH, CPU_CONSTANTS.DISPLAY_HEIGHT];
        private bool[,] m_PreviousPixels = new bool[CPU_CONSTANTS.DISPLAY_WIDTH, CPU_CONSTANTS.DISPLAY_HEIGHT];
        private bool m_PartyMode = false;
        private int m_Scale = 0;

        private bool m_WrapY = true;
        private bool m_Debug = false;

        private Random m_Rand = new Random();
        private long m_PaintCount = 0;

        System.Timers.Timer timer = new System.Timers.Timer();

        public MainForm()
        {
            InitializeComponent();
            m_Scale = ScreenPanel.Width / CPU_CONSTANTS.DISPLAY_WIDTH;

            for (int i = 0; i < m_Pixels.GetLength(0); i++)
            {
                for (int j = 0; j < m_Pixels.GetLength(1); j++)
                {
                    m_Pixels[i, j] = false;
                    m_PreviousPixels[i, j] = false;
                }
            }

            timer.Interval = ((1.0 / 60) * 1000)/2;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool changed = false;
            for (int i = 0; i < m_Pixels.GetLength(0); i++)
            {
                for (int j = 0; j < m_Pixels.GetLength(1); j++)
                {
                    if (m_Pixels[i, j] != m_PreviousPixels[i, j])
                        changed = true;

                }
            }

            if(changed)
                ScreenPanel.Invalidate();
        }

        private void Processor_DisplayEvent(object sender, DisplayEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate () { Processor_DisplayEvent(sender, e); }));
                return;
            }

            m_Pixels = e.Display;
        }

        private void ScreenPanel_Paint(object sender, PaintEventArgs e)
        {
            if(m_PartyMode && (m_PaintCount % 10 == 0))
            {
                m_SetBrush = new SolidBrush(Color.FromArgb(255,m_Rand.Next(0, 255), m_Rand.Next(0, 255), m_Rand.Next(0, 255)));
                m_UnSetBrush = new SolidBrush(Color.FromArgb(255, m_Rand.Next(0, 255), m_Rand.Next(0, 255), m_Rand.Next(0, 255)));
            }

            e.Graphics.FillRectangle(m_UnSetBrush, 0, 0, ScreenPanel.Width, ScreenPanel.Height);

            int scaledX = 0;
            for (int x = 0; x < m_Pixels.GetLength(0); x++)
            {
                int scaledY = 0;
                for (int y = 0; y < m_Pixels.GetLength(1); y++)
                {
                    if (m_Pixels[x, y])
                        e.Graphics.FillRectangle(m_SetBrush, scaledX, scaledY, m_Scale, m_Scale);

                    scaledY += m_Scale;
                }

                scaledX += m_Scale;
            }

            for (int i = 0; i < m_Pixels.GetLength(0); i++)
            {
                for (int j = 0; j < m_Pixels.GetLength(1); j++)
                {
                    m_PreviousPixels[i, j] = m_Pixels[i, j];
                }
            }

            m_PaintCount++;
        }

        private void StartEmulator()
        {
            m_Chip8Emulator.WrapY = m_WrapY;
            m_Chip8Emulator.DebugLog = m_Debug;
            m_Chip8Emulator.DisplayEvent += Processor_DisplayEvent;

            m_Running = true;

            Task.Run(() =>
            {
                try
                {
                    m_Chip8Emulator.Run();
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    m_Chip8Emulator.DisplayEvent -= Processor_DisplayEvent;
                    m_Chip8SaveState = null;
                    m_Chip8Emulator.Stop = true;
                    m_Chip8Emulator = null;
                    m_Running = false;
                }
            });
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(m_Running)
            {
                m_Chip8Emulator.DisplayEvent -= Processor_DisplayEvent;
                m_Chip8Emulator.Stop = true;
            }

            m_Chip8SaveState = null;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All Files (*.*)|*.*";
            ofd.Multiselect = false;

            DialogResult dr = ofd.ShowDialog();

            if (!dr.Equals(DialogResult.OK) || ofd.FileNames.Length < 1)
                return;

            m_Chip8Emulator = new Processor(ofd.FileName);

            StartEmulator();
        }

        private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (m_Chip8Emulator != null)
            {
                if (e.KeyChar == 53)
                    saveState5ToolStripMenuItem_Click(this, null);
                else if (e.KeyChar == 54)
                    loadState6ToolStripMenuItem_Click(this, null);
                else
                    m_Chip8Emulator.KeyPress(e.KeyChar);
            }
                
        }

        private void backgroundColourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            DialogResult dg = cd.ShowDialog();

            if (dg == DialogResult.OK)
                m_UnSetBrush = new SolidBrush(cd.Color);

            ScreenPanel.Invalidate();
        }

        private void pixelColourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            DialogResult dg = cd.ShowDialog();

            if(dg == DialogResult.OK)
                m_SetBrush = new SolidBrush(cd.Color);

            ScreenPanel.Invalidate();
        }

        private void partyModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            partyModeToolStripMenuItem.Checked = !partyModeToolStripMenuItem.Checked;
            m_PartyMode = partyModeToolStripMenuItem.Checked;
        }

        private void wrapYToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wrapYToolStripMenuItem.Checked = !wrapYToolStripMenuItem.Checked;
            m_WrapY = wrapYToolStripMenuItem.Checked;
        }

        private void debugLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            debugLogToolStripMenuItem.Checked = !debugLogToolStripMenuItem.Checked;
            m_Debug = debugLogToolStripMenuItem.Checked;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }

        private void saveState5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_Chip8Emulator != null)
                m_Chip8SaveState = m_Chip8Emulator.Copy();
        }

        private void loadState6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(m_Chip8SaveState != null)
            {
                if (m_Running)
                {
                    m_Chip8Emulator.DisplayEvent -= Processor_DisplayEvent;
                    m_Chip8Emulator.Stop = true;
                }

                m_Chip8Emulator = m_Chip8SaveState.Copy();
                StartEmulator();
            }

        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (m_Chip8Emulator != null)
                m_Chip8Emulator.Pause = true;
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (m_Chip8Emulator != null)
                m_Chip8Emulator.Pause = false;
        }
    }
}
