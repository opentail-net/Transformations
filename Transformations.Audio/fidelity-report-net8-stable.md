# Resampler Fidelity Report

Started: 2026-07-07T07:59:49.7336914+00:00
Passed: True
Thresholds: SNR >= 12.00 dB, PSNR >= 18.00 dB, MSE <= 0.03, frame error <= 2

| Result | Algorithm | Scenario | SNR dB | PSNR dB | MSE | Frames | Expected | Round trip | Time ms | Notes |
|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| PASS | Sinc | Music 48k to 44.1k stereo | 73.05 | 85.98 | 1.225E-09 | 44100 | 44100 | 48000 | 10637 |  |
| PASS | Hybrid | Music 48k to 44.1k stereo | 73.05 | 85.98 | 1.225E-09 | 44100 | 44100 | 48000 | 11 |  |
| PASS | Spline | Music 48k to 44.1k stereo | 61.07 | 73.99 | 1.935E-08 | 44099 | 44100 | 47998 | 4 |  |
| PASS | Linear | Music 48k to 44.1k stereo | 33.31 | 46.23 | 1.156E-05 | 44099 | 44100 | 47998 | 0 |  |
| PASS | Sinc | Speech 48k to 16k mono | 47.65 | 60.54 | 4.255E-07 | 16000 | 16000 | 48000 | 16 |  |
| PASS | Spline | Speech 48k to 16k mono | 31.08 | 43.97 | 1.93E-05 | 16000 | 16000 | 48000 | 1 |  |
| PASS | Linear | Speech 48k to 16k mono | 21.35 | 34.24 | 0.0001816 | 16000 | 16000 | 48000 | 30 |  |
| PASS | Hybrid | Speech 48k to 16k mono | 21.35 | 34.24 | 0.0001816 | 16000 | 16000 | 48000 | 0 |  |
| PASS | Linear | Upsample 16k to 48k mono | ∞ | ∞ | 0 | 48000 | 48000 | 16000 | 0 |  |
| PASS | Spline | Upsample 16k to 48k mono | ∞ | ∞ | 0 | 48000 | 48000 | 16000 | 3 |  |
| PASS | Hybrid | Upsample 16k to 48k mono | ∞ | ∞ | 0 | 48000 | 48000 | 16000 | 0 |  |
| PASS | Sinc | Upsample 16k to 48k mono | 360.23 | 373.12 | 2.347E-38 | 48000 | 48000 | 16000 | 3 |  |
