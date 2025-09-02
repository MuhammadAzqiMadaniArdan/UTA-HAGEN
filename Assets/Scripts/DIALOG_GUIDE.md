# Dialog System & Ending Guide

## Cara Mencapai Ending

### 1. Good Ending
- **Kondisi**: Greed Level < 30 dan Suspicion Level < 30 setelah menyelesaikan 10 chapter
- **Cara**: 
  - Pilih dialog yang mengurangi greed (teks hijau)
  - Hindari tindakan mencurigakan
  - Selesaikan transaksi dengan hati-hati
  - Jangan terlalu serakah mengambil uang

### 2. Neutral Ending
- **Kondisi**: Greed Level < 60 dan Suspicion Level < 60 setelah menyelesaikan 10 chapter
- **Cara**:
  - Seimbangkan antara keuntungan dan risiko
  - Sesekali ambil risiko tapi jangan berlebihan
  - Pilih dialog yang moderat

### 3. Bad Ending
- **Kondisi**: Greed Level ≥ 60 atau Suspicion Level ≥ 60 setelah menyelesaikan 10 chapter
- **Cara**:
  - Pilih dialog yang meningkatkan greed (teks merah)
  - Ambil risiko besar untuk keuntungan
  - Kurang hati-hati dalam bertindak

### 4. Caught Ending
- **Kondisi**: Suspicion Level mencapai 100 (kapan saja)
- **Cara**:
  - Tertangkap CCTV terlalu sering
  - Pilih dialog yang sangat mencurigakan
  - Gagal menutup laptop saat NPC melihat

### 5. Too Greedy Ending
- **Kondisi**: Greed Level mencapai 100 (kapan saja)
- **Cara**:
  - Selalu pilih opsi yang memberikan uang paling banyak
  - Abaikan risiko demi keuntungan
  - Pilih dialog serakah terus-menerus

## Sistem Dialog dengan NPC

### Setup NPC Dialog:

1. **Buat Dialog Data**:
   ```csharp
   // Contoh dialog data di Inspector
   Dialog Name: "Boss Conversation"
   Nodes:
   - Node 0: "Bagaimana pekerjaanmu hari ini?"
     Choices:
     - "Baik-baik saja" (Greed: 0, Suspicion: 0, Next: 1)
     - "Ada sedikit masalah" (Greed: 5, Suspicion: 10, Next: 2)
   \`\`\`

2. **Setup NPC GameObject**:
   - Tambah NPCDialogInteraction script
   - Assign Dialog Data
   - Set NPC Name dan Interaction Range
   - Buat UI Prompt (Canvas dengan Text "Press E to Talk")

3. **Dialog Conditions**:
   - `minGreedRequired`: Dialog hanya muncul jika greed ≥ nilai ini
   - `maxSuspicionAllowed`: Dialog tidak muncul jika suspicion > nilai ini
   - `requiredFlags`: Dialog memerlukan flag tertentu sudah di-set

### Cara Kerja Dialog System:

1. **Player mendekati NPC** → Interaction prompt muncul
2. **Tekan E** → Dialog dimulai, game state berubah ke Dialog
3. **Pilih response** → Greed/Suspicion berubah, flag di-set
4. **Dialog selesai** → Kembali ke gameplay normal

### Tips Programming Dialog:

- Gunakan `SetDialogFlag()` untuk unlock dialog baru
- `customAction` untuk trigger event khusus
- Typewriter effect otomatis dengan audio
- History dialog tersimpan untuk referensi
- Color coding: Hijau = mengurangi greed, Merah = menambah greed
</markdown>
