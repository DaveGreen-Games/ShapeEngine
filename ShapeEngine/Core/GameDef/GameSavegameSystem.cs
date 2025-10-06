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

    /// <summary>
    /// Saves information about the current savegame slot,
    /// including the active slot, number of used slots, and maximum slots.
    /// </summary>
    /// <param name="currentActiveSlot">The index of the currently active savegame slot.</param>
    /// <param name="usedSlots">The number of savegame slots currently in use.</param>
    /// <param name="maxSlots">The maximum number of savegame slots available.</param>
    /// <returns>True if the information was successfully saved; otherwise, false.</returns>
    /// <remarks>
    /// Saves the information to a file named "CurrentSavegameSlot.txt" in the <see cref="SavegameDirectoryPath"/>.
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
    public bool SaveSavegameSlotInformation(int currentActiveSlot, int usedSlots, int maxSlots)
    {
        if (!IsSavegameDirectoryValid) return false;
        if (currentActiveSlot < 0 || usedSlots < 0) return false;

        string path = Path.Combine(SavegameDirectoryPath, "CurrentSavegameSlot.txt");
        var content = $"{currentActiveSlot},{usedSlots},{maxSlots}";
        return ShapeFileManager.SaveText(content, path);
    }
    
    /// <summary>
    /// Loads information about the current savegame slot,
    /// including the active slot, number of used slots, and maximum slots.
    /// </summary>
    /// <param name="activeSlot">Outputs the index of the currently active savegame slot.</param>
    /// <param name="usedSlots">Outputs the number of savegame slots currently in use.</param>
    /// <param name="maxSlots">Outputs the maximum number of savegame slots available.</param>
    /// <returns>True if the information was successfully loaded; otherwise, false.</returns>
    /// <remarks>
    /// Loads the information from a file named "CurrentSavegameSlot.txt" in the <see cref="SavegameDirectoryPath"/>.
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
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

    /// <summary>
    /// Saves the specified content to a file at the given relative path within the save directory.
    /// Requires <see cref="IsSavegameDirectoryValid"/> to be true.
    /// </summary>
    /// <param name="relativeFilePath">The relative file path (including extension) within the save directory.</param>
    /// <param name="content">The content to save to the file.</param>
    /// <returns>True if the content was successfully saved; otherwise, false.</returns>
    /// <remarks>
    /// Saves to "<see cref="SaveDirectoryPath"/>/<paramref name="relativeFilePath"/>".
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
    public bool Save(string relativeFilePath, string content)
    {
        if (SaveDirectoryPath == string.Empty) return false;
        if (!Path.HasExtension(relativeFilePath)) return false;

        string path = Path.Combine(SaveDirectoryPath, relativeFilePath);
        return ShapeFileManager.SaveText(content, path);
    }

    /// <summary>
    /// Loads the content of a file at the given relative path within the save directory.
    /// </summary>
    /// <param name="relativeFilePath">The relative file path (including extension) within the save directory.</param>
    /// <param name="content">Outputs the content loaded from the file.</param>
    /// <returns>True if the content was successfully loaded; otherwise, false.</returns>
    /// <remarks>
    /// Loads from "<see cref="SaveDirectoryPath"/>/<paramref name="relativeFilePath"/>".
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
    public bool Load(string relativeFilePath, out string content)
    {
        content = string.Empty;

        if (SaveDirectoryPath == string.Empty) return false;
        if (!Path.HasExtension(relativeFilePath)) return false;

        string path = Path.Combine(SaveDirectoryPath, relativeFilePath);
        content = ShapeFileManager.LoadText(path);

        return true;
    }

    /// <summary>
    /// Saves the specified content to a file within a specific savegame slot directory.
    /// </summary>
    /// <param name="relativeFilePath">The relative file path (including extension) within the slot directory.</param>
    /// <param name="content">The content to save to the file.</param>
    /// <param name="slotNumber">The slot number to save the file in.</param>
    /// <returns>True if the content was successfully saved; otherwise, false.</returns>
    /// <remarks>
    /// Saves to "<see cref="SavegameDirectoryPath"/>/Slot-XX/<paramref name="relativeFilePath"/>".
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
    public bool SaveToSlot(string relativeFilePath, string content, int slotNumber)
    {
        string slotPath = GetSlotPath(relativeFilePath, slotNumber);
        return slotPath != string.Empty && Save(slotPath, content);
    }
    /// <summary>
    /// Loads the content of a file from a specific savegame slot directory.
    /// </summary>
    /// <param name="relativeFilePath">The relative file path (including extension) within the slot directory.</param>
    /// <param name="slotNumber">The slot number to load the file from.</param>
    /// <param name="content">Outputs the content loaded from the file.</param>
    /// <returns>True if the content was successfully loaded; otherwise, false.</returns>
    /// <remarks>
    /// Loads from "<see cref="SavegameDirectoryPath"/>/Slot-XX/<paramref name="relativeFilePath"/>".
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
    public bool LoadFromSlot(string relativeFilePath, int slotNumber, out string content)
    {
        string slotPath = GetSlotPath(relativeFilePath, slotNumber);
        content = string.Empty;
        return slotPath != string.Empty && Load(slotPath, out content);
    }

    /// <summary>
    /// Gets the full path to the specified savegame slot directory.
    /// Returns an empty string if the savegame directory is not valid.
    /// </summary>
    /// <param name="slotNumber">The slot number for which to get the directory path.</param>
    /// <returns>The full path to the slot directory, or an empty string if invalid
    /// ("<see cref="SavegameDirectoryPath"/>/Slot-XX/").</returns>
    /// <remarks>
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
    public string GetSlotPath(int slotNumber)
    {
        return !IsSavegameDirectoryValid ? string.Empty : Path.Combine(SavegameDirectoryPath, $"Slot-{slotNumber:D2}");
    }

    /// <summary>
    /// Gets the full path to a file within a specific savegame slot directory.
    /// Returns an empty string if the relative file path is empty, does not have an extension,
    /// or if the savegame directory is not valid.
    /// </summary>
    /// <param name="relativeFilePath">The relative file path (including extension) within the slot directory.</param>
    /// <param name="slotNumber">The slot number for which to get the file path.</param>
    /// <returns>The full path to the file in the slot directory, or an empty string if invalid
    /// ("<see cref="SavegameDirectoryPath"/>/Slot-XX/<paramref name="relativeFilePath"/>").</returns>
    /// <remarks>
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
    public string GetSlotPath(string relativeFilePath, int slotNumber)
    {
        if (relativeFilePath == string.Empty || !Path.HasExtension(relativeFilePath) || !IsSavegameDirectoryValid) return string.Empty;
        string slotDir = Path.Combine(SavegameDirectoryPath, $"Slot-{slotNumber:D2}");
        return Path.Combine(slotDir, relativeFilePath);
    }

    #endregion

    #region Savegame Backup System

    /// <summary>
    /// Saves backup information including the current backup number,
    /// the number of created backups, and the maximum number of backups.
    /// Writes this information to a file named "BackupInfo.txt" in the <see cref="SavegameBackupDirectory"/>.
    /// </summary>
    /// <param name="currentBackupNumber">The current backup number.</param>
    /// <param name="createdBackups">The total number of backups created.</param>
    /// <param name="maxBackups">The maximum number of backups allowed.</param>
    /// <returns>True if the information was successfully saved; otherwise, false.</returns>
    /// <remarks>
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
    public bool SaveBackupInformation(int currentBackupNumber, int createdBackups, int maxBackups)
    {
        if (!IsSavegameBackupDirectoryValid) return false;
        string backupInfoPath = Path.Combine(SavegameBackupDirectoryPath, "BackupInfo.txt");
        var content = $"{currentBackupNumber},{createdBackups},{maxBackups}";
        return ShapeFileManager.SaveText(content, backupInfoPath);
    }

    /// <summary>
    /// Loads backup information including the current backup number,
    /// the number of created backups, and the maximum number of backups.
    /// Reads this information from a file named "BackupInfo.txt" in the <see cref="SavegameBackupDirectory"/>.
    /// </summary>
    /// <param name="currentBackupNumber">Outputs the current backup number.</param>
    /// <param name="createdBackups">Outputs the total number of backups created.</param>
    /// <param name="maxBackups">Outputs the maximum number of backups allowed.</param>
    /// <returns>true if the information was successfully loaded; otherwise, false.</returns>
    /// <remarks>
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
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

    /// <summary>
    /// Creates a new backup of the current savegame directory.
    /// Increments the backup number, copies the savegame directory to a new backup directory,
    /// and returns the result along with the new backup number and updated count of created backups.
    /// </summary>
    /// <param name="currentBackupNumber">The current backup number.</param>
    /// <param name="createdBackups">The total number of backups created so far.</param>
    /// <param name="maxBackups">The maximum number of backups allowed. If less than or equal to 0, unlimited backups are allowed.</param>
    /// <returns>
    /// A tuple containing:
    /// - success: True if the backup was created successfully; otherwise, false.
    /// - newBackupNumber: The new backup number after incrementing.
    /// - createdBackups: The updated total number of backups created.
    /// </returns>
    /// <remarks>
    /// Backups are stored in "<see cref="SavegameBackupDirectoryPath"/>/Backup-XX" based on the backup number.
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
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

    /// <summary>
    /// Creates a backup of the current savegame directory at the specified backup number.
    /// Copies the contents of the savegame directory to a backup directory named "Backup-XX" in the savegame backup directory.
    /// Returns true if the backup was created successfully; otherwise, false.
    /// </summary>
    /// <param name="backupNumber">The backup number to use for the backup directory.</param>
    /// <returns>True if the backup was created successfully; otherwise, false.</returns>
    /// <remarks>
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
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
    /// <summary>
    /// Applies the previous backup based on the current backup number,
    /// total created backups, and maximum allowed backups.
    /// If <paramref name="maxBackups"/> is less than or equal to 0, the previous backup will be applied or the backup 0 if the current is 0. I
    /// f <paramref name="maxBackups"/> is greater than 0, than <see cref="GetPreviousBackup"/> is used to determine the previous backup to apply.
    /// Otherwise, calculates the previous backup using <see cref="GetPreviousBackup"/> and applies it.
    /// </summary>
    /// <param name="currentBackupNumber">The current backup number.</param>
    /// <param name="createdBackups">The total number of backups created.</param>
    /// <param name="maxBackups">The maximum number of backups allowed.
    /// If less than or equal to 0, unlimited backups are allowed.</param>
    /// <returns>True if the backup was applied successfully; otherwise, false.</returns>
    /// <remarks>
    /// If successful, this function will overwrite the current savegame directory with the contents of the selected backup.
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
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

    /// <summary>
    /// Applies a backup by copying the contents of <see cref="SavegameBackupDirectoryPath"/>/(Backup-XX)
    /// to the current savegame directory, overwriting its contents.
    /// </summary>
    /// <param name="backupNumber">The backup number to apply.</param>
    /// <returns>True if the backup was applied successfully; otherwise, false.</returns>
    /// <remarks>
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
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

    /// <summary>
    /// Gets the full path to a backup directory for the specified backup number.
    /// Returns an empty string if the savegame backup directory is not valid.
    /// </summary>
    /// <param name="backupNumber">The backup number for which to get the directory path.</param>
    /// <returns>The full path to the backup directory, or an empty string if invalid
    /// ("<see cref="SavegameBackupDirectoryPath"/>/Backup-XX").</returns>
    /// <remarks>
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
    public string GetBackupPath(int backupNumber)
    {
        return !IsSavegameBackupDirectoryValid ? string.Empty : Path.Combine(SavegameBackupDirectoryPath, $"Backup-{backupNumber:D2}");
    }
    
    /// <summary>
    /// Increments the backup number, wrapping around to 0 if it reaches the maximum number of backups.
    /// </summary>
    /// <param name="currentBackup">The current backup number.</param>
    /// <param name="maxBackups">The maximum number of backups allowed.</param>
    /// <returns>The next backup number, wrapped to 0 if it exceeds or equals <paramref name="maxBackups"/>.</returns>
    /// <remarks>
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
    public int IncrementBackup(int currentBackup, int maxBackups)
    {
        var cur = currentBackup + 1;
        if (cur >= maxBackups) cur = 0;
        return cur;
    }

    /// <summary>
    /// Gets the previous backup number, wrapping around to the last backup if the current is 0.
    /// </summary>
    /// <param name="currentBackup">The current backup number.</param>
    /// <param name="maxBackups">The maximum number of backups allowed.</param>
    /// <returns>The previous backup number, or the last backup if the current is 0.</returns>
    /// <remarks>
    /// Requires <see cref="IsSavegameBackupDirectoryValid"/> to be true.
    /// When constructing the <see cref="Game"/> object,
    /// ensure that <see cref="GameSettings.SaveDirectory"/> and <see cref="GameSettings.ApplicationName"/> are set
    /// in <see cref="GameSettings"/> to create a valid savegame backup directory.
    /// </remarks>
    public int GetPreviousBackup(int currentBackup, int maxBackups)
    {
        var prev = currentBackup - 1;
        if (prev < 0) prev = maxBackups - 1;
        return prev;
    }

    #endregion
}