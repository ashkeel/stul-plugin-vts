using System;
using UnityEngine;

namespace StulPlugin
{
    public class CostumeManager
    {
        private static HotkeyManager _manager;

        /// <summary>
        /// Switch to alternate costume, most likely only temporarily
        /// </summary>
        /// <param name="costume">Costume identifier</param>
        /// <returns>true if the costume switch could and was performed, false otherwise</returns>
        public bool SwitchTo(string costume)
        {
            if (_manager == null)
            {
                _manager = GameObject.Find("Hotkeys").GetComponent<HotkeyManager>();
            }
            switch (costume)
            {
                case "casual":
                    findAndExecuteHotkey("CostumeRedeem/casual");
                    return true;
                case "pain":
                    findAndExecuteHotkey("CostumeRedeem/pain");
                    return true;
                case "trash":
                    findAndExecuteHotkey("CostumeRedeem/trash");
                    return true;
            }

            return false;
        }

        private void findAndExecuteHotkey(string hotkeyName)
        {
            var hotkey = _manager.hotkeys.Find(entry => entry.Name == hotkeyName);
            if (hotkey == null)
            {
                throw new Exception("hotkey not found");
            }
            _manager.ExecuteHotkey(hotkey);
        }
    }
}