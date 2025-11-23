using PKHeX.Core;
using System;
namespace ShinyStashMap;

public class SSM : IPlugin
{
    public string Name { get; private set; } = "Stored Shiny Map";
    public int Priority { get; private set; } = 1;
    public ISaveFileProvider SaveFileEditor { get; private set; } = null!;
    protected IPKMView PKMEditor { get; private set; } = null!;

    public void Initialize(params object[] args)
    {
        SaveFileEditor = (ISaveFileProvider)Array.Find(args, z => z is ISaveFileProvider)!;
        PKMEditor = (IPKMView)Array.Find(args, z => z is IPKMView)!;
        var menu = (ToolStrip)Array.Find(args, z => z is ToolStrip)!;
        NotifySaveLoaded();
        LoadMenuStrip(menu);
    }
    private void LoadMenuStrip(ToolStrip menuStrip)
    {
        var items = menuStrip.Items;
        if (items.Find("Menu_Tools", false)[0] is not ToolStripDropDownItem tools)
            throw new ArgumentException(null, nameof(menuStrip));
        AddPluginControl(tools);
    }
    private void AddPluginControl(ToolStripDropDownItem tools)
    {
        Plugin.Click += (_, _) => new Form1(SaveFileEditor).Show();
        tools.DropDownItems.Add(Plugin);
    }
    public void NotifySaveLoaded()
    {

    }
    public virtual bool TryLoadFile(string filePath)
    {
        Console.WriteLine($"{Name} was provided with the file path, but chose to do nothing with it.");
        return false; // no action taken
    }
    private readonly ToolStripMenuItem Plugin = new("Stored Shiny Map");
}
