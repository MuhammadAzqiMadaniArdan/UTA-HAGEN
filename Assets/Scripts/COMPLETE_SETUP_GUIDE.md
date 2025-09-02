# Panduan Setup Lengkap Unity Stealth Game

## 1. SISTEM RANGE DAN INTERAKSI

### Cara Kerja Range System:
- **NPCDialogInteraction** menggunakan `Vector3.Distance()` untuk mengecek jarak player ke NPC
- Range default adalah 3 unit (dapat diubah di inspector)
- Sistem menggunakan `Update()` untuk terus mengecek jarak setiap frame
- Ketika player masuk range, UI prompt muncul
- Ketika player keluar range, UI prompt hilang

### Setup Interaksi Dialog dengan Tombol E:

#### A. Setup NPC untuk Dialog:
1. **Buat GameObject NPC:**
   \`\`\`
   - Buat Empty GameObject, beri nama "NPC_Guard"
   - Tambahkan Collider (untuk deteksi)
   - Tambahkan NavMeshAgent component
   - Tambahkan NPCController script
   - Tambahkan NPCDialogInteraction script
   \`\`\`

2. **Setup NPCDialogInteraction:**
   \`\`\`
   - Interaction Range: 3 (jarak deteksi)
   - Interaction Key: E (tombol untuk dialog)
   - NPC Name: "Security Guard"
   - Drag DialogData asset ke NPC Dialog Data
   \`\`\`

3. **Buat UI Interaction Prompt:**
   \`\`\`
   - Buat UI Text/TextMeshPro: "Tekan E untuk berbicara"
   - Drag ke Interaction Prompt field di NPCDialogInteraction
   \`\`\`

## 2. SETUP UI SISTEM

### A. Dialog UI Setup:
1. **Buat Dialog Canvas:**
   \`\`\`
   - Canvas dengan Render Mode: Screen Space - Overlay
   - Tambahkan Panel untuk background dialog
   - Tambahkan Text untuk nama karakter
   - Tambahkan Text untuk dialog content
   - Tambahkan Button untuk pilihan dialog (bisa multiple)
   \`\`\`

2. **Setup DialogSystem:**
   \`\`\`
   - Drag semua UI elements ke DialogSystem script
   - Set Canvas Group untuk fade in/out effect
   - Set typing speed untuk typewriter effect
   \`\`\`

### B. Game UI Setup:
1. **Status UI:**
   \`\`\`
   - Greed Bar: Slider dengan fill merah
   - Suspicion Bar: Slider dengan fill kuning
   - Money Display: Text untuk menampilkan uang
   \`\`\`

2. **Laptop UI:**
   \`\`\`
   - Panel untuk laptop screen
   - Input fields untuk transfer amount
   - Buttons untuk confirm/cancel
   - Progress bar untuk transfer
   \`\`\`

## 3. SETUP NPC SISTEM

### A. Basic NPC Setup:
1. **GameObject Structure:**
   \`\`\`
   NPC_Guard
   ├── Model (3D model atau capsule)
   ├── NavMeshAgent
   ├── Collider
   ├── NPCController
   ├── NPCDialogInteraction
   └── UI_Prompt (World Space Canvas)
   \`\`\`

2. **NavMesh Patrol Setup:**
   \`\`\`
   - Buat Empty GameObjects sebagai patrol points
   - Drag ke Patrol Points array di NPCController
   - Set Detection Range (jarak melihat player)
   - Set Field of View (sudut pandang)
   \`\`\`

### B. Dialog Data Setup:
1. **Buat DialogData ScriptableObject:**
   ```csharp
   [CreateAssetMenu(fileName = "New Dialog", menuName = "Dialog/Dialog Data")]
   public class DialogData : ScriptableObject
   {
       public DialogNode[] dialogNodes;
   }
   \`\`\`

2. **Setup Dialog Nodes:**
   \`\`\`
   - Node 0: "Halo, ada yang bisa saya bantu?"
   - Choices: ["Tidak ada", "Saya butuh bantuan"]
   - Next Node IDs sesuai pilihan
   \`\`\`

## 4. SETUP CCTV SISTEM

### A. CCTV Camera Setup:
1. **Buat CCTV GameObject:**
   \`\`\`
   CCTV_Camera
   ├── Model (kamera 3D atau cube)
   ├── Transform untuk arah pandang
   └── Gizmo untuk visualisasi range
   \`\`\`

2. **Setup di StealthSystem:**
   \`\`\`
   - Drag CCTV Transform ke CCTV Cameras array
   - Set Detection Range: 15 unit
   - Set Field of View: 90 derajat
   - Set Player Layer mask
   \`\`\`

### B. Hiding Spots Setup:
1. **Buat Hiding Spot:**
   \`\`\`
   - Empty GameObject di lokasi persembunyian
   - Set Hiding Radius: 2 unit
   - Drag ke Hiding Spots array di StealthSystem
   \`\`\`

## 5. SETUP LAYER DAN TAG

### A. Tags yang Diperlukan:
\`\`\`
- "Player" untuk player GameObject
- "Ground" untuk lantai/tanah
- "NPC" untuk semua NPC
- "CCTV" untuk kamera
- "HidingSpot" untuk tempat sembunyi
\`\`\`

### B. Layers yang Diperlukan:
\`\`\`
- Default (0)
- Player (8)
- NPC (9)
- Ground (10)
- UI (5)
\`\`\`

## 6. SETUP INPUT SYSTEM

### A. Input Actions Setup:
1. **Install Input System Package:**
   \`\`\`
   Window > Package Manager > Input System > Install
   \`\`\`

2. **Buat Input Actions Asset:**
   \`\`\`
   - Right click > Create > Input Actions
   - Nama: "PlayerInputActions"
   - Setup Movement, Look, Interact, Jump actions
   \`\`\`

## 7. SETUP NAVMESH

### A. NavMesh Baking:
1. **Pilih Ground Objects:**
   \`\`\`
   - Select semua lantai/ground
   - Mark sebagai "Navigation Static"
   \`\`\`

2. **Bake NavMesh:**
   \`\`\`
   - Window > AI > Navigation
   - Tab Bake
   - Set Agent Radius: 0.5
   - Set Agent Height: 2
   - Click Bake
   \`\`\`

## 8. TESTING DAN DEBUGGING

### A. Test Range System:
1. **Visual Debug:**
   \`\`\`
   - Gizmos di Scene view menunjukkan interaction range
   - CCTV field of view terlihat sebagai wireframe
   - NavMesh terlihat sebagai blue overlay
   \`\`\`

2. **Console Debug:**
   ```csharp
   Debug.Log($"Player distance: {distance}");
   Debug.Log($"Dialog started with: {npcName}");
   \`\`\`

### B. Common Issues:
1. **Dialog tidak muncul:**
   - Cek apakah DialogSystem ada di scene
   - Cek apakah DialogData ter-assign
   - Cek apakah Canvas aktif

2. **Range tidak bekerja:**
   - Cek apakah Player memiliki tag "Player"
   - Cek apakah interactionRange > 0
   - Cek apakah Update() berjalan

3. **CCTV tidak detect:**
   - Cek Layer Mask settings
   - Cek apakah ada obstacle menghalangi raycast
   - Cek Field of View angle

## 9. SCRIPT EXECUTION ORDER

Set Script Execution Order di Project Settings:
\`\`\`
1. GameManager (-100)
2. InputSystem (-50)
3. PlayerController (0)
4. DialogSystem (50)
5. StealthSystem (100)
\`\`\`

## 10. PERFORMANCE TIPS

1. **Optimasi Range Checking:**
   - Gunakan FixedUpdate() untuk physics checks
   - Implement object pooling untuk UI prompts
   - Cache component references

2. **Optimasi CCTV:**
   - Limit raycast frequency
   - Use coroutines untuk periodic checks
   - Disable distant cameras
\`\`\`

```csharp file="Scripts/Dialog/DialogData.cs"
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialog", menuName = "Dialog/Dialog Data")]
public class DialogData : ScriptableObject
{
    [Header("Dialog Information")]
    public string characterName;
    public DialogNode[] dialogNodes;
    
    [Header("Dialog Settings")]
    public bool canRepeat = true;
    public float typingSpeed = 0.05f;
    public AudioClip dialogSound;
}

[System.Serializable]
public class DialogNode
{
    [Header("Dialog Content")]
    [TextArea(3, 6)]
    public string dialogText;
    
    [Header("Character Info")]
    public string speakerName;
    public Sprite characterPortrait;
    
    [Header("Choices")]
    public DialogChoice[] choices;
    
    [Header("Actions")]
    public DialogAction[] actions;
    
    [Header("Conditions")]
    public DialogCondition[] conditions;
}

[System.Serializable]
public class DialogChoice
{
    [TextArea(1, 3)]
    public string choiceText;
    public int nextNodeID = -1;
    
    [Header("Requirements")]
    public float requiredGreed = 0f;
    public float requiredSuspicion = 0f;
    public bool requiresFlag = false;
    public string flagName = "";
    
    [Header("Effects")]
    public float greedChange = 0f;
    public float suspicionChange = 0f;
    public string setFlag = "";
}

[System.Serializable]
public class DialogAction
{
    public DialogActionType actionType;
    public string actionValue;
    public float actionAmount;
}

[System.Serializable]
public class DialogCondition
{
    public DialogConditionType conditionType;
    public string conditionKey;
    public float conditionValue;
    public bool conditionMet = false;
}

public enum DialogActionType
{
    SetFlag,
    AddMoney,
    RemoveMoney,
    ChangeGreed,
    ChangeSuspicion,
    TriggerEvent,
    PlaySound,
    ChangeScene
}

public enum DialogConditionType
{
    HasFlag,
    GreedLevel,
    SuspicionLevel,
    MoneyAmount,
    GameState,
    TimeOfDay
}
