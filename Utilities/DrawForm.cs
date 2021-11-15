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
using System.Runtime.Versioning;
using Rectangle = MobarezooServer.Utilities.Geometry.Rectangle;

public class MainForm : System.Windows.Forms.Form
{
    private System.ComponentModel.Container components;
    GameManager game;
    public MainForm(GameManager gm)
    {
        game = gm;
        InitializeComponent();
        CenterToScreen();
        SetStyle(ControlStyles.ResizeRedraw, true);
        this.TopMost = true;
        this.Size = new Size()
        {
            Height = 600,
            Width = 380
        };
        
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
        Graphics g = this.CreateGraphics();
        Pen p = new Pen(Color.Black, 2);
     
        if (game.rooms.Count > 0)
        {
            var room = game.rooms[0];
            lock (room.objectsLock)
            {
                foreach (GameObject go in room.objects)
                {
                    if (go.shape is Circle)
                    {
                        drawCircle(go.shape as Circle,  g , p);
                    }
                    else
                    {
                        drawRect(go.shape as Rectangle , g , p);
                    }
                }
                p.Color = Color.DarkGreen;
                foreach (var tr in room.traps)
                {
                    var rect = tr.shape;
                    if (rect is Circle circle)
                    {
                        drawCircle(circle,g,p);
                    }
                    else
                    {
                        drawRect((Rectangle) rect, g, p);
                    }
                }
                p.Color = Color.DarkRed;
                foreach (var ob in room.obstacles)
                {
                    var rect = ob.shape;
                    drawRect(rect as Rectangle, g, p);
                }
                p.Color = Color.DarkCyan;
                foreach (var pla in room.summoners)
                {
                    var pl = pla.champion;
                    drawCircle(pl.hitCircle , g , p);
                }
            }
        }
    }


    public void drawCircle(Circle circle , Graphics g , Pen p)
    {
        g.DrawEllipse(p,(700 + (circle.position.X) - circle.radius)  / 4 , (1100 + (circle.position.Y) - circle.radius) /4 , circle.radius/2 , circle.radius/2);
    }
    
    private void drawRect(MobarezooServer.Utilities.Geometry.Rectangle rect , Graphics g , Pen p)
    {
        List<PointF> points = new List<PointF>();
        var offsetX = 700;
        var offsetY = 1100;
        points.Add(new PointF() {
               X = ( rect.corner[0].X + offsetX)/4,
               Y = ( rect.corner[0].Y + offsetY)/4,
        });
        points.Add(new PointF()
        {
            X = (rect.corner[1].X + offsetX)/4,
            Y = (rect.corner[1].Y + offsetY)/4,
        });
        points.Add(new PointF()
        {
            X = (rect.corner[2].X + offsetX)/4,
            Y = (rect.corner[2].Y + offsetY)/4,
        });
        points.Add(new PointF()
        {
            X = (rect.corner[3].X + offsetX)/4,
            Y = (rect.corner[3].Y + offsetY)/4,
        });
        points.Add(new PointF()
        {
            X = (rect.corner[0].X + offsetX) / 4,
            Y = (rect.corner[0].Y + offsetY) / 4,
        });

        g.DrawLines(p, points.ToArray());
    }

    private void Form1_Resize(object sender, System.EventArgs e)
    {

    }
}