using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp8
{
    public partial class Form1 : Form
    {
        Timer t;
        Timer t2;
        Timer t3;

        int timecount = 0;
        int nBlocks = 24;
        Rectangle[] blocks = new Rectangle[100];
        Rectangle racket = new Rectangle();
        Rectangle ball = new Rectangle();
        bool[] bvisible = new bool[100];

        Font f = new Font("Arial", 20, FontStyle.Bold);     // game over & stage
        Font f2 = new Font("Arial", 30, FontStyle.Bold);    // all clear
        Font l = new Font("Arial", 10, FontStyle.Italic);   // life

        Brush racketColor = new SolidBrush(Color.PaleVioletRed);
        Brush ballColor = new SolidBrush(Color.LightPink);
        Brush[] blockColor = new SolidBrush[2];

        Pen pen = new Pen(Color.Black, 2);                  // ball
        Pen p = new Pen(Color.Pink, 10);                    // restart

        static Random rnd = new Random();   // for blocks (static -> avoid duplication)

        int Life = 3;
        int[] flag = new int[100];
        int[] idx = new int[100];
        int stage = 1;  // up to 3
        int stage_flag = 0;
        int sign = 0;

        int timer_flag = 0;

        int racketY = 400;
        int racketW = 120;
        int racketH = 10;

        int formW = 300;
        int formH = 500;

        int blockY = 60;
        int blockW = 30;
        int blockH = 20;

        int ballW = 10;
        int ballH = 10;

        Random r = new Random();            // for slope
        private double slope;
        private Graphics g;
        private int vDir = 1;

        public Form1()
        {
            blockColor[0] = new SolidBrush(Color.HotPink);
            blockColor[1] = new SolidBrush(Color.Pink);

            InitializeComponent();
            ClientSize = new Size(300, 500);
            this.Text = "Breaking block";

            initbVisible();
            initRacket();
            StartBall();

            t = new Timer();
            t.Interval = 100;
            t.Tick += t_Tick;
            t.Start();

            t2 = new Timer();
            t2.Interval = 10000;
            t2.Tick += t2_Tick;
            t2.Start();

            t3 = new Timer();
            t3.Interval = 1000;
            t3.Tick += t3_Tick;
            t3.Start();
        }
        private void StartBall()
        {
            initBall();
            slope = r.Next(5, 20) / 10.0;
            if (r.Next(2) == 1)
                slope = -slope;
        }

        private void initBall()
        {
            // initialization
            ball.X = formW / 2 - ballW / 2;
            ball.Y = racket.Y - 150;
            ball.Width = ballW;
            ball.Height = ballH;
        }

        private void initRacket()
        {
            // initialization
            racket.X = formW / 2 - racketW / 2;
            racket.Y = racketY;
            racket.Width = racketW;
            racket.Height = racketH;
        }

        private void initbVisible()
        {
            // initialization
            for (int i = 0; i < nBlocks; i++)
            {
                bvisible[i] = true;
                flag[i] = 0;
                idx[i] = 0;
            }

            // Create special blocks from stage 2 and above
            // The special block will not disappear until it is hit twice.
            if (stage == 2) // Stage2: Half of the total is special blocks.
            {
                for (int i = 0; i < nBlocks / 2; i++)
                {
                    flag[rnd.Next(0, nBlocks)] = 1;
                }
            }
            else if (stage == 3) // Stage3: Two-thirds of the total is special blocks.
            {
                for (int i = 0; i < nBlocks / 3; i++)
                {
                    flag[rnd.Next(0, nBlocks)] = 1;
                }
            }
        }

        // Determine if all blocks are invisible (It means the stage cleared)
        private void checkbVisible()
        {
            for (int i = 0; i < nBlocks; i++)
            {
                if (bvisible[i] == true) // if any of the blocks remain
                {
                    return;
                }
            }

            // if all blocks are invisible
            stage_flag = 0;
            stage++;    // move to the next stage
        }
        private void t3_Tick(object sender, EventArgs e)
        {
            timecount++;
        }
        private void t2_Tick(object sender, EventArgs e)
        {
            blockY += 20;
        }
        private void t_Tick(object sender, EventArgs e)
        {
            ball.X += (int)(10 * slope);
            ball.Y += vDir * 10;

            if (ball.X >= formW - ballW)
                slope = -slope;
            if (ball.X <= 0)
                slope = -slope;

            if (ball.IntersectsWith(racket))
            {
                vDir = -vDir;   // Change the direction of the ball
            }

            // game over condition 2
            for (int i = 0; i < nBlocks; i++)
            {
                if (blocks[i].IntersectsWith(racket))
                {
                    sign = 1;
                }
            }

            if (stage > 1)  // Some blocks have to be hit twice from stage 2 and above
            {
                for (int i = 0; i < nBlocks; i++)
                    if (bvisible[i] == true && blocks[i].IntersectsWith(ball))
                    {
                        if (flag[i] == 0)
                        {
                            flag[i] = 1;        // flag == 1: a once-hit block -> next time, it should be removed
                            idx[i] = 1;         // idx: for changing the color of the block
                        }
                        else if (flag[i] == 1)  // twice-hit -> remove
                        {
                            bvisible[i] = false;
                        }
                        vDir = -vDir;
                    }
            }

            else            // stage == 1: when hit, block disappears and ball changes the direction
            {
                for (int i = 0; i < nBlocks; i++)
                    if (bvisible[i] == true && blocks[i].IntersectsWith(ball))
                    {
                        bvisible[i] = false;
                        vDir = -vDir;
                    }
            }

            if (ball.Y <= ballH)    // when the ball reaches the border of the window, change the direction
                vDir = -vDir;

            t.Interval = 100;
            Invalidate();
            checkbVisible();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            g = e.Graphics;

            if (stage_flag == 0)    // stage_flag == 0: when the stage cleared and changed
            {
                if (stage == 4)     // All Stage Clear
                {
                    g.Clear(BackColor);
                    g.DrawString("You Win!", f2, Brushes.Gold, 60, 200);
                    t.Stop();
                    t2.Stop();
                    t3.Stop();
                }
                else                // To move on to the next stage
                {
                    t.Interval = 1000;  // excute for 1 second
                    g.Clear(BackColor);
                    g.DrawString("Stage " + stage.ToString(), f, Brushes.Black, 95, 200);
                    stage_flag = 1;
                    if (stage > 1)  // change the values of 'nBlocks' and 'racketW' from the second stage
                        NextStage();
                }
            }
            else                    // stage_flag == 1: game in progress
            {
                drawBlocks();
                drawRacket();
                drawBall();

                // show remaining value of 'life'
                g.DrawString("Life: " + Life.ToString(), l, Brushes.Blue, 10, 10);
                g.DrawString("Time: " + timecount.ToString(), l, Brushes.Blue, 220, 10);

                // sign == 1: blocks intersect with racket
                if (sign == 1)
                {
                    t3.Stop();
                    t2.Stop();
                    g.Clear(BackColor);
                    g.DrawString("Game Over", f, Brushes.Black, 80, 200);
                    t.Stop();
                }
                if (ball.Y > formH) // when the ball fell unnder the window form
                {
                    Life--;
                    if (Life <= 0)  // Game Over
                    {
                        t3.Stop();
                        t2.Stop();
                        g.Clear(BackColor);
                        g.DrawString("Game Over", f, Brushes.Black, 80, 200);
                        t.Stop();
                    }
                    else if (sign == 1)
                    {
                        t3.Stop();
                        t2.Stop();
                        g.Clear(BackColor);
                        g.DrawString("Game Over", f, Brushes.Black, 80, 200);
                        t.Stop();
                    }

                    else if (Life > 0)          // Restart
                    {
                        t3.Stop();
                        t2.Stop();
                        initBall();
                        initbVisible();
                        t.Interval = 1000;  // excute for 1 second
                        g.Clear(BackColor);
                        g.DrawRectangle(p, 90, 190, 125, 50);
                        g.DrawString("Restart", f, Brushes.Violet, 100, 200);
                        blockY = 60;
                        timecount = 0;
                        t3.Start();
                        t2.Start();
                    }
                }
            }
        }

        private void drawBall()
        {
            // According to the value of 'ballColor' and 'ball', draw a ball
            // And adding a border line
            g.FillRectangle(ballColor, ball);
            g.DrawRectangle(pen, ball);
        }

        private void drawRacket()
        {
            // According to the value of 'racketColor' and 'racket', draw a racket
            g.FillRectangle(racketColor, racket);
        }

        private void drawBlocks()
        {
            for (int i = 0; i < nBlocks; i++)
            {
                if (bvisible[i] == true)
                {
                    blocks[i] = new Rectangle((i % 8 + 1) * blockW, blockY + blockH * (i / 8 + 1), blockW - 1, blockH - 1);
                    g.FillRectangle(blockColor[idx[i]], blocks[i]);
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // a racket moves to the left
            if (e.KeyCode == Keys.Left)
            {
                if (racket.X >= 0)
                {
                    racket.X -= 10;
                }
            }

            // a racket moves to the right
            else if (e.KeyCode == Keys.Right)
            {
                if (racket.X < formW - racket.Width)
                {
                    racket.X += 10;
                }
            }

            //////////// an unrelated part of the game, for convenience only ////////////
            else if (e.KeyCode == Keys.Escape)  // Exit Window
            {
                Application.Exit();
            }
            else if (e.KeyCode == Keys.Space)   // Timer pause control
            {
                switch (timer_flag)
                {
                    case 0:
                        t3.Stop();
                        t.Stop();
                        t2.Stop();
                        timer_flag = 1;
                        break;
                    case 1:
                        t3.Start();
                        t.Start();
                        t2.Start();
                        timer_flag = 0;
                        break;
                }
            }
            else if (e.KeyCode == Keys.Q)    // Change to the next stage
            {
                stage_flag = 0;
                stage++;
            }
            else if (e.KeyCode == Keys.W)
            {
                blockY += 20;
            }
            ////////////////////////////////////////////////////////////////////////////
        }

        private void NextStage()
        {
            t.Stop();
            t2.Stop();
            t3.Stop();
            t.Interval = 1000;
            timecount = 0;
            // Change the value of 'nBlocks' and 'racketW' each time the stage changes
            nBlocks += 16;
            racketW -= 20;
            blockY = 60;

            initbVisible();
            initRacket();
            initBall();

            StartBall();
            t.Start();
            t2.Start();
            t3.Start();
        }
    }
}
