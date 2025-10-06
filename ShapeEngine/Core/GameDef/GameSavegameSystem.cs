using ShapeEngine.Core.Structs;
using ShapeEngine.StaticLib;

namespace ShapeEngine.Core.GameDef;



public partial class Game
{
    #region Public Members

    /// <summary>
    /// The directory where game data is saved.
    /// Points to <see cref="GameSettings.SaveDirectory"/>/<see cref="GameSettings.ApplicationName"/>.
    /// Will be empty if no save directory is set in <see cref="GameSettings"/>.
    /// </summary>
    public readonly DirectoryInfo SaveDirectory;

    /// <summary>
    /// Gets the full path of the save directory as a string.
    /// </summary>
    public string SaveDirectoryPath => SaveDirectory.FullName;

    /// <summary>
    /// Indicates whether the save directory is valid (exists and has a non-empty path).
    /// </summary>
    public bool IsSaveDirectoryValid => SaveDirectory is { Exists: true, FullName.Length: > 0 };

    /// <summary>
    /// The directory where backup copies of savegame data are stored.
    /// Resolves to "<see cref="SaveDirectoryPath"/>/Backups".
    /// Will be empty if the backup directory could not be created.
    /// </summary>
    public readonly DirectoryInfo SavegameBackupDirectory;

    /// <summary>
    /// Gets the full path of the save backup directory as a string.
    /// </summary>
    public string SavegameBackupDirectoryPath => SavegameBackupDirectory.FullName;

    /// <summary>
    /// Indicates whether the save backup directory is valid (exists and has a non-empty path).
    /// </summary>
    public bool IsSavegameBackupDirectoryValid => SavegameBackupDirectory is { Exists: true, FullName.Length: > 0 };

    /// <summary>
    /// The directory where savegame files are stored.
    /// This points to "<see cref="SaveDirectoryPath"/>/Savegames".
    /// </summary>
    public readonly DirectoryInfo SavegameDirectory;

    /// <summary>
    /// Gets the full path of the savegame directory as a string.
    /// </summary>
    public string SavegameDirectoryPath => SavegameDirectory.FullName;

    /// <summary>
    /// Indicates whether the savegame directory is valid (exists and has a non-empty path).
    /// </summary>
    public bool IsSavegameDirectoryValid => SavegameDirectory is { Exists: true, FullName.Length: > 0 };

    #endregion

    #region Savegame System

    public bool SaveSavegameSlotInformation(int currentActiveSlot, int usedSlots, int maxSlots)
    {
        if (!IsSavegameDirectoryValid) return false;
        if (currentActiveSlot < 0 || usedSlots < 0) return false;

        string path = Path.Combine(SavegameDirectoryPath, "CurrentSavegameSlot.txt");
        var content = $"{currentActiveSlot},{usedSlots},{maxSlots}";
        return ShapeFileManager.SaveText(content, path);
    }

    public bool LoadSavegameSlotInformation(out int activeSlot, out int usedSlots, out int maxSlots)
    {
        activeSlot = -1;
        usedSlots = -1;
        maxSlots = -1;
        if (!IsSavegameDirectoryValid) return false;

        string path = Path.Combine(SavegameDirectoryPath, "CurrentSavegameSlot.txt");
        string content = ShapeFileManager.LoadText(path);
        if (content.Length == 0) return false;

        var parts = content.Split(',');
        if (parts.Length != 3) return false;

        if (int.TryParse(parts[0], out activeSlot) && int.TryParse(parts[1], out usedSlots) && int.TryParse(parts[2], out maxSlots))
        {
            if (activeSlot >= 0 && usedSlots >= 0 && maxSlots >= 0) return true;
        }

        activeSlot = -1;
        usedSlots = -1;
        maxSlots = -1;
        return false;
    }

    public bool Save(string relativeFilePath, string content)
    {
        if (SaveDirectoryPath == string.Empty) return false;
        if (!Path.HasExtension(relativeFilePath)) return false;

        string path = Path.Combine(SaveDirectoryPath, relativeFilePath);
        return ShapeFileManager.SaveText(content, path);
    }

    public bool Load(string relativeFilePath, out string content)
    {
        content = string.Empty;

        if (SaveDirectoryPath == string.Empty) return false;
        if (!Path.HasExtension(relativeFilePath)) return false;

        string path = Path.Combine(SaveDirectoryPath, relativeFilePath);
        content = ShapeFileManager.LoadText(path);

        return true;
    }

    public bool SaveToSlot(string relativeFilePath, string content, int slotNumber)
    {
        string slotPath = GetSlotPath(relativeFilePath, slotNumber);
        return slotPath != string.Empty && Save(slotPath, content);
    }

    public bool LoadFromSlot(string relativeFilePath, int slotNumber, out string content)
    {
        string slotPath = GetSlotPath(relativeFilePath, slotNumber);
        content = string.Empty;
        return slotPath != string.Empty && Load(slotPath, out content);
    }

    public string GetSlotPath(int slotNumber)
    {
        return !IsSavegameDirectoryValid ? string.Empty : Path.Combine(SavegameDirectoryPath, $"Slot-{slotNumber:D2}");
    }

    public string GetSlotPath(string relativeFilePath, int slotNumber)
    {
        if (relativeFilePath == string.Empty || !Path.HasExtension(relativeFilePath) || !IsSavegameDirectoryValid) return string.Empty;
        string slotDir = Path.Combine(SavegameDirectoryPath, $"Slot-{slotNumber:D2}");
        return Path.Combine(slotDir, relativeFilePath);
    }

    #endregion

    #region Savegame Backup System

    public bool SaveBackupInformation(int currentBackupNumber, int createdBackups, int maxBackups)
    {
        if (!IsSavegameBackupDirectoryValid) return false;
        string backupInfoPath = Path.Combine(SavegameBackupDirectoryPath, "BackupInfo.txt");
        var content = $"{currentBackupNumber},{createdBackups},{maxBackups}";
        return ShapeFileManager.SaveText(content, backupInfoPath);
    }

    public bool LoadBackupInformation(out int currentBackupNumber, out int createdBackups, out int maxBackups)
    {
        currentBackupNumber = -1;
        createdBackups = -1;
        maxBackups = -1;
        if (!IsSavegameBackupDirectoryValid) return false;

        string backupInfoPath = Path.Combine(SavegameBackupDirectoryPath, "BackupInfo.txt");
        string content = ShapeFileManager.LoadText(backupInfoPath);
        if (string.IsNullOrEmpty(content)) return false;

        var parts = content.Split(',');
        if (parts.Length != 2) return false;

        if (int.TryParse(parts[0], out currentBackupNumber) && int.TryParse(parts[1], out createdBackups) && int.TryParse(parts[2], out maxBackups))
        {
            if (currentBackupNumber >= 0 && createdBackups >= 0) return true;
        }

        currentBackupNumber = -1;
        createdBackups = -1;
        maxBackups = -1;
        return false;
    }

    public (bool success, int newBackupNumber, int createdBackups) CreateBackup(int currentBackupNumber, int createdBackups, int maxBackups)
    {
        if (!IsSavegameDirectoryValid || !IsSavegameBackupDirectoryValid) return (false, currentBackupNumber, createdBackups);
        var nextBackup = currentBackupNumber;
        if (maxBackups <= 0)
        {
            nextBackup++;
        }
        else nextBackup = IncrementBackup(nextBackup, maxBackups);

        string sourcePath = SavegameDirectoryPath;
        string destinationPath = GetBackupPath(nextBackup);

        try
        {
            bool success = ShapeFileManager.CopyDirectory(sourcePath, destinationPath, true);
            return success ? (true, nextBackup, createdBackups + 1) : (false, currentBackupNumber, createdBackups);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to create backup{currentBackupNumber}: {ex.Message}");
            return (false, currentBackupNumber, createdBackups);
        }
    }

    public bool CreateBackup(int backupNumber)
    {
        if (!IsSavegameDirectoryValid || !IsSavegameBackupDirectoryValid) return false;

        string sourcePath = SavegameDirectoryPath;
        string destinationPath = GetBackupPath(backupNumber);

        try
        {
            return ShapeFileManager.CopyDirectory(sourcePath, destinationPath, true);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to create backup{backupNumber}: {ex.Message}");
            return false;
        }
    }

    public bool ApplyBackup(int currentBackupNumber, int createdBackups, int maxBackups)
    {
        if (currentBackupNumber < 0 || createdBackups <= 0) return false;
        if (maxBackups <= 0)
        {
            return currentBackupNumber == 0 ? ApplyBackup(0) : ApplyBackup(currentBackupNumber - 1);
        }

        int prevBackup = GetPreviousBackup(currentBackupNumber, maxBackups);
        return ApplyBackup(prevBackup);
    }

    public bool ApplyBackup(int backupNumber)
    {
        if (!IsSavegameDirectoryValid || !IsSavegameBackupDirectoryValid) return false;

        string sourcePath = GetBackupPath(backupNumber);
        string destinationPath = SavegameDirectoryPath;

        try
        {
            return ShapeFileManager.CopyDirectory(sourcePath, destinationPath, true);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to apply backup: {ex.Message}");
            return false;
        }
    }

    public string GetBackupPath(int backupNumber)
    {
        return !IsSavegameBackupDirectoryValid ? string.Empty : Path.Combine(SavegameBackupDirectoryPath, $"Backup-{backupNumber:D2}");
    }

    public int IncrementBackup(int currentBackup, int maxBackups)
    {
        var cur = currentBackup + 1;
        if (cur >= maxBackups) cur = 0;
        return cur;
    }

    public int GetPreviousBackup(int currentBackup, int maxBackups)
    {
        var prev = currentBackup - 1;
        if (prev < 0) prev = maxBackups - 1;
        return prev;
    }

    #endregion
}