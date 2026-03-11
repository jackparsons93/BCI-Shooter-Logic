# BCI-Shooter-Logic
A 2D Unity shooter where bullet power is controlled by real-time SMR and TBR brainwave ratios using OpenBCI

# BCI 2D Shooter (Neurofeedback)
This project uses an **OpenBCI Cyton** and **Ultracortex** to control a 2D shooter.

### How it works:
1. **Python Server:** Uses `BrainFlow` to stream EEG data and `SciPy` to extract raw frequency bands (Theta, SMR, Beta). It sends this data to Unity via UDP.
2. **Unity Receiver:** Processes the raw bands in C# to calculate:
   - **TBR (Theta/Beta Ratio):** For focus training.
   - **SMR Ratio:** For calm-alertness training.
3. **Game Logic:** The `performance_score` (0-100%) scales the bullet size, damage, and color in real-time.

### Key Scripts:
- `bci_server.py`: The "Dumb" server handling DSP.
- `BCIReceiver.cs`: The core neuro-engine in Unity.
- `PlayerShooter.cs`: Controls the player and ties weapon power to brain states.
