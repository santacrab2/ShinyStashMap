using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;
using ShinyStashMap.Properties;
using System.Text.RegularExpressions;
using Point = TransformScale;
namespace ShinyStashMap;

public partial class Form1 : Form
{
    private ISaveFileProvider SAV { get; }
    public Dictionary<string, (byte[], float[])> Spawners = [];
    public List<(PA9, byte[])> ShinyEntities = [];
    public Form1(ISaveFileProvider sav)
    {
        SAV = sav;
        InitializeComponent();
        var resourceText = Properties.Resources.t1_point_spawners;
        var Lines = resourceText.Replace("\r", "").Split('\n');
        Spawners = ParseToDictionary(Lines);
        GetShinyBlock();

    }
    public void GetShinyBlock()
    {
        var ShinyBlock = ((SAV9ZA)SAV.SAV).Accessor.GetBlock(0xF3A8569D).Data;
        int i = 0;
        while (ShinyBlock[i] != 0x45 && ShinyBlock[i+1] != 0x26 && ShinyBlock[i+2] != 0x22 && ShinyBlock[i+3] != 0x84 && ShinyBlock[i+4] != 0xE4 && ShinyBlock[i+5] != 0x9C && ShinyBlock[i+6] != 0xF2 && ShinyBlock[i+7] != 0xCB)
        {
            ShinyEntities.Add((new PA9(ShinyBlock[(i+0x8)..(i+0x8 + 0x158)].ToArray()), ShinyBlock[i..(i + 8)].ToArray()));
            i += 0x1F0;
        }
        setPBs();
    }
    public static ushort test = 0;
    public void setPBs()
    {
        for (int i = 0; i < ShinyEntities.Count; i++)
        {
            ((PictureBox)groupBox1.Controls[i]).Image = ShinyEntities[i].Item1.Sprite();
            ((PictureBox)groupBox1.Controls[i]).Click += (s, _) => renderpoint(groupBox1.Controls.IndexOf((Control)s));
        }
    }
    public void renderpoint(int index)
    {
        var hash = BitConverter.ToString(ShinyEntities[index].Item2.Reverse().ToArray()).Replace("-", "");
        var coords = Spawners[hash].Item2;
        var img = Resources.lumiose;
        using var gr = Graphics.FromImage(img);
        pictureBox1.BackgroundImage = img;
        using var brush = new SolidBrush(Color.Red);
        RenderPoints(gr, TransformLumiose, brush, new Point(coords[0], coords[2]));
        
    }
    public static float x;
    public static float y;
    public void RenderPoints(Graphics gr, MapTransform tr, Brush brush, params ReadOnlySpan<Point> coords)
    {
        foreach (var pt in coords)
        {
            const int length = (10 * 2) + 1; // Diameter of the circle
            x = (float)tr.ConvertX(pt.X) - (length / 2.0f);
            y = (float)tr.ConvertZ(pt.Z) - (length / 2.0f);
            gr.FillEllipse(brush, x, y, 100, 100);
        }
    }
    static Dictionary<string, (byte[],float[])> ParseToDictionary(IEnumerable<string> lines)
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
}

