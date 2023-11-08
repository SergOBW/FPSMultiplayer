using UnityEngine;
using UnityEditor;
using MFPSEditor;
using MFPS.InputManager;

public class AddonPlayerSideLeaningDoc : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    public string FolderPath = "mfps2/editor/side-leaning/";
    public NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-0.png", Image = null},
        new NetworkImages{Name = "img-1.png", Image = null},
        new NetworkImages{Name = "img-2.png", Image = null},
        new NetworkImages{Name = "img-3.png", Image = null},
        new NetworkImages{Name = "img-4.png", Image = null},
    };
    public Steps[] AllSteps = new Steps[] {
    new Steps { Name = "Integration", StepsLenght = 0 , DrawFunctionName = nameof(GetStartedDoc)},
    new Steps { Name = "Customize", StepsLenght = 0, DrawFunctionName = nameof(SecondSection) },
    };
    private readonly GifData[] AnimatedImages = new GifData[]
   {
        new GifData{ Path = "name.gif" },
   };

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, FolderPath, AnimatedImages);
        Style.highlightColor = ("#c9f17c").ToUnityColor();
        allowTextSuggestions = true;
    }

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }
    //final required////////////////////////////////////////////////

    void GetStartedDoc()
    {
        DrawText("This addon doesn't require to be enabled, you only have to run the auto-player setup, for it you can click in the button below");
        if(GUILayout.Button("Run Player Setup"))
        {
            Integrate();
        }
        Space(20);
        DrawText("If you add new players like the Player Packs or Player Selector addon, make sure to run the Player Setup again.");
    }

    void SecondSection()
    {
        DrawText("Once you run the player setup integration, you can customize the player side leaning movement in each player prefab.\n \nFor it, select the player prefab that you want to modify > in the root of the player prefab you will see the script <b>bl_PlayerSideLeaning</b> > in the inspector of that script you will have some properties that you can modify to adjust the leaning movement of the player:");

        DrawHorizontalColumn("Leaning Angle", "Determine the side angle amount (left and right) that the player rotates when leaning.");
        DrawHorizontalColumn("Lateral Displacement", "Determine the lateral position offset (left and right) applied to the player when leaning.");
        DrawHorizontalColumn("Pitch Offset", "The pitch rotation offset applied to the remote player upper body when is leaning, this in order to make the player keep the same Z euler rotation when leaning.");
        DrawHorizontalColumn("Smoothness", "Leaning Rotation smoothness");
    }

    /// <summary>
    /// 
    /// </summary>
    [MenuItem("MFPS/Addons/Side Leaning/Setup Players")]
    public static void Integrate()
    {
       if(bl_GameData.Instance.Player1) SetupPlayer(bl_GameData.Instance.Player1.gameObject);
       if(bl_GameData.Instance.Player2) SetupPlayer(bl_GameData.Instance.Player2.gameObject);

#if PSELECTOR
        var allPlayers = bl_PlayerSelector.Data.AllPlayers;
        foreach (var p in allPlayers)
        {
            SetupPlayer(p.Prefab);
        }
#endif

        if(!bl_InputData.Instance.DefaultMapped.ButtonMap.Exists(x => x.KeyName == "Leaning Left"))
        {
            var sl = new ButtonData()
            {
                KeyName = "Leaning Left",
                Description = "Leaning to the left",
                PrimaryKey = KeyCode.Q
            };
            bl_InputData.Instance.DefaultMapped.ButtonMap.Add(sl);

            var sr = new ButtonData()
            {
                KeyName = "Leaning Right",
                Description = "Leaning to the right",
                PrimaryKey = KeyCode.E
            };
            bl_InputData.Instance.DefaultMapped.ButtonMap.Add(sr);
            //bl_InputData.Instance.inputVersion = "1.0.003";

            EditorUtility.SetDirty(bl_InputData.Instance);
            EditorUtility.SetDirty(bl_InputData.Instance.DefaultMapped);

            Debug.Log("Side Leaning integrated for all player prefabs.");
        }
    }

    static void SetupPlayer(GameObject player)
    {
        if (player == null) return;

        if(player.GetComponent<bl_PlayerSideLeaning>() != null) { return; }

        var script = player.AddComponent<bl_PlayerSideLeaning>();
        EditorUtility.SetDirty(script);
        EditorUtility.SetDirty(player);
    }

    [MenuItem("MFPS/Tutorials/Side Leaning")]
    [MenuItem("MFPS/Addons/Side Leaning/Documentation")]

    static void Open()
    {
        GetWindow<AddonPlayerSideLeaningDoc>();
    }
}