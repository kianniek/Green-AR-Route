using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;

public class StopMultipleBanks : MonoBehaviour
{
    [FMODUnity.BankRef]
    public string[] bankNames;  // Array of bank names

    private List<FMOD.Studio.Bank> banks = new List<FMOD.Studio.Bank>();

    void OnEnable()
    {
        // Load all banks in the array
        foreach (var bankName in bankNames)
        {
            RuntimeManager.LoadBank(bankName, true);
            FMOD.Studio.Bank bank;
            RuntimeManager.StudioSystem.getBank(bankName, out bank);
            banks.Add(bank);
        }
        
        StopAllEventsInBanks();
    }

    public void StopAllEventsInBanks()
    {
        foreach (var bank in banks)
        {
            EventDescription[] eventDescriptions;
            int eventCount;

            // Get all events in the bank
            bank.getEventCount(out eventCount);
            bank.getEventList(out eventDescriptions);

            foreach (var eventDescription in eventDescriptions)
            {
                EventInstance eventInstance;
                eventDescription.createInstance(out eventInstance);
                eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                eventInstance.release(); // Ensure you release the instance after stopping it
            }

            // Optionally, unload the bank
            bank.unload();
        }
    }
}