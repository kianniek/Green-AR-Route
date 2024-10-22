using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuteBankFMOD : MonoBehaviour
{
    // Reference to the FMOD Bank
    [FMODUnity.BankRef]
    public string[] bankPaths;
    
    private void Start() {
        FMODUnity.RuntimeManager.StudioSystem.getBankList(out var loadedBanks);
        foreach (var bankPath in bankPaths)
        {
            
        foreach(var bank in loadedBanks) {
            bank.getPath(out var path);
            Debug.Log(path);
            
            var splitPath = path.Split('/');
            
            if(splitPath[1] != bankPath) {
                continue;
            }
            bank.getEventList(out var eventDescriptions);
            foreach(var eventDesc in eventDescriptions) {
                eventDesc.getPath(out var eventPath);
            }
        }
        }

    }
    
    public void ToggleMuteBank(bool mute) {
        if(mute) {
            MuteBank();
        } else {
            UnmuteBank();
        }
    }
    
    public void MuteBank() {
        FMODUnity.RuntimeManager.StudioSystem.getBankList(out var loadedBanks);
        foreach (var bankPath in bankPaths)
        {

            foreach (var bank in loadedBanks)
            {
                bank.getPath(out var path);
                var splitPath = path.Split('/');

                if (splitPath[1] != bankPath)
                {
                    continue;
                }

                bank.getEventList(out var eventDescriptions);
                foreach (var eventDesc in eventDescriptions)
                {
                    eventDesc.getPath(out var eventPath);
                    Debug.Log(eventPath);

                    // Mute the event
                    FMODUnity.RuntimeManager.StudioSystem.getEvent(eventPath, out var eventInstance);

                    // Mute the event
                    eventInstance.getInstanceList(out var instances);

                    foreach (var instance in instances)
                    {
                        instance.setVolume(0);
                    }
                }

                //bank.unload();
                Debug.Log($"{bankPath} unloaded");
            }
        }
    }
    
    public void UnmuteBank() {
        FMODUnity.RuntimeManager.StudioSystem.getBankList(out var loadedBanks);
        foreach (var bankPath in bankPaths)
        {
            foreach (var bank in loadedBanks)
            {
                bank.getPath(out var path);
                var splitPath = path.Split('/');

                if (splitPath[1] != bankPath)
                {
                    continue;
                }

                bank.getEventList(out var eventDescriptions);
                foreach (var eventDesc in eventDescriptions)
                {
                    eventDesc.getPath(out var eventPath);
                    Debug.Log(eventPath);

                    // Mute the event
                    FMODUnity.RuntimeManager.StudioSystem.getEvent(eventPath, out var eventInstance);

                    // Mute the event
                    eventInstance.getInstanceList(out var instances);

                    foreach (var instance in instances)
                    {
                        instance.setVolume(1f);
                    }
                }
            }
        }
    }
}
