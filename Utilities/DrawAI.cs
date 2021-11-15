using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using MobarezooServer.GamePlay;
using MobarezooServer.Gameplay.GameObjects;
using MobarezooServer.Utilities.Geometry;
using System.Collections.Generic;
using System.Linq;
using MobarezooServer.AI;
using Rectangle = MobarezooServer.Utilities.Geometry.Rectangle;

public class AIForm : System.Windows.Forms.Form
{
    private System.ComponentModel.Container components;
    AISystem.ArtificalRoom game;

    public AIForm(AISystem.ArtificalRoom gm)
    {
        game = gm;
        InitializeComponent();
        CenterToScreen();
        SetStyle(ControlStyles.ResizeRedraw, true);
        this.TopMost = true;
        BackColor = Color.FromArgb(77,77,77);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (components != null)
            {
                components.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Timer t = new Timer();
        t.Interval = 10;
        t.Tick += new System.EventHandler(this.updateStat);
        t.Start();
        Console.WriteLine("WE ARE INITIALIZED");
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(120 * 4, 200 * 4);
        this.Text = "ROOM 0";
        this.Resize += new System.EventHandler(this.Form1_Resize);
        this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
    }

    private void updateStat(object sender, System.EventArgs e)
    {
        this.Refresh();
    }

    private void MainForm_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
    {
        try
        {
            Graphics g = this.CreateGraphics();
            
            Pen p = new Pen(Color.DarkGray, 2);

            var room = game;
            foreach (var circle in room.gameObjects.ToList())
            {
                g.DrawEllipse(p, (700 + circle.position.X - 5) / 4, (circle.position.Y - 5 + 1100) / 4, 10 / 4f, 10 / 4f);
            }
            p.Color = Color.Red;
            p.Width = 5;
            g.DrawEllipse(p, (700 + room.boss.X - 5) / 4, (room.boss.Y - 5 + 1100) / 4, 25f, 25f);
            p.Width = 1;

            p.Color = Color.DarkCyan;
            //  pl = room.fightingAI;
            //  g.DrawEllipse(p, (700 + pl.position.X - rd) / 4, (pl.position.Y - rd + 1100) / 4, rd / 2, rd / 2);
            foreach (var pla in room.players)
            {
                var rd = 50;
                var pl = pla;
                g.DrawEllipse(p, (700 + pl.position.X - rd) / 4, (pl.position.Y - rd + 1100) / 4, rd / 2, rd / 2);
            }

            p.Color = Color.Red;
            p.Width = 5;
            g.DrawEllipse(p, (700 + room.boss.X - 5) / 4, (room.boss.Y - 5 + 1100) / 4, 25f, 25f);
        }
        catch (Exception m)
        {
            // nothing
        }
    }

    private void Form1_Resize(object sender, System.EventArgs e)
    {
    }
}