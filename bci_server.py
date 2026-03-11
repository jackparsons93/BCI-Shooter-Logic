import os
import time
import queue
import threading
import socket
import json
import numpy as np
from scipy.signal import butter, filtfilt, welch
from brainflow.board_shim import BoardShim, BrainFlowInputParams, BoardIds

class OpenBCIClient:
    def __init__(self, out_queue: queue.Queue):
        self.out_queue = out_queue
        self.serial_port = os.getenv("OPENBCI_SERIAL_PORT", "COM3")
        self.board_id = int(os.getenv("OPENBCI_BOARD_ID", str(BoardIds.CYTON_BOARD.value)))
        self.channel_map = {"F3": 0, "Fz": 1, "F4": 2, "C3": 3, "Cz": 4, "C4": 5, "Pz": 6, "Oz": 7}
        self.board = None

    def _make_board(self) -> BoardShim:
        params = BrainFlowInputParams()
        if self.board_id != BoardIds.SYNTHETIC_BOARD.value:
            params.serial_port = self.serial_port
        return BoardShim(self.board_id, params)

    @staticmethod
    def _bandpower(freqs, psd, lo, hi):
        mask = (freqs >= lo) & (freqs < hi)
        return float(np.trapezoid(psd[mask], freqs[mask])) if np.any(mask) else 0.0

    @staticmethod
    def _compute_bands(signal_uv, fs):
        x = np.asarray(signal_uv, dtype=np.float64)
        if len(x) < max(fs, 64): return {"theta": 0, "alpha": 0, "smr": 0, "betaL": 0, "betaH": 0, "gamma": 0}
        
        x = x - np.mean(x)
        b, a = butter(4, [1.0 / (fs / 2.0), 45.0 / (fs / 2.0)], btype="band")
        x = filtfilt(b, a, x)
        freqs, psd = welch(x, fs=fs, nperseg=min(len(x), len(x)))

        return {
            "theta": OpenBCIClient._bandpower(freqs, psd, 4.0, 8.0),
            "alpha": OpenBCIClient._bandpower(freqs, psd, 8.0, 12.0),
            "smr": OpenBCIClient._bandpower(freqs, psd, 12.0, 15.0),
            "betaL": OpenBCIClient._bandpower(freqs, psd, 15.0, 20.0),
            "betaH": OpenBCIClient._bandpower(freqs, psd, 20.0, 30.0),
            "gamma": OpenBCIClient._bandpower(freqs, psd, 30.0, 45.0),
        }

    def run(self):
        BoardShim.enable_dev_board_logger()
        try:
            self.board = self._make_board()
            self.board.prepare_session()
            print(f"SUCCESS: Connected to REAL board on {self.serial_port}.")
        except:
            print("WARNING: Real board not found. Switching to FAKE Synthetic Board.")
            self.board_id = BoardIds.SYNTHETIC_BOARD.value
            self.board = self._make_board()
            self.board.prepare_session()

        self.board.start_stream()
        fs = BoardShim.get_sampling_rate(self.board_id)
        eeg_rows = BoardShim.get_eeg_channels(self.board_id)

        try:
            while True:
                time.sleep(0.1) # 10Hz update rate
                win_samples = max(16, int(1.0 * fs)) # 1 second window
                data = self.board.get_current_board_data(win_samples)
                
                if data is None or data.size == 0 or data.shape[1] < max(64, win_samples // 2): continue

                payload = {}
                signal_qualities = []

                for site, logical_idx in self.channel_map.items():
                    if logical_idx >= len(eeg_rows): continue
                    sig = data[eeg_rows[logical_idx], :]
                    bands = self._compute_bands(sig, fs)
                    
                    # Package the raw bands for this specific site
                    for band_name, val in bands.items():
                        payload[f"{site}_{band_name}"] = val
                    
                    signal_qualities.append(float(np.std(sig)))

                payload["signal_ok"] = 1 if max(signal_qualities) > 1.0 else 0
                self.out_queue.put(payload)
        finally:
            try: self.board.stop_stream()
            except: pass
            try: self.board.release_session()
            except: pass

def main():
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    msg_q = queue.Queue()
    
    t = threading.Thread(target=lambda: OpenBCIClient(msg_q).run(), daemon=True)
    t.start()

    print("Streaming Raw Band Powers to Unity (127.0.0.1:5005)...")
    try:
        while True:
            payload = msg_q.get()
            sock.sendto(json.dumps(payload).encode('utf-8'), ("127.0.0.1", 5005))
    except KeyboardInterrupt:
        print("Shutting down...")

if __name__ == "__main__":
    main()