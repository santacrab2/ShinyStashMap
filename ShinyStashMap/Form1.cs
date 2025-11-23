using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using ShinyStashMap.Properties;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Point = TransformScale;
namespace ShinyStashMap;

// Thanks kwsch for like 75% of this code.
public partial class Form1 : Form
{
    private ISaveFileProvider SAV { get; }
    public Dictionary<string, (byte[], float[])> SpawnersLumiose = [];
    public Dictionary<string, (byte[], float[])> SpawnersLysandre = [];
    public Dictionary<string, (byte[], float[])> SpawnersSewers = [];
    public Dictionary<string, (byte[], float[])> SpawnersSewersB = [];

    public List<(PA9, byte[])> ShinyEntities = [];
    public bot Bot = new();
    public Form1(ISaveFileProvider sav)
    {
        SAV = sav;
        InitializeComponent();
        var resourceText = Properties.Resources.t1_point_spawners;
        var Lines = resourceText.Replace("\r", "").Split('\n');
        SpawnersLumiose = ParseToDictionary(Lines);
        resourceText = Properties.Resources.t2_point_spawners;
        Lines = resourceText.Replace("\r", "").Split('\n');
        SpawnersLysandre = ParseToDictionary(Lines);
        resourceText = Properties.Resources.t3_point_spawners;
        Lines = resourceText.Replace("\r", "").Split('\n');
        SpawnersSewers = ParseToDictionary(Lines);
        resourceText = Properties.Resources.t4_point_spawners;
        Lines = resourceText.Replace("\r", "").Split('\n');
        SpawnersSewersB = ParseToDictionary(Lines);
        GetShinyBlock();

    }
    public void GetShinyBlock()
    {
        if (ShinyEntities.Count > 0)
            ShinyEntities.Clear();
        var ShinyBlock = ((SAV9ZA)SAV.SAV).Accessor.GetBlock(0xF3A8569D).Data;
        int i = 0;
        while (BitConverter.ToString(ShinyBlock[i..(i + 8)].ToArray()).Replace("-","") != "45262284E49CF2CB" && (i + 0x1F0) <= ShinyBlock.Length)
        {
            ShinyEntities.Add((new PA9(ShinyBlock[(i + 0x8)..(i + 0x8 + 0x158)].ToArray()), ShinyBlock[i..(i + 8)].ToArray()));
            i += 0x1F0;
        }
        SetPBs();
    }
    public void SetPBs()
    {
        for (int i = 0; i < ShinyEntities.Count; i++)
        {
            ((PictureBox)groupBox1.Controls[i]).Image = ShinyEntities[i].Item1.Sprite();
            ((PictureBox)groupBox1.Controls[i]).Click += (s, _) => Renderpoint(groupBox1.Controls.IndexOf((Control)s));
            ((PictureBox)groupBox1.Controls[i]).MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    ToolStripMenuItem Teleport = new("Teleport");
                    Teleport.Click += (_, _) => Teleporter(groupBox1.Controls.IndexOf((Control)s));
                    ContextMenuStrip menu = new();
                    menu.Items.Add(Teleport);
                    ContextMenuStrip = menu;
                }
            };
        }
    }
    public void Teleporter(int index)
    {
        var hash = BitConverter.ToString(ShinyEntities[index].Item2.Reverse().ToArray()).Replace("-", "");
        byte[] coords = [];
        if (SpawnersLumiose.TryGetValue(hash, out (byte[], float[]) value))
        {
            coords = value.Item1;
            var pt = Bot.FollowMainPointer([..PlayerPositionPointer]);
            Bot.WriteBytes(coords, pt);
        }
        else if (SpawnersLysandre.TryGetValue(hash, out (byte[], float[]) value2))
        {
            coords = value2.Item1;
            var pt = Bot.FollowMainPointer([.. PlayerPositionPointer]);
            Bot.WriteBytes(coords, pt);
        }
        else if (SpawnersSewers.TryGetValue(hash, out (byte[], float[]) value3))
        {
            coords = value3.Item1;
            var pt = Bot.FollowMainPointer([.. PlayerPositionPointer]);
            Bot.WriteBytes(coords, pt);
        }
        else if (SpawnersSewersB.TryGetValue(hash, out (byte[], float[]) value4))
        {
            coords = value4.Item1;
            var pt = Bot.FollowMainPointer([.. PlayerPositionPointer]);
            Bot.WriteBytes(coords, pt);
        }
    }
    public void Renderpoint(int index)
    {
        var hash = BitConverter.ToString(ShinyEntities[index].Item2.Reverse().ToArray()).Replace("-", "");
        float[] coords = [];
        if (SpawnersLumiose.TryGetValue(hash, out (byte[], float[]) value))
        {
            coords = value.Item2;
            var img = Resources.lumiose;
            using var gr = Graphics.FromImage(img);
            pictureBox1.BackgroundImage = img;
            using var brush = new SolidBrush(Color.Red);
            RenderPoints(gr, TransformLumiose, brush, new Point(coords[0], coords[2]));
        }
        else if (SpawnersLysandre.TryGetValue(hash, out (byte[], float[]) value2))
        {
            coords = value2.Item2;
            var img = Resources.LysandreLabs;
            using var gr = Graphics.FromImage(img);
            pictureBox1.BackgroundImage = img;
            using var brush = new SolidBrush(Color.Red);
            RenderPoints(gr, TransformLysandreLabs, brush, new Point(coords[0], coords[2]));
        }
        else if (SpawnersSewers.TryGetValue(hash, out (byte[], float[]) value3))
        {
            coords = value3.Item2;
            var img = Resources.Sewers;
            using var gr = Graphics.FromImage(img);
            pictureBox1.BackgroundImage = img;
            using var brush = new SolidBrush(Color.Red);
            RenderPoints(gr, TransformLysandreLabs, brush, new Point(coords[0], coords[2]));
        }
        else if (SpawnersSewersB.TryGetValue(hash, out (byte[], float[]) value4))
        {
            coords = value4.Item2;
            var img = Resources.SewersB;
            using var gr = Graphics.FromImage(img);
            pictureBox1.BackgroundImage = img;
            using var brush = new SolidBrush(Color.Red);
            RenderPoints(gr, TransformLysandreLabs, brush, new Point(coords[0], coords[2]));
        }


    }
    public static float x;
    public static float y;
    public void RenderPoints(Graphics gr, MapTransform tr, Brush brush, params ReadOnlySpan<Point> coords)
    {
        foreach (var pt in coords)
        {
            x = (float)tr.ConvertX(pt.X) - (100 / 2.0f);
            y = (float)tr.ConvertZ(pt.Z) - (100 / 2.0f);
            gr.FillEllipse(brush, x, y, 100, 100);
        }
    }
    static Dictionary<string, (byte[], float[])> ParseToDictionary(IEnumerable<string> lines)
    {
        var hashRe = new Regex(@"([A-Fa-f0-9]{16})");
        var v3fRe = new Regex(@"V3f\((-?\d+\.?\d*),\s*(-?\d+\.?\d*),\s*(-?\d+\.?\d*)\)");

        var dict = new Dictionary<string, (byte[], float[])>();

        foreach (var line in lines)
        {
            try
            {
                string hash = hashRe.Match(line).Groups[1].Value;

                var m = v3fRe.Match(line);
                float x = float.Parse(m.Groups[1].Value);
                float y = float.Parse(m.Groups[2].Value);
                float z = float.Parse(m.Groups[3].Value);

                byte[] bytes = new byte[12];
                Buffer.BlockCopy(BitConverter.GetBytes(x), 0, bytes, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(y), 0, bytes, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(z), 0, bytes, 8, 4);

                dict[hash] = (bytes, new float[] { x, y, z });
            }
            catch
            {
                continue;
            }
        }
        return dict;
    }
    public static readonly MapTransform TransformLumiose = new(
       Texture: new(4096.0, 4096.0),
       Range: new(3940.0, 3940.0),
       Scale: new(1000.0, 1000.0),
       Dir: new(-1.0, -1.0),
       Offset: new(500.0, 500.0)
   );
    public static readonly MapTransform TransformLysandreLabs = new(
        Texture: new(2160.0, 2160.0),
        Range: new(1662.0, 2041.0),
        Scale: new(1662.0 / 10.291021, 2041.0 / 10.291021),
        Dir: new(-1.0, -1.0),
        Offset: new(-3.0, -80.0)
    );

    public static readonly MapTransform TransformSewersCh5 = new(
        Texture: new(2160.0, 2160.0),
        Range: new(1364.0, 1975.0),
        Scale: new(1364.0 / 6.2, 1975.0 / 6.2),
        Dir: new(1.0, 1.0),
        Offset: new(1.0, 146.0)
    );

    public static readonly MapTransform TransformSewersCh6 = new(
        Texture: new(2160.0, 2160.0),
        Range: new(1521.0, 1966.0),
        Scale: new(1521.0 / 16.714285, 1966.0 / 16.714285),
        Dir: new(1.0, 1.0),
        Offset: new(39.0, 45.0)
    );
    private void button1_Click(object sender, EventArgs e)
    {
        Bot.Connect(textBox1.Text, 6000);
        button1.Enabled = false;
        button1.Text = "Connected";
        ReadShinyStashLive();
    }
    private void ReadShinyStashLive()
    {
        var ShinyBlock = ((SAV9ZA)SAV.SAV).Accessor.GetBlock(0xF3A8569D);
        var newblock = Bot.ReadBytes([..ShinyStashPointer], ShinyBlock.Data.Length);
        ShinyBlock.ChangeData(newblock);
        GetShinyBlock();
    }
    public static IReadOnlyList<long> ShinyStashPointer { get; } = [0x5F0C250, 0x120, 0x168, 0x0];
    public static IReadOnlyList<long> PlayerPositionPointer { get; } = [0x41ED340, 0x248, 0x00, 0x138, 0x90];
    protected override void OnClosing(CancelEventArgs e)
    {
        try
        {
            Bot.Disconnect();
        }catch (SocketException)
        {
            // ignore
        }
        base.OnClosing(e);
    }
}

